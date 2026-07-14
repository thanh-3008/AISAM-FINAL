"use client";

import { useState, useEffect } from "react";
import { bulkCreateSchedules, type BulkItemResult } from "@/services/scheduleService";
import { fetchSocialIntegrations, type SocialIntegration } from "@/services/socialAccountService";
import { PLATFORM_CONFIG, PlatformIcon } from "@/lib/contentConstants";

interface BulkScheduleModalProps {
  items: { id: string; contentId: string; title?: string; brandId?: string; brandName?: string; type?: string }[];
  onClose: () => void;
  onSuccess: (message: string) => void;
}

export default function BulkScheduleModal({ items, onClose, onSuccess }: BulkScheduleModalProps) {
  const [scheduledAt, setScheduledAt] = useState("");
  const [integrations, setIntegrations] = useState<SocialIntegration[]>([]);
  const [selectedIntegrationIds, setSelectedIntegrationIds] = useState<string[]>([]);
  const [scheduling, setScheduling] = useState(false);
  const [error, setError] = useState("");
  const [itemResults, setItemResults] = useState<BulkItemResult[] | null>(null);
  const [showAllErrors, setShowAllErrors] = useState(false);
  const [stagger, setStagger] = useState(false);
  const [intervalMinutes, setIntervalMinutes] = useState(60);

  const uniqueBrandIds = [...new Set(items.map(i => i.brandId).filter(Boolean))];
  const singleBrand = uniqueBrandIds.length === 1;
  const brandName = items[0]?.brandName;
  const tiktokUnavailable = items.some((item) => item.type && item.type !== "VIDEO");

  useEffect(() => {
    if (singleBrand && uniqueBrandIds[0]) {
      fetchSocialIntegrations(uniqueBrandIds[0]).then(setIntegrations);
    }
  }, [singleBrand, uniqueBrandIds]);

  useEffect(() => {
    if (!tiktokUnavailable) return;
    const tiktokIds = new Set(integrations.filter((integration) => integration.provider === "tiktok").map((integration) => integration.id));
    setSelectedIntegrationIds((current) => current.filter((id) => !tiktokIds.has(id)));
  }, [integrations, tiktokUnavailable]);

  const handleSchedule = async () => {
    if (!scheduledAt) { setError("Please select a date and time."); return; }
    if (selectedIntegrationIds.length === 0) { setError("Please select at least one social account."); return; }
    setScheduling(true);
    setError("");

    const base = new Date(scheduledAt);
    const result = await bulkCreateSchedules({
      items: items.flatMap((i, idx) => {
        const time = new Date(base);
        if (stagger) time.setMinutes(time.getMinutes() + idx * intervalMinutes);
        return selectedIntegrationIds.map((integrationId) => ({
          contentId: i.contentId,
          integrationId,
          scheduledAt: time.toISOString(),
        }));
      }),
    });

    setItemResults(result.results ?? null);
    const successCount = (result.results ?? []).filter(r => r.success).length;
    const failedCount = (result.results ?? []).filter(r => !r.success).length;
    if (result.success && successCount === 0) {
      setError(result.message || "All items failed. Check that each content is Approved.");
    } else if (result.success && failedCount > 0) {
      setError(`${successCount}/${items.length * selectedIntegrationIds.length} created. ${failedCount} failed.`);
    } else if (result.success) {
      onSuccess(result.message || "Schedules created.");
    } else {
      setError(result.message || "Failed to schedule.");
    }
    setScheduling(false);
  };

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl p-6 animate-in fade-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
          <div className="flex items-center gap-3 mb-5">
            <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
              <span className="material-symbols-outlined text-[22px]">calendar_month</span>
            </div>
            <div>
              <h3 className="text-headline-sm text-on-surface font-bold">Schedule {items.length} Content</h3>
              <p className="text-body-sm text-on-surface-variant">{items.map(i => i.title || i.brandName).filter(Boolean).join(", ")}</p>
            </div>
            <button onClick={onClose} className="ml-auto p-2 hover:bg-surface-container rounded-lg transition-all">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="space-y-4 mb-5">
            <div>
              <label className="text-label-sm font-semibold text-on-surface mb-1.5 block">Start Date & Time</label>
              <input type="datetime-local" value={scheduledAt} onChange={(e) => setScheduledAt(e.target.value)}
                className="w-full px-4 py-2.5 rounded-xl border border-outline-variant/20 bg-surface-container text-on-surface text-body-sm focus:outline-none focus:ring-2 focus:ring-primary/30" />
            </div>

            {items.length > 1 && (
              <div className="flex items-center gap-3 p-3 rounded-xl bg-surface-container">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" checked={stagger} onChange={(e) => setStagger(e.target.checked)}
                    className="w-4 h-4 rounded accent-primary" />
                  <span className="text-label-sm text-on-surface font-medium">Stagger posting</span>
                </label>
                {stagger && (
                  <div className="flex flex-col gap-2 ml-auto">
                    <div className="flex items-center gap-1.5">
                      <span className="text-label-sm text-outline">every</span>
                      <input type="number" min={1} max={1440} value={intervalMinutes}
                        onChange={(e) => setIntervalMinutes(Number(e.target.value) || 60)}
                        className="w-16 px-2 py-1 rounded-lg border border-outline-variant/20 bg-surface-container-lowest text-label-sm text-on-surface text-center focus:outline-none focus:ring-2 focus:ring-primary/30" />
                      <span className="text-label-sm text-outline">min</span>
                    </div>
                    <div className="flex items-center gap-1">
                      {[15, 30, 60, 120].map(p => (
                        <button key={p} onClick={() => setIntervalMinutes(p)}
                          className={`px-2 py-0.5 rounded-md text-label-xs font-medium transition-all ${intervalMinutes === p ? "bg-primary text-on-primary" : "bg-surface-container-high text-on-surface-variant hover:bg-surface-container"}`}>
                          {p < 60 ? `${p}m` : `${p / 60}h`}
                        </button>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            )}

            {stagger && items.length > 1 && scheduledAt && (
              <div className="px-3 py-2 rounded-xl bg-blue-500/5 border border-blue-500/10 text-label-xs text-outline space-y-0.5">
                {items.map((i, idx) => {
                  const t = new Date(scheduledAt);
                  t.setMinutes(t.getMinutes() + idx * intervalMinutes);
                  return (
                    <div key={i.contentId} className="flex items-center gap-2">
                      <span className="w-1.5 h-1.5 rounded-full bg-primary/40" />
                      <span className="truncate flex-1">{i.title || `#${idx + 1}`}</span>
                      <span className="font-mono shrink-0">{t.toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" })}</span>
                    </div>
                  );
                })}
              </div>
            )}

            {!singleBrand && uniqueBrandIds.length > 1 && (
              <div className="px-4 py-3 rounded-xl bg-amber-500/10 text-amber-600 text-label-sm flex items-center gap-2">
                <span className="material-symbols-outlined text-[16px]">info</span>
                Selected items belong to different brands. Make sure the social account works for all brands.
              </div>
            )}

            {brandName && (
              <div>
                <label className="text-label-sm font-semibold text-on-surface mb-1.5 block">Social Accounts</label>
                {integrations.length === 0 ? (
                  <div className="py-4 text-center text-body-sm text-outline">No social accounts linked to {brandName}.</div>
                ) : (
                  <div className="space-y-2 max-h-48 overflow-y-auto">
                    {integrations.filter(i => i.isActive).map((int) => {
                      const cfg = PLATFORM_CONFIG[int.provider];
                      const isTikTokUnavailable = tiktokUnavailable && int.provider === "tiktok";
                      return (
                        <label key={int.id}
                          className={`flex items-center gap-3 p-3 rounded-xl border-2 transition-all ${
                            isTikTokUnavailable
                              ? "cursor-not-allowed opacity-50 border-outline-variant/10 bg-surface-container"
                              : selectedIntegrationIds.includes(int.id)
                              ? "border-primary bg-primary/5"
                              : "border-outline-variant/20 hover:border-primary/30 bg-surface-container"
                          }`}>
                          <input type="checkbox" value={int.id} checked={selectedIntegrationIds.includes(int.id)}
                            disabled={isTikTokUnavailable}
                            onChange={() => setSelectedIntegrationIds((current) => current.includes(int.id)
                              ? current.filter((id) => id !== int.id)
                              : [...current, int.id])}
                            className="w-4 h-4 rounded text-primary focus:ring-primary/30" />
                          <PlatformIcon platform={cfg?.icon || "default"} className="w-7 h-7" />
                          <div className="flex-1 min-w-0">
                            <p className="text-label-sm font-semibold text-on-surface">{int.accountName}</p>
                            <p className="text-label-xs text-outline">{cfg?.label || int.provider}</p>
                          </div>
                          {isTikTokUnavailable && <span className="text-label-2xs text-amber-700">Requires video</span>}
                        </label>
                      );
                    })}
                  </div>
                )}
                {tiktokUnavailable && integrations.some((integration) => integration.isActive && integration.provider === "tiktok") && (
                  <div className="mt-2 px-3 py-2 rounded-lg bg-amber-500/10 text-amber-700 text-label-xs flex items-start gap-2">
                    <span className="material-symbols-outlined text-[15px]">warning</span>
                    TikTok is hidden because every selected content item must contain a video.
                  </div>
                )}
              </div>
            )}
          </div>

          {error && (
            <div className="mb-4 px-4 py-3 rounded-xl bg-danger-red/10 text-danger-red text-label-sm font-semibold flex items-center gap-2">
              <span className="material-symbols-outlined text-[16px]">error</span>
              {error}
            </div>
          )}
          {itemResults && itemResults.filter(r => !r.success).length > 0 && (
            <div className="mb-4 px-4 py-3 rounded-xl bg-amber-500/10 text-amber-700 text-label-xs">
              <button onClick={() => setShowAllErrors(!showAllErrors)} className="flex items-center gap-1 font-semibold mb-1">
                <span className="material-symbols-outlined text-[14px]">{showAllErrors ? "expand_less" : "expand_more"}</span>
                {itemResults.filter(r => !r.success).length} failed — tap to {showAllErrors ? "hide" : "show"}
              </button>
              {showAllErrors && (
                <div className="space-y-1 max-h-32 overflow-y-auto">
                  {itemResults.filter(r => !r.success).map((r, index) => (
                    <div key={`${r.contentId}-${index}`} className="flex items-start gap-1.5">
                      <span className="material-symbols-outlined text-[12px] mt-0.5 shrink-0">chevron_right</span>
                      <span className="truncate">{r.contentId.slice(0, 8)}... — {r.error}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          <div className="flex items-center gap-3">
            <button onClick={onClose}
              className="flex-1 py-2.5 rounded-xl border border-outline-variant/20 text-label-sm font-semibold text-on-surface-variant hover:bg-surface-container transition-all">
              Cancel
            </button>
            <button onClick={handleSchedule} disabled={!scheduledAt || selectedIntegrationIds.length === 0 || scheduling}
              className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold hover:shadow-lg active:scale-[0.97] transition-all disabled:opacity-50 flex items-center justify-center gap-2">
              {scheduling ? (
                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <><span className="material-symbols-outlined text-[16px]">calendar_month</span> Schedule ({selectedIntegrationIds.length})</>
              )}
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
