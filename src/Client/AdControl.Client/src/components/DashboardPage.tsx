import { useEffect, useState } from "react";
import { Monitor, CheckCircle, AlertCircle, Clock } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "./ui/table";
import {apiClient} from "../api/apiClient.ts";
import ContentLoader from "react-content-loader";
import {getStatusBadge} from "./ScreensPage/StatusBadge.tsx";

type DashboardResponse = {
  success: boolean;
  error: string;
  dashboard: {
    activeScreens: number;
    connectedScreens: number;
    errorScreens: number;
    waitingScreens: number;
    actions: {
      screen: string;
      action_: string;
      lastUpdate: string;
      status: boolean;
    }[];
    locations: {
      screenLocation: string;
      count: number;
    }[];
  };
};

export function DashboardPage() {
  const [loading, setLoading] = useState(true);
  const [dashboard, setDashboard] = useState<DashboardResponse["dashboard"] | null>(null);

  useEffect(() => {
    const loadDashboard = async () => {
      try {
        const { data } = await apiClient.get<DashboardResponse>(
            `/screen/dashboard`
        );
        if (data.success) {
          setDashboard(data.dashboard);
          console.log(data.dashboard);
        }
      } catch (e) {
        console.error("Ошибка загрузки dashboard", e);
      } finally {
        setLoading(false);
      }
    };

    loadDashboard();
  }, []);

    const kpiData = [
        {
            label: "Активные экраны",
            key: "activeScreens",
            icon: Monitor,
            color: "#2563EB",
        },
        {
            label: "Подключено",
            key: "connectedScreens",
            icon: CheckCircle,
            color: "#10b981",
        },
        {
            label: "Ошибки",
            key: "errorScreens",
            icon: AlertCircle,
            color: "#ef4444",
        },
        {
            label: "Ожидает обновления",
            key: "waitingScreens",
            icon: Clock,
            color: "#f59e0b",
        },
    ];


  const activityLog =
      dashboard?.actions.map((item, index) => ({
        id: index,
        screen: item.screen,
        action: item.action_,
        time: item.lastUpdate,
        status: item.status ? "SUCCESS" : "ERROR",
      })) ?? [];

  const screenLocations =
      dashboard?.locations.map((loc) => ({
        city: loc.screenLocation,
        count: loc.count,
        status: loc.count > 0 ? "operational" : "warning",
      })) ?? [];


  return (
      <div className="space-y-6">
        <div>
          <h1>Доска</h1>
          <p className="text-gray-600 mt-1">Обзор вашей сети экранов</p>
        </div>

        {/* KPI */}
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
              {kpiData.map((kpi) => {
                  const Icon = kpi.icon;

                  return (
                      <Card key={kpi.label} className="shadow-sm">
                          <CardContent className="pt-6">
                              <div className="flex items-start justify-between">
                                  <div>
                                      <p className="text-gray-600">{kpi.label}</p>

                                      <div className="mt-2" style={{ height: "36px" }}>
                                          {loading ? (
                                              <KpiValueLoader />
                                          ) : (
                                              <p style={{fontSize: "2rem", fontWeight: 600, lineHeight: 1,}}>
                                                  {dashboard?.[kpi.key]}
                                              </p>
                                          )}
                                      </div>
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
          {/* Активность */}
          <Card className="xl:col-span-2">
            <CardHeader style={{gap: 0}}>
              <CardTitle>Активность</CardTitle>
            </CardHeader>
            <CardContent>
              <div
                  className="overflow-y-auto"
                  style={{
                    maxHeight: "55vh",
                  }}
              >
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Экран</TableHead>
                      <TableHead>Описание</TableHead>
                      <TableHead>Время</TableHead>
                      <TableHead>Статус</TableHead>
                    </TableRow>
                  </TableHeader>
                    <TableBody>
                        {loading
                            ? [...Array(6)].map((_, i) => (
                                <TableRow key={i}>
                                    <TableCell colSpan={4}>
                                        <TableRowLoader />
                                    </TableCell>
                                </TableRow>
                            ))
                            : activityLog.map((log) => (
                                <TableRow key={log.id}>
                                    <TableCell>{log.screen}</TableCell>
                                    <TableCell className="text-gray-600">{log.action}</TableCell>
                                    <TableCell className="text-gray-500">{log.time}</TableCell>
                                    <TableCell>{getStatusBadge(log.status)}</TableCell>
                                </TableRow>
                            ))}
                    </TableBody>
                </Table>
              </div>
            </CardContent>
          </Card>

          {/* Локации */}
          <Card>
            <CardHeader>
              <CardTitle>Местоположение экранов</CardTitle>
            </CardHeader>
            <CardContent>
                <div className="space-y-4 overflow-y-auto" style={{ maxHeight: "55vh" }}>
                    {loading
                        ? [...Array(5)].map((_, i) => <LocationRowLoader key={i} />)
                        : screenLocations.map((location) => (
                            <div
                                key={location.city}
                                className="flex items-center justify-between"
                            >
                                <div>
                                    <p className="text-gray-900">{location.city}</p>
                                    <p className="text-gray-500">
                                        {location.count} screens
                                    </p>
                                </div>
                                <div
                                    className={`w-2 h-2 rounded-full ${
                                        location.status === "operational"
                                            ? "bg-green-500"
                                            : "bg-yellow-500"
                                    }`}
                                />
                            </div>
                        ))}
                </div>

            </CardContent>
          </Card>
        </div>
      </div>
  );
}

const KpiValueLoader = () => (
    <ContentLoader
        animate={true}
        speed={10}
        width={90}
        height={36}
        viewBox="0 0 90 36"
        backgroundColor="#f3f3f3"
        foregroundColor="#ecebeb"
    >
        <rect x="0" y="4" rx="6" ry="6" width="80" height="28" />
    </ContentLoader>
);

const TableRowLoader = () => (
    <ContentLoader
        animate={true}
        speed={10}
        width="100%"
        height={32}
        viewBox="0 0 600 32"
        backgroundColor="#f3f3f3"
        foregroundColor="#ecebeb"
        preserveAspectRatio="none"
    >
        <rect x="0" y="6" rx="4" ry="4" width="120" height="18" />
        <rect x="140" y="6" rx="4" ry="4" width="220" height="18" />
        <rect x="380" y="6" rx="4" ry="4" width="120" height="18" />
        <rect x="520" y="6" rx="4" ry="4" width="60" height="18" />
    </ContentLoader>
);

const LocationRowLoader = () => (
    <ContentLoader
        animate={true}
        speed={10}
        width="100%"
        height={32}
        viewBox="0 0 300 32"
        backgroundColor="#f3f3f3"
        foregroundColor="#ecebeb"
        preserveAspectRatio="none"
    >
        <rect x="0" y="6" rx="4" ry="4" width="120" height="18" />
        <rect x="0" y="30" rx="4" ry="4" width="80" height="1" />
        <circle cx="290" cy="16" r="4" />
    </ContentLoader>
);
