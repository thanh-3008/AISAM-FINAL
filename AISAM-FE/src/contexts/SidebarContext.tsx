"use client";

import { createContext, useContext, useState, useCallback, type ReactNode } from "react";

interface SidebarContextType {
  open: boolean;
  toggle: () => void;
  setOpen: (v: boolean) => void;
}

const SidebarContext = createContext<SidebarContextType>({
  open: true,
  toggle: () => {},
  setOpen: () => {},
});

export function SidebarProvider({ children }: { children: ReactNode }) {
  const [open, setOpen] = useState(() => {
    if (typeof window === "undefined") return true;
    const saved = localStorage.getItem("aisam_sidebar_open");
    return saved !== null ? saved === "true" : true;
  });

  const toggle = useCallback(() => {
    setOpen((prev) => {
      const next = !prev;
      localStorage.setItem("aisam_sidebar_open", String(next));
      return next;
    });
  }, []);

  const setOpenFn = useCallback((v: boolean) => {
    setOpen(v);
    localStorage.setItem("aisam_sidebar_open", String(v));
  }, []);

  return (
    <SidebarContext.Provider value={{ open, toggle, setOpen: setOpenFn }}>
      {children}
    </SidebarContext.Provider>
  );
}

export function useSidebar() {
  return useContext(SidebarContext);
}
