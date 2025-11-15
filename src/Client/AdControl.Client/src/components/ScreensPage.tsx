import { useState, useEffect } from "react";
import { Plus, Search, Filter } from "lucide-react";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Card } from "./ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "./ui/table";
import { Badge } from "./ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "./ui/dialog";
import { Label } from "./ui/label";

import { useDispatch, useSelector } from "react-redux";
import type { RootState, AppDispatch } from "../store/store";
import { fetchScreens, setPagination } from "../store/screenSlice"; // ✅ исправлено имя

export function ScreensPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const dispatch = useDispatch<AppDispatch>();
  const { items, total, limit, offset, status } = useSelector(
      (state: RootState) => state.screens
  );

  useEffect(() => {
    dispatch(fetchScreens({ limit, offset }));
  }, [dispatch, limit, offset]);

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

  // 🔹 Фильтрация по имени, локации и статусу
  const filteredScreens = items.filter((screen) => {
    const matchesSearch =
        screen.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        screen.location.toLowerCase().includes(searchTerm.toLowerCase());
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

  return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1>Экраны</h1>
            <p className="text-gray-600 mt-1">Управляйте своей сетью экранов</p>
          </div>

          {/* Диалог добавления экрана */}
          <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
            <DialogTrigger asChild>
              <Button style={{ backgroundColor: "#2563EB" }} className="gap-2">
                <Plus className="h-4 w-4" />
                Добавить экран
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Добавить новый экран</DialogTitle>
                <DialogDescription>
                  Создайте новый рекламный экран в своей сети.
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4 py-4">
                <div className="space-y-2">
                  <Label htmlFor="screen-name">Название</Label>
                  <Input id="screen-name" placeholder="" />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="screen-id">Разрешение</Label>
                  <Input id="screen-id" placeholder="" />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="location">Расположение</Label>
                  <Input id="location" placeholder="" />
                </div>
              </div>
              <DialogFooter>
                <Button variant="outline" onClick={() => setIsDialogOpen(false)}>
                  Отменить
                </Button>
                <Button
                    style={{ backgroundColor: "#2563EB" }}
                    onClick={() => setIsDialogOpen(false)}
                >
                  Добавить экран
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
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
                <p className="p-4 text-gray-500">Загрузка экранов...</p>
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
                        <TableRow key={screen.id}>
                          <TableCell>{screen.name}</TableCell>
                          <TableCell className="text-gray-600">
                            {screen.location}
                          </TableCell>
                          <TableCell>{screen.resolution}</TableCell>
                          <TableCell>{getStatusBadge(screen.status)}</TableCell>
                          <TableCell className="text-right">
                            <Button variant="ghost" size="sm">
                              View
                            </Button>
                          </TableCell>
                        </TableRow>
                    ))}
                  </TableBody>
                </Table>
            )}
          </div>

          {/* Пагинация */}
          <div className="flex justify-between items-center p-2 border-t border-gray-200">
            <Button
                variant="outline"
                onClick={handlePrevPage}
                disabled={offset === 0}
            >
              Назад
            </Button>
            <p className="text-gray-600">
              Страница {Math.floor(offset / limit) + 1} из{" "}
              {Math.ceil(total / limit) || 1}
            </p>
            <Button
                variant="outline"
                onClick={handleNextPage}
                disabled={offset + limit >= total}
            >
              Вперёд
            </Button>
          </div>
        </Card>
      </div>
  );
}
