"use client";

import { useEffect, useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import { fetchAdminDashboardSummary, fetchAdminAnalyticsCharts, AdminDashboardSummary } from "@/services/adminService";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, Legend } from "recharts";

export default function AdminAnalyticsPage() {
  const [summary, setSummary] = useState<AdminDashboardSummary | null>(null);
  const [charts, setCharts] = useState<{ userRegistrations: any[]; revenue: any[] } | null>(null);

  useEffect(() => {
    fetchAdminDashboardSummary().then(setSummary);
    fetchAdminAnalyticsCharts().then(setCharts);
  }, []);

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Analytics" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-8">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Analytics</h2>
          <p className="text-gray-500 mt-1">Platform-wide metrics and trends.</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <AdminStatsCard title="Total Users" value={summary?.totalUsers ?? 0} icon="group" />
          <AdminStatsCard title="Active Workspaces" value={summary?.totalWorkspaces ?? 0} icon="apartment" />
          <AdminStatsCard title="Total Content" value={summary?.totalContent ?? 0} icon="description" />
          <AdminStatsCard title="Total Revenue" value={`${((summary?.totalRevenue ?? 0)).toLocaleString()} VND`} icon="payments" />
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">New Users (Last 7 Days)</h3>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={charts?.userRegistrations ?? []}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis tick={{ fontSize: 12 }} />
                <Tooltip />
                <Bar dataKey="users" fill="#4f46e5" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Revenue Overview</h3>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={charts?.revenue ?? []}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis tick={{ fontSize: 12 }} tickFormatter={(v) => `${(v / 1000000).toFixed(1)}M`} />
                <Tooltip formatter={(v) => `${Number(v).toLocaleString()} VND`} />
                <Legend />
                <Line type="monotone" dataKey="revenue" stroke="#10b981" strokeWidth={2} dot={{ r: 4 }} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      </main>
    </>
  );
}
