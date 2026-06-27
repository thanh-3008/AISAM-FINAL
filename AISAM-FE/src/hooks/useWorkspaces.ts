"use client";

import { useState, useEffect, useCallback } from "react";
import { getUserIdFromToken } from "@/lib/auth";
import { getStoredActiveWorkspace, storeActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";
import { clearActiveProfile } from "@/stores/profile-store";
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

async function waitForActiveWorkspaceFetch() {
  const startedAt = Date.now();
  while (fetchingWorkspaces && Date.now() - startedAt < 11000) {
    await new Promise((resolve) => setTimeout(resolve, 50));
  }
}

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

function resolveMemberRole(role: unknown): string | null {
  if (typeof role !== "number") return "Owner";
  const roleMap: Record<number, string> = {
    0: "Owner",
    1: "Owner",
    2: "Manager",
    3: "ContentCreator",
    4: "Viewer",
  };
  return roleMap[role] ?? "Viewer";
}

function isOwnerRole(role: unknown): boolean {
  return role === 0 || role === 1;
}

function getStoredWorkspaceFallback(userId: string | null): WorkspaceData | null {
  if (!userId) return null;
  const stored = getStoredActiveWorkspace();
  if (!stored) return null;

  return {
    id: stored.id,
    userId,
    name: stored.name,
    workspaceType: stored.workspaceType,
    plan: stored.workspaceType === 2 ? "Business" : "Personal",
    status: 1,
    createdAt: "",
    updatedAt: "",
    isOwner: true,
    memberRole: "Owner",
  };
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
  notifyCache();
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

  // Keep the server and first client render identical; browser storage is read by fetchWorkspaces.
  const [workspaces, setWorkspaces] = useState<WorkspaceData[]>([]);
  const [loading, setLoading] = useState(true);
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

    if (fetchingWorkspaces) {
      setLoading(true);
      await waitForActiveWorkspaceFetch();

      const sharedWorkspaces = cachedWorkspaces as WorkspaceData[] | null;
      if (sharedWorkspaces && sharedWorkspaces.some((workspace) => workspace.userId === userId)) {
        setWorkspaces(sharedWorkspaces);
      } else {
        const fallback = getStoredWorkspaceFallback(userId);
        setWorkspaces((prev) => prev.length > 0 ? prev : fallback ? [fallback] : []);
      }
      setLoading(false);
      return;
    }
    fetchingWorkspaces = true;


    let mapped: WorkspaceData[] = [];
    let fetched = false;
    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 10000);
      const res = await apiClient("/workspaces", { signal: controller.signal }) as { success: boolean; data?: Record<string, unknown>[] };
      clearTimeout(timeoutId);
      if (res?.success && res.data && Array.isArray(res.data)) {
        fetched = true;
        mapped = res.data.map((w) => ({
          id: String(w.id), userId, name: String(w.name),
          workspaceType: typeof w.workspaceType === "number" ? w.workspaceType : 1,
          plan: w.workspaceType === 2 ? "Business" : "Personal",
          status: typeof w.status === "number" ? w.status : 1,
          createdAt: String(w.createdAt), updatedAt: String(w.updatedAt),
          isOwner: isOwnerRole(w.currentUserRole),
          memberRole: resolveMemberRole(w.currentUserRole),
        }));
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load workspaces");
    } finally {
      fetchingWorkspaces = false;
    }

    if (!fetched) {
      const fallback = getStoredWorkspaceFallback(userId);
      setWorkspaces((prev) => {
        if (prev.length > 0) return prev;
        return fallback ? [fallback] : [];
      });
      setLoading(false);
      return;
    }

    cachedWorkspaces = mapped;
    setWorkspaces(mapped);
    setLoading(false);
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
    clearActiveProfile();
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

  const updateWorkspacePlan = useCallback((workspaceId: string, plan: string) => {
    const source = cachedWorkspaces ?? workspaces;
    const next = source.map((workspace) =>
      workspace.id === workspaceId ? { ...workspace, plan } : workspace
    );
    cachedWorkspaces = next;
    setWorkspaces(next);
    notifyCache();
  }, [workspaces]);

  return { workspaces, loading, error, activeWorkspace, selectWorkspace, clearSelectedWorkspace, refetch: fetchWorkspaces, updateWorkspacePlan };
}
