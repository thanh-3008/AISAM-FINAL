"use client";

import { useState, useEffect, useCallback } from "react";
import { getUserIdFromToken } from "@/lib/auth";
import { getStoredActiveWorkspace, storeActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";
import { fetchWorkspaces as fetchWorkspaceList } from "@/services/workspaceService";

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
    try {
      const workspaces = await fetchWorkspaceList();
      if (Array.isArray(workspaces)) {
        mapped = workspaces.map((w) => ({
          id: w.id,
          userId: w.userId ?? userId,
          name: w.name,
          workspaceType: w.workspaceType ?? 1,
          plan: w.workspaceType === 2 ? "Business" : "Personal",
          status: w.status ?? 1,
          bio: w.bio ?? null,
          companyName: w.companyName ?? null,
          createdAt: w.createdAt || new Date().toISOString(),
          updatedAt: w.updatedAt || new Date().toISOString(),
          isOwner: w.currentUserRole === 0,
          memberRole: typeof w.currentUserRole === "number" ? ["Owner", "Manager", "ContentCreator", "Viewer"][w.currentUserRole] ?? "Viewer" : "Owner",
        }));
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load workspaces");
    } finally {
      fetchingWorkspaces = false;
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
  const storedId = stored?.id ?? null;
  const storedName = stored?.name ?? null;
  const storedWorkspaceType = stored?.workspaceType ?? null;
  const activeWorkspaceId = activeWorkspace?.id ?? null;
  const activeWorkspaceName = activeWorkspace?.name ?? null;
  const activeWorkspaceType = activeWorkspace?.workspaceType ?? null;

  useEffect(() => {
    if (!activeWorkspaceId || !activeWorkspaceName || activeWorkspaceType === null) {
      return;
    }

    if (
      storedId !== activeWorkspaceId ||
      storedName !== activeWorkspaceName ||
      storedWorkspaceType !== activeWorkspaceType
    ) {
      storeActiveWorkspace({
        id: activeWorkspaceId,
        name: activeWorkspaceName,
        workspaceType: activeWorkspaceType,
      });
    }
  }, [storedId, storedName, storedWorkspaceType, activeWorkspaceId, activeWorkspaceName, activeWorkspaceType]);

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

  const updateWorkspacePlan = useCallback((workspaceId: string, plan: string) => {
    cachedWorkspaces = (cachedWorkspaces ?? workspaces).map((workspace) =>
      workspace.id === workspaceId ? { ...workspace, plan } : workspace
    );
    setWorkspaces(cachedWorkspaces);
    notifyCache();
  }, [workspaces]);

  return { workspaces, loading, error, activeWorkspace, selectWorkspace, clearSelectedWorkspace, updateWorkspacePlan, refetch: fetchWorkspaces };
}
