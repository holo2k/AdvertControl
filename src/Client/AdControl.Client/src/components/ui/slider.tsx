import * as React from "react";
import { cn } from "./utils";

interface SliderProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'value' | 'onChange' | 'defaultValue'> {
    value?: number[];
    defaultValue?: number[];
    min?: number;
    max?: number;
    step?: number;
    onValueChange?: (value: number[]) => void;
}

function Slider({
                    className,
                    defaultValue,
                    value,
                    min = 0,
                    max = 100,
                    step = 1,
                    onValueChange,
                    disabled,
                    id,
                    name,
                    ...props // Остальные стандартные пропсы input
                }: SliderProps) {
    const [internalValue, setInternalValue] = React.useState<number>(
        (value && value[0]) || (defaultValue && defaultValue[0]) || min
    );

    const currentValue = value ? value[0] : internalValue;

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const newValue = parseFloat(e.target.value);
        setInternalValue(newValue);
        if (onValueChange) {
            onValueChange([newValue]);
        }
    };

    return (
        <div className={cn("relative flex w-full items-center", className)}>
            <input
                type="range"
                min={min}
                max={max}
                step={step}
                value={currentValue}
                onChange={handleChange}
                disabled={disabled}
                id={id}
                name={name}
                className={cn(
                    "w-full h-4 bg-gray-200 rounded-lg appearance-none cursor-pointer",
                    "disabled:opacity-50 disabled:cursor-not-allowed",
                    "[&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:w-4 [&::-webkit-slider-thumb]:h-4",
                    "[&::-webkit-slider-thumb]:bg-blue-600 [&::-webkit-slider-thumb]:rounded-full [&::-webkit-slider-thumb]:cursor-pointer",
                    "[&::-webkit-slider-thumb]:border [&::-webkit-slider-thumb]:border-blue-600 [&::-webkit-slider-thumb]:shadow-sm",
                    "[&::-webkit-slider-thumb]:hover:ring-4 [&::-webkit-slider-thumb]:hover:ring-blue-100",
                    "[&::-moz-range-thumb]:w-4 [&::-moz-range-thumb]:h-4 [&::-moz-range-thumb]:bg-blue-600",
                    "[&::-moz-range-thumb]:rounded-full [&::-moz-range-thumb]:cursor-pointer [&::-moz-range-thumb]:border-0",
                    "[&::-moz-range-thumb]:hover:ring-4 [&::-moz-range-thumb]:hover:ring-blue-100"
                )}
                {...props} // Только стандартные HTML атрибуты
            />
        </div>
    );
}

export { Slider };
