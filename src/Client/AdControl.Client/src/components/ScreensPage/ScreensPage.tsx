import { useState, useEffect } from "react";
import { Plus, Search, Filter } from "lucide-react";
import { Button } from "../ui/button.tsx";
import { Input } from "../ui/input.tsx";
import { Card } from "../ui/card.tsx";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "../ui/table.tsx";
import { Badge } from "../ui/badge.tsx";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../ui/select.tsx";
import { useDispatch, useSelector } from "react-redux";
import type { RootState, AppDispatch } from "../../store/store.ts";
import { fetchScreens, setPagination, createScreen, resetCreateStatus } from "../../store/screenSlice.ts";
import { Pagination } from "./Pagination.tsx";
import { CreateScreenForm } from "./CreateScreenForm.tsx";
import ContentLoader from "react-content-loader";
import { useNavigate } from 'react-router-dom';

export function ScreensPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const navigate = useNavigate();


  const dispatch = useDispatch<AppDispatch>();
  const {
    items,
    total,
    limit,
    offset,
    status,
    createStatus,
    createError
  } = useSelector((state: RootState) => state.screens);

  useEffect(() => {
    dispatch(fetchScreens({ limit, offset }));
  }, [dispatch, limit, offset, createStatus]);

  useEffect(() => {
    if (createStatus === "succeeded") {
      setIsDialogOpen(false);
      dispatch(resetCreateStatus());

      if (offset !== 0 || items.length === 0) {
        dispatch(setPagination({ limit, offset: 0 }));
      }
    }
  }, [createStatus, dispatch, offset, items.length, limit]);

  const handleNextPage = () => {
    if (offset + limit < total) {
      dispatch(setPagination({ limit, offset: offset + limit }));
    }
  };

  const handlePrevPage = () => {
    if (offset - limit >= 0) {
      dispatch(setPagination({ limit, offset: offset - limit }));
    }
  };

  const handleCreateScreen = (screenData: {
    name: string;
    resolution: string;
    location: string;
  }) => {
    dispatch(createScreen(screenData));
  };

  const handleDialogOpenChange = (open: boolean) => {
    setIsDialogOpen(open);
    if (!open) {
      // Сбрасываем статус создания при закрытии диалога
      dispatch(resetCreateStatus());
    }
  };

  // 🔹 Фильтрация по имени, локации и статусу (с защитой от undefined)
  const filteredScreens = items.filter((screen) => {
    const screenName = screen.name || "";
    const screenLocation = screen.location || "";

    const matchesSearch =
        screenName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        screenLocation.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesStatus =
        statusFilter === "all" || screen.status === statusFilter;

    return matchesSearch && matchesStatus;
  });

  // 🔹 Бейджи статусов
  const getStatusBadge = (status: string | undefined) => {
    const styles = {
      connected: "bg-green-100 text-green-800 hover:bg-green-200",
      error: "bg-red-100 text-red-800 hover:bg-red-200",
      pending: "bg-yellow-100 text-yellow-800 hover:bg-yellow-200",
    };
    return (
        <Badge className={styles[status as keyof typeof styles] || ""}>
          {status || "unknown"}
        </Badge>
    );
  };

  // Компонент для загрузки таблицы
  const TableLoader = () => (
      <ContentLoader
          speed={2}
          width="100%"
          height={520}
          viewBox="0 0 1200 520"
          backgroundColor="#f3f3f3"
          foregroundColor="#ecebeb"
          className="w-full"
      >
        {/* Заголовки */}
        <rect x="5%"  y="12"  rx="4" ry="4" width="20%" height="48" />
        <rect x="28%" y="12"  rx="4" ry="4" width="22%" height="48" />
        <rect x="52%" y="12"  rx="4" ry="4" width="15%" height="48" />
        <rect x="68%" y="12"  rx="4" ry="4" width="18%" height="48" />
        <rect x="88%" y="12"  rx="4" ry="4" width="10%" height="48" />

        {/* Строка 1 */}
        <rect x="5%"  y="80"  rx="4" ry="4" width="22%" height="40" />
        <rect x="28%" y="80"  rx="4" ry="4" width="25%" height="40" />
        <rect x="52%" y="80"  rx="4" ry="4" width="12%" height="40" />
        <rect x="68%" y="76"  rx="12" ry="12" width="80" height="40" />
        <rect x="88%" y="76"  rx="6"  ry="6"  width="60" height="40" />

        {/* Строка 2 */}
        <rect x="5%"  y="140" rx="4" ry="4" width="22%" height="40" />
        <rect x="28%" y="140" rx="4" ry="4" width="25%" height="40" />
        <rect x="52%" y="140" rx="4" ry="4" width="12%" height="40" />
        <rect x="68%" y="136" rx="12" ry="12" width="80" height="40" />
        <rect x="88%" y="136" rx="6"  ry="6"  width="60" height="40" />

        <rect x="5%"  y="200" rx="4" ry="4" width="22%" height="40" />
        <rect x="28%" y="200" rx="4" ry="4" width="25%" height="40" />
        <rect x="52%" y="200" rx="4" ry="4" width="12%" height="40" />
        <rect x="68%" y="196" rx="12" ry="12" width="80" height="40" />
        <rect x="88%" y="196" rx="6"  ry="6"  width="60" height="40" />

        <rect x="5%"  y="260" rx="4" ry="4" width="22%" height="40" />
        <rect x="28%" y="260" rx="4" ry="4" width="25%" height="40" />
        <rect x="52%" y="260" rx="4" ry="4" width="12%" height="40" />
        <rect x="68%" y="256" rx="12" ry="12" width="80" height="40" />
        <rect x="88%" y="256" rx="6"  ry="6"  width="60" height="40" />
      </ContentLoader>
  );

  return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1>Экраны</h1>
            <p className="text-gray-600 mt-1">Управляйте своей сетью экранов</p>
          </div>

          <Button
              style={{ backgroundColor: "#2563EB" }}
              className="gap-2"
              onClick={() => setIsDialogOpen(true)}
          >
            <Plus className="h-4 w-4" />
            Добавить экран
          </Button>
        </div>

        {/* Таблица экранов */}
        <Card className="shadow-sm">
          <div className="p-4 border-b border-gray-200">
            <div className="flex gap-4">
              <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                <Input
                    placeholder="Искать по имени или расположению..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="pl-10"
                />
              </div>
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger className="w-48">
                  <Filter className="h-4 w-4 mr-2" />
                  <SelectValue placeholder="Статус" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Все статусы</SelectItem>
                  <SelectItem value="connected">Подключено</SelectItem>
                  <SelectItem value="error">Ошибка</SelectItem>
                  <SelectItem value="pending">Соединение</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div>
            {status === "loading" ? (
                <TableLoader />
            ) : filteredScreens.length === 0 ? (
                <p className="p-4 text-gray-500">Нет экранов</p>
            ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Название</TableHead>
                      <TableHead>Расположение</TableHead>
                      <TableHead>Разрешение</TableHead>
                      <TableHead>Статус подключения</TableHead>
                      <TableHead className="text-right">Действия</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {filteredScreens.map((screen) => (
                        <TableRow
                            key={screen.id}
                            className="cursor-pointer hover:bg-gray-50"
                            onClick={() => navigate(`/s`)}
                        >
                          <TableCell>{screen.name || "Не указано"}</TableCell>
                          <TableCell className="text-gray-600">
                            {screen.location || "Не указано"}
                          </TableCell>
                          <TableCell>{screen.resolution || "Не указано"}</TableCell>
                          <TableCell>{getStatusBadge(screen.status)}</TableCell>
                          <TableCell className="text-right">
                            <Button
                                variant="ghost"
                                size="sm"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  navigate(`/s`);
                                }}
                            >
                              View
                            </Button>
                          </TableCell>
                        </TableRow>
                    ))}
                  </TableBody>
                </Table>
            )}
          </div>

          {status !== "loading" && (
              <Pagination
                  offset={offset}
                  limit={limit}
                  total={total}
                  onNextPage={handleNextPage}
                  onPrevPage={handlePrevPage}
              />
          )}
        </Card>

        <CreateScreenForm
            isOpen={isDialogOpen}
            onOpenChange={handleDialogOpenChange}
            onSubmit={handleCreateScreen}
            isSubmitting={createStatus === "loading"}
            error={createError}
        />
      </div>
  );
}
