import {Label} from "../../../ui/label.tsx";
import type {ContentItem} from "../../types.ts";

interface VideoConfigProps {
    item: ContentItem;
}

export function VideoConfig({ item }: VideoConfigProps) {
    return (
        <div className="space-y-4">
            <h3 className="text-sm">Параметры видео</h3>

            <div className="space-y-2">
                <Label>Размер</Label>
                {item.size}
            </div>
        </div>
    );
}
