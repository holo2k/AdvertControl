import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Label } from "./ui/label";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { Switch } from "./ui/switch";
import { Separator } from "./ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";

export function SettingsPage() {
  return (
      <div className="space-y-6 max-w-4xl" style={{margin: "auto"}}>
        <div>
          <h1>Настройки системы</h1>
          <p className="text-gray-600 mt-1">Настройка параметров системы и интеграций</p>
        </div>

        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle>Основные настройки</CardTitle>
            <CardDescription>Базовая конфигурация системы и предпочтения</CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="language">Язык</Label>
              <Select defaultValue="ru">
                <SelectTrigger id="language">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="ru">Русский</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </CardContent>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>Email-уведомления</Label>
                <p className="text-gray-500">Получать обновления по электронной почте</p>
              </div>
              <Switch disabled />
            </div>
            <Separator />
            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>Оповещения о подключении экранов</Label>
                <p className="text-gray-500">Оповещать при отключении экранов</p>
              </div>
              <Switch disabled />
            </div>
            <Separator />
            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>Уведомления об ошибках синхронизации</Label>
                <p className="text-gray-500">Оповещать о сбоях синхронизации контента</p>
              </div>
              <Switch disabled/>
            </div>
            <Separator />
            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>Еженедельные отчеты</Label>
                <p className="text-gray-500">Получать еженедельные сводные отчеты</p>
              </div>
              <Switch disabled />
            </div>
          </CardContent>
        </Card>

        <div className="flex justify-end gap-2">
          <Button disabled style={{ backgroundColor: "#2563EB" }}>Сохранить изменения</Button>
        </div>
      </div>
  );
}
