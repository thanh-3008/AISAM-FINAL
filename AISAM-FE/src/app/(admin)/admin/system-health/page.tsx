"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import {
  AdminAuditLog,
  ServiceHealthItem,
  SystemHealthCheck,
  fetchAdminAuditLogs,
  fetchServiceHealth,
  fetchSystemHealth,
} from "@/services/adminService";

const statusVariant = (status: string): "success" | "warning" | "error" | "neutral" => {
  const value = status.toLowerCase();
  if (["healthy", "running"].includes(value)) return "success";
  if (["warning", "stale", "not started"].includes(value)) return "warning";
  if (["degraded", "unhealthy", "failed"].includes(value)) return "error";
  return "neutral";
};

export default function AdminSystemHealthPage() {
  const [checks, setChecks] = useState<SystemHealthCheck[]>([]);
  const [services, setServices] = useState<ServiceHealthItem[]>([]);
  const [logs, setLogs] = useState<AdminAuditLog[]>([]);
  const [overall, setOverall] = useState("Checking...");
  const [checkedAt, setCheckedAt] = useState<string>();
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    const [health, serviceHealth, audit] = await Promise.all([
      fetchSystemHealth(),
      fetchServiceHealth(),
      fetchAdminAuditLogs(1, 6),
    ]);
    if (health) {
      setChecks(health.checks);
      setOverall(health.overallStatus);
      setCheckedAt(health.checkedAt);
    }
    if (serviceHealth) setServices(serviceHealth.services);
    if (audit) setLogs(audit.items);
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);

  const metrics = useMemo(() => {
    const failures = services.reduce((sum, service) => sum + service.failureCount, 0);
    const successes = services.reduce((sum, service) => sum + service.successCount, 0);
    const incidents = services.filter((service) => service.isStale || service.status === "Degraded").length
      + checks.filter((check) => check.status !== "Healthy").length;
    const reliability = successes + failures === 0 ? 100 : (successes / (successes + failures)) * 100;
    return { failures, incidents, reliability, services: services.length + checks.length };
  }, [checks, services]);

  const metricCards = [
    { label: "System status", value: overall, icon: "health_and_safety", tone: statusVariant(overall) },
    { label: "Active warnings", value: metrics.incidents.toString(), icon: "warning", tone: metrics.incidents ? "warning" as const : "success" as const },
    { label: "Failed jobs", value: metrics.failures.toLocaleString(), icon: "error", tone: metrics.failures ? "error" as const : "success" as const },
    { label: "Job reliability", value: `${metrics.reliability.toFixed(1)}%`, icon: "verified", tone: metrics.reliability >= 99 ? "success" as const : "warning" as const },
    { label: "Monitored units", value: metrics.services.toString(), icon: "dns", tone: "neutral" as const },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Audit & Monitoring" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6 bg-gray-50/60">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">Audit & Monitoring</h2>
            <p className="text-gray-500 mt-1">Live platform health, background operations and recent administrative activity.</p>
          </div>
          <div className="flex items-center gap-3">
            {checkedAt && <span className="text-xs text-gray-400">Updated {new Date(checkedAt).toLocaleString()}</span>}
            <button onClick={load} disabled={loading} className="px-4 py-2 text-sm rounded-lg border border-gray-200 bg-white hover:bg-gray-50 flex items-center gap-2 disabled:opacity-50">
              <span className={`material-symbols-outlined text-[17px] ${loading ? "animate-spin" : ""}`}>refresh</span>
              Refresh
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-5 gap-4">
          {metricCards.map((metric) => (
            <div key={metric.label} className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium uppercase tracking-wide text-gray-500">{metric.label}</span>
                <span className={`material-symbols-outlined text-[21px] ${metric.tone === "success" ? "text-emerald-500" : metric.tone === "warning" ? "text-amber-500" : metric.tone === "error" ? "text-red-500" : "text-blue-500"}`}>{metric.icon}</span>
              </div>
              <p className="mt-3 text-2xl font-bold text-gray-900">{metric.value}</p>
            </div>
          ))}
        </div>

        <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
          <section className="xl:col-span-2 bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="flex items-center justify-between mb-5">
              <div><h3 className="font-semibold text-gray-900">Service status</h3><p className="text-sm text-gray-500">Infrastructure and background workers</p></div>
              <Link href="/admin/service-health" className="text-sm font-medium text-blue-600 hover:text-blue-700">View details</Link>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {checks.map((check) => (
                <div key={`check-${check.name}`} className="rounded-lg border border-gray-100 bg-gray-50 p-4 flex items-start justify-between gap-3">
                  <div><p className="font-medium text-gray-900">{check.name}</p><p className="text-xs text-gray-500 mt-1 line-clamp-2">{check.detail}</p></div>
                  <StatusBadge status={check.status} variant={statusVariant(check.status)} />
                </div>
              ))}
              {services.map((service) => (
                <div key={`service-${service.name}`} className="rounded-lg border border-gray-100 bg-gray-50 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div><p className="font-medium text-gray-900">{service.name}</p><p className="text-xs text-gray-500 mt-1">{service.successCount} success · {service.failureCount} failed</p></div>
                    <StatusBadge status={service.isStale ? "Stale" : service.status} variant={statusVariant(service.isStale ? "Stale" : service.status)} />
                  </div>
                  {service.lastHeartbeat && <p className="text-[11px] text-gray-400 mt-3">Heartbeat {new Date(service.lastHeartbeat).toLocaleString()}</p>}
                </div>
              ))}
              {!loading && checks.length === 0 && services.length === 0 && <p className="text-sm text-gray-500">No health data is available.</p>}
            </div>
          </section>

          <section className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="flex items-center justify-between mb-5">
              <div><h3 className="font-semibold text-gray-900">Recent activity</h3><p className="text-sm text-gray-500">Latest admin actions</p></div>
              <Link href="/admin/audit-logs" className="text-sm font-medium text-blue-600 hover:text-blue-700">All logs</Link>
            </div>
            <div className="space-y-4">
              {logs.map((log) => (
                <Link key={log.id} href={`/admin/audit-logs/${log.id}`} className="flex gap-3 group">
                  <span className="mt-1 h-8 w-8 shrink-0 rounded-full bg-blue-50 text-blue-600 flex items-center justify-center material-symbols-outlined text-[16px]">history</span>
                  <span className="min-w-0">
                    <span className="block text-sm font-medium text-gray-800 group-hover:text-blue-600 truncate">{log.actionType.replaceAll("_", " ")}</span>
                    <span className="block text-xs text-gray-500 truncate">{log.actorEmail ?? "Administrator"} · {log.targetTable}</span>
                    <span className="block text-[11px] text-gray-400 mt-1">{new Date(log.createdAt).toLocaleString()}</span>
                  </span>
                </Link>
              ))}
              {!loading && logs.length === 0 && <p className="text-sm text-gray-500">No recent audit activity.</p>}
            </div>
          </section>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {[
            { href: "/admin/audit-logs", icon: "receipt_long", title: "Audit Logs", text: "Inspect actors, targets and before/after changes." },
            { href: "/admin/service-health", icon: "monitoring_heart", title: "Background Services", text: "Review heartbeats, successes and failures." },
            { href: "/admin/system-health", icon: "health_and_safety", title: "Platform Health", text: "Refresh infrastructure and configuration checks." },
          ].map((item) => (
            <Link key={item.href + item.title} href={item.href} className="rounded-xl border border-gray-200 bg-white p-5 hover:border-blue-300 hover:shadow-sm transition-all">
              <span className="material-symbols-outlined text-blue-600">{item.icon}</span>
              <h3 className="font-semibold text-gray-900 mt-3">{item.title}</h3>
              <p className="text-sm text-gray-500 mt-1">{item.text}</p>
            </Link>
          ))}
        </div>
      </main>
    </>
  );
}
