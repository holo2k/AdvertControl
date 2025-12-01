import { LayoutDashboard, Monitor, Settings, FileText, ChevronLeft } from "lucide-react";
import { Button } from "./ui/button";
import { cn } from "./ui/utils";
import { Link, useLocation } from "react-router-dom";

interface SidebarProps {
  collapsed: boolean;
  onToggleCollapse: () => void;
}

const menuItems = [
  { path: "/dashboard", label: "Доска", icon: LayoutDashboard },
  { path: "/screens", label: "Экраны", icon: Monitor },
  { path: "/configurations", label: "Конфигурации", icon: Settings },
  { path: "/templates", label: "Шаблоны", icon: FileText },
];

export function Sidebar({ collapsed, onToggleCollapse }: SidebarProps) {
  const location = useLocation();

  return (
      <aside
          className={cn(
              "fixed left-0 top-16 bottom-0 bg-white border-r border-gray-200 transition-all duration-300",
              collapsed ? "w-16 z-10" : "w-64 z-50"
          )}
      >
        <div className="flex flex-col h-full">
          <nav className="flex-1 px-3 py-4 space-y-1">
            {menuItems.map((item) => {
              const Icon = item.icon;
              const isActive = location.pathname === item.path ||
                  (item.path === "/dashboard" && location.pathname === "/");

              return (
                  <Link
                      key={item.path}
                      to={item.path}
                      className="block"
                  >
                    <Button
                        variant="ghost"
                        className={cn(
                            "w-full justify-start gap-3 h-10",
                            isActive && "bg-blue-50 hover:bg-blue-100",
                            isActive ? "text-blue-700" : "text-gray-700"
                        )}
                    >
                      <Icon className="h-5 w-5 flex-shrink-0" />
                      {!collapsed && <span>{item.label}</span>}
                    </Button>
                  </Link>
              );
            })}
          </nav>

          <div className="p-3 border-t border-gray-200">
            <Button
                variant="ghost"
                size="icon"
                onClick={onToggleCollapse}
                className="w-full"
            >
              <ChevronLeft
                  className={cn(
                      "h-5 w-5 transition-transform",
                      collapsed && "rotate-180"
                  )}
              />
            </Button>
          </div>
        </div>
      </aside>
  );
}
