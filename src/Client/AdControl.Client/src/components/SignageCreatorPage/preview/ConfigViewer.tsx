// src/components/signage/preview/ConfigViewer.tsx
import CodeMirror from "@uiw/react-codemirror";
import { dracula } from "@uiw/codemirror-theme-dracula";
import { json } from "@codemirror/lang-json";
import { type SignageConfig, type ContentItem } from "../types";

interface ConfigViewerProps {
    config: SignageConfig;
    items: ContentItem[];
}

export function ConfigViewer({ config, items }: ConfigViewerProps) {
    const fullConfig = {
        signageName: config.name,
        aspectRatio: config.aspectRatio,
        customResolution: config.aspectRatio === "custom" ? {
            width: config.customWidth,
            height: config.customHeight,
        } : null,
        transition: config.transition,
        defaultDuration: config.defaultDuration,
        loopMode: config.loopMode,
        schedule: config.loopMode === "scheduled" ? {
            recurring: config.recurring,
            startTime: config.scheduleStart,
            endTime: config.scheduleEnd,
        } : null,
        contentItems: items.map(item => ({
            id: item.id,
            type: item.type,
            name: item.name,
            duration: item.duration,
            status: item.status,
            config: item.config,
        })),
        totalDuration: items.reduce((sum, i) => sum + i.duration, 0),
        itemsCount: items.length,
    };

    return (
        <div className="flex flex-col">
            <div className="px-6 py-3 bg-gray-900 text-white text-sm font-medium border-b border-gray-700">
                Signage Configuration (JSON)
            </div>
            <CodeMirror
                value={JSON.stringify(fullConfig, null, 2)}
                theme={dracula}
                extensions={[json()]}
                editable={false}
                basicSetup={{
                    lineNumbers: true,
                    foldGutter: true,
                    highlightActiveLine: false,
                }}
            />
        </div>
    );
}
