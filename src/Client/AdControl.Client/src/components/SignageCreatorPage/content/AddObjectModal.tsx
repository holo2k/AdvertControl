import {Dialog, DialogContent, DialogHeader, DialogTitle} from "../../ui/dialog.tsx";
import {useEffect, useState} from "react";
import {apiClient, MINIO_PUBLIC_URL} from "../../../api/apiClient.ts";
import {Search} from "lucide-react";
import {Input} from "../../ui/input.tsx";

interface AddObjectModalProps {
    dialogOpen: boolean;
    setDialogOpen: (open: boolean) => void;
    onFileSelect: (fileName: string) => void;
}

export const AddObjectModal = ({dialogOpen, setDialogOpen, onFileSelect} : AddObjectModalProps) => {
    const [files, setFiles] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [searchQuery, setSearchQuery] = useState("");

    useEffect(() => {
        const fetchFiles = async () => {
            try {
                const response = await apiClient.get("files/get-current-user-files-name");
                setFiles(response.data.filesName);
            } catch {
                setError("Не удалось загрузить файлы");
                setFiles([]);
            } finally {
                setLoading(false);
            }
        };

        fetchFiles();
    }, []);

    const filteredFiles = files.filter(fileName =>
        fileName.toLowerCase().includes(searchQuery.toLowerCase())
    );

    return (
        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
            <DialogContent className="max-w-4xl">
                <DialogHeader>
                    <DialogTitle>Ваши ранее загруженные фото и видео</DialogTitle>
                </DialogHeader>

                <div className="relative">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                    <Input
                        className="pl-10"
                        placeholder="Поиск..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                    />
                </div>

                {loading && (
                    <div className="flex justify-center items-center py-8">
                        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                    </div>
                )}

                {error && (
                    <div className="p-4 text-red-600 bg-red-50 rounded-lg">
                        {error}
                    </div>
                )}

                {!loading && !error && (
                    <div style={{maxHeight:"75vh", overflowY: "scroll"}}>
                        {filteredFiles.length === 0 ? (
                            <div className="text-center py-8 text-gray-500">
                                {searchQuery ? "Файлы по запросу не найдены" : "Нет загруженных файлов"}
                            </div>
                        ) : (
                            <div className="grid lg:grid-cols-2 grid-cols-3 gap-4 p-1">
                                {filteredFiles.map((fileName, index) => {
                                    if (fileName === "photo_2026-01-20_23-03-11_fdb5f84d_ca59a71e-8efc-410a-9ac0-a94c33df5641.jpg") {
                                        return null;
                                    }
                                    const fileUrl = `${MINIO_PUBLIC_URL}/${fileName}`
                                    return (
                                        <div
                                            key={index}
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                onFileSelect(fileName);
                                            }}
                                            className="flex flex-col rounded-lg border hover:shadow-md cursor-pointer transition-shadow duration-200 bg-white"
                                            style={{
                                                maxWidth: "400px",
                                                minWidth: "100px",
                                                padding: "5px",
                                            }}
                                        >
                                            <div className="relative w-full" style={{ aspectRatio: "1/1" }}>
                                                <img
                                                    src={fileUrl}
                                                    alt={fileName}
                                                    className="absolute inset-0 w-full h-full object-cover rounded-lg"
                                                />
                                            </div>
                                            <div className="p-2">
                                                <div className="flex items-center justify-between mb-1">
                                                    <p
                                                        className="text-sm text-gray-700 text-center truncate px-2"
                                                        title={fileName}
                                                        style={{
                                                            overflow: "hidden",
                                                            textOverflow: "ellipsis",
                                                            whiteSpace: "nowrap",
                                                            width: "100%",
                                                        }}
                                                    >
                                                        {fileName}
                                                    </p>
                                                </div>
                                            </div>
                                        </div>
                                    )
                                })}
                            </div>
                        )}
                    </div>
                )}
            </DialogContent>
        </Dialog>
    )
}
