"use client";

import { useState, useEffect, useRef } from "react";
import { useTheme } from "next-themes";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useWorkspaces, getWorkspaceTypeLabel } from "@/hooks/useWorkspaces";
import { getUserFromToken, logout } from "@/lib/auth";
import { useSidebar } from "@/contexts/SidebarContext";
import {
  getNotifications,
  getUnreadCount,
  markAllNotificationsRead,
  type NotificationListItem,
} from "@/services/notificationService";
interface HeaderProps {
  title?: string;
  breadcrumbs?: { label: string; href?: string }[];
}

function getInitials(name: string) {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

function getNotificationIcon(type: string): { icon: string; color: string; bg: string } {
  switch (type.toUpperCase()) {
    case "CONTENT_PUBLISHED":
      return { icon: "task_alt", color: "text-success-green", bg: "bg-success-green/10" };
    case "AI_SUGGESTION":
      return { icon: "auto_awesome", color: "text-secondary", bg: "bg-secondary/10" };
    case "CAMPAIGN":
      return { icon: "campaign", color: "text-primary", bg: "bg-primary/10" };
    case "APPROVAL":
      return { icon: "approval", color: "text-amber-600", bg: "bg-amber-50" };
    default:
      return { icon: "notifications", color: "text-primary", bg: "bg-primary/10" };
  }
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
      else setTimeAgo(new Date(dateStr).toLocaleDateString("en-US", { month: "short", day: "numeric" }));
    };
    
    calculateTimeAgo();
  }, [dateStr]);
  
  return timeAgo;
}

function TimeAgo({ dateStr }: { dateStr: string }) {
  const timeAgo = useTimeAgo(dateStr);
  return <span>{timeAgo}</span>;
}

