"use client";

import { PlatformIcon } from "@/lib/contentConstants";
import { type SocialAccount, getAccountStatus } from "@/services/socialAccountService";
import { PLATFORM_INFO, formatNumber } from "./socialUtils";

interface SocialStatsCardsProps {
  allAccounts: SocialAccount[];
}

export default function SocialStatsCards({ allAccounts }: SocialStatsCardsProps) {
  const connected = allAccounts.filter((a) => getAccountStatus(a) === "connected").length;
  const expired = allAccounts.filter((a) => getAccountStatus(a) === "expired").length;
  const error = allAccounts.filter((a) => getAccountStatus(a) === "error").length;
  const totalTargets = allAccounts.reduce((sum, a) => sum + (a.targets?.length || 0), 0);
  const totalFollowers = allAccounts.reduce((sum, a) => sum + (a.followers || 0), 0);

  const platformCounts = Object.keys(PLATFORM_INFO).map((platform) => ({
    platform,
    count: allAccounts.filter((a) => a.provider === platform).length,
    info: PLATFORM_INFO[platform],
  }));

  const maxPlatformCount = Math.max(...platformCounts.map((p) => p.count), 1);

  const stats = [
    { label: "Total", value: allAccounts.length, icon: "link", color: "text-primary", bg: "bg-primary/10" },
    { label: "Connected", value: connected, icon: "check_circle", color: "text-emerald-600", bg: "bg-emerald-50" },
    { label: "Expired", value: expired, icon: "warning", color: "text-amber-600", bg: "bg-amber-50" },
    { label: "Error", value: error, icon: "error", color: "text-danger-red", bg: "bg-danger-red/10" },
    { label: "Followers", value: formatNumber(totalFollowers), icon: "group", color: "text-sky-600", bg: "bg-sky-50" },
  ];

  return (
    <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-5 shadow-sm animate-fade-up" style={{ animationDelay: "0.1s" }}>
      <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-4">
        {stats.map((s) => (
          <div key={s.label} className="flex items-center gap-3">
            <div className={`w-9 h-9 rounded-lg ${s.bg} flex items-center justify-center ${s.color} shrink-0`}>
              <span className="material-symbols-outlined text-[18px]">{s.icon}</span>
            </div>
            <div>
              <p className="text-[10px] text-outline uppercase font-medium">{s.label}</p>
              <p className="text-lg font-bold text-on-surface leading-tight">{s.value}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="pt-4 border-t border-outline-variant/10">
        <div className="flex items-center justify-between mb-3">
          <span className="text-[11px] text-on-surface-variant font-semibold uppercase tracking-wide">Platforms</span>
          <span className="text-[11px] text-on-surface-variant font-medium">{totalTargets} targets</span>
        </div>
        <div className="flex items-center gap-6">
          {platformCounts.map(({ platform, count, info }) => (
            <div key={platform} className="flex items-center gap-2.5 flex-1">
              <div className={`w-6 h-6 rounded-md bg-gradient-to-br ${info.gradient} flex items-center justify-center shrink-0 shadow-sm`}>
                <PlatformIcon platform={platform} className="w-3.5 h-3.5 text-white" />
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between mb-1">
                  <span className="text-[11px] font-semibold text-on-surface">{info.label}</span>
                  <span className="text-[12px] font-bold text-on-surface ml-2">{count}</span>
                </div>
                <div className="h-1.5 bg-outline/10 rounded-full overflow-hidden">
                  <div className={`h-full bg-gradient-to-r ${info.gradient} rounded-full transition-all duration-500`} style={{ width: `${(count / maxPlatformCount) * 100}%` }} />
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
