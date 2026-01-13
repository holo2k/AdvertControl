import { useEffect, useState } from "react";
import { apiClient, MINIO_PUBLIC_URL } from "../api/apiClient";
import ContentLoader from "react-content-loader";

export const FilePage = () => {
    const [files, setFiles] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [selectedFile, setSelectedFile] = useState<string | null>(null);

    useEffect(() => {
        const fetchFiles = async () => {
            try {
                const response = await apiClient.get("files/get-current-user-files-name");
                console.log(response);
                setFiles(response.data.filesName);
            } catch (e: any) {
                setError("Не удалось загрузить файлы");
                setFiles([]);
            } finally {
                setLoading(false);
            }
        };

        fetchFiles();
    }, []);

    const isVideo = (name: string) =>
        ["mp4", "webm", "ogg"].some(ext => name.toLowerCase().endsWith(ext));

    const isImage = (name: string) =>
        ["jpg", "jpeg", "png", "gif", "webp"].some(ext =>
            name.toLowerCase().endsWith(ext)
        );

    const handleFileClick = (fileName: string) => {
        if (isImage(fileName) || isVideo(fileName)) {
            setSelectedFile(fileName);
        }
    };

    const handleCloseModal = () => {
        setSelectedFile(null);
    };

    // Закрытие по ESC
    useEffect(() => {
        const handleEsc = (e: KeyboardEvent) => {
            if (e.key === 'Escape') handleCloseModal();
        };
        window.addEventListener('keydown', handleEsc);
        return () => window.removeEventListener('keydown', handleEsc);
    }, []);

    if (error) return <p className="text-red-500">{error}</p>;

    return (
        <>
            {/* Модальное окно для предпросмотра */}
            {selectedFile && (
                <div
                    className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
                    onClick={handleCloseModal}
                >
                    <div
                        className="relative max-w-full max-h-full"
                        onClick={(e) => e.stopPropagation()}
                    >
                        {/* Кнопка закрытия */}
                        <button
                            onClick={handleCloseModal}
                            className="absolute -top-12 right-0 text-white hover:text-gray-300 transition-colors z-10 rounded-full p-2"
                            aria-label="Закрыть"
                        >
                            <svg
                                className="w-6 h-6"
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M6 18L18 6M6 6l12 12"
                                />
                            </svg>
                        </button>

                        {/* Контент */}
                        {isImage(selectedFile) && (
                            <img
                                src={`${MINIO_PUBLIC_URL}/${selectedFile}`}
                                alt={selectedFile}
                                className="max-w-[90vw] max-h-[90vh] object-contain rounded-lg"
                                style={{
                                    maxWidth: "calc(100vw - 40px)",
                                    maxHeight: "calc(100vh - 40px)"
                                }}
                            />
                        )}

                        {isVideo(selectedFile) && (
                            <div className="relative">
                                <video
                                    src={`${MINIO_PUBLIC_URL}/${selectedFile}`}
                                    controls
                                    autoPlay
                                    className="max-w-[90vw] max-h-[90vh] rounded-lg"
                                    style={{
                                        maxWidth: "calc(100vw - 40px)",
                                        maxHeight: "calc(100vh - 40px)"
                                    }}
                                />
                            </div>
                        )}
                    </div>
                </div>
            )}

            {/* Основной контент */}
            <div className="space-y-2">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-2xl font-semibold">Файлы</h1>
                        <p className="text-gray-600 mt-1">
                            Ваши ранее загруженные фото и видео
                        </p>
                    </div>
                </div>

                <div className="flex justify-center">
                    <div
                        className="grid gap-3 p-2 sm:p-4"
                        style={{
                            gridTemplateColumns: "repeat(auto-fill, minmax(160px, 1fr))",
                            width: "100%",
                        }}
                    >
                        {/* LOADING */}
                        {loading &&
                            Array.from({ length: 8 }).map((_, i) => (
                                <div
                                    key={i}
                                    className="flex flex-col rounded-lg border p-1"
                                    style={{ maxWidth: "300px" }}
                                >
                                    <FileSkeleton />
                                </div>
                            ))}

                        {/* EMPTY */}
                        {!loading && files.length === 0 && (
                            <p className="text-gray-500 col-span-full text-center">
                                Файлы не найдены
                            </p>
                        )}

                        {/* FILES */}
                        {!loading &&
                            files.map(file => {
                                const url = `${MINIO_PUBLIC_URL}/${file}`;

                                return (
                                    <div
                                        key={file}
                                        className="flex flex-col rounded-lg border hover:shadow-md cursor-pointer transition-shadow duration-200 bg-white"
                                        style={{
                                            maxWidth: "280px",
                                            minWidth: "100px",
                                            padding: "5px",
                                        }}
                                        onClick={() => handleFileClick(file)}
                                    >
                                        <div className="relative w-full" style={{ aspectRatio: "1/1" }}>
                                            {isImage(file) && (
                                                <img
                                                    src={url}
                                                    alt={file}
                                                    className="absolute inset-0 w-full h-full object-cover rounded-lg"
                                                />
                                            )}

                                            {isVideo(file) && (
                                                <div className="absolute inset-0 flex items-center justify-center bg-black rounded-lg">
                                                    <video
                                                        src={url}
                                                        className="absolute inset-0 w-full h-full object-cover rounded-lg"
                                                    />
                                                    <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-20 rounded-lg">
                                                        <div className="w-8 h-8 rounded-full bg-white bg-opacity-80 flex items-center justify-center">
                                                            <span className="text-black">▶</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            )}

                                            {!isImage(file) && !isVideo(file) && (
                                                <div className="absolute inset-0 flex items-center justify-center bg-gray-100 rounded-lg">
                                                    <div className="text-center p-2">
                                                        <div className="text-2xl mb-1">📄</div>
                                                        <span className="text-blue-600 text-xs font-medium">
                                                            Файл
                                                        </span>
                                                    </div>
                                                </div>
                                            )}
                                        </div>

                                        <div className="pt-2 border-t">
                                            <p
                                                className="text-xs text-gray-700 truncate text-center"
                                                title={file}
                                            >
                                                {file}
                                            </p>
                                        </div>
                                    </div>
                                );
                            })}
                    </div>
                </div>
            </div>
        </>
    );
}

const FileSkeleton = () => (
    <div className="w-full">
        <div className="relative w-full" style={{ aspectRatio: "1 / 1" }}>
            <ContentLoader
                speed={2}
                viewBox="0 0 100 100"
                width="100%"
                height="100%"
                preserveAspectRatio="none"
                backgroundColor="#f3f3f3"
                foregroundColor="#ecebeb"
                className="absolute inset-0 rounded-lg"
            >
                <rect x="0" y="0" rx="12" ry="12" width="100" height="100" />
            </ContentLoader>
        </div>

        <div className="mt-2 px-1">
            <ContentLoader
                speed={2}
                viewBox="0 0 100 14"
                width="100%"
                height={14}
                preserveAspectRatio="none"
                backgroundColor="#f3f3f3"
                foregroundColor="#ecebeb"
            >
                <rect x="0" y="0" rx="6" ry="6" width="100" height="14" />
            </ContentLoader>
        </div>
    </div>
);
