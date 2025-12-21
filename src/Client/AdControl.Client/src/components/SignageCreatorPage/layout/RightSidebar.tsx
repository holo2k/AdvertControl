import { ScrollArea } from "../../ui/scroll-area";
import { Label } from "../../ui/label";
import { Slider } from "../../ui/slider";
import { Separator } from "../../ui/separator";
import { TableConfig } from "../content/item-config/TableConfig";
import { ImageConfig } from "../content/item-config/ImageConfig";
import { VideoConfig } from "../content/item-config/VideoConfig";
import { TextConfig } from "../content/item-config/TextConfig";
import type { ContentItem } from "../types";
import {removeId} from "../../../utils.ts";

interface Props {
    item: ContentItem;
    onDurationChange: (duration: number) => void;
}

export function RightSidebar({ item, onDurationChange}: Props) {
    return (
        <div className="h-full w-75 border border-gray-200 bg-white overflow-hidden flex rounded-lg flex-col">
            <ScrollArea className="flex-1 p-6">
                <div className="space-y-6">
                    <h2 className="text-lg font-semibold">Конфигурация объекта</h2>

                    <div className="space-y-4">
                        <div className="space-y-2">
                            <Label>Имя объекта</Label>
                            <div className="w-64 truncate overflow-hidden text-ellipsis">
                                {removeId(item.url)}
                            </div>
                        </div>

                        <div className="space-y-2">
                            <Label>Продолжительность: {item.durationSeconds}с</Label>
                            {item.type !== "VIDEO" && <Slider
                                min={1}
                                max={60}
                                step={1}
                                value={[item.durationSeconds]}
                                onValueChange={([v]) => onDurationChange(v)}
                            />}
                        </div>

                        <Separator />

                        {item.type === "TABLE" && <TableConfig item={item}/>}
                        {item.type === "IMAGE" && <ImageConfig item = {item}/>}
                        {item.type === "VIDEO" && <VideoConfig item={item} />}
                        {item.type === "TEXT" && <TextConfig item={item}/>}
                    </div>
                </div>
            </ScrollArea>
        </div>
    );
}
