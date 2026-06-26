"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
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
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-[#191b24]">System Configuration</h1>

      <section className="bg-white border border-gray-200 rounded-2xl p-6 space-y-4">
        <h2 className="text-lg font-semibold text-[#191b24]">AI Provider</h2>
        <div>
          <label className="text-sm font-medium text-[#424656]">AI Provider</label>
          <input type="text" value={aiProvider} onChange={(e) => setAiProvider(e.target.value)}
            placeholder="gemini" className="w-full max-w-sm mt-1 px-4 py-2 rounded-xl border border-gray-200 text-sm focus:outline-none focus:border-[#004ccd]" />
        </div>
        <button onClick={() => mutation.mutate({ aiProvider })} disabled={mutation.isPending}
          className="px-4 py-2 rounded-xl bg-[#004ccd] text-white text-sm font-semibold disabled:opacity-50">
          {mutation.isPending ? "Saving..." : "Save Configuration"}
        </button>
        {saved && <p className="text-sm text-[#198038]">Configuration saved.</p>}
      </section>
    </div>
  );
}
