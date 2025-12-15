import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import type { RootState } from "./store";
import { apiClient } from "../api/apiClient";

export interface Screen {
    id: string;
    name: string;
    location: string;
    resolution: string;
    lastHeartbeatAt: number;
    pairedAt: number;
    createdAt: number;
    updatedAt: number;
    status?: "connected" | "pending" | "error";
}

interface CreateScreenData {
    name: string;
    resolution: string;
    location: string;
}

interface ScreensState {
    items: Screen[];
    total: number;
    status: "idle" | "loading" | "succeeded" | "failed";
    error: string | null;
    limit: number;
    offset: number;
    createStatus: "idle" | "loading" | "succeeded" | "failed";
    createError: string | null;
    currentScreen: Screen | null,
}

const initialState: ScreensState = {
    items: [],
    total: 0,
    status: "idle",
    error: null,
    limit: 20,
    offset: 0,
    createStatus: "idle",
    createError: null,
    currentScreen: null,
};

export const fetchScreens = createAsyncThunk(
    "screens/fetchScreens",
    async (
        { limit, offset }: { limit?: number; offset?: number } = {},
        { getState, rejectWithValue }
    ) => {
        const state = getState() as RootState;
        const token = state.auth.token;

        if (!token) return rejectWithValue("Нет токена авторизации");

        try {
            const response = await apiClient.get("/screen", {
                params: {
                    limit: limit ?? state.screens.limit,
                    offset: offset ?? state.screens.offset,
                },
            });

            return response.data;
        } catch (error: any) {
            return rejectWithValue(error.response?.data?.message || "Ошибка при загрузке экранов");
        }
    }
);

export const fetchScreen = createAsyncThunk(
    "screens/fetchScreen",
    async (
        id: string,
        { getState, rejectWithValue }
    ) => {
        const state = getState() as RootState;
        const token = state.auth.token;

        if (!token) return rejectWithValue("Нет токена авторизации");

        try {
            const response = await apiClient.get(`/screen/${id}`);
            return response.data;
        } catch (error: any) {
            return rejectWithValue(
                error.response?.data?.message ||
                error.response?.data?.error ||
                "Ошибка при загрузке экрана"
            );
        }
    }
);

export const createScreen = createAsyncThunk(
    "screens/createScreen",
    async (screenData: CreateScreenData, { getState, rejectWithValue }) => {
        const state = getState() as RootState;
        const token = state.auth.token;

        if (!token) return rejectWithValue("Нет токена авторизации");

        try {
            const response = await apiClient.post("/screen/pair/confirm", screenData);
            return response.data;
        } catch (error: any) {
            return rejectWithValue(
                error.response?.data?.message || "Ошибка при создании экрана"
            );
        }
    }
);

const screensSlice = createSlice({
    name: "screens",
    initialState,
    reducers: {
        addScreen: (state, action: PayloadAction<Screen>) => {
            state.items.push(action.payload);
            state.total += 1;
        },
        setPagination: (state, action: PayloadAction<{ limit: number; offset: number }>) => {
            state.limit = action.payload.limit;
            state.offset = action.payload.offset;
        },
        clearScreens: (state) => {
            state.items = [];
            state.total = 0;
            state.status = "idle";
            state.offset = 0;
        },
        resetCreateStatus: (state) => {
            state.createStatus = "idle";
            state.createError = null;
        },
        setCurrentScreen: (state, action: PayloadAction<Screen | null>) => {
            state.currentScreen = action.payload;
        },
    },
    extraReducers: (builder) => {
        builder
            // Fetch screens
            .addCase(fetchScreens.pending, (state) => {
                state.status = "loading";
                state.error = null;
            })
            .addCase(fetchScreens.fulfilled, (state, action) => {
                state.status = "succeeded";
                state.items = action.payload.items;
                state.total = action.payload.total;
            })
            .addCase(fetchScreens.rejected, (state, action) => {
                state.status = "failed";
                state.error = action.payload as string;
            })
            // Create screen
            .addCase(createScreen.pending, (state) => {
                state.createStatus = "loading";
                state.createError = null;
            })
            .addCase(createScreen.fulfilled, (state) => {
                state.createStatus = "succeeded";
                state.createError = null;
            })
            .addCase(createScreen.rejected, (state, action) => {
                state.createStatus = "failed";
                state.createError = action.payload as string;
            })
            .addCase(fetchScreen.pending, (state) => {
                state.status = "loading";
                state.error = null;
            })
            .addCase(fetchScreen.fulfilled, (state, action) => {
                state.status = "succeeded";
                // Обновляем или добавляем экран в список
                const index = state.items.findIndex(s => s.id === action.payload.id);
                if (index >= 0) {
                    state.items[index] = action.payload;
                } else {
                    state.items.push(action.payload);
                    state.total += 1;
                }
            })
            .addCase(fetchScreen.rejected, (state, action) => {
                state.status = "failed";
                state.error = action.payload as string;
            })
    },
});

export const { addScreen, setPagination, clearScreens, resetCreateStatus } = screensSlice.actions;
export default screensSlice.reducer;
