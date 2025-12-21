import {useEffect, useState, useRef, useMemo} from "react";
import type { SignageConfig } from "../SignageCreatorPage/types.ts";
import { Pause, Play, SkipBack, SkipForward } from "lucide-react";
import { MINIO_PUBLIC_URL } from "../../api/apiClient.ts";
import { buildMinioUrl, removeId } from "../../utils.ts";

interface ConfigPreviewProps {
    config: SignageConfig | null | undefined;
    resolution: string;
}

/**
 * Парсит строку "1920x1080"
 */
function parseResolution(resolution: string): { width: number; height: number } | null {
    if (!resolution) return null;

    const [w, h] = resolution.toLowerCase().split("x").map(Number);
    if (!w || !h) return null;

    return { width: w, height: h };
}

export function ConfigPreview({ config, resolution }: ConfigPreviewProps) {
    const [currentItemIndex, setCurrentItemIndex] = useState(0);
    const [isPlaying, setIsPlaying] = useState(true);
    const videoRef = useRef<HTMLVideoElement>(null);

    const { isValidConfig, sortedItems, currentItem, itemsCount } = useMemo(() => {
        const isValid = config && Array.isArray(config.items) && config.items.length > 0;

        if (!isValid) {
            return {
                isValidConfig: false,
                sortedItems: [],
                currentItem: null,
                itemsCount: 0
            };
        }

        const sorted = [...config.items].sort((a, b) => (a.order || 0) - (b.order || 0));
        const current = sorted[currentItemIndex];

        return {
            isValidConfig: true,
            sortedItems: sorted,
            currentItem: current,
            itemsCount: sorted.length
        };
    }, [config, currentItemIndex]);

    const parsedResolution = useMemo(() => parseResolution(resolution), [resolution]);

    const aspectRatio = useMemo(() =>
            parsedResolution ? parsedResolution.width / parsedResolution.height : 16 / 9,
        [parsedResolution]
    );

    const mediaUrl = useMemo(() =>
            currentItem ? buildMinioUrl(MINIO_PUBLIC_URL, currentItem.url) : "",
        [currentItem]
    );

    const isVideo = useMemo(() => currentItem?.type === "VIDEO", [currentItem]);

    /** Обработка воспроизведения/паузы видео */
    useEffect(() => {
        if (!videoRef.current) return;

        const video = videoRef.current;

        if (isPlaying) {
            video.play().catch(error => {
                console.error("Ошибка воспроизведения видео:", error);
            });
        } else {
            video.pause();
        }
    }, [isPlaying, currentItemIndex]);

    /** Автоматическое переключение слайдов */
    useEffect(() => {
        if (!isPlaying) return;

        const duration = currentItem?.durationSeconds || 5;
        const timer = setTimeout(() => {
            setCurrentItemIndex((i) => (i + 1) % itemsCount);
        }, duration * 1000);

        return () => clearTimeout(timer);
    }, [currentItemIndex, isPlaying, currentItem?.durationSeconds, itemsCount]);

    /** Обработчик клика по кнопке play/pause */
    const handlePlayPause = () => {
        setIsPlaying(prev => !prev);
    };

    /** Обработчик переключения на следующий слайд */
    const handleNext = () => {
        if (itemsCount === 1) {
            // Если только один элемент, сбрасываем его воспроизведение
            if (isVideo && videoRef.current) {
                videoRef.current.currentTime = 0;
                videoRef.current.play().catch(console.error);
            }
            setCurrentItemIndex(0);
        } else {
            setCurrentItemIndex(i => (i + 1) % itemsCount);
        }

        if (!isPlaying) {
            setIsPlaying(true);
        }
    };

    /** Обработчик переключения на предыдущий слайд */
    const handlePrev = () => {
        if (itemsCount === 1) {
            // Если только один элемент, сбрасываем его воспроизведение
            if (isVideo && videoRef.current) {
                videoRef.current.currentTime = 0;
                videoRef.current.play().catch(console.error);
            }
            // И перезапускаем таймер для этого же элемента
            setCurrentItemIndex(0);
        } else {
            setCurrentItemIndex(i => i === 0 ? itemsCount - 1 : i - 1);
        }

        // Если было на паузе, автоматически запускаем воспроизведение
        if (!isPlaying) {
            setIsPlaying(true);
        }
    };

    /** Обработчик клика по индикатору прогресса */
    const handleProgressClick = (index: number) => {
        setCurrentItemIndex(index);
        // Если было на паузе, автоматически запускаем воспроизведение
        if (!isPlaying) {
            setIsPlaying(true);
        }
    };

    /** Обработчик события окончания видео */
    const handleVideoEnded = () => {
        // Если видео закончилось и автоплей включен, переходим к следующему
        if (isPlaying) {
            handleNext();
        }
    };

    /** Обработчик события ошибки видео */
    const handleVideoError = () => {
        console.error("Ошибка загрузки видео");
        // Переходим к следующему элементу через 1 секунду
        setTimeout(() => {
            if (isPlaying) {
                handleNext();
            }
        }, 1000);
    };
    if (!isValidConfig) {
        return (
            <div className="bg-gray-100 rounded-lg aspect-video flex items-center justify-center">
                <p className="text-gray-500">No content available</p>
            </div>
        );
    }

    return (
        <div className="space-y-4">
            {/* ХОЛСТ */}
            <div
                className="rounded-lg overflow-hidden relative mx-auto"
                style={{
                    aspectRatio,
                    border: "1px solid rgb(229, 231, 235)",
                }}
            >
                {mediaUrl ? (
                    isVideo ? (
                        <video
                            ref={videoRef}
                            src={mediaUrl}
                            autoPlay={isPlaying}
                            muted
                            loop={false} // Не используем loop, т.к. управляем вручную
                            playsInline
                            className="absolute inset-0 w-full h-full"
                            style={{ objectFit: "cover" }}
                            onEnded={handleVideoEnded}
                            onError={handleVideoError}
                        />
                    ) : (
                        <img
                            src={mediaUrl}
                            alt={removeId(currentItem?.url || "")}
                            className="absolute inset-0 w-full h-full object-cover"
                            style={{ objectFit: "cover" }}
                            onError={() => console.error("Ошибка загрузки изображения")}
                        />
                    )
                ) : (
                    <div className="text-white flex items-center justify-center h-full">
                        No media
                    </div>
                )}
            </div>

            {/* ИНФО + КНОПКИ */}
            <div className="flex items-center justify-between">
                <div className="text-sm text-gray-600">
                    <div className="font-medium">{removeId(currentItem?.url || "")}</div>
                    <div className="text-xs">
                        {currentItemIndex + 1}/{itemsCount} • {resolution || "N/A"}
                    </div>
                </div>

                <div className="flex gap-2">
                    <button
                        onClick={handlePrev}
                        className="p-2 hover:bg-gray-100 rounded transition-colors"
                        aria-label="Предыдущий"
                    >
                        <SkipBack className="w-5 h-5" />
                    </button>

                    <button
                        onClick={handlePlayPause}
                        className="p-2 hover:bg-gray-100 rounded transition-colors"
                        aria-label={isPlaying ? "Пауза" : "Воспроизвести"}
                    >
                        {isPlaying ? <Pause className="w-5 h-5" /> : <Play className="w-5 h-5" />}
                    </button>

                    <button
                        onClick={handleNext}
                        className="p-2 hover:bg-gray-100 rounded transition-colors"
                        aria-label="Следующий"
                    >
                        <SkipForward className="w-5 h-5" />
                    </button>
                </div>
            </div>

            {/* ПРОГРЕСС */}
            <div className="flex gap-1">
                {sortedItems.map((_, i) => (
                    <button
                        key={i}
                        onClick={() => handleProgressClick(i)}
                        className={`h-1 flex-1 rounded cursor-pointer transition-all ${
                            i === currentItemIndex
                                ? "bg-blue-600"
                                : "bg-gray-300 hover:bg-gray-400"
                        }`}
                        aria-label={`Перейти к слайду ${i + 1}`}
                    />
                ))}
            </div>
        </div>
    );
}
