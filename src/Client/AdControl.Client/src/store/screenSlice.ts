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
    status?: "connected" | "pending" | "error"; // на случай будущих статусов
}

interface ScreensState {
    items: Screen[];
    total: number;
    status: "idle" | "loading" | "succeeded" | "failed";
    error: string | null;
    limit: number;
    offset: number;
}

const initialState: ScreensState = {
    items: [],
    total: 0,
    status: "idle",
    error: null,
    limit: 20,
    offset: 0,
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
    },
    extraReducers: (builder) => {
        builder
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
            });
    },
});

export const { addScreen, setPagination, clearScreens } = screensSlice.actions;
export default screensSlice.reducer;
