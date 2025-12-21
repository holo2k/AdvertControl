import { Button } from "../ui/button.tsx";

interface PaginationProps {
    offset: number;
    limit: number;
    total: number;
    onNextPage: () => void;
    onPrevPage: () => void;
}

export function Pagination({ offset, limit, total, onNextPage, onPrevPage }: PaginationProps) {
    return (
        <div className="flex justify-between items-center p-2 border-t border-gray-200">
            <Button
                variant="outline"
                onClick={onPrevPage}
                disabled={offset === 0}
            >
                Назад
            </Button>
            <p className="text-gray-600">
                Страница {Math.floor(offset / limit) + 1} из{" "}
                {Math.ceil(total / limit) || 1}
            </p>
            <Button
                variant="outline"
                onClick={onNextPage}
            >
                Вперёд
            </Button>
        </div>
    );
}
