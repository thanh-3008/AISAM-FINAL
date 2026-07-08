"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { apiClient } from "@/lib/apiClient";
import { deleteWorkspace } from "@/services/adminService";

const typeLabels: Record<number, string> = { 1: "Personal", 2: "Business" };
const statusLabels: Record<number, string> = { 0: "Active", 1: "Limited", 2: "Archived", 3: "Deleted" };

export default function AdminWorkspaceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [ws, setWs] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    apiClient(`/admin/workspaces/${id}`).then((res: any) => { setWs(res?.data); setLoading(false); });
  }, [id]);

  if (loading) return (
    <><AdminHeader breadcrumbs={[{ label: "Workspaces", href: "/admin/workspaces" }, { label: "Loading..." }]} /><main className="flex-1 p-8"><div className="animate-pulse h-64 bg-gray-100 rounded-xl" /></main></>
  );

  if (!ws) return (
    <><AdminHeader breadcrumbs={[{ label: "Workspaces", href: "/admin/workspaces" }, { label: "Not Found" }]} /><main className="flex-1 p-8"><p className="text-gray-500">Workspace not found.</p></main></>
  );

  const handleDelete = async () => {
    if (!confirm("Delete this workspace permanently?")) return;
    await deleteWorkspace(ws.id);
    router.push("/admin/workspaces");
  };

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Workspaces", href: "/admin/workspaces" }, { label: ws.name }]} />
      <main className="flex-1 p-8 space-y-6">
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Workspace Details</h3>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div><dt className="text-gray-500">Name</dt><dd className="font-medium text-gray-900">{ws.name}</dd></div>
            <div><dt className="text-gray-500">Type</dt><dd><StatusBadge status={typeLabels[ws.workspaceType] ?? "Unknown"} variant={ws.workspaceType === 2 ? "info" : "neutral"} /></dd></div>
            <div><dt className="text-gray-500">Status</dt><dd><StatusBadge status={statusLabels[ws.status] ?? "Unknown"} variant={ws.status === 0 ? "success" : ws.status === 1 ? "warning" : "error"} /></dd></div>
            <div><dt className="text-gray-500">Member Limit</dt><dd className="font-medium text-gray-900">{ws.memberLimit ?? "N/A"}</dd></div>
            <div><dt className="text-gray-500">Created</dt><dd className="font-medium text-gray-900">{new Date(ws.createdAt).toLocaleDateString()}</dd></div>
            <div><dt className="text-gray-500">ID</dt><dd className="font-mono text-xs text-gray-500">{ws.id}</dd></div>
          </dl>
        </div>

        <div className="flex items-center gap-3">
          <button onClick={() => router.push("/admin/workspaces")} className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50">Back to Workspaces</button>
          <button onClick={handleDelete} className="px-4 py-2 text-sm rounded-lg bg-red-600 text-white hover:bg-red-700">Delete Workspace</button>
        </div>
      </main>
    </>
  );
}
