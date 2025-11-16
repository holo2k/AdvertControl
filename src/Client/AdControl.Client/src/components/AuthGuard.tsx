import React, { useEffect, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchProfile } from "../features/profile/profileSlice";
import { useNavigate, useLocation } from "react-router-dom";

export function AuthGuard({ children }: { children: React.ReactNode }) {
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const location = useLocation();

    const { data, loading, error } = useSelector((state: any) => state.profile);
    const { token } = useSelector((state: any) => state.auth); // ✅ добавляем проверку токена

    const initialized = useRef(false);
    const requestInProgress = useRef(false);

    // 🚀 Если токена нет вообще — сразу на /unauthorized
    useEffect(() => {
        if (!token) {
            if (
                location.pathname !== "/login" &&
                location.pathname !== "/unauthorized"
            ) {
                navigate("/unauthorized", { replace: true });
            }
            return;
        }
    }, [token, navigate, location.pathname]);

    // 🧭 Подгружаем профиль, если токен есть
    useEffect(() => {
        if (!token) return; // без токена не вызываем fetchProfile

        if (requestInProgress.current) return;

        if (!initialized.current && !data && !loading && !error) {
            initialized.current = true;
            requestInProgress.current = true;

            dispatch(fetchProfile())
                .unwrap()
                .finally(() => {
                    requestInProgress.current = false;
                });
        }
    }, [token, data, loading, error, dispatch]);

    // 🔁 Следим за ошибками и состоянием
    useEffect(() => {
        if (!loading && !requestInProgress.current) {
            if (error) {
                if (
                    location.pathname !== "/unauthorized" &&
                    location.pathname !== "/login"
                ) {
                    navigate("/unauthorized", { replace: true });
                }
            } else if (data?.username) {
                if (
                    location.pathname === "/login" ||
                    location.pathname === "/unauthorized"
                ) {
                    navigate("/", { replace: true });
                }
            }
        }
    }, [data, error, loading, navigate, location.pathname]);

    // 🌀 Загрузка профиля
    if (loading && token && !initialized.current) {
        return (
            <div className="flex justify-center items-center h-screen text-gray-600">
                Загрузка профиля...
            </div>
        );
    }

    return <>{children}</>;
}
