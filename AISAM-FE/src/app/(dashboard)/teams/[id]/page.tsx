"use client";

import { useMemo, useState } from "react";
import Link from "next/link";
import Header from "@/components/layout/Header";

type TeamMember = {
  initials: string;
  name: string;
  email: string;
  role: "Leader" | "Manager" | "Member";
  brands: string[];
  lastActive: string;
};

const team = {
  name: "Creative Alpha",
  status: "Active",
  description: "Manage team members, roles, and assigned brand portfolios for the Creative Alpha cohort.",
  stats: [
    { label: "Members", value: "12", icon: "groups", color: "bg-primary-fixed text-primary" },
    { label: "Managed Brands", value: "3", icon: "category", color: "bg-secondary-fixed text-secondary" },
    { label: "Permission Sets", value: "5", icon: "admin_panel_settings", color: "bg-tertiary-fixed text-tertiary" },
  ],
};

const permissionHighlights = [
  {
    icon: "check_circle",
    iconClass: "text-success-green",
    title: "Can Generate",
    description: "Create AI assets and campaigns",
  },
  {
    icon: "check_circle",
    iconClass: "text-success-green",
    title: "Can Approve",
    description: "Review and approve internal drafts",
  },
  {
    icon: "cancel",
    iconClass: "text-danger-red",
    title: "Cannot Publish",
    description: "Requires Admin sign-off for live deployment",
  },
];

const brands = [
  { initials: "O", name: "OmniCorp" },
  { initials: "Z", name: "Zephyr Tech" },
  { initials: "V", name: "Vanguard" },
];

const members: TeamMember[] = [
  {
    initials: "SC",
    name: "Sarah Chen",
    email: "sarah.c@example.com",
    role: "Leader",
    brands: ["All"],
    lastActive: "2 hours ago",
  },
  {
    initials: "MJ",
    name: "Marcus Johnson",
    email: "marcus.j@example.com",
    role: "Manager",
    brands: ["O", "Z"],
    lastActive: "Yesterday",
  },
  {
    initials: "EL",
    name: "Elena Lopez",
    email: "elena.l@example.com",
    role: "Member",
    brands: ["V"],
    lastActive: "3 days ago",
  },
];

const roleStyles = {
  Leader: "bg-primary-fixed text-primary",
  Manager: "bg-secondary-fixed text-secondary",
  Member: "bg-surface-container-highest text-on-surface border border-outline-variant/30",
};

function BrandStack({ brands: assignedBrands }: { brands: string[] }) {
  if (assignedBrands.includes("All")) {
    return <span className="text-body-sm font-medium text-on-surface">All Brands</span>;
  }

  return (
    <div className="flex -space-x-2">
      {assignedBrands.map((brand, index) => (
        <span
          key={brand}
          className="flex h-7 w-7 items-center justify-center rounded-full border-2 border-surface-container-lowest bg-surface-container-highest text-[10px] font-bold text-on-surface"
          style={{ zIndex: assignedBrands.length - index }}
        >
          {brand}
        </span>
      ))}
    </div>
  );
}

