"use client";

import { useEffect, useState, useCallback } from "react";
import { useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import { fetchAdminAuditLogs, AdminAuditLog } from "@/services/adminService";

export default function AdminAuditLogsPage() {
  const router = useRouter();
  const [logs, setLogs] = useState<AdminAuditLog[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const loadLogs = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminAuditLogs(page);
    if (data) {
      setLogs(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page]);

  useEffect(() => { loadLogs(); }, [loadLogs]);

  const columns = [
    {
      key: "actionType",
      header: "Action",
      render: (l: AdminAuditLog) => <span className="font-medium text-gray-900">{l.actionType}</span>,
    },
    {
      key: "target",
      header: "Target",
      render: (l: AdminAuditLog) => <span className="text-gray-500">{l.targetTable} ({l.targetId.substring(0, 8)}...)</span>,
    },
    {
      key: "actorEmail",
      header: "Actor",
      render: (l: AdminAuditLog) => <span className="text-gray-600">{l.actorEmail ?? l.actorId.substring(0, 8)}</span>,
    },
    {
      key: "createdAt",
      header: "Date",
      render: (l: AdminAuditLog) => new Date(l.createdAt).toLocaleString(),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Audit Logs" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Audit Logs</h2>
          <p className="text-gray-500 mt-1">Track admin actions across the platform. {total} entries.</p>
        </div>
        {loading ? (
          <div className="space-y-3">{[...Array(8)].map((_, i) => <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />)}</div>
        ) : logs.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-gray-400">
            <span className="material-symbols-outlined text-5xl mb-4">receipt_long</span>
            <p className="text-lg font-medium text-gray-500">No audit logs</p>
          </div>
        ) : (
          <>
            <AdminDataTable columns={columns} data={logs} keyField="id" onRowClick={(log) => router.push(`/admin/audit-logs/${log.id}`)} />
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
