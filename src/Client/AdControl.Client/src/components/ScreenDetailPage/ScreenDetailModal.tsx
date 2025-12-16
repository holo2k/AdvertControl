import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus } from "lucide-react";

import {
    Dialog,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "../ui/dialog.tsx";

import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "../ui/select.tsx";

import { Button } from "../ui/button.tsx";
import { apiClient } from "../../api/apiClient.ts";
import { toast } from "../ui/sonner";
import type {SignageConfig} from "../SignageCreatorPage/types.ts";

interface ScreenDetailModalProps {
    dialogOpen: boolean;
    setDialogOpen: (open: boolean) => void;
    screenId: string;
}

export const ScreenDetailModal = ({
                                      dialogOpen,
                                      setDialogOpen,
                                      screenId,
                                  }: ScreenDetailModalProps) => {
    const navigate = useNavigate();

    const [mode, setMode] = useState<"from-current" | "new" | "">("");
    const [configs, setConfigs] = useState<SignageConfig[]>([]);
    const [selectedConfigId, setSelectedConfigId] = useState<string>("");
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (mode !== "from-current") {
            setConfigs([]);
            setSelectedConfigId("");
            return;
        }

        const fetchConfigs = async () => {
            try {
                setLoading(true);
                // Убрали деструктуризацию - response.data уже содержит массив
                const response = await apiClient.get<SignageConfig[]>("/config/current");

                // Проверяем структуру ответа
                if (Array.isArray(response.data)) {
                    setConfigs(response.data);
                } else if (response.data && Array.isArray(response.data.configs)) {
                    // Если ответ приходит в формате { configs: [...] }
                    setConfigs(response.data);
                } else {
                    console.warn("Неожиданная структура ответа:", response.data);
                    setConfigs([]);
                }

            } catch (error) {
                console.error("Ошибка загрузки конфигураций:", error);
                toast.error("Не удалось загрузить конфигурации");
                setConfigs([]);
            } finally {
                setLoading(false);
            }
        };

        fetchConfigs();
    }, [mode]);

    const handleCreate = () => {
        if (mode === "new") {
            navigate(`/screen/${screenId}/config`);
            return;
        }

        if (mode === "from-current") {
            if (!selectedConfigId) {
                toast.error("Выберите конфигурацию");
                return;
            }

            navigate(`/screen/${screenId}/config`, {
                state: {
                    configId: selectedConfigId,
                },
            });
        }
    };

    return (
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
            <DialogTrigger asChild>
                <Button>
                    <Plus className="w-4 h-4 mr-2" />
                    Создать конфиг
                </Button>
            </DialogTrigger>

            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Создать конфигурацию</DialogTitle>
                </DialogHeader>

                <div className="space-y-4">
                    <Select value={mode} onValueChange={(v) => setMode(v as any)}>
                        <SelectTrigger>
                            <SelectValue placeholder="Выберите вариант" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="from-current">
                                На основе текущего
                            </SelectItem>
                            <SelectItem value="new">
                                Создать новый конфиг
                            </SelectItem>
                        </SelectContent>
                    </Select>

                    {mode === "from-current" && (
                        <>
                            {loading ? (
                                <div className="py-4 text-center text-sm text-muted-foreground">
                                    Загрузка конфигураций...
                                </div>
                            ) : configs.length === 0 ? (
                                <div className="py-4 text-center text-sm text-muted-foreground">
                                    Нет доступных конфигураций
                                </div>
                            ) : (
                                <Select
                                    value={selectedConfigId}
                                    onValueChange={setSelectedConfigId}
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Выберите конфигурацию" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {configs.map((config) => (
                                            <SelectItem key={config.id} value={config.id}>
                                                {config.name || "Без названия"} •{" "}
                                                {config.screensCount || 0} экран(ов)
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            )}
                        </>
                    )}
                </div>

                <DialogFooter>
                    <Button
                        variant="outline"
                        onClick={() => setDialogOpen(false)}
                        disabled={loading}
                    >
                        Отменить
                    </Button>

                    <Button
                        onClick={handleCreate}
                        disabled={mode === "from-current" && !selectedConfigId}
                    >
                        Перейти
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};


const styles = {
    container: {
        backgroundColor: "#f9fafb",
    },
    inner: {
        maxWidth: "1920px",
    },
    header: {
        marginBottom: "32px",
    },
    title: {
        fontSize: "24px",
        fontWeight: "600",
        color: "#111827",
        marginBottom: "4px",
    },
    subtitle: {
        color: "#6b7280",
        fontSize: "14px",
    },
    grid: {
        display: "flex",
        gap: "24px",
        flexWrap: "wrap",
    },
    infoBlock: {
        backgroundColor: "#ffffff",
        borderRadius: "8px",
        border: "1px solid #e5e7eb",
        padding: "24px",
        width: "33.333%",
        minWidth: "320px",
        flex: "1 1 320px",
        display: "flex",
        flexDirection: "column",
    },
    previewBlock: {
        backgroundColor: "#ffffff",
        borderRadius: "8px",
        border: "1px solid #e5e7eb",
        padding: "24px",
        width: "66.666%",
        minWidth: "400px",
        flex: "2 1 400px",
    },
    blockTitle: {
        fontSize: "18px",
        fontWeight: "600",
        color: "#111827",
        marginBottom: "16px",
    },
    statusWrapper: {
        display: "flex",
        alignItems: "center",
        gap: "12px",
        marginBottom: "16px",
    },
    statusIcon: {
        fontSize: "20px",
    },
    statusBadge: {
        padding: "4px 12px",
        borderRadius: "9999px",
        fontSize: "14px",
        fontWeight: "500",
    },
    infoList: {
        display: "flex",
        flexDirection: "column",
        gap: "16px",
    },
    infoItem: {
        display: "flex",
        gap: "12px",
    },
    icon: {
        fontSize: "20px",
        flexShrink: 0,
    },
    infoLabel: {
        color: "#6b7280",
        fontSize: "14px",
        marginBottom: "2px",
    },
    infoValue: {
        color: "#111827",
        fontSize: "16px",
    },
    configSection: {
        marginTop: "24px",
        paddingTop: "24px",
        borderTop: "1px solid #e5e7eb",
    },
    configName: {
        fontSize: "16px",
        color: "#111827",
        marginBottom: "4px",
    },
    configMeta: {
        fontSize: "14px",
        color: "#6b7280",
    },
    primaryButton: {
        backgroundColor: "#2563EB",
        color: "#ffffff",
        padding: "8px 16px",
        borderRadius: "6px",
        border: "none",
        fontSize: "14px",
        fontWeight: "500",
        cursor: "pointer",
        display: "inline-flex",
        alignItems: "center",
        gap: "8px",
    },
    plusIcon: {
        fontSize: "18px",
        fontWeight: "bold",
    },
    dialogBody: {
        padding: "16px 0",
        display: "flex",
        flexDirection: "column",
        gap: "16px",
    },
    dialogText: {
        fontSize: "14px",
        color: "#4b5563",
    },
    noConfigText: {
        color: "#6b7280",
        fontSize: "14px",
    },
};
