import { useState } from "react";
import { toast } from "../ui/sonner";
import { LeftSidebar } from "./layout/LeftSidebar";
import { MainContent } from "./layout/MainContent";
import { RightSidebar } from "./layout/RightSidebar";
import { FullscreenPreview } from "./preview/FullscreenPreview";
import type { SignageConfig, ContentItem } from "./types";

export function SignageCreatorPage() {
    const [config, setConfig] = useState<SignageConfig>({
        name: "Untitled Signage",
        aspectRatio: "16:9",
        transition: "fade",
        defaultDuration: 10,
        loopMode: "continuous",
    });

    const [contentItems, setContentItems] = useState<ContentItem[]>([]);
    const [selectedItemId, setSelectedItemId] = useState<string | null>(null);
    const [currentPreviewIndex, setCurrentPreviewIndex] = useState(0);
    const [isPlaying, setIsPlaying] = useState(false);
    const [showFullscreen, setShowFullscreen] = useState(false);

    const selectedItem = contentItems.find(i => i.id === selectedItemId);

    const handleSave = () => toast.success("Saved as draft");
    const handlePublish = () => {
        if (contentItems.length === 0) return toast.error("Add at least one item");
        toast.success("Published!");
    };

    const updateItem = (id: string, updates: Partial<ContentItem>) => {
        setContentItems(prev => prev.map(i => i.id === id ? { ...i, ...updates } : i));
    };

    return (
        <div className="min-h-screen-87 flex gap-6">
            <LeftSidebar
                config={config}
                setConfig={setConfig}
                contentItems={contentItems}
                setContentItems={setContentItems}
                selectedItem={selectedItemId}
                setSelectedItem={setSelectedItemId}
                onSave={handleSave}
                onPublish={handlePublish}
            />

            <MainContent
                config={config}
                contentItems={contentItems}
                currentIndex={currentPreviewIndex}
                setCurrentIndex={setCurrentPreviewIndex}
                isPlaying={isPlaying}
                setIsPlaying={setIsPlaying}
                onFullscreen={() => setShowFullscreen(true)}
            />

            {selectedItem && (
                <RightSidebar
                    item={selectedItem}
                    onNameChange={(name) => updateItem(selectedItem.id, { name })}
                    onDurationChange={(duration) => updateItem(selectedItem.id, { duration })}
                    onConfigChange={(cfg) => updateItem(selectedItem.id, { config: { ...selectedItem.config, ...cfg } })}
                />
            )}

            {showFullscreen && (
                <FullscreenPreview
                    items={contentItems}
                    transition={config.transition}
                    onClose={() => setShowFullscreen(false)}
                />
            )}

        </div>
    );
}
