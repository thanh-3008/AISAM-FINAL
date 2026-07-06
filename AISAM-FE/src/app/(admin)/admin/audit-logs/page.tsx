"use client";

import { useEffect, useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import { apiClient } from "@/lib/apiClient";

interface AuditLog {
  id: string;
  actorId: string;
  actionType: string;
  targetTable: string;
  targetId: string;
  createdAt: string;
  notes?: string;
}

export default function AdminAuditLogsPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    apiClient("/admin/audit-logs")
      .then((res: any) => {
        setLogs(res?.data?.items ?? []);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, []);

  const columns = [
    {
      key: "actionType",
      header: "Action",
      render: (l: AuditLog) => <span className="font-medium text-gray-900">{l.actionType}</span>,
    },
    {
      key: "targetTable",
      header: "Target",
      render: (l: AuditLog) => <span className="text-gray-500">{l.targetTable}#{l.targetId.substring(0, 8)}</span>,
    },
    {
      key: "notes",
      header: "Notes",
      render: (l: AuditLog) => <span className="text-gray-500">{l.notes || "-"}</span>,
    },
    {
      key: "createdAt",
      header: "Date",
      render: (l: AuditLog) => new Date(l.createdAt).toLocaleString(),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Audit Logs" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Audit Logs</h2>
          <p className="text-gray-500 mt-1">Track admin actions across the platform.</p>
        </div>

        {loading ? (
          <div className="space-y-3">
            {[...Array(8)].map((_, i) => <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />)}
          </div>
        ) : (
          <AdminDataTable columns={columns} data={logs} keyField="id" />
        )}
      </main>
    </>
  );
}
