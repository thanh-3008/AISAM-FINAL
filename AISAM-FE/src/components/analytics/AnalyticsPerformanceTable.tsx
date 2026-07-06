"use client";

import { type CampaignPerformance } from "@/services/analyticsService";
import { formatNumber, formatPercent, getRoasColor, getStatusColor } from "./analyticsUtils";

interface AnalyticsPerformanceTableProps {
  campaigns: CampaignPerformance[];
  onViewFullReport?: () => void;
}

export default function AnalyticsPerformanceTable({ campaigns, onViewFullReport }: AnalyticsPerformanceTableProps) {
  const maxReach = Math.max(...campaigns.map((c) => c.reach), 1);

  return (
    <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/50 overflow-hidden shadow-xl animate-fade-up" style={{ animationDelay: "0.4s" }}>
      <div className="p-6 border-b border-outline-variant/30 flex justify-between items-center bg-gradient-to-r from-primary/5 to-transparent">
        <div>
          <h4 className="text-headline-sm text-on-surface">
            Performance Breakdown
          </h4>
          <p className="text-body-sm text-outline mt-1">Campaign metrics and ROI analysis</p>
        </div>
        <button
          onClick={onViewFullReport}
          className="group flex items-center gap-2 px-3 py-1.5 rounded-lg bg-primary/10 hover:bg-primary/20 text-primary font-semibold text-label-xs transition-all duration-300 hover:scale-105"
        >
          View Full Report
          <span className="material-symbols-outlined text-label-xs transition-transform group-hover:translate-x-1">
            arrow_outward
          </span>
        </button>
      </div>
      
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="bg-surface-container-high/50">
            <tr>
              <th className="px-6 py-4 text-left text-label-sm font-bold text-outline uppercase tracking-wider">
                Campaign
              </th>
              <th className="px-6 py-4 text-left text-label-sm font-bold text-outline uppercase tracking-wider">
                Reach
              </th>
              <th className="px-6 py-4 text-left text-label-sm font-bold text-outline uppercase tracking-wider">
                Clicks
              </th>
              <th className="px-6 py-4 text-left text-label-sm font-bold text-outline uppercase tracking-wider">
                CTR
              </th>
              <th className="px-6 py-4 text-right text-label-sm font-bold text-outline uppercase tracking-wider">
                ROAS
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-outline-variant/20">
            {campaigns.map((campaign, index) => {
              const roasColor = getRoasColor(campaign.roas);
              const statusColor = getStatusColor(campaign.status);
              const reachPercent = (campaign.reach / maxReach) * 100;

              return (
                <tr
                  key={campaign.id}
                  className="group hover:bg-gradient-to-r hover:from-primary/5 hover:to-transparent transition-all duration-300 animate-fade-up"
                  style={{ animationDelay: `${0.5 + index * 0.05}s` }}
                >
                  <td className="px-6 py-5">
                    <div className="flex items-center gap-3">
                      <div className="relative">
                        <div className={`w-3 h-3 rounded-full ${statusColor} shadow-lg`} />
                        <div className={`absolute inset-0 w-3 h-3 rounded-full ${statusColor} animate-ping opacity-40`} />
                      </div>
                      <div>
                        <span className="text-body-sm font-semibold text-on-surface group-hover:text-primary transition-colors duration-300">
                          {campaign.name}
                        </span>
                        <p className="text-label-xs text-outline mt-0.5 capitalize">{campaign.status}</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-5">
                    <div className="space-y-1">
                      <div className="text-body-sm font-semibold text-on-surface tabular-nums">
                        {formatNumber(campaign.reach)}
                      </div>
                      <div className="w-full h-1.5 bg-surface-container-high rounded-full overflow-hidden">
                        <div
                          className="h-full bg-gradient-to-r from-primary to-primary-container rounded-full transition-all duration-1000"
                          style={{ width: `${reachPercent}%` }}
                        />
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-5">
                    <span className="text-body-sm font-semibold text-on-surface tabular-nums">
                      {formatNumber(campaign.clicks)}
                    </span>
                  </td>
                  <td className="px-6 py-5">
                    <span className="text-body-sm font-semibold text-on-surface tabular-nums">
                      {formatPercent(campaign.ctr)}
                    </span>
                  </td>
                  <td className="px-6 py-5 text-right">
                    <span
                      className={`inline-flex items-center gap-1.5 px-4 py-2 ${roasColor.bg} ${roasColor.text} rounded-xl font-bold text-label-sm shadow-lg group-hover:scale-110 transition-transform duration-300`}
                    >
                      <span className="material-symbols-outlined text-label-md">trending_up</span>
                      {campaign.roas}x
                    </span>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <style>{`
        @keyframes fade-up {
          from { opacity: 0; transform: translateY(10px); }
          to { opacity: 1; transform: translateY(0); }
        }
        .animate-fade-up {
          animation: fade-up 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards;
          opacity: 0;
        }
      `}</style>
    </div>
  );
}
