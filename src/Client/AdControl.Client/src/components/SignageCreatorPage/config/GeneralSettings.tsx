// src/components/signage/config/GeneralSettings.tsx
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "../../ui/collapsible";
import { ChevronUp, ChevronDown, Settings } from "lucide-react";
import { Input } from "../../ui/input";
import { Label } from "../../ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "../../ui/select";
import { Slider } from "../../ui/slider";
import { Separator } from "../../ui/separator";
import type { SignageConfig } from "../types";
import { useState } from "react";

interface Props {
    config: SignageConfig;
    setConfig: React.Dispatch<React.SetStateAction<SignageConfig>>;
}

export function GeneralSettings({ config, setConfig }: Props) {
    const [open, setOpen] = useState(true);

    return (
        <Collapsible open={open} onOpenChange={setOpen}>
            <CollapsibleTrigger className="flex items-center justify-between w-full">
                <h2 className="flex items-center gap-2"><Settings className="w-5 h-5" />Описание экрана</h2>
                {open ? <ChevronUp /> : <ChevronDown />}
            </CollapsibleTrigger>

            <CollapsibleContent className="space-y-4 mt-4">
                <div className="space-y-2">
                    <Label htmlFor="display-name">Название</Label>
                    <Input
                        id="display-name"
                        value={config.name}
                        onChange={(e) => setConfig((c) => ({ ...c, name: e.target.value }))}
                    />
                </div>

                <div className="space-y-2">
                    <Label>Разрешение</Label>
                    <Select value={config.aspectRatio} onValueChange={(v: any) => setConfig(c => ({ ...c, aspectRatio: v }))}>
                        <SelectTrigger><SelectValue /></SelectTrigger>
                        <SelectContent>
                            <SelectItem value="16:9">1920x1080 </SelectItem>
                            <SelectItem value="custom">Свое</SelectItem>
                        </SelectContent>
                    </Select>
                </div>

                {config.aspectRatio === "custom" && (
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <Label>Ширина (пикс.)</Label>
                            <Input type="number" value={config.customWidth ?? 1920}
                                   onChange={e => setConfig(c => ({ ...c, customWidth: +e.target.value }))} />
                        </div>
                        <div className="space-y-2">
                            <Label>Высота (пикс.)</Label>
                            <Input type="number" value={config.customHeight ?? 1080}
                                   onChange={e => setConfig(c => ({ ...c, customHeight: +e.target.value }))} />
                        </div>
                    </div>
                )}

                <div className="space-y-2">
                    <Label>Продолжительность (по умолч): {config.defaultDuration}s</Label>
                    <Slider
                        min={1} max={60} step={1}
                        value={[config.defaultDuration]}
                        onValueChange={([v]) => setConfig(c => ({ ...c, defaultDuration: v }))}
                    />
                </div>
            </CollapsibleContent>
            <Separator className="mt-6" />
        </Collapsible>
    );
}
