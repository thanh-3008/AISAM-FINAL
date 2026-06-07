"use client";

import { type Campaign } from "@/services/campaignService";
import { formatCurrency, formatNumber } from "./campaignUtils";

interface CampaignStatsCardsProps {
  campaigns: Campaign[];
}

export default function CampaignStatsCards({ campaigns }: CampaignStatsCardsProps) {
  const active = campaigns.filter((c) => c.status === "ACTIVE").length;
  const paused = campaigns.filter((c) => c.status === "PAUSED").length;
  const completed = campaigns.filter((c) => c.status === "COMPLETED").length;
  const totalSpend = campaigns.reduce((sum, c) => sum + c.spend, 0);
  const totalBudget = campaigns.filter((c) => c.budget).reduce((sum, c) => sum + (c.budget || 0), 0);
  const totalImpressions = campaigns.reduce((sum, c) => sum + c.impressions, 0);
  const totalClicks = campaigns.reduce((sum, c) => sum + c.clicks, 0);
  const totalConversions = campaigns.reduce((sum, c) => sum + c.conversions, 0);

  const stats = [
    { label: "Total Campaigns", value: campaigns.length, icon: "campaign", color: "text-primary", bg: "bg-primary/10" },
    { label: "Active", value: active, icon: "play_circle", color: "text-emerald-600", bg: "bg-emerald-50" },
    { label: "Total Spend", value: formatCurrency(totalSpend), icon: "payments", color: "text-violet-600", bg: "bg-violet-50" },
    { label: "Impressions", value: formatNumber(totalImpressions), icon: "visibility", color: "text-blue-600", bg: "bg-blue-50" },
    { label: "Clicks", value: formatNumber(totalClicks), icon: "touch_app", color: "text-cyan-600", bg: "bg-cyan-50" },
    { label: "Conversions", value: formatNumber(totalConversions), icon: "conversion_path", color: "text-emerald-600", bg: "bg-emerald-50" },
  ];

  return (
    <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-5 shadow-sm animate-fade-up" style={{ animationDelay: "0.1s" }}>
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        {stats.map((s) => (
          <div key={s.label} className="flex items-center gap-3">
            <div className={`w-10 h-10 rounded-xl ${s.bg} flex items-center justify-center ${s.color} shrink-0`}>
              <span className="material-symbols-outlined text-[20px]">{s.icon}</span>
            </div>
            <div>
              <p className="text-label-xs text-outline uppercase font-medium">{s.label}</p>
              <p className="text-lg font-bold text-on-surface leading-tight">{s.value}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="pt-4 mt-4 border-t border-outline-variant/10">
        <div className="flex items-center justify-between mb-2">
          <span className="text-[11px] text-on-surface-variant font-semibold">Budget Utilization</span>
          <span className="text-[11px] text-on-surface-variant">
            {formatCurrency(totalSpend)} / {formatCurrency(totalBudget)}
          </span>
        </div>
        <div className="h-2 bg-surface-container-high rounded-full overflow-hidden">
          <div
            className="h-full bg-gradient-to-r from-primary to-secondary rounded-full transition-all duration-500"
            style={{ width: `${totalBudget > 0 ? (totalSpend / totalBudget) * 100 : 0}%` }}
          />
        </div>
        <div className="flex items-center justify-between mt-1">
          <span className="text-label-xs text-outline">{paused} paused · {completed} completed</span>
          <span className="text-label-xs text-primary font-medium">
            {totalBudget > 0 ? Math.round((totalSpend / totalBudget) * 100) : 0}% used
          </span>
        </div>
      </div>
    </div>
  );
}
