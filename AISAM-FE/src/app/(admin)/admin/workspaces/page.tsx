"use client";

import { useState } from "react";
import { motion } from "motion/react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import AdminDataTable from "@/components/admin/AdminDataTable";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";

interface WorkspaceItem { id: string; name: string; type: string; status: string; plan: string; memberCount: number; ownerEmail: string; creditBalance: number; createdAt: string; }

export default function AdminWorkspacesPage() {
  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["admin", "workspaces", page, searchTerm],
    queryFn: async () => {
      const q = new URLSearchParams({ page: String(page), pageSize: "10" });
      if (searchTerm) q.set("searchTerm", searchTerm);
      const res = await apiClient(`/admin/workspaces?${q.toString()}`);
      return res.data as { data: WorkspaceItem[]; totalCount: number; totalPages: number };
    },
  });

  const columns = [
    { key: "name", header: "Name", render: (w: WorkspaceItem) => <span className="font-medium">{w.name}</span> },
    { key: "type", header: "Type", render: (w: WorkspaceItem) => <AdminStatusBadge status={w.type} /> },
    { key: "status", header: "Status", render: (w: WorkspaceItem) => <AdminStatusBadge status={w.status} /> },
    { key: "plan", header: "Plan", render: (w: WorkspaceItem) => <AdminStatusBadge status={w.plan} /> },
    { key: "members", header: "Members", render: (w: WorkspaceItem) => w.memberCount },
    { key: "owner", header: "Owner", render: (w: WorkspaceItem) => <span className="text-body-sm">{w.ownerEmail}</span> },
    { key: "credits", header: "Credits", render: (w: WorkspaceItem) => w.creditBalance.toLocaleString() },
    { key: "created", header: "Created", render: (w: WorkspaceItem) => new Date(w.createdAt).toLocaleDateString() },
  ];

  return (
    <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }} className="space-y-6">
      <h1 className="text-headline-sm text-on-surface">Workspaces</h1>
      <input type="text" placeholder="Search by name..." value={searchTerm}
        onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }}
        className="w-full max-w-sm px-4 py-2 rounded-xl border border-outline-variant/30 bg-surface-container-lowest text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30" />
      <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl overflow-hidden">
        <AdminDataTable columns={columns} data={data?.data || []}
          totalCount={data?.totalCount || 0} page={page} pageSize={10}
          totalPages={data?.totalPages || 1} onPageChange={setPage} isLoading={isLoading}
          emptyMessage="No workspaces found." />
      </div>
    </motion.div>
  );
}
