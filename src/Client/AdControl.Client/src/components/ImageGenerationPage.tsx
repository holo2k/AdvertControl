import { useState } from "react";
import ContentLoader from "react-content-loader";
import { Sparkles, Image as ImageIcon, Upload } from "lucide-react";

import { Card } from "./ui/card";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Textarea } from "./ui/textarea";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "./ui/tabs";
import { Label } from "./ui/label";

import { genAI } from "../ai/geminiClient";
import {toast} from "./ui/sonner.tsx";

/* ===================== LOADER ===================== */

const ImageLoader = () => (
    <ContentLoader
        speed={2}
        width={400}
        height={300}
        backgroundColor="#f3f3f3"
        foregroundColor="#ecebeb"
    >
        <rect x="0" y="0" rx="12" ry="12" width="400" height="300" />
    </ContentLoader>
);

/* ===================== UTILS ===================== */

const fileToGenerativePart = async (file: File) => {
    const base64 = await file.arrayBuffer().then((b) =>
        btoa(
            new Uint8Array(b).reduce(
                (data, byte) => data + String.fromCharCode(byte),
                ""
            )
        )
    );

    return {
        inlineData: {
            data: base64,
            mimeType: file.type,
        },
    };
};

/* ===================== COMPONENT ===================== */

export function ImageGenerationPage() {
    const [prompt, setPrompt] = useState("");
    const [imageFile, setImageFile] = useState<File | null>(null);

    const [imageUrl, setImageUrl] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);

    /* ===================== TEXT → IMAGE ===================== */

    const generateFromText = async () => {
        if (!prompt) return toast.error("Введите описание");

        try {
            setLoading(true);
            setImageUrl(null);

            const model = genAI.getGenerativeModel({
                model: "imagen-3.0-generate-001",
            });

            const result = await model.generateContent(prompt);
            const response = await result.response;

            const image = response.candidates?.[0]?.content?.parts?.find(
                (p: any) => p.inlineData
            );

            if (!image?.inlineData?.data) {
                throw new Error("Изображение не получено");
            }

            setImageUrl(
                `data:image/png;base64,${image.inlineData.data}`
            );
        } catch (e) {
            console.error(e);
            toast.error("Ошибка генерации изображения");
        } finally {
            setLoading(false);
        }
    };

    /* ===================== IMAGE → IMAGE ===================== */

    const generateFromImage = async () => {
        if (!imageFile || !prompt) {
            toast.error("Добавьте изображение и описание");
            return;
        }

        try {
            setLoading(true);
            setImageUrl(null);

            const model = genAI.getGenerativeModel({
                model: "gemini-1.5-flash",
            });

            const imagePart = await fileToGenerativePart(imageFile);

            const result = await model.generateContent([
                prompt,
                imagePart,
            ]);

            const response = await result.response;

            const image = response.candidates?.[0]?.content?.parts?.find(
                (p: any) => p.inlineData
            );

            if (!image?.inlineData?.data) {
                throw new Error("Изображение не получено");
            }

            setImageUrl(
                `data:image/png;base64,${image.inlineData.data}`
            );
        } catch (e) {
            console.error(e);
            toast.error("Ошибка обработки изображения");
        } finally {
            setLoading(false);
        }
    };

    /* ===================== RENDER ===================== */

    return (
        <div className="space-y-6">
            <div>
                <h1>Генерация изображений</h1>
                <p className="text-gray-600 mt-1">
                    ИИ-генерация по текстовому описанию и изображению
                </p>
            </div>

            <Tabs defaultValue="text">
                <TabsList>
                    <TabsTrigger value="text">
                        <Sparkles className="h-4 w-4 mr-2" />
                        По тексту
                    </TabsTrigger>
                    <TabsTrigger value="image">
                        <ImageIcon className="h-4 w-4 mr-2" />
                        По изображению
                    </TabsTrigger>
                </TabsList>

                {/* TEXT → IMAGE */}
                <TabsContent value="text">
                    <Card className="p-6 space-y-4">
                        <div className="space-y-2">
                            <Label>Описание</Label>
                            <Textarea
                                rows={4}
                                placeholder="Футуристичный город в стиле киберпанк"
                                value={prompt}
                                onChange={(e) => setPrompt(e.target.value)}
                            />
                        </div>

                        <Button
                            onClick={generateFromText}
                            disabled={loading}
                            className="gap-2"
                            style={{ backgroundColor: "#2563EB" }}
                        >
                            <Sparkles className="h-4 w-4" />
                            Сгенерировать
                        </Button>
                    </Card>
                </TabsContent>

                {/* IMAGE → IMAGE */}
                <TabsContent value="image">
                    <Card className="p-6 space-y-4">
                        <div className="space-y-2">
                            <Label>Изображение</Label>
                            <Input
                                type="file"
                                accept="image/*"
                                onChange={(e) =>
                                    setImageFile(
                                        e.target.files?.[0] || null
                                    )
                                }
                            />
                        </div>

                        <div className="space-y-2">
                            <Label>Описание изменений</Label>
                            <Textarea
                                rows={3}
                                placeholder="Сделай стиль аниме, добавь неон"
                                value={prompt}
                                onChange={(e) => setPrompt(e.target.value)}
                            />
                        </div>

                        <Button
                            onClick={generateFromImage}
                            disabled={loading}
                            className="gap-2"
                            style={{ backgroundColor: "#2563EB" }}
                        >
                            <Upload className="h-4 w-4" />
                            Обработать
                        </Button>
                    </Card>
                </TabsContent>
            </Tabs>

            {/* RESULT */}
            <Card className="p-6 flex justify-center">
                {loading ? (
                    <ImageLoader />
                ) : imageUrl ? (
                    <img
                        src={imageUrl}
                        alt="Generated"
                        className="rounded-xl max-w-full"
                    />
                ) : (
                    <span className="text-gray-400">
                        Результат появится здесь
                    </span>
                )}
            </Card>
        </div>
    );
}
