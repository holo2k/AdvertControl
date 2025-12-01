import {useState } from "react";
import { Header } from "../Header";
import { Sidebar } from "../Sidebar";
import {Outlet} from "react-router-dom";


export const MainLayout = () => {
    const [sidebarCollapsed, setSidebarCollapsed] = useState(true);

    return (
        <div className="min-h-screen bg-gray-50">
            <Header />
            <Sidebar
                collapsed={sidebarCollapsed}
                onToggleCollapse={() => setSidebarCollapsed(!sidebarCollapsed)}
            />
            <main
                className="pt-16 transition-all duration-300 z-0"
                style={{
                    marginLeft: "4rem",
                }}
            >
                <div className="p-8"><Outlet /></div>
            </main>
        </div>
    );
};
