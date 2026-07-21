"use client";

import { useEffect, useState, useCallback } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminAnalyticsOverview, fetchAdminWorkspaceComparison, getAdminExportUrl, AdminAnalyticsOverview, WorkspaceComparisonItem } from "@/services/adminService";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, Legend, PieChart, Pie, Cell } from "recharts";

const COLORS = ["#4f46e5", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#06b6d4", "#f97316", "#ec4899"];

function formatNumber(n: number): string {
  if (n >= 1000000) return `${(n / 1000000).toFixed(1)}M`;
  if (n >= 1000) return `${(n / 1000).toFixed(1)}K`;
  return n.toLocaleString();
}

function formatCurrency(n: number): string {
  if (n >= 1000000000) return `${(n / 1000000000).toFixed(1)}B VND`;
  if (n >= 1000000) return `${(n / 1000000).toFixed(1)}M VND`;
  if (n >= 1000) return `${(n / 1000).toFixed(0)}K VND`;
  return `${n.toLocaleString()} VND`;
}

export default function AdminAnalyticsPage() {
  const [overview, setOverview] = useState<AdminAnalyticsOverview | null>(null);
  const [workspaces, setWorkspaces] = useState<WorkspaceComparisonItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [dateRange, setDateRange] = useState("30");

  const [topWorkspacesLimit, setTopWorkspacesLimit] = useState(10);
  const [spendRevLimit, setSpendRevLimit] = useState(10);
  const [roasLimit, setRoasLimit] = useState(10);
  const [engagementLimit, setEngagementLimit] = useState(10);
  const [wsTableLimit, setWsTableLimit] = useState(20);
  const [campaignsTableLimit, setCampaignsTableLimit] = useState(20);

  const [campaigns, setCampaigns] = useState<AdminAnalyticsOverview["topCampaigns"]>([]);

  const loadData = useCallback(async () => {
    setLoading(true);
    const now = new Date();
    const from = new Date(now.getTime() - parseInt(dateRange) * 86400000).toISOString();
    const to = now.toISOString();
    
    const [ov, ws, camps] = await Promise.all([
      fetchAdminAnalyticsOverview(from, to),
      fetchAdminWorkspaceComparison(from, to, 100),
      import("@/services/adminService").then(m => m.fetchAdminTopCampaigns(from, to, 100))
    ]);
    
    setOverview(ov);
    setWorkspaces(ws ?? []);
    setCampaigns(camps ?? []);
    setLoading(false);
  }, [dateRange]);

  useEffect(() => { loadData(); }, [loadData]);

  const handleExport = () => {
    const now = new Date();
    const from = new Date(now.getTime() - parseInt(dateRange) * 86400000).toISOString();
    const to = now.toISOString();
    const token = localStorage.getItem("aisam_token");
    const url = getAdminExportUrl(from, to);
    fetch(url, { headers: { Authorization: `Bearer ${token}` } })
      .then((r) => r.blob())
      .then((blob) => {
        const a = document.createElement("a");
        a.href = URL.createObjectURL(blob);
        a.download = `admin-report-${new Date().toISOString().split("T")[0]}.csv`;
        a.click();
      });
  };

  if (loading) {
    return (
      <>
        <AdminHeader breadcrumbs={[{ label: "Analytics" }]} />
        <main className="flex-1 flex flex-col items-center justify-center p-8 min-h-[500px]">
          <div className="flex flex-col items-center gap-4 bg-white p-8 rounded-2xl shadow-sm border border-gray-100 max-w-sm w-full text-center">
            <div className="relative">
              <div className="w-12 h-12 rounded-full border-4 border-blue-100 border-t-blue-600 animate-spin"></div>
              <div className="absolute inset-0 flex items-center justify-center">
                <span className="material-symbols-outlined text-blue-600 text-[20px]">analytics</span>
              </div>
            </div>
            <div>
              <h3 className="text-gray-900 font-semibold mb-1">Đang tải dữ liệu báo cáo...</h3>
              <p className="text-sm text-gray-500">Đang đồng bộ và chờ phản hồi từ Meta. Vui lòng đợi trong giây lát.</p>
            </div>
          </div>
        </main>
      </>
    );
  }

  const t = overview?.totals;
  const sys = overview?.systemStats;

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Analytics" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-8">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Reports & Analytics</h2>
            <p className="text-gray-500 mt-1">Platform-wide advertising performance across all workspaces.</p>
          </div>
          <div className="flex items-center gap-3">
            <select value={dateRange} onChange={(e) => setDateRange(e.target.value)} className="text-sm rounded-lg border border-gray-300 px-3 py-2">
              <option value="7">Last 7 days</option>
              <option value="30">Last 30 days</option>
              <option value="90">Last 90 days</option>
            </select>
            <button onClick={loadData} className="px-3 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50"><span className="material-symbols-outlined text-[18px]">refresh</span></button>
            <button onClick={handleExport} className="px-4 py-2 text-sm rounded-lg bg-blue-600 text-white hover:bg-blue-700 transition-colors flex items-center gap-2">
              <span className="material-symbols-outlined text-[16px]">download</span> Export CSV
            </button>
          </div>
        </div>

        {/* Platform KPIs */}
        <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-4">
          <AdminStatsCard title="Total Impressions" value={formatNumber(t?.impressions ?? 0)} icon="visibility" />
          <AdminStatsCard title="Total Clicks" value={formatNumber(t?.clicks ?? 0)} icon="ads_click" />
          <AdminStatsCard title="CTR" value={`${(t?.ctr ?? 0).toFixed(2)}%`} icon="trending_up" />
          <AdminStatsCard title="Ad Spend" value={formatCurrency(t?.spend ?? 0)} icon="payments" />
          <AdminStatsCard title="Est. Revenue" value={formatCurrency(t?.estimatedRevenue ?? 0)} icon="savings" />
        </div>

        {/* System Stats Row */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <AdminStatsCard title="Total Users" value={sys?.totalUsers ?? 0} icon="group" />
          <AdminStatsCard title="Workspaces" value={sys?.totalWorkspaces ?? 0} icon="apartment" />
          <AdminStatsCard title="Content Items" value={sys?.totalContent ?? 0} icon="description" />
          <AdminStatsCard title="Total Revenue" value={formatCurrency(sys?.totalRevenue ?? 0)} icon="account_balance" />
        </div>

        {/* Charts Row */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Top Workspaces Chart */}
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Top Workspaces by Revenue</h3>
              <select value={topWorkspacesLimit} onChange={(e) => setTopWorkspacesLimit(Number(e.target.value))} className="text-xs rounded border border-gray-300 px-2 py-1">
                <option value={5}>Top 5</option>
                <option value={10}>Top 10</option>
                <option value={20}>Top 20</option>
                <option value={50}>Top 50</option>
              </select>
            </div>
            <ResponsiveContainer width="100%" height={350}>
              <BarChart data={workspaces.slice(0, topWorkspacesLimit)} layout="vertical" margin={{ left: 100 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis type="number" tick={{ fontSize: 11 }} tickFormatter={formatCurrency} />
                <YAxis type="category" dataKey="workspaceName" tick={{ fontSize: 11 }} width={90} />
                <Tooltip formatter={(v: any) => formatCurrency(Number(v) || 0)} />
                <Bar dataKey="estimatedRevenue" fill="#4f46e5" radius={[0, 4, 4, 0]} name="Revenue" />
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* Spend vs Revenue Chart */}
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Spend vs Revenue</h3>
              <select value={spendRevLimit} onChange={(e) => setSpendRevLimit(Number(e.target.value))} className="text-xs rounded border border-gray-300 px-2 py-1">
                <option value={5}>Top 5</option>
                <option value={10}>Top 10</option>
                <option value={20}>Top 20</option>
              </select>
            </div>
            <ResponsiveContainer width="100%" height={350}>
              <BarChart data={workspaces.slice(0, spendRevLimit)}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="workspaceName" tick={{ fontSize: 10 }} angle={-30} textAnchor="end" height={60} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={formatCurrency} />
                <Tooltip formatter={(v: any) => formatCurrency(Number(v) || 0)} />
                <Legend />
                <Bar dataKey="spend" fill="#f59e0b" radius={[4, 4, 0, 0]} name="Ad Spend" />
                <Bar dataKey="estimatedRevenue" fill="#10b981" radius={[4, 4, 0, 0]} name="Revenue" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* ROAS + Engagement Charts */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">ROAS by Workspace</h3>
              <select value={roasLimit} onChange={(e) => setRoasLimit(Number(e.target.value))} className="text-xs rounded border border-gray-300 px-2 py-1">
                <option value={5}>Top 5</option>
                <option value={10}>Top 10</option>
                <option value={20}>Top 20</option>
              </select>
            </div>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={workspaces.slice(0, roasLimit)}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="workspaceName" tick={{ fontSize: 10 }} angle={-30} textAnchor="end" height={60} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `${v.toFixed(1)}x`} />
                <Tooltip formatter={(v: any) => `${Number(v || 0).toFixed(2)}x`} />
                <Bar dataKey="roas" fill="#8b5cf6" radius={[4, 4, 0, 0]} name="ROAS" />
              </BarChart>
            </ResponsiveContainer>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Engagement by Workspace</h3>
              <select value={engagementLimit} onChange={(e) => setEngagementLimit(Number(e.target.value))} className="text-xs rounded border border-gray-300 px-2 py-1">
                <option value={5}>Top 5</option>
                <option value={10}>Top 10</option>
                <option value={20}>Top 20</option>
              </select>
            </div>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={workspaces.slice(0, engagementLimit)}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="workspaceName" tick={{ fontSize: 10 }} angle={-30} textAnchor="end" height={60} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={formatNumber} />
                <Tooltip formatter={(v: any) => formatNumber(Number(v) || 0)} />
                <Bar dataKey="engagement" fill="#06b6d4" radius={[4, 4, 0, 0]} name="Engagement" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Workspace Comparison Table */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Workspace Performance Comparison</h3>
            <select value={wsTableLimit} onChange={(e) => setWsTableLimit(Number(e.target.value))} className="text-xs rounded border border-gray-300 px-2 py-1">
              <option value={10}>Top 10</option>
              <option value={20}>Top 20</option>
              <option value={50}>Top 50</option>
              <option value={100}>Top 100</option>
            </select>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-gray-50 border-b border-gray-200">
                  <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Workspace</th>
                  <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Posts</th>
                  <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Campaigns</th>
                  <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Impressions</th>
                  <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Clicks</th>
                  <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">CTR</th>
                  <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Spend</th>
                  <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Revenue</th>
                  <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">ROAS</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {workspaces.slice(0, wsTableLimit).map((w) => (
                  <tr key={w.workspaceId} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 font-medium text-gray-900">{w.workspaceName}</td>
                    <td className="px-4 py-3 text-right text-gray-600">{w.publishedPosts}</td>
                    <td className="px-4 py-3 text-right text-gray-600">{w.activeCampaigns}</td>
                    <td className="px-4 py-3 text-right text-gray-600">{formatNumber(w.impressions)}</td>
                    <td className="px-4 py-3 text-right text-gray-600">{formatNumber(w.clicks)}</td>
                    <td className="px-4 py-3 text-right text-gray-600">{(w.ctr).toFixed(2)}%</td>
                    <td className="px-4 py-3 text-right text-gray-600">{formatCurrency(w.spend)}</td>
                    <td className="px-4 py-3 text-right font-medium text-emerald-600">{formatCurrency(w.estimatedRevenue)}</td>
                    <td className="px-4 py-3 text-right">
                      <StatusBadge status={`${w.roas.toFixed(1)}x`} variant={w.roas >= 2 ? "success" : w.roas >= 1 ? "warning" : "error"} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Top Campaigns */}
        {campaigns && campaigns.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Top Campaigns (All Workspaces)</h3>
              <select value={campaignsTableLimit} onChange={(e) => setCampaignsTableLimit(Number(e.target.value))} className="text-xs rounded border border-gray-300 px-2 py-1">
                <option value={10}>Top 10</option>
                <option value={20}>Top 20</option>
                <option value={50}>Top 50</option>
                <option value={100}>Top 100</option>
              </select>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 border-b border-gray-200">
                    <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Campaign</th>
                    <th className="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Status</th>
                    <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Impressions</th>
                    <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Clicks</th>
                    <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">CTR</th>
                    <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">Spend</th>
                    <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">CPA</th>
                    <th className="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase">ROAS</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {campaigns.slice(0, campaignsTableLimit).map((c, i) => (
                    <tr key={i} className="hover:bg-gray-50">
                      <td className="px-4 py-3 font-medium text-gray-900">{c.campaignName}</td>
                      <td className="px-4 py-3">
                        <StatusBadge status={c.status} variant={c.status === "active" ? "success" : c.status === "completed" ? "neutral" : "warning"} />
                      </td>
                      <td className="px-4 py-3 text-right text-gray-600">{formatNumber(c.impressions)}</td>
                      <td className="px-4 py-3 text-right text-gray-600">{formatNumber(c.clicks)}</td>
                      <td className="px-4 py-3 text-right text-gray-600">{(c.ctr).toFixed(2)}%</td>
                      <td className="px-4 py-3 text-right text-gray-600">{formatCurrency(c.spend)}</td>
                      <td className="px-4 py-3 text-right text-gray-600">{formatCurrency(c.cpa)}</td>
                      <td className="px-4 py-3 text-right font-medium text-emerald-600">{c.roas.toFixed(1)}x</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </main>
    </>
  );
}
