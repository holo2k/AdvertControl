import { Card, CardHeader, CardTitle, CardContent } from "../../ui/card";
import type { ContentItem } from "../types";
import {truncateString} from "../../../utils.ts";

interface TimelineProps {
    items: ContentItem[];
    currentIndex: number;
    setCurrentIndex: React.Dispatch<React.SetStateAction<number>>;
    setIsPlaying: React.Dispatch<React.SetStateAction<boolean>>;
}

export function Timeline({
                             items,
                             currentIndex,
                             setCurrentIndex,
                             setIsPlaying,
                         }: TimelineProps) {
    const totalDuration = items.reduce((sum, i) => sum + i.durationSeconds, 0);

    return (
        <Card>
            <CardHeader>
                <CardTitle className="text-base">Временная шкала контента</CardTitle>
            </CardHeader>
            <CardContent>
                <div className="flex gap-2 overflow-x-auto pb-2">
                    {items.map((item, index) => {
                        const widthPercent = (item.durationSeconds / totalDuration) * 100;

                        return (
                            <div
                                key={item.url}
                                className={`
                  flex-shrink-0 h-16 rounded-lg border-2 flex items-center justify-center
                  text-xs px-3 cursor-pointer transition-all
                  ${currentIndex === index
                                    ? "border-blue-500 bg-blue-50"
                                    : "border-gray-300 bg-white hover:border-gray-400"
                                }
                `}
                                style={{ minWidth: `${Math.max(widthPercent, 10)}%` }}
                                onClick={() => {
                                    setCurrentIndex(index);
                                    setIsPlaying(false);
                                }}
                            >
                                <div className="text-center">
                                    <div className="truncate max-w-[100px] font-medium">{truncateString(item.url, 20)}</div>
                                    <div className="text-gray-500 mt-1">{item.durationSeconds}с</div>
                                </div>
                            </div>
                        );
                    })}
                </div>
            </CardContent>
        </Card>
    );
}
