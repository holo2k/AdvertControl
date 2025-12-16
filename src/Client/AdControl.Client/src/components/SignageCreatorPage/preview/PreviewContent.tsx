import { Image as ImageIcon,  Video } from "lucide-react";
import type { ContentItem } from "../types";
import {MINIO_PUBLIC_URL} from "../../../api/apiClient.ts";

interface PreviewContentProps {
    item: ContentItem;
}

export function PreviewContent({ item }: PreviewContentProps) {


    const getImageUrl = () => {
        if (!item.url) return null;
        return `${MINIO_PUBLIC_URL}/${encodeURIComponent(item.url)}`;
    };

    const fullImageUrl = item.type === "IMAGE" ? getImageUrl() : null;

    return (
        <div
            className="w-full h-full flex items-center justify-center"
            style={{ backgroundColor: "#FFFFFF" }}
        >
            {/* TABLE */}
            {item.type === "TABLE" && (
                <div className="w-full h-full p-8 overflow-auto">
                    <div className="bg-white rounded-lg overflow-hidden shadow-lg">
                        <div
                            className="p-4 text-white font-semibold"
                            style={{ backgroundColor: "#2563EB" }}
                        >
                            <h3>Sample Data Table</h3>
                        </div>
                        <div className="p-4">
                            <table className="w-full text-sm">
                                <thead>
                                <tr className="border-b">
                                    <th className="text-left p-2 font-medium">Column 1</th>
                                    <th className="text-left p-2 font-medium">Column 2</th>
                                    <th className="text-left p-2 font-medium">Column 3</th>
                                </tr>
                                </thead>
                                <tbody>
                                {[1, 2, 3, 4, 5].map((i) => (
                                    <tr
                                        key={i}
                                        className={"bg-gray-50"}
                                    >
                                        <td className="p-2">Data {i}-1</td>
                                        <td className="p-2">Data {i}-2</td>
                                        <td className="p-2">Data {i}-3</td>
                                    </tr>
                                ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            )}

            {/* IMAGE — теперь через прямую MinIO ссылку */}
            {item.type === "IMAGE" && (
                <>
                    {fullImageUrl ? (
                        <img
                            src={fullImageUrl}
                            alt={item.url}
                            className="max-w-full max-h-full object-cover"
                            style={{objectFit: "cover"}}
                            onError={(e) => {
                                (e.target as HTMLImageElement).style.display = "none";
                                const placeholder = e.currentTarget.nextElementSibling as HTMLElement;
                                if (placeholder) placeholder.classList.remove("hidden");
                            }}
                        />
                    ) : null}

                    {/* Заглушка, если нет URL или ошибка загрузки */}
                    {(!fullImageUrl) && (
                        <div className="hidden flex flex-col items-center justify-center text-white/70">
                            <ImageIcon className="w-32 h-32 opacity-50" />
                            <span className="mt-4 text-lg">No image uploaded</span>
                        </div>
                    )}
                </>
            )}

            {/* VIDEO — если добавишь позже */}
            {item.type === "VIDEO" && (
                // аналогично, если video тоже в MinIO
                <>
                    {item.url ? (
                        <video
                            src={`${MINIO_PUBLIC_URL}/${encodeURIComponent(item.url)}`}
                            autoPlay
                            playsInline
                            className="w-full h-full object-cover"
                        />
                    ) : (
                        <div className="w-full h-full flex items-center justify-center bg-black">
                            <Video className="w-32 h-32 text-white/50" />
                            <span className="ml-4 text-white/70 text-lg">No video uploaded</span>
                        </div>
                    )}
                </>
            )}

            {/* TEXT */}
            {item.type === "TEXT" && (
                <div
                    className="w-full h-full flex items-center justify-center p-12 text-center"
                    style={{
                        color: "#FFFFFF",
                        fontSize: `48px`,
                        textAlign: "center",
                    } as React.CSSProperties}
                >
                    {"Enter your text here"}
                </div>
            )}
        </div>
    );
}
