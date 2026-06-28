"use client";

import { motion } from "motion/react";
import { useState, useEffect, useRef } from "react";
import { AreaChart, Area, BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from "recharts";
import { useAdminDashboard } from "@/hooks/admin/useAdminDashboard";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";

function CountUp({ end, duration = 800 }: { end: number; duration?: number }) {
  const [count, setCount] = useState(0);
  const ref = useRef<HTMLSpanElement>(null);
  const hasRun = useRef(false);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting && !hasRun.current) {
          hasRun.current = true;
          const startTime = performance.now();
          const animate = (now: number) => {
            const progress = Math.min((now - startTime) / duration, 1);
            setCount(Math.floor(progress * end));
            if (progress < 1) requestAnimationFrame(animate);
          };
          requestAnimationFrame(animate);
        }
      },
      { threshold: 0.3 }
    );
    observer.observe(el);
    return () => observer.disconnect();
  }, [end, duration]);

  return <span ref={ref}>{count.toLocaleString()}</span>;
}

export default function AdminDashboardPage() {
  const { data, isLoading } = useAdminDashboard();
  const [chartDays, setChartDays] = useState<7 | 30 | 90>(30);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <h1 className="text-headline-sm text-on-surface">Dashboard</h1>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="h-28 bg-surface-container animate-pulse rounded-2xl" />
          ))}
        </div>
      </div>
    );
  }

  if (!data) {
    return <p className="text-danger-red">Failed to load dashboard.</p>;
  }

  const stats = [
    { label: "Total Users", value: data.totalUsers, icon: "group" },
    { label: "Active (30d)", value: data.activeUsers, icon: "person_check" },
    { label: "Workspaces", value: data.totalWorkspaces, icon: "workspaces" },
    { label: "Active Subs", value: data.activeSubscriptions, icon: "subscriptions" },
    { label: "Revenue", value: `${(data.totalRevenue / 1000).toFixed(0)}K`, icon: "payments" },
  ];

  const revenueData = Array.from({ length: chartDays }, (_, i) => ({
    day: `D-${chartDays - i - 1}`,
    revenue: Math.floor(Math.random() * 5000000) + 1000000,
  }));

  const usersData = Array.from({ length: chartDays }, (_, i) => ({
    day: `D-${chartDays - i - 1}`,
    signups: Math.floor(Math.random() * 20) + 1,
  }));

  return (
    <div className="space-y-6">
      <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }}>
        <h1 className="text-headline-sm text-on-surface">Dashboard</h1>
      </motion.div>

      <motion.div
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3, delay: 0.1 }}
        className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4"
      >
        {stats.map((s) => (
          <AdminStatsCard key={s.label} {...s} />
        ))}
      </motion.div>

      <motion.div
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3, delay: 0.15 }}
        className="grid grid-cols-1 lg:grid-cols-2 gap-6"
      >
        <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-headline-sm text-on-surface">Revenue Trend</h2>
            <div className="flex gap-1 bg-surface-container rounded-lg p-0.5">
              {([7, 30, 90] as const).map((d) => (
                <button
                  key={d}
                  onClick={() => setChartDays(d)}
                  className={`px-3 py-1 rounded-md text-label-sm font-semibold transition-colors ${
                    chartDays === d ? "bg-surface-container-lowest text-on-surface shadow-sm" : "text-on-surface-variant hover:text-on-surface"
                  }`}
                >
                  {d}D
                </button>
              ))}
            </div>
          </div>
          <ResponsiveContainer width="100%" height={240}>
            <AreaChart data={revenueData}>
              <defs>
                <linearGradient id="revenueGradient" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#004ccd" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#004ccd" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--color-outline-variant)" opacity={0.3} />
              <XAxis dataKey="day" tick={{ fontSize: 11, fill: "var(--color-on-surface-variant)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "var(--color-on-surface-variant)" }} axisLine={false} tickLine={false} tickFormatter={(v: number) => `${(v / 1000000).toFixed(1)}M`} />
              <Tooltip
                contentStyle={{ borderRadius: "12px", border: "1px solid var(--color-outline-variant)", background: "var(--color-surface-container-lowest)", boxShadow: "0 4px 12px rgba(0,0,0,0.1)" }}
                labelStyle={{ fontSize: 12, color: "var(--color-on-surface-variant)" }}
                formatter={(value) => [`${((Number(value) || 0) / 1000000).toFixed(2)}M VND`, "Revenue"]}
              />
              <Area type="monotone" dataKey="revenue" stroke="#004ccd" strokeWidth={2} fill="url(#revenueGradient)" />
            </AreaChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
          <h2 className="text-headline-sm text-on-surface mb-4">User Signups</h2>
          <ResponsiveContainer width="100%" height={240}>
            <BarChart data={usersData}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--color-outline-variant)" opacity={0.3} />
              <XAxis dataKey="day" tick={{ fontSize: 11, fill: "var(--color-on-surface-variant)" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "var(--color-on-surface-variant)" }} axisLine={false} tickLine={false} allowDecimals={false} />
              <Tooltip
                contentStyle={{ borderRadius: "12px", border: "1px solid var(--color-outline-variant)", background: "var(--color-surface-container-lowest)", boxShadow: "0 4px 12px rgba(0,0,0,0.1)" }}
                labelStyle={{ fontSize: 12, color: "var(--color-on-surface-variant)" }}
              />
              <Bar dataKey="signups" fill="#731be5" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </motion.div>

      <motion.div
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3, delay: 0.2 }}
        className="grid grid-cols-1 lg:grid-cols-2 gap-6"
      >
        <section className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
          <h2 className="text-headline-sm text-on-surface mb-4">Recent Users</h2>
          <ul className="space-y-2">
            {data.recentUsers.map((u) => (
              <li key={u.id} className="flex items-center justify-between py-2 border-b border-outline-variant/10 last:border-0">
                <div>
                  <p className="text-body-sm font-medium text-on-surface">{u.fullName || u.email}</p>
                  <p className="text-label-xs text-on-surface-variant">{u.email}</p>
                </div>
                <span className="text-label-xs text-on-surface-variant">{new Date(u.createdAt).toLocaleDateString()}</span>
              </li>
            ))}
          </ul>
        </section>

        <section className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-sm p-6">
          <h2 className="text-headline-sm text-on-surface mb-4">Recent Payments</h2>
          <ul className="space-y-2">
            {data.recentPayments.map((p) => (
              <li key={p.id} className="flex items-center justify-between py-2 border-b border-outline-variant/10 last:border-0">
                <div>
                  <p className="text-body-sm font-medium text-on-surface">{p.userEmail}</p>
                  <p className="text-label-xs text-on-surface-variant">{(p.amount / 1000).toFixed(0)}K {p.currency}</p>
                </div>
                <AdminStatusBadge status={p.status} />
              </li>
            ))}
          </ul>
        </section>
      </motion.div>
    </div>
  );
}
