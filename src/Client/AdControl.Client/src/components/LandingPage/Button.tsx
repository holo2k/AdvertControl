interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary';
  children: React.ReactNode;
}

export function Button({ variant = 'primary', children, className = '', ...props }: ButtonProps) {
  const baseStyles = "px-8 py-4 rounded-lg font-semibold transition-all duration-300 transform hover:scale-105";
  const variants = {
    primary: "bg-[#2563EB] text-white hover:bg-[#1d4ed8] shadow-lg hover:shadow-xl",
    secondary: "bg-white text-[#2563EB] border-2 border-[#2563EB] hover:bg-gray-50"
  };

  return (
    <button 
      className={`${baseStyles} ${variants[variant]} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}
