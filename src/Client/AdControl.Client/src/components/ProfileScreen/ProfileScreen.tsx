import { Card, CardContent } from "../ui/card.tsx";
import { Label } from "../ui/label.tsx";
import { Input } from "../ui/input.tsx";
import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchProfile } from "../../store/profileSlice.ts";
import { Button } from "../ui/button.tsx";
import { Avatar, AvatarFallback } from "../ui/avatar.tsx";
import { apiClient } from "../../api/apiClient.ts";
import "./ProfileScreen.css";

export const ProfileScreen = () => {
    const dispatch = useDispatch();

    const { data, loading } = useSelector((state: any) => state.profile);
    const { token } = useSelector((state: any) => state.auth);

    const [email, setEmail] = useState("");
    const [firstname, setFirstname] = useState("");
    const [lastname, setLastname] = useState("");
    const [phone, setPhone] = useState("");

    useEffect(() => {
        if (token) {
            dispatch(fetchProfile());
        }
    }, [dispatch, token]);

    useEffect(() => {
        if (data) {
            setEmail(data.email || "");
            setFirstname(data.firstName || "");
            setLastname(data.lastName || "");
            setPhone(data.phoneNumber || "");
        }
    }, [data]);

    const initials = data?.username
        ? data.username.split(" ").map((n: string) => n[0]?.toUpperCase()).join("")
        : "U";

    async function handleSave() {
        try {
            await apiClient.patch("/auth/update-current", {
                email,
                FirstName: firstname,
                LastName: lastname,
                phone,
            });
            dispatch(fetchProfile());
        } catch (e) {
            console.error(e);
            console.log("Ошибка при обновлении профиля");
        }
    }

    if (loading) return <div className="profile-screen">Загрузка...</div>;

    return (
        <div className="profile-screen">
            <div className="profile-header">
                <h1>Личная информация</h1>
            </div>

            <Card className="profile-card">
                <div style={{ display: "flex", flexDirection: "row" }}>
                    <Avatar className="profile-avatar">
                        <AvatarFallback className="avatar-fallback">
                            {initials}
                        </AvatarFallback>
                    </Avatar>
                    <h2 className="profile-username">{data?.username || "Не указано"}</h2>
                </div>

                <CardContent className="profile-content">
                    <div className="form-field">
                        <Label>Email</Label>
                        <Input
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </div>

                    <div className="form-field">
                        <Label>Имя</Label>
                        <Input
                            value={firstname}
                            onChange={(e) => setFirstname(e.target.value)}
                        />
                    </div>

                    <div className="form-field">
                        <Label>Фамилия</Label>
                        <Input
                            value={lastname}
                            onChange={(e) => setLastname(e.target.value)}
                        />
                    </div>

                    <div className="form-field">
                        <Label>Телефон</Label>
                        <Input
                            value={phone}
                            onChange={(e) => setPhone(e.target.value)}
                        />
                    </div>
                </CardContent>
            </Card>

            <div className="profile-actions">
                <Button className="save-button" onClick={handleSave}>
                    Сохранить изменения
                </Button>
            </div>
        </div>
    );
};
