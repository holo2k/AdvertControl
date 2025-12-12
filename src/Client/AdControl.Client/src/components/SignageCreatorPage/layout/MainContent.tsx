import { PreviewArea } from "../preview/PreviewArea";
import { Timeline } from "../preview/Timeline";
import type { ContentItem, SignageConfig } from "../types";

interface Props {
    config: SignageConfig;
    contentItems: ContentItem[];
    currentIndex: number;
    setCurrentIndex: React.Dispatch<React.SetStateAction<number>>;
    isPlaying: boolean;
    setIsPlaying: React.Dispatch<React.SetStateAction<boolean>>;
    onFullscreen: () => void;
}

export function MainContent({
                                config,
                                contentItems,
                                currentIndex,
                                setCurrentIndex,
                                isPlaying,
                                setIsPlaying,
                                onFullscreen,
                            }: Props) {

    return (
        <div className="flex-1 flex flex-col gap-6 overflow-hidden py-6 pr-6">
            <PreviewArea
                config={config}
                items={contentItems}
                currentIndex={currentIndex}
                setCurrentIndex={setCurrentIndex}
                isPlaying={isPlaying}
                setIsPlaying={setIsPlaying}
                onFullScreen={onFullscreen}
                contentItems={contentItems}
            />

            {contentItems.length > 0 && (
                <Timeline
                    items={contentItems}
                    currentIndex={currentIndex}
                    setCurrentIndex={setCurrentIndex}
                    setIsPlaying={setIsPlaying}
                />
            )}
        </div>
    );
}
