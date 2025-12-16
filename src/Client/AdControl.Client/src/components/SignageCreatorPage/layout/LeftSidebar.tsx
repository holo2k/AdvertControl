import { ScrollArea } from "../../ui/scroll-area";
import { GeneralSettings } from "../config/GeneralSettings";
import { ContentList } from "../config/ContentList";
import { ActionButtons } from "../config/ActionButtons";
import type { SignageConfig } from "../types";
import {apiClient} from "../../../api/apiClient.ts";
import { toast } from "../../ui/sonner.tsx";
import {useNavigate} from "react-router-dom";

interface Props {
    config: SignageConfig;
    setConfig: React.Dispatch<React.SetStateAction<SignageConfig>>;
    selectedItem: string | null;
    setSelectedItem: React.Dispatch<React.SetStateAction<string | null>>;
    onSave: () => void;
    onPublish: () => void;
    screenId: string | undefined;
}

export function LeftSidebar({
                                config,
                                setConfig,
                                selectedItem,
                                setSelectedItem,
                                screenId,
                            }: Props) {
    /* ================= UTILS ================= */

    const navigate = useNavigate();

    const getScreenIdFromQuery = (): string => {

        if (!screenId) {
            throw new Error("screenId отсутствует в query параметрах");
        }

        return screenId;
    };

    /* ================= API ================= */

    const createConfig = async (): Promise<string> => {
        const payload = {
            name: config.name,
            screensCount: config.screensCount,
            items: config.items.map((item, index) => ({
                durationSeconds: item.durationSeconds,
                order: index + 1,
                type: item.type,
                url: item.url,
                inlineData: item.inlineData ?? "",
                checksum: item.checksum ?? "",
                size: item.size ?? 0,
            })),
        };

        const response = await apiClient.post("/config", payload);
        return response.data.id;
    };

    const assignConfigToScreen = async (configId: string) => {
        const screenId = getScreenIdFromQuery();

        await apiClient.post(`/config/${configId}/assign`, {
            screenId,
            isActive: true,
        });
    };

    /* ================= ACTIONS ================= */

    const handlePublish = async () => {
        try {
            const configId = await createConfig();
            await assignConfigToScreen(configId);

            toast.success("Конфиг опубликован и отправлен на экран");
            navigate(`/screen/${screenId}`);
        } catch (error) {
            console.error(error);
            toast.error("Ошибка публикации конфига");
        }
    };

    /* ================= RENDER ================= */

    return (
        <div className="w-75 border border-gray-200 bg-white overflow-hidden flex rounded-lg flex-col">
            <ScrollArea className="flex-1 p-6">
                <div className="space-y-6">
                    <GeneralSettings config={config} setConfig={setConfig} />

                    <ContentList
                        items={config.items}
                        setConfig={setConfig}
                        selectedItem={selectedItem}
                        setSelectedItem={setSelectedItem}
                    />
                </div>
            </ScrollArea>

            <ActionButtons
                onPublish={handlePublish}
            />
        </div>
    );
}
