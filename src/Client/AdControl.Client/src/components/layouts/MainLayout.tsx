import React, { type ReactNode, useState } from "react";
import { Header } from "../Header";
import { Sidebar } from "../Sidebar";

interface MainLayoutProps {
    children: ReactNode;
}

export const MainLayout: React.FC<MainLayoutProps> = ({ children }) => {
    const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

    return (
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
                <div className="p-8">{children}</div>
            </main>
        </div>
    );
};
