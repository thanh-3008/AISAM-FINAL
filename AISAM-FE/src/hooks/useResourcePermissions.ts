"use client";
import { useEffect, useState } from "react";
import { checkPermissions, type PermissionCheck } from "@/services/permissionService";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";

export function useResourcePermissions(checks: PermissionCheck[]) {
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
    checkPermissions(requests).then(allowed => { if (!cancelled) setResult({ key, allowed }); })
      .catch(() => { if (!cancelled) setResult({ key, allowed: [] }); });
    return () => { cancelled = true; };
  }, [key, revision]);
  return (index: number) => result.key === key && result.allowed[index] === true;
}
