"use client";

import { useState, useEffect } from "react";
import { publishContent } from "@/services/contentService";
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
  const [selectedId, setSelectedId] = useState<string>("");
  const [publishing, setPublishing] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (brandId) {
      fetchSocialIntegrations(brandId).then(setIntegrations);
    }
  }, [brandId]);

  const handlePublish = async () => {
    if (!selectedId) return;
    setPublishing(true);
    setError("");
    try {
      const result = await publishContent(contentId, selectedId);
      if (result.success) {
        onSuccess();
      } else {
        setError(result.error || "Failed to publish. Please try again.");
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
              <p className="text-body-sm text-on-surface-variant">Select a social account to publish</p>
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
              {integrations.filter((i) => i.isActive).map((int) => {
                const cfg = PLATFORM_CONFIG[int.provider];
                return (
                  <label key={int.id}
                    className={`flex items-center gap-3 p-4 rounded-xl border-2 cursor-pointer transition-all ${
                      selectedId === int.id
                        ? "border-primary bg-primary/5"
                        : "border-outline-variant/20 hover:border-primary/30 bg-surface-container"
                    }`}>
                    <input type="radio" name="integration" value={int.id} checked={selectedId === int.id}
                      onChange={() => setSelectedId(int.id)} className="w-4 h-4 text-primary focus:ring-primary/30" />
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
            <button onClick={handlePublish} disabled={!selectedId || publishing}
              className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold hover:shadow-lg active:scale-[0.97] transition-all disabled:opacity-50 flex items-center justify-center gap-2">
              {publishing ? (
                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <><span className="material-symbols-outlined text-[16px]">send</span> Post Now</>
              )}
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
