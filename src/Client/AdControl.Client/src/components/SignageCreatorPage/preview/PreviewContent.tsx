import {Image as ImageIcon, Loader, Video} from "lucide-react";
import type { ContentItem } from "../types";

interface PreviewContentProps {
    item: ContentItem;
    transition: string;
}

export function PreviewContent({ item }: PreviewContentProps) {
    const getBackgroundColor = () => {
        if (item.type === "image") return (item.config?.backgroundColor as string) || "#000000";
        if (item.type === "text") return (item.config?.backgroundColor as string) || "#2563EB";
        return "#FFFFFF";
    };

    return (
        <div
            className="w-full h-full flex items-center justify-center"
            style={{ backgroundColor: getBackgroundColor() }}
        >
            {/* Table Preview */}
            {item.type === "table" && (
                <div className="w-full h-full p-8 overflow-auto">
                    <div className="bg-white rounded-lg overflow-hidden shadow-lg">
                        <div
                            className="p-4 text-white font-semibold"
                            style={{ backgroundColor: (item.config?.headerColor as string) || "#2563EB" }}
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
                                        className={
                                            item.config?.alternateRows && i % 2 === 0 ? "bg-gray-50" : ""
                                        }
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

            {/* Image Preview */}
            {item.type === "image" && item.config?.url ? (
                <img
                    src={item.config.url}
                    alt={item.name}
                    className="max-w-full max-h-full object-cover"
                    style={{
                        objectFit: (item.config.fit as "cover" | "contain" | "fill") || "cover",
                    }}
                />
            ) : item.type === "image" ? (
                <div className="w-full h-full flex items-center justify-center">
                    <ImageIcon className="w-32 h-32 text-white opacity-50" />
                    <span className="absolute text-black text-lg opacity-70">No image uploaded</span>
                </div>
            ) : null}

            {/* Video Preview */}
            {item.type === "video" && item.status === "processing" && (
                <div className="w-full h-full flex items-center justify-center bg-black">
                    <Loader className="w-16 h-16 text-white animate-spin" />
                    <span className="absolute text-white text-lg">Processing video...</span>
                </div>
            )}

            {item.type === "video" && item.status === "ready" && item.config?.url ? (
                <video
                    src={item.config.url}
                    autoPlay
                    muted={item.config.muted ?? true}
                    loop={item.config.loop ?? true}
                    playsInline
                    className="w-full h-full object-cover"
                />
            ) : item.type === "video" && item.status === "ready" ? (
                <div className="w-full h-full flex items-center justify-center bg-black">
                    <Video className="w-32 h-32 text-white/50" />
                    <span className="absolute text-black text-lg opacity-70">No video uploaded</span>
                </div>
            ) : null}

            {/* Text Preview */}
            {item.type === "text" && (
                <div
                    className="w-full h-full flex items-center justify-center p-12 text-center"
                    style={{
                        color: (item.config?.textColor as string) || "#FFFFFF",
                        fontSize: `${item.config?.fontSize || 48}px`,
                        textAlign: (item.config?.alignment as "left" | "center" | "right") || "center",
                    } as React.CSSProperties}
                >
                    {item.config?.content || "Enter your text here"}
                </div>
            )}
        </div>
    );
}
