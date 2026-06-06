"use client";

import { createContext, useContext, useState, useCallback, type ReactNode } from "react";

export interface Toast {
  id: number;
  message: string;
  icon: string;
}

interface ToastContextValue {
  toasts: Toast[];
  addToast: (message: string, icon?: string) => void;
  removeToast: (id: number) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

let toastId = 0;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const addToast = useCallback((message: string, icon = "check_circle") => {
    const id = ++toastId;
    setToasts((prev) => [...prev, { id, message, icon }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 3000);
  }, []);

  const removeToast = useCallback((id: number) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  return (
    <ToastContext.Provider value={{ toasts, addToast, removeToast }}>
      {children}
      <div className="fixed bottom-6 right-6 z-[999] flex flex-col gap-2">
        {toasts.map((t) => (
          <div
            key={t.id}
            className="flex items-center gap-2.5 px-4 py-3 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-xl text-body-sm text-on-surface min-w-[240px]"
            style={{ animation: "toast-in 0.3s ease-out forwards" }}
          >
            <span className="material-symbols-outlined text-[18px] text-primary">{t.icon}</span>
            <span className="font-medium">{t.message}</span>
            <button onClick={() => removeToast(t.id)} className="ml-auto text-outline/40 hover:text-outline transition-colors">
              <span className="material-symbols-outlined text-[16px]">close</span>
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error("useToast must be used within a ToastProvider");
  return ctx;
}
