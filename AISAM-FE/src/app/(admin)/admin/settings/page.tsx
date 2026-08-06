"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminSettings } from "@/services/adminService";

type AdminSetting = {
  id: string;
  key: string;
  value: string;
  isSecret?: boolean;
  isConfigured?: boolean;
  description?: string;
  updatedAt?: string;
};

const categories = [
  { title: "General System", description: "Maintenance mode, limits and platform behavior", href: "/admin/settings/system", icon: "tune", prefixes: ["system."] },
  { title: "AI Providers", description: "Models, image providers and credit costs", href: "/admin/settings/ai-providers", icon: "smart_toy", prefixes: ["ai."] },
  { title: "Email & Notifications", description: "SMTP delivery and system broadcasts", href: "/admin/settings/email", icon: "mark_email_read", prefixes: ["email."] },
  { title: "Security", description: "Password and administrator account controls", href: "/admin/settings/security", icon: "security", prefixes: ["security.", "auth."] },
];

const parseBoolean = (value?: string) => {
  if (!value) return false;
  try { return Boolean(JSON.parse(value)); } catch { return value.toLowerCase() === "true"; }
};

export default function AdminSettingsPage() {
  const [settings, setSettings] = useState<AdminSetting[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchAdminSettings().then((data) => {
      setSettings((data ?? []) as AdminSetting[]);
      setLoading(false);
    });
  }, []);

  const maintenance = parseBoolean(settings.find((setting) => setting.key === "system.maintenance_mode")?.value);
  const configuredCount = settings.filter((setting) => setting.isConfigured !== false && Boolean(setting.value)).length;
  const secretCount = settings.filter((setting) => setting.isSecret).length;
  const latestUpdate = useMemo(() => settings
    .filter((setting) => setting.updatedAt)
    .sort((a, b) => new Date(b.updatedAt!).getTime() - new Date(a.updatedAt!).getTime())[0]?.updatedAt, [settings]);

  const getCategoryStatus = (prefixes: string[]) => {
    const matches = settings.filter((setting) => prefixes.some((prefix) => setting.key.startsWith(prefix)));
    if (matches.length === 0) return { label: "Not configured", variant: "warning" as const, count: 0 };
    const configured = matches.filter((setting) => setting.isConfigured !== false && Boolean(setting.value)).length;
    return configured === matches.length
      ? { label: "Configured", variant: "success" as const, count: configured }
      : { label: `${configured}/${matches.length} ready`, variant: "warning" as const, count: configured };
  };

  const recentChanges = settings
    .filter((setting) => setting.updatedAt)
    .sort((a, b) => new Date(b.updatedAt!).getTime() - new Date(a.updatedAt!).getTime())
    .slice(0, 6);

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "System Configuration" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6 bg-gray-50/60">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">System Configuration</h2>
            <p className="text-gray-500 mt-1">Configuration health, protected credentials and platform-wide controls.</p>
          </div>
          <div className="flex gap-2">
            <Link href="/admin/broadcast" className="px-4 py-2 rounded-lg border border-gray-200 bg-white text-sm font-medium text-gray-700 hover:bg-gray-50 flex items-center gap-2">
              <span className="material-symbols-outlined text-[17px]">campaign</span>New broadcast
            </Link>
            <Link href="/admin/settings/system" className="px-4 py-2 rounded-lg bg-blue-600 text-white text-sm font-medium hover:bg-blue-700 flex items-center gap-2">
              <span className="material-symbols-outlined text-[17px]">tune</span>Configure system
            </Link>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <p className="text-xs font-medium uppercase tracking-wide text-gray-500">Environment</p>
            <div className="mt-3 flex items-center justify-between"><p className="text-xl font-bold text-gray-900">{process.env.NODE_ENV}</p><StatusBadge status="Active" variant="success" /></div>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <p className="text-xs font-medium uppercase tracking-wide text-gray-500">Maintenance</p>
            <div className="mt-3 flex items-center justify-between"><p className="text-xl font-bold text-gray-900">{maintenance ? "Enabled" : "Disabled"}</p><StatusBadge status={maintenance ? "Maintenance" : "Online"} variant={maintenance ? "warning" : "success"} /></div>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <p className="text-xs font-medium uppercase tracking-wide text-gray-500">Configured values</p>
            <p className="mt-3 text-2xl font-bold text-gray-900">{loading ? "—" : `${configuredCount}/${settings.length}`}</p>
          </div>
          <div className="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
            <p className="text-xs font-medium uppercase tracking-wide text-gray-500">Protected secrets</p>
            <div className="mt-3 flex items-center justify-between"><p className="text-2xl font-bold text-gray-900">{loading ? "—" : secretCount}</p><span className="material-symbols-outlined text-emerald-500">encrypted</span></div>
          </div>
        </div>

        <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
          <section className="xl:col-span-2 bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="mb-5"><h3 className="font-semibold text-gray-900">Configuration areas</h3><p className="text-sm text-gray-500">Select an area to inspect and update its settings.</p></div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {categories.map((category) => {
                const status = getCategoryStatus(category.prefixes);
                return (
                  <Link key={category.href} href={category.href} className="rounded-xl border border-gray-200 p-5 hover:border-blue-300 hover:shadow-sm transition-all group">
                    <div className="flex items-start justify-between gap-3">
                      <span className="h-10 w-10 rounded-lg bg-blue-50 text-blue-600 flex items-center justify-center material-symbols-outlined">{category.icon}</span>
                      <StatusBadge status={status.label} variant={status.variant} />
                    </div>
                    <h4 className="mt-4 font-semibold text-gray-900 group-hover:text-blue-600">{category.title}</h4>
                    <p className="text-sm text-gray-500 mt-1">{category.description}</p>
                    <p className="text-xs text-gray-400 mt-4">{status.count} stored values</p>
                  </Link>
                );
              })}
            </div>
          </section>

          <section className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="font-semibold text-gray-900">Quick actions</h3>
            <p className="text-sm text-gray-500 mt-1">Common administration workflows.</p>
            <div className="mt-5 space-y-2">
              {[
                { href: "/admin/settings/ai-providers", icon: "smart_toy", label: "Review AI provider" },
                { href: "/admin/settings/email", icon: "outgoing_mail", label: "Review email delivery" },
                { href: "/admin/settings/security", icon: "shield", label: "Security controls" },
                { href: "/admin/broadcast", icon: "campaign", label: "Send notification" },
                { href: "/admin/system-health", icon: "monitor_heart", label: "Check system health" },
              ].map((action) => (
                <Link key={action.href + action.label} href={action.href} className="flex items-center gap-3 rounded-lg px-3 py-3 text-sm text-gray-700 hover:bg-gray-50 hover:text-blue-600">
                  <span className="material-symbols-outlined text-[19px] text-gray-400">{action.icon}</span>
                  <span className="flex-1 font-medium">{action.label}</span>
                  <span className="material-symbols-outlined text-[17px] text-gray-300">chevron_right</span>
                </Link>
              ))}
            </div>
          </section>
        </div>

        <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
          <section className="xl:col-span-2 bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="flex items-center justify-between mb-5"><div><h3 className="font-semibold text-gray-900">Integration readiness</h3><p className="text-sm text-gray-500">Configuration presence by platform capability.</p></div><StatusBadge status={settings.length ? "Loaded" : "No data"} variant={settings.length ? "success" : "warning"} /></div>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
              {[
                { name: "AI", prefix: "ai.", icon: "auto_awesome" },
                { name: "Email", prefix: "email.", icon: "mail" },
                { name: "Platform", prefix: "system.", icon: "dns" },
              ].map((integration) => {
                const values = settings.filter((setting) => setting.key.startsWith(integration.prefix));
                const ready = values.length > 0 && values.every((setting) => setting.isConfigured !== false);
                return <div key={integration.name} className="rounded-lg bg-gray-50 p-4 flex items-center gap-3"><span className="material-symbols-outlined text-blue-600">{integration.icon}</span><div className="flex-1"><p className="font-medium text-gray-900">{integration.name}</p><p className="text-xs text-gray-500">{values.length} values</p></div><span className={`h-2.5 w-2.5 rounded-full ${ready ? "bg-emerald-500" : "bg-amber-400"}`} /></div>;
              })}
            </div>
          </section>

          <section className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="font-semibold text-gray-900">Recent configuration</h3>
            <p className="text-sm text-gray-500 mt-1">Latest stored setting updates.</p>
            <div className="mt-5 space-y-4">
              {recentChanges.map((setting) => (
                <div key={setting.id} className="flex gap-3">
                  <span className="h-8 w-8 shrink-0 rounded-full bg-gray-100 text-gray-500 flex items-center justify-center material-symbols-outlined text-[16px]">edit</span>
                  <div className="min-w-0"><p className="text-sm font-medium text-gray-800 truncate">{setting.key}</p><p className="text-xs text-gray-400">{new Date(setting.updatedAt!).toLocaleString()}</p></div>
                </div>
              ))}
              {!loading && recentChanges.length === 0 && <p className="text-sm text-gray-500">No configuration history available.</p>}
            </div>
            {latestUpdate && <p className="text-[11px] text-gray-400 mt-5 pt-4 border-t border-gray-100">Last update: {new Date(latestUpdate).toLocaleString()}</p>}
          </section>
        </div>
      </main>
    </>
  );
}
