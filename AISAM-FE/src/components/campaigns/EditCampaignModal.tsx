"use client";

import { useState, useEffect } from "react";
import { type Campaign, type CampaignObjective, type CreateCampaignData } from "@/services/campaignService";
import { OBJECTIVE_CONFIG, getCachedBrands } from "./campaignUtils";
import { fetchSocialAccounts, fetchAdAccounts, type SocialAccount, type AdAccount } from "@/services/socialAccountService";
import { fetchProducts } from "@/services/brandService";
import { fetchContents } from "@/services/contentService";

interface EditCampaignModalProps {
  campaign: Campaign | null;
  onClose: () => void;
  onUpdate: (id: string, data: CreateCampaignData) => void;
  isLoading: boolean;
}

const TARGETING_PRESETS: { label: string; value: string }[] = [
  { label: "Vietnam", value: '{"geo_locations":{"countries":["VN"]}}' },
  { label: "United States", value: '{"geo_locations":{"countries":["US"]}}' },
  { label: "Worldwide", value: '{"geo_locations":{"countries":[]}}' },
  { label: "Custom", value: "custom" },
];

export default function EditCampaignModal({ campaign, onClose, onUpdate, isLoading }: EditCampaignModalProps) {
  const [name, setName] = useState(campaign?.name || "");
  const [brandId, setBrandId] = useState(campaign?.brandId || "");
  const [objective, setObjective] = useState<CampaignObjective>(campaign?.objective || "AWARENESS");
  const [budget, setBudget] = useState(campaign?.budget?.toString() || "");
  const [startDate, setStartDate] = useState(campaign?.startDate ? campaign.startDate.split("T")[0] : "");
  const [endDate, setEndDate] = useState(campaign?.endDate ? campaign.endDate.split("T")[0] : "");
  const [dateError, setDateError] = useState("");

  const [products, setProducts] = useState<{ id: string; name: string; brandId: string }[]>([]);
  const [selectedProductId, setSelectedProductId] = useState(campaign?.productId || "");
  const [contents, setContents] = useState<{ id: string; title: string; brandId: string }[]>([]);
  const [selectedContentId, setSelectedContentId] = useState(campaign?.contentId || "");
  const [landingUrl, setLandingUrl] = useState(campaign?.landingUrl || "");

  const targetingPreset = TARGETING_PRESETS.find((p) => p.value === campaign?.targeting);
  const [selectedTargeting, setSelectedTargeting] = useState(targetingPreset?.value || TARGETING_PRESETS[0].value);
  const [customTargeting, setCustomTargeting] = useState(
    campaign?.targeting && !TARGETING_PRESETS.some((p) => p.value === campaign.targeting)
      ? campaign.targeting : ""
  );

  const [socialAccounts, setSocialAccounts] = useState<SocialAccount[]>([]);
  const [selectedSocialAccountId, setSelectedSocialAccountId] = useState("");
  const [adAccounts, setAdAccounts] = useState<AdAccount[]>([]);
  const [selectedAdAccount, setSelectedAdAccount] = useState(campaign?.adAccountId || "");
  const [loadingAdAccounts, setLoadingAdAccounts] = useState(false);

  useEffect(() => {
    fetchSocialAccounts().then((res) => {
      setSocialAccounts(res.data.filter((a) => a.provider === "facebook"));
    });
  }, []);

  useEffect(() => {
    if (!selectedSocialAccountId) {
      setAdAccounts([]);
      return;
    }
    setLoadingAdAccounts(true);
    setAdAccounts([]);
    fetchAdAccounts(selectedSocialAccountId).then((accounts) => {
      setAdAccounts(accounts);
      setLoadingAdAccounts(false);
    });
  }, [selectedSocialAccountId]);

  useEffect(() => {
    if (!brandId) {
      setProducts([]);
      setSelectedProductId("");
      setContents([]);
      setSelectedContentId("");
      return;
    }
    fetchProducts(brandId).then(setProducts);
    fetchContents({ brandId, pageSize: 100 }).then((res) => {
      if (res) {
        setContents(res.items.map((c) => ({ id: c.id, title: c.title, brandId: c.brandId })));
      }
    });
  }, [brandId]);

  if (!campaign) return null;

  const isDeployed = !!(campaign.facebookCampaignId);

  const handleSubmit = () => {
    const brand = getCachedBrands().find((b) => b.id === brandId);
    if (!name.trim() || !brand) return;
    if (!selectedAdAccount.trim()) return;
    if (startDate && endDate && new Date(startDate) > new Date(endDate)) {
      setDateError("End date must be after start date");
      return;
    }
    setDateError("");

    const finalTargeting = selectedTargeting === "custom"
      ? customTargeting
      : selectedTargeting;

    onUpdate(campaign.id, {
      name,
      brandId,
      brandName: brand.name,
      platform: campaign.platform || "facebook",
      productId: selectedProductId || null,
      contentId: selectedContentId || null,
      targeting: finalTargeting || null,
      adAccountId: selectedAdAccount.trim(),
      objective,
      budget: budget ? parseFloat(budget) : null,
      startDate: startDate || null,
      endDate: endDate || null,
      landingUrl: landingUrl || null,
    });
  };

  const isValid = name.trim() && brandId && selectedAdAccount.trim();

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-lg bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between sticky top-0 bg-surface-container-lowest z-10">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">edit</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Edit Campaign</h2>
                <p className="text-label-xs text-outline">Update campaign details</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="p-6 space-y-5">
            {isDeployed && (
              <div className="p-3 bg-amber-50 border border-amber-200 rounded-xl flex items-start gap-2">
                <span className="material-symbols-outlined text-[16px] text-amber-600 mt-0.5">lock</span>
                <p className="text-[11px] text-amber-700">
                  Campaign already deployed to Facebook. Budget, dates, targeting, product, and content are locked. You can still change the campaign name and landing URL.
                </p>
              </div>
            )}

            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Campaign Name</label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g. Summer Sale 2024"
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10 placeholder:text-outline/40"
              />
            </div>

            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Facebook Account</label>
              <select
                value={selectedSocialAccountId}
                onChange={(e) => setSelectedSocialAccountId(e.target.value)}
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10"
              >
                <option value="">Select Facebook account...</option>
                {socialAccounts.map((acc) => (
                  <option key={acc.id} value={acc.id}>{acc.accountName}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Ad Account</label>
              {selectedSocialAccountId ? (
                <select
                  value={selectedAdAccount}
                  onChange={(e) => setSelectedAdAccount(e.target.value)}
                  disabled={loadingAdAccounts || isDeployed}
                  className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10 disabled:opacity-50"
                >
                  {loadingAdAccounts ? (
                    <option value="">Loading...</option>
                  ) : adAccounts.length === 0 ? (
                    <option value="">No ad accounts found</option>
                  ) : (
                    <>
                      <option value="">Select ad account...</option>
                      {adAccounts.map((acc) => (
                        <option key={acc.id} value={acc.id}>
                          {acc.name} ({acc.id}) — {acc.currency}
                        </option>
                      ))}
                    </>
                  )}
                </select>
              ) : (
                <div className="w-full p-3 bg-surface-container-high border border-outline-variant/20 rounded-xl text-body-sm text-outline">
                  Select a Facebook account first
                </div>
              )}
            </div>

            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Brand</label>
              <select
                value={brandId}
                onChange={(e) => setBrandId(e.target.value)}
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10"
              >
                <option value="">Select brand...</option>
                {getCachedBrands().map((b) => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Product (optional)</label>
              <select
                value={selectedProductId}
                onChange={(e) => setSelectedProductId(e.target.value)}
                disabled={isDeployed}
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10 disabled:opacity-50"
              >
                <option value="">Select product...</option>
                {products.map((p) => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Content (optional)</label>
              <select
                value={selectedContentId}
                onChange={(e) => setSelectedContentId(e.target.value)}
                disabled={isDeployed}
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10 disabled:opacity-50"
              >
                <option value="">Select content...</option>
                {contents.map((c) => (
                  <option key={c.id} value={c.id}>{c.title || "(untitled)"}</option>
                ))}
              </select>
            </div>

            {/* Landing URL */}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Landing URL (optional)</label>
              <input
                type="url"
                value={landingUrl}
                onChange={(e) => setLandingUrl(e.target.value)}
                placeholder="https://example.com/product-page"
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10 placeholder:text-outline/40"
              />
              <p className="text-label-3xs text-outline mt-1">Default: Facebook page URL</p>
            </div>

            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Targeting</label>
              <div className="flex flex-wrap gap-2 mb-2">
                {TARGETING_PRESETS.map((p) => (
                  <button key={p.label}
                    onClick={() => setSelectedTargeting(p.value)}
                    disabled={isDeployed}
                    className={`px-3 py-1.5 rounded-lg text-label-xs font-semibold transition-all border disabled:opacity-40 disabled:cursor-not-allowed ${
                      selectedTargeting === p.value
                        ? "border-primary bg-primary/5 text-primary"
                        : "border-outline-variant/20 text-outline hover:text-on-surface"
                    }`}
                  >
                    {p.label}
                  </button>
                ))}
              </div>
              {selectedTargeting === "custom" && (
                <textarea
                  value={customTargeting}
                  onChange={(e) => setCustomTargeting(e.target.value)}
                  disabled={isDeployed}
                  placeholder='{"geo_locations":{"countries":["US","VN"]},"age_min":18,"age_max":65}'
                  rows={3}
                  className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-[11px] font-mono text-on-surface outline-none focus:ring-2 focus:ring-primary/10 placeholder:text-outline/40 disabled:opacity-50"
                />
              )}
            </div>

            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Objective</label>
              <div className="grid grid-cols-2 gap-2">
                {Object.entries(OBJECTIVE_CONFIG).map(([key, config]) => (
                  <button
                    key={key}
                    onClick={() => setObjective(key as CampaignObjective)}
                    disabled={isDeployed}
                    className={`flex items-center gap-2 p-3 rounded-xl border-2 transition-all disabled:opacity-40 disabled:cursor-not-allowed ${
                      objective === key
                        ? "border-primary bg-primary/5"
                        : "border-outline-variant/20 hover:border-outline-variant/40"
                    }`}
                  >
                    <div className={`w-8 h-8 rounded-lg ${config.bg} flex items-center justify-center`}>
                      <span className={`material-symbols-outlined text-[16px] ${config.color}`}>{config.icon}</span>
                    </div>
                    <span className="text-[11px] font-semibold text-on-surface">{config.label}</span>
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Total Budget (VND)</label>
              <input
                type="number"
                value={budget}
                onChange={(e) => setBudget(e.target.value)}
                placeholder="e.g. 5000"
                min="1"
                disabled={isDeployed}
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10 placeholder:text-outline/40 disabled:opacity-50"
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Start Date</label>
                <input
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  disabled={isDeployed}
                  className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10 disabled:opacity-50"
                />
              </div>
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">End Date</label>
                <input
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  disabled={isDeployed}
                  className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10 disabled:opacity-50"
                />
              </div>
            </div>
            {dateError && (
              <p className="text-label-xs text-red-600 mt-1">{dateError}</p>
            )}
          </div>

          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3 sticky bottom-0 bg-surface-container-lowest">
            <button
              onClick={onClose}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all"
            >
              Cancel
            </button>
            <button
              onClick={handleSubmit}
              disabled={!isValid || isLoading}
              className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 disabled:opacity-50 disabled:hover:scale-100 flex items-center gap-2"
            >
              {isLoading ? (
                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <span className="material-symbols-outlined text-[16px]">save</span>
              )}
              Save Changes
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
