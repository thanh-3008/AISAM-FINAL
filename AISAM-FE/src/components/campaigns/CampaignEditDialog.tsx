"use client";

import { useEffect, useState } from "react";
import {
  type AdCampaignDto,
  type AdCampaignObjective,
  type UpdateAdCampaignRequest,
} from "@/services/adCampaignService";
import { CAMPAIGN_OBJECTIVES, objectiveLabels, toDateInputValue } from "./campaignDisplay";

interface BrandOption {
  id: string;
  name: string;
}

interface CampaignEditDialogProps {
  campaign: AdCampaignDto | null;
  brands: BrandOption[];
  isLoading: boolean;
  error: string | null;
  onClose: () => void;
  onSave: (id: string, payload: UpdateAdCampaignRequest) => void;
}

export default function CampaignEditDialog({ campaign, brands, isLoading, error, onClose, onSave }: CampaignEditDialogProps) {
  const [brandId, setBrandId] = useState("");
  const [adAccountId, setAdAccountId] = useState("");
  const [name, setName] = useState("");
  const [objective, setObjective] = useState<AdCampaignObjective>("TRAFFIC");
  const [budget, setBudget] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  useEffect(() => {
    if (!campaign) return;
    setBrandId(campaign.brandId);
    setAdAccountId(campaign.adAccountId);
    setName(campaign.name);
    setObjective((campaign.objective || "TRAFFIC") as AdCampaignObjective);
    setBudget(campaign.budget?.toString() ?? "");
    setStartDate(toDateInputValue(campaign.startDate));
    setEndDate(toDateInputValue(campaign.endDate));
  }, [campaign]);

  if (!campaign) return null;

  const isValid = brandId && adAccountId.trim() && name.trim();

  const handleSave = () => {
    if (!isValid) return;
    onSave(campaign.id, {
      brandId,
      adAccountId: adAccountId.trim(),
      name: name.trim(),
      objective,
      budget: budget ? Number(budget) : null,
      startDate: startDate || null,
      endDate: endDate || null,
    });
  };

  return (
    <>
      <div className="fixed inset-0 z-50 bg-black/50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <section className="w-full max-w-xl rounded-2xl bg-surface-container-lowest shadow-2xl max-h-[90vh] overflow-y-auto" onClick={(event) => event.stopPropagation()}>
          <header className="p-5 border-b border-outline-variant/20 flex items-center justify-between">
            <div>
              <h2 className="text-title-lg font-bold text-on-surface">Edit campaign</h2>
              <p className="text-label-sm text-outline">Update local campaign details.</p>
            </div>
            <button onClick={onClose} className="w-9 h-9 rounded-lg hover:bg-surface-container material-symbols-outlined text-[18px]">close</button>
          </header>

          <div className="p-5 grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Field label="Brand">
              <select value={brandId} onChange={(event) => setBrandId(event.target.value)} className="input">
                <option value="">Select brand</option>
                {brands.map((brand) => <option key={brand.id} value={brand.id}>{brand.name}</option>)}
              </select>
            </Field>
            <Field label="Ad account">
              <input value={adAccountId} onChange={(event) => setAdAccountId(event.target.value)} className="input" />
            </Field>
            <Field label="Name">
              <input value={name} onChange={(event) => setName(event.target.value)} className="input" />
            </Field>
            <Field label="Objective">
              <select value={objective} onChange={(event) => setObjective(event.target.value as AdCampaignObjective)} className="input">
                {CAMPAIGN_OBJECTIVES.map((item) => <option key={item} value={item}>{objectiveLabels[item]}</option>)}
              </select>
            </Field>
            <Field label="Budget">
              <input type="number" min="0" value={budget} onChange={(event) => setBudget(event.target.value)} className="input" />
            </Field>
            <div className="grid grid-cols-2 gap-3">
              <Field label="Start">
                <input type="date" value={startDate} onChange={(event) => setStartDate(event.target.value)} className="input" />
              </Field>
              <Field label="End">
                <input type="date" value={endDate} onChange={(event) => setEndDate(event.target.value)} className="input" />
              </Field>
            </div>
          </div>

          <footer className="p-5 border-t border-outline-variant/20 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            {error ? <p className="text-label-sm font-semibold text-danger-red">{error}</p> : <span />}
            <div className="flex justify-end gap-2">
              <button onClick={onClose} className="h-10 px-4 rounded-xl border border-outline-variant/30 text-label-sm font-semibold">Cancel</button>
              <button onClick={handleSave} disabled={!isValid || isLoading} className="h-10 px-4 rounded-xl bg-primary text-on-primary text-label-sm font-semibold disabled:opacity-50">
                Save changes
              </button>
            </div>
          </footer>
        </section>
      </div>
    </>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block">
      <span className="text-label-sm font-semibold text-on-surface-variant mb-1.5 block">{label}</span>
      <div className="[&_.input]:w-full [&_.input]:h-10 [&_.input]:rounded-xl [&_.input]:border [&_.input]:border-outline-variant/30 [&_.input]:bg-surface-container-lowest [&_.input]:px-3 [&_.input]:text-body-sm [&_.input]:outline-none [&_.input]:focus:border-primary">
        {children}
      </div>
    </label>
  );
}
