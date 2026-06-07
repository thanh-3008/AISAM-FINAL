"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { motion, useReducedMotion, AnimatePresence } from "motion/react";
import Header from "@/components/layout/Header";
import {
  getNotifications,
  getNotificationDetail,
  markNotificationRead,
  markAllNotificationsRead,
  getUnreadCount,
  deleteNotification,
  type NotificationListItem,
  type NotificationDetail,
} from "@/services/notificationService";

const container = {
  hidden: { opacity: 0 },
  show: { opacity: 1, transition: { staggerChildren: 0.04 } },
};

const item = {
  hidden: { opacity: 0, y: 10 },
  show: { opacity: 1, y: 0, transition: { duration: 0.35, ease: [0.16, 1, 0.3, 1] as const } },
};

function getNotificationIcon(type: string): { icon: string; color: string; bg: string } {
  switch (type.toUpperCase()) {
    case "CONTENT_PUBLISHED":
      return { icon: "task_alt", color: "text-emerald-600", bg: "bg-emerald-50" };
    case "AI_SUGGESTION":
      return { icon: "auto_awesome", color: "text-secondary", bg: "bg-secondary/10" };
    case "CAMPAIGN":
      return { icon: "campaign", color: "text-primary", bg: "bg-primary/10" };
    case "APPROVAL":
      return { icon: "approval", color: "text-amber-600", bg: "bg-amber-50" };
    case "TEAM":
      return { icon: "group", color: "text-blue-600", bg: "bg-blue-50" };
    case "BILLING":
      return { icon: "receipt", color: "text-purple-600", bg: "bg-purple-50" };
    case "SYSTEM":
      return { icon: "info", color: "text-outline", bg: "bg-surface-container" };
    default:
      return { icon: "notifications", color: "text-primary", bg: "bg-primary/10" };
  }
}

// Format date to locale string (client-side only)
function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString("en-US", { 
    month: "short", 
    day: "numeric",
    year: "numeric"
  });
}

// Calculate time ago (client-side only to avoid hydration mismatch)
function useTimeAgo(dateStr: string): string {
  const [timeAgo, setTimeAgo] = useState<string>("");
  
  useEffect(() => {
    const calculateTimeAgo = () => {
      const now = Date.now();
      const date = new Date(dateStr).getTime();
      const diff = now - date;
      
      const minutes = Math.floor(diff / 60000);
      const hours = Math.floor(diff / 3600000);
      const days = Math.floor(diff / 86400000);
      
      if (minutes < 1) setTimeAgo("Just now");
      else if (minutes < 60) setTimeAgo(`${minutes}m ago`);
      else if (hours < 24) setTimeAgo(`${hours}h ago`);
      else if (days < 7) setTimeAgo(`${days}d ago`);
      else setTimeAgo(formatDate(dateStr));
    };
    
    calculateTimeAgo();
    const interval = setInterval(calculateTimeAgo, 60000); // Update every minute
    return () => clearInterval(interval);
  }, [dateStr]);
  
  return timeAgo;
}

function TimeAgo({ dateStr }: { dateStr: string }) {
  const timeAgo = useTimeAgo(dateStr);
  return <span>{timeAgo}</span>;
}

