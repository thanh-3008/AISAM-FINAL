"use client";

import { useState } from "react";
import Header from "@/components/layout/Header";

type CalendarEvent = {
  day: number;
  title: string;
  time?: string;
  platform?: "Facebook" | "Instagram" | "LinkedIn";
  status: "scheduled" | "published" | "failed" | "draft";
  type?: "text" | "image";
};

const stats = [
  { label: "Scheduled", value: "24", icon: "schedule", tone: "bg-primary/10 text-primary" },
  { label: "Published", value: "158", icon: "check_circle", tone: "bg-success-green/10 text-success-green" },
  { label: "Failed", value: "2", icon: "error", tone: "bg-danger-red/10 text-danger-red" },
];

const events: CalendarEvent[] = [
  { day: 2, title: "Product Launch Visuals", platform: "Instagram", status: "scheduled", type: "text" },
  { day: 6, title: "Q4 AI Strategy Post", time: "10:00 AM", platform: "Facebook", status: "published", type: "text" },
  { day: 9, title: "Visualizing Intelligence", platform: "Instagram", status: "scheduled", type: "image" },
  { day: 12, title: "Generated Campaign Draft", platform: "LinkedIn", status: "draft", type: "text" },
  { day: 16, title: "Creator Partnership Teaser", time: "2:30 PM", platform: "Instagram", status: "scheduled", type: "text" },
  { day: 19, title: "Audience Insight Carousel", platform: "Facebook", status: "scheduled", type: "image" },
  { day: 23, title: "Retargeting Copy Review", platform: "Facebook", status: "failed", type: "text" },
  { day: 26, title: "Weekend Growth Recap", time: "8:00 PM", platform: "LinkedIn", status: "scheduled", type: "text" },
];

const days = [
  { label: "SUN", short: "S" },
  { label: "MON", short: "M" },
  { label: "TUE", short: "T" },
  { label: "WED", short: "W" },
  { label: "THU", short: "T" },
  { label: "FRI", short: "F" },
  { label: "SAT", short: "S" },
];

const calendarCells = [
  { day: 31, muted: true },
  ...Array.from({ length: 30 }, (_, index) => ({ day: index + 1, muted: false })),
  ...Array.from({ length: 4 }, (_, index) => ({ day: index + 1, muted: true })),
];

const platformIcon: Record<NonNullable<CalendarEvent["platform"]>, string> = {
  Facebook: "social_leaderboard",
  Instagram: "photo_camera",
  LinkedIn: "work",
};

const statusStyles: Record<CalendarEvent["status"], string> = {
  scheduled: "bg-primary/8 border-primary/20 text-primary",
  published: "bg-success-green/10 border-success-green/20 text-success-green",
  failed: "bg-danger-red/8 border-danger-red/20 text-danger-red",
  draft: "bg-secondary/8 border-secondary/20 text-secondary",
};

const postPlatforms = [
  { label: "Facebook", icon: "social_leaderboard" },
  { label: "Instagram", icon: "photo_camera" },
  { label: "TikTok", icon: "movie" },
  { label: "LinkedIn", icon: "work" },
];

function EventChip({ event }: { event: CalendarEvent }) {
  if (event.type === "image") {
    return (
      <button className="mt-2 w-full overflow-hidden rounded-lg border border-outline-variant/40 bg-surface-container-lowest text-left shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md">
        <div className="h-12 w-full bg-[url('https://images.unsplash.com/photo-1558655146-9f40138edfeb?auto=format&fit=crop&w=600&q=80')] bg-cover bg-center" />
        <div className="px-2 py-1.5">
          <p className="truncate text-[11px] font-semibold text-on-surface">{event.title}</p>
        </div>
      </button>
    );
  }

  return (
    <button className={`mt-2 w-full rounded-lg border px-2 py-2 text-left transition-all hover:-translate-y-0.5 hover:shadow-sm ${statusStyles[event.status]}`}>
      <div className="mb-1 flex items-center gap-1.5">
        <span className="material-symbols-outlined text-[14px]">
          {event.status === "draft" ? "auto_awesome" : event.status === "failed" ? "error" : platformIcon[event.platform || "Facebook"]}
        </span>
        <span className="truncate text-[10px] font-bold uppercase tracking-wide">
          {event.status === "draft" ? "AI Draft" : event.status === "failed" ? "Failed" : event.platform}
        </span>
      </div>
      <p className="truncate text-[11px] font-medium text-on-surface">{event.time ? `${event.time} - ` : ""}{event.title}</p>
    </button>
  );
}

function NewPostModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [selectedPlatforms, setSelectedPlatforms] = useState(["Facebook"]);
  const [scheduleMode, setScheduleMode] = useState<"now" | "schedule">("now");

  if (!open) return null;

  const togglePlatform = (label: string) => {
    setSelectedPlatforms((current) => {
      if (current.includes(label)) {
        return current.length === 1 ? current : current.filter((item) => item !== label);
      }
      return [...current, label];
    });
  };

  return (
    <div className="fixed inset-0 z-[80] flex items-center justify-center p-4 sm:p-6">
      <button
        aria-label="Close new post modal"
        className="absolute inset-0 bg-inverse-surface/60 backdrop-blur-sm"
        onClick={onClose}
      />

      <section className="relative flex max-h-[90vh] w-full max-w-5xl overflow-hidden rounded-2xl bg-surface-container-lowest shadow-2xl md:flex-row">
        <div className="flex min-w-0 flex-1 flex-col border-outline-variant/30 md:border-r">
          <div className="flex items-center justify-between gap-4 border-b border-outline-variant/30 px-6 py-5">
            <div>
              <h2 className="text-headline-md text-on-surface">Create New Post</h2>
              <p className="mt-1 text-body-sm text-on-surface-variant">Compose content and choose how it should be published.</p>
            </div>
            <button className="flex h-9 w-9 items-center justify-center rounded-full transition-colors hover:bg-surface-container" onClick={onClose} title="Close">
              <span className="material-symbols-outlined text-[20px]">close</span>
            </button>
          </div>

          <div className="flex-1 overflow-y-auto px-6 py-6">
            <div className="space-y-6">
              <section>
                <label className="mb-3 block text-label-md uppercase tracking-wider text-outline">Select Platforms</label>
                <div className="flex flex-wrap gap-2">
                  {postPlatforms.map((platform) => (
                    (() => {
                      const selected = selectedPlatforms.includes(platform.label);
                      return (
                    <button
                      key={platform.label}
                      type="button"
                      onClick={() => togglePlatform(platform.label)}
                      className={`inline-flex items-center gap-2 rounded-xl border px-4 py-2 text-label-md transition-all active:scale-[0.97] ${
                        selected
                          ? "border-primary bg-primary/5 text-primary shadow-sm"
                          : "border-outline-variant/60 text-on-surface hover:border-primary hover:bg-primary/5 hover:text-primary"
                      }`}
                    >
                      <span className="material-symbols-outlined text-[20px]">{platform.icon}</span>
                      {platform.label}
                      {selected && <span className="material-symbols-outlined text-[16px]">check_circle</span>}
                    </button>
                      );
                    })()
                  ))}
                </div>
              </section>

              <section>
                <label className="mb-3 block text-label-md uppercase tracking-wider text-outline">Post Content</label>
                <textarea
                  className="min-h-32 w-full resize-none rounded-2xl border border-outline-variant/60 bg-surface-container-low p-4 text-body-md text-on-surface outline-none transition-all placeholder:text-outline/50 focus:border-primary focus:ring-2 focus:ring-primary/10"
                  placeholder="What's on your mind?"
                />
                <button className="mt-3 flex w-full cursor-pointer flex-col items-center justify-center gap-2 rounded-2xl border-2 border-dashed border-outline-variant/70 bg-surface-container-lowest p-8 text-center transition-colors hover:border-primary/50 hover:bg-surface-container">
                  <span className="material-symbols-outlined text-[32px] text-primary">cloud_upload</span>
                  <p className="text-label-md text-on-surface">
                    Drag and drop media or <span className="text-primary">browse</span>
                  </p>
                  <p className="text-[10px] uppercase tracking-wide text-outline">Supports JPG, PNG, MP4 (Max 50MB)</p>
                </button>
              </section>

              <section>
                <label className="mb-3 block text-label-md uppercase tracking-wider text-outline">Scheduling</label>
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  <button
                    type="button"
                    onClick={() => setScheduleMode("now")}
                    className={`flex items-center gap-3 rounded-2xl border p-4 text-left transition-all active:scale-[0.98] ${
                      scheduleMode === "now"
                        ? "border-primary bg-primary/5"
                        : "border-outline-variant/70 hover:border-primary hover:bg-primary/5"
                    }`}
                  >
                    <span className={`material-symbols-outlined ${scheduleMode === "now" ? "text-primary" : "text-on-surface-variant"}`}>bolt</span>
                    <span>
                      <span className="block text-label-md text-on-surface">Post Now</span>
                      <span className="block text-[10px] text-outline">Publish immediately</span>
                    </span>
                  </button>
                  <button
                    type="button"
                    onClick={() => setScheduleMode("schedule")}
                    className={`flex items-center gap-3 rounded-2xl border p-4 text-left transition-all active:scale-[0.98] ${
                      scheduleMode === "schedule"
                        ? "border-primary bg-primary/5"
                        : "border-outline-variant/70 hover:border-primary hover:bg-primary/5"
                    }`}
                  >
                    <span className={`material-symbols-outlined ${scheduleMode === "schedule" ? "text-primary" : "text-on-surface-variant"}`}>calendar_today</span>
                    <span>
                      <span className="block text-label-md text-on-surface">Schedule</span>
                      <span className="block text-[10px] text-outline">Pick date and time</span>
                    </span>
                  </button>
                </div>
                {scheduleMode === "schedule" && (
                  <div className="mt-4 grid grid-cols-1 gap-4 rounded-2xl border border-primary/20 bg-primary/5 p-4 sm:grid-cols-2">
                    <label className="block">
                      <span className="mb-2 block text-label-md uppercase tracking-wider text-outline">Date</span>
                      <input
                        type="date"
                        className="w-full rounded-xl border border-outline-variant/60 bg-surface-container-lowest px-4 py-3 text-body-sm text-on-surface outline-none transition-all focus:border-primary focus:ring-2 focus:ring-primary/10"
                      />
                    </label>
                    <label className="block">
                      <span className="mb-2 block text-label-md uppercase tracking-wider text-outline">Time</span>
                      <input
                        type="time"
                        className="w-full rounded-xl border border-outline-variant/60 bg-surface-container-lowest px-4 py-3 text-body-sm text-on-surface outline-none transition-all focus:border-primary focus:ring-2 focus:ring-primary/10"
                      />
                    </label>
                  </div>
                )}
              </section>
            </div>
          </div>

          <div className="flex flex-col-reverse gap-3 border-t border-outline-variant/30 px-6 py-5 sm:flex-row sm:items-center sm:justify-end">
            <button className="rounded-xl px-6 py-2.5 text-label-md text-on-surface-variant transition-colors hover:bg-surface-container">
              Save as Draft
            </button>
            <button className="rounded-xl bg-primary px-8 py-2.5 text-label-md text-on-primary shadow-lg shadow-primary/20 transition-all hover:opacity-90 active:scale-[0.97]">
              Schedule Post
            </button>
          </div>
        </div>

        <aside className="hidden w-[360px] shrink-0 flex-col items-center justify-center gap-6 bg-surface-container p-6 md:flex">
          <p className="text-label-md uppercase tracking-widest text-outline">Live Preview</p>
          <div className="relative aspect-[9/16] w-full overflow-hidden rounded-[2rem] border-[8px] border-enterprise-navy bg-white shadow-2xl">
            <div className="absolute left-0 top-0 flex h-6 w-full items-end justify-center bg-enterprise-navy pb-1">
              <div className="h-1 w-16 rounded-full bg-white/20" />
            </div>
            <div className="p-4 pt-9">
              <div className="mb-4 flex items-center gap-2">
                <div className="h-8 w-8 rounded-full bg-surface-dim" />
                <div className="space-y-1">
                  <div className="h-2 w-20 rounded bg-surface-dim" />
                  <div className="h-1.5 w-12 rounded bg-surface-dim/50" />
                </div>
              </div>
              <div className="mb-3 aspect-square w-full rounded-xl bg-gradient-to-br from-primary/20 via-secondary/10 to-surface-dim" />
              <div className="space-y-2">
                <div className="h-2 w-full rounded bg-surface-dim" />
                <div className="h-2 w-3/4 rounded bg-surface-dim" />
              </div>
              <div className="mt-5 flex items-center gap-4 text-outline">
                <span className="material-symbols-outlined text-[18px]">favorite</span>
                <span className="material-symbols-outlined text-[18px]">chat_bubble</span>
                <span className="material-symbols-outlined text-[18px]">send</span>
              </div>
            </div>
          </div>
          <div className="flex gap-4">
            <span className="material-symbols-outlined text-outline">smartphone</span>
            <span className="material-symbols-outlined text-outline/30">laptop</span>
            <span className="material-symbols-outlined text-outline/30">tablet</span>
          </div>
        </aside>
      </section>
    </div>
  );
}

