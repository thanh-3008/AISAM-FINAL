"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { invalidateWorkspaceCache } from "@/hooks/useWorkspaces";
import { invalidateProfileCache } from "@/hooks/useProfiles";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";
import { apiClient } from "@/lib/apiClient";
import { RbacContext, parseRbacContext, type RbacContextValue } from "@/contexts/RbacContext";

// Remount workspace-local providers and pages together; outstanding requests are
// rejected by apiClient if their workspace no longer matches browser state.
export default function WorkspaceBoundary({ children }: { children: React.ReactNode }) {
  const [revision, setRevision] = useState(0);
  const [denied, setDenied] = useState(false);
  const [verificationError, setVerificationError] = useState<string | null>(null);
  const [rbac, setRbac] = useState<RbacContextValue | null>(null);
  const [verified, setVerified] = useState(false);
  useEffect(() => {
    let contextRevision: string | null = null;
    let cancelled = false;
    let generation = 0;
    let workspaceId = getStoredActiveWorkspace()?.id ?? null;
    let checkingGeneration: number | null = null;
    let hasAppliedContext = false;
    const loadPermissionContext = () => {
      const controller = new AbortController();
      let timeoutId: ReturnType<typeof setTimeout> | undefined;
      const timeout = new Promise<never>((_, reject) => {
        timeoutId = setTimeout(() => {
          controller.abort();
          reject(new Error("Kiểm tra quyền quá thời gian. Vui lòng thử lại."));
        }, 10000);
      });
      return Promise.race([
        apiClient("/permissions/context", { signal: controller.signal }),
        timeout,
      ]).finally(() => { if (timeoutId) clearTimeout(timeoutId); });
    };
    const refreshPermissions = () => {
      setDenied(false);
      setVerificationError(null);
      void verify();
    };
    const switchWorkspace = () => {
      const nextWorkspaceId = getStoredActiveWorkspace()?.id ?? null;
      // Several hooks may persist the already-selected workspace again. That
      // emits the same event and previously restarted permission verification
      // forever for some workspaces. Only reset on a real ID transition.
      if (nextWorkspaceId === workspaceId) {
        void verify();
        return;
      }
      workspaceId = nextWorkspaceId;
      generation++;
      contextRevision = null;
      hasAppliedContext = false;
      checkingGeneration = null;
      invalidateWorkspaceCache();
      invalidateProfileCache();
      setRbac(null);
      setVerified(false);
      setDenied(false);
      setVerificationError(null);
      setRevision(n => n + 1);
      void verify();
    };
    const verify = async () => {
      const started = generation;
      // Coalesce focus, storage and permission events while the same workspace
      // verification is already running.
      if (checkingGeneration === started) return;
      checkingGeneration = started;
      try {
        const result = await loadPermissionContext();
        if (cancelled || started !== generation) return;
        if (!result?.data?.revision) throw new Error("Missing permission revision");
        // Parse before remembering the revision. Previously an invalid or
        // temporarily incompatible response poisoned contextRevision; every
        // retry with the same revision returned early while rbac stayed null.
        const parsedContext = parseRbacContext(result.data);
        const permissionsChanged = contextRevision !== null && contextRevision !== result.data.revision;
        // A feature-level 403 asks us to recheck the workspace. Keep the same
        // context object when its revision did not change, otherwise consumers
        // remount, repeat the forbidden request, and create a request loop.
        if (hasAppliedContext && contextRevision === result.data.revision) return;
        contextRevision = result.data.revision;
        hasAppliedContext = true;
        setRbac(parsedContext);
        setVerified(true); setDenied(false); setVerificationError(null);
        // A new Brand/Team changes the permission revision normally. Apply the
        // returned context directly and remount workspace-local data once. Do
        // not invalidate workspace/profile caches or issue a second request:
        // doing so can temporarily discard the active request context and turn
        // a valid Owner mutation into a false workspace denial.
        if (permissionsChanged) setRevision(n => n + 1);
      } catch (error) {
        if (cancelled || started !== generation) return;
        const status = (error as { status?: number } | null)?.status;
        // Only the permission endpoint's explicit 403 means the actor is not
        // allowed into this workspace. Network, server, parsing and cancelled
        // requests must never be presented as a revoked membership.
        if (status === 403) {
          setDenied(true);
          setVerificationError(null);
          return;
        }
        if (contextRevision !== null) return;
        const message = error instanceof Error && error.name !== "AbortError"
          ? error.message
          : "Không thể tải thông tin quyền lúc này.";
        setVerificationError(message);
      } finally {
        if (checkingGeneration === started) checkingGeneration = null;
      }
    };
    void verify();
    const timer = setInterval(verify, 60000);
    const deny = (event: Event) => {
      const detail = (event as CustomEvent).detail;
      // apiClient emits this only when the permission context itself is
      // forbidden. Feature/action 403 responses remain local to their caller.
      if (!detail?.path || detail.path === "/permissions/context") setDenied(true);
    };
    const storage = (event: StorageEvent) => { if (event.key === "aisam_active_workspace" || event.key === null) switchWorkspace(); };
    window.addEventListener("aisam-workspace-changed", switchWorkspace);
    window.addEventListener("aisam-permissions-changed", refreshPermissions);
    window.addEventListener("aisam-access-denied", deny);
    window.addEventListener("storage", storage);
    window.addEventListener("focus", verify);
    return () => {
      cancelled = true; clearInterval(timer);
      window.removeEventListener("aisam-workspace-changed", switchWorkspace);
      window.removeEventListener("aisam-permissions-changed", refreshPermissions);
      window.removeEventListener("aisam-access-denied", deny);
      window.removeEventListener("storage", storage);
      window.removeEventListener("focus", verify);
    };
  }, []);
  if (denied) return <div className="min-h-screen bg-slate-50 flex items-center justify-center p-6">
    <section role="alert" className="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-7 shadow-lg text-center">
      <div aria-hidden="true" className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-amber-50 text-2xl text-amber-700">!</div>
      <h1 className="text-xl font-semibold text-slate-900">Không thể truy cập workspace</h1>
      <p className="mt-3 text-sm leading-6 text-slate-600">Tài khoản hoặc quyền truy cập cần được kiểm tra lại. Bạn vẫn đang đăng nhập.</p>
      <div className="mt-6 flex flex-col gap-3">
        <button className="rounded-xl bg-blue-600 px-4 py-3 text-sm font-semibold text-white hover:bg-blue-700 focus-visible:outline-2 focus-visible:outline-blue-600" onClick={() => window.dispatchEvent(new Event("aisam-permissions-changed"))}>Kiểm tra lại quyền</button>
        <Link className="rounded-xl border border-slate-200 px-4 py-3 text-sm font-medium text-slate-700 hover:bg-slate-50" href="/overview">Chọn workspace khác</Link>
      </div>
    </section>
  </div>;
  // Permission context enriches the UI; it is not the security boundary. Every
  // data API enforces authorization on the server. Keep the application usable
  // while context is loading or temporarily unavailable, and block only after
  // an explicit 403 from /permissions/context.
  return <RbacContext.Provider value={rbac}><div key={revision}>
    {verificationError && (
      <div role="status" className="fixed right-4 top-4 z-[100] flex max-w-md items-center gap-3 rounded-xl border border-amber-200 bg-white px-4 py-3 text-sm text-slate-700 shadow-lg">
        <span className="material-symbols-outlined text-amber-600">sync_problem</span>
        <span className="flex-1">{verificationError}</span>
        <button className="font-semibold text-blue-600 hover:text-blue-700" onClick={() => window.dispatchEvent(new Event("aisam-permissions-changed"))}>Thử lại</button>
      </div>
    )}
    {!verified && !verificationError && <span className="sr-only" role="status">Đang đồng bộ quyền workspace…</span>}
    {children}
  </div></RbacContext.Provider>;
}
