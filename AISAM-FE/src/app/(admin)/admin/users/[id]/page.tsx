"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminUserDetail, deleteUser, AdminUser } from "@/services/adminService";

export default function AdminUserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [user, setUser] = useState<AdminUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    fetchAdminUserDetail(id).then((data) => {
      setUser(data);
      setLoading(false);
    });
  }, [id]);

  if (loading) {
    return (
      <>
        <AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: "Loading..." }]} />
        <main className="flex-1 p-8">
          <div className="animate-pulse space-y-4">
            <div className="h-8 w-64 bg-gray-200 rounded" />
            <div className="h-4 w-96 bg-gray-200 rounded" />
          </div>
        </main>
      </>
    );
  }

  if (!user) {
    return (
      <>
        <AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: "Not Found" }]} />
        <main className="flex-1 p-8">
          <p className="text-gray-500">User not found.</p>
        </main>
      </>
    );
  }

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: user.email }]} />
      <main className="flex-1 p-8 space-y-6">
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">User Details</h3>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div>
              <dt className="text-gray-500">Email</dt>
              <dd className="font-medium text-gray-900">{user.email}</dd>
            </div>
            <div>
              <dt className="text-gray-500">Full Name</dt>
              <dd className="font-medium text-gray-900">{user.fullName}</dd>
            </div>
            <div>
              <dt className="text-gray-500">Role</dt>
              <dd><StatusBadge status={user.roleName} variant={user.role === 2 ? "error" : "info"} /></dd>
            </div>
            <div>
              <dt className="text-gray-500">Status</dt>
              <dd><StatusBadge status={user.isEmailVerified ? "Active" : "Inactive"} variant={user.isEmailVerified ? "success" : "warning"} /></dd>
            </div>
            <div>
              <dt className="text-gray-500">Created At</dt>
              <dd className="font-medium text-gray-900">{new Date(user.createdAt).toLocaleDateString()}</dd>
            </div>
          </dl>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => router.push("/admin/users")}
            className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
          >
            Back to Users
          </button>
          {user.role !== 2 && (
            <button
              onClick={async () => {
                if (!confirm("Are you sure?")) return;
                await deleteUser(user.id);
                router.push("/admin/users");
              }}
              className="px-4 py-2 text-sm rounded-lg bg-red-600 text-white hover:bg-red-700 transition-colors"
            >
              Delete User
            </button>
          )}
        </div>
      </main>
    </>
  );
}
