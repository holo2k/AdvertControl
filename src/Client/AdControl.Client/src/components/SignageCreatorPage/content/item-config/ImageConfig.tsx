import {Label} from "../../../ui/label.tsx";
import type {ContentItem} from "../../types.ts";
import {bytesToMB} from "../../../../utils.ts";


interface ImageConfigProps {
    item: ContentItem;
}

export function ImageConfig({ item }: ImageConfigProps) {
    return (
        <div className="space-y-4">
            <h3 className="text-sm">Параметры изображения</h3>

            <div className="space-y-2">
                <Label>Размер</Label>
                {bytesToMB(item?.size || 0)}
            </div>

            <div className="space-y-2">
                <Label>Анимация</Label>

            </div>

            <div className="space-y-2">
                <Label>Цвет фона</Label>

            </div>
        </div>
    );
}
