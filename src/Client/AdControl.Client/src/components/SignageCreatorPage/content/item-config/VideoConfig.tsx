import {Label} from "../../../ui/label.tsx";
import {Slider} from "../../../ui/slider.tsx";
import {Switch} from "../../../ui/switch.tsx";
import {Select, SelectContent, SelectItem, SelectTrigger, SelectValue} from "../../../ui/select.tsx";

export function VideoConfig({
                                config,
                                onChange,
                            }: {
    config: any;
    onChange: (update: any) => void;
}) {
    return (
        <div className="space-y-4">
            <h3 className="text-sm">Настройки видео</h3>

            <div className="space-y-2">
                <Label>Громкость: {config.volume || 50}%</Label>
                <Slider
                    min={0}
                    max={100}
                    step={5}
                    value={[config.volume || 50]}
                    onValueChange={(value) => onChange({ volume: value[0] })}
                />
            </div>

            <div className="flex items-center justify-between">
                <Label htmlFor="muted">Без звука</Label>
                <Switch
                    id="muted"
                    checked={config.muted ?? false}
                    onCheckedChange={(checked) => onChange({ muted: checked })}
                />
            </div>

            <div className="flex items-center justify-between">
                <Label htmlFor="loop-video">Зациклить видео</Label>
                <Switch
                    id="loop-video"
                    checked={config.loop ?? false}
                    onCheckedChange={(checked) => onChange({ loop: checked })}
                />
            </div>

            <div className="space-y-2">
                <Label>Качество воспроизведения</Label>
                <Select
                    value={config.quality || "auto"}
                    onValueChange={(value) => onChange({ quality: value })}
                >
                    <SelectTrigger>
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="auto">Авто</SelectItem>
                        <SelectItem value="high">Высокое (1080p)</SelectItem>
                        <SelectItem value="medium">Среднее (720p)</SelectItem>
                        <SelectItem value="low">Низкое (480p)</SelectItem>
                    </SelectContent>
                </Select>
            </div>
        </div>
    );
}
