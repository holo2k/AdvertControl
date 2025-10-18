import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { useState } from "react";
import { Header } from "./components/Header";
import { Sidebar } from "./components/Sidebar";
import { DashboardPage } from "./components/DashboardPage";
import { ScreensPage } from "./components/ScreensPage";
import { ConfigurationsPage } from "./components/ConfigurationsPage";
import { TemplatesPage } from "./components/TemplatesPage";

export default function App() {
    const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

    return (
        <Router>
            <div className="min-h-screen bg-gray-50">
                <Header />
                <Sidebar
                    collapsed={sidebarCollapsed}
                    onToggleCollapse={() => setSidebarCollapsed(!sidebarCollapsed)}
                />
                <main
                    className="pt-16 transition-all duration-300"
                    style={{
                        marginLeft: sidebarCollapsed ? "4rem" : "16rem",
                    }}
                >
                    <div className="p-8">
                        <Routes>
                            <Route path="/" element={<DashboardPage />} />
                            <Route path="/dashboard" element={<DashboardPage />} />
                            <Route path="/screens" element={<ScreensPage />} />
                            <Route path="/configurations" element={<ConfigurationsPage />} />
                            <Route path="/templates" element={<TemplatesPage />} />
                        </Routes>
                    </div>
                </main>
            </div>
        </Router>
    );
}
