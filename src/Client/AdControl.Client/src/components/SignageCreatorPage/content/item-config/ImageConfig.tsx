import {Label} from "../../../ui/label.tsx";
import {Select, SelectContent, SelectItem, SelectTrigger, SelectValue} from "../../../ui/select.tsx";
import {Input} from "../../../ui/input.tsx";

export function ImageConfig({
                                config,
                                onChange,
                            }: {
    config: any;
    onChange: (update: any) => void;
}) {
    return (
        <div className="space-y-4">
            <h3 className="text-sm">Параметры изображения</h3>

            <div className="space-y-2">
                <Label>Режим отображения</Label>
                <Select
                    value={config.fit || "cover"}
                    onValueChange={(value) => onChange({ fit: value })}
                >
                    <SelectTrigger>
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="cover">Обложка (Заполнение)</SelectItem>
                        <SelectItem value="contain">Вместить (По размеру)</SelectItem>
                        <SelectItem value="fill">Растянуть</SelectItem>
                    </SelectContent>
                </Select>
            </div>

            <div className="space-y-2">
                <Label>Анимация</Label>
                <Select
                    value={config.animation || "none"}
                    onValueChange={(value) => onChange({ animation: value })}
                >
                    <SelectTrigger>
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="none">Без анимации</SelectItem>
                        <SelectItem value="zoom">Приближение</SelectItem>
                        <SelectItem value="pan">Панорамирование</SelectItem>
                    </SelectContent>
                </Select>
            </div>

            <div className="space-y-2">
                <Label>Цвет фона</Label>
                <Input
                    type="color"
                    value={config.backgroundColor || "#000000"}
                    onChange={(e) => onChange({ backgroundColor: e.target.value })}
                />
            </div>
        </div>
    );
}
