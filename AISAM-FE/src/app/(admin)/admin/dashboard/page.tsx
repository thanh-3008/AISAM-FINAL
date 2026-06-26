"use client";

import { useAdminDashboard } from "@/hooks/admin/useAdminDashboard";
import AdminStatsCard from "@/components/admin/AdminStatsCard";

export default function AdminDashboardPage() {
  const { data, isLoading } = useAdminDashboard();

  if (isLoading) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-bold text-[#191b24]">Dashboard</h1>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-28 bg-gray-100 rounded-2xl animate-pulse" />
          ))}
        </div>
      </div>
    );
  }

  if (!data) {
    return <p className="text-[#DA1E28]">Failed to load dashboard.</p>;
  }

  const stats = [
    { label: "Total Users", value: data.totalUsers, icon: "group" },
    { label: "Active (30d)", value: data.activeUsers, icon: "person_check" },
    { label: "Workspaces", value: data.totalWorkspaces, icon: "workspaces" },
    { label: "Active Subs", value: data.activeSubscriptions, icon: "subscriptions" },
    { label: "Revenue", value: `${(data.totalRevenue / 1000).toFixed(0)}K VND`, icon: "payments" },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-[#191b24]">Dashboard</h1>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
        {stats.map((s) => (
          <AdminStatsCard key={s.label} {...s} />
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <section className="bg-white border border-gray-200 rounded-2xl p-6">
          <h2 className="text-lg font-semibold text-[#191b24] mb-4">Recent Users</h2>
          <ul className="space-y-2">
            {data.recentUsers.map((u) => (
              <li key={u.id} className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
                <div>
                  <p className="text-sm font-medium text-[#191b24]">{u.fullName || u.email}</p>
                  <p className="text-[11px] text-[#424656]">{u.email}</p>
                </div>
                <span className="text-[11px] text-[#424656]">{new Date(u.createdAt).toLocaleDateString()}</span>
              </li>
            ))}
          </ul>
        </section>

        <section className="bg-white border border-gray-200 rounded-2xl p-6">
          <h2 className="text-lg font-semibold text-[#191b24] mb-4">Recent Payments</h2>
          <ul className="space-y-2">
            {data.recentPayments.map((p) => (
              <li key={p.id} className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
                <div>
                  <p className="text-sm font-medium text-[#191b24]">{p.userEmail}</p>
                  <p className="text-[11px] text-[#424656]">{(p.amount / 1000).toFixed(0)}K {p.currency}</p>
                </div>
                <span className={`text-[11px] font-semibold px-2 py-0.5 rounded-full ${
                  p.status === "Success" ? "bg-[#198038]/10 text-[#198038]" : "bg-gray-100 text-[#424656]"
                }`}>{p.status}</span>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </div>
  );
}
