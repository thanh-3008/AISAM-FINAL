"use client";

import { useMemo, useState } from "react";
import { type ChartView, type ScheduledPublishingPoint } from "@/services/analyticsService";

interface AnalyticsChartProps { data: ScheduledPublishingPoint[]; }

const WIDTH = 800;
const HEIGHT = 400;
const PADDING = { top: 32, right: 28, bottom: 62, left: 58 };

export default function AnalyticsChart({ data }: AnalyticsChartProps) {
  const [view, setView] = useState<ChartView>("daily");
  const [hoveredIndex, setHoveredIndex] = useState<number | null>(null);
  const displayData = useMemo(() => view === "weekly" ? aggregateWeekly(data) : data, [data, view]);
  const chartWidth = WIDTH - PADDING.left - PADDING.right;
  const chartHeight = HEIGHT - PADDING.top - PADDING.bottom;
  const maxValue = Math.max(...displayData.flatMap((p) => [p.completed, p.failed, p.pending]), 1);
  const maxPosts = Math.max(4, Math.ceil(maxValue / 4) * 4);
  const slotWidth = displayData.length ? chartWidth / displayData.length : chartWidth;
  const groupWidth = Math.min(54, slotWidth * 0.78);
  const barGap = Math.min(3, groupWidth * 0.06);
  const barWidth = (groupWidth - barGap * 2) / 3;
  const labelStep = Math.max(1, Math.ceil(displayData.length / 10));
  const totals = useMemo(() => {
    const completed = displayData.reduce((sum, p) => sum + p.completed, 0);
    const failed = displayData.reduce((sum, p) => sum + p.failed, 0);
    const pending = displayData.reduce((sum, p) => sum + p.pending, 0);
    const finished = completed + failed;
    return { completed, failed, pending, successRate: finished ? completed / finished * 100 : 0 };
  }, [displayData]);

  return (
    <section className="bg-linear-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/50 p-8 shadow-xl animate-fade-up">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between mb-7">
        <div className="flex items-start gap-3">
          <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600 ring-1 ring-indigo-100">
            <span className="material-symbols-outlined">monitoring</span>
          </span>
          <div>
            <div className="flex flex-wrap items-center gap-2">
              <h4 className="text-headline-sm text-on-surface">Publishing Performance</h4>
              <span className="rounded-full bg-emerald-50 px-2.5 py-1 text-[10px] font-bold uppercase tracking-wider text-emerald-700 ring-1 ring-emerald-100">Live</span>
            </div>
            <p className="mt-1 text-on-surface-variant text-body-sm">Reliability of scheduled content delivery</p>
          </div>
        </div>
        <div className="flex items-center self-start bg-surface-container-high rounded-xl p-1 shadow-inner">
          {(["daily", "weekly"] as ChartView[]).map((option) => (
            <button key={option} type="button" onClick={() => { setView(option); setHoveredIndex(null); }}
              className={`px-5 py-2 rounded-lg font-semibold text-label-sm capitalize transition-all duration-300 ${view === option ? "bg-linear-to-r from-primary to-primary-container text-on-primary shadow-lg" : "text-outline hover:text-on-surface"}`}>
              {option}
            </button>
          ))}
        </div>
      </div>

      {displayData.length > 0 && <div className="mb-6 overflow-hidden rounded-2xl border border-indigo-100/80 bg-linear-to-r from-indigo-50/80 via-white to-violet-50/70 p-4 shadow-sm">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-center lg:justify-between">
          <div className="grid flex-1 grid-cols-3 divide-x divide-outline-variant/40">
            <Summary label="Published" value={totals.completed} icon="check_circle" tone="green" />
            <Summary label="Failed" value={totals.failed} icon="error" tone="red" />
            <Summary label="Pending" value={totals.pending} icon="schedule" tone="indigo" />
          </div>
          <div className="flex items-center gap-3 border-t border-indigo-100 pt-4 lg:border-l lg:border-t-0 lg:pl-5 lg:pt-0">
            <div className="relative grid h-16 w-16 shrink-0 place-items-center rounded-full" style={{ background: `conic-gradient(#4f46e5 ${totals.successRate * 3.6}deg, #e0e7ff 0deg)` }}>
              <div className="grid h-12 w-12 place-items-center rounded-full bg-white shadow-inner"><span className="material-symbols-outlined text-xl text-indigo-600">verified</span></div>
            </div>
            <div><p className="text-[10px] font-bold uppercase tracking-[0.16em] text-indigo-500">Success rate</p><p className="text-2xl font-extrabold text-indigo-700">{totals.successRate.toFixed(1)}%</p></div>
          </div>
        </div>
      </div>}

      {displayData.length === 0 ? (
        <div className="h-[400px] rounded-xl bg-surface-container-lowest/50 flex flex-col items-center justify-center text-center px-6">
          <span className="material-symbols-outlined text-5xl text-outline/50 mb-3">event_busy</span>
          <p className="font-semibold text-on-surface">No scheduled posts in this period</p>
          <p className="text-body-sm text-on-surface-variant mt-1">Schedule content to start tracking publishing reliability.</p>
        </div>
      ) : (
        <div className="relative w-full overflow-hidden rounded-2xl border border-outline-variant/30 bg-white p-3 shadow-inner">
          <svg viewBox={`0 0 ${WIDTH} ${HEIGHT}`} className="w-full h-[400px]" preserveAspectRatio="none">
            <defs>
              <linearGradient id="publishedBar" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#22c55e" /><stop offset="1" stopColor="#16a34a" /></linearGradient>
              <linearGradient id="failedBar" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#fb7185" /><stop offset="1" stopColor="#e11d48" /></linearGradient>
              <linearGradient id="pendingBar" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#a5b4fc" /><stop offset="1" stopColor="#6366f1" /></linearGradient>
              <linearGradient id="plotBackground" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stopColor="#f8fafc" stopOpacity="0.85" /><stop offset="1" stopColor="#ffffff" stopOpacity="0.2" /></linearGradient>
              <filter id="barShadow" x="-30%" y="-20%" width="160%" height="150%"><feDropShadow dx="0" dy="3" stdDeviation="3" floodColor="#0f172a" floodOpacity="0.12" /></filter>
            </defs>
            <rect x={PADDING.left} y={PADDING.top} width={chartWidth} height={chartHeight} rx="14" fill="url(#plotBackground)" />
            {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
              const y = PADDING.top + chartHeight * (1 - ratio);
              return <g key={ratio}>
                <line x1={PADDING.left} y1={y} x2={PADDING.left + chartWidth} y2={y} stroke="currentColor" strokeOpacity={ratio === 0 ? "0.2" : "0.08"} strokeDasharray={ratio === 0 ? undefined : "7 5"} />
                <text x={PADDING.left - 12} y={y + 4} textAnchor="end" className="fill-outline text-xs">{Math.round(maxPosts * ratio)}</text>
              </g>;
            })}

            {displayData.map((p, i) => {
              const x = PADDING.left + slotWidth * i + (slotWidth - groupWidth) / 2;
              const pendingHeight = (p.pending / maxPosts) * chartHeight;
              const failedHeight = (p.failed / maxPosts) * chartHeight;
              const completedHeight = (p.completed / maxPosts) * chartHeight;
              const bottom = PADDING.top + chartHeight;
              const hovered = hoveredIndex === i;
              return <g key={`${p.date}-${i}`} onMouseEnter={() => setHoveredIndex(i)} onMouseLeave={() => setHoveredIndex(null)} className="cursor-pointer">
                <rect x={x - slotWidth * 0.1} y={PADDING.top} width={groupWidth + slotWidth * 0.2} height={chartHeight} rx="8" fill={hovered ? "currentColor" : "transparent"} opacity="0.035" />
                <rect x={x} y={bottom - completedHeight} width={barWidth} height={completedHeight} rx="5" fill="url(#publishedBar)" opacity={hovered ? 1 : 0.92} filter={hovered ? "url(#barShadow)" : undefined} />
                <rect x={x + barWidth + barGap} y={bottom - failedHeight} width={barWidth} height={failedHeight} rx="5" fill="url(#failedBar)" opacity={hovered ? 1 : 0.92} filter={hovered ? "url(#barShadow)" : undefined} />
                <rect x={x + (barWidth + barGap) * 2} y={bottom - pendingHeight} width={barWidth} height={pendingHeight} rx="5" fill="url(#pendingBar)" opacity={hovered ? 1 : 0.92} filter={hovered ? "url(#barShadow)" : undefined} />
                {hovered && <>
                  {p.completed > 0 && <text x={x + barWidth / 2} y={bottom - completedHeight - 7} textAnchor="middle" className="fill-emerald-700 text-[10px] font-bold">{p.completed}</text>}
                  {p.failed > 0 && <text x={x + barWidth + barGap + barWidth / 2} y={bottom - failedHeight - 7} textAnchor="middle" className="fill-rose-700 text-[10px] font-bold">{p.failed}</text>}
                  {p.pending > 0 && <text x={x + (barWidth + barGap) * 2 + barWidth / 2} y={bottom - pendingHeight - 7} textAnchor="middle" className="fill-indigo-700 text-[10px] font-bold">{p.pending}</text>}
                </>}
                {(i % labelStep === 0 || i === displayData.length - 1) &&
                  <text x={x + groupWidth / 2} y={HEIGHT - PADDING.bottom + 23} textAnchor="middle" className="fill-outline text-[11px]">{formatLabel(p.date, view)}</text>}
              </g>;
            })}

          </svg>

          {hoveredIndex !== null && (() => {
            const point = displayData[hoveredIndex];
            const left = ((hoveredIndex + 0.5) / displayData.length) * 100;
            return <div className="absolute top-5 z-10 min-w-48 -translate-x-1/2 rounded-xl bg-gray-950/95 p-4 text-xs text-white shadow-2xl pointer-events-none" style={{ left: `${Math.min(86, Math.max(14, left))}%` }}>
              <p className="font-bold mb-3">{formatTooltipDate(point.date, view)}</p>
              <Metric color="bg-green-500" label="Published" value={point.completed} />
              <Metric color="bg-red-500" label="Failed" value={point.failed} />
              <Metric color="bg-slate-400" label="Pending" value={point.pending} />
              <Metric color="bg-amber-400" label="Retry attempts" value={point.retryAttempts} />
              <div className="border-t border-white/15 mt-2 pt-2 flex justify-between gap-5"><span className="text-white/70">Success rate</span><strong>{point.successRate.toFixed(1)}%</strong></div>
            </div>;
          })()}
        </div>
      )}

      <div className="flex flex-wrap justify-center gap-x-8 gap-y-3 mt-6 text-label-sm font-semibold text-on-surface">
        <Legend color="bg-green-500" label="Published" /><Legend color="bg-red-500" label="Failed" />
        <Legend color="bg-indigo-400" label="Pending" />
      </div>
    </section>
  );
}

