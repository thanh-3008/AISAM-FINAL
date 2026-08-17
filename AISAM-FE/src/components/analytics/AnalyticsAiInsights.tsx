"use client";

import { useState, useEffect, useRef } from "react";
import { type AiInsight, type DateRange, fetchAiRecommendations } from "@/services/analyticsService";

interface AnalyticsAiInsightsProps {
  insights: AiInsight[];
  dateRange?: DateRange;
}

function parseAiResponse(text: string): { icon: string; message: string }[] {
  const lines = text.split("\n").filter(l => l.trim());
  const items: { icon: string; message: string }[] = [];
  const emojiIconMap: Record<string, string> = {
    "🔥": "local_fire_department",
    "📈": "trending_up",
    "⚡": "bolt",
    "📝": "edit_note",
    "🎯": "my_location",
    "💰": "payments",
    "🚀": "rocket_launch",
    "⚠️": "warning",
  };

  let current: { icon: string; message: string } | null = null;

  for (const line of lines) {
    let trimmed = line
      .replace(/\*\*(.*?)\*\*/g, "$1")
      .replace(/__(.*?)__/g, "$1")
      .replace(/^\d+\.\s*/, "")
      .replace(/^[-•]\s*/, "")
      .trim();

    if (!trimmed) continue;

    // Check if this line starts with a known emoji (possibly with variation selector)
    let matchedIcon: string | null = null;
    for (const [emoji, icon] of Object.entries(emojiIconMap)) {
      if (trimmed.startsWith(emoji) || trimmed.replace(/^\p{Emoji}+/u, "").length < trimmed.length) {
        if (trimmed.startsWith(emoji) || trimmed.match(new RegExp(`^${emoji}`))) {
          matchedIcon = icon;
          trimmed = trimmed.slice(emoji.length).trim();
          if (trimmed.startsWith(":")) trimmed = trimmed.slice(1).trim();
          break;
        }
      }
    }

    if (matchedIcon) {
      if (current && current.message) {
        items.push(current);
      }
      current = { icon: matchedIcon, message: trimmed };
    } else if (current) {
      // Continuation line for current item
      if (trimmed.length >= 3) {
        current.message += " " + trimmed;
      }
    }
  }

  if (current && current.message) {
    items.push(current);
  }

  // Fallback: if no items parsed, treat each line as a separate tip
  if (items.length === 0) {
    for (const line of lines) {
      let trimmed = line
        .replace(/\*\*(.*?)\*\*/g, "$1")
        .replace(/__(.*?)__/g, "$1")
        .replace(/^\d+\.\s*/, "")
        .replace(/^[-•]\s*/, "")
        .trim();
      if (!trimmed || trimmed.length < 10) continue;

      let icon = "lightbulb";
      let message = trimmed;
      for (const [emoji, icn] of Object.entries(emojiIconMap)) {
        if (trimmed.includes(emoji)) {
          icon = icn;
          message = trimmed.replace(emoji, "").trim();
          break;
        }
      }
      items.push({ icon, message });
    }
  }

  return items;
}

const COOLDOWN_MS = 8000;

export default function AnalyticsAiInsights({ insights, dateRange }: AnalyticsAiInsightsProps) {
  const [aiResponse, setAiResponse] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [cooldown, setCooldown] = useState(false);
  const cooldownRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    setAiResponse(null);
    setError(null);
  }, [dateRange]);

  useEffect(() => {
    return () => {
      if (cooldownRef.current) clearTimeout(cooldownRef.current);
    };
  }, []);

  const handleAskAi = async () => {
    if (loading || cooldown) return;
    setLoading(true);
    setError(null);
    setAiResponse(null);
    try {
      const response = await fetchAiRecommendations(dateRange);
      if (response) {
        setAiResponse(response);
        setCooldown(true);
        cooldownRef.current = setTimeout(() => setCooldown(false), COOLDOWN_MS);
      } else {
        setError("AI is not available at this time.");
      }
    } catch {
      setError("Failed to get AI recommendations.");
    } finally {
      setLoading(false);
    }
  };

  const aiItems = aiResponse ? parseAiResponse(aiResponse) : [];
  const showAiResults = aiItems.length > 0;

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
        ) : showAiResults ? (
          aiItems.map((item, index) => (
            <div
              key={index}
              className="group relative animate-fade-up"
              style={{ animationDelay: `${0.6 + index * 0.1}s` }}
            >
              <div className="relative bg-surface-container-low/80 backdrop-blur-sm p-4 rounded-xl border border-outline-variant/30 hover:border-primary/30 transition-all duration-500 hover:shadow-xl hover:-translate-y-1">
                <div className="flex items-start gap-3">
                  <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-primary to-secondary flex items-center justify-center shadow-md group-hover:scale-110 transition-transform duration-500">
                    <span className="material-symbols-outlined text-white text-label-md">
                      {item.icon}
                    </span>
                  </div>
                  <p className="text-body-sm leading-relaxed text-on-surface-variant flex-1 pt-1">
                    {item.message}
                  </p>
                </div>
              </div>
            </div>
          ))
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

      {/* CTA Button */}
      <button
        onClick={handleAskAi}
        disabled={loading || cooldown}
        className="group relative w-full mt-6 py-3 rounded-xl bg-gradient-to-r from-primary to-secondary text-on-primary text-label-sm font-semibold shadow-lg overflow-hidden hover:shadow-xl transition-all duration-300 hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
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
