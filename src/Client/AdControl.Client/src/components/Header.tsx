import { Search, Bell, ChevronDown } from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "./ui/dropdown-menu";
import { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchProfile } from "../features/profile/profileSlice";
import { logoutUser } from "../store/authSlice";
import { useNavigate } from "react-router-dom";

export function Header({ isMinimal = false }: { isMinimal?: boolean }) {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const { data, loading, error } = useSelector((state: any) => state.profile);
  const { token } = useSelector((state: any) => state.auth);

  useEffect(() => {
    if (token) {
      dispatch(fetchProfile());
    }
  }, [dispatch, token]);

  const initials = data?.username
      ? data.username
          .split(" ")
          .map((n: string) => n[0]?.toUpperCase())
          .join("")
      : "U";

  const handleLogin = () => {
    navigate("/login");
  };

  const handleLogout = () => {
    dispatch(logoutUser());
    setTimeout(() => navigate("/login"), 50);
  };

  return (
      <header className="h-16 bg-white border-b border-gray-200 px-6 flex items-center justify-between fixed top-0 left-0 right-0 z-10">
        <div className="flex items-center gap-4">
          <span className="font-semibold text-gray-800">AdControl</span>
        </div>

        {!isMinimal && (
            <div className="flex-1 max-w-xl mx-8">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                <Input
                    type="search"
                    placeholder="Поиск экранов, конфигураций..."
                    className="pl-10 bg-gray-50 border-gray-200"
                />
              </div>
            </div>
        )}

        {!isMinimal && (
            <div className="flex items-center gap-4">
              {!token ? (
                  <Button onClick={handleLogin} variant="default">
                    Войти
                  </Button>
              ) : (
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" className="gap-2">
                        <Avatar className="h-8 w-8">
                          <AvatarImage src="" />
                          <AvatarFallback className="bg-blue-100 text-blue-700">
                            {initials}
                          </AvatarFallback>
                        </Avatar>

                        {loading ? (
                            <span className="text-gray-500">Загрузка...</span>
                        ) : error ? (
                            <span className="text-red-500">Ошибка</span>
                        ) : (
                            <span className="text-gray-700">
                      {data?.username || "Неизвестно"}
                    </span>
                        )}

                        <ChevronDown className="h-4 w-4 text-gray-500" />
                      </Button>
                    </DropdownMenuTrigger>

                    <DropdownMenuContent align="end" className="w-48">
                      <DropdownMenuLabel>Мой аккаунт</DropdownMenuLabel>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem>Профиль</DropdownMenuItem>
                      <DropdownMenuItem>Настройки</DropdownMenuItem>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem
                          className="text-red-600"
                          onClick={handleLogout}
                      >
                        Выйти
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
              )}
            </div>
        )}
      </header>
  );
}
