"use client";
 
import { useState, useEffect } from "react";
import { type CampaignObjective, type CreateCampaignData } from "@/services/campaignService";
import { OBJECTIVE_CONFIG, getCachedBrands } from "./campaignUtils";
import { fetchSocialAccounts, fetchAdAccounts, type SocialAccount, type AdAccount } from "@/services/socialAccountService";
import { fetchProducts } from "@/services/brandService";
import { fetchContents } from "@/services/contentService";
import { PlatformIcon } from "@/lib/contentConstants";

interface CreateCampaignModalProps {
  open: boolean;
  onClose: () => void;
  onCreate: (data: CreateCampaignData) => void;
  isLoading: boolean;
}

export default function CreateCampaignModal({ open, onClose, onCreate, isLoading }: CreateCampaignModalProps) {
  const [name, setName] = useState("");
  const [brandId, setBrandId] = useState("");
  const [objective, setObjective] = useState<CampaignObjective>("AWARENESS");
  const [budget, setBudget] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [dateError, setDateError] = useState("");

  // Product & Content
  const [products, setProducts] = useState<{ id: string; name: string; brandId: string }[]>([]);
  const [selectedProductId, setSelectedProductId] = useState("");
  const [contents, setContents] = useState<{ id: string; title: string; brandId: string }[]>([]);
  const [selectedContentId, setSelectedContentId] = useState("");
  const [landingUrl, setLandingUrl] = useState("");

  const [platform, setPlatform] = useState<"facebook" | "instagram">("facebook");

  // Targeting
  const TARGETING_PRESETS: { label: string; value: string }[] = [
    { label: "Vietnam", value: '{"geo_locations":{"countries":["VN"]}}' },
    { label: "United States", value: '{"geo_locations":{"countries":["US"]}}' },
    { label: "Worldwide", value: '{"geo_locations":{"countries":[]}}' },
    { label: "Custom", value: "custom" },
  ];
  const [selectedTargeting, setSelectedTargeting] = useState(TARGETING_PRESETS[0].value);
  const [customTargeting, setCustomTargeting] = useState("");

  // Facebook account & ad account selection
  const [socialAccounts, setSocialAccounts] = useState<SocialAccount[]>([]);
  const [selectedSocialAccountId, setSelectedSocialAccountId] = useState("");
  const [adAccounts, setAdAccounts] = useState<AdAccount[]>([]);
  const [selectedAdAccount, setSelectedAdAccount] = useState("");
  const [loadingAdAccounts, setLoadingAdAccounts] = useState(false);

  useEffect(() => {
    if (open) {
      setSocialAccounts([]);
      setSelectedSocialAccountId("");
      setAdAccounts([]);
      setSelectedAdAccount("");
      setName("");
      setBrandId("");
      setObjective("AWARENESS");
      setBudget("");
      setStartDate("");
      setEndDate("");
      setDateError("");
      setSelectedTargeting(TARGETING_PRESETS[0].value);
      setCustomTargeting("");
      setLandingUrl("");
      setPlatform("facebook");
      loadSocialAccounts();
    }
  }, [open]);

  useEffect(() => {
    loadSocialAccounts();
  }, [platform]);

  function loadSocialAccounts() {
    fetchSocialAccounts().then((res) => {
      setSocialAccounts(res.data.filter((a) => a.provider === "facebook"));
    });
  }

  useEffect(() => {
    if (!selectedSocialAccountId) {
      setAdAccounts([]);
      setSelectedAdAccount("");
      return;
    }
    setLoadingAdAccounts(true);
    setAdAccounts([]);
    setSelectedAdAccount("");
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

  if (!open) return null;

  const handleSubmit = () => {
    const brand = getCachedBrands().find((b) => b.id === brandId);
    if (!name.trim() || !brand) return;
    if (!selectedAdAccount) return;
    if (startDate && endDate && new Date(startDate) > new Date(endDate)) {
      setDateError("End date must be after start date");
      return;
    }
    setDateError("");

    onCreate({
      name,
      brandId,
      brandName: brand.name,
      platform,
      productId: selectedProductId || null,
      contentId: selectedContentId || null,
      targeting: selectedTargeting === "custom" ? customTargeting || null : selectedTargeting,
      adAccountId: selectedAdAccount,
      objective,
      budget: budget ? parseFloat(budget) : null,
      startDate: startDate || null,
      endDate: endDate || null,
      landingUrl: landingUrl || null,
    });
  };

  const isValid = name.trim() && brandId && selectedAdAccount;

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-lg bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
          {/* Header */}
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between sticky top-0 bg-surface-container-lowest z-10">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">campaign</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Create Campaign</h2>
                <p className="text-label-xs text-outline">Set up your advertising campaign</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          {/* Content */}
          <div className="p-6 space-y-5">
            {/* Campaign Name */}
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

            {/* Platform */}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Platform</label>
              <div className="flex gap-2">
                <button
                  onClick={() => setPlatform("facebook")}
                  className={`flex items-center gap-2 flex-1 p-3 rounded-xl border-2 transition-all ${
                    platform === "facebook" ? "border-primary bg-primary/5" : "border-outline-variant/20 hover:border-outline-variant/40"
                  }`}
                >
                  <PlatformIcon platform="facebook" className="w-6 h-6" />
                  <span className="text-label-sm font-semibold text-on-surface">Facebook</span>
                </button>
                <button
                  onClick={() => setPlatform("instagram")}
                  className={`flex items-center gap-2 flex-1 p-3 rounded-xl border-2 transition-all ${
                    platform === "instagram" ? "border-primary bg-primary/5" : "border-outline-variant/20 hover:border-outline-variant/40"
                  }`}
                >
                  <PlatformIcon platform="instagram" className="w-6 h-6" />
                  <span className="text-label-sm font-semibold text-on-surface">Instagram</span>
                </button>
              </div>
              <p className="text-label-3xs text-outline mt-1">
                {platform === "instagram" ? "Instagram ads run through Facebook Ad Accounts" : "Facebook Ads Manager"}
              </p>
            </div>

            {/* Facebook Account */}
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

            {/* Ad Account */}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Ad Account</label>
              {selectedSocialAccountId ? (
                <select
                  value={selectedAdAccount}
                  onChange={(e) => setSelectedAdAccount(e.target.value)}
                  disabled={loadingAdAccounts}
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

            {/* Brand */}
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

            {/* Product */}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Product (optional)</label>
              <select
                value={selectedProductId}
                onChange={(e) => setSelectedProductId(e.target.value)}
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10"
              >
                <option value="">Select product...</option>
                {products.map((p) => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
            </div>

            {/* Content/Post */}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Content (optional)</label>
              <select
                value={selectedContentId}
                onChange={(e) => setSelectedContentId(e.target.value)}
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10"
              >
                <option value="">Select content...</option>
                {contents.map((c: any) => (
                  <option key={c.id} value={c.id}>
                    {c.title || "(untitled)"}
                    {c.platforms?.includes("instagram") ? " (on Instagram)" : ""}
                  </option>
                ))}
              </select>
              {platform === "instagram" && selectedContentId && (
                <p className="text-label-3xs text-primary mt-1">
                  If this content was published to Instagram, it will be boosted as an existing post instead of creating a new creative.
                </p>
              )}
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

            {/* Targeting */}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Targeting</label>
              <div className="flex flex-wrap gap-2 mb-2">
                {TARGETING_PRESETS.map((p) => (
                  <button key={p.label}
                    onClick={() => setSelectedTargeting(p.value)}
                    className={`px-3 py-1.5 rounded-lg text-label-xs font-semibold transition-all border ${
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
                  placeholder='{"geo_locations":{"countries":["US","VN"]},"age_min":18,"age_max":65}'
                  rows={3}
                  className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-[11px] font-mono text-on-surface outline-none focus:ring-2 focus:ring-primary/10 placeholder:text-outline/40"
                />
              )}
            </div>

            {/* Objective */}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Objective</label>
              <div className="grid grid-cols-2 gap-2">
                {Object.entries(OBJECTIVE_CONFIG).map(([key, config]) => (
                  <button
                    key={key}
                    onClick={() => setObjective(key as CampaignObjective)}
                    className={`flex items-center gap-2 p-3 rounded-xl border-2 transition-all ${
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

            {/* Budget */}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Total Budget (VND)</label>
              <input
                type="number"
                value={budget}
                onChange={(e) => setBudget(e.target.value)}
                placeholder="e.g. 5000"
                min="0"
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10 placeholder:text-outline/40"
              />
            </div>

            {/* Date Range */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Start Date</label>
                <input
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10"
                />
              </div>
              <div>
                <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">End Date</label>
                <input
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10"
                />
              </div>
            </div>
            {dateError && (
              <p className="text-label-xs text-red-600 mt-1">{dateError}</p>
            )}
          </div>

          {/* Footer */}
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
                <span className="material-symbols-outlined text-[16px]">add</span>
              )}
              Create Campaign
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
