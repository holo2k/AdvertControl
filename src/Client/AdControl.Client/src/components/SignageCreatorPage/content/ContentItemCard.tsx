import { GripVertical, Clock, CheckCircle, Loader2, AlertCircle, Copy, Trash2 } from "lucide-react";
import { Button } from "../../ui/button";
import {
    FileText as FileTextIcon,
    Image as ImageIcon,
    Video as VideoIcon,
} from "lucide-react";
import type { ContentItem } from "../types";

interface Props {
    item: ContentItem;
    isSelected: boolean;
    isDragging: boolean;
    onSelect: () => void;
    onDuplicate: () => void;
    onDelete: () => void;
    onDragStart: () => void;
    onDragOver: (e: React.DragEvent) => void;
    onDragEnd: () => void;
}

export function ContentItemCard({
                                    item,
                                    isSelected,
                                    isDragging,
                                    onSelect,
                                    onDuplicate,
                                    onDelete,
                                    onDragStart,
                                    onDragOver,
                                    onDragEnd,
                                }: Props) {
    const iconClass = "w-4 h-4";

    const typeIcons: Record<ContentItem["type"], React.JSX.Element> = {
        table: <FileTextIcon className={`${iconClass} text-blue-600`} />,
        image: <ImageIcon className={`${iconClass} text-green-600`} />,
        video: <VideoIcon className={`${iconClass} text-purple-600`} />,
        text: <FileTextIcon className={`${iconClass} text-orange-600`} />,
    };

    return (
        <div
            draggable
            onDragStart={onDragStart}
            onDragOver={onDragOver}
            onDragEnd={onDragEnd}
            onClick={onSelect}
            className={`
        p-3 border rounded-lg cursor-pointer transition-all
        ${isSelected ? "border-blue-500 bg-blue-50" : "border-gray-200 hover:border-gray-300"}
        ${isDragging ? "opacity-50" : ""}
      `}
        >
            <div className="flex items-start gap-3">
                <GripVertical className="w-5 h-5 text-gray-400 mt-1 flex-shrink-0" />
                <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                        {typeIcons[item.type]}
                        <span className="text-sm truncate">{item.name}</span>
                    </div>
                    <div className="flex items-center gap-2 text-xs text-gray-500">
                        <Clock className="w-3 h-3" />
                        {item.duration}s
                        {item.status === "ready" && <CheckCircle className="w-3 h-3 text-green-600" />}
                        {item.status === "processing" && (<Loader2 className="w-3 h-3 text-blue-600 animate-spin" />)}
                        {item.status === "error" && <AlertCircle className="w-3 h-3 text-red-600" />}
                    </div>
                </div>
                <div className="flex gap-1">
                    <Button
                        variant="ghost"
                        size="sm"
                        className="h-7 w-7 p-0"
                        onClick={(e) => {
                            e.stopPropagation();
                            onDuplicate();
                        }}
                    >
                        <Copy className="w-3 h-3" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="sm"
                        className="h-7 w-7 p-0 text-red-600 hover:text-red-700"
                        onClick={(e) => {
                            e.stopPropagation();
                            onDelete();
                        }}
                    >
                        <Trash2 className="w-3 h-3" />
                    </Button>
                </div>
            </div>
        </div>
    );
}
