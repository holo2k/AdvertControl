import {Label} from "../../../ui/label.tsx";
import {Input} from "../../../ui/input.tsx";
import {Slider} from "../../../ui/slider.tsx";
import {Select, SelectContent, SelectItem, SelectTrigger, SelectValue} from "../../../ui/select.tsx";

export function TextConfig({
                               config,
                               onChange,
                           }: {
    config: any;
    onChange: (update: any) => void;
}) {
    return (
        <div className="space-y-4">
            <h3 className="text-sm">Настройки текста</h3>

            <div className="space-y-2">
                <Label>Текст</Label>
                <Input
                    value={config.content || ""}
                    onChange={(e) => onChange({ content: e.target.value })}
                    placeholder="Введите текст"
                />
            </div>

            <div className="space-y-2">
                <Label>Размер шрифта: {config.fontSize || 48}px</Label>
                <Slider
                    min={24}
                    max={120}
                    step={4}
                    value={[config.fontSize || 48]}
                    onValueChange={(value) => onChange({ fontSize: value[0] })}
                />
            </div>

            <div className="space-y-2">
                <Label>Цвет текста</Label>
                <Input
                    type="color"
                    value={config.textColor || "#FFFFFF"}
                    onChange={(e) => onChange({ textColor: e.target.value })}
                />
            </div>

            <div className="space-y-2">
                <Label>Цвет фона</Label>
                <Input
                    type="color"
                    value={config.backgroundColor || "#2563EB"}
                    onChange={(e) => onChange({ backgroundColor: e.target.value })}
                />
            </div>

            <div className="space-y-2">
                <Label>Выравнивание текста</Label>
                <Select
                    value={config.alignment || "center"}
                    onValueChange={(value) => onChange({ alignment: value })}
                >
                    <SelectTrigger>
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="left">По левому краю</SelectItem>
                        <SelectItem value="center">По центру</SelectItem>
                        <SelectItem value="right">По правому краю</SelectItem>
                    </SelectContent>
                </Select>
            </div>
        </div>
    );
}
