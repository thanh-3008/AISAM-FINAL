"use client";

import { useRbac } from "@/contexts/RbacContext";
import RbacTeamManagement from "@/components/team/RbacTeamManagement";

export default function TeamPage() {
  const rbac = useRbac();

  // WorkspaceBoundary loads the permission contract asynchronously. The
  // application has completed the v2 cutover, so this route must never render
  // the former workspace Owner/Manager/ContentCreator/Viewer model while the
  // contract is pending or unavailable.
  if (!rbac) {
    return (
      <main className="mx-auto flex min-h-[50vh] w-full max-w-6xl items-center justify-center p-6">
        <section role="status" className="rounded-2xl border border-slate-200 bg-white px-8 py-7 text-center shadow-sm">
          <span className="material-symbols-outlined animate-spin text-3xl text-blue-600" aria-hidden="true">
            progress_activity
          </span>
          <h1 className="mt-3 text-lg font-semibold text-slate-900">Đang tải phân quyền hai tầng</h1>
          <p className="mt-2 text-sm text-slate-600">Vai trò workspace và vai trò Team đang được đồng bộ.</p>
          <button
            type="button"
            className="mt-5 rounded-xl border border-slate-200 px-4 py-2 text-sm font-medium text-blue-700 hover:bg-blue-50"
            onClick={() => window.dispatchEvent(new Event("aisam-permissions-changed"))}
          >
            Tải lại quyền
          </button>
        </section>
      </main>
    );
  }

  return <RbacTeamManagement />;
}
