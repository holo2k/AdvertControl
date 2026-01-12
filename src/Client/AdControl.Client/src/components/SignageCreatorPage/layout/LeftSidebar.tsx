import { ScrollArea } from "../../ui/scroll-area";
import { GeneralSettings } from "../config/GeneralSettings";
import { ContentList } from "../config/ContentList";
import { ActionButtons } from "../config/ActionButtons";
import type { SignageConfig } from "../types";
import { apiClient } from "../../../api/apiClient";
import { toast } from "../../ui/sonner";
import { useNavigate } from "react-router-dom";

interface Props {
    config: SignageConfig;
    setConfig: React.Dispatch<React.SetStateAction<SignageConfig>>;
    selectedItem: string | null;
    setSelectedItem: React.Dispatch<React.SetStateAction<string | null>>;
    screenId: string | undefined;
    isEdit: boolean;
}

export function LeftSidebar({
                                config,
                                setConfig,
                                selectedItem,
                                setSelectedItem,
                                screenId,
                                isEdit,
                            }: Props) {
    /* ================= UTILS ================= */

    const navigate = useNavigate();

    const getScreenIdFromQuery = (): string => {
        if (!screenId) {
            throw new Error("screenId отсутствует");
        }
        return screenId;
    };

    /* ================= PAYLOAD ================= */

    const buildConfigPayload = () => ({
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
    });

    /* ================= API ================= */

    const createConfig = async (): Promise<string> => {
        const response = await apiClient.post("/config", buildConfigPayload());
        return response.data.id;
    };

    const updateConfig = async () => {
        if (!config.id) {
            throw new Error("Отсутствует id конфига");
        }

        await apiClient.patch(
            `/config/${config.id}/update`,
            buildConfigPayload()
        );
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
            let configId = config.id;

            if (isEdit) {
                await updateConfig();
                toast.success("Конфиг обновлён");
            } else {
                configId = await createConfig();
                await assignConfigToScreen(configId);
                toast.success("Конфиг создан и отправлен на экран");
            }

            navigate(`screen/${screenId}`);
        } catch (error) {
            console.error(error);
            toast.error(
                isEdit
                    ? "Ошибка обновления конфига"
                    : "Ошибка публикации конфига"
            );
        }
    };

    /* ================= RENDER ================= */

    return (
        <div className=" w-75 border border-gray-200 bg-white overflow-hidden flex rounded-lg flex-col">
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

            <ActionButtons onPublish={handlePublish} />
        </div>
    );
}
