import { Monitor, MapPin, Activity } from "lucide-react";
import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import {useNavigate} from "react-router-dom";
import type { AppDispatch, RootState } from "../../store/store.ts";
import { fetchScreen } from "../../store/screenSlice.ts";
import { toast } from "../ui/sonner.tsx";
import { ConfigPreview } from "./ConfigPreview.tsx";
import {ScreenDetailModal} from "./ScreenDetailModal.tsx";
import { Button } from "../ui/button.tsx";
import {getStatusBadge} from "../ScreensPage/StatusBadge.tsx";
import { formatDateTimeShort} from "../../utils.ts";
import {Label} from "../ui/label.tsx";

export function ScreenDetail() {
    const { id } = useParams<{ id: string }>();
    const dispatch = useDispatch<AppDispatch>();
    const [dialogOpen, setDialogOpen] = useState(false);
    const navigate = useNavigate();

    const { currentScreen, currentStatus } = useSelector(
        (state: RootState) => state.screens
    );

    useEffect(() => {
        if (id) dispatch(fetchScreen(id));
    }, [id, dispatch]);

    useEffect(() => {
        if (currentStatus === "failed") {
            toast.error("Ошибка загрузки данных");
        }
    }, [currentStatus]);


    const screenData = {
        id,
        name: currentScreen?.screen?.name ?? "",
        location: currentScreen?.screen.location ?? "",
        resolution: currentScreen?.screen.resolution ?? "",
        status: getStatusBadge(currentScreen?.screen.status ?? ""),
        config: currentScreen?.config,
        pairedAt: formatDateTimeShort(currentScreen?.screen.pairedAt),
        createdAt: formatDateTimeShort(currentScreen?.screen.createdAt),
        updatedAt: formatDateTimeShort(currentScreen?.screen.updatedAt),
    };

    const hasConfig = Boolean(currentScreen?.config?.id);

    return (
        <div style={styles.container}>
            <style jsx>{`
      @media (max-width: 1024px) {
        .layout-grid {
          flex-direction: column;
        }
        .info-block,
        .preview-block {
          width: 100% !important;
          max-width: 100% !important;
        }
      }
    `}</style>

            <div style={styles.inner}>
                {/* Header */}


                {/* Main layout */}
                <div className="layout-grid" style={styles.grid}>
                    {/* LEFT - Информация об экране */}
                    <div className="info-block" style={styles.infoBlock}>
                        <div style={styles.header}>
                            <h1 style={styles.title}>{screenData.name}</h1>
                            <p style={styles.subtitle}>Screen ID: {screenData.id}</p>
                        </div>
                        <h2 style={styles.blockTitle}>Информация об экране</h2>

                        <div style={styles.statusWrapper}>
                            <Activity className="w-5 h-5 text-gray-400" />
                            <div>
                                <p style={styles.infoLabel}>Статус</p>
                                <p style={styles.infoValue}>{screenData.status}</p>
                            </div>

                        </div>

                        <div style={styles.infoList}>
                            <div style={styles.infoItem}>
                                <MapPin className="w-5 h-5 text-gray-400" />
                                <div>
                                    <p style={styles.infoLabel}>Расположение</p>
                                    <p style={styles.infoValue}>{screenData.location}</p>
                                </div>
                            </div>

                            <div style={styles.infoItem}>
                                <Monitor className="w-5 h-5 text-gray-400" />
                                <div>
                                    <p style={styles.infoLabel}>Разрешение</p>
                                    <p style={styles.infoValue}>{screenData.resolution}</p>
                                </div>
                            </div>
                            <div style={styles.infoItem}>
                                <div style={{marginTop: "10px", marginBottom: "-10px"}}>
                                    <Label>Последнее обновление</Label>
                                    <p style={styles.infoValue}>{screenData.updatedAt}</p>
                                </div>
                            </div>
                        </div>

                        {/* CONFIG BLOCK */}
                        <div style={styles.configSection}>


                            {hasConfig ? (
                                <>
                                <h3 style={styles.blockTitle}>Конфигурация</h3>
                                <div className="flex justify-between gap-2">

                                    <div>
                                        <p style={styles.configName}>{screenData.config?.name || "Название не задано"}</p>
                                        <p style={styles.configMeta}>
                                            {screenData.config!.items.length} объектов •{" "}
                                            {screenData.config!.screensCount} экранов
                                        </p>
                                    </div>
                                    <Button onClick={() => navigate(`/screen/${id}/config/edit`, {
                                        state: {
                                            configId: screenData.config.id,
                                        },
                                    })}>Перейти</Button>
                                </div>
                                </>
                            ) : (
                                <ScreenDetailModal
                                    dialogOpen={dialogOpen}
                                    setDialogOpen={setDialogOpen}
                                    screenId={screenData.id}/>
                            )}
                        </div>
                    </div>

                    {/* RIGHT - Предпросмотр */}
                    <div className="preview-block" style={styles.previewBlock}>
                        <h2 style={styles.blockTitle}>Предпросмотр</h2>

                        {hasConfig ? (
                            <ConfigPreview
                                config={currentScreen?.config}
                                resolution={screenData.resolution}
                            />
                        ) : (
                            <div style={styles.noConfigText}>
                                Конфигурация ещё не создана
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}

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
