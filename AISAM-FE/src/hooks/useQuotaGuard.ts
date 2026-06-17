"use client";

import { useCallback, useEffect, useState } from "react";
import { apiClient } from "@/lib/apiClient";

interface QuotaSummary {
  planName: string;
  promptRemaining: number;
  promptQuotaLimit: number;
  postRemaining: number;
  postQuotaLimit: number;
}

interface GenericResponse<T> {
  data?: T;
}

export function useQuotaGuard() {
  const [quota, setQuota] = useState<QuotaSummary | null>(null);
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refresh = useCallback(async () => {
    setIsLoading(true);
    try {
      const res: GenericResponse<QuotaSummary> = await apiClient("/quota/workspace/current");
      setQuota(res?.data ?? null);
      setError(null);
      return res?.data ?? null;
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
