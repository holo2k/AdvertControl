import axios from "axios";
import {toast} from "sonner";

const MINIO_PUBLIC_URL = 'https://advertcontrol.ru/files';

const apiClient = axios.create({
    baseURL: "https://advertcontrol.ru/api/",
    headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem("token");
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response) {
            const { status } = error.response;

            if (status === 401) {
                localStorage.removeItem("token");
                window.location.href = "/login";
            }
        } else if (error.request) {
            toast.error("Network error:", error.request);
        } else {
            toast.error("Error:", error.message);
        }
        return Promise.reject(error);
    }
);

export { apiClient, MINIO_PUBLIC_URL };
