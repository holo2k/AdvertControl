import { ScrollArea } from "../../ui/scroll-area";
import { Input } from "../../ui/input";
import { Label } from "../../ui/label";
import { Slider } from "../../ui/slider";
import { Separator } from "../../ui/separator";
import { TableConfig } from "../content/item-config/TableConfig";
import { ImageConfig } from "../content/item-config/ImageConfig";
import { VideoConfig } from "../content/item-config/VideoConfig";
import { TextConfig } from "../content/item-config/TextConfig";
import type { ContentItem } from "../types";

interface Props {
    item: ContentItem;
    onNameChange: (name: string) => void;
    onDurationChange: (duration: number) => void;
    onConfigChange: (update: Partial<ContentItem["config"]>) => void;
}

export function RightSidebar({ item, onNameChange, onDurationChange, onConfigChange }: Props) {
    return (
        <div className="w-80 border-l border-gray-200 bg-white">
            <ScrollArea className="h-full px-6 py-6">
                <div className="space-y-6">
                    <h2 className="text-lg font-semibold">Конфигурация объекта</h2>

                    <div className="space-y-4">
                        <div className="space-y-2">
                            <Label>Имя объекта</Label>
                            <Input value={item.name} onChange={e => onNameChange(e.target.value)} />
                        </div>

                        <div className="space-y-2">
                            <Label>Продолжительность: {item.duration}s</Label>
                            <Slider
                                min={1}
                                max={60}
                                step={1}
                                value={[item.duration]}
                                onValueChange={([v]) => onDurationChange(v)}
                            />
                        </div>

                        <Separator />

                        {item.type === "table" && <TableConfig config={item.config} onChange={onConfigChange} />}
                        {item.type === "image" && <ImageConfig config={item.config} onChange={onConfigChange} />}
                        {item.type === "video" && <VideoConfig config={item.config} onChange={onConfigChange} />}
                        {item.type === "text" && <TextConfig config={item.config} onChange={onConfigChange} />}
                    </div>
                </div>
            </ScrollArea>
        </div>
    );
}
