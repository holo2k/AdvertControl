import {useEffect, useState} from "react";
import {X} from "lucide-react";
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
        if (items.length === 0) return;

        const timer = setTimeout(() => {
            setCurrentIndex((prev) => (prev + 1) % items.length);
        }, items[currentIndex].durationSeconds * 1000);

        return () => clearTimeout(timer);
    }, [currentIndex, items]);

    return (
        <div className="fixed inset-0 bg-black z-50">
            <button
                onClick={onClose}
                style={{right: "6px", color: "gray"}}
                className="absolute top-6 z-10 bg-white/10 hover:bg-white/20 text-white p-3 rounded-full backdrop-blur-sm transition-colors"
            >
                <X className="w-6 h-6" />
            </button>

            <div style={{color: "gray"}} className="absolute bottom-6 right-6 z-10 text-white bg-white/10 backdrop-blur-sm px-4 py-2 rounded-full">
                {currentIndex + 1} / {items.length}
            </div>

            <div className="w-full h-full">
                <PreviewContent item={items[currentIndex]}/>
            </div>
        </div>
    );
}
