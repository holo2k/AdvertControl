import {Badge} from "../ui/badge.tsx";

export const getStatusBadge = (status: string | undefined) => {
    const statusMap: Record<string, { label: string; style: string }> = {
        'PAIRED': {
            label: 'Подключено',
            style: 'bg-green-100 text-green-800 hover:bg-green-200'
        },
        'NOT_PAIRED': {
            label: 'Не подключено',
            style: 'bg-yellow-100 text-yellow-800 hover:bg-yellow-200'
        },
        'SUCCESS': {
            label: 'Успешно',
            style: 'bg-green-100 text-green-800 hover:bg-green-200'
        },
        'ERROR': {
            label: 'Ошибка',
            style: 'bg-red-100 text-red-800 hover:bg-red-200'
        },
    };

    const statusData = status
        ? statusMap[status.toUpperCase()]
        : null;

    return (
        <Badge
            className={`
                ${statusData?.style || 'bg-gray-100 text-gray-800 hover:bg-gray-200'}
                font-medium text-xs px-2 py-0.5
            `}
        >
            {statusData?.label || status || 'Неизвестно'}
        </Badge>
    );
};
