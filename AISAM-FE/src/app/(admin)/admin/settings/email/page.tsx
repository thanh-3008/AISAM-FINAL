"use client";

import { useState, useEffect } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import { fetchAdminSettings, saveAdminSettings } from "@/services/adminService";

export default function AdminEmailSettingsPage() {
  const [saved, setSaved] = useState(false);
  const [loading, setLoading] = useState(true);
  const [smtpHost, setSmtpHost] = useState("smtp.gmail.com");
  const [smtpPort, setSmtpPort] = useState("587");
  const [username, setUsername] = useState("noreply@aisam.com");
  const [fromName, setFromName] = useState("AISAM");
  const [fromEmail, setFromEmail] = useState("noreply@aisam.com");

  useEffect(() => {
    fetchAdminSettings().then((settings) => {
      if (settings) {
        for (const s of settings) {
          if (s.key === "email.smtp_host") { try { setSmtpHost(JSON.parse(s.value)); } catch {} }
          if (s.key === "email.smtp_port") { try { setSmtpPort(JSON.parse(s.value)); } catch {} }
          if (s.key === "email.username") { try { setUsername(JSON.parse(s.value)); } catch {} }
          if (s.key === "email.from_name") { try { setFromName(JSON.parse(s.value)); } catch {} }
          if (s.key === "email.from_email") { try { setFromEmail(JSON.parse(s.value)); } catch {} }
        }
      }
      setLoading(false);
    });
  }, []);

  const handleSave = async () => {
    const ok = await saveAdminSettings({
      "email.smtp_host": JSON.stringify(smtpHost),
      "email.smtp_port": JSON.stringify(smtpPort),
      "email.username": JSON.stringify(username),
      "email.from_name": JSON.stringify(fromName),
      "email.from_email": JSON.stringify(fromEmail),
    });
    if (ok) {
      setSaved(true);
      setTimeout(() => setSaved(false), 2000);
    }
  };

  if (loading) return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Settings", href: "/admin/settings" }, { label: "Email" }]} />
      <main className="flex-1 p-8"><div className="animate-pulse"><div className="h-8 w-64 bg-gray-200 rounded" /></div></main>
    </>
  );

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Settings", href: "/admin/settings" }, { label: "Email" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Email Settings</h2>
          <p className="text-gray-500 mt-1">Configure SMTP server and email notification templates.</p>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 space-y-6">
          <div>
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">SMTP Configuration</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div><label className="block text-sm font-medium text-gray-700">SMTP Host</label><input type="text" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" value={smtpHost} onChange={(e) => setSmtpHost(e.target.value)} /></div>
              <div><label className="block text-sm font-medium text-gray-700">SMTP Port</label><input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" value={smtpPort} onChange={(e) => setSmtpPort(e.target.value)} /></div>
              <div><label className="block text-sm font-medium text-gray-700">Username</label><input type="text" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" value={username} onChange={(e) => setUsername(e.target.value)} /></div>
              <div><label className="block text-sm font-medium text-gray-700">Password</label><input type="password" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" /></div>
              <div><label className="block text-sm font-medium text-gray-700">From Name</label><input type="text" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" value={fromName} onChange={(e) => setFromName(e.target.value)} /></div>
              <div><label className="block text-sm font-medium text-gray-700">From Email</label><input type="email" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" value={fromEmail} onChange={(e) => setFromEmail(e.target.value)} /></div>
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
