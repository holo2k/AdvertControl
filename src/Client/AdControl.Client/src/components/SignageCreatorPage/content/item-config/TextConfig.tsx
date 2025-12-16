import {Label} from "../../../ui/label.tsx";
import type {ContentItem} from "../../types.ts";

interface TextConfigProps {
    item: ContentItem;
}

export function TextConfig({ item }: TextConfigProps) {
    return (
        <div className="space-y-4">
            <h3 className="text-sm">Параметры текстового фона</h3>
            <div className="space-y-2">
                <Label>Размер</Label>
                {item.size}
            </div>
        </div>
    );
}
