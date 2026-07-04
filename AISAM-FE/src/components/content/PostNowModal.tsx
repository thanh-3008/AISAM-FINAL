"use client";

import { useState, useEffect } from "react";
import { fetchContentById, publishContent, type ContentType } from "@/services/contentService";
import { fetchSocialIntegrations, type SocialIntegration } from "@/services/socialAccountService";
import { PLATFORM_CONFIG, PlatformIcon } from "@/lib/contentConstants";

interface PostNowModalProps {
  contentId: string;
  brandId?: string;
  onClose: () => void;
  onSuccess: () => void;
}

export default function PostNowModal({ contentId, brandId, onClose, onSuccess }: PostNowModalProps) {
  const [integrations, setIntegrations] = useState<SocialIntegration[]>([]);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [publishing, setPublishing] = useState(false);
  const [error, setError] = useState("");
  const [contentType, setContentType] = useState<ContentType | null>(null);

  useEffect(() => {
    if (brandId) {
      fetchSocialIntegrations(brandId).then(setIntegrations);
    }
  }, [brandId]);

  useEffect(() => {
    fetchContentById(contentId).then((content) => setContentType(content?.type ?? null));
  }, [contentId]);

  const tiktokUnavailable = contentType !== null && contentType !== "VIDEO";
  const selectableIntegrations = integrations.filter((integration) =>
    integration.isActive && !(tiktokUnavailable && integration.provider === "tiktok"));

  useEffect(() => {
    if (!tiktokUnavailable) return;
    const tiktokIds = new Set(integrations.filter((integration) => integration.provider === "tiktok").map((integration) => integration.id));
    setSelectedIds((current) => current.filter((id) => !tiktokIds.has(id)));
  }, [integrations, tiktokUnavailable]);

  const handlePublish = async () => {
    if (selectedIds.length === 0) return;
    setPublishing(true);
    setError("");
    try {
      const results = await Promise.all(selectedIds.map(async (integrationId) => ({
        integrationId,
        result: await publishContent(contentId, integrationId),
      })));
      const failed = results.filter(({ result }) => !result.success);
      if (failed.length === 0) {
        onSuccess();
      } else {
        const failedLabels = failed.map(({ integrationId, result }) => {
          const integration = integrations.find((item) => item.id === integrationId);
          return `${integration?.accountName || integrationId}: ${result.error || "Failed to publish"}`;
        });
        setSelectedIds(failed.map(({ integrationId }) => integrationId));
        setError(`${results.length - failed.length}/${results.length} published successfully. ${failedLabels.join("; ")}`);
      }
    } catch (e: any) {
      setError(e?.message || "Unknown error");
    }
    setPublishing(false);
  };

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl p-6 animate-in fade-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
          <div className="flex items-center gap-3 mb-5">
            <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
              <span className="material-symbols-outlined text-[22px]">publish</span>
            </div>
            <div>
              <h3 className="text-headline-sm text-on-surface font-bold">Post Now</h3>
              <p className="text-body-sm text-on-surface-variant">Select one or more social accounts</p>
            </div>
            <button onClick={onClose} className="ml-auto p-2 hover:bg-surface-container rounded-lg transition-all">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          {integrations.length === 0 ? (
            <div className="py-8 text-center">
              <span className="material-symbols-outlined text-4xl text-outline/30 mb-2">link_off</span>
              <p className="text-body-sm text-on-surface-variant">No social accounts linked to this brand yet.</p>
            </div>
          ) : (
            <div className="space-y-2 mb-5">
              {selectableIntegrations.map((int) => {
                const cfg = PLATFORM_CONFIG[int.provider];
                return (
                  <label key={int.id}
                    className={`flex items-center gap-3 p-4 rounded-xl border-2 cursor-pointer transition-all ${
                      selectedIds.includes(int.id)
                        ? "border-primary bg-primary/5"
                        : "border-outline-variant/20 hover:border-primary/30 bg-surface-container"
                    }`}>
                    <input type="checkbox" value={int.id} checked={selectedIds.includes(int.id)}
                      onChange={() => setSelectedIds((current) => current.includes(int.id)
                        ? current.filter((id) => id !== int.id)
                        : [...current, int.id])}
                      className="w-4 h-4 rounded text-primary focus:ring-primary/30" />
                    <PlatformIcon platform={cfg?.icon || "default"} className="w-8 h-8" />
                    <div className="flex-1 min-w-0">
                      <p className="text-label-sm font-semibold text-on-surface">{int.accountName}</p>
                      <p className="text-label-xs text-outline">{cfg?.label || int.provider}</p>
                    </div>
                  </label>
                );
              })}
            </div>
          )}

          {tiktokUnavailable && integrations.some((integration) => integration.isActive && integration.provider === "tiktok") && (
            <div className="mb-4 px-4 py-3 rounded-xl bg-amber-500/10 text-amber-700 text-label-sm flex items-start gap-2">
              <span className="material-symbols-outlined text-[17px]">warning</span>
              TikTok is hidden because TikTok Direct Post requires video content.
            </div>
          )}

          {error && (
            <div className="mb-4 px-4 py-3 rounded-xl bg-danger-red/10 text-danger-red text-label-sm font-semibold flex items-center gap-2">
              <span className="material-symbols-outlined text-[16px]">error</span>
              {error}
            </div>
          )}

          <div className="flex items-center gap-3">
            <button onClick={onClose}
              className="flex-1 py-2.5 rounded-xl border border-outline-variant/20 text-label-sm font-semibold text-on-surface-variant hover:bg-surface-container transition-all">
              Cancel
            </button>
            <button onClick={handlePublish} disabled={selectedIds.length === 0 || publishing}
              className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold hover:shadow-lg active:scale-[0.97] transition-all disabled:opacity-50 flex items-center justify-center gap-2">
              {publishing ? (
                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <><span className="material-symbols-outlined text-[16px]">send</span> Post to {selectedIds.length || 0}</>
              )}
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
