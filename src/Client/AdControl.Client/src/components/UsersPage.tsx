import { useState, useEffect } from "react";
import { Plus, Search, RefreshCw } from "lucide-react";
import ContentLoader from "react-content-loader";

import { Button } from "./ui/button";
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
import { Avatar, AvatarFallback } from "./ui/avatar";
import { Input } from "./ui/input";
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
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "./ui/select";

import { apiClient } from "../api/apiClient";
import { generateRandomPassword } from "../utils";

/* ===================== TYPES ===================== */

type ApiUser = {
    id: string;
    username: string;
    roles: string[];
    email: string;
    emailVerified: boolean;
    firstName: string;
    lastName: string;
    phoneNumber: string;
    enabled: boolean;
};

type FormState = {
    username: string;
    password: string;
    repeatPassword: string;
    role: "admin" | "user" | "";
};

const initialFormState: FormState = {
    username: "",
    password: "",
    repeatPassword: "",
    role: "",
};

/* ===================== LOADER ===================== */

const UsersTableLoader = () => (
    <ContentLoader
        speed={2}
        width="100%"
        height={240}
        backgroundColor="#f3f3f3"
        foregroundColor="#ecebeb"
    >
        <rect x="0" y="10" rx="6" ry="6" width="100%" height="40" />
        <rect x="0" y="60" rx="6" ry="6" width="100%" height="40" />
        <rect x="0" y="110" rx="6" ry="6" width="100%" height="40" />
        <rect x="0" y="160" rx="6" ry="6" width="100%" height="40" />
    </ContentLoader>
);

/* ===================== COMPONENT ===================== */

