export type AspectRatio = "16:9" | "9:16" | "4:3" | "custom";
export type Transition = "fade" | "slide" | "zoom" | "dissolve" | "none";
export type LoopMode = "continuous" | "scheduled";
export type ContentType = "table" | "image" | "video" | "text";

export interface SignageConfig {
    name: string;
    aspectRatio: AspectRatio;
    customWidth?: number;
    customHeight?: number;
    transition: Transition;
    defaultDuration: number;
    loopMode: LoopMode;
    recurring?: "daily" | "weekdays" | "weekends" | "weekly" | "specific";
    scheduleStart?: string;
    scheduleEnd?: string;
}

export interface ContentItem {
    id: string;
    type: ContentType;
    name: string;
    duration: number;
    status: "ready" | "processing" | "error";
    config: {
        // Для image
        url?: string;                    // <-- новое поле
        fit?: "cover" | "contain" | "fill";
        animation?: "none" | "kenburns" | "zoom";
        backgroundColor?: string;

        // Для text
        content?: string;
        fontSize?: number;
        textColor?: string;
        alignment?: "left" | "center" | "right";

        [key: string]: any;
    };
}

export interface ScreenConfig {
    id?: string;
    name: string;
    aspectRatio: string;
    transition: string;
    defaultDuration: number;
    loopMode: string;
    backgroundColor?: string;
    textColor?: string;
    contentItems: ContentItem[];
}

export interface ServerScreenResponse {
    id: string;
    name: string;
    location: string;
    resolution: string;
    lastHeartbeatAt: number;
    pairedAt: number;
    createdAt: number;
    updatedAt: number;
    status: "connected" | "pending" | "error";
    config?: ScreenConfig;
}
