"use client";

import { useEffect, useState, useCallback } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminContent, setContentStatus, deleteContent, AdminContent } from "@/services/adminService";

const contentStatusLabels: Record<number, string> = { 0: "Draft", 1: "Pending", 2: "Approved", 3: "Rejected", 4: "Published" };

export default function AdminContentPage() {
  const [contents, setContents] = useState<AdminContent[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const loadContent = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminContent(page);
    if (data) {
      setContents(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page]);

  useEffect(() => { loadContent(); }, [loadContent]);

  const handleStatusChange = async (id: string, currentStatus: number) => {
    const newStatus = currentStatus === 3 ? 2 : 3;
    const ok = await setContentStatus(id, newStatus);
    if (ok) loadContent();
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this content? This cannot be undone.")) return;
    const ok = await deleteContent(id);
    if (ok) loadContent();
  };

  const columns = [
    { key: "title", header: "Title" },
    {
      key: "isAiGenerated",
      header: "AI Generated",
      render: (c: AdminContent) => <StatusBadge status={c.isAiGenerated ? "Yes" : "No"} variant={c.isAiGenerated ? "info" : "neutral"} />,
    },
    {
      key: "status",
      header: "Status",
      render: (c: AdminContent) => {
        const label = contentStatusLabels[c.status] ?? "Unknown";
        const variant = c.status === 2 || c.status === 4 ? "success" : c.status === 1 ? "warning" : c.status === 3 ? "error" : "neutral";
        return <StatusBadge status={label} variant={variant} />;
      },
    },
    {
      key: "createdAt",
      header: "Created",
      render: (c: AdminContent) => new Date(c.createdAt).toLocaleDateString(),
    },
    {
      key: "actions",
      header: "Actions",
      render: (c: AdminContent) => (
        <div className="flex items-center gap-2">
          <button
            onClick={(e) => { e.stopPropagation(); handleStatusChange(c.id, c.status); }}
            className="text-xs px-2 py-1 rounded bg-gray-100 hover:bg-gray-200 text-gray-700 transition-colors"
          >
            {c.status === 3 ? "Approve" : "Reject"}
          </button>
          <button
            onClick={(e) => { e.stopPropagation(); handleDelete(c.id); }}
            className="text-xs px-2 py-1 rounded bg-red-50 hover:bg-red-100 text-red-600 transition-colors"
          >
            Delete
          </button>
        </div>
      ),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Content" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Content</h2>
          <p className="text-gray-500 mt-1">{total} total content items</p>
        </div>
        {loading ? (
          <div className="space-y-3">{[...Array(5)].map((_, i) => <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />)}</div>
        ) : (
          <>
            <AdminDataTable columns={columns} data={contents} keyField="id" />
            <div className="flex items-center justify-between">
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Previous</button>
              <span className="text-sm text-gray-500">Page {page}</span>
              <button onClick={() => setPage((p) => p + 1)} disabled={page * 20 >= total} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Next</button>
            </div>
          </>
        )}
      </main>
    </>
  );
}