export default function CalendarPage() {
  const [newPostOpen, setNewPostOpen] = useState(false);

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Publishing Calendar" }]} />
      <main className="p-8 h-[calc(100vh-64px)] overflow-y-auto space-y-8">
        <section className="flex flex-col gap-5 xl:flex-row xl:items-end xl:justify-between">
          <div>
            <div className="mb-2 flex items-center gap-2 text-label-md text-outline">
              <span>Content Workspace</span>
              <span className="material-symbols-outlined text-[14px]">chevron_right</span>
              <span className="font-bold text-primary">Publishing Calendar</span>
            </div>
            <h1 className="text-headline-lg text-on-surface tracking-tight">Publishing Calendar</h1>
            <p className="mt-2 text-body-md text-on-surface-variant">Plan scheduled posts, monitor failures, and review AI-generated campaign drafts.</p>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            <div className="flex rounded-xl bg-surface-container-high p-1">
              {["Month", "Week", "Day"].map((view) => (
                <button
                  key={view}
                  className={`rounded-lg px-4 py-2 text-label-md transition-all ${
                    view === "Month"
                      ? "bg-surface-container-lowest text-primary shadow-sm"
                      : "text-on-surface-variant hover:bg-surface-container-lowest/60"
                  }`}
                >
                  {view}
                </button>
              ))}
            </div>
            <button className="inline-flex items-center gap-2 rounded-xl border border-outline-variant/60 px-4 py-2 text-label-md text-on-surface transition-colors hover:bg-surface-container">
              <span className="material-symbols-outlined text-[18px]">calendar_today</span>
              Today
            </button>
            <button
              className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2 text-label-md text-on-primary shadow-md shadow-primary/20 transition-all hover:opacity-90 active:scale-[0.97]"
              onClick={() => setNewPostOpen(true)}
            >
              <span className="material-symbols-outlined text-[18px]">add</span>
              Schedule New Post
            </button>
          </div>
        </section>

        <section className="grid grid-cols-1 gap-gutter md:grid-cols-2 xl:grid-cols-4">
          {stats.map((item) => (
            <div key={item.label} className="rounded-2xl border border-outline-variant/30 bg-surface-container-lowest p-5 shadow-sm">
              <div className="flex items-center gap-4">
                <div className={`flex h-12 w-12 items-center justify-center rounded-2xl ${item.tone}`}>
                  <span className="material-symbols-outlined">{item.icon}</span>
                </div>
                <div>
                  <p className="text-body-sm text-outline">{item.label}</p>
                  <p className="text-headline-md text-on-surface">{item.value}</p>
                </div>
              </div>
            </div>
          ))}
          <div className="relative overflow-hidden rounded-2xl border border-secondary/20 bg-secondary/8 p-5 shadow-sm shadow-secondary/5">
            <div className="absolute right-3 top-3 rounded-full bg-secondary px-2 py-0.5 text-[10px] font-bold uppercase tracking-wide text-white">Roadmap</div>
            <div className="flex items-center gap-3">
              <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-secondary/10 text-secondary">
                <span className="material-symbols-outlined">auto_awesome</span>
              </div>
              <div>
                <p className="text-label-md text-secondary">Advanced Recurring</p>
                <p className="text-body-sm text-outline">Coming Soon</p>
              </div>
            </div>
          </div>
        </section>

        <section>
          <div className="rounded-t-2xl border-x border-t border-outline-variant/40 bg-surface-container-lowest p-4">
            <div className="flex flex-wrap items-center gap-5">
              <label className="flex items-center gap-2">
                <span className="text-label-md text-outline">Brand:</span>
                <select className="rounded-xl border border-outline-variant/30 bg-surface-container-low px-3 py-2 text-body-sm outline-none focus:border-primary">
                  <option>AISAM Global</option>
                  <option>TechVision Pro</option>
                  <option>Lumina Beauty</option>
                </select>
              </label>
              <div className="flex items-center gap-2">
                <span className="text-label-md text-outline">Platform:</span>
                {["social_leaderboard", "photo_camera", "work"].map((icon, index) => (
                  <button
                    key={icon}
                    className={`flex h-9 w-9 items-center justify-center rounded-full transition-colors ${
                      index === 1 ? "bg-primary/10 text-primary" : "bg-surface-container-high text-on-surface-variant hover:bg-primary/10 hover:text-primary"
                    }`}
                    title={index === 0 ? "Facebook" : index === 1 ? "Instagram" : "LinkedIn"}
                  >
                    <span className="material-symbols-outlined text-[18px]">{icon}</span>
                  </button>
                ))}
              </div>
              <label className="flex items-center gap-2">
                <span className="text-label-md text-outline">Status:</span>
                <select className="rounded-xl border border-outline-variant/30 bg-surface-container-low px-3 py-2 text-body-sm outline-none focus:border-primary">
                  <option>All Statuses</option>
                  <option>Scheduled</option>
                  <option>Draft</option>
                  <option>Published</option>
                </select>
              </label>
              <button className="ml-auto inline-flex items-center gap-1 text-label-md text-primary hover:underline">
                <span className="material-symbols-outlined text-[16px]">filter_list</span>
                Clear All Filters
              </button>
            </div>
          </div>

          <div className="overflow-hidden rounded-b-2xl border border-outline-variant/40 bg-surface-container-lowest shadow-sm">
            <div className="grid grid-cols-7 border-b border-outline-variant/40 bg-surface-container-low">
              {days.map((day) => (
                <div key={day.label} className="py-3 text-center text-label-md text-outline">
                  <span className="hidden sm:inline">{day.label}</span>
                  <span className="sm:hidden">{day.short}</span>
                </div>
              ))}
            </div>
            <div className="grid grid-cols-7 auto-rows-[minmax(112px,auto)]">
              {calendarCells.map((cell, index) => {
                const dayEvents = cell.muted ? [] : events.filter((event) => event.day === cell.day);
                const today = !cell.muted && cell.day === 6;

                return (
                  <div
                    key={`${cell.muted ? "muted" : "current"}-${cell.day}-${index}`}
                    className={`min-h-28 border-b border-r border-outline-variant/30 p-2 transition-colors last:border-r-0 hover:bg-surface-container-low ${
                      cell.muted ? "bg-surface-gray/40 text-outline/40" : "text-on-surface"
                    } ${today ? "relative z-10 bg-primary/5 ring-2 ring-inset ring-primary" : ""}`}
                  >
                    <div className="flex items-center justify-between gap-2">
                      <span className={`text-label-md ${today ? "font-bold text-primary" : ""}`}>
                        {cell.day}{today ? " Today" : ""}
                      </span>
                      {!cell.muted && dayEvents.length === 0 && (
                        <button className="hidden h-6 w-6 items-center justify-center rounded-full text-outline opacity-0 transition-opacity hover:bg-surface-container-high group-hover:opacity-100 sm:flex">
                          <span className="material-symbols-outlined text-[14px]">add</span>
                        </button>
                      )}
                    </div>

                    {cell.day === 1 && !cell.muted && (
                      <div className="mt-2 flex gap-1">
                        <span className="h-2 w-2 rounded-full bg-primary" />
                        <span className="h-2 w-2 rounded-full bg-secondary" />
                      </div>
                    )}

                    <div className="space-y-1">
                      {dayEvents.slice(0, 2).map((event) => (
                        <EventChip key={`${event.day}-${event.title}`} event={event} />
                      ))}
                      {dayEvents.length > 2 && (
                        <button className="mt-1 text-[10px] font-semibold text-outline hover:text-primary">+{dayEvents.length - 2} more</button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </section>

        <footer className="flex flex-col gap-3 text-body-sm text-outline md:flex-row md:items-center md:justify-between">
          <div className="flex flex-wrap items-center gap-4">
            <span className="inline-flex items-center gap-2"><span className="h-3 w-3 rounded-full bg-success-green" /> Published</span>
            <span className="inline-flex items-center gap-2"><span className="h-3 w-3 rounded-full bg-primary" /> Scheduled</span>
            <span className="inline-flex items-center gap-2"><span className="h-3 w-3 rounded-full bg-secondary" /> AI Draft</span>
            <span className="inline-flex items-center gap-2"><span className="h-3 w-3 rounded-full bg-danger-red" /> Action Required</span>
          </div>
          <span className="font-mono text-[11px]">API Endpoint: /api/content-schedules</span>
        </footer>
      </main>
      <NewPostModal open={newPostOpen} onClose={() => setNewPostOpen(false)} />
    </>
  );
}
