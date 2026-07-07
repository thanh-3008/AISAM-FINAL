"use client";

import { useEffect, useState, useCallback } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminSubscriptions, updateSubscription, AdminSubscription } from "@/services/adminService";

const planLabels: Record<number, string> = { 0: "Free", 1: "Plus", 2: "Premium", 3: "PlusTrial" };

export default function AdminSubscriptionsPage() {
  const [subs, setSubs] = useState<AdminSubscription[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const loadSubs = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminSubscriptions(page);
    if (data) { setSubs(data.items); setTotal(data.total); }
    setLoading(false);
  }, [page]);

  useEffect(() => { loadSubs(); }, [loadSubs]);

  const handleToggleActive = async (id: string, currentActive: boolean) => {
    const ok = await updateSubscription(id, { isActive: !currentActive });
    if (ok) loadSubs();
  };

  const handleExtend = async (id: string, currentEnd: string) => {
    const newEnd = new Date(new Date(currentEnd).getTime() + 30 * 86400000).toISOString();
    const ok = await updateSubscription(id, { endDate: newEnd });
    if (ok) loadSubs();
  };

  const columns = [
    { key: "workspaceName", header: "Workspace" },
    {
      key: "plan",
      header: "Plan",
      render: (s: AdminSubscription) => <StatusBadge status={planLabels[s.plan] ?? "Unknown"} variant={s.plan === 2 ? "error" : s.plan === 1 ? "info" : "neutral"} />,
    },
    {
      key: "isActive",
      header: "Status",
      render: (s: AdminSubscription) => <StatusBadge status={s.isActive ? "Active" : "Inactive"} variant={s.isActive ? "success" : "warning"} />,
    },
    {
      key: "endDate",
      header: "End Date",
      render: (s: AdminSubscription) => s.endDate ? new Date(s.endDate).toLocaleDateString() : "—",
    },
    {
      key: "actions",
      header: "Actions",
      render: (s: AdminSubscription) => (
        <div className="flex items-center gap-2">
          <button onClick={(e) => { e.stopPropagation(); handleToggleActive(s.id, s.isActive); }} className="text-xs px-2 py-1 rounded bg-gray-100 hover:bg-gray-200 transition-colors">
            {s.isActive ? "Cancel" : "Activate"}
          </button>
          <button onClick={(e) => { e.stopPropagation(); handleExtend(s.id, s.endDate); }} className="text-xs px-2 py-1 rounded bg-blue-50 hover:bg-blue-100 text-blue-600 transition-colors">
            +30 Days
          </button>
        </div>
      ),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Subscriptions" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div><h2 className="text-2xl font-bold text-gray-900">Subscriptions</h2><p className="text-gray-500 mt-1">{total} total subscriptions</p></div>
        {loading ? (
          <div className="space-y-3">{[...Array(5)].map((_, i) => <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />)}</div>
        ) : (
          <>
            <AdminDataTable columns={columns} data={subs} keyField="id" />
            <div className="flex items-center justify-between">
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Previous</button>
              <span className="text-sm text-gray-500">Page {page}</span>
              <button onClick={() => setPage((p) => p + 1)} disabled={page * 20 >= total} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Next</button>
            </div>
          </>
        )}
      </main>
    </>
  );
}
