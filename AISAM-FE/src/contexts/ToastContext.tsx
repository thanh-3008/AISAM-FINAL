"use client";

import { createContext, useContext, useState, useCallback, type ReactNode } from "react";
import { motion, AnimatePresence } from "motion/react";

export type ToastType = "success" | "error" | "warning" | "info";

export interface Toast {
  id: number;
  type: ToastType;
  title: string;
  message?: string;
}

interface ToastContextValue {
  toasts: Toast[];
  showToast: (toast: Omit<Toast, "id">) => void;
  addToast: (message: string, icon?: string) => void;
  removeToast: (id: number) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

let toastId = 0;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const showToast = useCallback((toast: Omit<Toast, "id">) => {
    const id = ++toastId;
    setToasts((prev) => [...prev, { ...toast, id }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 4000);
  }, []);

  const addToast = useCallback((message: string, icon = "check_circle") => {
    const type: ToastType = icon === "error" || icon === "delete" ? "error" : 
                           icon === "warning" || icon === "construction" ? "warning" : 
                           icon === "info" ? "info" : "success";
    showToast({ type, title: message });
  }, [showToast]);

  const removeToast = useCallback((id: number) => {
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
    <ToastContext.Provider value={{ toasts, showToast, addToast, removeToast }}>
      {children}
      <div className="fixed bottom-4 right-4 z-[999] flex flex-col gap-2 max-w-sm">
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
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToast must be used within a ToastProvider");
  return ctx;
}
