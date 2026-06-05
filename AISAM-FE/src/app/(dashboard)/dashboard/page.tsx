"use client";

import { useEffect, useRef, useState } from "react";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { useProfiles } from "@/hooks/useProfiles";

function CountUp({ value, suffix = "", duration = 1500 }: { value: string; suffix?: string; duration?: number }) {
  const num = parseFloat(value.replace(/[^0-9.]/g, ""));
  const isDecimal = value.includes(".");
  const [count, setCount] = useState(0);
  const ref = useRef<HTMLSpanElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const observer = new IntersectionObserver(([entry]) => {
      if (entry.isIntersecting) {
        const start = performance.now();
        const step = (now: number) => {
          const pct = Math.min((now - start) / duration, 1);
          const eased = 1 - Math.pow(1 - pct, 3);
          setCount(eased * num);
          if (pct < 1) requestAnimationFrame(step);
        };
        requestAnimationFrame(step);
        observer.disconnect();
      }
    }, { threshold: 0.3 });
    observer.observe(el);
    return () => observer.disconnect();
  }, [num, duration]);

  return <span ref={ref}>{isDecimal ? count.toFixed(1) : Math.round(count)}{suffix}</span>;
}

function AnimatedBar({ value, color, delay = 600 }: { value: number; color: string; delay?: number }) {
  const ref = useRef<HTMLDivElement>(null);
  const delayRef = useRef(delay);
  delayRef.current = delay;
  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const timer = setTimeout(() => { el.style.width = `${value}%`; }, delayRef.current);
    return () => clearTimeout(timer);
  }, [value]);
  return (
    <div className="w-full h-2 bg-surface-container/60 rounded-full overflow-hidden">
      <div ref={ref} className={`h-full ${color} rounded-full transition-all duration-1000 ease-out`} style={{ width: "0%" }} />
    </div>
  );
}

function DrawSVG({ children }: { children: React.ReactNode }) {
  return <g className="draw-svg">{children}</g>;
}

function parseCurrency(str: string) {
  return parseFloat(str.replace(/[^0-9.]/g, ""));
}

const kpiData = [
  { icon: "insights", iconBg: "from-blue-500/20 to-blue-600/10", iconColor: "text-blue-500", label: "Total Reach", value: "1.2M", delta: "+12%", deltaUp: true, gradient: "from-blue-500/5 to-transparent", accent: "#3b82f6" },
  { icon: "bolt", iconBg: "from-purple-500/20 to-purple-600/10", iconColor: "text-purple-500", label: "Engagement Rate", value: "4.8%", delta: "+0.5%", deltaUp: true, gradient: "from-purple-500/5 to-transparent", accent: "#a855f7" },
  { icon: "layers", iconBg: "from-amber-500/20 to-amber-600/10", iconColor: "text-amber-500", label: "Active Campaigns", value: "12", delta: "Steady", deltaUp: null, gradient: "from-amber-500/5 to-transparent", accent: "#f59e0b" },
  { icon: "token", iconBg: "from-emerald-500/20 to-emerald-600/10", iconColor: "text-emerald-500", label: "AI Credits", value: "850", max: "1000", pct: 85, gradient: "from-emerald-500/5 to-transparent", accent: "#10b981" },
];

const scheduleData = [
  { title: "Winter Collection 2024", platform: "FB", time: "Today, 2:30 PM", platformColor: "text-blue-600", platformBg: "bg-blue-100" },
  { title: "Coffee Morning Blast", platform: "IG", time: "Tomorrow, 9:00 AM", platformColor: "text-pink-600", platformBg: "bg-pink-100" },
  { title: "Enterprise Data Launch", platform: "LI", time: "Nov 3, 11:15 AM", platformColor: "text-blue-700", platformBg: "bg-blue-100" },
  { title: "Spring Launch Teaser", platform: "IG", time: "Nov 5, 10:00 AM", platformColor: "text-pink-600", platformBg: "bg-pink-100" },
  { title: "Customer Success Story", platform: "LI", time: "Nov 7, 2:00 PM", platformColor: "text-blue-700", platformBg: "bg-blue-100" },
  { title: "Weekend Flash Sale", platform: "FB", time: "Nov 9, 9:00 AM", platformColor: "text-blue-600", platformBg: "bg-blue-100" },
];

