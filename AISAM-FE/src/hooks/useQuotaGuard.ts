"use client";

import { useCallback, useEffect, useState } from "react";
import { fetchCurrentWorkspaceQuota, type QuotaSummary } from "@/services/quotaService";

export function useQuotaGuard() {
  const [quota, setQuota] = useState<QuotaSummary | null>(null);
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refresh = useCallback(async () => {
    setIsLoading(true);
    try {
      // [REFACTOR] Route quota reads through the service layer while preserving the same endpoint and guard behavior.
      const data = await fetchCurrentWorkspaceQuota();
      setQuota(data);
      setError(null);
      return data;
    } catch (err) {
      const nextError = err instanceof Error ? err : new Error("Failed to load quota.");
      setError(nextError);
      setQuota(null);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const initialLoadId = window.setTimeout(() => {
      void refresh();
    }, 0);
    const intervalId = window.setInterval(() => {
      void refresh();
    }, 60_000);

    return () => {
      window.clearTimeout(initialLoadId);
      window.clearInterval(intervalId);
    };
  }, [refresh]);

  return {
    quota,
    isLoading,
    error,
    refresh,
    canGenerateAI: (quota?.promptRemaining ?? 0) > 0,
    canPublish: (quota?.postRemaining ?? 0) > 0,
    canSchedule: (quota?.postRemaining ?? 0) > 0,
  };
}
