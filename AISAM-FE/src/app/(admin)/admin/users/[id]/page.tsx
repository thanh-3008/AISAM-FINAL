"use client";

import { useParams } from "next/navigation";
import { useAdminUserDetail } from "@/hooks/admin/useAdminUserDetail";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";
import AdminEmptyState from "@/components/admin/AdminEmptyState";

export default function AdminUserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data, isLoading } = useAdminUserDetail(id);

  if (isLoading) return <div className="space-y-4 animate-pulse"><div className="h-16 bg-gray-100 rounded-2xl" /></div>;
  if (!data) return <p className="text-[#DA1E28]">Failed to load user.</p>;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <div className="w-16 h-16 rounded-full bg-[#004ccd]/10 flex items-center justify-center">
          <span className="text-xl font-bold text-[#004ccd]">{data.fullName?.charAt(0)?.toUpperCase() || data.email.charAt(0).toUpperCase()}</span>
        </div>
        <div>
          <h1 className="text-2xl font-bold text-[#191b24]">{data.fullName || data.email}</h1>
          <p className="text-sm text-[#424656]">{data.email} · <AdminStatusBadge status={data.role} /></p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white border border-gray-200 rounded-2xl p-4">
          <p className="text-xs text-[#424656] uppercase font-semibold">Email Verified</p>
          <p className="text-lg font-semibold mt-1">{data.isEmailVerified ? "Yes" : "No"}</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-2xl p-4">
          <p className="text-xs text-[#424656] uppercase font-semibold">Joined</p>
          <p className="text-lg font-semibold mt-1">{new Date(data.createdAt).toLocaleDateString()}</p>
        </div>
        <div className="bg-white border border-gray-200 rounded-2xl p-4">
          <p className="text-xs text-[#424656] uppercase font-semibold">Last Login</p>
          <p className="text-lg font-semibold mt-1">{data.lastLoginAt ? new Date(data.lastLoginAt).toLocaleDateString() : "Never"}</p>
        </div>
      </div>

      <section className="bg-white border border-gray-200 rounded-2xl p-6">
        <h2 className="text-lg font-semibold text-[#191b24] mb-4">Workspaces</h2>
        {data.workspaces.length === 0 ? (
          <AdminEmptyState message="No workspaces." />
        ) : (
          <ul className="divide-y divide-gray-200">
            {data.workspaces.map((w) => (
              <li key={w.id} className="py-3 flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-[#191b24]">{w.name}</p>
                  <p className="text-[11px] text-[#424656]">{w.type} · {w.role}</p>
                </div>
                <AdminStatusBadge status={w.status} />
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="bg-white border border-gray-200 rounded-2xl p-6">
        <h2 className="text-lg font-semibold text-[#191b24] mb-4">Recent Payments</h2>
        {data.payments.length === 0 ? (
          <AdminEmptyState message="No payments." />
        ) : (
          <ul className="divide-y divide-gray-200">
            {data.payments.map((p) => (
              <li key={p.id} className="py-3 flex items-center justify-between">
                <span className="text-sm text-[#191b24]">{(p.amount / 1000).toFixed(0)}K {p.currency}</span>
                <div className="flex items-center gap-3">
                  <span className="text-[11px] text-[#424656]">{new Date(p.createdAt).toLocaleDateString()}</span>
                  <AdminStatusBadge status={p.status} />
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