export function UsersPage() {
    const [users, setUsers] = useState<ApiUser[]>([]);
    const [searchTerm, setSearchTerm] = useState("");

    const [isLoadingUsers, setIsLoadingUsers] = useState(false);
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [showPassword, setShowPassword] = useState(false);

    const [form, setForm] = useState<FormState>(initialFormState);

    /* ===================== EFFECTS ===================== */

    useEffect(() => {
        fetchUsers();
    }, []);

    useEffect(() => {
        if (!isDialogOpen) {
            resetForm();
        }
    }, [isDialogOpen]);

    /* ===================== API ===================== */

    const fetchUsers = async () => {
        try {
            setIsLoadingUsers(true);
            const { data } = await apiClient.get<ApiUser[]>("/get-users");
            setUsers(data);
        } catch (e) {
            console.error(e);
        } finally {
            setIsLoadingUsers(false);
        }
    };

    const handleCreateUser = async () => {
        try {
            setIsSubmitting(true);

            await apiClient.post("/users", {
                username: form.username,
                password: form.password,
                repeatPassword: form.repeatPassword,
                roles: [form.role],
            });

            await fetchUsers();
            setIsDialogOpen(false);
            resetForm();
        } catch (e) {
            console.error(e);
        } finally {
            setIsSubmitting(false);
        }
    };

    /* ===================== HELPERS ===================== */

    const updateForm = (key: keyof FormState, value: string) => {
        setForm((prev) => ({ ...prev, [key]: value }));
    };

    const resetForm = () => {
        setForm(initialFormState);
        setShowPassword(false);
    };

    const generatePassword = () => {
        const password = generateRandomPassword(10);
        setForm((prev) => ({
            ...prev,
            password,
            repeatPassword: password,
        }));
    };

    const copyToClipboard = (text: string) => {
        navigator.clipboard.writeText(text);
    };

    const filteredUsers = users.filter(
        (u) =>
            u.username.toLowerCase().includes(searchTerm.toLowerCase()) ||
            (u.email ?? "").toLowerCase().includes(searchTerm.toLowerCase())
    );

    const getRoleBadge = (roles: string[]) =>
        roles.includes("admin") ? (
            <Badge className="bg-purple-100 text-purple-800">Администратор</Badge>
        ) : (
            <Badge className="bg-blue-100 text-blue-800">Пользователь</Badge>
        );

    const getStatusBadge = (enabled: boolean) =>
        enabled ? (
            <Badge className="bg-green-100 text-green-800">Активен</Badge>
        ) : (
            <Badge variant="outline" className="text-gray-600">
                Отключён
            </Badge>
        );

    /* ===================== RENDER ===================== */

    return (
        <div className="space-y-6">
            {/* HEADER */}
            <div className="flex items-center justify-between">
                <div>
                    <h1>Пользователи</h1>
                    <p className="text-gray-600 mt-1">
                        Управление доступом пользователей
                    </p>
                </div>

                <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                    <DialogTrigger asChild>
                        <Button style={{ backgroundColor: "#2563EB" }} className="gap-2">
                            <Plus className="h-4 w-4" />
                            Добавить пользователя
                        </Button>
                    </DialogTrigger>

                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Новый пользователь</DialogTitle>
                            <DialogDescription>
                                Создание учетной записи
                            </DialogDescription>
                        </DialogHeader>

                        <div className="space-y-4 py-4">
                            <div className="space-y-2">
                                <Label>Логин</Label>
                                <Input
                                    value={form.username}
                                    onChange={(e) =>
                                        updateForm("username", e.target.value)
                                    }
                                />
                            </div>

                            <div className="space-y-2">
                                <Label>Пароль</Label>
                                <div className="flex gap-2">
                                    <Input
                                        type={showPassword ? "text" : "password"}
                                        value={form.password}
                                        onChange={(e) =>
                                            updateForm("password", e.target.value)
                                        }
                                    />
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        onClick={generatePassword}
                                    >
                                        <RefreshCw className="h-3 w-3" />
                                    </Button>
                                </div>
                            </div>

                            <div className="space-y-2">
                                <Label>Повторите пароль</Label>
                                <div className="flex gap-2">
                                    <Input
                                        type={showPassword ? "text" : "password"}
                                        value={form.repeatPassword}
                                        onChange={(e) =>
                                            updateForm("repeatPassword", e.target.value)
                                        }
                                    />
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        onClick={() => copyToClipboard(form.password)}
                                    >
                                        Копировать
                                    </Button>
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        onClick={() => setShowPassword(!showPassword)}
                                    >
                                        {showPassword ? "Скрыть" : "Показать"}
                                    </Button>
                                </div>
                            </div>

                            <div className="space-y-2">
                                <Label>Роль</Label>
                                <Select
                                    value={form.role}
                                    onValueChange={(v) =>
                                        updateForm("role", v)
                                    }
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Выберите роль" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="admin">
                                            Администратор
                                        </SelectItem>
                                        <SelectItem value="user">
                                            Пользователь
                                        </SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>

                        <DialogFooter>
                            <Button
                                variant="outline"
                                onClick={() => setIsDialogOpen(false)}
                            >
                                Отмена
                            </Button>
                            <Button
                                style={{ backgroundColor: "#2563EB" }}
                                onClick={handleCreateUser}
                                disabled={isSubmitting}
                            >
                                {isSubmitting ? "Создание..." : "Создать"}
                            </Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </div>

            {/* TABLE */}
            <Card className="shadow-sm">
                <div className="p-4 border-b border-gray-200">
                    <div className="relative">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                        <Input
                            className="pl-10"
                            placeholder="Поиск..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                </div>

                {isLoadingUsers ? (
                    <div className="p-4">
                        <UsersTableLoader />
                    </div>
                ) : filteredUsers.length === 0 ? (
                    <p className="p-4 mb-4 text-gray-500 m-auto">Нет пользователей</p>
                ) : (
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Пользователь</TableHead>
                                <TableHead>Email</TableHead>
                                <TableHead>Роль</TableHead>
                                <TableHead>Статус</TableHead>
                                <TableHead className="text-right">Действия</TableHead>
                            </TableRow>
                        </TableHeader>

                        <TableBody>
                            {filteredUsers.map((u) => {
                                const initials =
                                    `${u.firstName?.[0] ?? ""}${u.lastName?.[0] ?? ""}` ||
                                    u.username.slice(0, 2).toUpperCase();

                                return (
                                    <TableRow key={u.id}>
                                        <TableCell>
                                            <div className="flex items-center gap-3">
                                                <Avatar className="h-8 w-8">
                                                    <AvatarFallback className="bg-blue-100 text-blue-700">
                                                        {initials}
                                                    </AvatarFallback>
                                                </Avatar>
                                                {u.username}
                                            </div>
                                        </TableCell>
                                        <TableCell>{u.email || "—"}</TableCell>
                                        <TableCell>{getRoleBadge(u.roles)}</TableCell>
                                        <TableCell>
                                            {getStatusBadge(u.enabled)}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <Button variant="ghost" size="sm">
                                                Редактировать
                                            </Button>
                                        </TableCell>
                                    </TableRow>
                                );
                            })}
                        </TableBody>
                    </Table>
                )}
            </Card>
        </div>
    );
}
