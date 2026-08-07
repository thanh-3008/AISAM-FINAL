"use client";

import { useEffect, useState, useCallback } from "react";
import { useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import { fetchAdminAuditLogs, exportAdminAuditLogsCsv, AdminAuditLog } from "@/services/adminService";

export default function AdminAuditLogsPage() {
  const router = useRouter();
  const [logs, setLogs] = useState<AdminAuditLog[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [exporting, setExporting] = useState(false);

  // Filters
  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [actionType, setActionType] = useState("");
  const [targetTable, setTargetTable] = useState("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(searchTerm);
      setPage(1);
    }, 500);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Reset page when other filters change
  useEffect(() => { setPage(1); }, [actionType, targetTable, fromDate, toDate]);

  const loadLogs = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminAuditLogs(page, 20, actionType, targetTable, debouncedSearch, fromDate, toDate);
    if (data) {
      setLogs(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page, actionType, targetTable, debouncedSearch, fromDate, toDate]);

  useEffect(() => { loadLogs(); }, [loadLogs]);

  const handleExport = async () => {
    setExporting(true);
    const blob = await exportAdminAuditLogsCsv(actionType, targetTable, debouncedSearch, fromDate, toDate);
    if (blob) {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `AuditLogs_${new Date().toISOString().split('T')[0]}.csv`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
    } else {
      alert("Failed to export Audit Logs.");
    }
    setExporting(false);
  };

  const columns = [
    {
      key: "actionType",
      header: "Action",
      render: (l: AdminAuditLog) => <span className="font-medium text-gray-900">{l.actionType}</span>,
    },
    {
      key: "target",
      header: "Target",
      render: (l: AdminAuditLog) => <span className="text-gray-500">{l.targetTable} ({l.targetId.substring(0, 8)}...)</span>,
    },
    {
      key: "actorEmail",
      header: "Actor",
      render: (l: AdminAuditLog) => (
        <div className="flex flex-col">
          <span className="font-medium text-gray-900 text-sm">{l.actorName || l.actorEmail || l.actorId.substring(0, 8)}</span>
          {l.actorName && <span className="text-xs text-gray-500">{l.actorEmail}</span>}
        </div>
      ),
    },
    {
      key: "createdAt",
      header: "Date",
      render: (l: AdminAuditLog) => new Date(l.createdAt).toLocaleString(),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Audit Logs" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Audit Logs</h2>
            <p className="text-gray-500 mt-1">Track admin actions across the platform. {total} entries.</p>
          </div>
          <button 
            onClick={handleExport} 
            disabled={exporting || logs.length === 0}
            className="flex items-center justify-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 text-sm font-medium"
          >
            <span className="material-symbols-outlined text-sm">{exporting ? 'hourglass_empty' : 'download'}</span>
            {exporting ? 'Exporting...' : 'Export CSV'}
          </button>
        </div>

        {/* Filters */}
        <div className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm grid grid-cols-1 md:grid-cols-5 gap-4">
          <div className="relative">
            <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm">search</span>
            <input
              type="text"
              placeholder="Search Actor Email or Name..."
              className="w-full pl-9 pr-4 py-2 border border-gray-200 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <input
            type="text"
            placeholder="Action (e.g. UPDATE_CONTENT)"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
            value={actionType}
            onChange={(e) => setActionType(e.target.value)}
          />
          <input
            type="text"
            placeholder="Target Table"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
            value={targetTable}
            onChange={(e) => setTargetTable(e.target.value)}
          />
          <input
            type="date"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm text-gray-600 focus:ring-2 focus:ring-blue-500 outline-none"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
          />
          <input
            type="date"
            className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm text-gray-600 focus:ring-2 focus:ring-blue-500 outline-none"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
          />
        </div>

        {loading ? (
          <div className="space-y-3">{[...Array(8)].map((_, i) => <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />)}</div>
        ) : logs.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-16 text-gray-400">
            <span className="material-symbols-outlined text-5xl mb-4">receipt_long</span>
            <p className="text-lg font-medium text-gray-500">No audit logs</p>
          </div>
        ) : (
          <>
            <AdminDataTable columns={columns} data={logs} keyField="id" onRowClick={(log) => router.push(`/admin/audit-logs/${log.id}`)} />
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
