"use client";

import { useState, useEffect, Suspense } from "react";
import { usePathname, useSearchParams } from "next/navigation";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import {
  fetchSchedules, createSchedule,
  updateSchedule, deleteSchedule, type ScheduleItem,
  onScheduleChange,
} from "@/services/scheduleService";
import { fetchContents } from "@/services/contentService";
import { fetchBrands } from "@/services/brandService";
import { fetchSocialIntegrations, type SocialIntegration } from "@/services/socialAccountService";
import { PLATFORM_CONFIG, PlatformIcon, getTypeStyle, getTypeConfig, getBrandColor, type ContentType } from "@/lib/contentConstants";
import type { ContentItem } from "@/services/contentService";

type ViewMode = "month" | "week" | "list";

function getWeekDays(offset: number) {
  const now = new Date();
  const day = now.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  const monday = new Date(now);
  monday.setDate(now.getDate() + diff + offset * 7);
  const days: Date[] = [];
  for (let i = 0; i < 7; i++) {
    const d = new Date(monday);
    d.setDate(monday.getDate() + i);
    days.push(d);
  }
  return days;
}

function formatWeekRange(offset: number) {
  const days = getWeekDays(offset);
  const start = days[0];
  const end = days[6];
  const opts: Intl.DateTimeFormatOptions = { month: "short", day: "numeric" };
  return `${start.toLocaleDateString("en-US", opts)} – ${end.toLocaleDateString("en-US", opts)}, ${start.getFullYear()}`;
}

const MONTHS = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
const WEEKDAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

const STATUS_STYLE: Record<string, string> = {
  Pending: "bg-warning-amber/10 text-warning-amber border-warning-amber/20",
  Processing: "bg-sky-500/10 text-sky-500 border-sky-500/20",
  Completed: "bg-emerald-50 text-emerald-600 border-emerald-500/20",
  Failed: "bg-danger-red/10 text-danger-red border-danger-red/20",
};
const STATUS_DOT: Record<string, string> = {
  Pending: "bg-warning-amber",
  Processing: "bg-sky-500",
  Completed: "bg-emerald-500",
  Failed: "bg-danger-red",
};

function getDayGrid(year: number, month: number) {
  const first = new Date(year, month, 1);
  const last = new Date(year, month + 1, 0);
  const startPad = first.getDay();
  const days: (number | null)[] = [];
  for (let i = 0; i < startPad; i++) days.push(null);
  for (let d = 1; d <= last.getDate(); d++) days.push(d);
  return days;
}

function sameDay(a: Date, b: Date) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

function formatTime(iso: string) {
  const d = new Date(iso);
  return d.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
}

function formatDate(iso: string) {
  const d = new Date(iso);
  return d.toLocaleDateString("en-GB", { day: "numeric", month: "short", year: "numeric" });
}

type SortKey = "scheduledAt" | "title" | "status" | "brandName";
type SortDir = "asc" | "desc";

function renderSortIcon(activeKey: SortKey, direction: SortDir, key: SortKey) {
  if (activeKey !== key) {
    return <span className="material-symbols-outlined text-[12px] text-outline/20 ml-0.5">unfold_more</span>;
  }

  return (
    <span className="material-symbols-outlined text-[12px] text-primary ml-0.5">
      {direction === "asc" ? "expand_less" : "expand_more"}
    </span>
  );
}

