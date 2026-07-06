"use client";

import { useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";

export default function AdminEmailSettingsPage() {
  const [saved, setSaved] = useState(false);

  const handleSave = () => {
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  };

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
              <div>
                <label className="block text-sm font-medium text-gray-700">SMTP Host</label>
                <input type="text" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue="smtp.gmail.com" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">SMTP Port</label>
                <input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue={587} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Username</label>
                <input type="text" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue="noreply@aisam.com" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Password</label>
                <input type="password" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue="********" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">From Name</label>
                <input type="text" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue="AISAM" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">From Email</label>
                <input type="email" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue="noreply@aisam.com" />
              </div>
            </div>
          </div>

          <div className="border-t pt-6 flex items-center gap-3">
            <button onClick={handleSave} className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 transition-colors">
              Save Changes
            </button>
            <button className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors">
              Send Test Email
            </button>
            {saved && <span className="text-sm text-emerald-600">Saved successfully!</span>}
          </div>
        </div>
      </main>
    </>
  );
}
