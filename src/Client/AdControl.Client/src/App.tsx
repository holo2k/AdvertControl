import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { DashboardPage } from "./components/DashboardPage";
import { ScreensPage } from "./components/ScreensPage/ScreensPage.tsx";
import { TemplatesPage } from "./components/TemplatesPage";
import { LoginPage } from "./components/loginPage/LoginPage.tsx";
import { MainLayout } from "./components/layouts/MainLayout";
import {ProfileScreen} from "./components/ProfileScreen/ProfileScreen.tsx";
import {SignageCreatorPage} from './components/SignageCreatorPage/SignageCreatorPage.tsx'
import {ScreenDetail} from "./components/ScreenDetailPage/ScreenDetailPage.tsx";
import {SettingsPage} from "./components/SettingsPage.tsx";

export default function App() {
    return (
        <Router>
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/" element={<MainLayout />} >
                    <Route index element={<DashboardPage />} />
                    <Route path="dashboard" element={<DashboardPage />} />
                    <Route path="screens" element={<ScreensPage />} />
                    <Route path="templates" element={<TemplatesPage />} />
                    <Route path="profile" element={<ProfileScreen />} />
                    <Route path="settings" element={<SettingsPage />} />
                    <Route path="screen/:id" element={<ScreenDetail />} />
                    <Route path="screen/:id/config" element={<SignageCreatorPage />} />
                    <Route path="screen/:id/config/edit" element={<SignageCreatorPage />} />
                </Route>

                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </Router>
    );
}
