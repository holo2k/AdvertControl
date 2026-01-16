import { Card, CardContent } from "../ui/card.tsx";
import { Label } from "../ui/label.tsx";
import { Input } from "../ui/input.tsx";
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { useParams } from "react-router-dom"; // Импортируем useParams
import { fetchProfile } from "../../store/profileSlice.ts";
import { Button } from "../ui/button.tsx";
import { Avatar, AvatarFallback } from "../ui/avatar.tsx";
import { apiClient } from "../../api/apiClient.ts";
import "./ProfileScreen.css";
import type {AppDispatch} from "../../store/store.ts";

export const ProfileScreen = () => {
    const dispatch = useDispatch<AppDispatch>();
    const { id } = useParams(); // Получаем id из URL параметров

    const { data, loading } = useSelector((state: any) => state.profile);
    const { token } = useSelector((state: any) => state.auth);

    const [email, setEmail] = useState("");
    const [firstname, setFirstname] = useState("");
    const [lastname, setLastname] = useState("");
    const [phone, setPhone] = useState("");
    const [externalUserData, setExternalUserData] = useState<any>(null); // Данные внешнего пользователя
    const [externalLoading, setExternalLoading] = useState(false); // Загрузка внешних данных

    // Определяем, просматриваем ли мы свой профиль или чужой
    const isExternalProfile = !!id;

    // Загружаем данные в зависимости от наличия id
    useEffect(() => {
        if (token) {
            if (isExternalProfile) {
                // Загружаем данные внешнего пользователя
                fetchExternalUserData();
            } else {
                // Загружаем свой профиль
                dispatch(fetchProfile());
            }
        }
    }, [dispatch, token, id]);

    // Загрузка данных внешнего пользователя
    const fetchExternalUserData = async () => {
        if (!id) return;

        setExternalLoading(true);
        try {
            const response = await apiClient.post(`/auth/get-user-info-by/${id}`);
            setExternalUserData(response.data);

            // Устанавливаем данные из ответа
            if (response.data) {
                setEmail(response.data.email || "");
                setFirstname(response.data.firstName || "");
                setLastname(response.data.lastName || "");
                setPhone(response.data.phoneNumber || "");
            }
        } catch (error) {
            console.error("Ошибка при загрузке данных пользователя:", error);
        } finally {
            setExternalLoading(false);
        }
    };

    // Используем либо данные из Redux, либо данные внешнего пользователя
    const currentData = isExternalProfile ? externalUserData : data;

    useEffect(() => {
        if (currentData) {
            setEmail(currentData.email || "");
            setFirstname(currentData.firstName || "");
            setLastname(currentData.lastName || "");
            setPhone(currentData.phoneNumber || "");
        }
    }, [currentData]);

    // Получаем инициалы
    const initials = currentData?.username
        ? currentData.username.split(" ").map((n: string) => n[0]?.toUpperCase()).join("")
        : "U";

    async function handleSave() {
        // Сохранять изменения можно только для своего профиля
        if (isExternalProfile) {
            alert("Вы не можете изменять профиль другого пользователя");
            return;
        }

        try {
            await apiClient.patch("/auth/update-current", {
                email,
                FirstName: firstname,
                LastName: lastname,
                phone,
            });
            dispatch(fetchProfile()); // Обновляем данные профиля
        } catch (e) {
            console.error(e);
        }
    }

    if (loading || (isExternalProfile && externalLoading)) {
        return <div className="profile-screen">Загрузка...</div>;
    }

    return (
        <div className="profile-screen">
            <Card className="profile-card">
                <div className="profile-avatar-container">
                    <Avatar className="profile-avatar">
                        <AvatarFallback className="avatar-fallback">
                            {initials}
                        </AvatarFallback>
                    </Avatar>
                    <h2 className="profile-username">{currentData?.username || "Не указано"}</h2>
                </div>

                <CardContent className="profile-content">
                    <div className="form-field">
                        <Label>ID</Label>
                        <Input
                            value={id}
                            onChange={(e) => setFirstname(e.target.value)}
                            disabled={isExternalProfile}
                            style={{fontSize: "14px"}}
                        />
                    </div>
                    <div className="form-field">
                        <Label>Имя</Label>
                        <Input
                            value={firstname}
                            onChange={(e) => setFirstname(e.target.value)}
                            disabled={isExternalProfile}
                        />
                    </div>

                    <div className="form-field">
                        <Label>Фамилия</Label>
                        <Input
                            value={lastname}
                            onChange={(e) => setLastname(e.target.value)}
                            disabled={isExternalProfile}
                        />
                    </div>

                    <div className="form-field">
                        <Label>Телефон</Label>
                        <Input
                            value={phone}
                            onChange={(e) => setPhone(e.target.value)}
                            disabled={isExternalProfile}
                        />
                    </div>
                </CardContent>
            </Card>

            {/* Кнопка сохранения только для своего профиля */}
            {!isExternalProfile && (
                <div className="profile-actions">
                    <Button className="save-button" onClick={handleSave}>
                        Сохранить изменения
                    </Button>
                </div>
            )}
        </div>
    );
};
