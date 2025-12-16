// src/components/signage/config/GeneralSettings.tsx
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "../../ui/collapsible";
import { ChevronUp, ChevronDown, Settings } from "lucide-react";
import { Input } from "../../ui/input";
import { Label } from "../../ui/label";
import { Separator } from "../../ui/separator";
import type { SignageConfig } from "../types";
import { useState } from "react";
import {useSelector} from "react-redux";
import type {RootState} from "../../../store/store.ts";

interface Props {
    config: SignageConfig;
    setConfig: React.Dispatch<React.SetStateAction<SignageConfig>>;
}

export function GeneralSettings({ config, setConfig }: Props) {
    const [open, setOpen] = useState(true);

    const { currentScreen } = useSelector(
        (state: RootState) => state.screens
    );

    return (
        <Collapsible open={open} onOpenChange={setOpen}>
            <CollapsibleTrigger className="flex items-center justify-between w-full">
                <h2 className="flex items-center gap-2"><Settings className="w-5 h-5" />Описание экрана</h2>
                {open ? <ChevronUp /> : <ChevronDown />}
            </CollapsibleTrigger>

            <CollapsibleContent className="space-y-4 mt-4">
                <div className="space-y-2">
                    <Label htmlFor="display-name">Экран</Label>
                    <Input
                        disabled
                        id="display-name"
                        value={currentScreen?.screen.name || ""}
                        onChange={(e) => setConfig((c) => ({ ...c, name: e.target.value }))}
                    />
                </div>

                <div className="space-y-2">
                    <Label>Разрешение</Label>
                    <Input
                        disabled
                        id="display-name"
                        value={currentScreen?.screen.resolution || ""}
                        onChange={(e) => setConfig((c) => ({ ...c, name: e.target.value }))}
                    />
                </div>

            </CollapsibleContent>

            <Separator className="mt-6" />
            <div className="space-y-2 mt-4">
                <Label>Название конфига </Label>
                <Input
                    id="config-name"
                    value={config.name}
                    onChange={(e) => setConfig((c) => ({ ...c, name: e.target.value }))}
                />
            </div>
            <div className="space-y-2 mt-4">
                <Label>Кол-во экранов </Label>
                <Input
                    id="config-screensCount"
                    type="number"
                    min={1}
                    step={2}
                    value={config.screensCount}
                    onChange={(e) => {
                        const value = Number(e.target.value);

                        if (Number.isNaN(value)) return;

                        setConfig((c) => ({
                            ...c,
                            screensCount: value % 2 === 0 ? value : value - 1,
                        }));
                    }}
                />
            </div>
        </Collapsible>
    );
}
