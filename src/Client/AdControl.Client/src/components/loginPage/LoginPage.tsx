import React, { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { loginUser } from "../../store/authSlice";
import { useNavigate } from "react-router-dom";
import "./LoginPage.css";

export const LoginPage: React.FC = () => {
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const { loading, error} = useSelector((state: any) => state.auth);

    const [showPassword, setShowPassword] = useState(false);
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [isSuccess, setIsSuccess] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        const resultAction: any = await dispatch(loginUser({ username, password }));

        if (resultAction.meta.requestStatus === "fulfilled") {
            setIsSuccess(true);
            setTimeout(() => {
                navigate("/");
            }, 1000);
        }
    };

    return (
        <div className="login-wrapper">
            <header className="login-header">
                <h1 className="login-logo">AdControl</h1>
            </header>

            <main className="login-main">
                <div className="login-bg"></div>

                <div className="login-card">
                    <h2 className="login-title">Вход</h2>

                    <form className="login-form" onSubmit={handleSubmit}>
                        <div className="login-field">
                            <input
                                type="text"
                                id="login"
                                className="login-input"
                                placeholder=" "
                                required
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                            />
                            <label htmlFor="login" className="login-label">LOGIN</label>
                        </div>

                        <div className="login-field">
                            <input
                                type={showPassword ? "text" : "password"}
                                id="password"
                                className="login-input"
                                required
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                            <label htmlFor="password" className="login-label">PASSWORD</label>
                            <span
                                className="login-showpass"
                                onClick={() => setShowPassword(!showPassword)}
                            >
                                👁
                            </span>
                        </div>

                        <div className="login-options">
                            <label className="login-remember">
                                <input type="checkbox" /> Запомнить меня?
                            </label>
                            <a href="#" className="login-forgot">
                                Забыли пароль?
                            </a>
                        </div>

                        <button type="submit" className="login-btn" disabled={loading}>
                            {loading ? "Входим..." : "Войти в аккаунт"}
                        </button>

                        {error && <p className="login-error">{String(error)}</p>}
                        {isSuccess && <p className="login-success">Успешный вход!</p>}
                    </form>
                </div>
            </main>
        </div>
    );
};
