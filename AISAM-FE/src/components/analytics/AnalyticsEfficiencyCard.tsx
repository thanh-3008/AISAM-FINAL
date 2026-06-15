"use client";

import { type EfficiencyMetric } from "@/services/analyticsService";
import { useEffect, useState } from "react";

interface AnalyticsEfficiencyCardProps {
  metrics: EfficiencyMetric[];
}

function CircularProgress({ value, color, size = 120 }: { value: number; color: string; size?: number }) {
  const [animatedValue, setAnimatedValue] = useState(0);
  const strokeWidth = 8;
  const radius = (size - strokeWidth) / 2;
  const circumference = radius * 2 * Math.PI;
  const strokeDashoffset = circumference - (animatedValue / 100) * circumference;

  useEffect(() => {
    const timer = setTimeout(() => setAnimatedValue(value), 100);
    return () => clearTimeout(timer);
  }, [value]);

  const getColorClasses = () => {
    switch (color) {
      case "bg-primary":
        return { stroke: "#3b82f6", bg: "#3b82f620" };
      case "bg-secondary":
        return { stroke: "#8b5cf6", bg: "#8b5cf620" };
      default:
        return { stroke: "#3b82f6", bg: "#3b82f620" };
    }
  };

  const colors = getColorClasses();

  return (
    <div className="relative inline-flex items-center justify-center">
      <svg width={size} height={size} className="-rotate-90">
        {/* Background circle */}
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          stroke={colors.bg}
          strokeWidth={strokeWidth}
          fill="none"
        />
        {/* Progress circle */}
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          stroke={colors.stroke}
          strokeWidth={strokeWidth}
          fill="none"
          strokeDasharray={circumference}
          strokeDashoffset={strokeDashoffset}
          strokeLinecap="round"
          className="transition-all duration-1000 ease-out"
          style={{ filter: "drop-shadow(0 0 8px " + colors.stroke + "40)" }}
        />
      </svg>
      <div className="absolute inset-0 flex items-center justify-center">
        <div className="text-center">
          <span className="text-headline-sm bg-gradient-to-r from-on-surface to-on-surface-variant bg-clip-text text-transparent">
            {animatedValue}
          </span>
          <span className="text-label-xs font-bold text-outline">%</span>
        </div>
      </div>
    </div>
  );
}

export default function AnalyticsEfficiencyCard({ metrics }: AnalyticsEfficiencyCardProps) {
  return (
    <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/50 p-6 shadow-xl animate-fade-up" style={{ animationDelay: "0.6s" }}>
      <div className="mb-6">
        <h4 className="text-headline-sm text-on-surface">
          Efficiency Index
        </h4>
        <p className="text-body-sm text-outline mt-1">Performance optimization metrics</p>
      </div>

      <div className="grid grid-cols-2 gap-6">
        {metrics.map((metric, index) => (
          <div
            key={metric.label}
            className="flex flex-col items-center group animate-fade-up"
            style={{ animationDelay: `${0.7 + index * 0.1}s` }}
          >
            <div className="relative mb-4">
              <CircularProgress value={metric.value} color={metric.color} />
              {/* Glow effect on hover */}
              <div className={`absolute inset-0 rounded-full blur-xl opacity-0 group-hover:opacity-30 transition-opacity duration-500 ${
                metric.color === "bg-primary" ? "bg-primary" : "bg-secondary"
              }`} />
            </div>
            <div className="text-center">
              <p className="text-label-sm font-semibold text-on-surface group-hover:text-primary transition-colors duration-300">
                {metric.label}
              </p>
              <p className="text-label-sm text-outline mt-1">
                {metric.value >= 80 ? "Excellent" : metric.value >= 60 ? "Good" : "Needs Work"}
              </p>
            </div>
          </div>
        ))}
      </div>

      {/* Summary bar */}
      <div className="mt-6 pt-6 border-t border-outline-variant/30">
        <div className="flex items-center justify-between mb-2">
          <span className="text-label-sm font-semibold text-outline uppercase tracking-wider">Overall Score</span>
          <span className="text-label-md font-bold bg-gradient-to-r from-primary to-secondary bg-clip-text text-transparent">
            {Math.round(metrics.reduce((sum, m) => sum + m.value, 0) / metrics.length)}%
          </span>
        </div>
        <div className="w-full h-2 bg-surface-container-high rounded-full overflow-hidden">
          <div
            className="h-full bg-gradient-to-r from-primary via-secondary to-primary rounded-full transition-all duration-1000 ease-out relative overflow-hidden"
            style={{ width: `${metrics.reduce((sum, m) => sum + m.value, 0) / metrics.length}%` }}
          >
            <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/30 to-transparent animate-shimmer" />
          </div>
        </div>
      </div>

      <style>{`
        @keyframes fade-up {
          from { opacity: 0; transform: translateY(10px); }
          to { opacity: 1; transform: translateY(0); }
        }
        @keyframes shimmer {
          0% { transform: translateX(-100%); }
          100% { transform: translateX(100%); }
        }
        .animate-fade-up {
          animation: fade-up 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards;
          opacity: 0;
        }
        .animate-shimmer {
          animation: shimmer 2s ease-in-out infinite;
        }
      `}</style>
    </div>
  );
}
