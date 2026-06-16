"use client";

import { useRouter } from "next/navigation";

export type WorkspaceSection = "overview" | "my-profile" | "team" | "security" | "billing" | "subscription";

const sections: { key: WorkspaceSection; label: string; icon: string }[] = [
  { key: "overview", label: "Overview", icon: "dashboard" },
  { key: "my-profile", label: "Workspace Info", icon: "workspaces" },
  { key: "team", label: "Team", icon: "group" },
  { key: "security", label: "Security", icon: "lock" },
  { key: "billing", label: "Billing & Credits", icon: "credit_card" },
  { key: "subscription", label: "Subscription", icon: "workspace_premium" },
];

interface WorkspaceSettingsSidebarProps {
  activeSection: WorkspaceSection;
  onSectionChange: (section: WorkspaceSection) => void;
  workspaceName?: string;
  workspaceInitials?: string;
}

export default function WorkspaceSettingsSidebar({
  activeSection,
  onSectionChange,
  workspaceName,
  workspaceInitials,
}: WorkspaceSettingsSidebarProps) {
  const router = useRouter();

  return (
    <aside className="w-64 shrink-0 border-r border-outline-variant/20 bg-surface-container-low/30 flex flex-col">
      {/* Header */}
      <div className="p-5 border-b border-outline-variant/20">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary to-primary-container flex items-center justify-center text-on-primary text-label-sm font-bold shadow-sm">
            {workspaceInitials || "?"}
          </div>
          <div className="min-w-0">
            <p className="text-label-sm font-semibold text-on-surface truncate">{workspaceName || "Workspace"}</p>
            <p className="text-label-xs text-outline">Settings</p>
          </div>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 py-3 px-3 space-y-0.5 overflow-y-auto">
        {sections.map((section) => {
          const isActive = activeSection === section.key;
          return (
            <button
              key={section.key}
              onClick={() => onSectionChange(section.key)}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-body-sm text-left transition-all ${
                isActive
                  ? "bg-primary/10 text-primary font-semibold"
                  : "text-on-surface-variant hover:bg-surface-container hover:text-on-surface"
              }`}
            >
              <span className="material-symbols-outlined text-[18px]">{section.icon}</span>
              {section.label}
            </button>
          );
        })}
      </nav>

      {/* Back button */}
      <div className="p-3 border-t border-outline-variant/20">
        <button
          onClick={() => router.push("/dashboard")}
          className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-body-sm text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-all"
        >
          <span className="material-symbols-outlined text-[18px]">arrow_back</span>
          Back to Dashboard
        </button>
      </div>
    </aside>
  );
}
