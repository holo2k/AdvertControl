export type ContentType = "IMAGE" | "VIDEO" | "TEXT" | "TABLE" ;

export interface ContentItem {
    type: ContentType;
    durationSeconds: number;
    order: number;
    url?: string;
    inlineData?: string;
    checksum?: string;
    size?: number;
}

export interface SignageConfig {
    id?: string;
    name: string;
    screensCount: number;
    items: ContentItem[];
}
