"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import { apiClient } from "@/lib/apiClient";

export default function AdminAuditLogDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [log, setLog] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    apiClient(`/admin/audit-logs/${id}`)
      .then((res: any) => { setLog(res?.data); setLoading(false); })
      .catch(() => setLoading(false));
  }, [id]);

  if (loading) return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Audit Logs", href: "/admin/audit-logs" }, { label: "Loading..." }]} />
      <main className="flex-1 p-8"><div className="animate-pulse h-64 bg-gray-100 rounded-xl" /></main>
    </>
  );

  if (!log) return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Audit Logs", href: "/admin/audit-logs" }, { label: "Not Found" }]} />
      <main className="flex-1 p-8"><p className="text-gray-500">Audit log not found.</p></main>
    </>
  );

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Audit Logs", href: "/admin/audit-logs" }, { label: log.actionType }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Log Details</h3>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div><dt className="text-gray-500">Action</dt><dd className="font-medium text-gray-900">{log.actionType}</dd></div>
            <div><dt className="text-gray-500">Target</dt><dd className="font-medium text-gray-900">{log.targetTable} ({log.targetId.substring(0, 8)}...)</dd></div>
            <div><dt className="text-gray-500">Actor</dt><dd className="font-medium text-gray-900">{log.actorEmail}</dd></div>
            <div><dt className="text-gray-500">Date</dt><dd className="font-medium text-gray-900">{new Date(log.createdAt).toLocaleString()}</dd></div>
            {log.notes && <div className="col-span-2"><dt className="text-gray-500">Notes</dt><dd className="font-medium text-gray-900">{log.notes}</dd></div>}
          </dl>
        </div>

        {(log.oldValues || log.newValues) && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {log.oldValues && (
              <div className="bg-white rounded-xl border border-red-200 shadow-sm p-6">
                <h3 className="text-sm font-semibold text-red-600 uppercase tracking-wider mb-3 flex items-center gap-2">
                  <span className="material-symbols-outlined text-[16px]">remove</span> Before
                </h3>
                <pre className="text-xs font-mono bg-red-50 p-4 rounded-lg overflow-auto whitespace-pre-wrap text-gray-700">{JSON.stringify(JSON.parse(log.oldValues), null, 2)}</pre>
              </div>
            )}
            {log.newValues && (
              <div className="bg-white rounded-xl border border-emerald-200 shadow-sm p-6">
                <h3 className="text-sm font-semibold text-emerald-600 uppercase tracking-wider mb-3 flex items-center gap-2">
                  <span className="material-symbols-outlined text-[16px]">add</span> After
                </h3>
                <pre className="text-xs font-mono bg-emerald-50 p-4 rounded-lg overflow-auto whitespace-pre-wrap text-gray-700">{JSON.stringify(JSON.parse(log.newValues), null, 2)}</pre>
              </div>
            )}
          </div>
        )}

        <button onClick={() => router.push("/admin/audit-logs")} className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50">Back to Audit Logs</button>
      </main>
    </>
  );
}
