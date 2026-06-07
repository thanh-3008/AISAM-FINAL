"use client";

import { useState } from "react";
import { type Campaign, type CampaignObjective, type CreateCampaignData } from "@/services/campaignService";
import { OBJECTIVE_CONFIG, BRANDS } from "./campaignUtils";

interface EditCampaignModalProps {
  campaign: Campaign | null;
  onClose: () => void;
  onUpdate: (id: string, data: CreateCampaignData) => void;
  isLoading: boolean;
}

export default function EditCampaignModal({ campaign, onClose, onUpdate, isLoading }: EditCampaignModalProps) {
  const [name, setName] = useState(campaign?.name || "");
  const [brandId, setBrandId] = useState(campaign?.brandId || "");
  const [objective, setObjective] = useState<CampaignObjective>(campaign?.objective || "AWARENESS");
  const [budget, setBudget] = useState(campaign?.budget?.toString() || "");
  const [startDate, setStartDate] = useState(campaign?.startDate ? campaign.startDate.split("T")[0] : "");
  const [endDate, setEndDate] = useState(campaign?.endDate ? campaign.endDate.split("T")[0] : "");

  if (!campaign) return null;

  const handleSubmit = () => {
    const brand = BRANDS.find((b) => b.id === brandId);
    if (!name.trim() || !brand) return;

    onUpdate(campaign.id, {
      name,
      brandId,
      brandName: brand.name,
      objective,
      budget: budget ? parseFloat(budget) : null,
      startDate: startDate || null,
      endDate: endDate || null,
    });
  };

  const isValid = name.trim() && brandId;

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-lg bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
          {/* Header */}
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

            {/* Brand */}
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Brand</label>
              <select
                value={brandId}
                onChange={(e) => setBrandId(e.target.value)}
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/10"
              >
                <option value="">Select brand...</option>
                {BRANDS.map((b) => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
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
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Budget (USD)</label>
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
