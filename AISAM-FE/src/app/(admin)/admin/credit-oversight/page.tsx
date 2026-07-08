"use client";

import { useEffect, useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import { fetchAiCreditSummary, AiCreditSummary } from "@/services/adminService";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";

export default function AdminCreditOversightPage() {
  const [data, setData] = useState<AiCreditSummary | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchAiCreditSummary().then((d) => { setData(d); setLoading(false); });
  }, []);

  if (loading) return (
    <><AdminHeader breadcrumbs={[{ label: "AI & Credit" }]} /><main className="flex-1 p-8"><div className="animate-pulse h-64 bg-gray-100 rounded-xl" /></main></>
  );

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "AI & Credit Oversight" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-8">
        <div><h2 className="text-2xl font-bold text-gray-900">AI & Credit Oversight</h2><p className="text-gray-500 mt-1">Platform-wide AI usage and credit consumption.</p></div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <AdminStatsCard title="Total AI Generations" value={(data?.totalAiGenerations ?? 0).toLocaleString()} icon="smart_toy" />
          <AdminStatsCard title="Weekly AI Usage" value={(data?.weeklyAiGenerations ?? 0).toLocaleString()} icon="trending_up" />
          <AdminStatsCard title="Est. Credits Used" value={(data?.estimatedCreditSpent ?? 0).toLocaleString()} icon="toll" />
        </div>

        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Daily AI Generations (Last 7 Days)</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={data?.dailyAiData ?? []}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="name" tick={{ fontSize: 12 }} />
              <YAxis tick={{ fontSize: 12 }} />
              <Tooltip />
              <Bar dataKey="generations" fill="#8b5cf6" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">AI Cost Analysis</h3>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div><dt className="text-gray-500">Total Generations</dt><dd className="font-medium text-gray-900">{(data?.totalAiGenerations ?? 0).toLocaleString()}</dd></div>
            <div><dt className="text-gray-500">Est. Total Cost</dt><dd className="font-medium text-gray-900">{(data?.estimatedRevenue ?? 0).toLocaleString()} VND</dd></div>
            <div><dt className="text-gray-500">Avg Cost / Generation</dt><dd className="font-medium text-gray-900">100 VND</dd></div>
            <div><dt className="text-gray-500">Weekly Trend</dt><dd className="font-medium text-emerald-600">+{data?.weeklyAiGenerations ?? 0} this week</dd></div>
          </dl>
        </div>
      </main>
    </>
  );
}
