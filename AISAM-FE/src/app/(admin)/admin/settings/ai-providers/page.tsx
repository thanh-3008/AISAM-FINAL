"use client";

import { useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";

export default function AdminAiProvidersPage() {
  const [saved, setSaved] = useState(false);

  const handleSave = () => {
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  };

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Settings", href: "/admin/settings" }, { label: "AI Providers" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">AI Providers</h2>
          <p className="text-gray-500 mt-1">Configure AI model providers and API settings.</p>
        </div>

        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 space-y-6">
          <div>
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Text Generation</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Default Model</label>
                <select className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue="gemini-2.5-flash">
                  <option>gemini-2.5-flash</option>
                  <option>gemini-2.5-pro</option>
                  <option>gemini-1.5-flash</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Credit Cost per Generation</label>
                <input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue={1} />
              </div>
            </div>
          </div>

          <div className="border-t pt-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Image Generation</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Provider</label>
                <select className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue="vertex-ai">
                  <option>vertex-ai</option>
                  <option>openrouter</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Credit Cost per Image</label>
                <input type="number" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" defaultValue={5} />
              </div>
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
