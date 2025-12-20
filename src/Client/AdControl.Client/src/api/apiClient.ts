import axios from "axios";

const MINIO_PUBLIC_URL='http://localhost:9000/files'

const apiClient = axios.create({
    baseURL: "http://localhost:5000/api/",
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

export { apiClient, MINIO_PUBLIC_URL };
