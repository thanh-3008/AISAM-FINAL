"use client";

import { useEffect, useState, useCallback } from "react";
import { useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminUsers, setUserStatus, deleteUser, setUserRole, AdminUser } from "@/services/adminService";

export default function AdminUsersPage() {
  const router = useRouter();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const loadUsers = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminUsers(page);
    if (data) {
      setUsers(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page]);

  useEffect(() => { loadUsers(); }, [loadUsers]);

  const handleToggleStatus = async (userId: string, currentActive: boolean) => {
    const ok = await setUserStatus(userId, !currentActive);
    if (ok) loadUsers();
  };

  const handleDelete = async (userId: string) => {
    if (!confirm("Are you sure you want to delete this user? This action cannot be undone.")) return;
    const ok = await deleteUser(userId);
    if (ok) loadUsers();
  };

  const handleRoleChange = async (userId: string, currentRole: number) => {
    const newRole = currentRole === 2 ? 0 : 2;
    const label = newRole === 2 ? "Admin" : "User";
    if (!confirm(`Change this user's role to ${label}?`)) return;
    const ok = await setUserRole(userId, newRole);
    if (ok) loadUsers();
  };

  const columns = [
    { key: "email", header: "Email" },
    { key: "fullName", header: "Name" },
    {
      key: "role",
      header: "Role",
      render: (u: AdminUser) => <StatusBadge status={u.roleName} variant={u.role === 2 ? "error" : "info"} />,
    },
    {
      key: "status",
      header: "Status",
      render: (u: AdminUser) => (
        <StatusBadge status={u.isEmailVerified ? "Active" : "Inactive"} variant={u.isEmailVerified ? "success" : "warning"} />
      ),
    },
    {
      key: "actions",
      header: "Actions",
      render: (u: AdminUser) => (
        <div className="flex items-center gap-2">
          <button
            onClick={(e) => { e.stopPropagation(); handleToggleStatus(u.id, u.isEmailVerified); }}
            className="text-xs px-2 py-1 rounded bg-gray-100 hover:bg-gray-200 text-gray-700 transition-colors"
          >
            {u.isEmailVerified ? "Deactivate" : "Activate"}
          </button>
          <button
            onClick={(e) => { e.stopPropagation(); handleRoleChange(u.id, u.role); }}
            className="text-xs px-2 py-1 rounded bg-blue-50 hover:bg-blue-100 text-blue-600 transition-colors"
          >
            {u.role === 2 ? "Demote" : "Promote"}
          </button>
          {u.role !== 2 && (
            <button
              onClick={(e) => { e.stopPropagation(); handleDelete(u.id); }}
              className="text-xs px-2 py-1 rounded bg-red-50 hover:bg-red-100 text-red-600 transition-colors"
            >
              Delete
            </button>
          )}
        </div>
      ),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Users" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Users</h2>
            <p className="text-gray-500 mt-1">{total} total users</p>
          </div>
        </div>

        {loading ? (
          <div className="space-y-3">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />
            ))}
          </div>
        ) : users.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-gray-400">
            <span className="material-symbols-outlined text-5xl mb-4">person_off</span>
            <p className="text-lg font-medium text-gray-500">No users found</p>
          </div>
        ) : (
          <>
            <AdminDataTable
              columns={columns}
              data={users}
              keyField="id"
              onRowClick={(user) => router.push(`/admin/users/${user.id}`)}
            />
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
