"use client";
import { useEffect, useState, useMemo } from "react";
import { checkPermissions, type PermissionCheck } from "@/services/permissionService";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";

export interface ResourcePermissionChecker {
  (index: number): boolean;
  isReady: boolean;
}

export function useResourcePermissions(checks: PermissionCheck[]): ResourcePermissionChecker {
  const key = JSON.stringify([getStoredActiveWorkspace()?.id, checks]);
  const [result, setResult] = useState<{ key: string; allowed: boolean[] }>({ key: "", allowed: [] });
  const [revision, setRevision] = useState(0);
  useEffect(() => {
    const reset = () => { setResult({ key: "", allowed: [] }); setRevision(n => n + 1); };
    window.addEventListener("aisam-permissions-changed", reset);
    window.addEventListener("focus", reset);
    return () => { window.removeEventListener("aisam-permissions-changed", reset); window.removeEventListener("focus", reset); };
  }, []);
  useEffect(() => {
    let cancelled = false;
    const [, requests] = JSON.parse(key) as [string, PermissionCheck[]];
    if (requests.length === 0) {
      setResult({ key, allowed: [] });
      return () => { cancelled = true; };
    }
    checkPermissions(requests).then(allowed => { if (!cancelled) setResult({ key, allowed }); })
      .catch(() => { if (!cancelled) setResult({ key, allowed: [] }); });
    return () => { cancelled = true; };
  }, [key, revision]);

  const isReady = result.key === key;
  return useMemo(
    () =>
      Object.assign(
        (index: number) => isReady && result.allowed[index] === true,
        { isReady }
      ),
    [isReady, result.allowed]
  );
}
