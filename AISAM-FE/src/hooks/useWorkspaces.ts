"use client";

import { useState, useEffect, useCallback } from "react";
import { getUserIdFromToken } from "@/lib/auth";
import { apiClient } from "@/lib/apiClient";
import type { ApiResponse } from "@/lib/apiTypes";
import { getStoredActiveWorkspace, storeActiveWorkspace, clearActiveWorkspace } from "@/stores/workspace-store";

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
let workspaceSelectListeners: Array<() => void> = [];

function notifyWorkspaceSelected() {
  workspaceSelectListeners.forEach((fn) => { try { fn(); } catch { /* skip */ } });
}

const roleNames: Record<number, string> = {
  1: "Owner",
  2: "Manager",
  3: "ContentCreator",
  4: "Viewer",
};

function mapWorkspacePlan(workspaceType: number, planName?: string): string {
  if (planName === "Premium") return workspaceType === 2 ? "Business Pro" : "Personal Pro";
  if (planName === "Plus" || planName === "PlusTrial") return workspaceType === 2 ? "Business Plus" : "Personal Plus";
  return "Free";
}

function notifyCache() {
  cacheListeners = cacheListeners.filter((fn) => {
    try { fn(); } catch { /* skip */ }
    return true;
  });
}

function persistActiveWorkspace(workspaces: WorkspaceData[]) {
  const stored = getStoredActiveWorkspace();
  const selected = workspaces.find((workspace) => workspace.id === stored?.id)
    ?? workspaces.find((workspace) => workspace.status === 1)
    ?? workspaces[0];

  if (selected && selected.id !== stored?.id) {
    storeActiveWorkspace({
      id: selected.id,
      name: selected.name,
      workspaceType: selected.workspaceType,
    });
    notifyWorkspaceSelected();
  }
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
        const data = cachedWorkspaces;
        persistActiveWorkspace(data);
        queueMicrotask(() => {
          setWorkspaces(data);
          setLoading(false);
        });
        return;
      }
      cachedWorkspaces = null;
    }

    if (!userId) {
      queueMicrotask(() => {
        setLoading(false);
        setWorkspaces([]);
      });
      return;
    }

    try {
        setError(null);
        const result = await apiClient<ApiResponse<Array<{
          id: string;
          name: string;
          workspaceType: number;
          status: number;
          currentUserRole: number;
          createdAt: string;
          updatedAt: string;
        }>>>("/workspaces");
      if (result.success && Array.isArray(result.data)) {
        const mapped = await Promise.all(result.data.map(async (workspace): Promise<WorkspaceData> => {
          const quota = await apiClient<ApiResponse<{ planName: string }>>("/quota/workspace/current", {
            headers: { "X-Workspace-Id": workspace.id },
          }).catch(() => null);
          return {
            id: workspace.id,
            userId,
            name: workspace.name,
            workspaceType: workspace.workspaceType,
            plan: mapWorkspacePlan(workspace.workspaceType, quota?.data?.planName),
            status: workspace.status,
            createdAt: workspace.createdAt,
            updatedAt: workspace.updatedAt,
            isOwner: workspace.currentUserRole === 1,
            memberRole: roleNames[workspace.currentUserRole] ?? null,
          };
        }));
        cachedWorkspaces = mapped;
        persistActiveWorkspace(mapped);
        setWorkspaces(mapped);
      } else {
        cachedWorkspaces = [];
        clearActiveWorkspace();
        setWorkspaces([]);
      }
    } catch (error) {
      setError(error instanceof Error ? error.message : "Failed to load workspaces");
      cachedWorkspaces = [];
      clearActiveWorkspace();
      setWorkspaces([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const isMounted = { current: true };
    queueMicrotask(() => {
      if (isMounted.current) fetchWorkspaces();
    });
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
