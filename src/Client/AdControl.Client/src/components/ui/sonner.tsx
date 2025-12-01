const Toaster = () => {
    return null; // No-op component for compatibility
};

// Toast function for compatibility
export const toast = {
    success: (message: string) => {
        console.log("Success:", message);
    },
    error: (message: string) => {
        console.error("Error:", message);
    },
    info: (message: string) => {
        console.info("Info:", message);
    },
    warning: (message: string) => {
        console.warn("Warning:", message);
    },
};

export { Toaster };
