import { useState, useEffect } from "react";
import { PlatformIcon } from "@/lib/contentConstants";
import { type SocialAccount, type AvailableTarget, getAvailableTargets, linkTargets } from "@/services/socialAccountService";
import { PLATFORM_INFO, getAccountDisplayName } from "./socialUtils";
import { fetchBrands } from "@/services/brandService";

interface ManageTargetsModalProps {
  account: SocialAccount | null;
  onClose: () => void;
  onSuccess: () => void;
}

export default function ManageTargetsModal({ account, onClose, onSuccess }: ManageTargetsModalProps) {
  const [availableTargets, setAvailableTargets] = useState<AvailableTarget[]>([]);
  const [selectedTargetIds, setSelectedTargetIds] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [linking, setLinking] = useState(false);
  const [brands, setBrands] = useState<{ id: string; name: string }[]>([]);
  const [brandId, setBrandId] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    if (!account) return;
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      try {
        const [targets, brandData] = await Promise.all([getAvailableTargets(account.id), fetchBrands()]);
        if (!cancelled) {
          const linkedIds = (account.targets || []).map((t) => t.providerTargetId);
          setAvailableTargets(targets.filter((t) => !linkedIds.includes(t.providerTargetId)));
          setBrands(brandData);
          setBrandId(brandData[0]?.id ?? "");
        }
      } catch {
        if (!cancelled) setAvailableTargets([]);
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [account]);

  const handleToggleTarget = (targetId: string) => {
    setSelectedTargetIds((prev) =>
      prev.includes(targetId) ? prev.filter((id) => id !== targetId) : [...prev, targetId]
    );
  };

  const handleSelectAll = () => {
    if (selectedTargetIds.length === availableTargets.length) {
      setSelectedTargetIds([]);
    } else {
      setSelectedTargetIds(availableTargets.map((t) => t.providerTargetId));
    }
  };

  const handleLink = async () => {
    if (!account || selectedTargetIds.length === 0 || !brandId) return;
    setLinking(true);
    setError("");
    try {
      await linkTargets(account.id, { targetIds: selectedTargetIds, brandId, provider: account.provider });
      onSuccess();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to link targets");
    } finally {
      setLinking(false);
    }
  };

  if (!account) return null;

  const platformInfo = PLATFORM_INFO[account.provider];
  const displayName = getAccountDisplayName(account);

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-lg bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[90vh] flex flex-col" onClick={(e) => e.stopPropagation()}>
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between shrink-0">
            <div className="flex items-center gap-3">
              <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${platformInfo?.gradient || "from-primary to-primary/70"} flex items-center justify-center text-white`}>
                <PlatformIcon platform={account.provider} className="w-5 h-5" />
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Manage Targets</h2>
                <p className="text-label-xs text-outline">{displayName}</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="p-6 overflow-y-auto flex-1">
            {error && <p className="mb-4 rounded-lg bg-danger-red/10 px-3 py-2 text-label-sm text-danger-red">{error}</p>}
            <label className="mb-4 block text-label-sm font-semibold text-on-surface-variant">
              Brand
              <select value={brandId} onChange={(event) => setBrandId(event.target.value)}
                className="mt-1.5 w-full rounded-lg border border-outline-variant/20 bg-surface-container px-3 py-2 text-body-sm">
                {brands.length === 0 && <option value="">No brands available</option>}
                {brands.map((brand) => <option key={brand.id} value={brand.id}>{brand.name}</option>)}
              </select>
            </label>
            {loading ? (
              <div className="flex items-center justify-center py-12">
                <span className="w-8 h-8 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
              </div>
            ) : availableTargets.length === 0 ? (
              <div className="text-center py-12">
                <span className="material-symbols-outlined text-4xl text-outline/30 mb-3">link_off</span>
                <p className="text-body-sm text-on-surface font-medium">No available targets</p>
                <p className="text-[11px] text-outline mt-1">All targets are already linked or none available</p>
              </div>
            ) : (
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <p className="text-[11px] text-outline font-semibold uppercase">Available Targets ({availableTargets.length})</p>
                  <button onClick={handleSelectAll} className="text-[11px] text-primary font-semibold hover:underline">
                    {selectedTargetIds.length === availableTargets.length ? "Deselect All" : "Select All"}
                  </button>
                </div>

                <div className="space-y-2">
                  {availableTargets.map((target) => (
                    <label key={target.providerTargetId} className={`flex items-center gap-3 p-3 rounded-xl border-2 cursor-pointer transition-all ${
                      selectedTargetIds.includes(target.providerTargetId)
                        ? "border-primary bg-primary/5"
                        : "border-outline-variant/20 hover:border-outline-variant/40"
                    }`}>
                      <input
                        type="checkbox"
                        checked={selectedTargetIds.includes(target.providerTargetId)}
                        onChange={() => handleToggleTarget(target.providerTargetId)}
                        className="w-4 h-4 rounded border-outline-variant/30 text-primary focus:ring-primary/20"
                      />
                      {target.profilePictureUrl ? (
                        <img src={target.profilePictureUrl} alt={target.name} className="w-8 h-8 rounded-lg object-cover" />
                      ) : (
                        <div className="w-8 h-8 rounded-lg bg-surface-container-high flex items-center justify-center">
                          <span className="material-symbols-outlined text-[16px] text-outline">
                            {target.type === "page" ? "web" : target.type === "group" ? "group" : "person"}
                          </span>
                        </div>
                      )}
                      <div className="flex-1 min-w-0">
                        <p className="text-[11px] font-semibold text-on-surface truncate">{target.name}</p>
                        <p className="text-label-2xs text-outline uppercase">{target.type}{target.category ? ` · ${target.category}` : ""}</p>
                      </div>
                    </label>
                  ))}
                </div>
              </div>
            )}
          </div>

          {availableTargets.length > 0 && (
            <div className="p-6 border-t border-outline-variant/20 flex items-center justify-between shrink-0">
              <p className="text-[11px] text-outline">
                {selectedTargetIds.length} of {availableTargets.length} selected
              </p>
              <div className="flex items-center gap-3">
                <button onClick={onClose}
                  className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all">
                  Cancel
                </button>
                <button onClick={handleLink} disabled={selectedTargetIds.length === 0 || linking}
                  className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 disabled:opacity-50 disabled:hover:scale-100 flex items-center gap-2">
                  {linking ? (
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  ) : (
                    <span className="material-symbols-outlined text-[16px]">link</span>
                  )}
                  Link Selected
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
