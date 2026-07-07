"use client";

import { useEffect, useState, useCallback } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchServiceHealth, ServiceHealthItem } from "@/services/adminService";

export default function AdminServiceHealthPage() {
  const [services, setServices] = useState<ServiceHealthItem[]>([]);
  const [overall, setOverall] = useState("Loading...");
  const [loading, setLoading] = useState(true);
  const [lastRefresh, setLastRefresh] = useState("");

  const loadHealth = useCallback(async () => {
    const data = await fetchServiceHealth();
    if (data) {
      setServices(data.services);
      setOverall(data.overallStatus);
      setLastRefresh(new Date().toLocaleTimeString());
    }
    setLoading(false);
  }, []);

  useEffect(() => { loadHealth(); }, [loadHealth]);

  useEffect(() => {
    const timer = setInterval(loadHealth, 30000);
    return () => clearInterval(timer);
  }, [loadHealth]);

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Service Health" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Background Services</h2>
            <p className="text-gray-500 mt-1">
              Overall: <StatusBadge
                status={overall}
                variant={overall === "Healthy" ? "success" : overall === "Warning" ? "warning" : overall === "Degraded" ? "error" : "info"}
              />
              <span className="ml-2 text-xs text-gray-400">Last refresh: {lastRefresh}</span>
            </p>
          </div>
          <button onClick={loadHealth} className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50 flex items-center gap-2">
            <span className="material-symbols-outlined text-[16px]">refresh</span> Refresh
          </button>
        </div>

        {loading ? (
          <div className="space-y-3">{[...Array(5)].map((_, i) => <div key={i} className="h-16 bg-gray-100 rounded-xl animate-pulse" />)}</div>
        ) : (
          <div className="space-y-4">
            {services.map((svc) => (
              <div key={svc.name} className={`bg-white rounded-xl border ${svc.isStale ? "border-amber-300" : svc.status === "Degraded" ? "border-red-300" : "border-gray-200"} shadow-sm p-5`}>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <div className={`w-3 h-3 rounded-full ${svc.status === "Running" ? "bg-emerald-500" : svc.status === "Degraded" ? "bg-red-500" : svc.status === "Not Started" ? "bg-gray-300" : "bg-amber-500"} ${svc.status === "Running" ? "animate-pulse" : ""}`} />
                    <div>
                      <h3 className="font-semibold text-gray-900">{svc.name}</h3>
                      <p className="text-xs text-gray-500">Last heartbeat: {svc.lastHeartbeat ? new Date(svc.lastHeartbeat).toLocaleString() : "Never"}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-6">
                    <div className="text-center">
                      <div className="text-lg font-bold text-emerald-600">{svc.successCount}</div>
                      <div className="text-xs text-gray-500">Success</div>
                    </div>
                    <div className="text-center">
                      <div className={`text-lg font-bold ${svc.failureCount > 0 ? "text-red-600" : "text-gray-400"}`}>{svc.failureCount}</div>
                      <div className="text-xs text-gray-500">Failed</div>
                    </div>
                    <StatusBadge status={svc.status} variant={svc.status === "Running" ? "success" : svc.isStale ? "warning" : svc.status === "Degraded" ? "error" : "neutral"} />
                  </div>
                </div>
                {svc.lastError && (
                  <div className="mt-3 p-3 bg-red-50 rounded-lg text-xs text-red-700 font-mono">{svc.lastError}</div>
                )}
              </div>
            ))}
          </div>
        )}
      </main>
    </>
  );
}
