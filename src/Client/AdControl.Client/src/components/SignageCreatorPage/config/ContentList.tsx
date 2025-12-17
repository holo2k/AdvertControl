import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "../../ui/collapsible.tsx";
import {ChevronUp, ChevronDown, FileText, Loader2, Upload} from "lucide-react";
import { useState } from "react";
import { toast } from "../../ui/sonner.tsx";
import { AddContentButton } from "../content/AddContentButton.tsx";
import { ContentItemCard } from "../content/ContentItemCard.tsx";
import type { ContentItem, ContentType, SignageConfig } from "../types.ts";

interface Props {
    items: ContentItem[];
    setConfig: React.Dispatch<React.SetStateAction<SignageConfig>>;
    selectedItem: string | null;
    setSelectedItem: React.Dispatch<React.SetStateAction<string | null>>;
}

export function ContentList({ items, setConfig, selectedItem, setSelectedItem }: Props) {
    const [open, setOpen] = useState(true);
    const [draggedId, setDraggedId] = useState<string | null>(null);
    const [uploading, setUploading] = useState(false);

    const getNextOrder = () => {
        if (items.length === 0) return 0;
        const maxOrder = Math.max(...items.map(item => item.order || 0));
        return maxOrder + 1;
    };

    // Функция для обновления порядка всех элементов
    const updateAllOrders = (items: ContentItem[]): ContentItem[] => {
        return items
            .map((item, index) => ({
                ...item,
                order: index
            }));
    };

    const handleAdd = (type: ContentType, extraConfig?: any) => {
        const newOrder = getNextOrder();
        const newItem: ContentItem = {
            id: `item-${Date.now()}`,
            type,
            name: `${type} ${items.length + 1}`,
            durationSeconds: 10,
            status: "ready",
            order: newOrder,
            url: extraConfig?.url,
            ...extraConfig
        };

        setConfig(prev => ({
            ...prev,
            items: [...prev.items, newItem],
        }));

        setSelectedItem(newItem?.url || "");
        toast.success("Элемент добавлен");
    };

    const handleDuplicate = (url: string) => {
        const item = items.find(i => i.url === url);
        if (!item) return;

        const newOrder = getNextOrder();
        const copy: ContentItem = {
            ...item,
            url: `${item.url} (Copy)`,
            order: newOrder,
        };

        setConfig(prev => ({
            ...prev,
            items: [...prev.items, copy],
        }));

        toast.success("Элемент скопирован");
    };

    const handleDelete = (url: string) => {
        setConfig(prev => {
            const filteredItems = prev.items.filter(i => i.url !== url);
            const reorderedItems = updateAllOrders(filteredItems);
            return {
                ...prev,
                items: reorderedItems
            };
        });

        if (selectedItem === url) setSelectedItem(null);
        toast.success("Элемент удален");
    };

    const handleReorder = (dragUrl: string, targetUrl: string) => {
        if (dragUrl === targetUrl) return;

        setConfig(prev => {
            const newItems = [...prev.items];
            const dragIdx = newItems.findIndex(i => i.url === dragUrl);
            const targetIdx = newItems.findIndex(i => i.url === targetUrl);

            if (dragIdx === -1 || targetIdx === -1) return prev;

            // Удаляем перетаскиваемый элемент
            const [draggedItem] = newItems.splice(dragIdx, 1);

            // Вставляем его на новую позицию
            newItems.splice(targetIdx, 0, draggedItem);

            // Обновляем порядок всех элементов
            const reorderedItems = updateAllOrders(newItems);

            return { ...prev, items: reorderedItems };
        });
    };

    const handleDragStart = (id: string) => setDraggedId(id);

    const handleDragOver = (e: React.DragEvent, targetId: string) => {
        e.preventDefault();
        if (!draggedId || draggedId === targetId) return;
        handleReorder(draggedId, targetId);
    };

    const handleDragEnd = () => setDraggedId(null);

    // Сортируем элементы по порядку для отображения
    const sortedItems = [...items].sort((a, b) => {
        const orderA = a.order || 0;
        const orderB = b.order || 0;
        return orderA - orderB;
    });

    return (
        <Collapsible open={open} onOpenChange={setOpen}>
            <CollapsibleTrigger className="flex items-center justify-between w-full">
                <h2 className="flex items-center gap-2">
                    <FileText className="w-5 h-5" /> Кол-во объектов ({items.length})
                </h2>
                {open ? <ChevronUp /> : <ChevronDown />}
            </CollapsibleTrigger>

            <CollapsibleContent className="space-y-4 mt-4">
                <div className="flex flex-col sm:flex-row gap-2">
                    <AddContentButton onAdd={handleAdd} />
                </div>

                {uploading && (
                    <div className="flex flex-col items-center justify-center p-4 bg-blue-50 rounded-lg">
                        <Loader2 className="w-8 h-8 animate-spin text-blue-600 mb-2" />
                        <p className="text-sm text-gray-600">Загрузка на сервер...</p>
                    </div>
                )}

                {sortedItems.length === 0 ? (
                    <div className="text-center py-12 text-gray-500 border-2 border-dashed border-gray-200 rounded-lg">
                        <p className="text">Добавьте контент для отображения на экранах</p>
                    </div>
                ) : (
                    <div className="space-y-2 overflow-y-auto" style={{height: "25vh"}}>

                        {sortedItems.map((item, index) => (
                            <ContentItemCard
                                key={item.url || ""}
                                item={item}
                                index={index}
                                totalItems={sortedItems.length}
                                isSelected={selectedItem === item.url}
                                isDragging={draggedId === item.url}
                                onSelect={() => setSelectedItem(item?.url || "")}
                                onDuplicate={() => handleDuplicate(item?.url || "")}
                                onDelete={() => handleDelete(item?.url || "")}
                                onDragStart={() => handleDragStart(item?.url || "")}
                                onDragOver={(e) => handleDragOver(e, item?.url || "")}
                                onDragEnd={handleDragEnd}
                            />
                        ))}
                    </div>
                )}
            </CollapsibleContent>
        </Collapsible>
    );
}
