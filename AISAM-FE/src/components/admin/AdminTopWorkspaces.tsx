"use client";

import { useEffect, useState } from "react";
import { fetchAdminTopWorkspaces, fetchAdminAiCreditBreakdown, AdminTopWorkspace } from "@/services/adminService";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer
} from "recharts";

export default function AdminTopWorkspaces() {
  const [data, setData] = useState<AdminTopWorkspace[]>([]);
  const [aiCreditData, setAiCreditData] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [limit, setLimit] = useState(10);
  const [period, setPeriod] = useState("month");

  useEffect(() => {
    setLoading(true);
    Promise.all([
      fetchAdminTopWorkspaces(limit, period),
      fetchAdminAiCreditBreakdown()
    ]).then(([topRes, aiRes]) => {
      setData(topRes || []);
      setAiCreditData(aiRes || []);
      setLoading(false);
    });
  }, [limit, period]);

  const formatCurrency = (val: number) => `${val.toLocaleString()} VND`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-bold text-gray-900">Top Workspaces Analytics</h2>
        <div className="flex gap-2">
          <select
            value={period}
            onChange={(e) => setPeriod(e.target.value)}
            className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm"
          >
            <option value="day">Today</option>
            <option value="week">This Week</option>
            <option value="month">This Month</option>
            <option value="year">This Year</option>
            <option value="all">All Time</option>
          </select>
          <select
            value={limit}
            onChange={(e) => setLimit(Number(e.target.value))}
            className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm"
          >
            <option value={10}>Top 10</option>
            <option value={20}>Top 20</option>
            <option value={50}>Top 50</option>
            <option value={100}>Top 100</option>
          </select>
        </div>
      </div>

      {loading ? (
        <div className="h-[400px] bg-white rounded-xl border border-gray-200 animate-pulse flex items-center justify-center">
          <span className="material-symbols-outlined animate-spin text-4xl text-gray-300">progress_activity</span>
        </div>
      ) : data.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-8 text-center text-gray-500">
          No workspace data available.
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">SaaS Revenue vs Ad Spend</h3>
              <div className="h-[300px]">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={data}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
                    <XAxis dataKey="workspaceName" tick={{ fontSize: 12 }} />
                    <YAxis tick={{ fontSize: 12 }} width={80} />
                    <Tooltip formatter={(value: any) => formatCurrency(value as number)} />
                    <Legend />
                    <Bar dataKey="saaSRevenue" name="SaaS Revenue" fill="#3B82F6" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="adSpend" name="Ad Spend" fill="#EF4444" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>

            <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm">
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Ad Revenue vs Ad Spend</h3>
              <div className="h-[300px]">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={data}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
                    <XAxis dataKey="workspaceName" tick={{ fontSize: 12 }} />
                    <YAxis tick={{ fontSize: 12 }} width={80} />
                    <Tooltip formatter={(value: any) => formatCurrency(value as number)} />
                    <Legend />
                    <Bar dataKey="adRevenue" name="Ad Revenue" fill="#10B981" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="adSpend" name="Ad Spend" fill="#EF4444" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Performance Breakdown</h3>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead className="bg-gray-50 text-gray-500 uppercase">
                  <tr>
                    <th className="px-6 py-4 font-medium">Workspace</th>
                    <th className="px-6 py-4 font-medium">SaaS Revenue</th>
                    <th className="px-6 py-4 font-medium">Ad Spend</th>
                    <th className="px-6 py-4 font-medium">Ad Revenue</th>
                    <th className="px-6 py-4 font-medium">ROAS</th>
                    <th className="px-6 py-4 font-medium">Engagement</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {data.map((row) => (
                    <tr key={row.workspaceId} className="hover:bg-gray-50">
                      <td className="px-6 py-4 font-medium text-gray-900">{row.workspaceName}</td>
                      <td className="px-6 py-4 text-blue-600 font-medium">{formatCurrency(row.saaSRevenue)}</td>
                      <td className="px-6 py-4 text-red-600">{formatCurrency(row.adSpend)}</td>
                      <td className="px-6 py-4 text-emerald-600">{formatCurrency(row.adRevenue)}</td>
                      <td className="px-6 py-4 font-medium">{row.roas}x</td>
                      <td className="px-6 py-4">{row.engagement.toLocaleString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* AI Credit Breakdown Chart */}
          <div className="bg-white rounded-xl border border-gray-200 p-6 shadow-sm mt-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">AI Generations Breakdown by Workspace (Top 50)</h3>
            {aiCreditData.length === 0 ? (
              <div className="h-[300px] flex items-center justify-center text-gray-500">No AI credit data available</div>
            ) : (
              <div className="h-[300px]">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={aiCreditData}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
                    <XAxis dataKey="workspaceName" tick={{ fontSize: 12 }} />
                    <YAxis tick={{ fontSize: 12 }} width={80} />
                    <Tooltip />
                    <Legend />
                    <Bar dataKey="totalGenerations" name="Total AI Generations" fill="#8B5CF6" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}
