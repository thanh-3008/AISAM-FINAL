"use client";

import { useState, useEffect, useCallback } from "react";
import { getUserIdFromToken } from "@/lib/auth";
import { getStoredActiveWorkspace, storeActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";
import { apiClient } from "@/lib/apiClient";

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
let workspaceSelectListeners: Array<() => void> = [];

function notifyWorkspaceSelected() {
  workspaceSelectListeners.forEach((fn) => { try { fn(); } catch { /* skip */ } });
}

function getPlanName(profileType: number): string {
  if (profileType === 0) return "Free";
  if (profileType === 2) return "Business Plus";
  return "Personal Pro";
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
  const [, forceRender] = useState(0);

  useEffect(() => {
    const handler = () => forceRender((n) => n + 1);
    workspaceSelectListeners.push(handler);
    return () => {
      workspaceSelectListeners = workspaceSelectListeners.filter((fn) => fn !== handler);
    };
  }, []);

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


    let mapped: WorkspaceData[] = [];
    try {
      const res = await apiClient("/workspaces") as { success: boolean; data?: Record<string, unknown>[] };
      if (res?.success && res.data && Array.isArray(res.data)) {
        mapped = res.data.map((w) => ({
          id: String(w.id), userId, name: String(w.name),
          workspaceType: typeof w.workspaceType === "number" ? w.workspaceType : 1,
          plan: w.workspaceType === 2 ? "Business" : "Personal",
          status: typeof w.status === "number" ? w.status : 1,
          createdAt: String(w.createdAt), updatedAt: String(w.updatedAt),
          isOwner: w.currentUserRole === 0,
          memberRole: typeof w.currentUserRole === "number" ? ["Owner", "Manager", "ContentCreator", "Viewer"][w.currentUserRole] ?? "Viewer" : "Owner",
        }));
      }
    } catch { /* /workspaces fail */ }

    cachedWorkspaces = mapped;
    setWorkspaces(mapped);
    setLoading(false);
    fetchingWorkspaces = false;
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
    notifyWorkspaceSelected();
  }, []);

  const clearSelectedWorkspace = useCallback(() => {
    clearActiveWorkspace();
  }, []);

  return { workspaces, loading, error, activeWorkspace, selectWorkspace, clearSelectedWorkspace, refetch: fetchWorkspaces };
}
