import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { apiClient } from "../api/apiClient";

export const loginUser = createAsyncThunk(
    "auth/loginUser",
    async ({ username, password }: { username: string; password: string }, { rejectWithValue }) => {
        try {
            const response = await apiClient.post("/auth/login", { username, password });
            const { token, user } = response.data;

            localStorage.setItem("token", token);
            return { token, user };
        } catch (error: any) {
            return rejectWithValue(error.response?.data?.message || "Ошибка входа");
        }
    }
);

export const logoutUser = createAsyncThunk("auth/logoutUser", async (_, { dispatch }) => {
    try {
        await apiClient.post("/auth/logout", {});
    } catch (e) {
    }
    localStorage.removeItem("token");

    // 🔧 Сбрасываем профиль, если используешь profileSlice
    dispatch({ type: "profile/clearProfile" });

    return {};
});
const authSlice = createSlice({
    name: "auth",
    initialState: {
        token: localStorage.getItem("token") || null,
        user: null,
        loading: false,
        error: null as string | null,
    },
    reducers: {},
    extraReducers: (builder) => {
        builder
            .addCase(loginUser.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(loginUser.fulfilled, (state, action) => {
                state.loading = false;
                state.token = action.payload.token;
                state.user = action.payload.user;
            })
            .addCase(loginUser.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload as string;
            })
            .addCase(logoutUser.fulfilled, (state) => {
                state.token = null;
                state.user = null;
            });
    },
});

export default authSlice.reducer;
