"use client";

import { useEffect, useState, useCallback, useMemo } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminPayments, AdminPayment, refundAdminPayment, fetchAdminDashboardSummary, AdminDashboardSummary } from "@/services/adminService";
import { apiClient } from "@/lib/apiClient";
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";

function formatCurrency(n: number): string {
  if (n >= 1_000_000) return `${(n / 1_000_000).toFixed(1)}M`;
  if (n >= 1_000) return `${(n / 1_000).toFixed(0)}K`;
  return n.toLocaleString();
}

export default function AdminPaymentsPage() {
  const [payments, setPayments] = useState<AdminPayment[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  
  // Filters
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [searchQuery, setSearchQuery] = useState("");
  const [dateRange, setDateRange] = useState("30"); // days

  // Detail Modal
  const [selectedPayment, setSelectedPayment] = useState<AdminPayment | null>(null);
  const [refundReason, setRefundReason] = useState("");
  const [refunding, setRefunding] = useState(false);
  const [refundResult, setRefundResult] = useState("");

  // Revenue Chart Data
  const [chartData, setChartData] = useState<any[]>([]);
  const [summary, setSummary] = useState<AdminDashboardSummary | null>(null);

  const loadPayments = useCallback(async () => {
    setLoading(true);
    let statusParam: number | undefined = undefined;
    if (statusFilter === "completed") statusParam = 1;
    if (statusFilter === "pending") statusParam = 0;
    if (statusFilter === "failed") statusParam = 2;
    if (statusFilter === "refunded") statusParam = 3;

    const data = await fetchAdminPayments(page, 20, statusParam);
    if (data) {
      setPayments(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page, statusFilter]);

  const loadStats = useCallback(async () => {
    const sum = await fetchAdminDashboardSummary();
    if (sum) setSummary(sum);

    try {
      const res = await apiClient(`/admin/dashboard/charts`);
      if (res?.data?.revenue30Day) {
        setChartData(res.data.revenue30Day);
      }
    } catch {}
  }, []);

  useEffect(() => { loadPayments(); }, [loadPayments]);
  useEffect(() => { loadStats(); }, [loadStats]);

  const filteredPayments = useMemo(() => {
    if (!searchQuery) return payments;
    const lower = searchQuery.toLowerCase();
    return payments.filter(p => 
      (p.userEmail && p.userEmail.toLowerCase().includes(lower)) ||
      (p.transactionId && p.transactionId.toLowerCase().includes(lower)) ||
      p.id.toLowerCase().includes(lower)
    );
  }, [payments, searchQuery]);

  const handleRefund = async () => {
    if (!selectedPayment || !refundReason.trim()) return;
    setRefunding(true);
    const ok = await refundAdminPayment(selectedPayment.id, refundReason);
    if (ok) {
      setRefundResult("Refunded successfully!");
      loadPayments();
      setTimeout(() => {
        setSelectedPayment(null);
        setRefundResult("");
        setRefundReason("");
      }, 1500);
    } else {
      setRefundResult("Refund failed.");
    }
    setRefunding(false);
  };

  const columns = [
    {
      key: "transactionId",
      header: "Transaction ID",
      render: (p: AdminPayment) => (
        <span className="font-mono text-xs text-gray-500">
          {p.transactionId ? p.transactionId.substring(0, 12) + "..." : p.id.substring(0, 8) + "..."}
        </span>
      ),
    },
    {
      key: "userEmail",
      header: "User Email",
      render: (p: AdminPayment) => <span className="text-gray-900 font-medium">{p.userEmail || "—"}</span>,
    },
    {
      key: "amount",
      header: "Amount",
      render: (p: AdminPayment) => <span className="font-bold text-gray-900">{p.amount.toLocaleString()} {p.currency}</span>,
    },
    {
      key: "status",
      header: "Status",
      render: (p: AdminPayment) => {
        let label = "Pending";
        let variant: "neutral" | "info" | "success" | "warning" | "error" = "warning";
        if (p.status === 1) { label = "Completed"; variant = "success"; }
        if (p.status === 2) { label = "Failed"; variant = "error"; }
        if (p.status === 3) { label = "Refunded"; variant = "neutral"; }
        return <StatusBadge status={label} variant={variant} />;
      },
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
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Payments & Refunds</h2>
            <p className="text-gray-500 mt-1">Manage platform revenue, transactions, and refunds.</p>
          </div>
        </div>

        {/* Top Stats */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-5 flex items-center justify-between">
            <div>
              <p className="text-xs text-gray-500 uppercase font-semibold">Total Revenue</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{summary?.totalRevenue?.toLocaleString() || 0} VND</p>
            </div>
            <span className="material-symbols-outlined text-4xl text-amber-100 bg-amber-50 rounded-full p-2">payments</span>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-5 flex items-center justify-between">
            <div>
              <p className="text-xs text-gray-500 uppercase font-semibold">Transactions</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{total}</p>
            </div>
            <span className="material-symbols-outlined text-4xl text-blue-100 bg-blue-50 rounded-full p-2">receipt_long</span>
          </div>
        </div>

        {/* Revenue Chart */}
        {chartData.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Revenue (Last 30 Days)</h3>
            <ResponsiveContainer width="100%" height={250}>
              <AreaChart data={chartData}>
                <defs>
                  <linearGradient id="colorRev" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#10b981" stopOpacity={0.3} />
                    <stop offset="95%" stopColor="#10b981" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={formatCurrency} />
                <Tooltip formatter={(v: any) => `${formatCurrency(Number(v))} VND`} />
                <Area type="monotone" dataKey="revenue" stroke="#10b981" strokeWidth={2} fill="url(#colorRev)" name="Revenue" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        )}

        {/* Filters */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4 flex flex-col md:flex-row items-center gap-4">
          <input
            type="text"
            placeholder="Search email or trans ID..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full md:w-64 px-4 py-2 border border-gray-300 rounded-lg text-sm"
          />
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="w-full md:w-48 text-sm rounded-lg border border-gray-300 px-3 py-2">
            <option value="all">All Status</option>
            <option value="completed">Completed</option>
            <option value="pending">Pending</option>
            <option value="failed">Failed</option>
            <option value="refunded">Refunded</option>
          </select>
        </div>

        {loading ? (
          <div className="space-y-3">
            {[...Array(5)].map((_, i) => <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />)}
          </div>
        ) : (
          <>
            <AdminDataTable columns={columns} data={filteredPayments} keyField="id" onRowClick={(p) => setSelectedPayment(p)} />
            <div className="flex items-center justify-between">
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Previous</button>
              <span className="text-sm text-gray-500">Page {page}</span>
              <button onClick={() => setPage((p) => p + 1)} disabled={page * 20 >= total} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Next</button>
            </div>
          </>
        )}
      </main>

      {/* Payment Detail Modal */}
      {selectedPayment && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg overflow-hidden flex flex-col max-h-[90vh]">
            <div className="p-5 flex items-center justify-between border-b border-gray-100">
              <h3 className="font-bold text-lg text-gray-900">Payment Details</h3>
              <button onClick={() => { setSelectedPayment(null); setRefundResult(""); setRefundReason(""); }} className="text-gray-400 hover:text-gray-600">
                <span className="material-symbols-outlined">close</span>
              </button>
            </div>
            <div className="p-6 overflow-y-auto space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div><span className="text-gray-500 block">ID</span><span className="font-mono">{selectedPayment.id}</span></div>
                <div><span className="text-gray-500 block">Transaction ID</span><span className="font-mono">{selectedPayment.transactionId || "—"}</span></div>
                <div><span className="text-gray-500 block">User Email</span><span className="font-medium">{selectedPayment.userEmail || "—"}</span></div>
                <div>
                  <span className="text-gray-500 block">Status</span>
                  <StatusBadge
                    status={selectedPayment.status === 1 ? "Completed" : selectedPayment.status === 0 ? "Pending" : selectedPayment.status === 3 ? "Refunded" : "Failed"}
                    variant={selectedPayment.status === 1 ? "success" : selectedPayment.status === 0 ? "warning" : selectedPayment.status === 3 ? "neutral" : "error"}
                  />
                </div>
                <div><span className="text-gray-500 block">Amount</span><span className="font-bold text-lg">{selectedPayment.amount.toLocaleString()} {selectedPayment.currency}</span></div>
                <div><span className="text-gray-500 block">Date</span><span>{new Date(selectedPayment.createdAt).toLocaleString()}</span></div>
                
                {selectedPayment.status === 3 && (
                  <>
                    <div className="col-span-2"><span className="text-gray-500 block">Refunded At</span><span>{selectedPayment.refundedAt ? new Date(selectedPayment.refundedAt).toLocaleString() : "—"}</span></div>
                    <div className="col-span-2"><span className="text-gray-500 block">Refund Reason</span><span className="text-red-600">{selectedPayment.refundReason || "—"}</span></div>
                  </>
                )}
              </div>

              {/* Refund Action */}
              {selectedPayment.status === 1 && (
                <div className="mt-6 border-t pt-4">
                  <h4 className="text-sm font-semibold text-gray-900 mb-2">Process Refund</h4>
                  <div className="space-y-3">
                    <input
                      type="text"
                      placeholder="Reason for refund (required)"
                      value={refundReason}
                      onChange={(e) => setRefundReason(e.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm"
                    />
                    <div className="flex items-center gap-3">
                      <button
                        onClick={handleRefund}
                        disabled={refunding || !refundReason.trim()}
                        className="px-4 py-2 bg-red-600 text-white rounded-lg text-sm hover:bg-red-700 disabled:opacity-50 transition-colors"
                      >
                        {refunding ? "Processing..." : "Mark as Refunded"}
                      </button>
                      {refundResult && <span className={`text-sm ${refundResult.includes("success") ? "text-emerald-600" : "text-red-600"}`}>{refundResult}</span>}
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
}
