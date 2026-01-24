import { Monitor, MapPin, Activity } from "lucide-react";
import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import {useNavigate} from "react-router-dom";
import type { AppDispatch, RootState } from "../../store/store.ts";
import { fetchScreen } from "../../store/screenSlice.ts";
import { toast } from "../ui/toast.ts";
import { ConfigPreview } from "./ConfigPreview.tsx";
import {ScreenDetailModal} from "./ScreenDetailModal.tsx";
import { Button } from "../ui/button.tsx";
import {getStatusBadge} from "../ScreensPage/StatusBadge.tsx";
import {formatDateTimeShort, getWordByCount} from "../../utils.ts";
import {Label} from "../ui/label.tsx";
import "./screen-detail.css"

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
        <div className="screen-detail-container">
            <div className="screen-detail-inner">
                {/* Header */}
                <div className="screen-detail-header">
                    <h1 className="screen-detail-title">{screenData.name}</h1>
                    <p className="screen-detail-subtitle">Screen ID: {screenData.id}</p>
                </div>

                {/* Main layout */}
                <div className="layout-grid">
                    {/* LEFT - Информация об экране */}
                    <div className="info-block">
                        <h2 className="block-title">Информация об экране</h2>

                        <div className="status-wrapper">
                            <Activity className="w-5 h-5 text-gray-400" />
                            <div>
                                <p className="info-label">Статус</p>
                                <p className="info-value">{screenData.status}</p>
                            </div>
                        </div>

                        <div className="info-list">
                            <div className="info-item">
                                <MapPin className="w-5 h-5 text-gray-400" />
                                <div>
                                    <p className="info-label">Расположение</p>
                                    <p className="info-value">{screenData.location}</p>
                                </div>
                            </div>

                            <div className="info-item">
                                <Monitor className="w-5 h-5 text-gray-400" />
                                <div>
                                    <p className="info-label">Разрешение</p>
                                    <p className="info-value">{screenData.resolution}</p>
                                </div>
                            </div>

                            <div className="info-item">
                                <div style={{ marginTop: "10px", marginBottom: "10px" }}>
                                    <Label>Последнее обновление</Label>
                                    <p className="info-value">{screenData.updatedAt}</p>
                                </div>
                            </div>
                        </div>

                        {/* CONFIG BLOCK */}
                        <div className="config-section">
                            {hasConfig ? (
                                <>
                                    <h3 className="block-title">Конфигурация</h3>
                                    <div className="flex justify-between gap-2">
                                        <div>
                                            <p className="config-name">{screenData.config?.name || "Название не задано"}</p>
                                            <p className="config-meta">
                                                {getWordByCount(screenData.config!.items?.length, ["объект", "объекта", "объектов"])}
                                                {" • "}
                                                {getWordByCount(screenData.config!.screensCount, ["экран", "экрана", "экранов"])}
                                            </p>
                                        </div>
                                        <Button onClick={() => navigate(`/crm/screen/${id}/config/edit`, {
                                            state: {
                                                configId: screenData.config?.id,
                                            },
                                        })}>
                                            Перейти
                                        </Button>
                                    </div>
                                </>
                            ) : (
                                <ScreenDetailModal
                                    dialogOpen={dialogOpen}
                                    setDialogOpen={setDialogOpen}
                                    screenId={screenData.id || ""}
                                />
                            )}
                        </div>
                    </div>

                    {/* RIGHT - Предпросмотр */}
                    <div className="preview-block">
                        <h2 className="block-title">Предпросмотр</h2>

                        {hasConfig ? (
                            <ConfigPreview
                                config={currentScreen?.config}
                                resolution={screenData.resolution}
                            />
                        ) : (
                            <div className="no-config-text">
                                Конфигурация ещё не создана
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
