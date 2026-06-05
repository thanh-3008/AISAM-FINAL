"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState, useEffect } from "react";
import { useProfiles, getProfileTypeLabel } from "@/hooks/useProfiles";
import { getUserFromToken, logout } from "@/lib/auth";
import { useSidebar } from "@/contexts/SidebarContext";

type NavItemConfig = {
  label: string;
  href: string;
  icon: string;
  disabled?: boolean;
};

const navSections: { label: string; items: NavItemConfig[] }[] = [
  {
    label: "Dashboard",
    items: [
      { label: "Dashboard", href: "/dashboard", icon: "dashboard" },
    ],
  },
  {
    label: "Content Workspace",
    items: [
      { label: "Brand Kit", href: "/brands", icon: "inventory_2" },
      { label: "Content", href: "/content", icon: "photo_library", disabled: true },
      { label: "Approvals", href: "/approvals", icon: "fact_check", disabled: true },
      { label: "Posts", href: "/posts", icon: "send", disabled: true },
      { label: "Calendar", href: "/calendar", icon: "event", disabled: true },
    ],
  },
  {
    label: "Marketing",
    items: [
      { label: "Social Accounts", href: "/social", icon: "share", disabled: true },
      { label: "Campaigns", href: "/campaigns", icon: "campaign", disabled: true },
      { label: "Analysis", href: "/analytics", icon: "bar_chart", disabled: true },
    ],
  },
];

