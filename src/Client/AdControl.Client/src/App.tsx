import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { DashboardPage } from "./components/DashboardPage";
import { ScreensPage } from "./components/ScreensPage/ScreensPage.tsx";
import { ConfigurationsPage } from "./components/ConfigurationsPage";
import { TemplatesPage } from "./components/TemplatesPage";
import { LoginPage } from "./components/loginPage/LoginPage.tsx";
import { MainLayout } from "./components/layouts/MainLayout";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { UnauthorizedPage } from "./components/UnauthorizedPage";
import { ConfigEditor } from "./components/configEditor/ConfigEditor.tsx";
import {ProfileScreen} from "./components/ProfileScreen/ProfileScreen.tsx";
import {SignageCreatorPage} from './components/SignageCreatorPage/SignageCreatorPage.tsx'

export default function App() {
    return (
        <Router>
            <Routes>
                {/* Публичные роуты */}
                <Route path="/login" element={<LoginPage />} />
                <Route path="/unauthorized" element={<UnauthorizedPage />} />

                {/* Защищённые роуты */}
                <Route
                    path="/"
                    element={
                        <ProtectedRoute>
                            <MainLayout />
                        </ProtectedRoute>
                    }
                >
                    <Route index element={<DashboardPage />} />
                    <Route path="dashboard" element={<DashboardPage />} />
                    <Route path="screens" element={<ScreensPage />} />
                    <Route path="configurations" element={<ConfigurationsPage />} />
                    <Route path="templates" element={<TemplatesPage />} />
                    <Route path="config-edit" element={<ConfigEditor />} />
                    <Route path="profile" element={<ProfileScreen />} />
                    <Route path="s" element={<SignageCreatorPage />} />
                </Route>

                {/* Любой неизвестный путь */}
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </Router>
    );
}
