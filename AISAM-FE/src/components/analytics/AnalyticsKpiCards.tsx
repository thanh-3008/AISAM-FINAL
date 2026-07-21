"use client";

import { type KpiData } from "@/services/analyticsService";
import { formatCurrency, getTrendIcon, getTrendColor, getTrendLabel } from "./analyticsUtils";

interface AnalyticsKpiCardsProps {
  kpi: KpiData;
}

export default function AnalyticsKpiCards({ kpi }: AnalyticsKpiCardsProps) {
  const sparkData = kpi.sparklines || { spend: [], engagement: [], impressions: [], clicks: [], conversions: [] };  

  const stats = [
    {
      label: "Total Reach",
      description: "Tổng lượt tiếp cận: Số lượng người dùng duy nhất đã xem quảng cáo hoặc nội dung của bạn.",
      value: kpi.totalReach,
      format: (v: number) => formatCurrency(v).replace('$', ''),
      trend: kpi.totalReachTrend,
      icon: "visibility",
      gradient: "from-blue-500 via-blue-600 to-indigo-600",
      bgGlow: "bg-blue-500/10",
      sparkline: sparkData.impressions.length > 0 ? sparkData.impressions : [0],
    },
    {
      label: "Total Interactions",
      description: "Tổng lượt tương tác: Số lượt Like, Share, Comment, Clicks thu về được.",
      value: kpi.totalInteractions,
      format: (v: number) => formatCurrency(v).replace('$', ''),
      trend: kpi.totalInteractionsTrend,
      icon: "ads_click",
      gradient: "from-purple-500 via-purple-600 to-pink-600",
      bgGlow: "bg-purple-500/10",
      sparkline: sparkData.engagement.length > 0 ? sparkData.engagement : [0],
    },
    {
      label: "Avg. CPE",
      description: "Cost Per Engagement (Chi phí mỗi tương tác): Chi phí trung bình bạn phải bỏ ra để có một lượt tương tác.",
      value: kpi.avgCpe,
      format: (v: number) => formatCurrency(v),
      trend: kpi.avgCpeTrend,
      icon: "payments",
      gradient: "from-orange-500 via-orange-600 to-red-600",
      bgGlow: "bg-orange-500/10",
      sparkline: sparkData.spend.length > 0 ? sparkData.spend : [0],
    },
    {
      label: "Published Posts",
      description: "Tổng số bài viết hoặc chiến dịch đã được xuất bản trong khoảng thời gian này.",
      value: kpi.publishedPosts,
      format: (v: number) => `${v}`,
      trend: 0,
      icon: "post_add",
      gradient: "from-emerald-500 via-emerald-600 to-teal-600",
      bgGlow: "bg-emerald-500/10",
      sparkline: [0], // No sparkline for single number
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      {stats.map((stat, index) => (
        <div
          key={stat.label}
          className="group relative bg-gradient-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/50 overflow-hidden shadow-lg hover:shadow-2xl transition-all duration-500 hover:-translate-y-2 animate-fade-up"
          style={{ animationDelay: `${index * 0.1}s` }}
        >
          {/* Animated gradient background */}
          <div className={`absolute inset-0 bg-gradient-to-br ${stat.gradient} opacity-0 group-hover:opacity-5 transition-opacity duration-500`} />
          
          {/* Glow effect */}
          <div className={`absolute -top-20 -right-20 w-40 h-40 ${stat.bgGlow} rounded-full blur-3xl opacity-0 group-hover:opacity-100 transition-opacity duration-500`} />

          <div className="relative p-6">
            {/* Header */}
            <div className="flex items-start justify-between mb-4">
              <div>
                <div className="flex items-center gap-1 mb-1 relative group/tooltip w-max">
                  <p className="text-label-sm font-semibold text-outline uppercase tracking-wider">
                    {stat.label}
                  </p>
                  <span className="material-symbols-outlined text-[14px] text-outline-variant cursor-help">info</span>
                  <div className="absolute bottom-full left-0 mb-2 w-64 p-3 bg-surface-container-highest border border-outline-variant/30 text-on-surface text-body-sm rounded-xl shadow-xl opacity-0 invisible group-hover/tooltip:opacity-100 group-hover/tooltip:visible transition-all duration-300 z-10 pointer-events-none">
                    {stat.description}
                  </div>
                </div>
                <div className="flex items-baseline gap-2">
                  <h3 className="text-headline-md bg-gradient-to-r from-on-surface to-on-surface-variant bg-clip-text text-transparent">
                    {stat.format(stat.value)}
                  </h3>
                </div>
              </div>
              <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${stat.gradient} flex items-center justify-center text-white shadow-md group-hover:scale-110 group-hover:rotate-6 transition-transform duration-500`}>
                <span className="material-symbols-outlined text-xl">{stat.icon}</span>
              </div>
            </div>

            {/* Sparkline */}
            <div className="mb-3">
              {(() => {
                const maxVal = Math.max(...stat.sparkline);
                if (maxVal <= 0 || stat.sparkline.length < 2) return <div className="h-8" />;
                return (
              <svg viewBox="0 0 100 30" className="w-full h-8" preserveAspectRatio="none">
                <defs>
                  <linearGradient id={`sparkline-${index}`} x1="0%" y1="0%" x2="0%" y2="100%">
                    <stop offset="0%" stopColor="currentColor" stopOpacity="0.3" />
                    <stop offset="100%" stopColor="currentColor" stopOpacity="0" />
                  </linearGradient>
                </defs>
                <path
                  d={`M 0 ${30 - (stat.sparkline[0] / maxVal) * 30} ${stat.sparkline
                    .map((val, i) => `L ${(i / (stat.sparkline.length - 1)) * 100} ${30 - (val / maxVal) * 30}`)
                    .join(" ")} L 100 30 L 0 30 Z`}
                  fill={`url(#sparkline-${index})`}
                  className={stat.trend >= 0 ? "text-success-green" : "text-danger-red"}
                />
                <path
                  d={`M 0 ${30 - (stat.sparkline[0] / maxVal) * 30} ${stat.sparkline
                    .map((val, i) => `L ${(i / (stat.sparkline.length - 1)) * 100} ${30 - (val / maxVal) * 30}`)
                    .join(" ")}`}
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.5"
                  className={stat.trend >= 0 ? "text-success-green" : "text-danger-red"}
                />
              </svg>
                );
              })()}
            </div>

            {/* Trend */}
            <div className={`flex items-center gap-1 text-label-sm font-semibold ${getTrendColor(stat.trend)}`}>
              <span className="material-symbols-outlined text-label-md">{getTrendIcon(stat.trend)}</span>
              <span>{getTrendLabel(stat.trend)}</span>
              <span className="text-outline font-normal text-label-xs ml-1">vs last period</span>
            </div>
          </div>

          {/* Bottom border gradient */}
          <div className={`absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r ${stat.gradient} transform scale-x-0 group-hover:scale-x-100 transition-transform duration-500 origin-left`} />
        </div>
      ))}

      <style>{`
        @keyframes fade-up {
          from { opacity: 0; transform: translateY(20px); }
          to { opacity: 1; transform: translateY(0); }
        }
        .animate-fade-up {
          animation: fade-up 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
          opacity: 0;
        }
      `}</style>
    </div>
  );
}
