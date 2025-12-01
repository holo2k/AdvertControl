import { Button } from "../../ui/button";
import { Save, Upload } from "lucide-react";

interface Props {
    onSave: () => void;
    onPublish: () => void;
}

export function ActionButtons({ onSave, onPublish }: Props) {
    return (
        <div className="p-4 space-y-2 bg-white">
            <Button variant="outline" className="w-full gap-2" onClick={onSave}>
                <Save className="w-4 h-4" />
                Сохранить черновик
            </Button>
            <Button
                className="w-full gap-2"
                style={{ backgroundColor: "#2563EB" }}
                onClick={onPublish}
            >
                <Upload className="w-4 h-4" />
                Загрузить в экран
            </Button>
        </div>
    );
}
