"use client";

import { useEffect, useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import AdminTopWorkspaces from "@/components/admin/AdminTopWorkspaces";
import { fetchAdminDashboardSummary, AdminDashboardSummary } from "@/services/adminService";

export default function AdminDashboardPage() {
  const [summary, setSummary] = useState<AdminDashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchAdminDashboardSummary().then((data) => {
      setSummary(data);
      setLoading(false);
    });
  }, []);

  return (
    <>
      <AdminHeader title="Dashboard" />
      <main className="flex-1 p-8 overflow-y-auto space-y-8">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">System Overview</h2>
          <p className="text-gray-500 mt-1">Monitor key metrics across the platform.</p>
        </div>

        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {[...Array(4)].map((_, i) => (
              <div key={i} className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 animate-pulse">
                <div className="h-4 w-24 bg-gray-200 rounded mb-3" />
                <div className="h-8 w-16 bg-gray-200 rounded" />
              </div>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <AdminStatsCard title="Total Users" value={summary?.totalUsers ?? 0} icon="group" />
            <AdminStatsCard title="Total Workspaces" value={summary?.totalWorkspaces ?? 0} icon="apartment" />
            <AdminStatsCard title="Total Content" value={summary?.totalContent ?? 0} icon="description" />
            <AdminStatsCard title="Total Revenue" value={`${(summary?.totalRevenue ?? 0).toLocaleString()} VND`} icon="payments" />
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Quick Actions</h3>
            <div className="grid grid-cols-2 gap-3">
              <a href="/admin/users" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-blue-600">group_add</span>
                Manage Users
              </a>
              <a href="/admin/workspaces" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-blue-600">apartment</span>
                Manage Workspaces
              </a>
              <a href="/admin/payments" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-blue-600">receipt_long</span>
                View Payments
              </a>
              <a href="/admin/settings" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-blue-600">settings</span>
                System Settings
              </a>
            </div>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">System Info</h3>
            <div className="space-y-3 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-500">Admin Role</span>
                <span className="font-medium">Super Admin</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Access Level</span>
                <span className="font-medium">Full System Access</span>
              </div>
            </div>
          </div>
        </div>

        <AdminTopWorkspaces />
      </main>
    </>
  );
}