export default function NotificationsPage() {
  const router = useRouter();
  const reduceMotion = useReducedMotion();
  
  const [notifications, setNotifications] = useState<NotificationListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [unreadCount, setUnreadCount] = useState(0);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filter, setFilter] = useState<"all" | "unread">("all");
  const [markingAll, setMarkingAll] = useState(false);
  const [detailId, setDetailId] = useState<string | null>(null);
  const [detail, setDetail] = useState<NotificationDetail | null>(null);
  const [deleting, setDeleting] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const loadingDetail = detailId !== null && detail === null;

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      const data = await getNotifications(page, 20);
      if (data) {
        setNotifications(data.data);
        setTotalPages(data.totalPages);
      }
      const count = await getUnreadCount();
      setUnreadCount(count);
      setLoading(false);
    };
    fetchData();
  }, [page]);

  const handleMarkRead = async (id: string) => {
    await markNotificationRead(id);
    setNotifications((prev) =>
      prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
    );
    setUnreadCount((prev) => Math.max(0, prev - 1));
  };

  const handleDelete = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (deleting === id) return;
    setDeleting(id);
    const success = await deleteNotification(id);
    if (success) {
      setNotifications((prev) => prev.filter((n) => n.id !== id));
      setUnreadCount((prev) => Math.max(0, prev - 1));
    }
    setToast("Notification deleted");
    setTimeout(() => setToast(null), 2500);
    setDeleting(null);
  };

  const handleOpenDetail = async (id: string) => {
    await handleMarkRead(id);
    setDetailId(id);
    setDetail(null);
    const d = await getNotificationDetail(id);
    setDetail(d);
  };

  const handleMarkAllRead = async () => {
    setMarkingAll(true);
    const success = await markAllNotificationsRead();
    if (success) {
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
      setUnreadCount(0);
    }
    setMarkingAll(false);
  };

  const filteredNotifications = filter === "unread" 
    ? notifications.filter((n) => !n.isRead) 
    : notifications;

  return (
    <div className="min-h-[100dvh] bg-surface flex flex-col">
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Notifications" }]} />
      
      <main className="flex-1 overflow-auto">
        <div className="max-w-4xl mx-auto p-6 md:p-8">
          {/* Header */}
          <motion.div
            initial={reduceMotion ? false : { opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4 }}
            className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8"
          >
            <div>
              <div className="flex items-center gap-3">
                <h1 className="text-3xl font-bold text-on-surface tracking-tight">Notifications</h1>
                {unreadCount > 0 && (
                  <span className="px-2.5 py-0.5 bg-danger-red/10 text-danger-red text-label-sm font-bold rounded-full">
                    {unreadCount}
                  </span>
                )}
              </div>
              <p className="text-body-sm text-on-surface-variant mt-1.5">
                Stay updated with your latest activities
              </p>
            </div>
            
            {unreadCount > 0 && (
              <motion.button
                whileTap={reduceMotion ? {} : { scale: 0.97 }}
                onClick={handleMarkAllRead}
                disabled={markingAll}
                className="px-4 py-2 bg-surface-container border border-outline-variant/30 text-on-surface rounded-xl text-body-sm font-semibold hover:bg-surface-container-high transition-all inline-flex items-center gap-2 disabled:opacity-50"
              >
                {markingAll ? (
                  <>
                    <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                    </svg>
                    Marking...
                  </>
                ) : (
                  <>
                    <span className="material-symbols-outlined text-[18px]">done_all</span>
                    Mark all read
                  </>
                )}
              </motion.button>
            )}
          </motion.div>

          {/* Filter Tabs */}
          <motion.div
            initial={reduceMotion ? false : { opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.05 }}
            className="flex gap-1 p-1 bg-surface-container/60 rounded-xl mb-6 w-fit border border-outline-variant/10"
          >
            {[
              { key: "all" as const, label: "All" },
              { key: "unread" as const, label: "Unread" },
            ].map((f) => (
              <motion.button
                key={f.key}
                whileTap={reduceMotion ? {} : { scale: 0.95 }}
                onClick={() => setFilter(f.key)}
                className={`px-5 py-2 rounded-lg text-body-sm font-medium transition-all ${
                  filter === f.key
                    ? "bg-surface-container-lowest text-on-surface shadow-sm"
                    : "text-on-surface-variant hover:text-on-surface"
                }`}
              >
                {f.label}
                {f.key === "unread" && unreadCount > 0 && (
                  <span className="ml-2 px-1.5 py-0.5 bg-danger-red/10 text-danger-red text-label-xs font-bold rounded-full">
                    {unreadCount}
                  </span>
                )}
              </motion.button>
            ))}
          </motion.div>

          {/* Notifications List */}
          {loading ? (
            <div className="space-y-3">
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} className="bg-surface-container-lowest border border-outline-variant/15 rounded-2xl p-5 animate-pulse">
                  <div className="flex items-start gap-4">
                    <div className="w-11 h-11 rounded-xl bg-surface-container shrink-0" />
                    <div className="flex-1 space-y-2">
                      <div className="h-4 bg-surface-container rounded w-3/4" />
                      <div className="h-3 bg-surface-container rounded w-1/2" />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : filteredNotifications.length === 0 ? (
            <motion.div
              initial={reduceMotion ? false : { opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5 }}
              className="bg-surface-container-lowest border border-outline-variant/15 rounded-2xl p-12 text-center"
            >
              <div className="w-20 h-20 mx-auto mb-5 rounded-2xl bg-surface-container flex items-center justify-center">
                <span className="material-symbols-outlined text-outline/40 text-4xl">
                  {filter === "unread" ? "mark_email_read" : "notifications_off"}
                </span>
              </div>
              <h3 className="text-body-lg text-on-surface font-semibold mb-2">
                {filter === "unread" ? "All caught up!" : "No notifications yet"}
              </h3>
              <p className="text-body-sm text-on-surface-variant max-w-sm mx-auto">
                {filter === "unread"
                  ? "You've read all your notifications. Great job staying on top of things!"
                  : "When you receive notifications, they'll appear here."}
              </p>
            </motion.div>
          ) : (
            <motion.div
              variants={reduceMotion ? undefined : container}
              initial={reduceMotion ? undefined : "hidden"}
              animate="show"
              className="space-y-2"
            >
              <AnimatePresence mode="popLayout">
                {filteredNotifications.map((notification) => {
                  const { icon, color, bg } = getNotificationIcon(notification.type);
                  return (
                    <motion.div
                      key={notification.id}
                      variants={reduceMotion ? undefined : item}
                      layout
                      exit={reduceMotion ? undefined : { opacity: 0, x: -20 }}
                      onClick={() => handleOpenDetail(notification.id)}
                      className={`bg-surface-container-lowest border rounded-2xl p-5 cursor-pointer transition-all group ${
                        notification.isRead
                          ? "border-outline-variant/15 hover:border-outline-variant/30"
                          : "border-primary/20 bg-primary/[0.02] hover:border-primary/40 hover:shadow-sm"
                      }`}
                    >
                      <div className="flex items-start gap-4">
                        <div className={`w-11 h-11 rounded-xl ${bg} flex items-center justify-center shrink-0 group-hover:scale-105 transition-transform`}>
                          <span className={`material-symbols-outlined ${color} text-[22px]`} style={{ fontVariationSettings: "'FILL' 1" }}>
                            {icon}
                          </span>
                        </div>
                        
                        <div className="flex-1 min-w-0">
                          <div className="flex items-start justify-between gap-3">
                            <div className="flex-1 min-w-0">
                              <p className="text-body-sm font-semibold truncate text-on-surface">
                                {notification.title}
                              </p>
                              <p className="text-body-sm text-on-surface-variant mt-0.5 line-clamp-2">
                                {notification.message}
                              </p>
                            </div>
                            
                            <div className="flex items-center gap-1 shrink-0">
                              {!notification.isRead && (
                                <span className="w-2.5 h-2.5 bg-primary rounded-full animate-pulse" />
                              )}
                              <button
                                onClick={(e) => handleDelete(notification.id, e)}
                                className="p-1.5 rounded-lg hover:bg-danger-red/10 text-outline hover:text-danger-red opacity-0 group-hover:opacity-100 transition-all"
                                title="Delete notification"
                              >
                                <span className="material-symbols-outlined text-[16px]">
                                  {deleting === notification.id ? "hourglass_top" : "delete"}
                                </span>
                              </button>
                            </div>
                          </div>
                          
                          <div className="flex items-center gap-3 mt-2">
                            <span className="text-label-sm text-outline flex items-center gap-1">
                              <span className="material-symbols-outlined text-[14px]">schedule</span>
                              <TimeAgo dateStr={notification.createdAt} />
                            </span>
                            {notification.actionUrl && (
                              <span className="text-label-sm text-primary font-medium flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                View details
                                <span className="material-symbols-outlined text-[14px]">arrow_forward</span>
                              </span>
                            )}
                          </div>
                        </div>
                      </div>
                    </motion.div>
                  );
                })}
              </AnimatePresence>
            </motion.div>
          )}

          {/* Pagination */}
          {totalPages > 1 && !loading && (
            <motion.div
              initial={reduceMotion ? false : { opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ delay: 0.3 }}
              className="flex items-center justify-center gap-2 mt-8"
            >
              <motion.button
                whileTap={reduceMotion ? {} : { scale: 0.95 }}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="p-2 rounded-xl border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-all disabled:opacity-40 disabled:cursor-not-allowed"
              >
                <span className="material-symbols-outlined text-[20px]">chevron_left</span>
              </motion.button>
              
              <div className="flex items-center gap-1">
                {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                  <motion.button
                    key={p}
                    whileTap={reduceMotion ? {} : { scale: 0.95 }}
                    onClick={() => setPage(p)}
                    className={`w-9 h-9 rounded-xl text-body-sm font-medium transition-all ${
                      page === p
                        ? "bg-primary text-on-primary shadow-sm shadow-primary/20"
                        : "text-on-surface-variant hover:bg-surface-container"
                    }`}
                  >
                    {p}
                  </motion.button>
                ))}
              </div>
              
              <motion.button
                whileTap={reduceMotion ? {} : { scale: 0.95 }}
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="p-2 rounded-xl border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-all disabled:opacity-40 disabled:cursor-not-allowed"
              >
                <span className="material-symbols-outlined text-[20px]">chevron_right</span>
              </motion.button>
            </motion.div>
          )}
        </div>
      </main>

      {/* Toast */}
      <AnimatePresence>
        {toast && (
          <motion.div
            initial={{ opacity: 0, y: 40 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: 40 }}
            transition={{ duration: 0.25, ease: [0.16, 1, 0.3, 1] as const }}
            className="fixed bottom-6 left-1/2 -translate-x-1/2 z-50 px-5 py-3 bg-surface-container-lowest border border-outline-variant/30 rounded-2xl shadow-xl flex items-center gap-2.5"
          >
            <span className="material-symbols-outlined text-success-green text-[18px]" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
            <span className="text-body-sm font-semibold text-on-surface">{toast}</span>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Detail Modal */}
      <AnimatePresence>
        {detailId && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm"
            onClick={() => setDetailId(null)}
          >
            <motion.div
              initial={{ scale: 0.95, opacity: 0, y: 10 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.95, opacity: 0, y: 10 }}
              transition={{ duration: 0.2, ease: [0.16, 1, 0.3, 1] as const }}
              onClick={(e: React.MouseEvent) => e.stopPropagation()}
              className="bg-surface-container-lowest rounded-2xl shadow-2xl border border-outline-variant/20 w-full max-w-lg max-h-[80vh] overflow-y-auto"
            >
              {loadingDetail ? (
                <div className="p-8 space-y-4">
                  <div className="h-6 bg-surface-container rounded w-3/4 animate-pulse" />
                  <div className="h-4 bg-surface-container rounded w-full animate-pulse" />
                  <div className="h-4 bg-surface-container rounded w-1/2 animate-pulse" />
                </div>
              ) : detail ? (
                <>
                  <div className="flex items-start justify-between p-6 border-b border-outline-variant/20">
                    <div className="flex items-center gap-3">
                      <div className={`w-10 h-10 rounded-xl ${getNotificationIcon(detail.type).bg} flex items-center justify-center`}>
                        <span className={`material-symbols-outlined ${getNotificationIcon(detail.type).color} text-[20px]`} style={{ fontVariationSettings: "'FILL' 1" }}>
                          {getNotificationIcon(detail.type).icon}
                        </span>
                      </div>
                      <div>
                        <h3 className="text-body-lg font-bold text-on-surface">{detail.title}</h3>
                        <span className="text-label-xs text-outline">{formatDate(detail.createdAt)}</span>
                      </div>
                    </div>
                    <button onClick={() => setDetailId(null)} className="p-1.5 hover:bg-surface-container rounded-lg transition-colors">
                      <span className="material-symbols-outlined text-[18px]">close</span>
                    </button>
                  </div>
                  <div className="p-6 space-y-4">
                    <p className="text-body-md text-on-surface-variant leading-relaxed">{detail.message}</p>
                    {detail.metadata && Object.keys(detail.metadata).length > 0 && (
                      <div className="bg-surface-container rounded-xl p-4 space-y-2">
                        {Object.entries(detail.metadata).map(([key, val]) => (
                          <div key={key} className="flex items-center justify-between text-label-sm">
                            <span className="text-outline capitalize">{key.replace(/([A-Z])/g, " $1")}</span>
                            <span className="text-on-surface font-medium">{val}</span>
                          </div>
                        ))}
                      </div>
                    )}
                    <div className="flex items-center gap-2 pt-2">
                      {detail.actionUrl && (
                        <button
                          onClick={() => router.push(detail.actionUrl!)}
                          className="px-4 py-2 bg-primary text-on-primary rounded-xl text-label-sm font-semibold hover:shadow-lg hover:shadow-primary/20 transition-all"
                        >
                          Go to {detail.type.toLowerCase().replace("_", " ")}
                        </button>
                      )}
                      <button
                        onClick={() => setDetailId(null)}
                        className="px-4 py-2 bg-surface-container border border-outline-variant/30 text-on-surface rounded-xl text-label-sm font-semibold hover:bg-surface-container-high transition-all"
                      >
                        Close
                      </button>
                    </div>
                  </div>
                </>
              ) : (
                <div className="p-8 text-center">
                  <p className="text-body-sm text-on-surface-variant">Notification not found.</p>
                  <button onClick={() => setDetailId(null)} className="mt-4 text-label-sm text-primary font-semibold">Close</button>
                </div>
              )}
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
