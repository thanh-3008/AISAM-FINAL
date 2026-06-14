"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useState, useEffect } from "react";
import { useWorkspaces, getWorkspaceTypeLabel } from "@/hooks/useWorkspaces";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import { getUserFromToken, logout } from "@/lib/auth";
import { useSidebar } from "@/contexts/SidebarContext";
import CreateProfileModal from "@/components/profiles/CreateProfileModal";

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
      { label: "Dashboard", href: "/dashboard", icon: "space_dashboard" },
    ],
  },
  {
    label: "Content Workspace",
    items: [
      { label: "Brand Kit", href: "/brands", icon: "palette" },
      { label: "Content", href: "/content", icon: "description" },
      { label: "Approvals", href: "/approvals", icon: "task_alt" },
      { label: "Posts", href: "/posts", icon: "send" },
      { label: "Calendar", href: "/calendar", icon: "calendar_month" },
    ],
  },
  {
    label: "Marketing",
    items: [
      { label: "Social Accounts", href: "/social", icon: "public" },
      { label: "Campaigns", href: "/campaigns", icon: "campaign" },
      { label: "Analysis", href: "/analytics", icon: "bar_chart" },
    ],
  },
  {
    label: "Administration",
    items: [
      { label: "Team Management", href: "/team", icon: "group" },
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
        <span className="ml-auto text-label-2xs text-outline/30 font-semibold tracking-wider">SOON</span>
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
  const router = useRouter();
  const [workspaceOpen, setWorkspaceOpen] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const { workspaces, loading, activeWorkspace, selectWorkspace } = useWorkspaces();
  const [user, setUser] = useState<{ name?: string; email?: string } | null>(() => getUserFromToken());
  const [hoveredWorkspace, setHoveredWorkspace] = useState<string | null>(null);

  const isActive = (href: string) =>
    pathname === href || pathname.startsWith(href + "/");

  const displayName = activeWorkspace?.name || "No Workspace";
  const displayPlan = activeWorkspace ? getWorkspaceTypeLabel(activeWorkspace.workspaceType) : "—";
  const initials = activeWorkspace ? getInitials(activeWorkspace.name) : "?";

  const { open, toggle } = useSidebar();
  const featureGate = useFeatureGate();

  const visibleSections = navSections
    .map((section) => ({
      ...section,
      items: section.items.filter((item) => {
        if (item.label === "Team Management") return featureGate.canAccess("teamManagement");
        return true;
      }),
    }))
    .filter((section) => section.items.length > 0);

  return (
    <aside
      className={`fixed left-0 top-0 h-full bg-surface-container-lowest/90 backdrop-blur-xl border-r border-outline-variant/30 flex flex-col z-50 transition-transform duration-300 ${open ? "translate-x-0" : "-translate-x-full"}`}
      style={{ width: "var(--spacing-sidebar-width)" }}
    >
      {/* Logo + Toggle */}
      <div className="flex items-center justify-between px-5 pt-6 pb-5 border-b border-outline-variant/20 mx-4">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-gradient-to-br from-primary to-primary-container rounded-xl flex items-center justify-center shadow-lg shadow-primary/20 shrink-0">
            <span className="material-symbols-outlined text-on-primary text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>
              psychology
            </span>
          </div>
          <div className={open ? "" : "hidden"}>
            <h1 className="text-headline-sm font-bold bg-gradient-to-r from-primary to-primary-container bg-clip-text text-transparent leading-none">AISAM</h1>
            <p className="text-label-sm text-outline leading-none mt-0.5">AI Ad Manager</p>
          </div>
        </div>
        <button onClick={toggle} className={`w-8 h-8 rounded-xl hover:bg-surface-container flex items-center justify-center transition-all ${open ? "" : "hidden"}`} title="Collapse sidebar">
          <span className="material-symbols-outlined text-outline text-[18px]">chevron_left</span>
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 px-4 pb-4 relative">
        {visibleSections.map((section) => {
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

      {/* Logout */}
      <div className="border-t border-outline-variant/20 px-4 pt-3 pb-4">
        <button
            onClick={async () => { await logout(); window.location.href = "/login"; }}
            className="w-full group relative flex items-center gap-3 px-4 py-2.5 rounded-xl transition-all duration-200 text-on-surface-variant hover:bg-surface-container hover:text-danger-red text-left"
          >
            <span className="material-symbols-outlined text-[20px] group-hover:scale-110 transition-transform duration-200">logout</span>
            <span className="text-body-sm font-semibold">Logout</span>
          </button>
      </div>

      {/* Workspace Selector - Bottom */}
      <div className="relative px-4 pb-4 mt-auto">
        <button
          onClick={() => setWorkspaceOpen(!workspaceOpen)}
          className="w-full px-3 py-2.5 rounded-xl bg-gradient-to-r from-surface-container to-surface-container-low border border-outline-variant/20 flex items-center gap-2.5 hover:from-surface-container-high hover:to-surface-container transition-all duration-200 text-left group"
        >
          <div className="w-7 h-7 rounded-full bg-gradient-to-br from-primary to-primary-container flex items-center justify-center shrink-0 text-on-primary text-label-sm font-bold shadow-sm">
            {loading ? "?" : initials}
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2">
              <p className="text-label-md text-on-surface truncate font-medium">
                {loading ? (
                  <span className="inline-block w-16 h-3 bg-surface-container-high rounded animate-pulse" />
                ) : displayName}
              </p>
              {!loading && activeWorkspace && (
                <span className={`inline-flex items-center gap-1 px-1.5 py-0.5 rounded-full text-label-2xs font-semibold border ${
                  activeWorkspace.workspaceType === 2 
                    ? "bg-purple-50 text-purple-700 border-purple-200/50" 
                    : "bg-blue-50 text-blue-700 border-blue-200/50"
                }`}>
                  <span className="material-symbols-outlined text-[10px]">
                    {activeWorkspace.workspaceType === 2 ? "business" : "person"}
                  </span>
                  {activeWorkspace.workspaceType === 2 ? "Business" : "Personal"}
                </span>
              )}
            </div>
            <p className="text-label-xs text-label-sm text-on-surface-variant truncate">
              {loading ? "" : displayPlan}
            </p>
          </div>
          <span className={`material-symbols-outlined text-on-surface-variant text-[18px] shrink-0 transition-transform duration-200 ${workspaceOpen ? "rotate-180" : ""}`}>
            unfold_more
          </span>
        </button>

        {workspaceOpen && (
          <>
            <div className="fixed inset-0 z-10" onClick={() => setWorkspaceOpen(false)} />
            <div className="absolute left-4 right-4 bottom-full mb-2 bg-surface-container-lowest/95 backdrop-blur-xl border border-outline-variant/20 rounded-xl shadow-2xl z-20 py-1.5 max-h-60 overflow-y-auto animate-in fade-in slide-in-from-bottom-2 duration-200">
              {workspaces.length === 0 ? (
                <div className="px-4 py-3 text-center">
                  <p className="text-label-sm text-on-surface-variant mb-2">No workspaces yet</p>
                  <button
                    onClick={() => { setWorkspaceOpen(false); setShowCreateModal(true); }}
                    className="text-label-sm text-primary font-semibold hover:text-primary/80"
                  >
                    Create workspace
                  </button>
                </div>
              ) : (
                <>
                  {workspaces.map((w) => {
                    const active = activeWorkspace?.id === w.id;
                    return (
                      <button
                        key={w.id}
                        onClick={() => { selectWorkspace(w); setWorkspaceOpen(false); }}
                        onMouseEnter={() => setHoveredWorkspace(w.id)}
                        onMouseLeave={() => setHoveredWorkspace(null)}
                        className={`w-full flex items-center gap-3 px-4 py-2.5 transition-all duration-150 text-left ${
                          active
                            ? "bg-gradient-to-r from-primary/8 to-transparent"
                            : hoveredWorkspace === w.id ? "bg-surface-container" : ""
                        }`}
                      >
                        <div className={`w-7 h-7 rounded-full flex items-center justify-center shrink-0 text-label-sm font-bold transition-all ${
                          active
                            ? "bg-primary text-on-primary shadow-sm"
                            : "bg-surface-container-high text-on-surface-variant"
                        }`}>
                          {getInitials(w.name)}
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2">
                            <p className="text-body-sm text-on-surface truncate font-medium">{w.name}</p>
                            <span className={`inline-flex items-center gap-1 px-1.5 py-0.5 rounded-full text-label-2xs font-semibold border shrink-0 ${
                              w.workspaceType === 2 
                                ? "bg-purple-50 text-purple-700 border-purple-200/50" 
                                : "bg-blue-50 text-blue-700 border-blue-200/50"
                            }`}>
                              <span className="material-symbols-outlined text-[10px]">
                                {w.workspaceType === 2 ? "business" : "person"}
                              </span>
                              {w.workspaceType === 2 ? "Business" : "Personal"}
                            </span>
                          </div>
                          <p className="text-label-xs text-label-sm text-on-surface-variant truncate">{getWorkspaceTypeLabel(w.workspaceType)}</p>
                        </div>
                        {active && (
                          <span className="material-symbols-outlined text-primary text-[16px] shrink-0">check</span>
                        )}
                      </button>
                    );
                  })}
                </>
              )}
            </div>
          </>
        )}
      </div>

      <CreateProfileModal open={showCreateModal} onClose={() => setShowCreateModal(false)} />
    </aside>
  );
}
