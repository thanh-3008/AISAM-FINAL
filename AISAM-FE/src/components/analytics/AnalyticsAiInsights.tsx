"use client";

import { useState, useEffect, useRef } from "react";
import { type AiInsight, type DateRange, fetchAiRecommendations, type AiRecommendationsResponse } from "@/services/analyticsService";

interface AnalyticsAiInsightsProps {
  insights: AiInsight[];
  dateRange?: DateRange;
  brandId?: string;
  platform?: string;
}

const COOLDOWN_MS = 8000;

export default function AnalyticsAiInsights({ insights, dateRange, brandId, platform }: AnalyticsAiInsightsProps) {
  const [aiResponse, setAiResponse] = useState<AiRecommendationsResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [cooldown, setCooldown] = useState(false);
  const cooldownRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    setAiResponse(null);
    setError(null);
  }, [dateRange, brandId, platform]);

  useEffect(() => {
    return () => {
      if (cooldownRef.current) clearTimeout(cooldownRef.current);
    };
  }, []);

  const handleAskAi = async (forceRefresh = false) => {
    if (loading || (cooldown && !forceRefresh)) return;
    setLoading(true);
    setError(null);
    setAiResponse(null);
    try {
      const response = await fetchAiRecommendations(dateRange, forceRefresh, brandId, platform);
      if (response && !response.error) {
        setAiResponse(response);
        setCooldown(true);
        if (cooldownRef.current) clearTimeout(cooldownRef.current);
        cooldownRef.current = setTimeout(() => setCooldown(false), COOLDOWN_MS);
      } else if (response?.error) {
        setError(response.message || "Failed to parse AI output.");
      } else {
        setError("AI is not available at this time.");
      }
    } catch {
      setError("Failed to get AI recommendations.");
    } finally {
      setLoading(false);
    }
  };

  const showAiResults = !!aiResponse;

  const getBorderColor = (type: AiInsight["type"]) => {
    switch (type) {
      case "recommendation": return "from-blue-500 via-cyan-500 to-blue-500";
      case "sentiment": return "from-purple-500 via-pink-500 to-purple-500";
      case "trend": return "from-orange-500 via-red-500 to-orange-500";
    }
  };

  const getIcon = (type: AiInsight["type"]) => {
    switch (type) {
      case "recommendation": return "lightbulb";
      case "sentiment": return "mood";
      case "trend": return "trending_up";
    }
  };

  const getIconGradient = (type: AiInsight["type"]) => {
    switch (type) {
      case "recommendation": return "from-blue-500 to-cyan-500";
      case "sentiment": return "from-purple-500 to-pink-500";
      case "trend": return "from-orange-500 to-red-500";
    }
  };

  return (
    <div className="relative bg-gradient-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/50 p-6 shadow-xl overflow-hidden animate-fade-up" style={{ animationDelay: "0.5s" }}>
      <div className="absolute -top-20 -right-20 w-40 h-40 bg-gradient-to-br from-primary/20 to-secondary/20 rounded-full blur-3xl animate-pulse-slow" />
      <div className="absolute -bottom-20 -left-20 w-40 h-40 bg-gradient-to-br from-secondary/20 to-tertiary/20 rounded-full blur-3xl animate-pulse-slow" style={{ animationDelay: "1s" }} />

      <div className="flex items-center gap-3 mb-6 relative z-10">
        <div className="relative">
          <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-primary to-secondary flex items-center justify-center shadow-lg">
            <span className="material-symbols-outlined text-white text-xl" style={{ fontVariationSettings: '"FILL" 1' }}>
              auto_awesome
            </span>
          </div>
          <div className="absolute inset-0 w-12 h-12 rounded-xl bg-gradient-to-br from-primary to-secondary animate-ping opacity-20" />
        </div>
        <div className="flex-1">
          <h4 className="text-headline-sm text-on-surface">
            {showAiResults ? "AI Content Review" : "AI Performance Summary"}
          </h4>
          <p className="text-body-sm text-outline mt-0.5">
            {showAiResults ? "Strengths, weaknesses and next-post actions" : "Analyze performance data for better posts"}
          </p>
        </div>
        {showAiResults && (
          <button
            onClick={() => setAiResponse(null)}
            className="p-1.5 rounded-lg hover:bg-surface-container-high text-outline hover:text-on-surface transition-colors"
            title="Back to default view"
          >
            <span className="material-symbols-outlined text-body-sm">close</span>
          </button>
        )}
      </div>

      {/* AI Response or Default Insights */}
      <div className="space-y-4 relative z-10">
        {loading && !aiResponse ? (
          <div className="space-y-3 animate-pulse">
            {[1, 2, 3].map((i) => (
              <div key={i} className="bg-surface-container-low/50 rounded-xl p-4">
                <div className="flex items-start gap-3">
                  <div className="w-8 h-8 rounded-lg bg-primary/20" />
                  <div className="flex-1 space-y-2 pt-1">
                    <div className="h-3 bg-primary/10 rounded w-3/4" />
                    <div className="h-3 bg-primary/10 rounded w-full" />
                    <div className="h-3 bg-primary/10 rounded w-2/3" />
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : showAiResults ? (
          <div className="space-y-4 animate-fade-up">
            {aiResponse.summary && (
              <div className="rounded-xl border border-indigo-200 bg-indigo-50/70 p-4">
                <p className="text-xs font-bold uppercase tracking-wider text-indigo-600 mb-1">Tổng quan</p>
                <p className="text-sm leading-relaxed text-on-surface">{aiResponse.summary}</p>
              </div>
            )}

            <AnalysisSection
              title="Điểm mạnh"
              icon="thumb_up"
              tone="positive"
              items={(aiResponse.strengths || []).map((item) => ({
                title: item.title,
                evidence: item.evidence,
                detail: item.meaning,
              }))}
            />

            <AnalysisSection
              title="Điểm yếu"
              icon="warning"
              tone="negative"
              items={(aiResponse.weaknesses || []).map((item) => ({
                title: item.title,
                evidence: item.evidence,
                detail: item.impact,
              }))}
            />

            {(aiResponse.next_post_actions || []).length > 0 && (
              <section className="rounded-xl border border-blue-200 bg-blue-50/50 p-4">
                <h5 className="mb-3 flex items-center gap-2 text-sm font-bold text-blue-700">
                  <span className="material-symbols-outlined text-[19px]">rocket_launch</span>
                  Cải thiện cho bài đăng tiếp theo
                </h5>
                <div className="space-y-3">
                  {aiResponse.next_post_actions!.map((item, index) => (
                    <div key={`${item.action}-${index}`} className="rounded-lg bg-white p-3 shadow-sm ring-1 ring-blue-100">
                      <div className="mb-1 flex items-start justify-between gap-2">
                        <p className="text-sm font-bold text-on-surface">{index + 1}. {item.action}</p>
                        <PriorityBadge priority={item.priority} />
                      </div>
                      <p className="text-xs leading-relaxed text-on-surface-variant">{item.reason}</p>
                      {item.kpi_target && <p className="mt-2 flex items-center gap-1.5 text-xs font-semibold text-blue-700"><span className="material-symbols-outlined text-[15px]">track_changes</span>Mục tiêu: {item.kpi_target}</p>}
                    </div>
                  ))}
                </div>
              </section>
            )}

            {aiResponse.data_note && <p className="rounded-lg bg-amber-50 px-3 py-2 text-xs text-amber-800 ring-1 ring-amber-100">{aiResponse.data_note}</p>}
          </div>
        ) : insights.length > 0 ? (
          insights.map((insight, index) => (
            <div
              key={insight.id}
              className="group relative animate-fade-up"
              style={{ animationDelay: `${0.6 + index * 0.1}s` }}
            >
              <div className={`absolute inset-0 bg-gradient-to-r ${getBorderColor(insight.type)} rounded-xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 blur-sm`} />
              
              <div className="relative bg-surface-container-low/80 backdrop-blur-sm p-5 rounded-xl border border-outline-variant/30 hover:border-transparent transition-all duration-500 hover:shadow-2xl hover:-translate-y-1">
                <div className="flex items-start gap-3 mb-3">
                  <div className={`w-9 h-9 rounded-lg bg-gradient-to-br ${getIconGradient(insight.type)} flex items-center justify-center shadow-md group-hover:scale-110 group-hover:rotate-6 transition-transform duration-500`}>
                    <span className="material-symbols-outlined text-white text-label-md">
                      {getIcon(insight.type)}
                    </span>
                  </div>
                  <div className="flex-1">
                    {insight.type === "sentiment" ? (
                      <>
                        <p className="text-label-xs font-bold text-outline uppercase tracking-wider mb-2">{insight.title}</p>
                        <div className="flex items-center justify-between">
                          <span className="text-label-sm font-bold bg-gradient-to-r from-on-surface to-on-surface-variant bg-clip-text text-transparent">{insight.message}</span>
                          <div className="flex gap-1">
                            <span className="w-2 h-8 bg-gradient-to-t from-purple-500 to-pink-500 rounded-full animate-pulse-slow" />
                            <span className="w-2 h-10 bg-gradient-to-t from-purple-500 to-pink-500 rounded-full animate-pulse-slow" style={{ animationDelay: "0.2s" }} />
                            <span className="w-2 h-6 bg-gradient-to-t from-purple-500/50 to-pink-500/50 rounded-full animate-pulse-slow" style={{ animationDelay: "0.4s" }} />
                          </div>
                        </div>
                      </>
                    ) : (
                      <>
                        {insight.type === "trend" && (
                          <p className="text-label-sm font-bold bg-gradient-to-r from-orange-500 to-red-500 bg-clip-text text-transparent mb-2">{insight.title}</p>
                        )}
                        <p className="text-body-sm leading-relaxed text-on-surface-variant">
                          {insight.highlight ? (
                            <>
                              {insight.message.split(insight.highlight)[0]}
                              <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-gradient-to-r from-success-green/20 to-emerald-500/20 text-success-green font-bold rounded-lg mx-1">
                                <span className="material-symbols-outlined text-label-xs">trending_up</span>
                                {insight.highlight}
                              </span>
                              {insight.message.split(insight.highlight)[1]}
                            </>
                          ) : (
                            insight.message
                          )}
                        </p>
                      </>
                    )}
                  </div>
                </div>
              </div>
            </div>
          ))
        ) : (
          <p className="text-body-sm text-outline text-center py-4">No data available yet.</p>
        )}

        {error && (
          <p className="text-body-sm text-danger-red text-center py-2">{error}</p>
        )}
      </div>

      {showAiResults && (
        <div className="mt-4 p-3 bg-surface-container-low/50 rounded-lg border border-outline-variant/30 flex items-start gap-2 relative z-10 animate-fade-up" style={{ animationDelay: "1s" }}>
          <span className="material-symbols-outlined text-outline text-[16px] mt-0.5">info</span>
          <p className="text-xs text-on-surface-variant italic leading-relaxed">
            Lưu ý: Đề xuất trên được tạo bởi AI nhằm mục đích hỗ trợ tham khảo. Vui lòng suy nghĩ kỹ lưỡng trước khi đưa ra các quyết định marketing quan trọng.
          </p>
        </div>
      )}

      {/* CTA Buttons */}
      <div className="flex gap-3 mt-6">
        <button
          onClick={() => handleAskAi(true)}
          disabled={loading || cooldown}
          className="flex-1 group relative py-3 rounded-xl bg-gradient-to-r from-primary to-secondary text-on-primary text-label-sm font-semibold shadow-lg overflow-hidden hover:shadow-xl transition-all duration-300 hover:scale-[1.02] disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
        >
          <span className="absolute inset-0 bg-gradient-to-r from-white/0 via-white/20 to-white/0 -translate-x-full group-hover:translate-x-full transition-transform duration-1000" />
          <span className="relative flex items-center justify-center gap-2">
            {loading ? (
              <>
                <span className="material-symbols-outlined text-label-sm animate-spin">progress_activity</span>
                Analyzing...
              </>
            ) : cooldown ? (
              <>
                <span className="material-symbols-outlined text-label-sm">check_circle</span>
                Wait 8s to retry
              </>
            ) : (
              <>
                Analyze Posts
                <span className="material-symbols-outlined text-label-sm group-hover:rotate-12 transition-transform duration-300">
                  auto_awesome
                </span>
              </>
            )}
          </span>
        </button>

        <button
          onClick={() => handleAskAi(true)}
          disabled={loading}
          className="px-4 py-3 rounded-xl bg-surface-container-high text-on-surface hover:text-primary hover:bg-primary/10 border border-outline-variant/30 text-label-sm font-semibold shadow-sm transition-all duration-300 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 flex items-center justify-center"
          title="Force refresh AI insights"
        >
          <span className={`material-symbols-outlined text-label-sm ${loading ? 'animate-spin' : ''}`}>
            refresh
          </span>
        </button>
      </div>

      <style>{`
        @keyframes fade-up {
          from { opacity: 0; transform: translateY(10px); }
          to { opacity: 1; transform: translateY(0); }
        }
        @keyframes pulse-slow {
          0%, 100% { opacity: 0.3; transform: scale(1); }
          50% { opacity: 0.6; transform: scale(1.1); }
        }
        .animate-fade-up {
          animation: fade-up 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards;
          opacity: 0;
        }
        .animate-pulse-slow {
          animation: pulse-slow 3s ease-in-out infinite;
        }
      `}</style>
    </div>
  );
}

function AnalysisSection({
  title,
  icon,
  tone,
  items,
}: {
  title: string;
  icon: string;
  tone: "positive" | "negative";
  items: Array<{ title: string; evidence: string; detail: string }>;
}) {
  if (items.length === 0) return null;
  const styles = tone === "positive"
    ? { section: "border-emerald-200 bg-emerald-50/50", heading: "text-emerald-700", dot: "bg-emerald-500", evidence: "text-emerald-700 bg-emerald-100/70" }
    : { section: "border-rose-200 bg-rose-50/50", heading: "text-rose-700", dot: "bg-rose-500", evidence: "text-rose-700 bg-rose-100/70" };

  return (
    <section className={`rounded-xl border p-4 ${styles.section}`}>
      <h5 className={`mb-3 flex items-center gap-2 text-sm font-bold ${styles.heading}`}>
        <span className="material-symbols-outlined text-[19px]">{icon}</span>{title}
      </h5>
      <div className="space-y-3">
        {items.map((item, index) => (
          <div key={`${item.title}-${index}`} className="rounded-lg bg-white p-3 shadow-sm">
            <p className="flex items-start gap-2 text-sm font-bold text-on-surface"><i className={`mt-1.5 h-2 w-2 shrink-0 rounded-full ${styles.dot}`} />{item.title}</p>
            <p className={`my-2 rounded-md px-2 py-1 text-xs font-semibold ${styles.evidence}`}>{item.evidence}</p>
            <p className="text-xs leading-relaxed text-on-surface-variant">{item.detail}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

function PriorityBadge({ priority }: { priority: "HIGH" | "MEDIUM" | "LOW" }) {
  const color = priority === "HIGH" ? "bg-rose-100 text-rose-700" : priority === "MEDIUM" ? "bg-amber-100 text-amber-700" : "bg-slate-100 text-slate-600";
  return <span className={`shrink-0 rounded-full px-2 py-0.5 text-[9px] font-extrabold tracking-wider ${color}`}>{priority}</span>;
}
