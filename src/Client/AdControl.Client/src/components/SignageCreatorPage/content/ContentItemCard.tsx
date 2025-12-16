import { GripVertical, Clock, Copy, Trash2 } from "lucide-react";
import { Button } from "../../ui/button";
import {
    FileText as FileTextIcon,
    Image as ImageIcon,
    Video as VideoIcon,
} from "lucide-react";
import type { ContentItem } from "../types";
import {truncateString} from "../../../utils.ts";

interface Props {
    item: ContentItem;
    index: number;
    totalItems: number;
    isSelected: boolean;
    isDragging: boolean;
    onSelect: () => void;
    onDuplicate: () => void;
    onDelete: () => void;
    onMoveUp: () => void;
    onMoveDown: () => void;
    onDragStart: () => void;
    onDragOver: (e: React.DragEvent) => void;
    onDragEnd: () => void;
}

export function ContentItemCard({
                                    item,
                                    index,
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
        TABLE: <FileTextIcon className={`${iconClass} text-blue-100`} />,
        IMAGE: <ImageIcon className={`${iconClass} text-green-100`} />,
        VIDEO: <VideoIcon className={`${iconClass} text-purple-100`} />,
        TEXT: <FileTextIcon className={`${iconClass} text-orange-100`} />,
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
                {/* Иконка перетаскивания */}
                <div className="flex flex-col items-center justify-center gap-1 mt-1 flex-shrink-0">
                    <GripVertical className="w-5 h-5 text-gray-400 cursor-move" />
                </div>

                <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between mb-1">
                        <div className="flex items-center gap-2">
                            {typeIcons[item.type]}
                            <span className="text-sm font-medium truncate break-all" style={{maxWidth: "200px",}}>
                                {truncateString(item.url, 20)  }
                            </span>
                        </div>
                    </div>

                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2 text-xs text-gray-500 mt-2">
                            <Clock className="w-3 h-3" />
                            <span>{item.durationSeconds} секунд</span>
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
                                title="Дублировать"
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
                                title="Удалить"
                            >
                                <Trash2 className="w-3 h-3" />
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
