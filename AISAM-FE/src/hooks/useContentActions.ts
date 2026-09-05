"use client";

import { useEffect, useState } from "react";
import { apiClient } from "@/lib/apiClient";
import { useAccessContext } from "@/contexts/AccessContext";

export function useContentActions(contentId: string) {
  const access = useAccessContext();
  const [actions, setActions] = useState<Record<string, boolean>>({});
  useEffect(() => {
    let cancelled = false;
    setActions({});
    if (access && contentId) void apiClient(`/access/content/${contentId}/actions`, { cache: "no-store" })
      .then(result => { if (!cancelled && result?.success) setActions(result.data ?? {}); })
      .catch(() => { if (!cancelled) setActions({}); });
    return () => { cancelled = true; };
  }, [access?.workspaceId, access?.version, contentId]);
  return actions;
}
