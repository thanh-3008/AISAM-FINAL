"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { invalidateWorkspaceCache } from "@/hooks/useWorkspaces";
import { invalidateProfileCache } from "@/hooks/useProfiles";
import { apiClient } from "@/lib/apiClient";

// Remount workspace-local providers and pages together; outstanding requests are
// rejected by apiClient if their workspace no longer matches browser state.
export default function WorkspaceBoundary({ children }: { children: React.ReactNode }) {
  const [revision, setRevision] = useState(0);
  const [denied, setDenied] = useState(false);
  useEffect(() => {
    let contextRevision: string | null = null;
    let cancelled = false;
    let generation = 0;
    const reset = () => { generation++; contextRevision = null; invalidateWorkspaceCache(); invalidateProfileCache(); setDenied(false); setRevision(n => n + 1); };
    const verify = async () => {
      const started = generation;
      try {
        const result = await apiClient("/permissions/context");
        if (cancelled || started !== generation || !result?.data?.revision) return;
        if (contextRevision && contextRevision !== result.data.revision) reset();
        contextRevision = result.data.revision;
      } catch { /* apiClient displays access failures; network errors do not erase drafts. */ }
    };
    void verify();
    const timer = setInterval(verify, 60000);
    const deny = (event: Event) => {
      const detail = (event as CustomEvent).detail;
      // A forbidden action/paid feature must not revoke the whole workspace UI.
      // Recheck the workspace context; its own 403 is authoritative.
      if (!detail?.path || detail.path === "/permissions/context") setDenied(true);
      else void verify();
    };
    const storage = (event: StorageEvent) => { if (event.key === "aisam_active_workspace" || event.key === null) reset(); };
    window.addEventListener("aisam-workspace-changed", reset);
    window.addEventListener("aisam-permissions-changed", reset);
    window.addEventListener("aisam-access-denied", deny);
    window.addEventListener("storage", storage);
    window.addEventListener("focus", verify);
    return () => {
      cancelled = true; clearInterval(timer);
      window.removeEventListener("aisam-workspace-changed", reset);
      window.removeEventListener("aisam-permissions-changed", reset);
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
        <button className="rounded-xl bg-blue-600 px-4 py-3 text-sm font-semibold text-white hover:bg-blue-700 focus-visible:outline-2 focus-visible:outline-blue-600" onClick={() => { invalidateWorkspaceCache(); setDenied(false); setRevision(n => n + 1); }}>Kiểm tra lại quyền</button>
        <Link className="rounded-xl border border-slate-200 px-4 py-3 text-sm font-medium text-slate-700 hover:bg-slate-50" href="/overview">Chọn workspace khác</Link>
      </div>
    </section>
  </div>;
  return <div key={revision}>{children}</div>;
}
