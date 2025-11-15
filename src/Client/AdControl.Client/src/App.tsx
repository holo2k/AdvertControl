import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { DashboardPage } from "./components/DashboardPage";
import { ScreensPage } from "./components/ScreensPage";
import { ConfigurationsPage } from "./components/ConfigurationsPage";
import { TemplatesPage } from "./components/TemplatesPage";
import { LoginPage } from "./components/loginPage/LoginPage.tsx";
import { MainLayout } from "./components/layouts/MainLayout";
import { AuthGuard } from "./components/AuthGuard";
import UnauthorizedPage from "./components/UnauthorizedPage";

export default function App() {
    return (
        <Router>
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/unauthorized" element={<UnauthorizedPage />} />

                <Route
                    path="/*"
                    element={
                        <AuthGuard>
                            <MainLayout>
                                <Routes>
                                    <Route path="/" element={<DashboardPage />} />
                                    <Route path="/dashboard" element={<DashboardPage />} />
                                    <Route path="/screens" element={<ScreensPage />} />
                                    <Route path="/configurations" element={<ConfigurationsPage />} />
                                    <Route path="/templates" element={<TemplatesPage />} />
                                </Routes>
                            </MainLayout>
                        </AuthGuard>
                    }
                />

                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </Router>
    );
}
