"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { motion } from "motion/react";
import { apiClient } from "@/lib/apiClient";
import { useState } from "react";

export default function AdminConfigPage() {
  const qc = useQueryClient();
  const [aiProvider, setAiProvider] = useState("");
  const [saved, setSaved] = useState(false);

  const { data } = useQuery({
    queryKey: ["admin", "config"],
    queryFn: async () => {
      const res = await apiClient("/admin/config");
      return res.data?.config || {};
    },
  });

  const mutation = useMutation({
    mutationFn: (config: Record<string, any>) =>
      apiClient("/admin/config", { method: "PUT", data: { config } }),
    onSuccess: () => { setSaved(true); qc.invalidateQueries({ queryKey: ["admin", "config"] }); },
  });

  return (
    <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }} className="space-y-6">
      <h1 className="text-headline-sm text-on-surface">System Configuration</h1>

      <section className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6 space-y-4">
        <h2 className="text-headline-sm text-on-surface">AI Provider</h2>
        <div>
          <label className="text-label-sm font-medium text-on-surface-variant">AI Provider</label>
          <input type="text" value={aiProvider} onChange={(e) => setAiProvider(e.target.value)}
            placeholder="gemini" className="w-full max-w-sm mt-1 px-4 py-2 rounded-xl border border-outline-variant/30 text-body-sm focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/30" />
        </div>
        <button onClick={() => mutation.mutate({ aiProvider })} disabled={mutation.isPending}
          className="px-4 py-2 rounded-xl bg-primary text-on-primary text-body-sm font-semibold disabled:opacity-50 hover:bg-primary-container transition-colors">
          {mutation.isPending ? "Saving..." : "Save Configuration"}
        </button>
        {saved && <p className="text-body-sm text-success-green">Configuration saved.</p>}
      </section>
    </motion.div>
  );
}
