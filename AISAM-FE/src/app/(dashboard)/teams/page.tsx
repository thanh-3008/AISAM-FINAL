"use client";

import { useMemo, useState } from "react";
import Link from "next/link";
import Header from "@/components/layout/Header";

type Team = {
  id: string;
  initials: string;
  name: string;
  description: string;
  members: number;
  brands: number;
  tone: "primary" | "secondary" | "tertiary";
};

type Member = {
  initials: string;
  name: string;
  email: string;
  role: "Owner" | "Admin" | "Editor";
  status: "Active" | "Pending";
  teams: string;
};

const teams: Team[] = [
  {
    id: "creative-alpha",
    initials: "MK",
    name: "Marketing Team",
    description: "Global marketing initiatives, campaign strategy, and budget governance.",
    members: 10,
    brands: 4,
    tone: "primary",
  },
  {
    id: "content-creators",
    initials: "CC",
    name: "Content Creators",
    description: "AI-assisted content production, design review, and channel packaging.",
    members: 13,
    brands: 2,
    tone: "secondary",
  },
  {
    id: "brand-managers",
    initials: "BM",
    name: "Brand Managers",
    description: "Brand compliance, product positioning, and asset ownership.",
    members: 4,
    brands: 6,
    tone: "tertiary",
  },
];

const members: Member[] = [
  {
    initials: "SC",
    name: "Sarah Connor",
    email: "sarah@aisam.intelligence",
    role: "Owner",
    status: "Active",
    teams: "Marketing, Admin",
  },
  {
    initials: "JD",
    name: "James Doe",
    email: "j.doe@marketing.com",
    role: "Admin",
    status: "Active",
    teams: "Content Creators",
  },
  {
    initials: "ML",
    name: "Maya Lin",
    email: "maya@design.co",
    role: "Editor",
    status: "Pending",
    teams: "Brand Managers",
  },
];

const toneStyles = {
  primary: {
    icon: "bg-primary-fixed text-primary",
    badge: "bg-primary-fixed text-primary",
    bar: "bg-primary",
  },
  secondary: {
    icon: "bg-secondary-fixed text-secondary",
    badge: "bg-secondary-fixed text-secondary",
    bar: "bg-secondary",
  },
  tertiary: {
    icon: "bg-tertiary-fixed text-tertiary",
    badge: "bg-tertiary-fixed text-tertiary",
    bar: "bg-tertiary",
  },
};

const roleStyles = {
  Owner: "bg-primary-fixed text-primary",
  Admin: "bg-secondary-fixed text-secondary",
  Editor: "bg-tertiary-fixed text-tertiary",
};

function CreateTeamModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <button
        aria-label="Close create team modal"
        className="absolute inset-0 bg-enterprise-navy/55 backdrop-blur-sm"
        onClick={onClose}
      />
      <section className="relative w-full max-w-xl overflow-hidden rounded-2xl border border-outline-variant/40 bg-surface-container-lowest shadow-2xl">
        <div className="flex items-center justify-between border-b border-outline-variant/30 px-6 py-5">
          <div>
            <h2 className="text-headline-sm font-bold text-on-surface">Create New Team</h2>
            <p className="mt-1 text-body-sm text-on-surface-variant">Set ownership, brand scope, and initial collaborators.</p>
          </div>
          <button
            className="flex h-9 w-9 items-center justify-center rounded-full text-on-surface-variant transition-all hover:bg-surface-container active:scale-95"
            onClick={onClose}
            title="Close"
          >
            <span className="material-symbols-outlined text-[20px]">close</span>
          </button>
        </div>

        <div className="max-h-[70vh] space-y-5 overflow-y-auto px-6 py-6">
          <label className="block space-y-2">
            <span className="text-label-md uppercase tracking-wider text-on-surface-variant">Team Name</span>
            <input
              className="w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-4 py-3 text-body-md outline-none transition-all placeholder:text-outline/50 focus:border-primary/60 focus:ring-2 focus:ring-primary/10"
              placeholder="e.g. Creative Explorers"
              type="text"
            />
          </label>

          <label className="block space-y-2">
            <span className="text-label-md uppercase tracking-wider text-on-surface-variant">Description</span>
            <textarea
              className="min-h-24 w-full resize-none rounded-xl border border-outline-variant/50 bg-surface-container-low px-4 py-3 text-body-md outline-none transition-all placeholder:text-outline/50 focus:border-primary/60 focus:ring-2 focus:ring-primary/10"
              placeholder="Describe this team's focus and goals..."
            />
          </label>

          <div className="space-y-2">
            <span className="text-label-md uppercase tracking-wider text-on-surface-variant">Assign Brands</span>
            <div className="flex flex-wrap gap-2 rounded-xl border border-outline-variant/50 bg-surface-container-low p-3">
              {["Lumina Tech", "Summit Outdoor"].map((brand) => (
                <span key={brand} className="inline-flex items-center gap-1 rounded-lg bg-primary-fixed px-2.5 py-1 text-[11px] font-bold text-primary">
                  {brand}
                  <button className="flex items-center" title={`Remove ${brand}`}>
                    <span className="material-symbols-outlined text-[14px]">close</span>
                  </button>
                </span>
              ))}
              <input
                className="min-w-32 flex-1 bg-transparent px-1 py-1 text-body-sm outline-none placeholder:text-outline/50"
                placeholder="Search brands..."
                type="text"
              />
            </div>
          </div>

          <label className="block space-y-2">
            <span className="text-label-md uppercase tracking-wider text-on-surface-variant">Initial Members</span>
            <div className="relative">
              <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-[20px] text-outline">search</span>
              <input
                className="w-full rounded-xl border border-outline-variant/50 bg-surface-container-low py-3 pl-10 pr-4 text-body-md outline-none transition-all placeholder:text-outline/50 focus:border-primary/60 focus:ring-2 focus:ring-primary/10"
                placeholder="Search members by name or email..."
                type="text"
              />
            </div>
          </label>

          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <label className="space-y-2">
              <span className="text-label-md uppercase tracking-wider text-on-surface-variant">Default Role</span>
              <select className="w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-4 py-3 text-body-sm outline-none focus:border-primary/60 focus:ring-2 focus:ring-primary/10">
                <option>Editor</option>
                <option>Admin</option>
                <option>Viewer</option>
              </select>
            </label>
            <label className="space-y-2">
              <span className="text-label-md uppercase tracking-wider text-on-surface-variant">Governance</span>
              <select className="w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-4 py-3 text-body-sm outline-none focus:border-primary/60 focus:ring-2 focus:ring-primary/10">
                <option>Require approvals</option>
                <option>Open publishing</option>
                <option>Admin approval only</option>
              </select>
            </label>
          </div>
        </div>

        <div className="flex flex-col-reverse gap-3 border-t border-outline-variant/30 bg-surface-container-low px-6 py-5 sm:flex-row sm:justify-end">
          <button
            className="rounded-xl px-5 py-2.5 text-label-md text-on-surface-variant transition-all hover:bg-surface-container-high active:scale-95"
            onClick={onClose}
          >
            Cancel
          </button>
          <button className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-label-md text-on-primary shadow-sm transition-all hover:opacity-90 active:scale-95">
            <span className="material-symbols-outlined text-[18px]">group_add</span>
            Create Team
          </button>
        </div>
      </section>
    </div>
  );
}

