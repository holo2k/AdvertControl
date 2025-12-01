// src/components/signage/content/AddContentButton.tsx
import { Button } from "../../ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "../../ui/dropdown-menu";
import { Plus, FileText, Image as ImageIcon, Video } from "lucide-react";
import type { ContentType } from "../types";
import { toast } from "../../ui/sonner";
import { useRef } from "react";

interface Props {
    onAdd: (type: ContentType, config?: any) => void;
}

export function AddContentButton({ onAdd }: Props) {
    const imageInputRef = useRef<HTMLInputElement>(null);
    const videoInputRef = useRef<HTMLInputElement>(null);

    // === Загрузка изображения ===
    const openImagePicker = () => imageInputRef.current?.click();
    const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        if (!file.type.startsWith("image/")) return toast.error("Выберите изображение");
        if (file.size > 15 * 1024 * 1024) return toast.error("Изображение слишком большое (>15 МБ)");

        const reader = new FileReader();
        reader.onload = () => {
            onAdd("image", {
                url: reader.result as string,
                fit: "cover",
                animation: "none",
                backgroundColor: "#000000",
            });
            toast.success(`${file.name} загружено`);
        };
        reader.readAsDataURL(file);
        e.target.value = "";
    };

    // === Загрузка видео ===
    const openVideoPicker = () => videoInputRef.current?.click();
    const handleVideoUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        const validTypes = ["video/mp4", "video/webm", "video/quicktime"];
        if (!validTypes.includes(file.type)) {
            return toast.error("Поддерживаемые форматы: MP4, WebM, MOV");
        }

        // 100 МБ лимит — можно увеличить
        if (file.size > 100 * 1024 * 1024) {
            return toast.error("Видео слишком большое (макс. 100 МБ)");
        }

        const reader = new FileReader();
        reader.onload = () => {
            onAdd("video", {
                url: reader.result as string,
                volume: 50,
                loop: true,
                muted: true,
                quality: "auto",
            });
            toast.success(`${file.name} загружено`);
        };
        reader.onerror = () => toast.error("Ошибка чтения видео");
        reader.readAsDataURL(file);

        e.target.value = "";
    };

    return (
        <>
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button className="w-full gap-2" style={{ backgroundColor: "#2563EB" }}>
                        <Plus className="w-4 h-4" /> Добавить контент
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-56">
                    <DropdownMenuItem onSelect={() => onAdd("table")}>
                        <FileText className="w-4 h-4 mr-2" /> Загрузить таблицу
                    </DropdownMenuItem>

                    <DropdownMenuItem onSelect={openImagePicker}>
                        <ImageIcon className="w-4 h-4 mr-2" /> Загрузить изображение
                    </DropdownMenuItem>

                    <DropdownMenuItem onSelect={openVideoPicker}>
                        <Video className="w-4 h-4 mr-2" /> Загрузить видео
                    </DropdownMenuItem>

                    <DropdownMenuItem onSelect={() => onAdd("text")}>
                        <FileText className="w-4 h-4 mr-2" /> Добавить текст
                    </DropdownMenuItem>
                </DropdownMenuContent>
            </DropdownMenu>

            {/* Скрытые инпуты */}
            <input
                ref={imageInputRef}
                type="file"
                accept="image/*"
                className="hidden"
                onChange={handleImageUpload}
            />
            <input
                ref={videoInputRef}
                type="file"
                accept="video/mp4,video/webm"
                className="hidden"
                onChange={handleVideoUpload}
            />
        </>
    );
}
