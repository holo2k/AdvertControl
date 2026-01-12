import { ChevronDown } from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "../ui/avatar.tsx";
import { Button } from "../ui/button.tsx";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "../ui/dropdown-menu.tsx";
import { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchProfile } from "../../store/profileSlice.ts";
import { logoutUser } from "../../store/authSlice.ts";
import { useNavigate } from "react-router-dom";
import ContentLoader from "react-content-loader";

export function Header({ isMinimal = false }: { isMinimal?: boolean }) {
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const { data, loading, error } = useSelector((state: any) => state.profile);
  const { token } = useSelector((state: any) => state.auth);

  useEffect(() => {
    if (token) {
      // @ts-expect-error фикс линтера
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
    // @ts-expect-error фикс линтера
    dispatch(logoutUser());
    setTimeout(() => navigate("/login"), 50);
  };

  // ContentLoader для имени пользователя
  const UsernameLoader = () => (
      <ContentLoader
          speed={2}
          width={120}
          height={20}
          viewBox="0 0 120 20"
          backgroundColor="#f3f3f3"
          foregroundColor="#ecebeb"
      >
        <rect x="0" y="0" rx="4" ry="4" width="80" height="12" />
      </ContentLoader>
  );

  return (
      <header className="h-16 bg-white border-b border-gray-200 px-6 flex items-center justify-between fixed top-0 left-0 right-0 z-50">
        <div className="flex items-center gap-2">
          <img src="/icon.ico" alt="Логотип AdvertControl" className="h-8 w-8"/>
          <span className="font-semibold text-gray-800">AdvertControl</span>
        </div>

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
                            {loading ? (
                                <ContentLoader
                                    speed={2}
                                    width={32}
                                    height={32}
                                    viewBox="0 0 32 32"
                                    backgroundColor="#f3f3f3"
                                    foregroundColor="#ecebeb"
                                >
                                  <circle cx="16" cy="16" r="16" />
                                </ContentLoader>
                            ) : (
                                initials
                            )}
                          </AvatarFallback>
                        </Avatar>

                        {loading ? (
                            <UsernameLoader />
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
                      <DropdownMenuItem onClick={() => navigate("/crm/profile")}>
                        Профиль
                      </DropdownMenuItem>
                      <DropdownMenuItem onClick={() => navigate("/crm/settings")}>Настройки</DropdownMenuItem>
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
