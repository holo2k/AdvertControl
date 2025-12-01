import {Card, CardContent} from "../ui/card.tsx";
import {Label} from "../ui/label.tsx";
import {Input} from "../ui/input.tsx";
import { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchProfile } from "../../store/profileSlice.ts";
import {Button} from "../ui/button.tsx";
import {Avatar, AvatarFallback} from "../ui/avatar.tsx";
import "./ProfileScreen.css";

export const ProfileScreen = () => {
    const dispatch = useDispatch();

    const { data } = useSelector((state: any) => state.profile);
    const { token } = useSelector((state: any) => state.auth);

    useEffect(() => {
        if (token) {
            dispatch(fetchProfile());
        }
    }, [dispatch, token]);

    const initials = data?.username
        ? data.username
            .split(" ")
            .map((n: string) => n[0]?.toUpperCase())
            .join("")
        : "U";

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
                        <Label htmlFor="company-name">Пользователь</Label>
                        <Input id="company-name" defaultValue={data?.username || "Не указано"} />
                    </div>
                    <div className="form-field">
                        <Label htmlFor="company-name">email</Label>
                        <Input id="company-name" defaultValue={data?.email || "Не указано"} />
                    </div>
                    <div className="form-field">
                        <Label htmlFor="company-name">Имя</Label>
                        <Input id="company-name" defaultValue={data?.firstname || "Не указано"} />
                    </div>
                    <div className="form-field">
                        <Label htmlFor="company-name">Фамилия</Label>
                        <Input id="company-name" defaultValue={data?.lastname || "Не указано"} />
                    </div>
                    <div className="form-field">
                        <Label htmlFor="company-name">Телефон</Label>
                        <Input id="company-name" defaultValue={data?.phoneNumber || "Не указан"} />
                    </div>

                </CardContent>
            </Card>

            <div className="profile-actions">
                <Button variant="outline">Отмена</Button>
                <Button className="save-button">Сохранить изменения</Button>
            </div>
        </div>
    )
}