function CalendarContent() {
  const featureGate = useFeatureGate();
  const { activeWorkspace } = useWorkspaces();
  const today = new Date();
  const [view, setView] = useState<ViewMode>("month");
  const [year, setYear] = useState(today.getFullYear());
  const [month, setMonth] = useState(today.getMonth());
  const [weekOffset, setWeekOffset] = useState(0);
  const [schedules, setSchedules] = useState<ScheduleItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedDay, setSelectedDay] = useState<Date | null>(today);
  const [sortKey, setSortKey] = useState<SortKey>("scheduledAt");
  const [sortDir, setSortDir] = useState<SortDir>("asc");
  const [toast, setToast] = useState<string | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [contents, setContents] = useState<ContentItem[]>([]);
  const [integrations, setIntegrations] = useState<SocialIntegration[]>([]);
  const [form, setForm] = useState({ contentId: "", integrationId: "", date: "", time: "" });
  const [actionId, setActionId] = useState<string | null>(null);
  const [deletedItem, setDeletedItem] = useState<ScheduleItem | null>(null);
  const [editingSchedule, setEditingSchedule] = useState<ScheduleItem | null>(null);
  const [editForm, setEditForm] = useState({ date: "", time: "", integrationId: "" });
  const [filterBrand, setFilterBrand] = useState("");
  const [filterPlatform, setFilterPlatform] = useState("");
  const [filterStatus, setFilterStatus] = useState("");

  const brands = [...new Set(schedules.map((s) => s.brandName).filter(Boolean))] as string[];
  const pendingCount = schedules.filter((s) => s.status === "Pending" || s.status === "Processing").length;
  const completedCount = schedules.filter((s) => s.status === "Completed").length;
  const failedCount = schedules.filter((s) => s.status === "Failed").length;

  const filteredSchedules = schedules.filter((s) => {
    if (filterBrand && s.brandName !== filterBrand) return false;
    if (filterPlatform && s.platform !== filterPlatform) return false;
    if (filterStatus && s.status !== filterStatus) return false;
    return true;
  });

  const pathname = usePathname();
  const searchParams = useSearchParams();

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const [sched, cont, brands] = await Promise.all([
          fetchSchedules({ pageSize: 100 }),
          fetchContents({ pageSize: 100 }),
          fetchBrands(),
        ]);
        const integrationsArrays = await Promise.all(brands.map((b) => fetchSocialIntegrations(b.id)));
        const integ = integrationsArrays.flat();
        setSchedules(sched.data.data);
        if (sched.error) setToast(sched.error);
        setContents(cont?.items ?? []);
        setIntegrations(integ);
      } catch { /* ignore */ }
      setLoading(false);
    };
    load();

    const pollSchedules = async () => {
      const res = await fetchSchedules({ pageSize: 100 });
      setSchedules(res.data.data);
    };
    const pollInterval = setInterval(pollSchedules, 30_000);

    const unsubscribe = onScheduleChange(load);
    const handleVisibilityChange = () => {
      if (document.visibilityState === "visible") {
        load();
      }
    };
    document.addEventListener("visibilitychange", handleVisibilityChange);
    return () => {
      unsubscribe();
      document.removeEventListener("visibilitychange", handleVisibilityChange);
      clearInterval(pollInterval);
    };
  }, [pathname, activeWorkspace?.id]);

  useEffect(() => { if (toast) setTimeout(() => setToast(null), 3000); }, [toast]);

  useEffect(() => {
    const contentId = searchParams.get("contentId");
    if (contentId && contents.length > 0) {
      const match = contents.find((c) => c.id === contentId && c.status !== "Published");
      if (match) {
        const now = new Date();
        const date = now.toISOString().slice(0, 10);
        const time = `${String(now.getHours()).padStart(2, "0")}:${String(now.getMinutes()).padStart(2, "0")}`;
        setForm({ contentId, integrationId: "", date, time });
        setShowCreate(true);
      }
    }
  }, [searchParams, contents]);

  const handlePrev = () => {
    if (month === 0) { setYear((y) => y - 1); setMonth(11); }
    else setMonth((m) => m - 1);
  };
  const handleNext = () => {
    if (month === 11) { setYear((y) => y + 1); setMonth(0); }
    else setMonth((m) => m + 1);
  };
  const goToday = () => { const d = new Date(); setYear(d.getFullYear()); setMonth(d.getMonth()); setSelectedDay(d); setWeekOffset(0); };

  const handleSort = (k: SortKey) => {
    if (sortKey === k) setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    else { setSortKey(k); setSortDir("asc"); }
  };

  const daySchedules = filteredSchedules.filter((s) => selectedDay && sameDay(new Date(s.scheduledAt), selectedDay));
  const sortedDay = [...daySchedules].sort((a, b) => {
    let cmp = 0;
    if (sortKey === "scheduledAt") cmp = new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime();
    else if (sortKey === "title") cmp = (a.title || "").localeCompare(b.title || "");
    else if (sortKey === "status") cmp = a.status.localeCompare(b.status);
    else if (sortKey === "brandName") cmp = (a.brandName || "").localeCompare(b.brandName || "");
    return sortDir === "asc" ? cmp : -cmp;
  });

  const days = getDayGrid(year, month);

  const getSchedulesForDay = (d: number) => {
    const date = new Date(year, month, d);
    return filteredSchedules.filter((s) => sameDay(new Date(s.scheduledAt), date));
  };

  const handleCreate = async () => {
    if (!form.contentId || !form.integrationId || !form.date || !form.time) return;
    setActionId("create");
    const scheduledAt = new Date(`${form.date}T${form.time}`).toISOString();
    const result = await createSchedule({ contentId: form.contentId, integrationId: form.integrationId, scheduledAt });
    if (result.data) {
      setSchedules((prev) => [result.data!, ...prev]);
      setToast("Schedule created");
      setShowCreate(false);
      setForm({ contentId: "", integrationId: "", date: "", time: "" });
    } else {
      setToast(result.error || "Failed to create schedule");
    }
    setActionId(null);
  };

  const handleDelete = async (id: string) => {
    setActionId(id);
    const item = schedules.find((s) => s.id === id);
    const result = await deleteSchedule(id);
    if (result.success) {
      setSchedules((prev) => prev.filter((s) => s.id !== id));
      setDeletedItem(item || null);
      setToast("Schedule deleted");
    } else {
      setToast(result.error || "Failed to delete schedule");
    }
    setActionId(null);
  };

  const handleUndoDelete = () => {
    if (!deletedItem) return;
    setSchedules((prev) => [deletedItem, ...prev]);
    setDeletedItem(null);
    setToast("Schedule restored");
  };

  const openEditModal = (s: ScheduleItem) => {
    const d = new Date(s.scheduledAt);
    setEditingSchedule(s);
    setEditForm({
      date: d.toISOString().slice(0, 10),
      time: d.toTimeString().slice(0, 5),
      integrationId: s.integrationId || "",
    });
  };

  const handleEditSave = async () => {
    if (!editingSchedule || !editForm.date || !editForm.time || !editForm.integrationId) return;
    setActionId("edit");
    const scheduledAt = new Date(`${editForm.date}T${editForm.time}`).toISOString();
    const result = await updateSchedule(editingSchedule.id, { integrationId: editForm.integrationId, scheduledAt });
    if (result.success) {
      setSchedules((prev) => prev.map((s) =>
        s.id === editingSchedule.id
          ? { ...s, scheduledAt, integrationId: editForm.integrationId }
          : s
      ));
      setToast("Schedule updated");
      setEditingSchedule(null);
    } else {
      setToast(result.error || "Failed to update schedule");
    }
    setActionId(null);
  };

  if (featureGate.isResolvingPlan) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Calendar" }]} />
        <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
          <div className="max-w-7xl mx-auto flex items-center justify-center min-h-[60vh]">
            <div className="text-center max-w-md">
              <div className="w-16 h-16 mx-auto mb-6 bg-primary/10 rounded-2xl flex items-center justify-center">
                <span className="material-symbols-outlined text-primary text-[32px] animate-spin">progress_activity</span>
              </div>
              <h2 className="text-headline-md text-on-surface font-bold mb-2">Checking subscription</h2>
              <p className="text-body-md text-on-surface-variant">Syncing your current workspace plan...</p>
            </div>
          </div>
        </main>
      </>
    );
  }

  if (!featureGate.canAccess("schedulePost")) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Calendar" }]} />
        <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
          <div className="max-w-7xl mx-auto flex items-center justify-center min-h-[60vh]">
            <div className="text-center max-w-md">
              <div className="w-16 h-16 mx-auto mb-6 bg-outline/10 rounded-2xl flex items-center justify-center">
                <span className="material-symbols-outlined text-outline text-[32px]">lock</span>
              </div>
              <h2 className="text-headline-md text-on-surface font-bold mb-2">Content Calendar</h2>
              <p className="text-body-md text-on-surface-variant mb-6">This feature requires a paid Plus plan or higher. Upgrade to schedule and manage your content calendar.</p>
              <Link href="/pricing" className="inline-flex items-center gap-2 px-6 py-3 bg-primary text-on-primary rounded-xl text-label-sm font-bold hover:scale-105 transition-all">
                View Plans
                <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
              </Link>
            </div>
          </div>
        </main>
      </>
    );
  }

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Calendar" },
      ]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-7xl mx-auto space-y-6">
          {/* ── Header ── */}
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div className="flex items-center gap-4">
              <span className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary/10 to-secondary/10 text-primary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">calendar_month</span>
              </span>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Content Calendar</h1>
                <p className="text-[11px] text-outline">{schedules.length} total · {pendingCount} pending</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <div className="flex items-center rounded-xl border border-outline-variant/20 p-0.5 bg-surface-container-low">
                {(["month", "week", "list"] as ViewMode[]).map((v) => (
                  <button key={v} onClick={() => setView(v)}
                    className={`px-3 py-1.5 rounded-lg text-label-xs font-semibold transition-all ${view === v ? "bg-surface-container-lowest text-on-surface shadow-sm" : "text-outline hover:text-on-surface"
                      }`}>
                    <span className="flex items-center gap-1.5">
                      <span className="material-symbols-outlined text-[14px]">{v === "month" ? "calendar_view_month" : v === "week" ? "view_week" : "list"}</span>
                      {v === "month" ? "Month" : v === "week" ? "Week" : "List"}
                    </span>
                  </button>
                ))}
              </div>
              <button onClick={() => setShowCreate(true)}
                className="px-4 py-2 rounded-xl bg-primary text-on-primary text-label-sm font-bold flex items-center gap-1.5 hover:bg-primary/90 transition-all shadow-sm">
                <span className="material-symbols-outlined text-[14px]">add</span>
                Schedule
              </button>
            </div>
          </div>

          {/* ── Workflow Summary ── */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="bg-surface-container-lowest rounded-xl border border-outline-variant/20 p-4 flex items-center gap-4 shadow-sm">
              <div className="w-11 h-11 rounded-full bg-warning-amber/10 flex items-center justify-center text-warning-amber">
                <span className="material-symbols-outlined">schedule</span>
              </div>
              <div>
                <p className="text-[11px] text-outline font-medium">Pending</p>
                <p className="text-headline-sm font-bold text-on-surface">{pendingCount}</p>
              </div>
            </div>
            <div className="bg-surface-container-lowest rounded-xl border border-outline-variant/20 p-4 flex items-center gap-4 shadow-sm">
              <div className="w-11 h-11 rounded-full bg-emerald-50 flex items-center justify-center text-emerald-600">
                <span className="material-symbols-outlined">check_circle</span>
              </div>
              <div>
                <p className="text-[11px] text-outline font-medium">Completed</p>
                <p className="text-headline-sm font-bold text-on-surface">{completedCount}</p>
              </div>
            </div>
            <div className="bg-surface-container-lowest rounded-xl border border-outline-variant/20 p-4 flex items-center gap-4 shadow-sm">
              <div className="w-11 h-11 rounded-full bg-danger-red/10 flex items-center justify-center text-danger-red">
                <span className="material-symbols-outlined">error</span>
              </div>
              <div>
                <p className="text-[11px] text-outline font-medium">Failed</p>
                <p className="text-headline-sm font-bold text-on-surface">{failedCount}</p>
              </div>
            </div>
            <div className="bg-surface-container-lowest rounded-xl border border-secondary/20 p-4 flex items-center gap-4 shadow-sm relative overflow-hidden" style={{ boxShadow: "0 0 15px rgba(115, 27, 229, 0.08)", borderColor: "rgba(115, 27, 229, 0.25)" }}>
              <span className="absolute top-2 right-2 px-1.5 py-0.5 bg-secondary text-on-secondary text-label-3xs font-bold rounded uppercase tracking-wider">Roadmap</span>
              <div className="w-11 h-11 rounded-full bg-secondary/10 flex items-center justify-center text-secondary">
                <span className="material-symbols-outlined">auto_awesome</span>
              </div>
              <div>
                <p className="text-[11px] text-outline font-medium">Advanced Recurring</p>
                <p className="text-label-sm font-semibold text-secondary">Coming Soon</p>
              </div>
            </div>
          </div>

          {/* ── Filters ── */}
          <div className="bg-surface-container-lowest rounded-xl border border-outline-variant/20 px-5 py-3 flex flex-wrap items-center gap-x-8 gap-y-3 shadow-sm">
            <div className="flex items-center gap-2">
              <span className="text-label-sm text-outline font-semibold">Brand:</span>
              <select value={filterBrand} onChange={(e) => setFilterBrand(e.target.value)}
                className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-1.5 text-[11px] text-on-surface focus:ring-2 focus:ring-primary/10 outline-none transition-all">
                <option value="">All Brands</option>
                {brands.map((b) => <option key={b} value={b}>{b}</option>)}
              </select>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-label-sm text-outline font-semibold">Platform:</span>
              <div className="flex gap-1">
                {Object.entries(PLATFORM_CONFIG).map(([key, cfg]) => (
                  <button key={key} onClick={() => setFilterPlatform(filterPlatform === key ? "" : key)}
                    className={`w-8 h-8 rounded-full flex items-center justify-center transition-all ${filterPlatform === key ? "bg-primary/10 text-primary" : "bg-surface-container-high text-outline hover:bg-surface-container"
                      }`}>
                    <PlatformIcon platform={cfg.icon} className="w-[14px] h-[14px]" />
                  </button>
                ))}
              </div>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-label-sm text-outline font-semibold">Status:</span>
              <select value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}
                className="bg-surface-container-low border border-outline-variant/20 rounded-lg px-3 py-1.5 text-[11px] text-on-surface focus:ring-2 focus:ring-primary/10 outline-none transition-all">
                <option value="">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="Processing">Processing</option>
                <option value="Completed">Completed</option>
                <option value="Failed">Failed</option>
              </select>
            </div>
            {(filterBrand || filterPlatform || filterStatus) && (
              <button onClick={() => { setFilterBrand(""); setFilterPlatform(""); setFilterStatus(""); }}
                className="text-label-sm text-primary font-semibold hover:underline flex items-center gap-1 ml-auto">
                <span className="material-symbols-outlined text-[14px]">filter_list</span>
                Clear All Filters
              </button>
            )}
          </div>

          {loading ? (
            <div className="flex items-center justify-center py-32">
              <span className="w-8 h-8 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
            </div>
          ) : view === "month" ? (
            <>
              <div className="flex gap-6">
                {/* ── Calendar Grid ── */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between mb-4">
                    <div className="flex items-center gap-2">
                      <button onClick={handlePrev} className="p-1.5 hover:bg-surface-container rounded-lg transition-all">
                        <span className="material-symbols-outlined text-[18px]">chevron_left</span>
                      </button>
                      <h2 className="text-headline-sm font-bold text-on-surface min-w-[180px] text-center">{MONTHS[month]} {year}</h2>
                      <button onClick={handleNext} className="p-1.5 hover:bg-surface-container rounded-lg transition-all">
                        <span className="material-symbols-outlined text-[18px]">chevron_right</span>
                      </button>
                    </div>
                    <button onClick={goToday}
                      className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-outline-variant/20 text-label-xs font-semibold text-on-surface-variant hover:bg-surface-container transition-all">
                      <span className="material-symbols-outlined text-[14px]">calendar_today</span>
                      Today
                    </button>
                  </div>
                  <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-xl overflow-hidden">
                    <div className="grid grid-cols-7 bg-surface-container-low">
                      {WEEKDAYS.map((d) => (
                        <div key={d} className="px-3 py-2.5 text-label-xs font-bold text-outline uppercase tracking-wider text-center">{d}</div>
                      ))}
                    </div>
                    <div className="grid grid-cols-7" style={{ gridAutoRows: "minmax(120px, auto)" }}>
                      {days.map((d, i) => {
                        const date = d ? new Date(year, month, d) : null;
                        const dayScheds = d ? getSchedulesForDay(d) : [];
                        const isToday = date && sameDay(date, today);
                        const isSelected = date && selectedDay && sameDay(date, selectedDay);
                        return (
                          <button key={i} onClick={() => d && setSelectedDay(new Date(year, month, d))}
                            className={`min-h-[120px] p-2 border-b border-r border-outline-variant/10 text-left transition-all hover:bg-surface-container-low/50 ${isSelected ? "bg-primary/5 ring-2 ring-inset ring-primary/20" : ""
                              } ${isToday && !isSelected ? "ring-2 ring-inset ring-primary/10" : ""} ${!d ? "bg-surface-container-low/30 opacity-50" : ""}`}>
                            {d && (
                              <>
                                <span className={`inline-flex items-center justify-center w-7 h-7 rounded-full text-[11px] font-semibold mb-1 ${isToday ? "bg-primary text-on-primary" : "text-on-surface"
                                  }`}>{d}</span>
                                {dayScheds.length > 0 && (
                                  <div className="space-y-1 mt-1">
                                    {dayScheds.slice(0, 3).map((s) => {
                                      const cfg = PLATFORM_CONFIG[s.platform || ""];
                                      return (
                                        <div key={s.id}
                                          className={`flex items-center gap-1 px-1.5 py-1 rounded text-label-2xs font-semibold truncate border ${s.status === "Completed" ? "bg-emerald-50/60 border-emerald-200/40 text-emerald-700" :
                                            s.status === "Failed" ? "bg-danger-red/5 border-danger-red/20 text-danger-red" :
                                              "bg-white border-outline-variant/30 text-on-surface shadow-sm"
                                            }`}>
                                          {cfg && <PlatformIcon platform={cfg.icon} className="w-[10px] h-[10px] shrink-0" />}
                                          <span className="truncate">{s.title}</span>
                                        </div>
                                      );
                                    })}
                                    {dayScheds.length > 3 && (
                                      <span className="text-label-3xs text-outline font-medium px-1 flex items-center gap-1">
                                        <span className="w-1 h-1 rounded-full bg-outline/40" />
                                        +{dayScheds.length - 3} more
                                      </span>
                                    )}
                                  </div>
                                )}
                              </>
                            )}
                          </button>
                        );
                      })}
                    </div>
                  </div>
                </div>

                {/* ── Day Detail Panel ── */}
                <div className="w-80 shrink-0">
                  <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-xl p-5 sticky top-0">
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="text-label-sm font-bold text-on-surface">
                        {selectedDay ? selectedDay.toLocaleDateString("en-GB", { weekday: "long", day: "numeric", month: "long" }) : "Select a day"}
                      </h3>
                      <span className="text-label-xs text-outline bg-surface-container-high px-2 py-0.5 rounded-full font-semibold">{daySchedules.length}</span>
                    </div>
                    {daySchedules.length === 0 ? (
                      <div className="flex flex-col items-center py-12 text-center">
                        <span className="material-symbols-outlined text-3xl text-outline/20 mb-2">event_busy</span>
                        <p className="text-[11px] text-outline">No schedules for this day</p>
                      </div>
                    ) : (
                      <div className="space-y-2">
                        {sortedDay.map((s) => {
                          const cfg = PLATFORM_CONFIG[s.platform || ""];
                          return (
                            <div key={s.id} className="group p-3 rounded-xl bg-surface-container-low border border-outline-variant/10 hover:border-outline-variant/30 hover:shadow-sm transition-all cursor-pointer"
                              onClick={() => openEditModal(s)}>
                              <div className="flex items-start gap-3">
                                <div className="flex flex-col items-center min-w-[36px] pt-0.5">
                                  <span className="text-[13px] font-bold text-on-surface leading-none">{new Date(s.scheduledAt).getDate()}</span>
                                  <span className="text-label-3xs text-outline uppercase">{MONTHS[new Date(s.scheduledAt).getMonth()].slice(0, 3)}</span>
                                </div>
                                <div className="flex-1 min-w-0">
                                  <div className="flex items-center gap-1.5 mb-1">
                                    {s.type && (
                                      <div className={`w-5 h-5 rounded bg-gradient-to-br ${getTypeStyle(s.type as ContentType)} flex items-center justify-center text-white shrink-0`}>
                                        <span className="material-symbols-outlined text-label-2xs">{getTypeConfig(s.type as ContentType).icon}</span>
                                      </div>
                                    )}
                                    <p className="text-[11px] font-semibold text-on-surface truncate">{s.title}</p>
                                  </div>
                                  <div className="flex items-center gap-2">
                                    <span className={`inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-label-3xs font-bold border ${STATUS_STYLE[s.status] || ""}`}>
                                      <span className={`w-1 h-1 rounded-full ${STATUS_DOT[s.status] || "bg-outline"}`} />
                                      {s.status}
                                    </span>
                                    {cfg && (
                                      <span className="flex items-center gap-0.5 text-label-3xs font-semibold" style={{ color: cfg.color }}>
                                        <PlatformIcon platform={cfg.icon} className="w-[8px] h-[8px]" />
                                        {cfg.label}
                                      </span>
                                    )}
                                  </div>
                                  <div className="flex items-center gap-2 mt-1">
                                    <span className="text-label-2xs text-outline">
                                      <span className="material-symbols-outlined text-label-2xs align-text-bottom">schedule</span>
                                      {formatTime(s.scheduledAt)}
                                    </span>
                                    {s.brandName && (
                                      <>
                                        <span className="text-outline/20">·</span>
                                        <span className="text-label-2xs text-outline">{s.brandName}</span>
                                      </>
                                    )}
                                  </div>
                                </div>
                                <button onClick={(e) => { e.stopPropagation(); handleDelete(s.id); }} disabled={actionId === s.id}
                                  className="p-1 opacity-0 group-hover:opacity-100 text-outline/40 hover:text-danger-red transition-all disabled:opacity-20">
                                  {actionId === s.id ? (
                                    <span className="w-3 h-3 border-2 border-danger-red/30 border-t-danger-red rounded-full animate-spin block" />
                                  ) : (
                                    <span className="material-symbols-outlined text-[14px]">close</span>
                                  )}
                                </button>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                </div>
              </div>
              {/* ── Legend ── */}
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                  {[
                    { color: "bg-emerald-500", label: "Completed" },
                    { color: "bg-warning-amber", label: "Pending" },
                    { color: "bg-sky-500", label: "Processing" },
                    { color: "bg-danger-red", label: "Failed" },
                  ].map((item) => (
                    <div key={item.label} className="flex items-center gap-1.5">
                      <span className={`w-2.5 h-2.5 rounded-full ${item.color}`} />
                      <span className="text-label-xs text-outline font-medium">{item.label}</span>
                    </div>
                  ))}
                </div>
                <p className="text-label-xs text-outline italic">API: /api/content-schedules</p>
              </div>
            </>
          ) : view === "week" ? (
            /* ── Week View ── */
            <div>
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-2">
                  <button onClick={() => setWeekOffset((o) => o - 1)} className="p-1.5 hover:bg-surface-container rounded-lg transition-all">
                    <span className="material-symbols-outlined text-[18px]">chevron_left</span>
                  </button>
                  <h2 className="text-headline-sm font-bold text-on-surface min-w-[240px] text-center">{formatWeekRange(weekOffset)}</h2>
                  <button onClick={() => setWeekOffset((o) => o + 1)} className="p-1.5 hover:bg-surface-container rounded-lg transition-all">
                    <span className="material-symbols-outlined text-[18px]">chevron_right</span>
                  </button>
                </div>
                <button onClick={() => { const d = new Date(); setYear(d.getFullYear()); setMonth(d.getMonth()); setWeekOffset(0); }}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-outline-variant/20 text-label-xs font-semibold text-on-surface-variant hover:bg-surface-container transition-all">
                  <span className="material-symbols-outlined text-[14px]">calendar_today</span>
                  Today
                </button>
              </div>
              <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-xl overflow-hidden">
                <div className="grid grid-cols-7 bg-surface-container-low border-b border-outline-variant/20">
                  {WEEKDAYS.map((d, i) => {
                    const weekDays = getWeekDays(weekOffset);
                    const isToday = sameDay(weekDays[i], today);
                    return (
                      <div key={d} className={`px-3 py-2.5 text-center ${isToday ? "text-primary" : "text-outline"}`}>
                        <div className="text-label-2xs font-bold uppercase tracking-wider">{d}</div>
                        <div className={`inline-flex items-center justify-center w-7 h-7 rounded-full text-[11px] font-bold mt-0.5 ${isToday ? "bg-primary text-on-primary" : "text-on-surface"
                          }`}>{weekDays[i].getDate()}</div>
                      </div>
                    );
                  })}
                </div>
                <div className="grid grid-cols-7 divide-x divide-outline-variant/10" style={{ gridAutoRows: "minmax(250px, auto)" }}>
                  {getWeekDays(weekOffset).map((dayDate, i) => {
                    const dayScheds = filteredSchedules.filter((s) => sameDay(new Date(s.scheduledAt), dayDate));
                    const isToday = sameDay(dayDate, today);
                    return (
                      <div key={i} className={`p-2 ${isToday ? "bg-primary/5" : ""} ${!dayScheds.length ? "bg-surface-container-low/20" : ""}`}>
                        {dayScheds.length === 0 ? (
                          <div className="flex flex-col items-center justify-center h-full py-8 text-center">
                            <span className="material-symbols-outlined text-lg text-outline/20">event_busy</span>
                            <p className="text-label-2xs text-outline/40 mt-1">No schedules</p>
                          </div>
                        ) : (
                          <div className="space-y-1.5">
                            {dayScheds.map((s) => {
                              const cfg = PLATFORM_CONFIG[s.platform || ""];
                              return (
                                <div key={s.id} onClick={() => openEditModal(s)}
                                  className="group p-2 rounded-lg border bg-white hover:shadow-sm transition-all cursor-pointer"
                                  style={{ borderColor: cfg?.color + "30" }}>
                                  <div className="flex items-center gap-1.5 mb-1">
                                    {cfg && <PlatformIcon platform={cfg.icon} className="w-[10px] h-[10px]" />}
                                    <span className="text-label-2xs font-semibold truncate flex-1">{s.title}</span>
                                    <span className={`w-1.5 h-1.5 rounded-full shrink-0 ${STATUS_DOT[s.status] || "bg-outline"}`} />
                                  </div>
                                  <div className="flex items-center gap-2">
                                    <span className="text-label-3xs text-outline flex items-center gap-0.5">
                                      <span className="material-symbols-outlined text-label-3xs">schedule</span>
                                      {formatTime(s.scheduledAt)}
                                    </span>
                                    {s.brandName && (
                                      <>
                                        <span className="text-outline/20">·</span>
                                        <span className="text-label-3xs text-outline truncate">{s.brandName}</span>
                                      </>
                                    )}
                                  </div>
                                </div>
                              );
                            })}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          ) : (
            /* ── List View ── */
            <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-xl overflow-hidden shadow-sm">
              <div className="px-6 py-4 border-b border-outline-variant/20 flex items-center justify-between">
                <span className="text-label-sm text-outline font-semibold">
                  {filteredSchedules.length} of {schedules.length} schedules
                </span>
                <div className="flex items-center gap-3">
                  {[
                    { color: "bg-emerald-500", label: "Completed" },
                    { color: "bg-warning-amber", label: "Pending" },
                    { color: "bg-sky-500", label: "Processing" },
                    { color: "bg-danger-red", label: "Failed" },
                  ].map((item) => (
                    <div key={item.label} className="flex items-center gap-1">
                      <span className={`w-2 h-2 rounded-full ${item.color}`} />
                      <span className="text-label-2xs text-outline">{item.label}</span>
                    </div>
                  ))}
                </div>
              </div>
              {filteredSchedules.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-16 text-center">
                  <span className="material-symbols-outlined text-4xl text-outline/20 mb-3">calendar_month</span>
                  <p className="text-body-sm text-outline font-medium">No schedules match your filters</p>
                  <button onClick={() => { setFilterBrand(""); setFilterPlatform(""); setFilterStatus(""); }}
                    className="mt-2 text-label-sm text-primary font-semibold hover:underline">Clear all filters</button>
                </div>
              ) : (
                <table className="w-full text-left border-collapse">
                  <thead className="bg-surface-container-low">
                    <tr>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider cursor-pointer select-none hover:text-on-surface"
                        onClick={() => handleSort("scheduledAt")}><span className="flex items-center gap-0.5">Date{renderSortIcon(sortKey, sortDir, "scheduledAt")}</span></th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider cursor-pointer select-none hover:text-on-surface"
                        onClick={() => handleSort("title")}><span className="flex items-center gap-0.5">Content{renderSortIcon(sortKey, sortDir, "title")}</span></th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider cursor-pointer select-none hover:text-on-surface"
                        onClick={() => handleSort("brandName")}><span className="flex items-center gap-0.5">Brand{renderSortIcon(sortKey, sortDir, "brandName")}</span></th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider">Platform</th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider cursor-pointer select-none hover:text-on-surface"
                        onClick={() => handleSort("status")}><span className="flex items-center gap-0.5">Status{renderSortIcon(sortKey, sortDir, "status")}</span></th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider">Attempts</th>
                      <th className="px-6 py-4 text-label-sm text-outline font-semibold uppercase tracking-wider text-right">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-outline-variant/20">
                    {[...filteredSchedules].sort((a, b) => {
                      let cmp = 0;
                      if (sortKey === "scheduledAt") cmp = new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime();
                      else if (sortKey === "title") cmp = (a.title || "").localeCompare(b.title || "");
                      else if (sortKey === "status") cmp = a.status.localeCompare(b.status);
                      else if (sortKey === "brandName") cmp = (a.brandName || "").localeCompare(b.brandName || "");
                      return sortDir === "asc" ? cmp : -cmp;
                    }).map((s) => {
                      const cfg = PLATFORM_CONFIG[s.platform || ""];
                      return (
                        <tr key={s.id} className="hover:bg-surface-container-low/60 transition-colors group">
                          <td className="px-6 py-4">
                            <div className="flex flex-col">
                              <span className="text-body-sm font-semibold text-on-surface">{formatDate(s.scheduledAt)}</span>
                              <span className="text-label-xs text-outline">{formatTime(s.scheduledAt)}</span>
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex items-center gap-3">
                              {s.type && (
                                <div className={`w-10 h-8 rounded-lg bg-gradient-to-br ${getTypeStyle(s.type as ContentType)} flex items-center justify-center text-white shrink-0`}>
                                  <span className="material-symbols-outlined text-[14px]">{getTypeConfig(s.type as ContentType).icon}</span>
                                </div>
                              )}
                              <p className="text-body-sm font-semibold text-on-surface">{s.title || "Untitled"}</p>
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex items-center gap-2">
                              <span className="w-2 h-2 rounded-full shrink-0" style={{ backgroundColor: getBrandColor(s.brandName || "") }} />
                              <span className="text-body-sm text-on-surface">{s.brandName || "—"}</span>
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            {cfg ? (
                              <span className="flex items-center gap-1 px-2 py-0.5 rounded text-label-xs font-semibold"
                                style={{ backgroundColor: cfg.color + "12", color: cfg.color }}>
                                <PlatformIcon platform={cfg.icon} className="w-[10px] h-[10px]" />
                                {cfg.label}
                              </span>
                            ) : <span className="text-label-xs text-outline">—</span>}
                          </td>
                          <td className="px-6 py-4">
                            <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-xs font-bold border ${STATUS_STYLE[s.status] || ""}`}>
                              <span className={`w-1.5 h-1.5 rounded-full ${STATUS_DOT[s.status] || "bg-outline"}`} />
                              {s.status}
                            </span>
                          </td>
                          <td className="px-6 py-4">
                            <span className="text-[11px] text-outline">{s.attemptCount}</span>
                            {s.lastError && <span className="text-label-2xs text-danger-red block">{s.lastError}</span>}
                          </td>
                          <td className="px-6 py-4 text-right">
                            <button onClick={() => handleDelete(s.id)} disabled={actionId === s.id}
                              className="p-2 text-outline/40 hover:text-danger-red hover:bg-danger-red/10 rounded-lg transition-all disabled:opacity-40">
                              {actionId === s.id ? (
                                <span className="w-3.5 h-3.5 border-2 border-danger-red/30 border-t-danger-red rounded-full animate-spin block" />
                              ) : (
                                <span className="material-symbols-outlined text-[17px]">delete</span>
                              )}
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              )}
            </div>
          )}
        </div>

        {/* ── Create Schedule Modal ── */}
        {showCreate && (
          <>
            <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={() => setShowCreate(false)} />
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={() => setShowCreate(false)}>
              <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl p-6 animate-in fade-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
                    <span className="material-symbols-outlined text-[20px]">post_add</span>
                  </div>
                  <div>
                    <h3 className="text-label-sm font-bold text-on-surface">Schedule Content</h3>
                    <p className="text-label-xs text-outline">Pick content, platform, and time</p>
                  </div>
                </div>
                <div className="space-y-4">
                  <div>
                    <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Content</label>
                    <select value={form.contentId} onChange={(e) => setForm((f) => ({ ...f, contentId: e.target.value }))}
                      className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-2.5 text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all">
                      <option value="">Select content...</option>
                      {contents.filter((c) => c.status !== "Published").map((c) => (
                        <option key={c.id} value={c.id}>{c.title} ({c.brandName})</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Social Account</label>
                    <select value={form.integrationId} onChange={(e) => setForm((f) => ({ ...f, integrationId: e.target.value }))}
                      className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-2.5 text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all">
                      <option value="">Select social account...</option>
                      {integrations.filter((i) => i.isActive && (!form.contentId || i.brandId === contents.find(c => c.id === form.contentId)?.brandId)).map((i) => (
                        <option key={i.id} value={i.id}>{i.accountName} - {i.targetName} ({i.provider})</option>
                      ))}
                    </select>
                    {integrations.length === 0 && (
                      <p className="text-label-xs text-outline mt-2">No social accounts connected. Please connect accounts in Social page.</p>
                    )}
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Date</label>
                      <input type="date" value={form.date} onChange={(e) => setForm((f) => ({ ...f, date: e.target.value }))}
                        className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-2.5 text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all" />
                    </div>
                    <div>
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Time</label>
                      <input type="time" value={form.time} onChange={(e) => setForm((f) => ({ ...f, time: e.target.value }))}
                        className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-2.5 text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all" />
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-3 mt-6">
                  <button onClick={() => setShowCreate(false)}
                    className="flex-1 py-2.5 rounded-xl border border-outline-variant/20 text-label-sm font-semibold text-outline hover:text-on-surface transition-all">Cancel</button>
                  <button onClick={handleCreate} disabled={!form.contentId || !form.integrationId || !form.date || !form.time || actionId === "create"}
                    className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold flex items-center justify-center gap-2 hover:bg-primary/90 transition-all disabled:opacity-50 shadow-sm">
                    {actionId === "create" ? (
                      <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    ) : (
                      <span className="material-symbols-outlined text-[16px]">add</span>
                    )}
                    Create Schedule
                  </button>
                </div>
              </div>
            </div>
          </>
        )}

        {/* ── Edit Schedule Modal ── */}
        {editingSchedule && (
          <>
            <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={() => setEditingSchedule(null)} />
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={() => setEditingSchedule(null)}>
              <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl p-6 animate-in fade-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
                    <span className="material-symbols-outlined text-[20px]">edit</span>
                  </div>
                  <div>
                    <h3 className="text-label-sm font-bold text-on-surface">Edit Schedule</h3>
                    <p className="text-label-xs text-outline">{editingSchedule.title || "Untitled"}</p>
                  </div>
                </div>
                <div className="space-y-4">
                  <div>
                    <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Social Account</label>
                    <select value={editForm.integrationId} onChange={(e) => setEditForm((f) => ({ ...f, integrationId: e.target.value }))}
                      className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-2.5 text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all">
                      <option value="">Select social account...</option>
                      {integrations.filter((i) => i.isActive).map((i) => (
                        <option key={i.id} value={i.id}>{i.accountName} - {i.targetName} ({i.provider})</option>
                      ))}
                    </select>
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Date</label>
                      <input type="date" value={editForm.date} onChange={(e) => setEditForm((f) => ({ ...f, date: e.target.value }))}
                        className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-2.5 text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all" />
                    </div>
                    <div>
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Time</label>
                      <input type="time" value={editForm.time} onChange={(e) => setEditForm((f) => ({ ...f, time: e.target.value }))}
                        className="w-full bg-surface-container-low border border-outline-variant/20 rounded-lg px-4 py-2.5 text-body-sm text-on-surface focus:ring-2 focus:ring-primary/10 focus:border-primary/40 outline-none transition-all" />
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-3 mt-6">
                  <button onClick={() => setEditingSchedule(null)}
                    className="flex-1 py-2.5 rounded-xl border border-outline-variant/20 text-label-sm font-semibold text-outline hover:text-on-surface transition-all">Cancel</button>
                  <button onClick={handleEditSave} disabled={!editForm.integrationId || !editForm.date || !editForm.time || actionId === "edit"}
                    className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold flex items-center justify-center gap-2 hover:bg-primary/90 transition-all disabled:opacity-50 shadow-sm">
                    {actionId === "edit" ? (
                      <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    ) : (
                      <span className="material-symbols-outlined text-[16px]">save</span>
                    )}
                    Save Changes
                  </button>
                </div>
              </div>
            </div>
          </>
        )}

        {/* ── Toast ── */}
        {toast && (
          <div className="fixed bottom-8 left-1/2 -translate-x-1/2 z-[100] flex items-center gap-3 bg-inverse-surface text-inverse-on-surface px-5 py-3 rounded-xl shadow-2xl animate-in fade-in slide-in-from-bottom-2 duration-300">
            <span className="material-symbols-outlined text-[16px]">{toast.includes("deleted") ? "delete" : "check"}</span>
            <span className="text-label-sm font-semibold">{toast}</span>
            {toast === "Schedule deleted" && deletedItem ? (
              <button onClick={handleUndoDelete}
                className="ml-2 px-3 py-1 rounded-lg bg-white/10 text-label-sm font-bold hover:bg-white/20 transition-all">
                Undo
              </button>
            ) : (
              <button onClick={() => setToast(null)} className="p-0.5 hover:bg-white/10 rounded-full">
                <span className="material-symbols-outlined text-[12px]">close</span>
              </button>
            )}
          </div>
        )}
      </main>
    </>
  );
}

export default function CalendarPage() {
  return (
    <Suspense fallback={null}>
      <CalendarContent />
    </Suspense>
  );
}
