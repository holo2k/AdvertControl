import { useRef } from "react";
import { Button } from "../../ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "../../ui/dropdown-menu";
import { Plus, FileText, Image as ImageIcon, Video } from "lucide-react";
import type { ContentItem, ContentType } from "../types";
import { toast } from "../../ui/sonner";
import {apiClient} from "../../../api/apiClient.ts";


interface Props {
    onAdd: (type: ContentType, item: ContentItem) => void;
}

/* ================= COMPONENT ================= */

export function AddContentButton({ onAdd }: Props) {
    const imageInputRef = useRef<HTMLInputElement>(null);
    const videoInputRef = useRef<HTMLInputElement>(null);

    /* ================= UPLOAD ================= */

    const uploadFile = async (file: File): Promise<string> => {
        const formData = new FormData();
        formData.append("file", file);

        const response = await apiClient.post("files/upload", formData, {
            headers: {
                "Content-Type": "multipart/form-data",
            },
        });

        return response.data.fileUrl;
    };

    /* ================= IMAGE ================= */

    const openImagePicker = () => imageInputRef.current?.click();

    const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        if (!file.type.startsWith("image/")) {
            toast.error("Выберите изображение");
            return;
        }

        if (file.size > 100 * 1024 * 1024) {
            toast.error("Изображение слишком большое (>100 МБ)");
            return;
        }

        try {
            const fileUrl = await uploadFile(file);

            const newItem: ContentItem = {
                type: "IMAGE",
                durationSeconds: 10,
                size: file.size,
                url: fileUrl,
                order: 1
            };

            onAdd("IMAGE", newItem);
            toast.success("Изображение загружено");
        } catch {
            toast.error("Ошибка загрузки изображения");
        } finally {
            e.target.value = "";
        }
    };

    /* ================= VIDEO ================= */

    const openVideoPicker = () => videoInputRef.current?.click();

    const handleVideoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        const validTypes = ["video/mp4", "video/webm", "video/quicktime"];
        if (!validTypes.includes(file.type)) {
            toast.error("Поддерживаемые форматы: MP4, WebM, MOV");
            return;
        }

        if (file.size > 100 * 1024 * 1024) {
            toast.error("Видео слишком большое (макс. 100 МБ)");
            return;
        }

        try {
            const fileUrl = await uploadFile(file);
            const durationSeconds = await getVideoDuration(file);
            const newItem: ContentItem = {
                type: "VIDEO",
                durationSeconds: durationSeconds,
                size: file.size,
                order: 0,
                url: fileUrl,
            };

            onAdd("VIDEO", newItem);
            toast.success("Видео загружено");
        } catch {
            toast.error("Ошибка загрузки видео");
        } finally {
            e.target.value = "";
        }
    };

    /* ================= RENDER ================= */

    return (
        <>
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button className="w-full gap-2" style={{ backgroundColor: "#f1f1f3", color: "#000", border: "1px solid #cdcbcb" }}>
                        <Plus className="w-4 h-4" /> Добавить контент
                    </Button>
                </DropdownMenuTrigger>

                <DropdownMenuContent className="w-56">
                    <DropdownMenuItem onSelect={openImagePicker}>
                        <ImageIcon className="w-4 h-4 mr-2" />
                        Загрузить изображение
                    </DropdownMenuItem>

                    <DropdownMenuItem onSelect={openVideoPicker}>
                        <Video className="w-4 h-4 mr-2" />
                        Загрузить видео
                    </DropdownMenuItem>

                    <DropdownMenuItem disabled>
                        <FileText className="w-4 h-4 mr-2" />
                        Загрузить таблицу
                    </DropdownMenuItem>

                    <DropdownMenuItem disabled>
                        <FileText className="w-4 h-4 mr-2" />
                        Добавить текст
                    </DropdownMenuItem>
                </DropdownMenuContent>
            </DropdownMenu>

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
                accept="video/mp4,video/webm,video/quicktime"
                className="hidden"
                onChange={handleVideoUpload}
            />
        </>
    );
}

const getVideoDuration = (file: File): Promise<number> => {
    return new Promise((resolve, reject) => {
        const video = document.createElement('video');
        video.preload = 'metadata';

        video.onloadedmetadata = () => {
            window.URL.revokeObjectURL(video.src);
            resolve(Math.ceil(video.duration));
        };

        video.onerror = () => {
            window.URL.revokeObjectURL(video.src);
            reject(new Error('Не удалось получить длительность видео'));
        };

        video.src = URL.createObjectURL(file);
    });
};
