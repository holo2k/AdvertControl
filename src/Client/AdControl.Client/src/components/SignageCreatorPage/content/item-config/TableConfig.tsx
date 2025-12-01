import {Label} from "../../../ui/label.tsx";
import {Input} from "../../../ui/input.tsx";

export function TableConfig({
                         config,
                         onChange,
                     }: {
    config: any;
    onChange: (update: any) => void;
}) {
    return (
        <div className="space-y-4">
            <h3 className="text-sm">Параметры таблицы</h3>

            <div className="space-y-2">
                <Label>Цвет заголовка</Label>
                <Input
                    type="color"
                    value={config.headerColor || "#2563EB"}
                    onChange={(e) => onChange({ headerColor: e.target.value })}
                />
            </div>

            <div className="space-y-2">
                <Label>Строк</Label>
                <Input
                    type="number"
                    value={config.pagination || 10}
                    onChange={(e) =>
                        onChange({ pagination: parseInt(e.target.value) })
                    }
                />
            </div>
        </div>
    );
}
