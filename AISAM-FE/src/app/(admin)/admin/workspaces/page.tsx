"use client";

import { useState } from "react";
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
    { key: "owner", header: "Owner", render: (w: WorkspaceItem) => <span className="text-sm">{w.ownerEmail}</span> },
    { key: "credits", header: "Credits", render: (w: WorkspaceItem) => w.creditBalance.toLocaleString() },
    { key: "created", header: "Created", render: (w: WorkspaceItem) => new Date(w.createdAt).toLocaleDateString() },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-[#191b24]">Workspaces</h1>
      <input type="text" placeholder="Search by name..." value={searchTerm}
        onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }}
        className="w-full max-w-sm px-4 py-2 rounded-xl border border-gray-200 bg-white text-sm focus:outline-none focus:border-[#004ccd]" />
      <div className="bg-white border border-gray-200 rounded-2xl overflow-hidden">
        <AdminDataTable columns={columns} data={data?.data || []}
          totalCount={data?.totalCount || 0} page={page} pageSize={10}
          totalPages={data?.totalPages || 1} onPageChange={setPage} isLoading={isLoading}
          emptyMessage="No workspaces found." />
      </div>
    </div>
  );
}
