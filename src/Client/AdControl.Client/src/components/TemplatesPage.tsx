import { useState } from "react";
import { Plus, Copy, Edit } from "lucide-react";
import { Button } from "./ui/button";
import { Card, CardFooter, CardHeader, CardTitle } from "./ui/card";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";

const templates = [
  {
    id: 1,
    name: "Витрина товаров",
    category: "Розница",
    dimensions: "1920x1080",
    color: "#3b82f6",
  },
  {
    id: 2,
    name: "Погода и новости", 
    category: "Информация",
    dimensions: "1920x1080",
    color: "#10b981",
  },
  {
    id: 3,
    name: "Анонс мероприятий",
    category: "События",
    dimensions: "1080x1920",
    color: "#8b5cf6",
  },
  {
    id: 4,
    name: "Корпоративное приветствие",
    category: "Корпоративный",
    dimensions: "1920x1080",
    color: "#f59e0b",
  },
  {
    id: 5,
    name: "Расписание транспорта",
    category: "Транспорт",
    dimensions: "1920x1080",
    color: "#06b6d4",
  },
  {
    id: 6,
    name: "Ресторанное меню",
    category: "Еда и напитки",
    dimensions: "1080x1920",
    color: "#ef4444",
  },
];

export function TemplatesPage() {
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1>Шаблоны</h1>
          <p className="text-gray-600 mt-1">Создавайте шаблоны отображения и управляйте ими</p>
        </div>
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button style={{ backgroundColor: "#2563EB" }} className="gap-2">
              <Plus className="h-4 w-4" />
              Создать шаблон
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Создать новый шаблон</DialogTitle>
              <DialogDescription>
                Разработайте новый шаблон контента для ваших рекламных экранов.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="template-name">Название</Label>
                <Input id="template-name" placeholder="e.g., Product Showcase" />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="category">Категория</Label>
                  <Select>
                    <SelectTrigger id="category">
                      <SelectValue placeholder="Select category" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="retail">Retail</SelectItem>
                      <SelectItem value="info">Information</SelectItem>
                      <SelectItem value="events">Events</SelectItem>
                      <SelectItem value="corporate">Corporate</SelectItem>
                      <SelectItem value="transport">Transportation</SelectItem>
                      <SelectItem value="food">Food & Beverage</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="dimensions">Разрешения</Label>
                  <Select>
                    <SelectTrigger id="dimensions">
                      <SelectValue placeholder="Select size" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1920x1080">1920x1080 (Landscape)</SelectItem>
                      <SelectItem value="1080x1920">1080x1920 (Portrait)</SelectItem>
                      <SelectItem value="3840x2160">3840x2160 (4K)</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsDialogOpen(false)}>
                Отменить
              </Button>
              <Button style={{ backgroundColor: "#2563EB" }} onClick={() => setIsDialogOpen(false)}>
                Создать шаблон
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {templates.map((template) => (
          <Card key={template.id} className="shadow-sm hover:shadow-md transition-shadow overflow-hidden">
            <div
              className="h-40 flex items-center justify-center"
              style={{ backgroundColor: `${template.color}15` }}
            >
              <div
                className="w-24 h-16 rounded-lg flex items-center justify-center"
                style={{ backgroundColor: template.color }}
              >
                <span className="text-white text-xs">Preview</span>
              </div>
            </div>
            <CardHeader>
              <div className="flex items-start justify-between">
                <CardTitle className="text-base font-semibold">{template.name}</CardTitle>
              </div>
              <p className="text-gray-500 mt-1">{template.dimensions}</p>
            </CardHeader>
            <CardFooter className="gap-2">
              <Button variant="outline" size="sm" className="flex-1 gap-2">
                <Edit className="h-4 w-4" />
                Редактировать
              </Button>
              <Button variant="outline" size="sm" className="flex-1 gap-2">
                <Copy className="h-4 w-4" />
                Копия
              </Button>
            </CardFooter>
          </Card>
        ))}
      </div>

      {templates.length === 0 && (
        <div className="flex flex-col items-center justify-center py-12">
          <div className="w-64 h-48 rounded-lg bg-gray-100 flex items-center justify-center mb-4">
            <p className="text-gray-400">Шаблонов пока нет</p>
          </div>
          <p className="text-gray-600 mb-4">Создайте свой первый шаблон контента, чтобы начать работу</p>
          <Button style={{ backgroundColor: "#2563EB" }} className="gap-2">
            <Plus className="h-4 w-4" />
            Создать шаблон
          </Button>
        </div>
      )}
    </div>
  );
}
