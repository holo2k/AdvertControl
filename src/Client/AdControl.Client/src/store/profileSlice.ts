import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { apiClient } from "../api/apiClient.ts";

export const fetchProfile = createAsyncThunk(
    "profile/fetchProfile",
    async (_, { rejectWithValue }) => {
        try {
            const userResponse = await apiClient.post(`/auth/get-current-user-info`);
            return userResponse.data;

        } catch (error: any) {
            console.error("Ошибка при загрузке профиля:", error);
            return rejectWithValue(error.response?.data || "Ошибка загрузки профиля");
        }
    }
);

const profileSlice = createSlice({
    name: "profile",
    initialState: {
        data: null,
        loading: true,
        error: null,
    },
    reducers: {
            clearProfile(state) {
                state.data = null;
                state.error = null;
                state.loading = false;
            }
        },
    extraReducers: (builder) => {
        builder
            .addCase(fetchProfile.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(fetchProfile.fulfilled, (state, action) => {
                state.loading = false;
                state.data = action.payload;
            })
            .addCase(fetchProfile.rejected, (state, action) => {
                state.loading = false;
                // @ts-expect-error состояние ошибки
                state.error = action.payload;
            });
    },
});

export default profileSlice.reducer;
export const { clearProfile } = profileSlice.actions;
