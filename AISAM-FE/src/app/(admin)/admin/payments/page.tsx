"use client";

import { useEffect, useState, useCallback } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminPayments, AdminPayment } from "@/services/adminService";

export default function AdminPaymentsPage() {
  const [payments, setPayments] = useState<AdminPayment[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState<string>("all");

  const loadPayments = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminPayments(page);
    if (data) {
      setPayments(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page]);

  useEffect(() => { loadPayments(); }, [loadPayments]);

  const columns = [
    {
      key: "id",
      header: "Transaction ID",
      render: (p: AdminPayment) => <span className="font-mono text-xs text-gray-500">{p.id.substring(0, 8)}...</span>,
    },
    {
      key: "amount",
      header: "Amount",
      render: (p: AdminPayment) => <span className="font-medium">{p.amount.toLocaleString()} {p.currency}</span>,
    },
    {
      key: "status",
      header: "Status",
      render: (p: AdminPayment) => (
        <StatusBadge
          status={p.status === 1 ? "Completed" : p.status === 0 ? "Pending" : "Failed"}
          variant={p.status === 1 ? "success" : p.status === 0 ? "warning" : "error"}
        />
      ),
    },
    {
      key: "createdAt",
      header: "Date",
      render: (p: AdminPayment) => new Date(p.createdAt).toLocaleDateString(),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Payments" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Payments</h2>
          <p className="text-gray-500 mt-1">{total} total transactions</p>
        </div>

        <div className="flex items-center gap-3">
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="text-sm rounded-lg border border-gray-300 px-3 py-2">
            <option value="all">All Status</option>
            <option value="completed">Completed</option>
            <option value="pending">Pending</option>
            <option value="failed">Failed</option>
          </select>
        </div>

        {loading ? (
          <div className="space-y-3">
            {[...Array(5)].map((_, i) => <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />)}
          </div>
        ) : (
          <>
            <AdminDataTable columns={columns} data={payments} keyField="id" />
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
