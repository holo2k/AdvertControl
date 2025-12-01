import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "../../ui/collapsible.tsx";
import { ChevronUp, ChevronDown, FileText, Upload } from "lucide-react";
import { useState } from "react";
import { toast } from "../../ui/sonner.tsx";
import { AddContentButton } from "../content/AddContentButton.tsx";
import { ContentItemCard } from "../content/ContentItemCard.tsx";
import type { ContentItem, ContentType } from "../types.ts";

interface Props {
    contentItems: ContentItem[];
    setContentItems: React.Dispatch<React.SetStateAction<ContentItem[]>>;
    selectedItem: string | null;
    setSelectedItem: React.Dispatch<React.SetStateAction<string | null>>;
    defaultDuration: number;
}

export function ContentList({ contentItems, setContentItems, selectedItem, setSelectedItem, defaultDuration }: Props) {
    const [open, setOpen] = useState(true);
    const [draggedId, setDraggedId] = useState<string | null>(null);

    const handleAdd = (type: ContentType, extraConfig?: any) => {
        const defaultConfigs = {
            image: {
                url: "",
                fit: "cover",
                animation: "none",
                backgroundColor: "#000000",
                ...extraConfig,
            },
            text: {
                content: "Enter your text here",
                fontSize: 48,
                textColor: "#FFFFFF",
                backgroundColor: "#2563EB",
                alignment: "center",
                ...extraConfig,
            },
            video: {
                volume: 50,
                loop: false,
                muted: false,
                ...extraConfig,
            },
            table: {
                columns: [],
                headerColor: "#2563EB",
                alternateRows: true,
                ...extraConfig,
            },
        };

        const newItem: ContentItem = {
            id: `item-${Date.now()}`,
            type,
            name: `${type} ${contentItems.length + 1}`,
            duration: defaultDuration,
            status: "ready",
            config: {
                ...defaultConfigs[type],
            },
        };

        setContentItems(prev => [...prev, newItem]);
        setSelectedItem(newItem.id);
    };

    const handleDuplicate = (id: string) => {
        const item = contentItems.find(i => i.id === id);
        if (!item) return;
        const copy: ContentItem = {
            ...item,
            id: `item-${Date.now()}`,
            name: `${item.name} (Copy)`,
        };
        setContentItems(prev => [...prev, copy]);
        toast.success("Duplicated");
    };

    const handleDelete = (id: string) => {
        setContentItems(prev => prev.filter(i => i.id !== id));
        if (selectedItem === id) setSelectedItem(null);
        toast.success("Removed");
    };

    const handleDragStart = (id: string) => setDraggedId(id);

    const handleDragOver = (e: React.DragEvent, targetId: string) => {
        e.preventDefault();
        if (!draggedId || draggedId === targetId) return;

        setContentItems(prev => {
            const dragIdx = prev.findIndex(i => i.id === draggedId);
            const targetIdx = prev.findIndex(i => i.id === targetId);
            const newItems = [...prev];
            const [removed] = newItems.splice(dragIdx, 1);
            newItems.splice(targetIdx, 0, removed);
            return newItems;
        });
    };

    const handleDragEnd = () => setDraggedId(null);

    return (
        <Collapsible open={open} onOpenChange={setOpen}>
            <CollapsibleTrigger className="flex items-center justify-between w-full">
                <h2 className="flex items-center gap-2"><FileText className="w-5 h-5" /> Кол-во объектов ({contentItems.length})</h2>
                {open ? <ChevronUp /> : <ChevronDown />}
            </CollapsibleTrigger>

            <CollapsibleContent className="space-y-4 mt-4">
                <AddContentButton onAdd={handleAdd} />

                {contentItems.length === 0 ? (
                    <div className="text-center p-8 px-2 border-2 border-dashed border-gray-300 rounded-lg">
                        <Upload className="w-12 h-12 mx-auto text-gray-400 mb-3" />
                        <p className="text-gray-600 text-sm mb-1">Контент еще не добавлен</p>
                        <p className="text-gray-400 text-xs">Нажмите "Добавить контент", чтобы начать</p>
                    </div>
                ) : (
                    <div className="space-y-2">
                        {contentItems.map(item => (
                            <ContentItemCard
                                key={item.id}
                                item={item}
                                isSelected={selectedItem === item.id}
                                isDragging={draggedId === item.id}
                                onSelect={() => setSelectedItem(item.id)}
                                onDuplicate={() => handleDuplicate(item.id)}
                                onDelete={() => handleDelete(item.id)}
                                onDragStart={() => handleDragStart(item.id)}
                                onDragOver={(e) => handleDragOver(e, item.id)}
                                onDragEnd={handleDragEnd}
                            />
                        ))}
                    </div>
                )}
            </CollapsibleContent>
        </Collapsible>
    );
}
