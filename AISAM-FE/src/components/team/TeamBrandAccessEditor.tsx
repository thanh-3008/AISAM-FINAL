"use client";

import { useEffect, useState } from "react";
import { getTeamBrandAccess, setTeamBrandAccess } from "@/services/teamService";
import { fetchSocialIntegrations, type SocialIntegration } from "@/services/socialAccountService";

export default function TeamBrandAccessEditor({ teamId, brandId, brandName, editable }: {
  teamId: string; brandId: string; brandName: string; editable: boolean;
}) {
  const [mode, setMode] = useState<"ALL" | "SPECIFIC">("SPECIFIC");
  const [ids, setIds] = useState<string[]>([]);
  const [channels, setChannels] = useState<SocialIntegration[]>([]);
  const [ready, setReady] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  useEffect(() => {
    let cancelled = false;
    setReady(false); setError("");
    Promise.all([getTeamBrandAccess(teamId, brandId), fetchSocialIntegrations(brandId)])
      .then(([access, available]) => {
        if (cancelled) return;
        if (!access.success || !access.data) throw new Error("Access unavailable");
        setMode(access.data.mode); setIds(access.data.channelIds); setChannels(available); setReady(true);
      }).catch(() => { if (!cancelled) setError("Không thể tải quyền channel."); });
    return () => { cancelled = true; };
  }, [teamId, brandId]);

  const save = async () => {
    setSaving(true); setError("");
    try {
      const result = await setTeamBrandAccess(teamId, brandId, mode, mode === "ALL" ? [] : ids);
      if (!result.success) setError("Không thể lưu quyền channel.");
    } catch { setError("Không thể lưu quyền channel."); }
    finally { setSaving(false); }
  };
  return <fieldset disabled={!ready || !editable || saving} className="border rounded-xl p-4 space-y-3">
    <legend className="px-2 font-semibold">{brandName}</legend>
    <label className="block">Channel access <select value={mode} onChange={e => { setMode(e.target.value as "ALL" | "SPECIFIC"); setIds([]); }}>
      <option value="ALL">ALL</option><option value="SPECIFIC">SPECIFIC</option>
    </select></label>
    {mode === "SPECIFIC" && channels.map(channel => <label className="flex gap-2" key={channel.id}>
      <input type="checkbox" checked={ids.includes(channel.id)} onChange={e => setIds(current => e.target.checked ? [...current, channel.id] : current.filter(id => id !== channel.id))} />
      {channel.targetName}
    </label>)}
    {editable && <button type="button" className="px-3 py-2 border rounded" onClick={save}>Lưu quyền channel</button>}
    {error && <p role="alert">{error}</p>}
  </fieldset>;
}
