"use client";

import { useState } from "react";
import { motion } from "motion/react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import AdminDataTable from "@/components/admin/AdminDataTable";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";

interface SubItem { id: string; workspaceName: string; plan: string; isActive: boolean; startDate: string; endDate?: string; createdAt: string; }

export default function AdminSubscriptionsPage() {
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ["admin", "subscriptions", page],
    queryFn: async () => {
      const res = await apiClient(`/admin/subscriptions?page=${page}&pageSize=10`);
      return res.data as { data: SubItem[]; totalCount: number; totalPages: number };
    },
  });

  const columns = [
    { key: "workspace", header: "Workspace", render: (s: SubItem) => <span className="font-medium">{s.workspaceName}</span> },
    { key: "plan", header: "Plan", render: (s: SubItem) => <AdminStatusBadge status={s.plan} /> },
    { key: "active", header: "Active", render: (s: SubItem) => <AdminStatusBadge status={s.isActive ? "Active" : "Inactive"} /> },
    { key: "start", header: "Start", render: (s: SubItem) => new Date(s.startDate).toLocaleDateString() },
    { key: "end", header: "End", render: (s: SubItem) => s.endDate ? new Date(s.endDate).toLocaleDateString() : "-" },
    { key: "created", header: "Created", render: (s: SubItem) => new Date(s.createdAt).toLocaleDateString() },
  ];

  return (
    <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }} className="space-y-6">
      <h1 className="text-headline-sm text-on-surface">Subscriptions</h1>
      <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl overflow-hidden">
        <AdminDataTable columns={columns} data={data?.data || []}
          totalCount={data?.totalCount || 0} page={page} pageSize={10}
          totalPages={data?.totalPages || 1} onPageChange={setPage} isLoading={isLoading}
          emptyMessage="No subscriptions found." />
      </div>
    </motion.div>
  );
}
