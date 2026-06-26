"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import AdminDataTable from "@/components/admin/AdminDataTable";

interface LogItem { id: string; actorEmail?: string; action?: string; targetTable?: string; targetId?: string; createdAt: string; }

export default function AdminAuditLogsPage() {
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ["admin", "audit-logs", page],
    queryFn: async () => {
      const res = await apiClient(`/admin/audit-logs?page=${page}&pageSize=20`);
      return res.data as { data: LogItem[]; totalCount: number; totalPages: number };
    },
  });

  const columns = [
    { key: "actor", header: "Actor", render: (l: LogItem) => <span className="font-medium">{l.actorEmail || "System"}</span> },
    { key: "action", header: "Action", render: (l: LogItem) => l.action || "-" },
    { key: "table", header: "Table", render: (l: LogItem) => l.targetTable || "-" },
    { key: "target", header: "Target", render: (l: LogItem) => l.targetId ? l.targetId.substring(0, 8) + "..." : "-" },
    { key: "created", header: "Date", render: (l: LogItem) => new Date(l.createdAt).toLocaleString() },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-[#191b24]">Audit Logs</h1>
      <div className="bg-white border border-gray-200 rounded-2xl overflow-hidden">
        <AdminDataTable columns={columns} data={data?.data || []}
          totalCount={data?.totalCount || 0} page={page} pageSize={20}
          totalPages={data?.totalPages || 1} onPageChange={setPage} isLoading={isLoading}
          emptyMessage="No audit logs found." />
      </div>
    </div>
  );
}
