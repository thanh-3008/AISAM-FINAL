"use client";

import { useEffect, useState, useCallback, useRef } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import AdminTopWorkspaces from "@/components/admin/AdminTopWorkspaces";
import {
  fetchAdminDashboardSummary,
  AdminDashboardSummary,
  fetchAdminAnalyticsCharts,
} from "@/services/adminService";
import {
  AreaChart,
  Area,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from "recharts";

interface DashboardCharts {
  userRegistrations: { name: string; users: number }[];
  revenue: { name: string; revenue: number }[];
  contentCreated: { name: string; content: number }[];
  aiGenerations: { name: string; generations: number }[];
  revenue30Day: { name: string; revenue: number }[];
}

function formatCurrency(n: number): string {
  if (n >= 1_000_000) return `${(n / 1_000_000).toFixed(1)}M`;
  if (n >= 1_000) return `${(n / 1_000).toFixed(0)}K`;
  return n.toLocaleString();
}

const POLL_INTERVAL_MS = 60_000; // 60 seconds

export default function AdminDashboardPage() {
  const [summary, setSummary] = useState<AdminDashboardSummary | null>(null);
  const [charts, setCharts] = useState<DashboardCharts | null>(null);
  const [activeUsers, setActiveUsers] = useState<{ dau: number; mau: number; date: string; month: string } | null>(null);
  const [loading, setLoading] = useState(true);
  const [lastRefresh, setLastRefresh] = useState("");
  const pollingRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const loadData = useCallback(async () => {
    const [sum, ch, au] = await Promise.all([
      fetchAdminDashboardSummary(),
      fetchAdminAnalyticsCharts(),
      import("@/services/adminService").then(m => m.fetchAdminActiveUsers()) // dynamic import to avoid hoisting issues, or just standard call if imported
    ]);
    if (sum) setSummary(sum);
    if (ch) {
      // Normalize keys from BE (PascalCase → camelCase handled by apiClient)
      setCharts({
        userRegistrations: (ch as any).userRegistrations ?? [],
        revenue: (ch as any).revenue ?? [],
        contentCreated: (ch as any).contentCreated ?? [],
        aiGenerations: (ch as any).aiGenerations ?? [],
        revenue30Day: (ch as any).revenue30Day ?? [],
      });
    }
    if (au) setActiveUsers(au);
    setLastRefresh(new Date().toLocaleTimeString());
    setLoading(false);
  }, []);

  // Initial load
  useEffect(() => {
    loadData();
  }, [loadData]);

  // Auto-refresh every 60s
  useEffect(() => {
    pollingRef.current = setInterval(() => {
      loadData();
    }, POLL_INTERVAL_MS);
    return () => {
      if (pollingRef.current) clearInterval(pollingRef.current);
    };
  }, [loadData]);

  const statCards = [
    { title: "Total Users", value: (summary as any)?.totalUsers ?? 0, icon: "group", color: "text-blue-600" },
    { title: "Active Users (DAU/MAU)", value: `${activeUsers?.dau ?? 0} / ${activeUsers?.mau ?? 0}`, icon: "trending_up", color: "text-indigo-600" },
    { title: "Total Workspaces", value: (summary as any)?.totalWorkspaces ?? 0, icon: "apartment", color: "text-violet-600" },
    { title: "Total Content", value: (summary as any)?.totalContent ?? 0, icon: "description", color: "text-emerald-600" },
    {
      title: "Total Revenue",
      value: `${formatCurrency((summary as any)?.totalRevenue ?? 0)} VND`,
      icon: "payments",
      color: "text-amber-600",
    },
    {
      title: "AI Generations",
      value: ((summary as any)?.totalAiGenerations ?? 0).toLocaleString(),
      icon: "smart_toy",
      color: "text-pink-600",
    },
  ];

  return (
    <>
      <AdminHeader title="Dashboard" />
      <main className="flex-1 p-8 overflow-y-auto space-y-8">
        {/* Header row */}
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">System Overview</h2>
            <p className="text-gray-500 mt-1">Monitor key metrics across the platform.</p>
          </div>
          <div className="flex items-center gap-3">
            {lastRefresh && (
              <span className="text-xs text-gray-400">Last updated: {lastRefresh}</span>
            )}
            <button
              onClick={loadData}
              className="flex items-center gap-1.5 px-3 py-1.5 text-sm rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors"
            >
              <span className="material-symbols-outlined text-[16px]">refresh</span>
              Refresh
            </button>
          </div>
        </div>

        {/* Stat Cards */}
        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-6">
            {[...Array(6)].map((_, i) => (
              <div key={i} className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 animate-pulse">
                <div className="h-4 w-24 bg-gray-200 rounded mb-3" />
                <div className="h-8 w-16 bg-gray-200 rounded" />
              </div>
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-6">
            {statCards.map((s) => (
              <AdminStatsCard key={s.title} title={s.title} value={s.value} icon={s.icon} />
            ))}
          </div>
        )}

        {/* Charts Row 1: User Registrations + Revenue (7 days) */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">
              New Users (Last 7 Days)
            </h3>
            {charts ? (
              <ResponsiveContainer width="100%" height={220}>
                <AreaChart data={charts.userRegistrations}>
                  <defs>
                    <linearGradient id="colorUsers" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#4f46e5" stopOpacity={0.2} />
                      <stop offset="95%" stopColor="#4f46e5" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Area
                    type="monotone"
                    dataKey="users"
                    stroke="#4f46e5"
                    strokeWidth={2}
                    fill="url(#colorUsers)"
                    name="New Users"
                  />
                </AreaChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-[220px] animate-pulse bg-gray-100 rounded-lg" />
            )}
          </div>

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">
              Daily Revenue — VND (Last 7 Days)
            </h3>
            {charts ? (
              <ResponsiveContainer width="100%" height={220}>
                <AreaChart data={charts.revenue}>
                  <defs>
                    <linearGradient id="colorRev" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#10b981" stopOpacity={0.2} />
                      <stop offset="95%" stopColor="#10b981" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} tickFormatter={formatCurrency} />
                  <Tooltip formatter={(v: any) => `${formatCurrency(Number(v))} VND`} />
                  <Area
                    type="monotone"
                    dataKey="revenue"
                    stroke="#10b981"
                    strokeWidth={2}
                    fill="url(#colorRev)"
                    name="Revenue"
                  />
                </AreaChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-[220px] animate-pulse bg-gray-100 rounded-lg" />
            )}
          </div>
        </div>

        {/* Charts Row 2: Content Created + AI Generations */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">
              Content Created (Last 7 Days)
            </h3>
            {charts ? (
              <ResponsiveContainer width="100%" height={220}>
                <BarChart data={charts.contentCreated}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Bar dataKey="content" fill="#f59e0b" radius={[4, 4, 0, 0]} name="Content" />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-[220px] animate-pulse bg-gray-100 rounded-lg" />
            )}
          </div>

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">
              AI Generations (Last 7 Days)
            </h3>
            {charts ? (
              <ResponsiveContainer width="100%" height={220}>
                <BarChart data={charts.aiGenerations}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Bar dataKey="generations" fill="#8b5cf6" radius={[4, 4, 0, 0]} name="AI Generations" />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-[220px] animate-pulse bg-gray-100 rounded-lg" />
            )}
          </div>
        </div>

        {/* Revenue 30 days trend */}
        {charts && charts.revenue30Day.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">
              Revenue Trend — Last 30 Days (VND)
            </h3>
            <ResponsiveContainer width="100%" height={200}>
              <AreaChart data={charts.revenue30Day}>
                <defs>
                  <linearGradient id="colorRev30" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#4f46e5" stopOpacity={0.15} />
                    <stop offset="95%" stopColor="#4f46e5" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 10 }} interval={4} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={formatCurrency} />
                <Tooltip formatter={(v: any) => `${formatCurrency(Number(v))} VND`} />
                <Area
                  type="monotone"
                  dataKey="revenue"
                  stroke="#4f46e5"
                  strokeWidth={2}
                  fill="url(#colorRev30)"
                  name="Revenue"
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        )}

        {/* Quick Actions + System Info */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Quick Actions</h3>
            <div className="grid grid-cols-2 gap-3">
              <a href="/admin/users" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-blue-600">group_add</span>
                Manage Users
              </a>
              <a href="/admin/workspaces" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-violet-600">apartment</span>
                Manage Workspaces
              </a>
              <a href="/admin/payments" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-emerald-600">receipt_long</span>
                View Payments
              </a>
              <a href="/admin/analytics" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-amber-600">analytics</span>
                Analytics
              </a>
              <a href="/admin/content" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-pink-600">flag</span>
                Content Moderation
              </a>
              <a href="/admin/settings" className="flex items-center gap-2 p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors text-sm">
                <span className="material-symbols-outlined text-gray-600">settings</span>
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
              <div className="flex justify-between">
                <span className="text-gray-500">Auto-refresh</span>
                <span className="font-medium text-emerald-600">Every 60s ●</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Last Refresh</span>
                <span className="font-medium">{lastRefresh || "—"}</span>
              </div>
            </div>
          </div>
        </div>

        <AdminTopWorkspaces />
      </main>
    </>
  );
}
