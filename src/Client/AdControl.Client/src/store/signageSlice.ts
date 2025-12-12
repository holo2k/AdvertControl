// src/store/signageSlice.ts
import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { SignageState, ContentItem, SignageConfig } from "./types";

const initialState: SignageState = {
    config: {
        name: "Untitled Signage",
        aspectRatio: "16:9",
        transition: "fade",
        defaultDuration: 10,
        loopMode: "continuous",
    },
    items: [],
    selectedItemId: null,
    currentPreviewIndex: 0,
    isPlaying: false,
    showCode: false,
};

export const signageSlice = createSlice({
    name: "signage",
    initialState,
    reducers: {
        setConfig: (state, action: PayloadAction<Partial<SignageConfig>>) => {
            state.config = { ...state.config, ...action.payload };
        },

        addItem: (state, action: PayloadAction<Omit<ContentItem, "id">>) => {
            const newItem: ContentItem = {
                ...action.payload,
                id: `item-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
            };
            state.items.push(newItem);
            state.selectedItemId = newItem.id;
        },

        updateItem: (
            state,
            action: PayloadAction<{ id: string; updates: Partial<ContentItem> }>
        ) => {
            const { id, updates } = action.payload;
            const item = state.items.find((i) => i.id === id);
            if (item) Object.assign(item, updates);
        },

        updateItemConfig: (
            state,
            action: PayloadAction<{ id: string; config: Record<string, any> }>
        ) => {
            const { id, config } = action.payload;
            const item = state.items.find((i) => i.id === id);
            if (item) item.config = { ...item.config, ...config };
        },

        deleteItem: (state, action: PayloadAction<string>) => {
            state.items = state.items.filter((i) => i.id !== action.payload);
            if (state.selectedItemId === action.payload) {
                state.selectedItemId = state.items[0]?.id || null;
            }
        },

        reorderItems: (state, action: PayloadAction<ContentItem[]>) => {
            state.items = action.payload;
        },

        setSelectedItem: (state, action: PayloadAction<string | null>) => {
            state.selectedItemId = action.payload;
        },

        setCurrentPreviewIndex: (state, action: PayloadAction<number>) => {
            state.currentPreviewIndex = action.payload;
        },

        setIsPlaying: (state, action: PayloadAction<boolean>) => {
            state.isPlaying = action.payload;
        },

        setShowCode: (state, action: PayloadAction<boolean>) => {
            state.showCode = action.payload;
        },

        resetSignage: () => initialState,
    },
});

export const {
    setConfig,
    addItem,
    updateItem,
    updateItemConfig,
    deleteItem,
    reorderItems,
    setSelectedItem,
    setCurrentPreviewIndex,
    setIsPlaying,
    setShowCode,
    resetSignage,
} = signageSlice.actions;

export default signageSlice.reducer;
