"use client";

import { useState, useEffect, useCallback } from "react";
import { getUserIdFromToken } from "@/lib/auth";
import { getStoredActiveProfile, storeActiveProfile, clearActiveProfile } from "@/stores/profile-store";
import { apiClient } from "@/lib/apiClient";

export interface Profile {
  id: string;
  userId: string;
  name: string;
  profileType: number;
  subscriptionId: string | null;
  companyName: string | null;
  bio: string | null;
  avatarUrl: string | null;
  status: number;
  createdAt: string;
  updatedAt: string;
  isOwner: boolean;
  memberRole: string | null;
}

const profileTypeLabels = ["Free", "Basic", "Pro"];

export function getProfileTypeLabel(type: number): string {
  return profileTypeLabels[type] ?? "Unknown";
}

let cachedProfiles: Profile[] | null = null;
let cacheListeners: Array<() => void> = [];
let fetchingProfiles = false;

function notifyCache() {
  cacheListeners = cacheListeners.filter((fn) => {
    try { fn(); } catch { /* skip */ }
    return true;
  });
}

export function invalidateProfileCache() {
  cachedProfiles = null;
  notifyCache();
}

export function addProfileToCache(profile: Profile) {
  if (cachedProfiles) {
    const exists = cachedProfiles.some((p) => p.id === profile.id);
    if (!exists) cachedProfiles = [...cachedProfiles, profile];
  } else {
    cachedProfiles = [profile];
  }
}

export function useProfiles() {
  const [profiles, setProfiles] = useState<Profile[]>(() => {
    if (!cachedProfiles) return [];
    const userId = getUserIdFromToken();
    if (userId && cachedProfiles.some((p) => p.userId === userId)) return cachedProfiles;
    cachedProfiles = null;
    return [];
  });
  const [loading, setLoading] = useState(!cachedProfiles);
  const [error, setError] = useState<string | null>(null);

  const fetchProfiles = useCallback(async () => {
    const userId = getUserIdFromToken();

    if (cachedProfiles) {
      const belongsToUser = userId && cachedProfiles.some((p) => p.userId === userId);
      if (belongsToUser) {
        setProfiles(cachedProfiles);
        setLoading(false);
        return;
      }
      cachedProfiles = null;
    }

    if (!userId) {
      setLoading(false);
      setProfiles([]);
      return;
    }

    if (fetchingProfiles) return;
    fetchingProfiles = true;

    try {
      const res = await apiClient(`/profiles/user/${userId}`) as { success: boolean; data?: Profile[]; message?: string };
      if (res?.success && Array.isArray(res.data)) {
        cachedProfiles = res.data;
        setProfiles(res.data);
      } else {
        setError(res?.message || "Failed to load profiles");
      }
    } catch (err: any) {
      setError(err?.message || "Network error. Please check your connection");
    } finally {
      setLoading(false);
      fetchingProfiles = false;
    }
  }, []);

  useEffect(() => {
    const isMounted = { current: true };
    fetchProfiles();
    const listener = () => {
      if (isMounted.current) fetchProfiles();
    };
    cacheListeners.push(listener);
    return () => {
      isMounted.current = false;
      cacheListeners = cacheListeners.filter((fn) => fn !== listener);
    };
  }, [fetchProfiles]);

  const stored = getStoredActiveProfile();
  const storedMatch = stored ? profiles.find((p) => p.id === stored.id) : null;
  const fallbackMatch = profiles.length === 1
    ? profiles[0]
    : profiles.find((p) => p.status === 1) || null;
  const activeProfile = storedMatch || fallbackMatch;
  const storedId = stored?.id ?? null;
  const storedName = stored?.name ?? null;
  const storedProfileType = stored?.profileType ?? null;
  const activeProfileId = activeProfile?.id ?? null;
  const activeProfileName = activeProfile?.name ?? null;
  const activeProfileType = activeProfile?.profileType ?? null;

  useEffect(() => {
    if (storedId && !storedMatch && profiles.length > 1) {
      clearActiveProfile();
      return;
    }

    if (!activeProfileId || !activeProfileName || activeProfileType === null) {
      return;
    }

    if (
      storedId !== activeProfileId ||
      storedName !== activeProfileName ||
      storedProfileType !== activeProfileType
    ) {
      storeActiveProfile({
        id: activeProfileId,
        name: activeProfileName,
        profileType: activeProfileType,
      });
    }
  }, [storedId, storedName, storedProfileType, storedMatch, activeProfileId, activeProfileName, activeProfileType, profiles.length]);

  const selectProfile = useCallback((profile: Profile) => {
    storeActiveProfile({
      id: profile.id,
      name: profile.name,
      profileType: profile.profileType,
    });
    setProfiles((prev) => {
      if (prev.some((p) => p.id === profile.id)) return prev;
      return [...prev, profile];
    });
  }, []);

  const clearSelectedProfile = useCallback(() => {
    clearActiveProfile();
  }, []);

  return { profiles, loading, error, activeProfile, selectProfile, clearSelectedProfile, refetch: fetchProfiles };
}
