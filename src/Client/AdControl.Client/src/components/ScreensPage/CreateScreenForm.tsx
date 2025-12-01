import { useState, useEffect } from "react";
import { Button } from "../ui/button.tsx";
import { Input } from "../ui/input.tsx";
import { Label } from "../ui/label.tsx";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "../ui/dialog.tsx";

interface CreateScreenFormProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
    onSubmit: (screenData: {
        code: number;
        name: string;
        resolution: string;
        location: string;
    }) => void;
    isSubmitting?: boolean;
    error?: string | null;
}

export function CreateScreenForm({
                                     isOpen,
                                     onOpenChange,
                                     onSubmit,
                                     isSubmitting = false,
                                     error = null
                                 }: CreateScreenFormProps) {
    const [formData, setFormData] = useState({
        code: null,
        name: "",
        resolution: "",
        location: "",
    });
    const [formErrors, setFormErrors] = useState({
        code: null,
        name: "",
        resolution: "",
        location: "",
    });

    // Сбрасываем форму при открытии/закрытии диалога
    useEffect(() => {
        if (isOpen) {
            setFormData({ name: "", resolution: "", location: "", code: null });
            setFormErrors({ name: "", resolution: "", location: "", code: null });
        }
    }, [isOpen]);

    // Валидация формы
    const validateForm = () => {
        const errors = {
            code: "",
            name: "",
            resolution: "",
            location: "",
        };

        if (!formData.code) {
            errors.code = "Код привязки обязателен";
        }

        if (!formData.name.trim()) {
            errors.name = "Название обязательно";
        }

        if (!formData.resolution.trim()) {
            errors.resolution = "Разрешение обязательно";
        }

        if (!formData.location.trim()) {
            errors.location = "Расположение обязательно";
        }

        setFormErrors(errors);
        return !errors.name && !errors.resolution && !errors.location && !errors.code;
    };

    const handleSubmit = () => {
        if (validateForm()) {
            onSubmit(formData);
        }
    };

    const handleChange = (field: string, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        // Очищаем ошибку при вводе
        if (formErrors[field as keyof typeof formErrors]) {
            setFormErrors(prev => ({ ...prev, [field]: "" }));
        }
    };

    const handleClose = () => {
        onOpenChange(false);
    };

    return (
        <Dialog open={isOpen} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Добавить новый экран</DialogTitle>
                    <DialogDescription>
                        Создайте новый рекламный экран в своей сети.
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-4 py-4">
                    <div className="space-y-2">
                        <Label htmlFor="screen-name">Код привязки *</Label>
                        <Input
                            id="screen-name"
                            placeholder="Введите код привязки"
                            value={formData.code}
                            onChange={(e) => handleChange("name", e.target.value)}
                            className={formErrors.code ? "border-red-500" : ""}
                            disabled={isSubmitting}
                        />
                        {formErrors.code && (
                            <p className="text-sm text-red-600">{formErrors.code}</p>
                        )}
                    </div>
                    <div className="space-y-2">
                        <Label htmlFor="screen-name">Название *</Label>
                        <Input
                            id="screen-name"
                            placeholder="Введите название экрана"
                            value={formData.name}
                            onChange={(e) => handleChange("name", e.target.value)}
                            className={formErrors.name ? "border-red-500" : ""}
                            disabled={isSubmitting}
                        />
                        {formErrors.name && (
                            <p className="text-sm text-red-600">{formErrors.name}</p>
                        )}
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="screen-resolution">Разрешение *</Label>
                        <Input
                            id="screen-resolution"
                            placeholder="Например: 1920x1080"
                            value={formData.resolution}
                            onChange={(e) => handleChange("resolution", e.target.value)}
                            className={formErrors.resolution ? "border-red-500" : ""}
                            disabled={isSubmitting}
                        />
                        {formErrors.resolution && (
                            <p className="text-sm text-red-600">{formErrors.resolution}</p>
                        )}
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="location">Расположение *</Label>
                        <Input
                            id="location"
                            placeholder="Введите расположение экрана"
                            value={formData.location}
                            onChange={(e) => handleChange("location", e.target.value)}
                            className={formErrors.location ? "border-red-500" : ""}
                            disabled={isSubmitting}
                        />
                        {formErrors.location && (
                            <p className="text-sm text-red-600">{formErrors.location}</p>
                        )}
                    </div>

                    {/* Отображение ошибки сервера */}
                    {error && (
                        <div className="p-3 bg-red-50 border border-red-200 rounded-md">
                            <p className="text-sm text-red-800">{error}</p>
                        </div>
                    )}
                </div>

                <DialogFooter>
                    <Button
                        variant="outline"
                        onClick={handleClose}
                        disabled={isSubmitting}
                    >
                        Отменить
                    </Button>
                    <Button
                        style={{ backgroundColor: "#2563EB" }}
                        onClick={handleSubmit}
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? "Создание..." : "Добавить экран"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
