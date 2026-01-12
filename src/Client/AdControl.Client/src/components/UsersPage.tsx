import { useState } from "react";
import { Plus, Search } from "lucide-react";
import { Button } from "./ui/button";
import { Card } from "./ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "./ui/table";
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

const users = [
    { id: 1, name: "Сара Джонсон", email: "sarah.j@company.com", role: "Администратор", status: "active", initials: "СД" },
    { id: 2, name: "Майкл Чен", email: "michael.c@company.com", role: "Оператор", status: "active", initials: "МЧ" },
    { id: 3, name: "Эмили Робертс", email: "emily.r@company.com", role: "Администратор", status: "active", initials: "ЭР" },
    { id: 4, name: "Дэвид Мартинес", email: "david.m@company.com", role: "Техник", status: "active", initials: "ДМ" },
    { id: 5, name: "Лиза Андерсон", email: "lisa.a@company.com", role: "Оператор", status: "inactive", initials: "ЛА" },
    { id: 6, name: "Джеймс Уилсон", email: "james.w@company.com", role: "Техник", status: "active", initials: "ДУ" },
];

export function UsersPage() {
    const [searchTerm, setSearchTerm] = useState("");
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const filteredUsers = users.filter((user) =>
        user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const getRoleBadge = (role: string) => {
        const styles = {
            "Администратор": "bg-purple-100 text-purple-800 hover:bg-purple-200",
            "Оператор": "bg-blue-100 text-blue-800 hover:bg-blue-200",
            "Техник": "bg-green-100 text-green-800 hover:bg-green-200",
        };
        return <Badge className={styles[role as keyof typeof styles]}>{role}</Badge>;
    };

    const getStatusBadge = (status: string) => {
        return status === "active" ? (
            <Badge className="bg-green-100 text-green-800 hover:bg-green-200">Активен</Badge>
        ) : (
            <Badge variant="outline" className="text-gray-600">Неактивен</Badge>
        );
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1>Пользователи</h1>
                    <p className="text-gray-600 mt-1">Управление доступом пользователей и правами</p>
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
                            <DialogTitle>Добавить нового пользователя</DialogTitle>
                            <DialogDescription>
                                Создайте новую учетную запись пользователя с соответствующим уровнем доступа.
                            </DialogDescription>
                        </DialogHeader>
                        <div className="space-y-4 py-4">
                            <div className="space-y-2">
                                <Label htmlFor="user-name">Полное имя</Label>
                                <Input id="user-name" placeholder="Например, Иван Иванов" />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="email">Email адрес</Label>
                                <Input id="email" type="email" placeholder="ivan.ivanov@company.com" />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="role">Роль</Label>
                                <Select>
                                    <SelectTrigger id="role">
                                        <SelectValue placeholder="Выберите роль" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="admin">Администратор - Полный доступ</SelectItem>
                                        <SelectItem value="operator">Оператор - Управление контентом</SelectItem>
                                        <SelectItem value="technician">Техник - Обслуживание экранов</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>
                        <DialogFooter>
                            <Button variant="outline" onClick={() => setIsDialogOpen(false)}>
                                Отмена
                            </Button>
                            <Button style={{ backgroundColor: "#2563EB" }} onClick={() => setIsDialogOpen(false)}>
                                Добавить пользователя
                            </Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </div>

            <Card className="shadow-sm">
                <div className="p-4 border-b border-gray-200">
                    <div className="relative">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                        <Input
                            placeholder="Поиск по имени или email..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="pl-10"
                        />
                    </div>
                </div>

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
                        {filteredUsers.map((user) => (
                            <TableRow key={user.id}>
                                <TableCell>
                                    <div className="flex items-center gap-3">
                                        <Avatar className="h-8 w-8">
                                            <AvatarFallback className="bg-blue-100 text-blue-700">
                                                {user.initials}
                                            </AvatarFallback>
                                        </Avatar>
                                        <span>{user.name}</span>
                                    </div>
                                </TableCell>
                                <TableCell className="text-gray-600">{user.email}</TableCell>
                                <TableCell>{getRoleBadge(user.role)}</TableCell>
                                <TableCell>{getStatusBadge(user.status)}</TableCell>
                                <TableCell className="text-right">
                                    <Button variant="ghost" size="sm">
                                        Редактировать
                                    </Button>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </Card>
        </div>
    );
}