function Metric({ color, label, value }: { color: string; label: string; value: number }) {
  return <div className="flex items-center justify-between gap-5 mb-2"><span className="flex items-center gap-2 text-white/70"><i className={`w-2 h-2 rounded-full ${color}`} />{label}</span><strong>{value}</strong></div>;
}

function Legend({ color, label }: { color: string; label: string }) {
  return <span className="flex items-center gap-2"><i className={`h-3 w-3 rounded-sm ${color}`} />{label}</span>;
}

function Summary({ label, value, icon, tone }: { label: string; value: number | string; icon: string; tone: "green" | "red" | "indigo" }) {
  const styles = {
    green: "bg-emerald-100 text-emerald-700",
    red: "bg-rose-100 text-rose-700",
    indigo: "bg-indigo-100 text-indigo-700",
  }[tone];
  return <div className="flex items-center justify-center gap-3 px-3 py-2 sm:px-5">
    <span className={`grid h-9 w-9 shrink-0 place-items-center rounded-xl ${styles}`}><span className="material-symbols-outlined text-lg">{icon}</span></span>
    <div><p className="text-[10px] font-bold uppercase tracking-wider text-outline">{label}</p><p className="text-xl font-extrabold leading-tight text-on-surface">{value}</p></div>
  </div>;
}

