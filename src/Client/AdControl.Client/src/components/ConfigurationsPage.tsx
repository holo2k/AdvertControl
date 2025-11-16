import { useState } from "react";
import { Plus, Calendar, User } from "lucide-react";
import { Button } from "./ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Badge } from "./ui/badge";
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
import { Input } from "./ui/input";
import { Textarea } from "./ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { getScreensWord } from "../utils";

const configurations = [
  {
    id: 1,
    name: "Стандартная розничная конфигурация",
    author: "Сара Джонсон",
    created: "10 окт. 2025",
    updateFrequency: "Каждые 30 минут",
    contentType: "Смешанный медиа-контент",
    screens: 45,
  },
  {
    id: 2,
    name: "Дисплей транспортного узла",
    author: "Майкл Чен",
    created: "8 окт. 2025",
    updateFrequency: "Каждые 15 минут",
    contentType: "Видео + Погода",
    screens: 28,
  },
  {
    id: 3,
    name: "Система информирования офиса",
    author: "Эмили Робертс",
    created: "5 окт. 2025",
    updateFrequency: "Каждые 2 часа",
    contentType: "Статика + Объявления",
    screens: 67,
  },
  {
    id: 4,
    name: "Конфигурация для мероприятий",
    author: "Давид Мартинес",
    created: "28 сен. 2025",
    updateFrequency: "Вручную",
    contentType: "На основе событий",
    screens: 12,
  },
];

export function ConfigurationsPage() {
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1>Конфигурации</h1>
          <p className="text-gray-600 mt-1">Управление конфигурациями дисплеев и их планированием</p>
        </div>
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button style={{ backgroundColor: "#2563EB" }} className="gap-2">
              <Plus className="h-4 w-4" />
              Создать конфигурацию
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Создать новую конфигурацию</DialogTitle>
              <DialogDescription>
                Настройте новую конфигурацию отображения для ваших экранов.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="config-name">Название</Label>
                <Input id="config-name" placeholder="" />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="update-freq">Частота обновления</Label>
                  <Select>
                    <SelectTrigger id="update-freq">
                      <SelectValue placeholder="Выберите частоту" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="15min">Каждые 15 минут</SelectItem>
                      <SelectItem value="30min">Каждые 30 минут</SelectItem>
                      <SelectItem value="1hour">Каждый час</SelectItem>
                      <SelectItem value="2hour">Каждые  часа</SelectItem>
                      <SelectItem value="manual">Вручную</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="content-type">Тип контента</Label>
                  <Select>
                    <SelectTrigger id="content-type">
                      <SelectValue placeholder="Выберите тип" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="mixed">Смешанное медиа</SelectItem>
                      <SelectItem value="video">Только видео</SelectItem>
                      <SelectItem value="static">Статичное изображение</SelectItem>
                      <SelectItem value="dynamic">Динамический контент</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="description">Описание</Label>
                <Textarea
                  id="description"
                  placeholder="Опишите назначение и настройки этой конфигурации..."
                  rows={3}
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsDialogOpen(false)}>
                Отмена
              </Button>
              <Button style={{ backgroundColor: "#2563EB" }} onClick={() => setIsDialogOpen(false)}>
                Создать конфигурацию
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {configurations.map((config) => (
          <Card key={config.id} className="shadow-sm hover:shadow-md transition-shadow">
            <CardHeader>
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <CardTitle className="mb-2">{config.name}</CardTitle>
                  <CardDescription>
                    <div className="flex items-center gap-1 text-gray-600">
                      <User className="h-3 w-3" />
                      <span>{config.author}</span>
                    </div>
                  </CardDescription>
                </div>
                <Badge className="bg-blue-100 text-blue-800 hover:bg-blue-200">
                  {getScreensWord(config.screens)}
                </Badge>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Частота обновления</span>
                  <span className="text-gray-900">{config.updateFrequency}</span>
                </div>
                <div className="flex items-center justify-between text-sm">
                  <span className="text-gray-600">Тип контента</span>
                  <span className="text-gray-900">{config.contentType}</span>
                </div>
                <div className="flex items-center gap-1 text-sm text-gray-500">
                  <Calendar className="h-3 w-3" />
                  <span>Создано {config.created}</span>
                </div>
              </div>
              <div className="mt-4 pt-4 border-t border-gray-200 flex gap-2">
                <Button variant="outline" size="sm" className="flex-1">
                  Редактировать
                </Button>
                <Button variant="outline" size="sm" className="flex-1">
                  Дублировать
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
