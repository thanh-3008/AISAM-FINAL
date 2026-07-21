"use client";

import { useEffect, useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import { fetchAiCreditSummary, AiCreditSummary, adjustAdminCredits, fetchAdminWorkspaces, AdminWorkspace } from "@/services/adminService";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";

export default function AdminCreditOversightPage() {
  const [data, setData] = useState<AiCreditSummary | null>(null);
  const [loading, setLoading] = useState(true);

  const [adjustModalOpen, setAdjustModalOpen] = useState(false);
  const [workspaces, setWorkspaces] = useState<AdminWorkspace[]>([]);
  const [selectedWorkspace, setSelectedWorkspace] = useState("");
  const [adjustAmount, setAdjustAmount] = useState("");
  const [adjustReason, setAdjustReason] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    fetchAiCreditSummary().then((d) => { setData(d); setLoading(false); });
    fetchAdminWorkspaces(1, 100).then((res) => { if (res) setWorkspaces(res.items); });
  }, []);

  const handleAdjustCredits = async () => {
    const amount = parseInt(adjustAmount);
    if (!selectedWorkspace || isNaN(amount) || !adjustReason) return;

    setSubmitting(true);
    const success = await adjustAdminCredits(selectedWorkspace, amount, adjustReason);
    setSubmitting(false);

    if (success) {
      setAdjustModalOpen(false);
      setAdjustAmount("");
      setAdjustReason("");
      setSelectedWorkspace("");
      alert("Credits adjusted successfully!");
    } else {
      alert("Failed to adjust credits. Please try again.");
    }
  };

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
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">AI Cost Analysis</h3>
            <button onClick={() => setAdjustModalOpen(true)} className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors">
              <span className="material-symbols-outlined text-[20px]">account_balance_wallet</span>
              Adjust Credits
            </button>
          </div>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div><dt className="text-gray-500">Total Generations</dt><dd className="font-medium text-gray-900">{(data?.totalAiGenerations ?? 0).toLocaleString()}</dd></div>
            <div><dt className="text-gray-500">Est. Total Cost</dt><dd className="font-medium text-gray-900">{(data?.estimatedRevenue ?? 0).toLocaleString()} VND</dd></div>
            <div><dt className="text-gray-500">Avg Cost / Generation</dt><dd className="font-medium text-gray-900">100 VND</dd></div>
            <div><dt className="text-gray-500">Weekly Trend</dt><dd className="font-medium text-emerald-600">+{data?.weeklyAiGenerations ?? 0} this week</dd></div>
          </dl>
        </div>
      </main>

      {adjustModalOpen && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden">
            <div className="p-4 border-b border-gray-200 flex justify-between items-center bg-gray-50">
              <h3 className="text-lg font-semibold text-gray-900">Adjust Credits</h3>
              <button onClick={() => setAdjustModalOpen(false)} className="text-gray-400 hover:text-gray-600">
                <span className="material-symbols-outlined">close</span>
              </button>
            </div>
            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Workspace</label>
                <select
                  value={selectedWorkspace}
                  onChange={(e) => setSelectedWorkspace(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500 outline-none"
                >
                  <option value="">Select a workspace...</option>
                  {workspaces.map((ws) => (
                    <option key={ws.id} value={ws.id}>{ws.name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Amount (e.g., 100 or -50)</label>
                <input
                  type="number"
                  value={adjustAmount}
                  onChange={(e) => setAdjustAmount(e.target.value)}
                  placeholder="Enter amount"
                  className="w-full border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500 outline-none"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Reason</label>
                <input
                  type="text"
                  value={adjustReason}
                  onChange={(e) => setAdjustReason(e.target.value)}
                  placeholder="e.g., Compensation for system error"
                  className="w-full border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-blue-500 outline-none"
                />
              </div>
            </div>
            <div className="p-4 border-t border-gray-200 bg-gray-50 flex justify-end gap-3">
              <button
                onClick={() => setAdjustModalOpen(false)}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleAdjustCredits}
                disabled={submitting || !selectedWorkspace || !adjustAmount || !adjustReason}
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
              >
                {submitting ? "Processing..." : "Confirm Adjustment"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
