import { Monitor, CheckCircle, AlertCircle, Clock } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "./ui/table";
import { Badge } from "./ui/badge";

const kpiData = [
  { label: "Активные экраны", value: "0", icon: Monitor, color: "#2563EB" },
  { label: "Подключено", value: "0", icon: CheckCircle, color: "#10b981" },
  { label: "Ошибки", value: "0", icon: AlertCircle, color: "#ef4444" },
  { label: "Ожидает обновления", value: "0", icon: Clock, color: "#f59e0b" },
];

//пока формат api неизвестен
const activityLog: any[] = [];
const screenLocations: any[] = [];

export function DashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1>Доска</h1>
        <p className="text-gray-600 mt-1">Обзор вашей сети экранов</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
        {kpiData.map((kpi) => {
          const Icon = kpi.icon;
          return (
            <Card key={kpi.label} className="shadow-sm">
              <CardContent className="pt-6">
                <div className="flex items-start justify-between">
                  <div>
                    <p className="text-gray-600">{kpi.label}</p>
                    <p className="mt-2" style={{ fontSize: "2rem", fontWeight: 600, lineHeight: 1 }}>
                      {kpi.value}
                    </p>
                  </div>
                  <div
                    className="rounded-lg p-3"
                    style={{ backgroundColor: `${kpi.color}15` }}
                  >
                    <Icon className="h-6 w-6" style={{ color: kpi.color }} />
                  </div>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        <Card className="xl:col-span-2 shadow-sm">
          <CardHeader>
            <CardTitle>Недавняя активность</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Экран</TableHead>
                  <TableHead>Действие</TableHead>
                  <TableHead>Время</TableHead>
                  <TableHead>Статус</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {activityLog.map((log) => (
                  <TableRow key={log.id}>
                    <TableCell>{log.screen}</TableCell>
                    <TableCell className="text-gray-600">{log.action}</TableCell>
                    <TableCell className="text-gray-500">{log.time}</TableCell>
                    <TableCell>
                      <Badge
                        variant={log.status === "success" ? "default" : "destructive"}
                        className={log.status === "success" ? "bg-green-100 text-green-800 hover:bg-green-200" : ""}
                      >
                        {log.status}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle>Местоположение экранов</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {screenLocations.map((location) => (
                <div key={location.city} className="flex items-center justify-between">
                  <div>
                    <p className="text-gray-900">{location.city}</p>
                    <p className="text-gray-500">{location.count} screens</p>
                  </div>
                  <div
                    className={`w-2 h-2 rounded-full ${
                      location.status === "operational" ? "bg-green-500" : "bg-yellow-500"
                    }`}
                  />
                </div>
              ))}
            </div>
            <div className="mt-6 p-4 bg-gray-50 rounded-lg border-2 border-dashed border-gray-200 flex items-center justify-center h-32">
              <p className="text-gray-500">Map widget placeholder</p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
