"use client";

import { useState } from "react";
import { apiClient } from "@/lib/apiClient";

export default function AdminToolsPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");
  const [result, setResult] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [batchCount, setBatchCount] = useState(5);
  const [batchLoading, setBatchLoading] = useState(false);

  const seedUser = async () => {
    setLoading(true);
    try {
      const res = await apiClient("/admin/seed/demo-user", {
        method: "POST", data: { email, password, fullName },
      });
      setResult(`Created: ${res.data.email}`);
    } catch (e: any) { setResult(`Error: ${e.message}`); }
    setLoading(false);
  };

  const seedBatch = async () => {
    setBatchLoading(true);
    try {
      const res = await apiClient("/admin/seed/batch-users", {
        method: "POST", data: { count: batchCount },
      });
      setResult(`Created ${res.data.count} demo users`);
    } catch (e: any) { setResult(`Error: ${e.message}`); }
    setBatchLoading(false);
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-[#191b24]">Admin Tools</h1>

      <section className="bg-white border border-gray-200 rounded-2xl p-6 space-y-4">
        <h2 className="text-lg font-semibold text-[#191b24]">Seed Demo User</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <input type="email" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)}
            className="px-4 py-2 rounded-xl border border-gray-200 text-sm focus:outline-none focus:border-[#004ccd]" />
          <input type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)}
            className="px-4 py-2 rounded-xl border border-gray-200 text-sm focus:outline-none focus:border-[#004ccd]" />
          <input type="text" placeholder="Full Name" value={fullName} onChange={(e) => setFullName(e.target.value)}
            className="px-4 py-2 rounded-xl border border-gray-200 text-sm focus:outline-none focus:border-[#004ccd]" />
        </div>
        <button onClick={seedUser} disabled={loading || !email || !password}
          className="px-4 py-2 rounded-xl bg-[#004ccd] text-white text-sm font-semibold disabled:opacity-50">
          {loading ? "Creating..." : "Create Demo User"}
        </button>
      </section>

      <section className="bg-white border border-gray-200 rounded-2xl p-6 space-y-4">
        <h2 className="text-lg font-semibold text-[#191b24]">Seed Batch Users</h2>
        <div className="flex items-center gap-3">
          <input type="number" value={batchCount} onChange={(e) => setBatchCount(Number(e.target.value))}
            min={1} max={50} className="w-24 px-4 py-2 rounded-xl border border-gray-200 text-sm focus:outline-none focus:border-[#004ccd]" />
          <button onClick={seedBatch} disabled={batchLoading}
            className="px-4 py-2 rounded-xl bg-[#004ccd] text-white text-sm font-semibold disabled:opacity-50">
            {batchLoading ? "Creating..." : `Create ${batchCount} Users`}
          </button>
        </div>
      </section>

      {result && (
        <div className="bg-white border border-gray-200 rounded-2xl p-4">
          <p className="text-sm text-[#191b24]">{result}</p>
        </div>
      )}
    </div>
  );
}
