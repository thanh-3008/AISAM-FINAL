"use client";

import { useState, useMemo, useRef, useEffect, useCallback } from "react";
import { type ChartDataPoint, type ChartView } from "@/services/analyticsService";
import { formatCurrency } from "./analyticsUtils";

interface AnalyticsChartProps {
  data: ChartDataPoint[];
}

export default function AnalyticsChart({ data }: AnalyticsChartProps) {
  const [view, setView] = useState<ChartView>("daily");
  const [hoveredIndex, setHoveredIndex] = useState<number | null>(null);
  const [isAnimating, setIsAnimating] = useState(true);
  const svgRef = useRef<SVGSVGElement>(null);

  const displayData = useMemo(() => {
    return view === "weekly" ? aggregateWeekly(data) : data;
  }, [data, view]);

  const timerRef = useRef<NodeJS.Timeout | null>(null);
  const prevViewRef = useRef(view);
  const prevDataRef = useRef(data);

  if (prevViewRef.current !== view || prevDataRef.current !== data) {
    prevViewRef.current = view;
    prevDataRef.current = data;
    setIsAnimating(true);
    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => setIsAnimating(false), 1000);
  }

  useEffect(() => {
    return () => { if (timerRef.current) clearTimeout(timerRef.current); };
  }, []);

  const maxSpend = Math.max(...displayData.map((d) => d.spend));
  const maxConversions = Math.max(...displayData.map((d) => d.conversions));

  const width = 800;
  const height = 400;
  const padding = { top: 40, right: 40, bottom: 60, left: 80 };
  const chartWidth = width - padding.left - padding.right;
  const chartHeight = height - padding.top - padding.bottom;

  // Generate smooth bezier curve path
  const generateSmoothPath = useCallback((dataPoints: number[], maxValue: number) => {
    return dataPoints
      .map((point, i) => {
        const x = padding.left + (i / (dataPoints.length - 1)) * chartWidth;
        const y = padding.top + chartHeight - (point / maxValue) * chartHeight;
        
        if (i === 0) return `M ${x} ${y}`;
        
        const prevX = padding.left + ((i - 1) / (dataPoints.length - 1)) * chartWidth;
        const prevY = padding.top + chartHeight - (dataPoints[i - 1] / maxValue) * chartHeight;
        
        const controlX1 = prevX + (x - prevX) / 3;
        const controlX2 = x - (x - prevX) / 3;
        
        return `C ${controlX1} ${prevY} ${controlX2} ${y} ${x} ${y}`;
      })
      .join(" ");
  }, [padding.left, padding.top, chartWidth, chartHeight]);

  const spendPath = useMemo(() => {
    return generateSmoothPath(displayData.map((d) => d.spend), maxSpend);
  }, [displayData, maxSpend, generateSmoothPath]);

  const conversionsPath = useMemo(() => {
    return generateSmoothPath(displayData.map((d) => d.conversions), maxConversions);
  }, [displayData, maxConversions, generateSmoothPath]);

  // Generate area paths
  const spendAreaPath = useMemo(() => {
    const firstX = padding.left;
    const lastX = padding.left + chartWidth;
    const bottomY = padding.top + chartHeight;
    return `${spendPath} L ${lastX} ${bottomY} L ${firstX} ${bottomY} Z`;
  }, [spendPath, chartWidth, chartHeight, padding.left, padding.top]);

  const conversionsAreaPath = useMemo(() => {
    const firstX = padding.left;
    const lastX = padding.left + chartWidth;
    const bottomY = padding.top + chartHeight;
    return `${conversionsPath} L ${lastX} ${bottomY} L ${firstX} ${bottomY} Z`;
  }, [conversionsPath, chartWidth, chartHeight, padding.left, padding.top]);

  const handleMouseMove = (e: React.MouseEvent<SVGSVGElement>) => {
    if (!svgRef.current) return;
    const rect = svgRef.current.getBoundingClientRect();
    const x = ((e.clientX - rect.left) / rect.width) * width;
    const index = Math.round(((x - padding.left) / chartWidth) * (displayData.length - 1));
    if (index >= 0 && index < displayData.length) {
      setHoveredIndex(index);
    }
  };

  return (
    <div className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/50 p-8 shadow-xl animate-fade-up" style={{ animationDelay: "0.3s" }}>
      {/* Header */}
      <div className="flex justify-between items-start mb-8">
        <div>
          <h4 className="text-headline-sm text-on-surface mb-2">
            Spend vs. Conversions
          </h4>
          <p className="text-on-surface-variant text-body-sm">
            {view === "daily" ? "Daily" : "Weekly"} performance metrics with trend analysis
          </p>
        </div>
        <div className="flex items-center gap-2 bg-surface-container-high rounded-xl p-1 shadow-inner">
          <button
            onClick={() => setView("daily")}
            className={`px-5 py-2 rounded-lg font-semibold text-label-sm transition-all duration-300 ${
              view === "daily"
                ? "bg-gradient-to-r from-primary to-primary-container text-on-primary shadow-lg scale-105"
                : "text-outline hover:text-on-surface hover:bg-surface-container-low"
            }`}
          >
            Daily
          </button>
          <button
            onClick={() => setView("weekly")}
            className={`px-5 py-2 rounded-lg font-semibold text-label-sm transition-all duration-300 ${
              view === "weekly"
                ? "bg-gradient-to-r from-primary to-primary-container text-on-primary shadow-lg scale-105"
                : "text-outline hover:text-on-surface hover:bg-surface-container-low"
            }`}
          >
            Weekly
          </button>
        </div>
      </div>

      {/* SVG Chart */}
      <div className="relative w-full overflow-hidden rounded-xl bg-surface-container-lowest/50 p-4">
        <svg
          ref={svgRef}
          viewBox={`0 0 ${width} ${height}`}
          className="w-full h-[400px] cursor-crosshair"
          preserveAspectRatio="none"
          onMouseMove={handleMouseMove}
          onMouseLeave={() => setHoveredIndex(null)}
        >
          <defs>
            {/* Premium gradients */}
            <linearGradient id="spendGradient" x1="0%" y1="0%" x2="0%" y2="100%">
              <stop offset="0%" stopColor="#3b82f6" stopOpacity="0.4" />
              <stop offset="50%" stopColor="#3b82f6" stopOpacity="0.2" />
              <stop offset="100%" stopColor="#3b82f6" stopOpacity="0" />
            </linearGradient>

            <linearGradient id="conversionsGradient" x1="0%" y1="0%" x2="0%" y2="100%">
              <stop offset="0%" stopColor="#8b5cf6" stopOpacity="0.4" />
              <stop offset="50%" stopColor="#8b5cf6" stopOpacity="0.2" />
              <stop offset="100%" stopColor="#8b5cf6" stopOpacity="0" />
            </linearGradient>

            <linearGradient id="lineGradientSpend" x1="0%" y1="0%" x2="100%" y2="0%">
              <stop offset="0%" stopColor="#3b82f6" />
              <stop offset="50%" stopColor="#60a5fa" />
              <stop offset="100%" stopColor="#3b82f6" />
            </linearGradient>

            <linearGradient id="lineGradientConversions" x1="0%" y1="0%" x2="100%" y2="0%">
              <stop offset="0%" stopColor="#8b5cf6" />
              <stop offset="50%" stopColor="#a78bfa" />
              <stop offset="100%" stopColor="#8b5cf6" />
            </linearGradient>

            <filter id="glow">
              <feGaussianBlur stdDeviation="4" result="coloredBlur" />
              <feMerge>
                <feMergeNode in="coloredBlur" />
                <feMergeNode in="SourceGraphic" />
              </feMerge>
            </filter>

            <filter id="shadow">
              <feDropShadow dx="0" dy="4" stdDeviation="8" floodOpacity="0.3" />
            </filter>
          </defs>

          {/* Animated grid lines */}
          {[0, 0.25, 0.5, 0.75, 1].map((ratio, i) => {
            const y = padding.top + chartHeight * (1 - ratio);
            return (
              <g key={i} className="animate-fade-in" style={{ animationDelay: `${i * 0.1}s` }}>
                <line
                  x1={padding.left}
                  y1={y}
                  x2={padding.left + chartWidth}
                  y2={y}
                  stroke="currentColor"
                  strokeOpacity="0.1"
                  strokeDasharray="8 4"
                  className="transition-all duration-500"
                />
                <text
                  x={padding.left - 15}
                  y={y + 5}
                  textAnchor="end"
                  className="fill-outline text-xs font-semibold"
                >
                  {formatCurrency(maxSpend * ratio)}
                </text>
              </g>
            );
          })}

          {/* Area fills with animation */}
          <path
            d={spendAreaPath}
            fill="url(#spendGradient)"
            className={`transition-all duration-1000 ${isAnimating ? "opacity-0 scale-95" : "opacity-100 scale-100"}`}
            style={{ transformOrigin: "center bottom" }}
          />
          <path
            d={conversionsAreaPath}
            fill="url(#conversionsGradient)"
            className={`transition-all duration-1000 delay-200 ${isAnimating ? "opacity-0 scale-95" : "opacity-100 scale-100"}`}
            style={{ transformOrigin: "center bottom" }}
          />

          {/* Lines with glow and draw animation */}
          <path
            d={spendPath}
            fill="none"
            stroke="url(#lineGradientSpend)"
            strokeWidth="3"
            strokeLinecap="round"
            strokeLinejoin="round"
            className={`chart-line ${isAnimating ? "animate-draw" : ""}`}
            filter="url(#glow)"
          />
          <path
            d={conversionsPath}
            fill="none"
            stroke="url(#lineGradientConversions)"
            strokeWidth="3"
            strokeLinecap="round"
            strokeLinejoin="round"
            className={`chart-line ${isAnimating ? "animate-draw-delayed" : ""}`}
            filter="url(#glow)"
          />

          {/* Interactive hover elements */}
          {hoveredIndex !== null && (() => {
            const point = displayData[hoveredIndex];
            const x = padding.left + (hoveredIndex / (displayData.length - 1)) * chartWidth;
            const spendY = padding.top + chartHeight - (point.spend / maxSpend) * chartHeight;
            const conversionY = padding.top + chartHeight - (point.conversions / maxConversions) * chartHeight;

            return (
              <g className="animate-fade-in-fast">
                {/* Vertical crosshair */}
                <line
                  x1={x}
                  y1={padding.top}
                  x2={x}
                  y2={padding.top + chartHeight}
                  stroke="currentColor"
                  strokeOpacity="0.3"
                  strokeWidth="2"
                  strokeDasharray="4 4"
                />

                {/* Spend point with pulse */}
                <circle cx={x} cy={spendY} r="12" fill="#3b82f6" opacity="0.2" className="animate-pulse-ring" />
                <circle cx={x} cy={spendY} r="8" fill="#3b82f6" filter="url(#shadow)" />
                <circle cx={x} cy={spendY} r="4" fill="white" />

                {/* Conversions point with pulse */}
                <circle cx={x} cy={conversionY} r="12" fill="#8b5cf6" opacity="0.2" className="animate-pulse-ring" />
                <circle cx={x} cy={conversionY} r="8" fill="#8b5cf6" filter="url(#shadow)" />
                <circle cx={x} cy={conversionY} r="4" fill="white" />

                {/* Premium tooltip */}
                <g transform={`translate(${Math.min(x + 20, width - 200)}, ${Math.max(spendY - 100, padding.top)})`}>
                  <rect width="180" height="110" rx="12" fill="rgba(30, 30, 40, 0.95)" filter="url(#shadow)" />
                  <rect width="180" height="110" rx="12" fill="none" stroke="rgba(255,255,255,0.1)" strokeWidth="1" />
                  
                  <text x="15" y="25" fill="white" fontSize="12" fontWeight="bold">
                    {point.date}
                  </text>
                  
                  <circle cx="15" cy="45" r="4" fill="#3b82f6" />
                  <text x="25" y="50" fill="rgba(255,255,255,0.7)" fontSize="11">Spend</text>
                  <text x="165" y="50" fill="white" fontSize="12" fontWeight="bold" textAnchor="end">
                    {formatCurrency(point.spend)}
                  </text>
                  
                  <circle cx="15" cy="70" r="4" fill="#8b5cf6" />
                  <text x="25" y="75" fill="rgba(255,255,255,0.7)" fontSize="11">Conversions</text>
                  <text x="165" y="75" fill="white" fontSize="12" fontWeight="bold" textAnchor="end">
                    {point.conversions}
                  </text>
                  
                  <line x1="15" y1="85" x2="165" y2="85" stroke="rgba(255,255,255,0.1)" />
                  <text x="15" y="100" fill="rgba(255,255,255,0.7)" fontSize="11">CPC</text>
                  <text x="165" y="100" fill="white" fontSize="12" fontWeight="bold" textAnchor="end">
                    ${point.cpc.toFixed(2)}
                  </text>
                </g>
              </g>
            );
          })()}
        </svg>
      </div>

      {/* Legend */}
      <div className="flex justify-center gap-12 mt-8 pt-6 border-t border-outline-variant/30">
        <div className="flex items-center gap-3 group cursor-pointer">
          <div className="relative">
            <span className="w-4 h-4 rounded-full bg-gradient-to-r from-blue-500 to-blue-600 block shadow-lg" />
            <span className="absolute inset-0 w-4 h-4 rounded-full bg-blue-500 animate-ping opacity-20" />
          </div>
          <span className="font-semibold text-label-sm text-on-surface group-hover:text-primary transition-colors">
            Total Spend ($)
          </span>
        </div>
        <div className="flex items-center gap-3 group cursor-pointer">
          <div className="relative">
            <span className="w-4 h-4 rounded-full bg-gradient-to-r from-purple-500 to-purple-600 block shadow-lg" />
            <span className="absolute inset-0 w-4 h-4 rounded-full bg-purple-500 animate-ping opacity-20" style={{ animationDelay: "0.5s" }} />
          </div>
          <span className="font-semibold text-label-sm text-on-surface group-hover:text-secondary transition-colors">
            Conversions
          </span>
        </div>
      </div>

      <style>{`
        @keyframes fade-in {
          from { opacity: 0; transform: translateY(10px); }
          to { opacity: 1; transform: translateY(0); }
        }
        @keyframes fade-in-fast {
          from { opacity: 0; }
          to { opacity: 1; }
        }
        @keyframes draw {
          from { stroke-dashoffset: 2000; }
          to { stroke-dashoffset: 0; }
        }
        @keyframes pulse-ring {
          0% { transform: scale(1); opacity: 0.3; }
          100% { transform: scale(1.5); opacity: 0; }
        }
        .animate-fade-in {
          animation: fade-in 0.6s ease-out forwards;
          opacity: 0;
        }
        .animate-fade-in-fast {
          animation: fade-in-fast 0.15s ease-out forwards;
        }
        .animate-draw {
          stroke-dasharray: 2000;
          animation: draw 1.5s cubic-bezier(0.4, 0, 0.2, 1) forwards;
        }
        .animate-draw-delayed {
          stroke-dasharray: 2000;
          animation: draw 1.5s cubic-bezier(0.4, 0, 0.2, 1) 0.3s forwards;
        }
        .animate-pulse-ring {
          animation: pulse-ring 1.5s ease-out infinite;
        }
        .chart-line {
          transition: d 0.8s cubic-bezier(0.4, 0, 0.2, 1);
        }
      `}</style>
    </div>
  );
}

function aggregateWeekly(data: ChartDataPoint[]): ChartDataPoint[] {
  const weeks: ChartDataPoint[] = [];
  for (let i = 0; i < data.length; i += 7) {
    const weekData = data.slice(i, i + 7);
    if (weekData.length === 0) continue;

    const totalSpend = weekData.reduce((sum, d) => sum + d.spend, 0);
    const totalConversions = weekData.reduce((sum, d) => sum + d.conversions, 0);
    const avgCpc = weekData.reduce((sum, d) => sum + d.cpc, 0) / weekData.length;

    weeks.push({
      date: `Week ${Math.floor(i / 7) + 1}`,
      spend: totalSpend,
      conversions: totalConversions,
      cpc: avgCpc,
    });
  }
  return weeks;
}
