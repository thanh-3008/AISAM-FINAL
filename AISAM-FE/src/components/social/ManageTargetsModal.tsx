import { useEffect, useMemo, useState } from "react";
import { PlatformIcon } from "@/lib/contentConstants";
import { type AvailableTarget, type SocialAccount, getAvailableTargets, linkTargets } from "@/services/socialAccountService";
import { fetchBrands } from "@/services/brandService";
import { PLATFORM_INFO, getAccountDisplayName } from "./socialUtils";

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
  const [selectedBrandId, setSelectedBrandId] = useState("");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!account) return;

    let cancelled = false;
    const load = async () => {
      setLoading(true);
      setError(null);

      try {
        const [targets, brandList] = await Promise.all([
          getAvailableTargets(account.id),
          fetchBrands(),
        ]);

        if (cancelled) return;

        const pendingBrandId = sessionStorage.getItem("social_connect_brand_id") || "";
        const defaultBrandId = pendingBrandId && brandList.some((brand) => brand.id === pendingBrandId)
          ? pendingBrandId
          : brandList[0]?.id || "";

        setBrands(brandList);
        setAvailableTargets(targets);
        setSelectedBrandId(defaultBrandId);
        setSelectedTargetIds(targets
          .filter((target) => target.linkedBrandId === defaultBrandId)
          .map((target) => target.providerTargetId));
      } catch (err) {
        if (!cancelled) {
          setAvailableTargets([]);
          setError(err instanceof Error ? err.message : "Unable to load targets");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };

    load();
    return () => { cancelled = true; };
  }, [account]);

  useEffect(() => {
    setSelectedTargetIds(availableTargets
      .filter((target) => target.linkedBrandId === selectedBrandId)
      .map((target) => target.providerTargetId));
  }, [availableTargets, selectedBrandId]);

  const selectableTargets = useMemo(
    () => availableTargets.filter((target) => !target.linkedBrandId || target.linkedBrandId === selectedBrandId),
    [availableTargets, selectedBrandId],
  );

  const handleToggleTarget = (targetId: string) => {
    const target = availableTargets.find((item) => item.providerTargetId === targetId);
    if (!target || (target.linkedBrandId && target.linkedBrandId !== selectedBrandId)) return;

    setSelectedTargetIds((prev) =>
      prev.includes(targetId) ? prev.filter((id) => id !== targetId) : [...prev, targetId],
    );
  };

  const handleSelectAll = () => {
    if (selectableTargets.length === 0) return;
    if (selectedTargetIds.length === selectableTargets.length) {
      setSelectedTargetIds([]);
      return;
    }

    setSelectedTargetIds(selectableTargets.map((target) => target.providerTargetId));
  };

  const handleLink = async () => {
    if (!account || selectedTargetIds.length === 0 || !selectedBrandId) return;

    setLinking(true);
    setError(null);
    try {
      await linkTargets(account.id, selectedTargetIds, selectedBrandId, account.provider);
      sessionStorage.removeItem("social_connect_brand_id");
      onClose();
      void onSuccess();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to link selected targets");
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
                <h2 className="text-headline-sm font-bold text-on-surface">Choose Brand & Page</h2>
                <p className="text-label-xs text-outline">{displayName}</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="p-6 overflow-y-auto flex-1">
            {loading ? (
              <div className="flex items-center justify-center py-12">
                <span className="w-8 h-8 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
              </div>
            ) : availableTargets.length === 0 ? (
              <div className="text-center py-12">
                <span className="material-symbols-outlined text-4xl text-outline/30 mb-3">link_off</span>
                <p className="text-body-sm text-on-surface font-medium">No available targets</p>
                <p className="text-[11px] text-outline mt-1">{error || "No Page/account was returned by the provider"}</p>
              </div>
            ) : (
              <div className="space-y-4">
                <div>
                  <label className="text-[11px] text-outline font-semibold uppercase block mb-1.5">Brand đăng bài</label>
                  <select
                    value={selectedBrandId}
                    onChange={(e) => setSelectedBrandId(e.target.value)}
                    className="w-full p-2.5 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
                  >
                    {brands.length === 0 && <option value="">No brands available</option>}
                    {brands.map((brand) => (
                      <option key={brand.id} value={brand.id}>{brand.name}</option>
                    ))}
                  </select>
                  <p className="text-label-2xs text-outline mt-1.5">Một Brand có thể chọn nhiều Page. Một Page chỉ được thuộc một Brand.</p>
                </div>

                <div className="flex items-center justify-between">
                  <p className="text-[11px] text-outline font-semibold uppercase">Pages ({selectableTargets.length}/{availableTargets.length} selectable)</p>
                  <button onClick={handleSelectAll} className="text-[11px] text-primary font-semibold hover:underline">
                    {selectedTargetIds.length === selectableTargets.length ? "Deselect All" : "Select All"}
                  </button>
                </div>

                <div className="space-y-2">
                  {availableTargets.map((target) => {
                    const isLocked = !!target.linkedBrandId && target.linkedBrandId !== selectedBrandId;
                    const isSelected = selectedTargetIds.includes(target.providerTargetId);

                    return (
                      <label key={target.providerTargetId} className={`flex items-center gap-3 p-3 rounded-xl border-2 transition-all ${
                        isLocked
                          ? "border-outline-variant/10 bg-surface-container-low opacity-70 cursor-not-allowed"
                          : isSelected
                            ? "border-primary bg-primary/5 cursor-pointer"
                            : "border-outline-variant/20 hover:border-outline-variant/40 cursor-pointer"
                      }`}>
                        <input
                          type="checkbox"
                          checked={isSelected}
                          disabled={isLocked}
                          onChange={() => handleToggleTarget(target.providerTargetId)}
                          className="w-4 h-4 rounded border-outline-variant/30 text-primary focus:ring-primary/20 disabled:opacity-40"
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
                          <p className="text-[11px] font-semibold text-on-surface truncate">{target.name || target.providerTargetId}</p>
                          <p className="text-label-2xs text-outline uppercase">{target.type}{target.category ? ` · ${target.category}` : ""}</p>
                          {target.linkedBrandName && (
                            <p className={`text-label-2xs mt-0.5 ${isLocked ? "text-danger-red" : "text-emerald-600"}`}>
                              {isLocked ? `Already linked to ${target.linkedBrandName}` : "Linked to this brand"}
                            </p>
                          )}
                        </div>
                      </label>
                    );
                  })}
                </div>

                {error && <p className="text-label-xs text-danger-red">{error}</p>}
              </div>
            )}
          </div>

          {availableTargets.length > 0 && (
            <div className="p-6 border-t border-outline-variant/20 flex items-center justify-between shrink-0">
              <p className="text-[11px] text-outline">
                {selectedTargetIds.length} selected
              </p>
              <div className="flex items-center gap-3">
                <button onClick={onClose}
                  className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all">
                  Cancel
                </button>
                <button onClick={handleLink} disabled={selectedTargetIds.length === 0 || !selectedBrandId || linking}
                  className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 disabled:opacity-50 disabled:hover:scale-100 flex items-center gap-2">
                  {linking ? (
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  ) : (
                    <span className="material-symbols-outlined text-[16px]">link</span>
                  )}
                  Save Mapping
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
