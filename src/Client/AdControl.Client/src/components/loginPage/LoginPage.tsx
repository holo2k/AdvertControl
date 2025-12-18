import React, { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { loginUser } from "../../store/authSlice";
import { useNavigate } from "react-router-dom";
import { Eye, EyeOff } from "lucide-react";
import "./LoginPage.css";
import Dither from './Background.tsx';

export const LoginPage: React.FC = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const { loading, error } = useSelector((state: any) => state.auth);

    const [showPassword, setShowPassword] = useState(false);
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [isSuccess, setIsSuccess] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        // @ts-expect-error хзхзхз
        const resultAction: any = await dispatch(loginUser({ username, password }));

        if (resultAction.meta.requestStatus === "fulfilled") {
            setIsSuccess(true);
            setTimeout(() => navigate("/"), 1000);
        }
    };

    return (
        <div className="login-page-wrapper">
            <div className="dither-container">
                <Dither
                    waveColor={[0.4, 0.4, 0.4]}
                    disableAnimation={false}
                    enableMouseInteraction={false}
                    mouseRadius={0.2}
                    colorNum={100}
                    waveAmplitude={0.3}
                    waveFrequency={3}
                    waveSpeed={0.05}
                />
            </div>

            <div className="login-card">
                <h1 className="login-logo">AdControl</h1>
                <p className="login-subtitle">
                    Войдите в систему, чтобы управлять своими экранами
                </p>

                <form className="login-form" onSubmit={handleSubmit}>
                    <label className="login-label">ЛОГИН</label>
                    <input
                        type="text"
                        className="login-input"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />

                    <label className="login-label">ПАРОЛЬ</label>
                    <div className="login-password">
                        <input
                            type={showPassword ? "text" : "password"}
                            className="login-input"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                        />
                        <button
                            type="button"
                            className="login-eye"
                            onClick={() => setShowPassword(!showPassword)}
                        >
                            {showPassword ? <EyeOff size={14} /> : <Eye size={14} />}
                        </button>
                    </div>

                    <a href="#" className="login-forgot">
                        Забыли пароль?
                    </a>

                    <button className="login-button" disabled={loading}>
                        {loading ? "Входим..." : "Войти"}
                    </button>

                    {error && <div className="login-error">{String(error)}</div>}
                    {isSuccess && <div className="login-success active">Успешный вход</div>}
                </form>
                <footer className="login-footer">
                    © 2025 AdControl. All rights reserved.
                </footer>
            </div>
        </div>
    );
};
