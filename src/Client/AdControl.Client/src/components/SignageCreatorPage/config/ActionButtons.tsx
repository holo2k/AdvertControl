import { Button } from "../../ui/button";
import { Upload } from "lucide-react";

interface Props {
    onPublish: () => void;
}

export function ActionButtons({ onPublish }: Props) {
    return (
        <div className="p-4 space-y-2 bg-white">
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