const campaignsData = [
  { name: "Winter Collection 2024", platform: "FACEBOOK", color: "text-blue-600", bg: "bg-blue-50", budget: "$5,000", spent: "$3,240", status: "Active" },
  { name: "Coffee Morning Blast", platform: "INSTAGRAM", color: "text-pink-600", bg: "bg-pink-50", budget: "$2,500", spent: "$1,120", status: "Active" },
  { name: "Enterprise Data Launch", platform: "LINKEDIN", color: "text-blue-700", bg: "bg-blue-50", budget: "$12,000", spent: "$8,450", status: "Active" },
  { name: "Flash Sale Q4", platform: "FACEBOOK", color: "text-blue-600", bg: "bg-blue-50", budget: "$1,500", spent: "$1,500", status: "Completed" },
  { name: "Brand Awareness 2024", platform: "INSTAGRAM", color: "text-pink-600", bg: "bg-pink-50", budget: "$8,000", spent: "$2,100", status: "Active" },
];

const aiSuggestions = [
  { icon: "trending_up", bg: "from-blue-500/10 to-blue-600/5", color: "text-blue-500", title: "Eco-Friendly Packaging", desc: "Trending in your niche. 85% predicted engagement for video content." },
  { icon: "schedule", bg: "from-purple-500/10 to-purple-600/5", color: "text-purple-500", title: "Morning Routine Series", desc: "High conversion potential for Instagram Stories between 7-9 AM." },
  { icon: "psychology", bg: "from-orange-500/10 to-orange-600/5", color: "text-orange-500", title: "Behind the Scenes", desc: "Builds brand trust. Recommended for LinkedIn carousel posts." },
  { icon: "celebration", bg: "from-emerald-500/10 to-emerald-600/5", color: "text-emerald-500", title: "Holiday Gift Guide", desc: "Early interest detected. Start teaser campaigns this week." },
];

