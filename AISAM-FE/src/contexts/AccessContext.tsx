"use client";

import { createContext, useContext, useEffect, useRef, useState } from "react";
import { usePathname } from "next/navigation";
import Link from "next/link";
import { apiClient } from "@/lib/apiClient";
import { ACCESS_CHANGED, clearProtectedCaches } from "@/lib/accessEvents";
import { useWorkspaces } from "@/hooks/useWorkspaces";

export interface AccessContextData {
  workspaceId: string;
  userId: string;
  role: "Owner" | "Manager" | "ContentCreator" | "Viewer";
  teamIds: string[];
  version: string;
  canViewAnalytics: boolean;
  canViewOwnAnalytics: boolean;
  canManageTeams: boolean;
  canManageTasks: boolean;
  canCreateContent: boolean;
  canReviewContent: boolean;
  canPublish: boolean;
}

const Context = createContext<AccessContextData | null>(null);
export const useAccessContext = () => useContext(Context);

export function AccessProvider({ children, chrome }: { children: React.ReactNode; chrome?: React.ReactNode }) {
  const { activeWorkspace } = useWorkspaces();
  const pathname = usePathname();
  const [denied, setDenied] = useState(false);
  const [access, setAccess] = useState<AccessContextData | null>(null);
  const [revision, setRevision] = useState(0);
  const lastContext = useRef("");
  useEffect(() => {
    let sequence = 0;
    let disposed = false;
    setAccess(null);
    clearProtectedCaches();
    setDenied(false);
    lastContext.current = "";
    const refresh = async (invalidate = false) => {
      const current = ++sequence;
      if (invalidate) { setAccess(null); lastContext.current = ""; }
      if (!activeWorkspace?.id) return;
      try {
        const result = await apiClient("/access/context", { cache: "no-store" });
        if (!disposed && current === sequence && result?.success && result.data?.workspaceId === activeWorkspace.id) {
          const fingerprint = JSON.stringify(result.data);
          if (lastContext.current !== fingerprint) {
            clearProtectedCaches();
            lastContext.current = fingerprint;
            setRevision((value) => value + 1);
          }
          setAccess(result.data as AccessContextData);
        }
      } catch { if (!disposed && current === sequence) setAccess(null); }
    };
    void refresh();
    const changed = (event: Event) => { if ((event as CustomEvent).detail === "denied") setDenied(true); void refresh(true); };
    const revalidate = () => { void refresh(); };
    window.addEventListener(ACCESS_CHANGED, changed);
    window.addEventListener("focus", revalidate);
    const timer = window.setInterval(revalidate, 30_000);
    return () => {
      disposed = true;
      ++sequence;
      window.clearInterval(timer);
      window.removeEventListener(ACCESS_CHANGED, changed);
      window.removeEventListener("focus", revalidate);
    };
  }, [activeWorkspace?.id, pathname]);
  return <Context.Provider value={access}>{chrome}{denied ? <div className="p-6" role="alert">Quyền truy cập đã thay đổi. Vui lòng chọn trang khác.</div> : <AccessBoundary key={`${activeWorkspace?.id}:${revision}`}>{children}</AccessBoundary>}</Context.Provider>;
}

function AccessBoundary({ children }: { children: React.ReactNode }) {
  const access = useAccessContext();
  const path = usePathname();
  if (["/overview", "/pricing", "/credit-pack"].includes(path)) return children;
  if (!access) return <div className="p-6" role="status">Đang xác minh quyền truy cập…</div>;
  const aggregate = path === "/analytics" || path === "/workspace-dashboard" || path === "/dashboard";
  const own = path === "/creator-history" || path === "/own-analytics" || path === "/credit-history";
  const denied = aggregate && !access.canViewAnalytics || own && !access.canViewOwnAnalytics ||
    path.startsWith("/approvals") && !access.canReviewContent || path.startsWith("/calendar") && !access.canPublish;
  if (denied) return <div className="p-6" role="alert">
    <p>Bạn không có quyền xem trang này.</p>
    {access.canViewOwnAnalytics && <Link className="underline" href="/own-analytics">Xem analytics và lịch sử cá nhân</Link>}
  </div>;
  return children;
}
