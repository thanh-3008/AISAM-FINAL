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
    const deny = () => { setDenied(true); };
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
  if (denied) return <main role="alert" className="p-8 space-y-4">
    <h1 className="text-xl font-semibold">Không có quyền truy cập</h1>
    <p>Quyền hoặc trạng thái workspace có thể đã thay đổi. Dữ liệu cũ đã được ẩn. Bạn vẫn đang đăng nhập.</p>
    <button className="border rounded-lg px-4 py-2" onClick={() => { invalidateWorkspaceCache(); setDenied(false); setRevision(n => n + 1); }}>Kiểm tra lại quyền</button>
    <Link className="ml-4 underline" href="/overview">Chọn workspace khác</Link>
  </main>;
  return <div key={revision}>{children}</div>;
}