function aggregateWeekly(data: ScheduledPublishingPoint[]): ScheduledPublishingPoint[] {
  const groups = new Map<string, ScheduledPublishingPoint[]>();
  data.forEach((point) => {
    const date = new Date(`${point.date}T00:00:00`);
    const day = date.getDay();
    date.setDate(date.getDate() - (day === 0 ? 6 : day - 1));
    const weekStart = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
    groups.set(weekStart, [...(groups.get(weekStart) || []), point]);
  });

  return [...groups.entries()].sort(([a], [b]) => a.localeCompare(b)).map(([date, group]) => {
    const completed = group.reduce((sum, p) => sum + p.completed, 0);
    const failed = group.reduce((sum, p) => sum + p.failed, 0);
    const finished = completed + failed;
    return { date, completed, failed,
      pending: group.reduce((sum, p) => sum + p.pending, 0),
      retryAttempts: group.reduce((sum, p) => sum + p.retryAttempts, 0),
      successRate: finished === 0 ? 0 : Math.round((completed / finished) * 1000) / 10 };
  });
}

function formatLabel(date: string, view: ChartView): string {
  const value = new Date(`${date}T00:00:00`);
  return view === "weekly" ? value.toLocaleDateString("en-US", { month: "short", day: "numeric" }) : value.toLocaleDateString("en-US", { day: "2-digit", month: "short" });
}

function formatTooltipDate(date: string, view: ChartView): string {
  const formatted = new Date(`${date}T00:00:00`).toLocaleDateString("en-US", { year: "numeric", month: "short", day: "numeric" });
  return view === "weekly" ? `Week of ${formatted}` : formatted;
}
