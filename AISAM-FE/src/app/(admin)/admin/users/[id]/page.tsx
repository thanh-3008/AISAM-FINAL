"use client";

import { useParams } from "next/navigation";
import { motion } from "motion/react";
import { useAdminUserDetail } from "@/hooks/admin/useAdminUserDetail";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";
import AdminEmptyState from "@/components/admin/AdminEmptyState";

export default function AdminUserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data, isLoading } = useAdminUserDetail(id);

  if (isLoading) return <div className="space-y-4 animate-pulse"><div className="h-16 bg-surface-container rounded-2xl" /></div>;
  if (!data) return <p className="text-danger-red">Failed to load user.</p>;

  return (
    <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }} className="space-y-6">
      <div className="flex items-center gap-4">
        <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center">
          <span className="text-xl font-bold text-primary">{data.fullName?.charAt(0)?.toUpperCase() || data.email.charAt(0).toUpperCase()}</span>
        </div>
        <div>
          <h1 className="text-headline-sm text-on-surface">{data.fullName || data.email}</h1>
          <p className="text-body-sm text-on-surface-variant">{data.email} · <AdminStatusBadge status={data.role} /></p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-surface-container-low border border-outline-variant/10 rounded-2xl p-4">
          <p className="text-label-xs text-on-surface-variant uppercase font-semibold">Email Verified</p>
          <p className="text-body-lg font-semibold mt-1">{data.isEmailVerified ? "Yes" : "No"}</p>
        </div>
        <div className="bg-surface-container-low border border-outline-variant/10 rounded-2xl p-4">
          <p className="text-label-xs text-on-surface-variant uppercase font-semibold">Joined</p>
          <p className="text-body-lg font-semibold mt-1">{new Date(data.createdAt).toLocaleDateString()}</p>
        </div>
        <div className="bg-surface-container-low border border-outline-variant/10 rounded-2xl p-4">
          <p className="text-label-xs text-on-surface-variant uppercase font-semibold">Last Login</p>
          <p className="text-body-lg font-semibold mt-1">{data.lastLoginAt ? new Date(data.lastLoginAt).toLocaleDateString() : "Never"}</p>
        </div>
      </div>

      <section className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
        <h2 className="text-headline-sm text-on-surface mb-4">Workspaces</h2>
        {data.workspaces.length === 0 ? (
          <AdminEmptyState message="No workspaces." />
        ) : (
          <ul className="divide-y divide-outline-variant/10">
            {data.workspaces.map((w) => (
              <li key={w.id} className="py-3 flex items-center justify-between">
                <div>
                  <p className="text-body-sm font-medium text-on-surface">{w.name}</p>
                  <p className="text-label-3xs text-on-surface-variant">{w.type} · {w.role}</p>
                </div>
                <AdminStatusBadge status={w.status} />
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
        <h2 className="text-headline-sm text-on-surface mb-4">Recent Payments</h2>
        {data.payments.length === 0 ? (
          <AdminEmptyState message="No payments." />
        ) : (
          <ul className="divide-y divide-outline-variant/10">
            {data.payments.map((p) => (
              <li key={p.id} className="py-3 flex items-center justify-between">
                <span className="text-body-sm text-on-surface">{(p.amount / 1000).toFixed(0)}K {p.currency}</span>
                <div className="flex items-center gap-3">
                  <span className="text-label-3xs text-on-surface-variant">{new Date(p.createdAt).toLocaleDateString()}</span>
                  <AdminStatusBadge status={p.status} />
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </motion.div>
  );
}
