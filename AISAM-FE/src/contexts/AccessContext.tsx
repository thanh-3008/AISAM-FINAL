"use client";

import { createContext, useContext, useEffect, useRef, useState } from "react";
import { usePathname } from "next/navigation";
import Link from "next/link";
import { apiClient } from "@/lib/apiClient";
import { ACCESS_CHANGED, clearProtectedCaches } from "@/lib/accessEvents";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { clearActiveWorkspace } from "@/stores/workspace-store";
import { logout } from "@/lib/auth";

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
  const [accessError, setAccessError] = useState(false);
  const [revision, setRevision] = useState(0);
  const lastContext = useRef("");
  const lastWorkspaceId = useRef<string | undefined>(undefined);
  const retryRef = useRef<(() => void) | null>(null);

  useEffect(() => {
    let sequence = 0;
    let disposed = false;
    
    // Only reset access if switching to a DIFFERENT workspace
    if (activeWorkspace?.id !== lastWorkspaceId.current) {
      setAccess(null);
      clearProtectedCaches();
      lastContext.current = "";
      lastWorkspaceId.current = activeWorkspace?.id;
    }
    setDenied(false);
    setAccessError(false);

    const refresh = async (invalidate = false) => {
      const current = ++sequence;
      setAccessError(false);
      if (invalidate) { setAccess(null); lastContext.current = ""; }
      if (!activeWorkspace?.id) return;
      try {
        const result = await apiClient("/access/context", { cache: "no-store" });
        if (!disposed && current === sequence) {
          if (result?.success && result.data?.workspaceId === activeWorkspace.id) {
            setAccessError(false);
            const fingerprint = JSON.stringify(result.data);
            if (lastContext.current !== fingerprint) {
              clearProtectedCaches();
              lastContext.current = fingerprint;
              setRevision((value) => value + 1);
            }
            setAccess(result.data as AccessContextData);
          } else {
            setAccess(null);
            setAccessError(true);
          }
        }
      } catch (err: unknown) {
        if (!disposed && current === sequence) {
          setAccess(null);
          const status = (err as { status?: number })?.status;
          if (status === 403) {
            clearActiveWorkspace();
            setDenied(true);
            setAccessError(false);
          } else {
            setAccessError(true);
          }
        }
      }
    };

    const revalidate = () => { void refresh(); };
    retryRef.current = revalidate;
    void refresh();

    const changed = (event: Event) => {
      if ((event as CustomEvent).detail === "denied") setDenied(true);
      void refresh(true);
    };

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

  return (
    <Context.Provider value={access}>
      {chrome}
      {denied ? (
        <div className="flex-1 flex items-center justify-center p-6 min-h-[50vh]">
          <div className="max-w-md w-full p-6 bg-white dark:bg-neutral-900 rounded-2xl border border-neutral-200 dark:border-neutral-800 shadow-xl text-center" role="alert">
            <div className="w-12 h-12 rounded-full bg-amber-500/10 text-amber-600 flex items-center justify-center mx-auto mb-4">
              <span className="material-symbols-outlined text-2xl">lock_reset</span>
            </div>
            <h2 className="text-base font-bold text-neutral-900 dark:text-neutral-100 mb-2">Quyền truy cập đã thay đổi. Vui lòng chọn trang khác.</h2>
            <p className="text-xs text-neutral-500 mb-6">
              Workspace hiện tại không còn khả dụng hoặc tài khoản của bạn chưa được cấp quyền truy cập.
            </p>
            <div className="flex flex-col gap-2.5">
              <Link
                href="/overview"
                onClick={() => { clearActiveWorkspace(); }}
                className="w-full py-2.5 px-4 rounded-xl bg-blue-600 text-white font-medium text-xs hover:bg-blue-700 transition-colors shadow-sm"
              >
                Vào trang Overview để chọn Workspace
              </Link>
              <Link
                href="/admin/dashboard"
                className="w-full py-2.5 px-4 rounded-xl bg-neutral-100 dark:bg-neutral-800 text-neutral-800 dark:text-neutral-200 font-medium text-xs hover:bg-neutral-200 dark:hover:bg-neutral-700 transition-colors"
              >
                Trang Quản trị Admin
              </Link>
              <button
                type="button"
                onClick={() => {
                  void logout().then(() => { window.location.href = "/login"; });
                }}
                className="w-full py-2 px-4 rounded-xl text-neutral-500 hover:text-red-600 text-xs transition-colors"
              >
                Đăng xuất tài khoản
              </button>
            </div>
          </div>
        </div>
      ) : (
        <AccessBoundary
          key={`${activeWorkspace?.id}:${revision}`}
          error={accessError}
          onRetry={() => retryRef.current?.()}
        >
          {children}
        </AccessBoundary>
      )}
    </Context.Provider>
  );
}

export function AccessBoundary({
  children,
  error = false,
  onRetry,
}: {
  children: React.ReactNode;
  error?: boolean;
  onRetry?: () => void;
}) {
  const access = useAccessContext();
  const path = usePathname();
  if (["/overview", "/pricing", "/credit-pack"].includes(path)) return children;
  if (error) {
    return (
      <div className="p-6" role="alert">
        <p className="font-semibold text-red-600">Không thể kết nối đến máy chủ xác minh quyền truy cập.</p>
        <p className="text-sm text-gray-500 mt-1">Vui lòng kiểm tra lại kết nối mạng hoặc thử lại.</p>
        {onRetry && (
          <button
            type="button"
            onClick={onRetry}
            className="mt-3 px-3 py-1.5 text-xs font-medium rounded-md bg-blue-600 text-white hover:bg-blue-700 transition-colors"
          >
            Thử lại
          </button>
        )}
      </div>
    );
  }
  if (!access) return <div className="p-6" role="status">Đang xác minh quyền truy cập…</div>;
  const aggregate = path === "/workspace-dashboard" || path === "/dashboard";
  const analytics = path === "/analytics";
  const own = path === "/creator-history" || path === "/own-analytics" || path === "/credit-history";
  const denied = (aggregate && !access.canViewAnalytics) ||
    (analytics && !access.canViewAnalytics && !access.canViewOwnAnalytics) ||
    (own && !access.canViewOwnAnalytics) ||
    (path.startsWith("/approvals") && !access.canReviewContent) ||
    (path.startsWith("/calendar") && !access.canPublish);
  if (denied) return <div className="p-6" role="alert">
    <p>Bạn không có quyền xem trang này.</p>
    {access.canViewOwnAnalytics && <Link className="underline" href="/analytics?tab=personal">Xem analytics và lịch sử cá nhân</Link>}
  </div>;
  return children;
}
