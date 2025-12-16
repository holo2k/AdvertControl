import {Label} from "../../../ui/label.tsx";
import type {ContentItem} from "../../types.ts";

interface TableConfigProps {
    item: ContentItem;
}

export function TableConfig({item}: TableConfigProps) {
    return (
        <div className="space-y-4">
            <h3 className="text-sm">Параметры таблицы</h3>
            <div className="space-y-2">
                <Label>Размер</Label>
                {item.size}
            </div>
        </div>
    );
}
