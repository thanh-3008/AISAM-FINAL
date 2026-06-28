"use client";

import { useState } from "react";
import { motion } from "motion/react";
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
    <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }} className="space-y-6">
      <h1 className="text-headline-sm text-on-surface">Admin Tools</h1>

      <section className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6 space-y-4">
        <h2 className="text-headline-sm text-on-surface">Seed Demo User</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <input type="email" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)}
            className="px-4 py-2 rounded-xl border border-outline-variant/30 text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30" />
          <input type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)}
            className="px-4 py-2 rounded-xl border border-outline-variant/30 text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30" />
          <input type="text" placeholder="Full Name" value={fullName} onChange={(e) => setFullName(e.target.value)}
            className="px-4 py-2 rounded-xl border border-outline-variant/30 text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30" />
        </div>
        <button onClick={seedUser} disabled={loading || !email || !password}
          className="px-4 py-2 rounded-xl bg-primary text-on-primary text-body-sm font-semibold disabled:opacity-50 hover:bg-primary-container transition-colors">
          {loading ? "Creating..." : "Create Demo User"}
        </button>
      </section>

      <section className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6 space-y-4">
        <h2 className="text-headline-sm text-on-surface">Seed Batch Users</h2>
        <div className="flex items-center gap-3">
          <input type="number" value={batchCount} onChange={(e) => setBatchCount(Number(e.target.value))}
            min={1} max={50} className="w-24 px-4 py-2 rounded-xl border border-outline-variant/30 text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30" />
          <button onClick={seedBatch} disabled={batchLoading}
            className="px-4 py-2 rounded-xl bg-primary text-on-primary text-body-sm font-semibold disabled:opacity-50 hover:bg-primary-container transition-colors">
            {batchLoading ? "Creating..." : `Create ${batchCount} Users`}
          </button>
        </div>
      </section>

      {result && (
        <div className="bg-surface-container-low border border-outline-variant/10 rounded-2xl p-4">
          <p className="text-body-sm text-on-surface">{result}</p>
        </div>
      )}
    </motion.div>
  );
}
