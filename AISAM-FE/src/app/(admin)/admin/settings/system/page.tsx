"use client";

import { useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";

export default function AdminSystemSettingsPage() {
  const [saved, setSaved] = useState(false);
  const [maintenanceMode, setMaintenanceMode] = useState(false);

  const handleSave = () => {
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  };

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Settings", href: "/admin/settings" }, { label: "System" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">System Settings</h2>
          <p className="text-gray-500 mt-1">Configure rate limits, maintenance mode, and feature toggles.</p>
        </div>

        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 space-y-6">
          <div>
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">General</h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-900">Maintenance Mode</p>
                  <p className="text-xs text-gray-500">Block all non-admin access to the platform</p>
                </div>
                <button
                  onClick={() => setMaintenanceMode(!maintenanceMode)}
                  className={`relative w-11 h-6 rounded-full transition-colors ${maintenanceMode ? "bg-blue-600" : "bg-gray-300"}`}
                >
                  <span className={`absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-transform ${maintenanceMode ? "translate-x-5" : "translate-x-0.5"}`} />
                </button>
              </div>
            </div>
          </div>

          <div className="border-t pt-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Rate Limits</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">API Rate Limit (requests/min)</label>
                <input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue={60} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">AI Generation Limit (per user/hour)</label>
                <input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue={20} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Max File Upload Size (MB)</label>
                <input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue={10} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Session Timeout (minutes)</label>
                <input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue={60} />
              </div>
            </div>
          </div>

          <div className="border-t pt-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Feature Toggles</h3>
            <div className="space-y-3">
              {[
                { label: "AI Image Generation", desc: "Allow users to generate images via AI", enabled: true },
                { label: "AI Video Generation", desc: "Allow users to generate videos via AI", enabled: false },
                { label: "Social Publishing", desc: "Auto-publish content to social platforms", enabled: true },
                { label: "Team Management", desc: "Allow workspace team management features", enabled: true },
              ].map((feature) => (
                <div key={feature.label} className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
                  <div>
                    <p className="text-sm font-medium text-gray-900">{feature.label}</p>
                    <p className="text-xs text-gray-500">{feature.desc}</p>
                  </div>
                  <span className={`text-xs px-2 py-1 rounded-full font-medium ${feature.enabled ? "bg-emerald-100 text-emerald-700" : "bg-gray-100 text-gray-500"}`}>
                    {feature.enabled ? "Enabled" : "Disabled"}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="border-t pt-6 flex items-center gap-3">
            <button onClick={handleSave} className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 transition-colors">
              Save Changes
            </button>
            {saved && <span className="text-sm text-emerald-600">Saved successfully!</span>}
          </div>
        </div>
      </main>
    </>
  );
}
