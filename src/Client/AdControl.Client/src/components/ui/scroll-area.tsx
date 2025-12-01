import { cn } from "./utils";

interface ScrollAreaProps extends React.HTMLAttributes<HTMLDivElement> {
    children: React.ReactNode;
}

function ScrollArea({ className, children, ...props }: ScrollAreaProps) {
    return (
        <div
            className={cn(
                "relative overflow-auto",
                "scrollbar-thin scrollbar-thumb-gray-300 scrollbar-track-gray-100",
                "[&::-webkit-scrollbar]:w-2.5",
                "[&::-webkit-scrollbar-track]:bg-gray-100",
                "[&::-webkit-scrollbar-thumb]:bg-gray-300",
                "[&::-webkit-scrollbar-thumb]:rounded-full",
                "[&::-webkit-scrollbar-thumb]:hover:bg-gray-400",
                className
            )}
            {...props}
        >
            {children}
        </div>
    );
}
// @ts-expect-error фикс линтера
// eslint-disable-next-line @typescript-eslint/no-unused-vars
function ScrollBar({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
    // This is a no-op component for compatibility
    return null;
}

export { ScrollArea, ScrollBar };