export default function TeamDetailPage() {
  const [query, setQuery] = useState("");

  const filteredMembers = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return members;
    return members.filter((member) =>
      [member.name, member.email, member.role, member.lastActive, ...member.brands].some((value) =>
        value.toLowerCase().includes(q),
      ),
    );
  }, [query]);

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Teams", href: "/teams" }, { label: team.name }]} />
      <main className="h-[calc(100vh-64px)] overflow-y-auto bg-surface p-4 sm:p-6 lg:p-8">
        <div className="mx-auto flex w-full max-w-7xl flex-col gap-8">
          <section className="flex flex-col gap-5 rounded-2xl border border-outline-variant/25 bg-surface-container-lowest p-5 shadow-sm sm:p-6 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <div className="mb-3 flex flex-wrap items-center gap-2 text-body-sm text-on-surface-variant">
                <Link className="transition-colors hover:text-primary" href="/teams">Teams</Link>
                <span className="material-symbols-outlined text-[16px]">chevron_right</span>
                <span className="text-on-surface">{team.name}</span>
              </div>
              <div className="flex flex-wrap items-center gap-3">
                <h1 className="text-headline-md font-bold text-on-surface sm:text-headline-lg">{team.name}</h1>
                <span className="inline-flex items-center gap-1.5 rounded-full border border-primary/20 bg-primary-fixed px-2.5 py-1 text-label-sm font-bold text-primary">
                  <span className="h-1.5 w-1.5 rounded-full bg-primary" />
                  {team.status}
                </span>
              </div>
              <p className="mt-2 max-w-2xl text-body-md text-on-surface-variant">{team.description}</p>
            </div>
            <div className="flex flex-col gap-2 sm:flex-row">
              <button className="inline-flex items-center justify-center gap-2 rounded-xl border border-outline-variant/40 bg-surface-container-lowest px-4 py-2.5 text-label-md text-on-surface transition-all hover:bg-surface-container active:scale-95">
                <span className="material-symbols-outlined text-[18px]">edit</span>
                Edit Details
              </button>
              <div className="group relative">
                <button disabled className="inline-flex w-full cursor-not-allowed items-center justify-center gap-2 rounded-xl border border-outline-variant/40 bg-surface-dim px-4 py-2.5 text-label-md text-on-surface-variant opacity-60">
                  <span className="material-symbols-outlined text-[18px]">swap_horiz</span>
                  Transfer Ownership
                </button>
                <div className="pointer-events-none absolute bottom-full left-1/2 mb-2 w-max -translate-x-1/2 rounded-lg bg-inverse-surface px-3 py-1.5 text-label-sm text-inverse-on-surface opacity-0 shadow-lg transition-opacity group-hover:opacity-100">
                  Backend integration pending
                </div>
              </div>
            </div>
          </section>

          <section className="grid grid-cols-1 gap-4 md:grid-cols-3">
            {team.stats.map((stat) => (
              <div key={stat.label} className="rounded-2xl border border-outline-variant/25 bg-surface-container-lowest p-5 shadow-sm">
                <div className="flex items-center gap-4">
                  <div className={`flex h-12 w-12 items-center justify-center rounded-xl ${stat.color}`}>
                    <span className="material-symbols-outlined text-[24px]">{stat.icon}</span>
                  </div>
                  <div>
                    <p className="text-label-md text-on-surface-variant">{stat.label}</p>
                    <p className="text-headline-md font-bold text-on-surface">{stat.value}</p>
                  </div>
                </div>
              </div>
            ))}
          </section>

          <section className="grid grid-cols-1 gap-6 lg:grid-cols-3">
            <div className="relative overflow-hidden rounded-2xl border border-outline-variant/25 bg-surface-container-lowest p-6 shadow-sm">
              <div className="pointer-events-none absolute right-0 top-0 h-32 w-32 translate-x-1/2 -translate-y-1/2 rounded-full bg-primary/5 blur-2xl" />
              <h2 className="mb-5 flex items-center gap-2 text-headline-sm font-bold text-on-surface">
                <span className="material-symbols-outlined text-primary">admin_panel_settings</span>
                Permission Highlights
              </h2>
              <div className="space-y-4">
                {permissionHighlights.map((item) => (
                  <div key={item.title} className="flex items-start gap-3">
                    <span className={`material-symbols-outlined mt-0.5 text-[20px] ${item.iconClass}`}>{item.icon}</span>
                    <div>
                      <p className="text-body-sm font-bold text-on-surface">{item.title}</p>
                      <p className="text-label-sm text-on-surface-variant">{item.description}</p>
                    </div>
                  </div>
                ))}
              </div>
              <button className="mt-5 inline-flex items-center gap-1 text-label-md text-primary transition-colors hover:text-primary-container">
                View Full Matrix
                <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
              </button>
            </div>

            <div className="rounded-2xl border border-outline-variant/25 bg-surface-container-lowest p-6 shadow-sm lg:col-span-2">
              <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <h2 className="flex items-center gap-2 text-headline-sm font-bold text-on-surface">
                  <span className="material-symbols-outlined text-secondary">category</span>
                  Managed Brands
                </h2>
                <button className="inline-flex items-center gap-1.5 text-label-md text-primary transition-colors hover:text-primary-container">
                  <span className="material-symbols-outlined text-[18px]">add</span>
                  Assign Brand
                </button>
              </div>
              <p className="mb-5 text-body-sm text-on-surface-variant">
                This team is authorized to generate content and manage campaigns for the following brand portfolios.
              </p>
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                {brands.map((brand) => (
                  <button key={brand.name} className="group flex flex-col items-center rounded-xl border border-outline-variant/40 p-4 transition-all hover:bg-surface-container">
                    <span className="mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-surface-container-highest text-headline-sm font-bold text-on-surface transition-transform group-hover:scale-105">
                      {brand.initials}
                    </span>
                    <span className="text-center text-label-md text-on-surface">{brand.name}</span>
                  </button>
                ))}
                <button className="flex flex-col items-center justify-center rounded-xl border border-dashed border-outline-variant/60 p-4 text-on-surface-variant opacity-80 transition-all hover:bg-surface-container hover:opacity-100">
                  <span className="mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-surface-variant">
                    <span className="material-symbols-outlined">add</span>
                  </span>
                  <span className="text-center text-label-md">Add New</span>
                </button>
              </div>
            </div>
          </section>

          <section className="overflow-hidden rounded-2xl border border-outline-variant/25 bg-surface-container-lowest shadow-sm">
            <div className="flex flex-col gap-4 border-b border-outline-variant/25 bg-surface-bright/60 px-5 py-5 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Team Members</h2>
                <p className="mt-1 text-body-sm text-on-surface-variant">Manage individual access and roles within this team.</p>
              </div>
              <div className="flex flex-col gap-2 sm:flex-row">
                <div className="relative">
                  <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-[18px] text-outline">search</span>
                  <input
                    className="w-full rounded-xl border border-outline-variant/40 bg-surface py-2 pl-9 pr-4 text-body-sm outline-none transition-all placeholder:text-outline/50 focus:border-primary/60 focus:ring-2 focus:ring-primary/10 sm:w-64"
                    onChange={(event) => setQuery(event.target.value)}
                    placeholder="Search members..."
                    type="text"
                    value={query}
                  />
                </div>
                <button className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-4 py-2 text-label-md text-on-primary transition-all hover:bg-primary-container active:scale-95">
                  <span className="material-symbols-outlined text-[18px]">person_add</span>
                  Add Member
                </button>
              </div>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full min-w-[820px] text-left">
                <thead className="bg-surface-container">
                  <tr className="border-b border-outline-variant/25 text-label-md text-on-surface-variant">
                    <th className="px-5 py-4 font-bold">Name</th>
                    <th className="px-5 py-4 font-bold">Role</th>
                    <th className="px-5 py-4 font-bold">Assigned Brands</th>
                    <th className="px-5 py-4 font-bold">Last Active</th>
                    <th className="px-5 py-4 text-right font-bold">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-variant">
                  {filteredMembers.map((member) => (
                    <tr key={member.email} className="group transition-colors hover:bg-surface-container/50">
                      <td className="px-5 py-4">
                        <div className="flex items-center gap-3">
                          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-surface-dim text-label-md font-bold text-on-surface-variant">
                            {member.initials}
                          </div>
                          <div>
                            <p className="text-body-md font-semibold text-on-surface">{member.name}</p>
                            <p className="text-body-sm text-on-surface-variant">{member.email}</p>
                          </div>
                        </div>
                      </td>
                      <td className="px-5 py-4">
                        <span className={`inline-flex rounded-full px-2.5 py-1 text-label-sm font-bold ${roleStyles[member.role]}`}>
                          {member.role}
                        </span>
                      </td>
                      <td className="px-5 py-4">
                        <BrandStack brands={member.brands} />
                      </td>
                      <td className="px-5 py-4 text-body-sm text-on-surface-variant">{member.lastActive}</td>
                      <td className="px-5 py-4">
                        <div className="flex justify-end">
                          <button className="flex h-8 w-8 items-center justify-center rounded-lg text-on-surface-variant opacity-100 transition-all hover:bg-surface-dim hover:text-primary sm:opacity-0 sm:group-hover:opacity-100" title="Member actions">
                            <span className="material-symbols-outlined text-[20px]">more_vert</span>
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          <section className="border-t border-outline-variant/30 pt-8">
            <h2 className="mb-4 text-headline-sm font-bold text-on-surface">Pending Invites</h2>
            <div className="flex flex-col items-center justify-center rounded-2xl border border-dashed border-outline-variant bg-surface-container-lowest px-6 py-14 text-center shadow-sm">
              <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-surface-container text-on-surface-variant">
                <span className="material-symbols-outlined text-[32px]">group_off</span>
              </div>
              <h3 className="text-headline-sm font-bold text-on-surface">No members added yet</h3>
              <p className="mt-2 max-w-sm text-body-sm text-on-surface-variant">
                You haven&apos;t invited anyone to this team yet. Invite members to start collaborating on campaigns.
              </p>
              <button className="mt-5 inline-flex items-center gap-2 rounded-xl bg-primary-container px-4 py-2.5 text-label-md text-on-primary-container transition-all hover:bg-primary hover:text-on-primary active:scale-95">
                <span className="material-symbols-outlined text-[18px]">mail</span>
                Send Invites
              </button>
            </div>
          </section>
        </div>
      </main>
    </>
  );
}
