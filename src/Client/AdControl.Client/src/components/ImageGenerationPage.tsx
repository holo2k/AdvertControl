import { useState } from "react";
import { Sparkles } from "lucide-react";

import { Card } from "./ui/card";
import { Button } from "./ui/button";
import { Textarea } from "./ui/textarea";
import { toast } from "./ui/sonner.tsx";
import { apiClient, MINIO_PUBLIC_URL } from "../api/apiClient.ts";

export function ImageGenerationPage() {
    const [prompt, setPrompt] = useState("");
    const [imageUrl, setImageUrl] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [generating, setGenerating] = useState(false);

    const generateFromText = async () => {
        if (!prompt.trim()) {
            toast.error("Введите описание");
            return;
        }

        try {
            setLoading(true);
            setGenerating(true);
            setImageUrl(null);

            const response = await apiClient.post("image/generate/", {
                prompt: prompt,
            });
            console.log("response", response);
            if (response.data.fileUrl) {
                const fullImageUrl = `${MINIO_PUBLIC_URL}/${response.data.fileUrl}`;
                setImageUrl(fullImageUrl);
            } else {
                throw new Error("Не получено имя файла в ответе");
            }
        } catch (error: any) {
            console.error("Ошибка генерации:", error);
            toast.error(
                error.response?.data?.message ||
                error.response?.data?.detail ||
                "Ошибка генерации изображения"
            );
        } finally {
            setLoading(false);
            setTimeout(() => setGenerating(false), 300);
        }
    };

    return (
        <div className="space-y-6">
            <div>
                <h1 >Создать изображение</h1>
                <p className="text-gray-600">
                    ИИ-генерация по текстовому описанию
                </p>
            </div>

            <Card className="p-4 mt-auto">
                {imageUrl && (
                    <div className="flex justify-center">
                        <img
                            src={imageUrl}
                            alt="Generated"
                            className="rounded-xl image-generated"
                            onError={() => {
                                toast.error("Не удалось загрузить изображение");
                                setImageUrl(null);
                            }}
                        />
                    </div>
                )}
                <Textarea
                    rows={10}
                    placeholder="Описание"
                    value={prompt}
                    onChange={(e) => setPrompt(e.target.value)}
                    style={{ minHeight: "80px", maxHeight: "120px" }}
                />

                <Button
                    onClick={generateFromText}
                    disabled={loading}
                    className="gap-2 relative"
                    style={{ backgroundColor: "#2563EB" }}
                >
                    {generating && (
                        <div className="absolute inset-0 bg-blue-600">
                            <div className="absolute inset-0 flex items-center justify-center">
                                <div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent"></div>
                            </div>
                        </div>
                    )}

                    <Sparkles className={`h-4 w-4 ${generating ? 'invisible' : ''}`} />
                    <span className={generating ? 'invisible' : ''}>
            {loading ? 'Генерация...' : 'Сгенерировать'}
          </span>
                </Button>
            </Card>

        </div>
    );
}
