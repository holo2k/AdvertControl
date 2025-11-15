import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { apiClient } from "../../api/apiClient";

export const fetchProfile = createAsyncThunk(
    "profile/fetchProfile",
    async (_, { rejectWithValue }) => {
        try {
            const idResponse = await apiClient.post("/auth/get-current-user-id", {});
            const userId = idResponse.data?.id;
            if (!userId) throw new Error("Не удалось получить ID пользователя");

            const userResponse = await apiClient.post(`/auth/get-user-info-by/${userId}`, {});
            return userResponse.data;

        } catch (error) {
            console.error("Ошибка при загрузке профиля:", error);
            return rejectWithValue(error.response?.data || "Ошибка загрузки профиля");
        }
    }
);

const profileSlice = createSlice({
    name: "profile",
    initialState: {
        data: null,
        loading: false,
        error: null,
    },
    reducers: {},
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
                state.error = action.payload;
            });
    },
});

export default profileSlice.reducer;
