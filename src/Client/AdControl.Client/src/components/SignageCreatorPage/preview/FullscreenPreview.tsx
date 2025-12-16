import {useEffect, useState} from "react";
import {Pause, Play, X} from "lucide-react";
import {Button} from "../../ui/button.tsx";
import type {ContentItem} from "../types.ts";
import {PreviewContent} from "./PreviewContent.tsx";

export function FullscreenPreview({
                               items,
                               onClose,
                           }: {
    items: ContentItem[];
    onClose: () => void;
}) {
    const [currentIndex, setCurrentIndex] = useState(0);
    const [isPlaying, setIsPlaying] = useState(true);

    useEffect(() => {
        const handleEsc = (e: KeyboardEvent) => {
            if (e.key === "Escape") {
                onClose();
            }
        };

        window.addEventListener("keydown", handleEsc);
        return () => window.removeEventListener("keydown", handleEsc);
    }, [onClose]);

    useEffect(() => {
        if (!isPlaying || items.length === 0) return;

        const timer = setTimeout(() => {
            setCurrentIndex((prev) => (prev + 1) % items.length);
        }, items[currentIndex].durationSeconds * 1000);

        return () => clearTimeout(timer);
    }, [isPlaying, currentIndex, items]);

    return (
        <div className="fixed inset-0 bg-black z-50">
            <button
                onClick={onClose}
                className="absolute top-6 right-6 z-10 bg-white/10 hover:bg-white/20 text-white p-3 rounded-full backdrop-blur-sm transition-colors"
            >
                <X className="w-6 h-6" />
            </button>

            <div className="absolute bottom-6 left-6 z-10 flex gap-2">
                <Button
                    variant="outline"
                    className="bg-white/10 hover:bg-white/20 text-white border-white/20"
                    onClick={() => setIsPlaying(!isPlaying)}
                >
                    {isPlaying ? <Pause className="w-4 h-4" /> : <Play className="w-4 h-4" />}
                </Button>
            </div>

            <div className="absolute bottom-6 right-6 z-10 text-white bg-white/10 backdrop-blur-sm px-4 py-2 rounded-full">
                {currentIndex + 1} / {items.length}
            </div>

            <div className="w-full h-full">
                <PreviewContent item={items[currentIndex]}/>
            </div>
        </div>
    );
}
