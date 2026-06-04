"use client";

import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import type { ProfileResponseDto } from "@/features/profile/types/profile";

type ProfileState = {
  activeProfile: ProfileResponseDto | null;
  setActiveProfile: (profile: ProfileResponseDto | null) => void;
  clearActiveProfile: () => void;
};

export const useProfileStore = create<ProfileState>()(
  persist(
    (set) => ({
      activeProfile: null,
      setActiveProfile: (profile) => set({ activeProfile: profile }),
      clearActiveProfile: () => set({ activeProfile: null })
    }),
    {
      name: "aisam-active-profile",
      storage: createJSONStorage(() => localStorage)
    }
  )
);
