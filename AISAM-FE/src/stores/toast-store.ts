"use client";

import { create } from "zustand";

export type ToastItem = {
  id: string;
  title: string;
  description?: string;
  tone?: "neutral" | "error" | "success";
};

type ToastState = {
  items: ToastItem[];
  push: (item: Omit<ToastItem, "id">) => void;
  remove: (id: string) => void;
};

export const useToastStore = create<ToastState>((set) => ({
  items: [],
  push: (item) =>
    set((state) => ({
      items: [...state.items, { ...item, id: crypto.randomUUID() }]
    })),
  remove: (id) =>
    set((state) => ({
      items: state.items.filter((item) => item.id !== id)
    }))
}));
