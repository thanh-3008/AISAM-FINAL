"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminUserDetail, deleteUser } from "@/services/adminService";

export default function AdminUserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [user, setUser] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    fetchAdminUserDetail(id).then((data: any) => { setUser(data); setLoading(false); });
  }, [id]);

  if (loading) return (
    <><AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: "Loading..." }]} /><main className="flex-1 p-8"><div className="animate-pulse space-y-4"><div className="h-8 w-64 bg-gray-200 rounded" /></div></main></>
  );

  if (!user) return (
    <><AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: "Not Found" }]} /><main className="flex-1 p-8"><p className="text-gray-500">User not found.</p></main></>
  );

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: user.email }]} />
      <main className="flex-1 p-8 space-y-6">
        {/* User Info */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">User Details</h3>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div><dt className="text-gray-500">Email</dt><dd className="font-medium text-gray-900">{user.email}</dd></div>
            <div><dt className="text-gray-500">Full Name</dt><dd className="font-medium text-gray-900">{user.fullName}</dd></div>
            <div><dt className="text-gray-500">Role</dt><dd><StatusBadge status={user.roleName} variant={user.role === 2 ? "error" : "info"} /></dd></div>
            <div><dt className="text-gray-500">Status</dt><dd><StatusBadge status={user.isEmailVerified ? "Active" : "Inactive"} variant={user.isEmailVerified ? "success" : "warning"} /></dd></div>
            <div><dt className="text-gray-500">Created At</dt><dd className="font-medium text-gray-900">{new Date(user.createdAt).toLocaleDateString()}</dd></div>
            <div><dt className="text-gray-500">Workspaces</dt><dd className="font-medium text-gray-900">{user.workspaceCount ?? 0}</dd></div>
          </dl>
        </div>

        {/* Workspaces */}
        {user.workspaces && user.workspaces.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Workspaces ({user.workspaces.length})</h3>
            <div className="space-y-2">
              {user.workspaces.map((w: any) => (
                <div key={w.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900 text-sm">{w.name}</p>
                    <p className="text-xs text-gray-500">{w.typeName} · {w.status === 0 ? "Active" : "Limited"}</p>
                  </div>
                  <span className="text-xs text-gray-400">{new Date(w.createdAt).toLocaleDateString()}</span>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Sessions */}
        {user.sessions && user.sessions.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Recent Sessions ({user.sessions.length})</h3>
            <div className="space-y-2">
              {user.sessions.map((s: any, i: number) => (
                <div key={i} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg text-xs">
                  <span className="text-gray-500">{new Date(s.createdAt).toLocaleString()}</span>
                  <span className="text-gray-400 truncate max-w-xs ml-4">{s.userAgent || "Unknown"}</span>
                  <StatusBadge status={s.isActive ? "Active" : "Ended"} variant={s.isActive ? "success" : "neutral"} />
                </div>
              ))}
            </div>
          </div>
        )}

        <div className="flex items-center gap-3">
          <button onClick={() => router.push("/admin/users")} className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50">Back to Users</button>
          {user.role !== 2 && (
            <button onClick={async () => { if (!confirm("Are you sure?")) return; await deleteUser(user.id); router.push("/admin/users"); }} className="px-4 py-2 text-sm rounded-lg bg-red-600 text-white hover:bg-red-700">Delete User</button>
          )}
        </div>
      </main>
    </>
  );
}