export default function TeamsPage() {
  const [createOpen, setCreateOpen] = useState(false);
  const [query, setQuery] = useState("");

  const filteredMembers = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return members;
    return members.filter((member) =>
      [member.name, member.email, member.role, member.teams].some((value) => value.toLowerCase().includes(q)),
    );
  }, [query]);

  const filteredTeams = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return teams;
    return teams.filter((team) => [team.name, team.description].some((value) => value.toLowerCase().includes(q)));
  }, [query]);

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Teams" }]} />
      <main className="h-[calc(100vh-64px)] overflow-y-auto p-4 sm:p-6 lg:p-8">
        <div className="mx-auto flex w-full max-w-7xl flex-col gap-8">
          <section className="flex flex-col gap-4 rounded-2xl border border-outline-variant/25 bg-surface-container-lowest px-5 py-5 shadow-sm sm:px-6 lg:flex-row lg:items-center lg:justify-between">
            <div className="flex items-start gap-4">
              <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-primary-fixed text-primary">
                <span className="material-symbols-outlined text-[24px]">group</span>
              </div>
              <div>
                <h1 className="text-headline-md font-bold text-on-surface sm:text-headline-lg">Teams & Collaboration</h1>
                <p className="mt-1 max-w-2xl text-body-sm text-on-surface-variant sm:text-body-md">
                  Manage organizational hierarchy, team roles, and collaborative brand environments.
                </p>
              </div>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row">
              <button
                className="inline-flex items-center justify-center gap-2 rounded-xl border border-primary/50 px-4 py-2.5 text-label-md text-primary transition-all hover:bg-primary-fixed active:scale-95"
                onClick={() => setCreateOpen(true)}
              >
                <span className="material-symbols-outlined text-[18px]">add_circle</span>
                Create Team
              </button>
              <button className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-label-md text-on-primary shadow-sm transition-all hover:opacity-90 active:scale-95">
                <span className="material-symbols-outlined text-[18px]">person_add</span>
                Invite Member
              </button>
            </div>
          </section>

          <section className="grid grid-cols-1 gap-4 md:grid-cols-3">
            {[
              { label: "Total Members", value: "24", icon: "group", iconClass: "bg-primary-fixed text-primary" },
              { label: "Active Teams", value: "6", icon: "schema", iconClass: "bg-secondary-fixed text-secondary" },
              { label: "Pending Invites", value: "3", icon: "mail", iconClass: "bg-tertiary-fixed text-tertiary" },
            ].map((item) => (
              <div key={item.label} className="rounded-2xl border border-outline-variant/25 bg-surface-container-lowest p-5 shadow-sm">
                <div className="flex items-center gap-4">
                  <div className={`flex h-12 w-12 items-center justify-center rounded-xl ${item.iconClass}`}>
                    <span className="material-symbols-outlined text-[24px]">{item.icon}</span>
                  </div>
                  <div>
                    <p className="text-label-md text-on-surface-variant">{item.label}</p>
                    <p className="text-headline-md font-bold text-on-surface">{item.value}</p>
                  </div>
                </div>
              </div>
            ))}
          </section>

          <section className="flex flex-col gap-4">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <h2 className="text-headline-sm font-bold text-on-surface">Teams</h2>
              <div className="relative w-full sm:max-w-sm">
                <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-[20px] text-outline">search</span>
                <input
                  className="w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest py-2.5 pl-10 pr-4 text-body-sm outline-none transition-all placeholder:text-outline/50 focus:border-primary/60 focus:ring-2 focus:ring-primary/10"
                  onChange={(event) => setQuery(event.target.value)}
                  placeholder="Search teams, members, or brands..."
                  type="text"
                  value={query}
                />
              </div>
            </div>

            <div className="grid grid-cols-1 gap-5 lg:grid-cols-3">
              {filteredTeams.map((team) => {
                const styles = toneStyles[team.tone];
                return (
                  <Link key={team.name} href={`/teams/${team.id}`} className="group overflow-hidden rounded-2xl border border-outline-variant/25 bg-surface-container-lowest shadow-sm transition-all hover:-translate-y-1 hover:shadow-lg">
                    <div className={`h-1.5 ${styles.bar}`} />
                    <div className="p-5">
                      <div className="mb-4 flex items-start justify-between">
                        <div className={`flex h-11 w-11 items-center justify-center rounded-xl text-label-md font-extrabold ${styles.icon}`}>
                          {team.initials}
                        </div>
                        <button className="flex h-9 w-9 items-center justify-center rounded-full text-on-surface-variant opacity-100 transition-all hover:bg-surface-container sm:opacity-0 sm:group-hover:opacity-100" title="Team actions">
                          <span className="material-symbols-outlined text-[20px]">arrow_forward</span>
                        </button>
                      </div>
                      <h3 className="text-headline-sm font-bold text-on-surface">{team.name}</h3>
                      <p className="mt-1 line-clamp-2 min-h-10 text-body-sm text-on-surface-variant">{team.description}</p>
                      <div className="mt-5 flex items-center justify-between border-t border-outline-variant/20 pt-4">
                        <div className="flex items-center gap-2 text-label-sm text-on-surface-variant">
                          <span className="material-symbols-outlined text-[17px]">groups</span>
                          {team.members} members
                        </div>
                        <span className={`rounded-lg px-2.5 py-1 text-[11px] font-bold ${styles.badge}`}>{team.brands} Brands</span>
                      </div>
                    </div>
                  </Link>
                );
              })}
            </div>
          </section>

          <section className="overflow-hidden rounded-2xl border border-outline-variant/25 bg-surface-container-lowest shadow-sm">
            <div className="flex flex-col gap-3 border-b border-outline-variant/25 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Member Management</h2>
                <p className="text-body-sm text-on-surface-variant">Review roles, invite status, and assigned teams.</p>
              </div>
              <button className="inline-flex h-10 w-10 items-center justify-center rounded-xl border border-outline-variant/40 text-on-surface-variant transition-all hover:bg-surface-container" title="Filter members">
                <span className="material-symbols-outlined text-[20px]">filter_list</span>
              </button>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full min-w-[760px] text-left">
                <thead>
                  <tr className="border-b border-outline-variant/20 bg-surface-container text-label-md text-on-surface-variant">
                    <th className="px-5 py-4 font-bold">Member</th>
                    <th className="px-5 py-4 font-bold">Role</th>
                    <th className="px-5 py-4 font-bold">Status</th>
                    <th className="px-5 py-4 font-bold">Assigned Teams</th>
                    <th className="px-5 py-4 text-right font-bold">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-outline-variant/20">
                  {filteredMembers.map((member) => (
                    <tr key={member.email} className="transition-colors hover:bg-primary-fixed/20">
                      <td className="px-5 py-4">
                        <div className="flex items-center gap-3">
                          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-surface-container-high text-label-md font-extrabold text-primary">
                            {member.initials}
                          </div>
                          <div>
                            <p className="text-body-sm font-bold text-on-surface">{member.name}</p>
                            <p className="text-label-sm text-on-surface-variant">{member.email}</p>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-4">
                        <span className={`rounded-full px-2.5 py-1 text-[10px] font-bold uppercase tracking-wider ${roleStyles[member.role]}`}>
                          {member.role}
                        </span>
                      </td>
                      <td className="px-5 py-4">
                        <span className="inline-flex items-center gap-2 text-body-sm text-on-surface">
                          <span className={`h-2 w-2 rounded-full ${member.status === "Active" ? "bg-success-green" : "bg-warning-amber"}`} />
                          {member.status}
                        </span>
                      </td>
                      <td className="px-5 py-4 text-body-sm text-on-surface-variant">{member.teams}</td>
                      <td className="px-5 py-4">
                        <div className="flex justify-end gap-1">
                          <button className="flex h-8 w-8 items-center justify-center rounded-full text-on-surface-variant transition-all hover:bg-primary-fixed hover:text-primary" title={member.status === "Pending" ? "Resend invite" : "Edit member"}>
                            <span className="material-symbols-outlined text-[18px]">{member.status === "Pending" ? "mail" : "edit"}</span>
                          </button>
                          <button className="flex h-8 w-8 items-center justify-center rounded-full text-on-surface-variant transition-all hover:bg-error-container hover:text-danger-red" title={member.status === "Pending" ? "Cancel invite" : "Remove member"}>
                            <span className="material-symbols-outlined text-[18px]">{member.status === "Pending" ? "close" : "delete"}</span>
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="flex flex-col gap-3 border-t border-outline-variant/20 px-5 py-4 text-label-sm text-on-surface-variant sm:flex-row sm:items-center sm:justify-between">
              <span>Showing {filteredMembers.length} of 24 members</span>
              <div className="flex gap-2">
                <button className="rounded-lg border border-outline-variant/40 px-3 py-1.5 opacity-50" disabled>Previous</button>
                <button className="rounded-lg border border-outline-variant/40 px-3 py-1.5 transition-all hover:bg-surface-container">Next</button>
              </div>
            </div>
          </section>

          <section className="overflow-hidden rounded-2xl bg-enterprise-navy text-white shadow-sm">
            <div className="grid gap-6 p-6 md:grid-cols-[1fr_auto] md:items-center lg:p-8">
              <div>
                <div className="mb-3 inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/10 px-3 py-1 text-label-sm text-primary-fixed-dim">
                  <span className="material-symbols-outlined text-[16px]">auto_awesome</span>
                  Enterprise Intelligence
                </div>
                <h2 className="text-headline-md font-bold">Advanced Workflows Coming Soon</h2>
                <p className="mt-2 max-w-2xl text-body-md text-white/70">
                  Approval SLAs, automated delegation, and multi-brand governance are being prepared for larger operations.
                </p>
                <div className="mt-5 flex flex-wrap gap-2">
                  {["Approval SLA Tracking", "Smart Delegation AI", "Regional Governance"].map((item) => (
                    <span key={item} className="inline-flex items-center gap-2 rounded-lg border border-white/10 bg-white/10 px-3 py-1.5 text-label-sm">
                      <span className="material-symbols-outlined text-[15px] text-success-green">check_circle</span>
                      {item}
                    </span>
                  ))}
                </div>
              </div>
              <button className="inline-flex items-center justify-center gap-2 rounded-xl bg-white px-5 py-3 text-label-md font-bold text-enterprise-navy transition-all hover:bg-primary-fixed active:scale-95">
                Request Beta Access
                <span className="material-symbols-outlined text-[18px]">arrow_forward</span>
              </button>
            </div>
          </section>
        </div>
      </main>

      <CreateTeamModal open={createOpen} onClose={() => setCreateOpen(false)} />
    </>
  );
}