function getInitials(name: string) {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

function NavItem({ href, icon, label, active, disabled }: NavItemConfig & { active: boolean }) {
  const content = (
    <>
      <span className={`absolute left-0 top-1/2 -translate-y-1/2 w-1 rounded-r-full transition-all duration-300 ${
        active ? "h-5 bg-primary" : "h-0 bg-transparent"
      }`} />
      <span className={`material-symbols-outlined text-[20px] transition-all duration-200 ${
        active ? "scale-110" : disabled ? "" : "group-hover:scale-110"
      }`}>
        {icon}
      </span>
      <span className={`text-body-sm font-semibold ${disabled ? "text-outline/40" : ""}`}>{label}</span>
    </>
  );

  if (disabled) {
    return (
      <div className="group relative flex items-center gap-3 px-4 py-2.5 rounded-xl text-on-surface-variant/40 cursor-not-allowed" title="Coming soon">
        {content}
        <span className="ml-auto text-[9px] text-outline/30 font-semibold tracking-wider">SOON</span>
      </div>
    );
  }

  return (
    <Link
      href={href}
      className={`group relative flex items-center gap-3 px-4 py-2.5 rounded-xl transition-all duration-200 ${
        active
          ? "bg-gradient-to-r from-primary/10 to-transparent text-primary font-semibold"
          : "text-on-surface-variant hover:bg-surface-container hover:text-on-surface"
      }`}
    >
      {content}
    </Link>
  );
}

export default function Sidebar() {
  const pathname = usePathname();
  const [profileOpen, setProfileOpen] = useState(false);
  const { profiles, loading, activeProfile } = useProfiles();
  const [user, setUser] = useState<{ name?: string; email?: string } | null>(null);
  const [hoveredProfile, setHoveredProfile] = useState<string | null>(null);

  useEffect(() => {
    setUser(getUserFromToken());
  }, []);

  const isActive = (href: string) =>
    pathname === href || pathname.startsWith(href + "/");

  const displayName = activeProfile?.name || "No Profile";
  const displayPlan = activeProfile ? getProfileTypeLabel(activeProfile.profileType) : "—";
  const initials = activeProfile ? getInitials(activeProfile.name) : "?";

  const { open, toggle } = useSidebar();

  return (
    <aside
      className={`fixed left-0 top-0 h-full w-sidebar-width bg-surface-container-lowest/90 backdrop-blur-xl border-r border-outline-variant/30 flex flex-col z-50 transition-transform duration-300 ${open ? "translate-x-0" : "-translate-x-full"}`}
    >
      {/* Logo + Toggle */}
      <div className="flex items-center justify-between px-5 pt-6 pb-5 border-b border-outline-variant/20 mx-4">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-gradient-to-br from-primary to-primary-container rounded-xl flex items-center justify-center shadow-lg shadow-primary/20 shrink-0">
            <span className="material-symbols-outlined text-on-primary text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>
              auto_awesome
            </span>
          </div>
          <div className={open ? "" : "hidden"}>
            <h1 className="text-headline-sm font-bold bg-gradient-to-r from-primary to-primary-container bg-clip-text text-transparent leading-none">AISAM</h1>
            <p className="text-label-sm text-outline leading-none mt-0.5">AI Ad Manager</p>
          </div>
        </div>
        <button onClick={toggle} className={`w-8 h-8 rounded-xl hover:bg-surface-container flex items-center justify-center transition-all ${open ? "" : "hidden"}`} title="Collapse sidebar">
          <span className="material-symbols-outlined text-outline text-[18px]">menu_open</span>
        </button>
      </div>

      {/* Profile Selector */}
      <div className="relative px-4 mt-4 mb-3">
        <button
          onClick={() => setProfileOpen(!profileOpen)}
          className="w-full px-3 py-2.5 rounded-xl bg-gradient-to-r from-surface-container to-surface-container-low border border-outline-variant/20 flex items-center gap-2.5 hover:from-surface-container-high hover:to-surface-container transition-all duration-200 text-left group"
        >
          <div className="w-7 h-7 rounded-full bg-gradient-to-br from-primary to-primary-container flex items-center justify-center shrink-0 text-on-primary text-label-sm font-bold shadow-sm">
            {loading ? "?" : initials}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-label-md text-on-surface truncate font-medium">
              {loading ? (
                <span className="inline-block w-16 h-3 bg-surface-container-high rounded animate-pulse" />
              ) : displayName}
            </p>
            <p className="text-[10px] text-label-sm text-on-surface-variant truncate">
              {loading ? "" : displayPlan}
            </p>
          </div>
          <span className={`material-symbols-outlined text-on-surface-variant text-[18px] shrink-0 transition-transform duration-200 ${profileOpen ? "rotate-180" : ""}`}>
            unfold_more
          </span>
        </button>

        {profileOpen && (
          <>
            <div className="fixed inset-0 z-10" onClick={() => setProfileOpen(false)} />
            <div className="absolute left-4 right-4 top-full mt-1.5 bg-surface-container-lowest/95 backdrop-blur-xl border border-outline-variant/20 rounded-xl shadow-2xl z-20 py-1.5 max-h-60 overflow-y-auto animate-in fade-in slide-in-from-top-2 duration-200">
              {profiles.map((p) => {
                const active = activeProfile?.id === p.id;
                return (
                  <Link
                    key={p.id}
                    href={`/profiles/${p.id}`}
                    onClick={() => setProfileOpen(false)}
                    onMouseEnter={() => setHoveredProfile(p.id)}
                    onMouseLeave={() => setHoveredProfile(null)}
                    className={`flex items-center gap-3 px-4 py-2.5 transition-all duration-150 ${
                      active
                        ? "bg-gradient-to-r from-primary/8 to-transparent"
                        : hoveredProfile === p.id ? "bg-surface-container" : ""
                    }`}
                  >
                    <div className={`w-7 h-7 rounded-full flex items-center justify-center shrink-0 text-label-sm font-bold transition-all ${
                      active
                        ? "bg-primary text-on-primary shadow-sm"
                        : "bg-surface-container-high text-on-surface-variant"
                    }`}>
                      {getInitials(p.name)}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-body-sm text-on-surface truncate font-medium">{p.name}</p>
                      <p className="text-[10px] text-label-sm text-on-surface-variant truncate">{getProfileTypeLabel(p.profileType)}</p>
                    </div>
                    {active && (
                      <span className="material-symbols-outlined text-primary text-[16px] shrink-0">check</span>
                    )}
                  </Link>
                );
              })}
              <div className="border-t border-outline-variant/20 mt-1 pt-1 mx-3">
                <Link
                  href="/profiles/new"
                  onClick={() => setProfileOpen(false)}
                  className="flex items-center gap-3 px-4 py-2.5 rounded-xl hover:bg-surface-container transition-all duration-150 text-primary text-body-sm font-medium"
                >
                  <span className="material-symbols-outlined text-[18px]">add_circle</span>
                  Create New Profile
                </Link>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto space-y-1 px-4 pb-4 scrollbar-thin relative">
        <div className="sticky top-0 h-4 bg-gradient-to-b from-surface-container-lowest/90 to-transparent pointer-events-none -mx-4 -mt-4 mb-0 z-10" />
        {navSections.map((section) => {
          const isContent = section.label === "Content Workspace";
          const isMarketing = section.label === "Marketing";
          return (
            <div key={section.label} className={isContent || isMarketing ? "mt-5" : ""}>
              {section.label !== "Dashboard" && (
                <p className="text-label-sm text-outline/50 mb-2 px-2 tracking-wider">
                  {section.label}
                </p>
              )}
              <div className="space-y-0.5">
                  {section.items.map((item) => (
                  <NavItem
                    key={item.href}
                    href={item.href}
                    icon={item.icon}
                    label={item.label}
                    active={isActive(item.href)}
                    disabled={item.disabled}
                  />
                ))}
              </div>
            </div>
          );
        })}
      </nav>

      {/* Scroll fade */}
      <div className="h-3 bg-gradient-to-t from-surface-container-lowest/90 to-transparent -mt-3 pointer-events-none relative z-10" />

      {/* System Section */}
      <div className="border-t border-outline-variant/20 px-4 pt-3 pb-4">
        <p className="text-label-sm text-outline/50 mb-2 px-2 tracking-wider">System</p>
        <div className="space-y-0.5">
          <div className="group relative flex items-center gap-3 px-4 py-2.5 rounded-xl text-on-surface-variant/40 cursor-not-allowed" title="Coming soon">
            <span className="material-symbols-outlined text-[20px]">settings</span>
            <span className="text-body-sm font-semibold">Settings</span>
            <span className="ml-auto text-[9px] text-outline/30 font-semibold tracking-wider">SOON</span>
          </div>
          <button
            onClick={async () => { await logout(); window.location.href = "/login"; }}
            className="w-full group relative flex items-center gap-3 px-4 py-2.5 rounded-xl transition-all duration-200 text-on-surface-variant hover:bg-surface-container hover:text-danger-red text-left"
          >
            <span className="material-symbols-outlined text-[20px] group-hover:scale-110 transition-transform duration-200">logout</span>
            <span className="text-body-sm font-semibold">Logout</span>
          </button>
        </div>
      </div>
    </aside>
  );
}
