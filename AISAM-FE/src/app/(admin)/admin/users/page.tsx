"use client";

import { useState } from "react";
import Link from "next/link";
import { motion } from "motion/react";
import { useAdminUsers } from "@/hooks/admin/useAdminUsers";
import AdminDataTable from "@/components/admin/AdminDataTable";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";
import type { AdminUserListItem } from "@/services/adminService";

export default function AdminUsersPage() {
  const [page, setPage] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");

  const { data, isLoading } = useAdminUsers({ page, pageSize: 10, searchTerm: searchTerm || undefined });

  const columns = [
    { key: "email", header: "Email", render: (u: AdminUserListItem) => <span className="font-medium">{u.email}</span> },
    { key: "fullName", header: "Name", render: (u: AdminUserListItem) => u.fullName || "-" },
    { key: "role", header: "Role", render: (u: AdminUserListItem) => <AdminStatusBadge status={u.role} /> },
    { key: "verified", header: "Verified", render: (u: AdminUserListItem) => u.isEmailVerified ? "Yes" : "No" },
    { key: "workspaces", header: "Workspaces", render: (u: AdminUserListItem) => u.workspaceCount },
    { key: "createdAt", header: "Joined", render: (u: AdminUserListItem) => new Date(u.createdAt).toLocaleDateString() },
    {
      key: "actions", header: "", render: (u: AdminUserListItem) => (
        <Link href={`/admin/users/${u.id}`} className="text-primary hover:underline text-body-sm">View</Link>
      ),
    },
  ];

  return (
    <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }} className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-headline-sm text-on-surface">Users</h1>
      </div>

      <input
        type="text"
        placeholder="Search by email or name..."
        value={searchTerm}
        onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }}
        className="w-full max-w-sm px-4 py-2 rounded-xl border border-outline-variant/30 bg-surface-container-lowest text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30"
      />

      <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl overflow-hidden">
        <AdminDataTable
          columns={columns}
          data={data?.data || []}
          totalCount={data?.totalCount || 0}
          page={page}
          pageSize={10}
          totalPages={data?.totalPages || 1}
          onPageChange={setPage}
          isLoading={isLoading}
          emptyMessage="No users found."
        />
      </div>
    </motion.div>
  );
}
