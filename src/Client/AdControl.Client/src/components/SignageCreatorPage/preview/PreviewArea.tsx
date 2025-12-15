import { Card, CardHeader, CardContent } from "../../ui/card";
import { Button } from "../../ui/button";
import {Play, Pause, Square, Upload, Maximize2} from "lucide-react";
import { PreviewContent } from "./PreviewContent";
import type { ContentItem, SignageConfig } from "../types";
import {useEffect} from "react";

interface PreviewAreaProps {
    config: SignageConfig;
    items: ContentItem[];
    currentIndex: number;
    setCurrentIndex: React.Dispatch<React.SetStateAction<number>>;
    isPlaying: boolean;
    setIsPlaying: React.Dispatch<React.SetStateAction<boolean>>;
    onFullScreen: React.Dispatch<React.SetStateAction<boolean>>;
    contentItems: ContentItem[];
}

export function PreviewArea({
                                config,
                                items,
                                currentIndex,
                                setCurrentIndex,
                                isPlaying,
                                setIsPlaying,
                                onFullScreen,
                                contentItems,
                            }: PreviewAreaProps) {

    useEffect(() => {
        if (!isPlaying || items.length === 0) return;

        const currentItem = items[currentIndex];
        const duration = (currentItem?.duration || config.defaultDuration) * 1000; // в мс

        const timer = setTimeout(() => {
            setCurrentIndex((prev) => (prev + 1) % items.length);
        }, duration);

        return () => clearTimeout(timer);
    }, [isPlaying, currentIndex, items, config.defaultDuration]);

    // Остановка при смене items
    useEffect(() => {
        setIsPlaying(false);
    }, [items.length]);


    return (
        <Card className="flex-1 flex flex-col overflow-hidden">
            <CardHeader className="bg-gray-50" style={{ padding: "8px", paddingBottom: "0px" }} >
                <div className="flex items-center justify-between">
                    <div className="flex items-center justify-between">
                        <Button variant="outline" onClick={() => onFullScreen(true)} disabled={contentItems.length === 0} className="gap-2">
                            <Maximize2 className="w-4 h-4" /> На весь экран
                        </Button>
                    </div>
                    {items.length > 0 && (
                        <div className="flex items-center gap-3">
              <span className="text-sm text-gray-600">
                {currentIndex + 1} / {items.length}
              </span>
                            <div className="flex gap-1">
                                <Button variant="outline" size="sm" onClick={() => setIsPlaying(!isPlaying)}>
                                    {isPlaying ? <Pause className="w-4 h-4" /> : <Play className="w-4 h-4" />}
                                </Button>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => {
                                        setIsPlaying(false);
                                    }}
                                >
                                    <Square className="w-4 h-4" />
                                </Button>
                            </div>
                        </div>
                    )}
                </div>
            </CardHeader>

            <CardContent className="flex-1 flex items-center justify-center p-2 bg-gray-100">
                {items.length === 0 ? (
                    <div className="text-center">
                        <div className="w-32 h-32 mx-auto mb-6 bg-gray-200 rounded-xl flex items-center justify-center">
                            <Upload className="w-16 h-16 text-gray-400" />
                        </div>
                        <h3 className="text-xl mb-2">Контент не добавлен</h3>
                        <p className="text-gray-600 mb-6">
                            Добавьте объекты с боковой панели для предварительного просмотра
                        </p>
                    </div>
                ) : (
                    <div>
                        <PreviewContent
                            item={items[currentIndex]}
                            transition={config.transition}
                        />
                    </div>
                )}
            </CardContent>
        </Card>
    );
}
