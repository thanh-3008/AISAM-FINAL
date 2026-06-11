"use client";

import { createContext, useContext, useState, useCallback, ReactNode } from "react";
import { motion, AnimatePresence } from "motion/react";

type ToastType = "success" | "error" | "warning" | "info";

interface Toast {
  id: string;
  type: ToastType;
  title: string;
  message?: string;
  duration?: number;
}

interface ToastContextType {
  showToast: (toast: Omit<Toast, "id">) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const showToast = useCallback((toast: Omit<Toast, "id">) => {
    const id = Math.random().toString(36).substring(7);
    const newToast = { ...toast, id };
    setToasts((prev) => [...prev, newToast]);

    const duration = toast.duration || 4000;
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, duration);
  }, []);

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const getToastStyles = (type: ToastType) => {
    const styles = {
      success: {
        bg: "bg-emerald-50 border-emerald-200",
        icon: "check_circle",
        iconColor: "text-emerald-500",
        titleColor: "text-emerald-900",
        messageColor: "text-emerald-700",
      },
      error: {
        bg: "bg-red-50 border-red-200",
        icon: "error",
        iconColor: "text-red-500",
        titleColor: "text-red-900",
        messageColor: "text-red-700",
      },
      warning: {
        bg: "bg-amber-50 border-amber-200",
        icon: "warning",
        iconColor: "text-amber-500",
        titleColor: "text-amber-900",
        messageColor: "text-amber-700",
      },
      info: {
        bg: "bg-blue-50 border-blue-200",
        icon: "info",
        iconColor: "text-blue-500",
        titleColor: "text-blue-900",
        messageColor: "text-blue-700",
      },
    };
    return styles[type];
  };

  return (
    <ToastContext.Provider value={{ showToast }}>
      {children}
      <div className="fixed bottom-4 right-4 z-[100] flex flex-col gap-2 max-w-sm">
        <AnimatePresence>
          {toasts.map((toast) => {
            const styles = getToastStyles(toast.type);
            return (
              <motion.div
                key={toast.id}
                initial={{ opacity: 0, y: 20, scale: 0.95 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, y: -20, scale: 0.95 }}
                transition={{ duration: 0.2 }}
                className={`flex items-start gap-3 p-4 rounded-xl border shadow-lg ${styles.bg}`}
              >
                <span className={`material-symbols-outlined ${styles.iconColor} text-[20px] shrink-0 mt-0.5`}>
                  {styles.icon}
                </span>
                <div className="flex-1 min-w-0">
                  <p className={`text-sm font-semibold ${styles.titleColor}`}>{toast.title}</p>
                  {toast.message && (
                    <p className={`text-sm mt-0.5 ${styles.messageColor}`}>{toast.message}</p>
                  )}
                </div>
                <button
                  onClick={() => removeToast(toast.id)}
                  className="shrink-0 text-gray-400 hover:text-gray-600 transition-colors"
                >
                  <span className="material-symbols-outlined text-[16px]">close</span>
                </button>
              </motion.div>
            );
          })}
        </AnimatePresence>
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error("useToast must be used within a ToastProvider");
  }
  return context;
}