export default function DashboardPage() {
  const { activeProfile } = useProfiles();
  const profileName = activeProfile?.name || "User";
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => setVisible(true), 100);
    return () => clearTimeout(timer);
  }, []);

  return (
    <>
      <style>{`
        @keyframes fade-up {
          from { opacity: 0; transform: translateY(24px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        @keyframes fade-right {
          from { opacity: 0; transform: translateX(-16px); }
          to   { opacity: 1; transform: translateX(0); }
        }
        @keyframes scale-in {
          from { opacity: 0; transform: scale(0.92); }
          to   { opacity: 1; transform: scale(1); }
        }
        @keyframes draw-line {
          from { stroke-dashoffset: 2000; }
          to   { stroke-dashoffset: 0; }
        }
        @keyframes float-y {
          0%, 100% { transform: translateY(0); }
          50%      { transform: translateY(-6px); }
        }
        @keyframes glow-pulse {
          0%, 100% { box-shadow: 0 0 8px rgba(15, 98, 254, 0.15); }
          50%      { box-shadow: 0 0 20px rgba(15, 98, 254, 0.3); }
        }
        @keyframes slide-up-row {
          from { opacity: 0; transform: translateY(12px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        @keyframes bar-grow {
          from { height: 0; }
        }
        @keyframes shimmer {
          0% { background-position: -200% 0; }
          100% { background-position: 200% 0; }
        }
        @keyframes border-dance {
          0%, 100% { border-color: rgba(15, 98, 254, 0.1); }
          50% { border-color: rgba(15, 98, 254, 0.3); }
        }
        .animate-fade-up { animation: fade-up 0.6s ease-out forwards; opacity: 0; }
        .animate-fade-right { animation: fade-right 0.5s ease-out forwards; opacity: 0; }
        .animate-scale-in { animation: scale-in 0.5s ease-out forwards; opacity: 0; }
        .animate-float { animation: float-y 3s ease-in-out infinite; }
        .animate-glow-pulse { animation: glow-pulse 2s ease-in-out infinite; }
        .chart-line { stroke-dasharray: 2000; animation: draw-line 2.5s ease-out forwards; }
        .chart-fill { animation: fade-up 1.5s ease-out 0.6s forwards; opacity: 0; }
        .card-hover {
          transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        }
        .card-hover:hover {
          transform: translateY(-4px);
          box-shadow: 0 16px 48px rgba(0, 0, 0, 0.08);
        }
        .shimmer-bg {
          background: linear-gradient(90deg, transparent, rgba(255,255,255,0.03), transparent);
          background-size: 200% 100%;
          animation: shimmer 3s ease-in-out infinite;
        }
      `}</style>

      <Header />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto space-y-8">
        {/* ===== HERO CARD ===== */}
        <div className={`relative overflow-hidden rounded-2xl bg-gradient-to-br from-primary/[0.12] via-surface to-surface-container border border-primary/20 shadow-sm ${visible ? "animate-scale-in" : ""}`} style={{ animationDelay: "0s" }}>
          <div className="absolute top-0 right-0 w-96 h-96 bg-gradient-to-bl from-primary/20 via-primary/[0.08] to-transparent rounded-full blur-3xl pointer-events-none" />
          <div className="absolute bottom-0 left-0 w-64 h-64 bg-gradient-to-tr from-secondary/[0.08] via-transparent to-transparent rounded-full blur-3xl pointer-events-none" />
          <div className="absolute inset-x-0 top-0 h-0.5 bg-gradient-to-r from-transparent via-primary/50 to-transparent" />

          <div className="relative z-10 p-8">
            <div className="flex items-start justify-between gap-8">
              <div className="flex-1">
                <div className="flex items-center gap-3 mb-4">
                  <span className="px-3 py-1 bg-primary/10 text-primary rounded-full text-label-sm font-semibold inline-flex items-center gap-1.5">
                    <span className="w-1.5 h-1.5 bg-primary rounded-full animate-pulse" />
                    Dashboard Overview
                  </span>
                  <span className="text-label-sm text-outline">{new Date().toLocaleDateString("en-US", { weekday: "long", month: "long", day: "numeric" })}</span>
                </div>

                <h2 className="text-headline-lg text-on-surface tracking-tight mb-2">
                  {(() => {
                    const h = new Date().getHours();
                    if (h < 12) return "Good morning";
                    if (h < 18) return "Good afternoon";
                    return "Good evening";
                  })()}, <span className="text-primary">{profileName}</span>!
                </h2>
                <p className="text-body-lg text-on-surface-variant max-w-xl mb-6">Here&apos;s your brand performance snapshot. Everything you need is right here.</p>

                <div className="flex flex-wrap items-center gap-4">
                  <div className="flex items-center gap-3 bg-surface-container rounded-xl px-4 py-2.5">
                    <div className="flex -space-x-1.5">
                      <div className="w-8 h-8 rounded-full bg-[#1877F2] flex items-center justify-center hover:scale-110 transition-transform shadow-sm" style={{ animation: "float-y 2s ease-in-out infinite", animationDelay: "0s" }}>
                        <svg className="w-4 h-4 text-white" viewBox="0 0 24 24" fill="currentColor"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/></svg>
                      </div>
                      <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-[#F58529] via-[#DD2A7B] to-[#8134AF] flex items-center justify-center hover:scale-110 transition-transform shadow-sm" style={{ animation: "float-y 2s ease-in-out infinite", animationDelay: "0.15s" }}>
                        <svg className="w-4 h-4 text-white" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zM12 0C8.741 0 8.333.014 7.053.072 2.695.272.273 2.69.073 7.052.014 8.333 0 8.741 0 12c0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98C8.333 23.986 8.741 24 12 24c3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98C15.668.014 15.259 0 12 0zm0 5.838a6.162 6.162 0 100 12.324 6.162 6.162 0 000-12.324zM12 16a4 4 0 110-8 4 4 0 010 8zm6.406-11.845a1.44 1.44 0 100 2.881 1.44 1.44 0 000-2.881z"/></svg>
                      </div>
                      <div className="w-8 h-8 rounded-full bg-[#0A66C2] flex items-center justify-center hover:scale-110 transition-transform shadow-sm" style={{ animation: "float-y 2s ease-in-out infinite", animationDelay: "0.3s" }}>
                        <svg className="w-4 h-4 text-white" viewBox="0 0 24 24" fill="currentColor"><path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/></svg>
                      </div>
                    </div>
                    <span className="text-label-sm text-on-surface-variant">3 platforms connected</span>
                  </div>
                  <div className="flex items-center gap-2.5 bg-surface-container rounded-xl px-4 py-2.5">
                    <span className="material-symbols-outlined text-success-green text-[18px]">check_circle</span>
                    <div>
                      <span className="text-label-sm text-on-surface-variant">Status</span>
                      <p className="text-label-md text-success-green font-semibold">All systems nominal</p>
                    </div>
                  </div>
                </div>
              </div>

              <div className="hidden lg:flex flex-col items-center gap-3 shrink-0">
                <div className="w-20 h-20 rounded-2xl bg-gradient-to-br from-primary/10 to-primary/5 border border-outline-variant/30 flex items-center justify-center shadow-sm">
                  <span className="text-[28px] font-bold text-primary">
                    {profileName.split(" ").map(w => w[0]).join("").toUpperCase().slice(0, 2)}
                  </span>
                </div>
                <div className="text-center">
                  <p className="text-body-sm text-on-surface font-semibold">{profileName}</p>
                  <p className="text-label-sm text-outline">Active Profile</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* ===== KPI GRID ===== */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-gutter">
          {kpiData.map((kpi, i) => (
            <div
              key={kpi.label}
              className={`relative bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden group ${visible ? "animate-fade-up" : ""} card-hover`}
              style={{ animationDelay: `${0.08 * i}s` }}
            >
              <div className={`absolute inset-0 bg-gradient-to-br ${kpi.gradient} pointer-events-none`} />
              <div className="absolute inset-x-0 top-0 h-0.5 scale-x-0 group-hover:scale-x-100 transition-transform duration-500" style={{ background: `linear-gradient(90deg, transparent, ${kpi.accent}, transparent)` }} />
              <div className="relative p-6">
                <div className="flex items-start justify-between mb-4">
                  <div className={`w-11 h-11 rounded-xl bg-gradient-to-br ${kpi.iconBg} flex items-center justify-center ${kpi.iconColor} group-hover:scale-110 transition-transform duration-300`}>
                    <span className="material-symbols-outlined text-[22px]">{kpi.icon}</span>
                  </div>
                  {kpi.delta !== null && (
                    <span className={`flex items-center gap-1 text-label-sm px-2 py-1 rounded-full font-semibold ${
                      kpi.deltaUp === true ? "bg-emerald-50 text-emerald-600" :
                      kpi.deltaUp === false ? "bg-red-50 text-red-500" :
                      "bg-surface-container-high text-on-surface-variant"
                    }`}>
                      {kpi.deltaUp === true && <span className="material-symbols-outlined text-[14px]">trending_up</span>}
                      {kpi.deltaUp === false && <span className="material-symbols-outlined text-[14px]">trending_down</span>}
                      {kpi.delta}
                    </span>
                  )}
                </div>
                <p className="text-label-sm text-on-surface-variant mb-1.5 font-medium">{kpi.label}</p>
                <div className="flex items-baseline gap-2">
                  <h3 className="text-[32px] font-bold text-on-surface leading-none tracking-tight">
                    <CountUp value={kpi.value} />
                  </h3>
                  {kpi.max && <span className="text-label-md text-outline">/ {kpi.max}</span>}
                </div>
                {kpi.pct && (
                  <div className="mt-3">
                    <AnimatedBar value={kpi.pct} color="bg-gradient-to-r from-emerald-400 to-emerald-500" delay={800 + i * 100} />
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>

        {/* ===== CHART + UPCOMING ===== */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-gutter">
          {/* Chart */}
          <div className={`lg:col-span-2 bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.32s" }}>
            <div className="flex items-center justify-between mb-6">
              <div>
                <div className="flex items-center gap-2 mb-1">
                  <h4 className="text-headline-sm text-on-surface">Performance Overview</h4>
                  <span className="px-2 py-0.5 bg-primary/10 text-primary rounded-full text-[10px] font-semibold inline-flex items-center gap-1">
                    <span className="w-1.5 h-1.5 bg-primary rounded-full animate-pulse" />
                    LIVE
                  </span>
                </div>
                <p className="text-body-sm text-on-surface-variant">Daily engagement trends over the last 30 days</p>
              </div>
              <div className="flex items-center gap-1.5 bg-surface-container rounded-lg p-1">
                <button className="px-3 py-1.5 bg-white text-on-surface rounded-md text-label-sm font-semibold shadow-sm transition-all hover:shadow-md">30D</button>
                <button className="px-3 py-1.5 text-on-surface-variant rounded-md text-label-sm hover:text-on-surface transition-all">90D</button>
              </div>
            </div>
            <div className="relative h-72 w-full">
              <svg className="absolute inset-0 h-full w-full pointer-events-none" viewBox="0 0 1000 300" preserveAspectRatio="none">
                <defs>
                  <linearGradient id="chartGrad" x1="0" x2="0" y1="0" y2="1">
                    <stop offset="0%" stopColor="#0f62fe" stopOpacity="0.25" />
                    <stop offset="100%" stopColor="#0f62fe" stopOpacity="0" />
                  </linearGradient>
                  <linearGradient id="chartGrad2" x1="0" x2="0" y1="0" y2="1">
                    <stop offset="0%" stopColor="#731be5" stopOpacity="0.15" />
                    <stop offset="100%" stopColor="#731be5" stopOpacity="0" />
                  </linearGradient>
                  <filter id="glow">
                    <feGaussianBlur stdDeviation="3" result="blur" />
                    <feMerge><feMergeNode in="blur" /><feMergeNode in="SourceGraphic" /></feMerge>
                  </filter>
                </defs>
                <line x1="0" y1="250" x2="1000" y2="250" stroke="#e0e0e0" strokeOpacity="0.12" strokeWidth="0.5" strokeDasharray="4,4" />
                <line x1="0" y1="200" x2="1000" y2="200" stroke="#e0e0e0" strokeOpacity="0.12" strokeWidth="0.5" strokeDasharray="4,4" />
                <line x1="0" y1="150" x2="1000" y2="150" stroke="#e0e0e0" strokeOpacity="0.12" strokeWidth="0.5" strokeDasharray="4,4" />
                <line x1="0" y1="100" x2="1000" y2="100" stroke="#e0e0e0" strokeOpacity="0.12" strokeWidth="0.5" strokeDasharray="4,4" />
                <line x1="0" y1="50" x2="1000" y2="50" stroke="#e0e0e0" strokeOpacity="0.12" strokeWidth="0.5" strokeDasharray="4,4" />
                <path className="chart-fill" d="M0,200 Q100,180 200,220 T400,150 T600,100 T800,180 T1000,50 L1000,300 L0,300 Z" fill="url(#chartGrad)" />
                <path className="chart-line" d="M0,200 Q100,180 200,220 T400,150 T600,100 T800,180 T1000,50" fill="none" stroke="#0f62fe" strokeLinecap="round" strokeWidth="2.5" filter="url(#glow)" />
                <circle cx="1000" cy="50" r="4" fill="#0f62fe" stroke="#fff" strokeWidth="2" className="animate-float" />
                <circle cx="400" cy="150" r="3" fill="#731be5" stroke="#fff" strokeWidth="1.5" />
                <path className="chart-fill" d="M0,230 Q100,210 200,250 T400,180 T600,140 T800,210 T1000,90 L1000,300 L0,300 Z" fill="url(#chartGrad2)" style={{ animationDelay: "0.8s" }} />
                <path className="chart-line" d="M0,230 Q100,210 200,250 T400,180 T600,140 T800,210 T1000,90" fill="none" stroke="#731be5" strokeLinecap="round" strokeWidth="1.5" strokeDasharray="6,4" style={{ animationDelay: "0.5s" }} />
              </svg>
              <div className="absolute top-4 right-4 flex items-center gap-3">
                <div className="flex items-center gap-1.5">
                  <div className="w-3 h-0.5 bg-[#0f62fe] rounded animate-glow-pulse" />
                  <span className="text-[10px] text-outline">Engagement</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="w-3 h-0.5 bg-[#731be5]" style={{ borderTop: "1px dashed" }} />
                  <span className="text-[10px] text-outline">Predicted</span>
                </div>
              </div>
              <div className="absolute right-0 top-0 bottom-0 w-1/4 bg-gradient-to-r from-transparent to-primary/[0.02] border-l border-dashed border-primary/20 flex items-center justify-center backdrop-blur-[1px]">
                <div className="bg-gradient-to-r from-primary to-secondary text-white px-4 py-1.5 rounded-full text-[10px] font-semibold flex items-center gap-1.5 shadow-lg shadow-primary/30 hover:scale-105 transition-transform">
                  <span className="material-symbols-outlined text-[12px]">auto_awesome</span>
                  AI PREDICTION
                </div>
              </div>
            </div>
            <div className="flex justify-between mt-4 px-1 text-[10px] text-outline font-medium">
              <span className="hover:text-on-surface transition-colors cursor-pointer">Oct 01</span>
              <span className="relative hover:text-on-surface transition-colors cursor-pointer">Oct 15 <span className="absolute -top-1 -right-2 w-1.5 h-1.5 bg-primary rounded-full animate-ping" style={{ animationDuration: "2s" }} /></span>
              <span className="text-primary font-bold cursor-pointer">Today</span>
              <span className="text-primary-fixed-dim cursor-pointer">Oct 30</span>
            </div>
          </div>

          {/* Upcoming */}
          <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 flex flex-col ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.4s" }}>
            <div className="flex items-center justify-between mb-5">
              <div className="flex items-center gap-2">
                <h4 className="text-headline-sm text-on-surface">Schedule</h4>
                <span className="px-1.5 py-0.5 bg-amber-50 text-amber-600 rounded-md text-[10px] font-semibold">{scheduleData.length}</span>
              </div>
              <button className="text-label-sm text-primary font-semibold hover:text-primary-container transition-colors flex items-center gap-1 group">
                View All
                <span className="material-symbols-outlined text-[14px] group-hover:translate-x-0.5 transition-transform">arrow_forward</span>
              </button>
            </div>
            <div className="space-y-2 flex-1">
              {scheduleData.map((post, i) => (
                <div
                  key={i}
                  className="group flex items-center gap-3 p-2.5 rounded-xl hover:bg-surface-container hover:shadow-sm transition-all duration-200 cursor-pointer"
                  style={{ animation: `slide-up-row 0.4s ease-out ${0.42 + i * 0.06}s forwards`, opacity: 0 }}
                >
                  <div className={`w-10 h-10 rounded-xl ${post.platformBg} flex items-center justify-center shrink-0 group-hover:scale-110 group-hover:rotate-3 transition-all duration-300`}>
                    <span className={`text-[11px] font-bold ${post.platformColor}`}>{post.platform}</span>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-body-sm font-medium text-on-surface truncate group-hover:text-primary transition-colors">{post.title}</p>
                    <p className="text-[11px] text-outline">{post.time}</p>
                  </div>
                  <div className="opacity-0 group-hover:opacity-100 transition-all duration-200 translate-x-2 group-hover:translate-x-0">
                    <span className="material-symbols-outlined text-outline text-[18px]">more_vert</span>
                  </div>
                </div>
              ))}
            </div>
            <div className="mt-4 pt-4 border-t border-outline-variant/20">
              <div className="p-3.5 bg-gradient-to-r from-purple-500/[0.06] to-purple-500/[0.02] rounded-xl border border-purple-500/10 hover:border-purple-500/20 transition-colors">
                <div className="flex items-center gap-2 mb-1.5">
                  <span className="material-symbols-outlined text-purple-500 text-[16px] animate-float">auto_awesome</span>
                  <span className="text-label-sm text-purple-600 font-semibold">AI Insight</span>
                </div>
                <p className="text-[11px] leading-relaxed text-on-surface-variant">
                  Your audience is <span className="text-on-surface font-semibold">24% more active</span> at 8:00 PM on Sundays. Consider rescheduling.
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* ===== RECENT CAMPAIGNS ===== */}
        <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.48s" }}>
          <div className="px-6 py-5 border-b border-outline-variant/20 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <h4 className="text-headline-sm text-on-surface">Recent Campaigns</h4>
              <span className="px-2 py-0.5 bg-primary/10 text-primary rounded-full text-[10px] font-semibold">{campaignsData.length} active</span>
            </div>
            <button className="flex items-center gap-1.5 text-label-sm text-primary font-semibold hover:text-primary-container transition-colors group">
              <span className="material-symbols-outlined text-[16px] group-hover:scale-110 transition-transform">download</span>
              Export CSV
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="text-label-sm text-outline border-b border-outline-variant/10">
                  <th className="px-6 py-3.5 font-semibold">Campaign Name</th>
                  <th className="px-6 py-3.5 font-semibold">Platform</th>
                  <th className="px-6 py-3.5 font-semibold">Budget</th>
                  <th className="px-6 py-3.5 font-semibold">Spent</th>
                  <th className="px-6 py-3.5 font-semibold">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-outline-variant/10">
                {campaignsData.map((row, i) => (
                  <tr
                    key={i}
                    className="group hover:bg-surface-container/40 transition-colors duration-150"
                    style={{ animation: `slide-up-row 0.4s ease-out ${0.5 + i * 0.08}s forwards`, opacity: 0 }}
                  >
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-lg bg-surface-container-high flex items-center justify-center group-hover:scale-110 group-hover:bg-primary/10 transition-all duration-300">
                          <span className="material-symbols-outlined text-outline group-hover:text-primary text-[16px] transition-colors">campaign</span>
                        </div>
                        <span className="text-body-sm font-medium text-on-surface group-hover:text-primary transition-colors">{row.name}</span>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span className={`px-2.5 py-1 ${row.bg} ${row.color} rounded-lg text-[10px] font-bold tracking-wide inline-block hover:scale-105 transition-transform`}>{row.platform}</span>
                    </td>
                    <td className="px-6 py-4 text-body-sm text-on-surface font-medium">{row.budget}</td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <span className="text-body-sm text-on-surface font-medium">{row.spent}</span>
                        <span className="text-label-sm text-outline">({Math.round(parseCurrency(row.spent) / parseCurrency(row.budget) * 100)}%)</span>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-sm font-semibold ${
                        row.status === "Active" ? "bg-emerald-50 text-emerald-600" : "bg-surface-container-high text-on-surface-variant"
                      }`}>
                        <span className={`w-1.5 h-1.5 rounded-full ${row.status === "Active" ? "bg-emerald-500 animate-pulse" : "bg-outline"}`} />
                        {row.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* ===== BOTTOM GRID ===== */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-gutter">
          {[
            { title: "Geographic Distribution", icon: "map", color: "from-blue-500/10 to-blue-600/5", iconColor: "text-blue-500", type: "geo", data: [
              { label: "United States", value: 38 }, { label: "United Kingdom", value: 22 }, { label: "Germany", value: 15 }, { label: "Japan", value: 12 }, { label: "Others", value: 13 }
            ] },
            { title: "Top Demographics", icon: "pie_chart", color: "from-purple-500/10 to-purple-600/5", iconColor: "text-purple-500", type: "demographics", data: [
              { label: "18-24", value: 28 }, { label: "25-34", value: 42 }, { label: "35-44", value: 20 }, { label: "45+", value: 10 }
            ] },
            { title: "Device Breakdown", icon: "devices", color: "from-amber-500/10 to-amber-600/5", iconColor: "text-amber-500", type: "devices", data: [
              { label: "Mobile", value: 72, color: "bg-gradient-to-r from-blue-500 to-blue-400" },
              { label: "Desktop", value: 24, color: "bg-gradient-to-r from-purple-500 to-purple-400" },
              { label: "Tablet", value: 4, color: "bg-gradient-to-r from-amber-500 to-amber-400" },
            ] },
          ].map((item, i) => (
            <div
              key={item.title}
              className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 ${visible ? "animate-fade-up" : ""} card-hover`}
              style={{ animationDelay: `${0.56 + 0.08 * i}s` }}
            >
              <div className="flex items-center gap-2.5 mb-5">
                <div className={`w-9 h-9 rounded-xl bg-gradient-to-br ${item.color} flex items-center justify-center ${item.iconColor}`}>
                  <span className="material-symbols-outlined text-[20px]">{item.icon}</span>
                </div>
                <h5 className="text-label-md text-on-surface font-semibold">{item.title}</h5>
              </div>
              <div className="space-y-3">
                {item.data.map((d, j) => (
                  <div key={d.label} className="space-y-1">
                    <div className="flex justify-between items-center">
                      <span className="text-label-sm text-on-surface-variant">{d.label}</span>
                      <span className="text-label-sm text-on-surface font-semibold">{d.value}%</span>
                    </div>
                    <AnimatedBar
                      value={d.value}
                      color={(d as any).color || (item.type === "geo" ? "bg-gradient-to-r from-blue-500 to-blue-400" : "bg-gradient-to-r from-purple-500 to-purple-400")}
                      delay={900 + i * 100 + j * 50}
                    />
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>

        {/* ===== AI SUGGESTIONS ===== */}
        <div className={`${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.8s" }}>
          <div className="flex items-center justify-between mb-5">
            <div className="flex items-center gap-2">
              <div className="w-2 h-2 bg-gradient-to-r from-purple-500 to-pink-500 rounded-full animate-pulse" />
              <h4 className="text-headline-sm text-on-surface">AI Content Suggestions</h4>
            </div>
            <div className="flex items-center gap-1.5">
              <button className="w-8 h-8 rounded-full border border-outline-variant/30 flex items-center justify-center hover:bg-surface-container hover:border-primary/30 transition-all hover:shadow-sm">
                <span className="material-symbols-outlined text-[16px] text-outline">chevron_left</span>
              </button>
              <button className="w-8 h-8 rounded-full border border-outline-variant/30 flex items-center justify-center hover:bg-surface-container hover:border-primary/30 transition-all hover:shadow-sm">
                <span className="material-symbols-outlined text-[16px] text-outline">chevron_right</span>
              </button>
            </div>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-gutter">
            {aiSuggestions.map((item, i) => (
              <div
                key={item.title}
                className="group bg-surface-container-lowest rounded-xl border border-outline-variant/20 shadow-sm p-5 card-hover cursor-pointer relative overflow-hidden"
                style={{ animation: `fade-up 0.5s ease-out ${0.88 + i * 0.08}s forwards`, opacity: 0 }}
              >
                <div className="absolute inset-0 bg-gradient-to-br from-transparent via-transparent to-primary/[0.02] opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none" />
                <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${item.bg} flex items-center justify-center ${item.color} mb-4 group-hover:scale-110 group-hover:rotate-3 transition-all duration-300`}>
                  <span className="material-symbols-outlined text-[20px]">{item.icon}</span>
                </div>
                <h6 className="text-label-md text-on-surface font-semibold mb-2 group-hover:text-primary transition-colors">{item.title}</h6>
                <p className="text-[12px] text-on-surface-variant leading-relaxed">{item.desc}</p>
                <div className="mt-3 flex items-center gap-1 text-[10px] text-primary font-semibold opacity-0 group-hover:opacity-100 transition-all duration-200 translate-y-1 group-hover:translate-y-0">
                  View insights <span className="material-symbols-outlined text-[12px]">arrow_forward</span>
                </div>
                <div className="absolute top-0 right-0 w-16 h-16 bg-gradient-to-bl from-primary/[0.03] to-transparent rounded-bl-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
              </div>
            ))}
          </div>
        </div>

        {/* ===== PLATFORM DISTRIBUTION ===== */}
        <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.96s" }}>
          <div className="flex items-center gap-2 mb-6">
            <h4 className="text-headline-sm text-on-surface">Platform Distribution</h4>
            <span className="px-2 py-0.5 bg-primary/10 text-primary rounded-full text-[10px] font-semibold">TOTAL 4</span>
          </div>
          <div className="h-56 flex items-end gap-6 px-4 pb-2 border-b border-outline-variant/20">
            {[
              { label: "Facebook", height: "85%", color: "bg-gradient-to-t from-blue-500 to-blue-400", value: "45%" },
              { label: "Instagram", height: "65%", color: "bg-gradient-to-t from-purple-500 to-purple-400", value: "28%" },
              { label: "LinkedIn", height: "40%", color: "bg-gradient-to-t from-amber-500 to-amber-400", value: "18%" },
              { label: "Others", height: "25%", color: "bg-gradient-to-t from-outline to-outline-variant", value: "9%" },
            ].map((item, i) => (
              <div key={i} className="flex-1 flex flex-col items-center gap-2 group">
                <span className="text-[10px] text-outline font-semibold opacity-0 group-hover:opacity-100 transition-opacity">{item.value}</span>
                <div
                  className={`w-full ${item.color} rounded-t-lg transition-all duration-700 group-hover:rounded-t-xl`}
                  style={{
                    height: "0%",
                    animation: visible ? `bar-grow 0.8s ease-out ${1.0 + i * 0.15}s forwards` : "none",
                  }}
                />
                <span className="text-label-sm text-outline font-medium group-hover:text-on-surface transition-colors">{item.label}</span>
              </div>
            ))}
          </div>
        </div>

        {/* ===== FOOTER ===== */}
        <footer className="flex items-center justify-between text-outline/50">
          <div className="flex items-center gap-5">
            <span className="text-label-sm font-mono">v1.4.2-alpha</span>
            <span className="w-1 h-1 rounded-full bg-outline/20" />
            <span className="text-label-sm flex items-center gap-1.5">
              <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full animate-pulse" />
              All systems operational
            </span>
          </div>
          <div className="flex items-center gap-1 text-label-sm font-mono">
            <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full" />
            API
          </div>
        </footer>
      </main>
    </>
  );
}
