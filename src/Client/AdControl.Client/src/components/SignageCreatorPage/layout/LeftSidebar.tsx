import { ScrollArea } from "../../ui/scroll-area";
import { GeneralSettings } from "../config/GeneralSettings";
import { ContentList } from "../config/ContentList";
import { ActionButtons } from "../config/ActionButtons";
import type { SignageConfig, ContentItem } from "../types";

interface Props {
    config: SignageConfig;
    setConfig: React.Dispatch<React.SetStateAction<SignageConfig>>;
    contentItems: ContentItem[];
    setContentItems: React.Dispatch<React.SetStateAction<ContentItem[]>>;
    selectedItem: string | null;
    setSelectedItem: React.Dispatch<React.SetStateAction<string | null>>;
    onSave: () => void;
    onPublish: () => void;
}

export function LeftSidebar({
                                config,
                                setConfig,
                                contentItems,
                                setContentItems,
                                selectedItem,
                                setSelectedItem,
                                onSave,
                                onPublish,
                            }: Props) {
    return (
        <div className="w-75 border border-gray-200 bg-white overflow-hidden flex rounded-lg flex-col ">
            <ScrollArea className="flex-1 p-6">
                <div className="space-y-6" >
                    <GeneralSettings config={config} setConfig={setConfig} />
                    <ContentList
                        contentItems={contentItems}
                        setContentItems={setContentItems}
                        selectedItem={selectedItem}
                        setSelectedItem={setSelectedItem}
                        defaultDuration={config.defaultDuration}
                    />
                </div>
            </ScrollArea>
            <ActionButtons onSave={onSave} onPublish={onPublish} />
        </div>
    );
}