export default function Header({ breadcrumbs }: HeaderProps) {
  const router = useRouter();
  const [notifOpen, setNotifOpen] = useState(false);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const { theme, setTheme } = useTheme();
  const [user, setUser] = useState<{ name?: string; email?: string } | null>(null);
  const [unreadCount, setUnreadCount] = useState(0);
  const [recentNotifs, setRecentNotifs] = useState<NotificationListItem[]>([]);
  const [loadingNotifs, setLoadingNotifs] = useState(false);
  const [markingAll, setMarkingAll] = useState(false);
  const { activeWorkspace } = useWorkspaces();
  const { toggle } = useSidebar();
  const notifRef = useRef<HTMLDivElement>(null);
  const userMenuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    setUser(getUserFromToken());
  }, []);

  // Fetch unread count
  useEffect(() => {
    const fetchCount = async () => {
      const count = await getUnreadCount();
      setUnreadCount(count);
    };
    fetchCount();
    const interval = setInterval(fetchCount, 30000); // Poll every 30s
    return () => clearInterval(interval);
  }, [activeWorkspace?.id]);

  // Fetch recent notifications when dropdown opens
  useEffect(() => {
    if (notifOpen) {
      const fetchNotifs = async () => {
        setLoadingNotifs(true);
        const data = await getNotifications(1, 5);
        if (data) {
          setRecentNotifs(data.data);
        }
        setLoadingNotifs(false);
      };
      fetchNotifs();
    }
  }, [notifOpen]);

  const handleMarkAllRead = async () => {
    setMarkingAll(true);
    const success = await markAllNotificationsRead();
    if (success) {
      setRecentNotifs((prev) => prev.map((n) => ({ ...n, isRead: true })));
      setUnreadCount(0);
    }
    setMarkingAll(false);
  };

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (notifRef.current && !notifRef.current.contains(e.target as Node)) {
        setNotifOpen(false);
      }
      if (userMenuRef.current && !userMenuRef.current.contains(e.target as Node)) {
        setUserMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const [mounted, setMounted] = useState(false);
  useEffect(() => {
    setMounted(true);
  }, []);

  const handleLogout = async () => {
    setUserMenuOpen(false);
    await logout();
    window.location.href = "/login";
 window.location.href = "/login";
  };

  const displayName = mounted ? (user?.name || activeWorkspace?.name || "User") : "User";
  const initials = getInitials(displayName);
  const displayPlan = mounted ? (activeWorkspace ? getWorkspaceTypeLabel(activeWorkspace.workspaceType) : "No Workspace") : "Loading...";
  const settingsHref = activeWorkspace ? `/profiles/${activeWorkspace.id}` : "/overview";

  return (
    <header className="h-16 bg-surface-gray border-b border-outline-variant/30 flex justify-between items-center px-gutter z-40 sticky top-0">
      <div className="flex items-center gap-4 flex-1 min-w-0">
        {/* Sidebar Toggle — pill shape to match icon buttons */}
        <button onClick={toggle} className="w-9 h-9 rounded-full hover:bg-surface-container flex items-center justify-center transition-all shrink-0 active:scale-95" title="Toggle sidebar">
          <span className="material-symbols-outlined text-on-surface-variant text-[20px]">menu</span>
        </button>

        {/* Breadcrumbs / Search */}
        {breadcrumbs && breadcrumbs.length > 0 ? (
          <nav className="flex items-center gap-1 min-w-0">
            {breadcrumbs.map((crumb, i) => (
              <span key={i} className="flex items-center gap-1 min-w-0">
                {i > 0 && (
                  <span className="material-symbols-outlined text-outline/50 text-[14px] shrink-0">chevron_right</span>
                )}
                {crumb.href && i < breadcrumbs.length - 1 ? (
                  <Link href={crumb.href} className="text-body-sm text-on-surface-variant hover:text-on-surface transition-colors truncate">
                    {crumb.label}
                  </Link>
                ) : (
                  <span className="text-body-sm font-bold text-on-surface truncate">{crumb.label}</span>
                )}
              </span>
            ))}
          </nav>
        ) : (
          <div className="relative max-w-md w-full">
            <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline/60 text-[18px]">search</span>
            <input
              className="w-full bg-surface-container-lowest border border-outline-variant/50 rounded-full pl-10 pr-4 py-2 text-body-sm placeholder:text-outline/40 focus:border-primary/50 focus:ring-2 focus:ring-primary/10 outline-none transition-all"
              placeholder="Search insights, campaigns, or assets..."
              type="text"
            />
          </div>
        )}
      </div>

      {/* Actions */}
      <div className="flex items-center gap-1">

        {/* Notifications */}
        <div className="relative" ref={notifRef}>
          <button
            onClick={() => setNotifOpen(!notifOpen)}
            className="hover:bg-surface-container rounded-full p-2 transition-all relative group"
          >
            <span className="material-symbols-outlined text-on-surface-variant text-[22px] group-hover:scale-110 transition-transform duration-200" style={{ fontVariationSettings: "'FILL' 1" }}>notifications_active</span>
            {unreadCount > 0 && (
              <span className="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] bg-danger-red rounded-full text-label-xs font-bold text-white flex items-center justify-center px-1 shadow-sm ring-2 ring-surface">
                {unreadCount > 9 ? "9+" : unreadCount}
              </span>
            )}
          </button>

          {notifOpen && (
            <div className="absolute right-0 top-11 w-80 bg-surface-container-lowest border border-outline-variant/30 rounded-2xl shadow-lg z-50 animate-in fade-in slide-in-from-top-2 duration-200">
              <div className="flex items-center justify-between px-5 py-3.5 border-b border-outline-variant/20">
                <span className="text-headline-sm font-bold">Notifications</span>
                {unreadCount > 0 && (
                  <button 
                    onClick={handleMarkAllRead}
                    disabled={markingAll}
                    className="text-label-sm text-primary hover:text-primary/80 transition-colors disabled:opacity-50"
                  >
                    {markingAll ? "Marking..." : "Mark all read"}
                  </button>
                )}
              </div>
              <div className="divide-y divide-outline-variant/10 max-h-72 overflow-y-auto">
                {loadingNotifs ? (
                  <div className="px-5 py-8 text-center">
                    <div className="w-6 h-6 border-2 border-primary/20 border-t-primary rounded-full animate-spin mx-auto mb-2" />
                    <p className="text-label-sm text-on-surface-variant">Loading...</p>
                  </div>
                ) : recentNotifs.length === 0 ? (
                  <div className="px-5 py-8 text-center">
                    <span className="material-symbols-outlined text-outline/40 text-3xl mb-2 block">notifications_off</span>
                    <p className="text-label-sm text-on-surface-variant">No notifications yet</p>
                  </div>
                ) : (
                  recentNotifs.map((n) => {
                    const { icon, color, bg } = getNotificationIcon(n.type);
                    return (
                      <Link 
                        key={n.id} 
                        href="/notifications"
                        onClick={() => setNotifOpen(false)}
                        className={`flex items-start gap-3 px-5 py-3.5 hover:bg-surface-container/60 transition-colors cursor-pointer ${n.isRead ? "" : "bg-primary/[0.03]"}`}
                      >
                        <div className={`w-9 h-9 rounded-full ${bg} flex items-center justify-center shrink-0`}>
                          <span className={`material-symbols-outlined ${color} text-[18px]`} style={{ fontVariationSettings: "'FILL' 1" }}>{icon}</span>
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-body-sm text-on-surface font-medium line-clamp-1">{n.title}</p>
                          <p className="text-label-sm text-outline/60 mt-0.5"><TimeAgo dateStr={n.createdAt} /></p>
                        </div>
                        {!n.isRead && <span className="w-1.5 h-1.5 bg-primary rounded-full shrink-0 mt-2.5" />}
                      </Link>
                    );
                  })
                )}
              </div>
              <div className="px-5 py-3 border-t border-outline-variant/20">
                <Link href="/notifications" className="text-label-sm text-primary hover:text-primary/80 transition-colors flex items-center gap-1 justify-center">
                  View all notifications
                  <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
                </Link>
              </div>
            </div>
          )}
        </div>

        {/* Settings */}
        <button 
          onClick={() => {
            router.push(settingsHref);
          }}
          className="hover:bg-surface-container rounded-full p-2 transition-all relative group"
        >
          <span className="material-symbols-outlined text-on-surface-variant text-[22px] group-hover:rotate-90 transition-transform duration-300" style={{ fontVariationSettings: "'FILL' 1" }}>settings_suggest</span>
        </button>

        {/* Divider */}
        <div className="h-7 w-px bg-outline-variant/40 mx-1" />

        {/* User Avatar + Dropdown */}
        <div className="relative" ref={userMenuRef}>
          <button
            onClick={() => setUserMenuOpen(!userMenuOpen)}
            className="flex items-center gap-3 cursor-pointer group px-2 py-1.5 rounded-full hover:bg-surface-container transition-all"
          >
            <div className="text-right hidden sm:block">
              <p className="text-body-sm font-semibold text-on-surface text-left leading-tight">{displayName}</p>
              <p className="text-label-sm text-outline text-left leading-tight">{displayPlan}</p>
            </div>
            <div className="w-9 h-9 rounded-full border-2 border-primary-fixed/60 shadow-sm group-hover:border-primary transition-all overflow-hidden bg-surface-container-high shrink-0">
              <div className="w-full h-full flex items-center justify-center text-primary font-semibold text-label-sm">
                {initials}
              </div>
            </div>
          </button>

          {userMenuOpen && (
            <>
              <div className="fixed inset-0 z-40" onClick={() => setUserMenuOpen(false)} />
              <div className="absolute right-0 top-full mt-1.5 w-56 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-lg z-50 py-1.5 animate-in fade-in slide-in-from-top-2 duration-200">
                <div className="px-4 py-2.5 border-b border-outline-variant/10">
                  <p className="text-body-sm font-semibold text-on-surface">{displayName}</p>
                  <p className="text-label-sm text-outline/60">{user?.email || "No email"}</p>
                </div>
                <div className="pt-1">
                  <Link
                    href="/pricing"
                    onClick={() => setUserMenuOpen(false)}
                    className="flex items-center gap-3 px-4 py-2.5 text-body-sm text-on-surface hover:bg-surface-container transition-colors"
                  >
                    <span className="material-symbols-outlined text-[18px] text-outline/60">workspace_premium</span>
                    Upgrade Plan
                  </Link>
                  <Link
                    href={settingsHref}
                    onClick={() => setUserMenuOpen(false)}
                    className="flex items-center gap-3 px-4 py-2.5 text-body-sm text-on-surface hover:bg-surface-container transition-colors"
                  >
                    <span className="material-symbols-outlined text-[18px] text-outline/60">account_circle</span>
                    Workspace Settings
                  </Link>
                  <button
                    onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
                    className="w-full flex items-center gap-3 px-4 py-2.5 text-body-sm text-on-surface hover:bg-surface-container transition-colors text-left"
                  >
                    <span className="material-symbols-outlined text-[18px] text-outline/60" style={{ fontVariationSettings: "'FILL' 1" }}>{theme === 'dark' ? "light_mode" : "dark_mode"}</span>
                    {theme === 'dark' ? "Light Mode" : "Dark Mode"}
                  </button>
                </div>
                <div className="border-t border-outline-variant/10 mt-1 pt-1">
                  <button
                    onClick={handleLogout}
                    className="w-full flex items-center gap-3 px-4 py-2.5 text-body-sm text-danger-red hover:bg-danger-red/5 transition-colors text-left"
                  >
                    <span className="material-symbols-outlined text-[18px]">logout</span>
                    Logout
                  </button>
                </div>
              </div>
            </>
          )}
        </div>
      </div>

    </header>
  );
}


