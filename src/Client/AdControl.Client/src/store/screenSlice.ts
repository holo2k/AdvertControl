import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import type { RootState } from "./store";
import { apiClient } from "../api/apiClient";
import type {SignageConfig} from "../components/SignageCreatorPage/types.ts";

export interface ScreenDto {
    id: string;
    userId: string;
    name: string;
    location: string;
    resolution: string;
    lastHeartbeatAt: number;
    pairedAt: string;
    createdAt: string;
    updatedAt: string;
    status?: string;
}

export interface Screen {
    screen: ScreenDto;
    type: string[];
    config: SignageConfig;
}

interface CreateScreenData {
    name: string;
    resolution: string;
    location: string;
}

interface ScreensState {
    items: ScreenDto[];
    total: number;

    listStatus: "idle" | "loading" | "succeeded" | "failed";
    listError: string | null;

    currentScreen: Screen | null;
    currentStatus: "idle" | "loading" | "succeeded" | "failed";
    currentError: string | null;

    limit: number;
    offset: number;

    createStatus: "idle" | "loading" | "succeeded" | "failed";
    createError: string | null;
}

const initialState: ScreensState = {
    items: [],
    total: 0,

    listStatus: "idle",
    listError: null,

    currentScreen: null,
    currentStatus: "idle",
    currentError: null,

    limit: 20,
    offset: 0,

    createStatus: "idle",
    createError: null,
};

// Для списка экранов, вероятно, API возвращает массив ScreenDto
export const fetchScreens = createAsyncThunk<
    { items: ScreenDto[]; total: number },
    { limit?: number; offset?: number } | undefined,
    { state: RootState; rejectValue: string }
>(
    "screens/fetchScreens",
    async ({ limit, offset } = {}, { getState, rejectWithValue }) => {
        const token = getState().auth.token;
        if (!token) return rejectWithValue("Нет токена авторизации");

        try {
            const response = await apiClient.get("/screen", {
                params: {
                    limit: limit ?? getState().screens.limit,
                    offset: offset ?? getState().screens.offset,
                },
            });

            return response.data;
        } catch (error: any) {
            return rejectWithValue(
                error.response?.data?.message || "Ошибка при загрузке экранов"
            );
        }
    }
);

// Для одиночного экрана возвращается ScreenResponseDto
export const fetchScreen = createAsyncThunk<
    Screen,
    string,
    { state: RootState; rejectValue: string }
>(
    "screens/fetchScreen",
    async (id, { getState, rejectWithValue }) => {
        const token = getState().auth.token;
        if (!token) return rejectWithValue("Нет токена авторизации");

        try {
            const response = await apiClient.get(`screen/${id}`);
            const data = response.data;
            return {
                screen: data.screen,
                type: [],
                config: data.cfg
            };

        } catch (error: any) {
            return rejectWithValue(
                error.response?.data?.message ||
                error.response?.data?.error ||
                "Ошибка при загрузке экрана"
            );
        }
    }
);

export const createScreen = createAsyncThunk<
    ScreenDto,
    CreateScreenData,
    { state: RootState; rejectValue: string }
>(
    "screens/createScreen",
    async (screenData, { getState, rejectWithValue }) => {
        const token = getState().auth.token;
        if (!token) return rejectWithValue("Нет токена авторизации");

        try {
            const response = await apiClient.post(
                "/screen/pair/confirm",
                screenData
            );
            // Если бэкенд возвращает полный объект ScreenResponseDto, измените тип
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
        clearScreens(state) {
            state.items = [];
            state.total = 0;
            state.listStatus = "idle";
            state.offset = 0;
        },

        resetCreateStatus(state) {
            state.createStatus = "idle";
            state.createError = null;
        },

        clearCurrentScreen(state) {
            state.currentScreen = null;
            state.currentStatus = "idle";
            state.currentError = null;
        },

        setPagination(
            state,
            action: PayloadAction<{ limit: number; offset: number }>
        ) {
            state.limit = action.payload.limit;
            state.offset = action.payload.offset;
        },

        // Дополнительные редьюсеры для обновления данных экрана
        updateCurrentScreenConfig(
            state,
            action: PayloadAction<Partial<SignageConfig>>
        ) {
            if (state.currentScreen) {
                state.currentScreen.config = {
                    ...state.currentScreen.config,
                    ...action.payload,
                };
            }
        },

        updateCurrentScreenTypes(
            state,
            action: PayloadAction<string[]>
        ) {
            if (state.currentScreen) {
                state.currentScreen.type = action.payload;
            }
        },
    },

    extraReducers: (builder) => {
        builder
            /* ===== List ===== */
            .addCase(fetchScreens.pending, (state) => {
                state.listStatus = "loading";
                state.listError = null;
            })
            .addCase(fetchScreens.fulfilled, (state, action) => {
                state.listStatus = "succeeded";
                state.items = action.payload.items;
                state.total = action.payload.total;
            })
            .addCase(fetchScreens.rejected, (state, action) => {
                state.listStatus = "failed";
                state.listError = action.payload ?? "Unknown error";
            })

            /* ===== Single Screen ===== */
            .addCase(fetchScreen.pending, (state) => {
                state.currentStatus = "loading";
                state.currentError = null;
            })
            .addCase(fetchScreen.fulfilled, (state, action) => {
                state.currentStatus = "succeeded";
                state.currentScreen = action.payload;

                // Обновляем базовую информацию об экране в списке
                const screenData = action.payload.screen;
                const index = state.items.findIndex(
                    (s) => s.id === screenData.id
                );

                if (index >= 0) {
                    state.items[index] = screenData;
                } else {
                    state.items.push(screenData);
                    state.total += 1;
                }
            })
            .addCase(fetchScreen.rejected, (state, action) => {
                state.currentStatus = "failed";
                state.currentError = action.payload ?? "Unknown error";
            })

            /* ===== Create Screen ===== */
            .addCase(createScreen.pending, (state) => {
                state.createStatus = "loading";
                state.createError = null;
            })
            .addCase(createScreen.fulfilled, (state, action) => {
                state.createStatus = "succeeded";
                state.items.unshift(action.payload);
                state.total += 1;
            })
            .addCase(createScreen.rejected, (state, action) => {
                state.createStatus = "failed";
                state.createError = action.payload ?? "Unknown error";
            });
    },
});

export const {
    clearScreens,
    resetCreateStatus,
    clearCurrentScreen,
    setPagination,
    updateCurrentScreenConfig,
    updateCurrentScreenTypes,
} = screensSlice.actions;

// Селекторы
export const selectCurrentScreen = (state: RootState) => state.screens.currentScreen;
export const selectCurrentScreenData = (state: RootState) => state.screens.currentScreen?.screen;
export const selectCurrentScreenConfig = (state: RootState) => state.screens.currentScreen?.config;
export const selectCurrentScreenTypes = (state: RootState) => state.screens.currentScreen?.type;
export const selectScreensList = (state: RootState) => state.screens.items;
export const selectScreensTotal = (state: RootState) => state.screens.total;
export const selectScreensPagination = (state: RootState) => ({
    limit: state.screens.limit,
    offset: state.screens.offset,
});

export default screensSlice.reducer;
