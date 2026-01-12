import { type LucideIcon, Download } from 'lucide-react';

interface DownloadCardProps {
  icon: LucideIcon;
  platform: string;
  fileType: string;
  fileSize: string;
  index: number;
}

export function DownloadCard({ icon: Icon, platform, fileType, fileSize }: DownloadCardProps) {
  return (
    <div className="flex flex-col items-center text-center">
        <div className="w-20 h-20 bg-gradient-to-br from-[#2563EB] to-[#1d4ed8] rounded-2xl flex items-center justify-center mb-4 shadow-lg">
          <Icon className="w-10 h-10 text-white" />
        </div>
        <h3 className="text-lg mb-2">{platform}</h3>
        <p className="text-sm text-gray-500 mb-4">{fileSize}</p>
        <button className=" bg-[#2563EB] text-white py-3 px-4 rounded-lg hover:bg-[#1d4ed8] transition-colors duration-200 flex items-center justify-center gap-2 group">
          <Download className="w-4 h-4 group-hover:animate-bounce" />
          <span>Скачать {fileType}</span>
        </button>
      </div>
  );
}
