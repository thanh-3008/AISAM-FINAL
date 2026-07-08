"use client";

import { useEffect, useState, useCallback } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchSystemHealth, fetchServiceHealth, SystemHealthCheck, ServiceHealthItem } from "@/services/adminService";

export default function AdminSystemHealthPage() {
  const [checks, setChecks] = useState<SystemHealthCheck[]>([]);
  const [overall, setOverall] = useState("Checking...");
  const [services, setServices] = useState<ServiceHealthItem[]>([]);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    const [health, svc] = await Promise.all([fetchSystemHealth(), fetchServiceHealth()]);
    if (health) { setChecks(health.checks); setOverall(health.overallStatus); }
    if (svc) setServices(svc.services);
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);

  if (loading) return (
    <><AdminHeader breadcrumbs={[{ label: "System Health" }]} /><main className="flex-1 p-8"><div className="animate-pulse h-64 bg-gray-100 rounded-xl" /></main></>
  );

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Platform Health" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Platform Health</h2>
            <p className="text-gray-500 mt-1">Overall: <StatusBadge status={overall} variant={overall === "Healthy" ? "success" : overall === "Degraded" ? "error" : "warning"} /></p>
          </div>
          <button onClick={load} className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50 flex items-center gap-2">
            <span className="material-symbols-outlined text-[16px]">refresh</span> Refresh
          </button>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Infrastructure</h3>
            <div className="space-y-3">
              {checks.map((c, i) => (
                <div key={i} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div><p className="font-medium text-gray-900">{c.name}</p><p className="text-xs text-gray-500">{c.detail}</p></div>
                  <StatusBadge status={c.status} variant={c.status === "Healthy" ? "success" : "error"} />
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Background Services</h3>
            <div className="space-y-3">
              {services.map((s, i) => (
                <div key={i} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900">{s.name}</p>
                    <p className="text-xs text-gray-500">Success: {s.successCount} · Failed: {s.failureCount}{s.isStale ? " · Stale" : ""}</p>
                  </div>
                  <StatusBadge status={s.isStale ? "Stale" : s.status} variant={s.isStale ? "warning" : s.status === "Running" ? "success" : "error"} />
                </div>
              ))}
            </div>
          </div>
        </div>
      </main>
    </>
  );
}
