"use client";

import { useState, useEffect, useRef } from "react";
import { type AiInsight, type DateRange, fetchAiRecommendations, type AiRecommendationsResponse } from "@/services/analyticsService";

interface AnalyticsAiInsightsProps {
  insights: AiInsight[];
  dateRange?: DateRange;
}

const COOLDOWN_MS = 8000;

export default function AnalyticsAiInsights({ insights, dateRange }: AnalyticsAiInsightsProps) {
  const [aiResponse, setAiResponse] = useState<AiRecommendationsResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [cooldown, setCooldown] = useState(false);
  const cooldownRef = useRef<NodeJS.Timeout | null>(null);
  const activeRequestRef = useRef<AbortController | null>(null);

  useEffect(() => {
    activeRequestRef.current?.abort();
    activeRequestRef.current = null;
    setAiResponse(null);
    setError(null);
    setLoading(false);
  }, [dateRange]);

  useEffect(() => {
    return () => {
      activeRequestRef.current?.abort();
      activeRequestRef.current = null;
      if (cooldownRef.current) clearTimeout(cooldownRef.current);
    };
  }, []);

  const handleAskAi = async (forceRefresh = false) => {
    if (loading || (cooldown && !forceRefresh)) return;
    setLoading(true);
    setError(null);
    setAiResponse(null);
    const requestController = new AbortController();
    activeRequestRef.current = requestController;
    try {
      const response = await fetchAiRecommendations(dateRange, forceRefresh, { signal: requestController.signal });
      if (activeRequestRef.current !== requestController) return;
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
      if (activeRequestRef.current === requestController) {
        setError("Failed to get AI recommendations.");
      }
    } finally {
      if (activeRequestRef.current === requestController) {
        activeRequestRef.current = null;
        setLoading(false);
      }
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
            {showAiResults ? "AI Recommendations" : "AI Performance Summary"}
          </h4>
          <p className="text-body-sm text-outline mt-0.5">
            {showAiResults ? "Real-time analysis from Gemini" : "Click to get AI-powered insights"}
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
        ) : showAiResults && aiResponse?.recommendations ? (
          <div className="space-y-4 animate-fade-up">
            {aiResponse.recommendations.map((rec, index) => {
              const colors = 
                rec.priority === "HIGH" ? "border-red-500/30 bg-red-500/5 text-red-500" :
                rec.priority === "MID" ? "border-yellow-500/30 bg-yellow-500/5 text-yellow-500" :
                "border-green-500/30 bg-green-500/5 text-green-500";
              const icon = 
                rec.priority === "HIGH" ? "priority_high" :
                rec.priority === "MID" ? "warning" : "check_circle";

              return (
                <div key={index} className="group relative" style={{ animationDelay: `${0.1 + index * 0.1}s` }}>
                  <div className={`relative p-5 rounded-xl border ${colors} hover:shadow-lg transition-all duration-300`}>
                    <div className="flex items-start gap-4">
                      <div className="shrink-0 mt-1">
                        <span className="material-symbols-outlined text-[24px]">
                          {icon}
                        </span>
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-2">
                          <span className={`px-2 py-0.5 rounded text-[10px] font-bold tracking-wider bg-current/10 ${colors.split(" ")[2]}`}>
                            {rec.priority} PRIORITY
                          </span>
                        </div>
                        <h4 className="text-on-surface font-bold mb-2 text-lg">{rec.title}</h4>
                        <p className="text-on-surface-variant text-sm mb-4 leading-relaxed bg-surface-container-high/30 p-3 rounded-lg italic border-l-2 border-current">
                          {rec.rationale}
                        </p>
                        
                        <div className="mb-4">
                          <h5 className="text-sm font-semibold text-on-surface mb-2 flex items-center gap-2">
                            <span className="material-symbols-outlined text-[18px]">format_list_bulleted</span>
                            Actionable Steps
                          </h5>
                          <ul className="space-y-2">
                            {rec.actionable_steps.map((step, idx) => (
                              <li key={idx} className="flex items-start gap-2 text-sm text-on-surface-variant">
                                <span className="material-symbols-outlined text-[16px] text-primary shrink-0 mt-0.5">check</span>
                                <span>{step}</span>
                              </li>
                            ))}
                          </ul>
                        </div>

                        {rec.kpi_target && (
                          <div className="flex items-center gap-2 text-sm font-semibold text-primary bg-primary/5 px-3 py-2 rounded-lg border border-primary/10">
                            <span className="material-symbols-outlined text-[18px]">track_changes</span>
                            Target: {rec.kpi_target}
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
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
          onClick={() => handleAskAi(false)}
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
                Ask AI Assistant
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
