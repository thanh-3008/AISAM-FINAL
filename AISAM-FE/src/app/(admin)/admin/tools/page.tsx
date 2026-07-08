"use client";

import { useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import { seedDemoUsers, seedDemoContent } from "@/services/adminService";

export default function AdminToolsPage() {
  const [userCount, setUserCount] = useState(5);
  const [contentCount, setContentCount] = useState(10);
  const [result, setResult] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSeedUsers = async () => {
    setLoading(true);
    const r = await seedDemoUsers(userCount);
    setResult(r ? `Created ${r.count} demo users` : "Failed");
    setLoading(false);
  };

  const handleSeedContent = async () => {
    setLoading(true);
    const r = await seedDemoContent(contentCount);
    setResult(r ? `Created ${r.created} demo content items` : "Failed");
    setLoading(false);
  };

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Dev Tools" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div><h2 className="text-2xl font-bold text-gray-900">Developer Tools</h2><p className="text-gray-500 mt-1">Seed demo data for testing and presentations.</p></div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 max-w-3xl">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 space-y-4">
            <h3 className="font-semibold text-gray-900">Seed Demo Users</h3>
            <div>
              <label className="block text-sm font-medium text-gray-700">Number of users</label>
              <input type="number" value={userCount} onChange={(e) => setUserCount(parseInt(e.target.value) || 5)} min={1} max={50} className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" />
            </div>
            <button onClick={handleSeedUsers} disabled={loading} className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:opacity-50">Create Users</button>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 space-y-4">
            <h3 className="font-semibold text-gray-900">Seed Demo Content</h3>
            <div>
              <label className="block text-sm font-medium text-gray-700">Number of items</label>
              <input type="number" value={contentCount} onChange={(e) => setContentCount(parseInt(e.target.value) || 10)} min={1} max={100} className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" />
            </div>
            <button onClick={handleSeedContent} disabled={loading} className="px-4 py-2 bg-emerald-600 text-white text-sm rounded-lg hover:bg-emerald-700 disabled:opacity-50">Create Content</button>
          </div>
        </div>

        {result && <div className={`text-sm p-4 rounded-lg max-w-3xl ${result.includes("Failed") ? "bg-red-50 text-red-700" : "bg-emerald-50 text-emerald-700"}`}>{result}</div>}
      </main>
    </>
  );
}
