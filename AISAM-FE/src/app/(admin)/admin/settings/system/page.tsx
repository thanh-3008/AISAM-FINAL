"use client";

import { useState, useEffect } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import { fetchAdminSettings, saveAdminSettings } from "@/services/adminService";

const features = [
  { key: "feature.ai_image", label: "AI Image Generation", desc: "Allow users to generate images via AI" },
  { key: "feature.ai_video", label: "AI Video Generation", desc: "Allow users to generate videos via AI" },
  { key: "feature.social_publish", label: "Social Publishing", desc: "Auto-publish content to social platforms" },
  { key: "feature.team_mgmt", label: "Team Management", desc: "Allow workspace team management features" },
];

export default function AdminSystemSettingsPage() {
  const [saved, setSaved] = useState(false);
  const [loading, setLoading] = useState(true);
  const [maintenanceMode, setMaintenanceMode] = useState(false);
  const [apiRateLimit, setApiRateLimit] = useState("60");
  const [aiLimit, setAiLimit] = useState("20");
  const [maxUpload, setMaxUpload] = useState("10");
  const [sessionTimeout, setSessionTimeout] = useState("60");
  const [enabledFeatures, setEnabledFeatures] = useState<string[]>([]);

  useEffect(() => {
    fetchAdminSettings().then((settings) => {
      if (settings) {
        for (const s of settings) {
          if (s.key === "system.maintenance_mode") { try { setMaintenanceMode(JSON.parse(s.value)); } catch {} }
          if (s.key === "system.rate_limit") { try { setApiRateLimit(JSON.parse(s.value)); } catch {} }
          if (s.key === "system.ai_limit") { try { setAiLimit(JSON.parse(s.value)); } catch {} }
          if (s.key === "system.max_upload") { try { setMaxUpload(JSON.parse(s.value)); } catch {} }
          if (s.key === "system.session_timeout") { try { setSessionTimeout(JSON.parse(s.value)); } catch {} }
          if (s.key === "system.enabled_features") { try { setEnabledFeatures(JSON.parse(s.value)); } catch {} }
        }
      }
      setLoading(false);
    });
  }, []);

  const toggleFeature = (key: string) => {
    setEnabledFeatures((prev) => prev.includes(key) ? prev.filter((k) => k !== key) : [...prev, key]);
  };

  const handleSave = async () => {
    const ok = await saveAdminSettings({
      "system.maintenance_mode": JSON.stringify(maintenanceMode),
      "system.rate_limit": JSON.stringify(apiRateLimit),
      "system.ai_limit": JSON.stringify(aiLimit),
      "system.max_upload": JSON.stringify(maxUpload),
      "system.session_timeout": JSON.stringify(sessionTimeout),
      "system.enabled_features": JSON.stringify(enabledFeatures),
    });
    if (ok) {
      setSaved(true);
      setTimeout(() => setSaved(false), 2000);
    }
  };

  if (loading) return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Settings", href: "/admin/settings" }, { label: "System" }]} />
      <main className="flex-1 p-8"><div className="animate-pulse"><div className="h-8 w-64 bg-gray-200 rounded" /></div></main>
    </>
  );

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
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-900">Maintenance Mode</p>
                <p className="text-xs text-gray-500">Block all non-admin access</p>
              </div>
              <button onClick={() => setMaintenanceMode(!maintenanceMode)} className={`relative w-11 h-6 rounded-full transition-colors ${maintenanceMode ? "bg-blue-600" : "bg-gray-300"}`}>
                <span className={`absolute top-0.5 w-5 h-5 rounded-full bg-white shadow transition-transform ${maintenanceMode ? "translate-x-5" : "translate-x-0.5"}`} />
              </button>
            </div>
          </div>
          <div className="border-t pt-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Rate Limits</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div><label className="block text-sm font-medium text-gray-700">API Rate Limit (rpm)</label><input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" value={apiRateLimit} onChange={(e) => setApiRateLimit(e.target.value)} /></div>
              <div><label className="block text-sm font-medium text-gray-700">AI Generation Limit (/hr)</label><input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" value={aiLimit} onChange={(e) => setAiLimit(e.target.value)} /></div>
              <div><label className="block text-sm font-medium text-gray-700">Max Upload Size (MB)</label><input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" value={maxUpload} onChange={(e) => setMaxUpload(e.target.value)} /></div>
              <div><label className="block text-sm font-medium text-gray-700">Session Timeout (min)</label><input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" value={sessionTimeout} onChange={(e) => setSessionTimeout(e.target.value)} /></div>
            </div>
          </div>
          <div className="border-t pt-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Feature Toggles</h3>
            <div className="space-y-3">
              {features.map((f) => (
                <div key={f.key} className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
                  <div>
                    <p className="text-sm font-medium text-gray-900">{f.label}</p>
                    <p className="text-xs text-gray-500">{f.desc}</p>
                  </div>
                  <button onClick={() => toggleFeature(f.key)} className={`text-xs px-2 py-1 rounded-full font-medium ${enabledFeatures.includes(f.key) ? "bg-emerald-100 text-emerald-700" : "bg-gray-100 text-gray-500"}`}>
                    {enabledFeatures.includes(f.key) ? "Enabled" : "Disabled"}
                  </button>
                </div>
              ))}
            </div>
          </div>
          <div className="border-t pt-6 flex items-center gap-3">
            <button onClick={handleSave} className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 transition-colors">Save Changes</button>
            {saved && <span className="text-sm text-emerald-600">Saved successfully!</span>}
          </div>
        </div>
      </main>
    </>
  );
}
