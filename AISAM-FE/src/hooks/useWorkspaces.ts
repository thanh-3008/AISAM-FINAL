"use client";

import { useState, useEffect, useCallback } from "react";
import { getToken, getUserIdFromToken } from "@/lib/auth";
import { getStoredActiveWorkspace, storeActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";
import { getMockWorkspaces } from "@/lib/mockWorkspace";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5116/api";

export interface WorkspaceData {
  id: string;
  userId: string;
  name: string;
  workspaceType: number;
  plan: string;
  status: number;
  bio?: string | null;
  companyName?: string | null;
  createdAt: string;
  updatedAt: string;
  isOwner: boolean;
  memberRole: string | null;
}

const workspaceTypeLabels: Record<number, string> = {
  1: "Personal",
  2: "Business",
};

export function getWorkspaceTypeLabel(type: number): string {
  return workspaceTypeLabels[type] ?? "Unknown";
}

let cachedWorkspaces: WorkspaceData[] | null = null;
let cacheListeners: Array<() => void> = [];
let fetchingWorkspaces = false;

function getPlanName(profileType: number): string {
  return profileType === 2 ? "Business" : "Personal";
}

function notifyCache() {
  cacheListeners = cacheListeners.filter((fn) => {
    try { fn(); } catch { /* skip */ }
    return true;
  });
}

export function invalidateWorkspaceCache() {
  cachedWorkspaces = null;
  notifyCache();
}

export function addWorkspaceToCache(workspace: WorkspaceData) {
  if (cachedWorkspaces) {
    const exists = cachedWorkspaces.some((p) => p.id === workspace.id);
    if (!exists) cachedWorkspaces = [...cachedWorkspaces, workspace];
  } else {
    cachedWorkspaces = [workspace];
  }
}

export function useWorkspaces() {
  const [workspaces, setWorkspaces] = useState<WorkspaceData[]>(() => {
    if (!cachedWorkspaces) return [];
    const userId = getUserIdFromToken();
    if (userId && cachedWorkspaces.some((p) => p.userId === userId)) return cachedWorkspaces;
    cachedWorkspaces = null;
    return [];
  });
  const [loading, setLoading] = useState(!cachedWorkspaces);
  const [error, setError] = useState<string | null>(null);

  const fetchWorkspaces = useCallback(async () => {
    const userId = getUserIdFromToken();

    if (cachedWorkspaces) {
      const belongsToUser = userId && cachedWorkspaces.some((p) => p.userId === userId);
      if (belongsToUser) {
        setWorkspaces(cachedWorkspaces);
        setLoading(false);
        return;
      }
      cachedWorkspaces = null;
    }

    if (!userId) {
      setLoading(false);
      setWorkspaces([]);
      return;
    }

    if (fetchingWorkspaces) return;
    fetchingWorkspaces = true;

    const fetchFromProfiles = async () => {
      try {
        const res = await fetch(`${API_URL}/profiles/user/${userId}`, {
          headers: { Authorization: `Bearer ${getToken()}` },
        });
        const result = await res.json();
        if (result.success && Array.isArray(result.data)) {
          const mapped: WorkspaceData[] = result.data.map((p: any) => ({
            id: p.id,
            userId: p.userId,
            name: p.name,
            workspaceType: p.profileType ?? 1,
            plan: getPlanName(p.profileType ?? 0),
            status: p.status,
            createdAt: p.createdAt,
            updatedAt: p.updatedAt,
            isOwner: p.isOwner ?? true,
            memberRole: p.memberRole ?? "Owner",
          }));
          cachedWorkspaces = mapped;
          setWorkspaces(mapped);
        } else {
          // Fallback to mock data
          const mockData = getMockWorkspaces(userId);
          cachedWorkspaces = mockData;
          setWorkspaces(mockData);
        }
      } catch {
        // Fallback to mock data
        const mockData = getMockWorkspaces(userId);
        cachedWorkspaces = mockData;
        setWorkspaces(mockData);
      }
    };

    try {
      const res = await fetch(`${API_URL}/workspaces/user/${userId}`, {
        headers: { Authorization: `Bearer ${getToken()}` },
      });
      const result = await res.json();
      if (result.success && Array.isArray(result.data)) {
        cachedWorkspaces = result.data;
        setWorkspaces(result.data);
      } else {
        await fetchFromProfiles();
      }
    } catch {
      await fetchFromProfiles();
    } finally {
      setLoading(false);
      fetchingWorkspaces = false;
    }
  }, []);

  useEffect(() => {
    const isMounted = { current: true };
    fetchWorkspaces();
    const listener = () => {
      if (isMounted.current) fetchWorkspaces();
    };
    cacheListeners.push(listener);
    return () => {
      isMounted.current = false;
      cacheListeners = cacheListeners.filter((fn) => fn !== listener);
    };
  }, [fetchWorkspaces]);

  const stored = getStoredActiveWorkspace();
  const storedMatch = stored ? workspaces.find((p) => p.id === stored.id) : null;
  const fallbackMatch = workspaces.find((p) => p.status === 1) || workspaces[0] || null;
  const activeWorkspace = storedMatch || fallbackMatch;

  const selectWorkspace = useCallback((workspace: WorkspaceData) => {
    storeActiveWorkspace({
      id: workspace.id,
      name: workspace.name,
      workspaceType: workspace.workspaceType,
    });
    setWorkspaces((prev) => {
      if (prev.some((p) => p.id === workspace.id)) return prev;
      return [...prev, workspace];
    });
  }, []);

  const clearSelectedWorkspace = useCallback(() => {
    clearActiveWorkspace();
  }, []);

  return { workspaces, loading, error, activeWorkspace, selectWorkspace, clearSelectedWorkspace, refetch: fetchWorkspaces };
}
