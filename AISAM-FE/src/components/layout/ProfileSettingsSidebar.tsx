"use client";

import { useRouter } from "next/navigation";

export type ProfileSection = "my-profile" | "team" | "security" | "billing" | "subscription";

const sections: { key: ProfileSection; label: string; icon: string }[] = [
  { key: "my-profile", label: "My Profile", icon: "person" },
  { key: "team", label: "Team", icon: "group" },
  { key: "security", label: "Security", icon: "lock" },
  { key: "billing", label: "Billing & Quota", icon: "credit_card" },
  { key: "subscription", label: "Subscription", icon: "workspace_premium" },
];

interface ProfileSettingsSidebarProps {
  activeSection: ProfileSection;
  onSectionChange: (section: ProfileSection) => void;
  profileName?: string;
  profileInitials?: string;
}

export default function ProfileSettingsSidebar({
  activeSection,
  onSectionChange,
  profileName,
  profileInitials,
}: ProfileSettingsSidebarProps) {
  const router = useRouter();

  return (
    <aside className="w-64 shrink-0 border-r border-outline-variant/30 bg-surface-container-low/50 flex flex-col">
      {/* Profile Info */}
      <div className="p-5 border-b border-outline-variant/20">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary/10 to-primary/5 flex items-center justify-center text-primary font-bold text-body-sm">
            {profileInitials || "?"}
          </div>
          <div className="min-w-0 flex-1">
            <p className="text-body-sm font-semibold text-on-surface truncate">{profileName || "Profile"}</p>
            <p className="text-label-sm text-outline">Settings</p>
          </div>
        </div>
      </div>

      {/* Navigation */}
      <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
        {sections.map((sec) => {
          const isActive = activeSection === sec.key;
          return (
            <button
              key={sec.key}
              onClick={() => onSectionChange(sec.key)}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-body-sm font-semibold transition-all text-left ${
                isActive
                  ? "bg-primary/10 text-primary shadow-sm"
                  : "text-on-surface-variant hover:bg-surface-container hover:text-on-surface"
              }`}
            >
              <span className={`material-symbols-outlined text-[20px] ${isActive ? "text-primary" : ""}`}>
                {sec.icon}
              </span>
              <span>{sec.label}</span>
            </button>
          );
        })}
      </nav>

      {/* Back link */}
      <div className="p-3 border-t border-outline-variant/20">
        <button
          onClick={() => router.push("/profiles")}
          className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-body-sm font-semibold text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-all"
        >
          <span className="material-symbols-outlined text-[20px]">arrow_back</span>
          <span>All Profiles</span>
        </button>
      </div>
    </aside>
  );
}
