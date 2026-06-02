"use client";

import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import type { TokenResponse, UserDto } from "@/features/auth/types/auth";

type AuthState = {
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: string | null;
  user: UserDto | null;
  hydrated: boolean;
  setSession: (session: TokenResponse) => void;
  clearSession: () => void;
  markHydrated: () => void;
  isAuthenticated: () => boolean;
  isTokenExpired: () => boolean;
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
      user: null,
      hydrated: false,
      setSession: (session) =>
        set({
          accessToken: session.accessToken,
          refreshToken: session.refreshToken,
          expiresAt: session.expiresAt,
          user: session.user
        }),
      clearSession: () =>
        set({
          accessToken: null,
          refreshToken: null,
          expiresAt: null,
          user: null
        }),
      markHydrated: () => set({ hydrated: true }),
      isAuthenticated: () => {
        const state = get();
        return Boolean(state.accessToken && state.user);
      },
      isTokenExpired: () => {
        const { expiresAt } = get();
        if (!expiresAt) {
          return true;
        }
        return new Date(expiresAt).getTime() <= Date.now() + 30_000;
      }
    }),
    {
      name: "aisam-auth",
      storage: createJSONStorage(() => localStorage),
      onRehydrateStorage: () => (state) => {
        state?.markHydrated();
      }
    }
  )
);
