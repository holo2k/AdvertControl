import { useEffect, useState } from "react";
import type { SignageConfig } from "../SignageCreatorPage/types.ts";
import { Pause, Play, SkipBack, SkipForward } from "lucide-react";
import { MINIO_PUBLIC_URL } from "../../api/apiClient.ts";
import {buildMinioUrl} from "../../utils.ts";

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

    if (!config || !Array.isArray(config.items) || config.items.length === 0) {
        return (
            <div className="bg-gray-100 rounded-lg aspect-video flex items-center justify-center">
                <p className="text-gray-500">No content available</p>
            </div>
        );
    }

    const sortedItems = [...config.items].sort(
        (a, b) => (a.order || 0) - (b.order || 0)
    );

    const currentItem = sortedItems[currentItemIndex];
    const itemsCount = sortedItems.length;

    const parsedResolution = parseResolution(resolution);
    const aspectRatio = parsedResolution
        ? parsedResolution.width / parsedResolution.height
        : 16 / 9;


    const mediaUrl = buildMinioUrl(MINIO_PUBLIC_URL, currentItem.url);

    const isVideo =
        currentItem.type === "VIDEO" ||
        currentItem.type === 1;

    /** autoplay */
    useEffect(() => {
        if (!isPlaying) return;

        const duration = currentItem.durationSeconds || 5;
        const timer = setTimeout(() => {
            setCurrentItemIndex((i) => (i + 1) % itemsCount);
        }, duration * 1000);

        return () => clearTimeout(timer);
    }, [currentItemIndex, isPlaying]);

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
                            src={mediaUrl}
                            autoPlay
                            muted
                            loop={isPlaying}
                            playsInline
                            className="absolute inset-0 w-full h-full"
                            style={{objectFit: "cover"}}
                        />
                    ) : (
                        <img
                            src={mediaUrl}
                            alt={currentItem.name}
                            className="absolute inset-0 w-full h-full object-cover"
                            style={{objectFit: "cover"}}
                        />
                    )
                ) : (
                    <div className="text-white flex items-center justify-center h-full">
                        No media
                    </div>
                )}
            </div>

            {/* ИНФО + КНОПКИ */}
            <div className="flex items-center justify-between" style={{bottom: 0}}>
                <div className="text-sm text-gray-600">
                    <div className="font-medium">{currentItem.url}</div>
                    <div className="text-xs">
                        {currentItemIndex + 1}/{itemsCount} • {resolution || "N/A"}
                    </div>
                </div>

                <div className="flex gap-2" >
                    <button onClick={() =>
                        setCurrentItemIndex(i => i === 0 ? itemsCount - 1 : i - 1)
                    }>
                        <SkipBack />
                    </button>

                    <button onClick={() => setIsPlaying(p => !p)}>
                        {isPlaying ? <Pause /> : <Play />}
                    </button>

                    <button onClick={() =>
                        setCurrentItemIndex(i => (i + 1) % itemsCount)
                    }>
                        <SkipForward />
                    </button>
                </div>
            </div>

            {/* ПРОГРЕСС */}
            <div className="flex gap-1">
                {sortedItems.map((_, i) => (
                    <div
                        key={i}
                        onClick={() => setCurrentItemIndex(i)}
                        className={`h-1 flex-1 rounded cursor-pointer ${
                            i === currentItemIndex
                                ? "bg-blue-100"
                                : "bg-gray-50"
                        }`}
                    />
                ))}
            </div>
        </div>
    );
}
