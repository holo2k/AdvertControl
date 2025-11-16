import { configureStore } from "@reduxjs/toolkit";
import authReducer from "./authSlice";
import profileReducer from "../features/profile/profileSlice";
import screensReducer from "./screenSlice";

export const store = configureStore({
    reducer: {
        auth: authReducer,
        profile: profileReducer,
        screens: screensReducer,
    },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
