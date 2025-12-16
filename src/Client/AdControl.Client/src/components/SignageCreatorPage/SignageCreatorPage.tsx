import { useEffect, useState } from "react";

import { toast } from "../ui/sonner";

import { LeftSidebar } from "./layout/LeftSidebar";
import { MainContent } from "./layout/MainContent";
import { RightSidebar } from "./layout/RightSidebar";
import { FullscreenPreview } from "./preview/FullscreenPreview";
import { useLocation, useParams } from "react-router-dom";

import type { SignageConfig, ContentItem } from "./types";
import { apiClient } from "../../api/apiClient.ts";


export function SignageCreatorPage() {
    const { id: screenId } = useParams<{ id: string }>();
    const location = useLocation();

    const configId = location.state?.configId as string | undefined;
    const [config, setConfig] = useState<SignageConfig>({
        name: "",
        screensCount: 1,
        items: [],
    });

    const [loading, setLoading] = useState(false);
    const [selectedItemUrl, setSelectedItemUrl] = useState<string | null>(null);
    const [currentPreviewIndex, setCurrentPreviewIndex] = useState(0);
    const [isPlaying, setIsPlaying] = useState(false);
    const [showFullscreen, setShowFullscreen] = useState(false);

    const selectedItem = config.items.find(i => i.url === selectedItemUrl);

    useEffect(() => {
        console.log(configId);
        if (!configId) return;

        const fetchConfig = async () => {
            try {
                setLoading(true);
                const { data } = await apiClient.get<SignageConfig>(`/config/${configId}`);

                // Маппинг типов
                const mappedItems = data.items?.map(item => ({
                    ...item,
                    type: item.type === 0 ? "IMAGE" :
                        item.type === 1 ? "VIDEO" :
                            item.type === 2 ? "AUDIO" : "TEXT" // добавьте другие типы при необходимости
                })) ?? [];

                setConfig({
                    name: data.name ?? "",
                    screensCount: data.screensCount ?? 1,
                    items: mappedItems, // используем преобразованные элементы
                });

            } catch (e) {
                console.error(e);
                toast.error("Ошибка загрузки конфига");
            } finally {
                setLoading(false);
            }
        };
        fetchConfig();
    }, [configId]);

    const handleSave = () => toast.success("Saved as draft");

    const handlePublish = () => {
        if (config.items.length === 0) {
            toast.error("Add at least one item");
            return;
        }
        toast.success("Published!");
    };

    const updateItem = (url: string, updates: Partial<ContentItem>) => {
        setConfig(prev => ({
            ...prev,
            items: prev.items.map(item =>
                item.url === url ? { ...item, ...updates } : item
            ),
        }));
    };

    if (loading) {
        return (
            <div className="min-h-screen flex items-center justify-center text-gray-500">
                Loading configuration...
            </div>
        );
    }

    return (
        <div className="min-h-screen-87 flex gap-6" style={{ maxWidth: "1920px" }}>
            <LeftSidebar
                config={config}
                setConfig={setConfig}
                selectedItem={selectedItemUrl}
                setSelectedItem={setSelectedItemUrl}
                onSave={handleSave}
                onPublish={handlePublish}
                screenId={screenId}
            />

            <MainContent
                config={config}
                currentIndex={currentPreviewIndex}
                setCurrentIndex={setCurrentPreviewIndex}
                isPlaying={isPlaying}
                setIsPlaying={setIsPlaying}
                onFullscreen={() => setShowFullscreen(true)}
            />

            {selectedItem && (
                <RightSidebar
                    item={selectedItem}
                    onDurationChange={(durationSeconds) =>
                        updateItem(selectedItem?.url || '', { durationSeconds })
                    }
                />
            )}

            {showFullscreen && (
                <FullscreenPreview
                    items={config.items}
                    onClose={() => setShowFullscreen(false)}
                />
            )}
        </div>
    );
}
