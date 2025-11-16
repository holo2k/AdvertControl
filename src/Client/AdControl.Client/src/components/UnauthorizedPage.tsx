import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { AlertTriangle } from "lucide-react";
import { useNavigate } from "react-router-dom";
import "./UnauthorizedPage.css";

export default function UnauthorizedPage() {
    const navigate = useNavigate();

    return (
        <div className="unauth-page">
            <Card className="unauth-card">
                <CardHeader className="unauth-header">
                    <div className="unauth-icon">
                        <AlertTriangle className="icon" />
                    </div>
                    <CardTitle className="unauth-title">
                        Доступ запрещён
                    </CardTitle>
                </CardHeader>

                <CardContent className="unauth-content">
                    <p className="unauth-text">
                        У вас нет доступа к этой странице.
                        Пожалуйста, выполните вход, чтобы продолжить.
                    </p>

                    <Button
                        onClick={() => navigate("/login")}
                        className="unauth-btn"
                    >
                        Перейти на страницу входа
                    </Button>
                </CardContent>
            </Card>
        </div>
    );
}
