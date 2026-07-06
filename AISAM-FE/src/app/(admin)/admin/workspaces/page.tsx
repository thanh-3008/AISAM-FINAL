"use client";

import { useEffect, useState, useCallback } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminWorkspaces, AdminWorkspace } from "@/services/adminService";

const workspaceTypeLabels: Record<number, string> = { 1: "Personal", 2: "Business" };
const workspaceStatusLabels: Record<number, string> = { 0: "Active", 1: "Limited", 2: "Archived", 3: "Deleted" };

export default function AdminWorkspacesPage() {
  const [workspaces, setWorkspaces] = useState<AdminWorkspace[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const loadWorkspaces = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminWorkspaces(page);
    if (data) {
      setWorkspaces(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page]);

  useEffect(() => { loadWorkspaces(); }, [loadWorkspaces]);

  const columns = [
    { key: "name", header: "Name" },
    {
      key: "workspaceType",
      header: "Type",
      render: (w: AdminWorkspace) => (
        <StatusBadge status={workspaceTypeLabels[w.workspaceType] ?? "Unknown"} variant={w.workspaceType === 2 ? "info" : "neutral"} />
      ),
    },
    {
      key: "status",
      header: "Status",
      render: (w: AdminWorkspace) => {
        const label = workspaceStatusLabels[w.status] ?? "Unknown";
        const variant = w.status === 0 ? "success" : w.status === 1 ? "warning" : "error";
        return <StatusBadge status={label} variant={variant} />;
      },
    },
    {
      key: "createdAt",
      header: "Created",
      render: (w: AdminWorkspace) => new Date(w.createdAt).toLocaleDateString(),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Workspaces" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Workspaces</h2>
          <p className="text-gray-500 mt-1">{total} total workspaces</p>
        </div>

        {loading ? (
          <div className="space-y-3">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />
            ))}
          </div>
        ) : (
          <>
            <AdminDataTable columns={columns} data={workspaces} keyField="id" />
            <div className="flex items-center justify-between">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50"
              >
                Previous
              </button>
              <span className="text-sm text-gray-500">Page {page}</span>
              <button
                onClick={() => setPage((p) => p + 1)}
                disabled={page * 20 >= total}
                className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50"
              >
                Next
              </button>
            </div>
          </>
        )}
      </main>
    </>
  );
}
