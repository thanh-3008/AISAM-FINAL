"use client";

import { useMemo, useState, useEffect } from "react";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import { fetchWorkspaceMembers, transferWorkspaceOwnership, type WorkspaceMember, type WorkspaceMemberRole } from "@/services/workspaceService";

function getRoleBadge(role: WorkspaceMemberRole) {
  switch (role) {
    case "Owner":
      return { label: "Owner", color: "text-primary", bg: "bg-primary-fixed", icon: "star" };
    case "Manager":
      return { label: "Manager", color: "text-secondary", bg: "bg-secondary-fixed", icon: "manage_accounts" };
    case "ContentCreator":
      return { label: "Content Creator", color: "text-tertiary", bg: "bg-tertiary-fixed", icon: "edit_note" };
    case "Viewer":
      return { label: "Viewer", color: "text-outline", bg: "bg-surface-container", icon: "visibility" };
    default:
      return { label: role, color: "text-outline", bg: "bg-surface-container", icon: "person" };
  }
}

function getInitials(name: string): string {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

function InviteMemberModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <button
        aria-label="Close modal"
        className="absolute inset-0 bg-enterprise-navy/55 backdrop-blur-sm"
        onClick={onClose}
      />
      <section className="relative w-full max-w-md overflow-hidden rounded-2xl border border-outline-variant/40 bg-surface-container-lowest shadow-2xl">
        <div className="flex items-center justify-between border-b border-outline-variant/30 px-6 py-5">
          <div>
            <h2 className="text-headline-sm font-bold text-on-surface">Invite Member</h2>
            <p className="mt-1 text-body-sm text-on-surface-variant">Add a new collaborator to this workspace.</p>
          </div>
          <button
            className="flex h-9 w-9 items-center justify-center rounded-full text-on-surface-variant transition-all hover:bg-surface-container active:scale-95"
            onClick={onClose}
          >
            <span className="material-symbols-outlined text-[20px]">close</span>
          </button>
        </div>

        <div className="space-y-5 px-6 py-6">
          <label className="block space-y-2">
            <span className="text-label-md uppercase tracking-wider text-on-surface-variant">Email Address</span>
            <input
              type="email"
              placeholder="colleague@company.com"
              className="w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-4 py-3 text-body-md outline-none transition-all placeholder:text-outline/50 focus:border-primary/60 focus:ring-2 focus:ring-primary/10"
            />
          </label>
          <label className="block space-y-2">
            <span className="text-label-md uppercase tracking-wider text-on-surface-variant">Role</span>
            <select className="w-full rounded-xl border border-outline-variant/50 bg-surface-container-low px-4 py-3 text-body-md outline-none focus:border-primary/60 focus:ring-2 focus:ring-primary/10">
              <option value="Manager">Manager</option>
              <option value="ContentCreator">Content Creator</option>
              <option value="Viewer">Viewer</option>
            </select>
          </label>
        </div>

        <div className="flex flex-col-reverse gap-3 border-t border-outline-variant/30 bg-surface-container-low px-6 py-5 sm:flex-row sm:justify-end">
          <button
            className="rounded-xl px-5 py-2.5 text-label-md text-on-surface-variant transition-all hover:bg-surface-container-high active:scale-95"
            onClick={onClose}
          >
            Cancel
          </button>
          <button
            className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-label-md text-on-primary shadow-sm transition-all hover:opacity-90 active:scale-95"
            onClick={onClose}
          >
            <span className="material-symbols-outlined text-[18px]">send</span>
            Send Invite
          </button>
        </div>
      </section>
    </div>
  );
}

export default function TeamsPage() {
  const { activeWorkspace } = useWorkspaces();
  const featureGate = useFeatureGate();
  const [inviteOpen, setInviteOpen] = useState(false);
  const [query, setQuery] = useState("");
  const [members, setMembers] = useState<WorkspaceMember[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadMembers = async () => {
      setLoading(true);
      try {
        const data = await fetchWorkspaceMembers();
        if (data) {
          setMembers(data.data);
        }
      } catch (error) {
        console.error("Failed to load workspace members:", error);
      } finally {
        setLoading(false);
      }
    };
    loadMembers();
  }, [activeWorkspace?.id]);

  const handleTransferOwnership = async (memberId: string, memberName: string) => {
    if (!window.confirm(`Are you sure you want to transfer ownership of this workspace to ${memberName}? You will lose owner privileges and become a Manager.`)) return;
    
    try {
      const success = await transferWorkspaceOwnership(memberId);
      if (success) {
        window.location.reload();
      } else {
        alert("Failed to transfer ownership. Please try again.");
      }
    } catch {
      alert("An error occurred while transferring ownership.");
    }
  };

  const filteredMembers = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return members;
    return members.filter((member) =>
      [member.name, member.email, member.role].some((value) => value.toLowerCase().includes(q))
    );
  }, [query, members]);

  if (!featureGate.canAccess("teamManagement")) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Teams" }]} />
        <main className="h-[calc(100vh-64px)] overflow-y-auto p-4 sm:p-6 lg:p-8">
          <div className="mx-auto flex w-full max-w-7xl flex-col gap-8">
            <div className="mx-auto mt-12 flex min-h-[50vh] max-w-md flex-col items-center justify-center text-center">
              <div className="mb-6 flex h-20 w-20 items-center justify-center rounded-2xl bg-outline/10">
                <span className="material-symbols-outlined text-[40px] text-outline">lock</span>
              </div>
              <h2 className="mb-2 text-headline-md font-bold text-on-surface">Team Management Locked</h2>
              <p className="mb-6 text-body-md text-on-surface-variant">
                This feature requires a <strong>Business plan</strong> or higher. Upgrade your workspace to manage team members and collaborative workflows.
              </p>
              <Link href="/pricing" className="inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-3 text-label-md font-bold text-on-primary transition-all hover:scale-105">
                Upgrade Workspace
                <span className="material-symbols-outlined text-[18px]">arrow_forward</span>
              </Link>
            </div>
          </div>
        </main>
      </>
    );
  }

  const isOwner = members.some((m) => m.role === "Owner");

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Teams" }]} />
      <main className="h-[calc(100vh-64px)] overflow-y-auto p-4 sm:p-6 lg:p-8">
        <div className="mx-auto flex w-full max-w-7xl flex-col gap-8">
          {/* Header Banner */}
          <section className="flex flex-col gap-4 rounded-2xl border border-outline-variant/25 bg-surface-container-lowest px-5 py-5 shadow-sm sm:px-6 lg:flex-row lg:items-center lg:justify-between">
            <div className="flex items-start gap-4">
              <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-primary-fixed text-primary">
                <span className="material-symbols-outlined text-[24px]">group</span>
              </div>
              <div>
                <h1 className="text-headline-md font-bold text-on-surface sm:text-headline-lg">Teams & Collaboration</h1>
                <p className="mt-1 max-w-2xl text-body-sm text-on-surface-variant sm:text-body-md">
                  Manage organizational roles, invite collaborators, and scale your AI workflows.
                </p>
              </div>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row">
              <button
                className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-label-md text-on-primary shadow-sm transition-all hover:opacity-90 active:scale-95"
                onClick={() => setInviteOpen(true)}
              >
                <span className="material-symbols-outlined text-[18px]">person_add</span>
                Invite Member
              </button>
            </div>
          </section>

          {/* Stats */}
          <section className="grid grid-cols-1 gap-4 md:grid-cols-3">
            {[
              { label: "Total Members", value: members.length.toString(), icon: "group", iconClass: "bg-primary-fixed text-primary" },
              { label: "Workspace Name", value: activeWorkspace?.name || "-", icon: "workspaces", iconClass: "bg-secondary-fixed text-secondary" },
              { label: "Pending Invites", value: "0", icon: "mail", iconClass: "bg-tertiary-fixed text-tertiary" },
            ].map((item) => (
              <div key={item.label} className="rounded-2xl border border-outline-variant/25 bg-surface-container-lowest p-5 shadow-sm">
                <div className="flex items-center gap-4">
                  <div className={`flex h-12 w-12 items-center justify-center rounded-xl ${item.iconClass}`}>
                    <span className="material-symbols-outlined text-[24px]">{item.icon}</span>
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="text-label-md text-on-surface-variant">{item.label}</p>
                    <p className="truncate text-headline-sm font-bold text-on-surface">{item.value}</p>
                  </div>
                </div>
              </div>
            ))}
          </section>

          {/* Members Table */}
          <section className="flex flex-col gap-4">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Member Management</h2>
                <p className="text-body-sm text-on-surface-variant">Review roles, invite status, and access levels.</p>
              </div>
              <div className="relative w-full sm:max-w-sm">
                <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-[20px] text-outline">search</span>
                <input
                  className="w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest py-2.5 pl-10 pr-4 text-body-sm outline-none transition-all placeholder:text-outline/50 focus:border-primary/60 focus:ring-2 focus:ring-primary/10"
                  onChange={(event) => setQuery(event.target.value)}
                  placeholder="Search members by name or email..."
                  type="text"
                  value={query}
                />
              </div>
            </div>

            <div className="overflow-hidden rounded-2xl border border-outline-variant/25 bg-surface-container-lowest shadow-sm">
              <div className="overflow-x-auto">
                <table className="w-full min-w-[760px] text-left">
                  <thead>
                    <tr className="border-b border-outline-variant/20 bg-surface-container text-label-md text-on-surface-variant">
                      <th className="px-5 py-4 font-bold">Member</th>
                      <th className="px-5 py-4 font-bold">Role</th>
                      <th className="px-5 py-4 font-bold">Status</th>
                      <th className="px-5 py-4 text-right font-bold">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-outline-variant/20">
                    {loading ? (
                      [1, 2, 3].map((i) => (
                        <tr key={i}>
                          <td className="px-5 py-4">
                            <div className="flex animate-pulse items-center gap-3">
                              <div className="h-10 w-10 rounded-full bg-surface-container" />
                              <div className="space-y-2">
                                <div className="h-4 w-32 rounded bg-surface-container" />
                                <div className="h-3 w-48 rounded bg-surface-container" />
                              </div>
                            </div>
                          </td>
                          <td className="px-5 py-4"><div className="h-6 w-20 animate-pulse rounded-full bg-surface-container" /></td>
                          <td className="px-5 py-4"><div className="h-6 w-20 animate-pulse rounded-full bg-surface-container" /></td>
                          <td className="px-5 py-4"><div className="h-8 w-16 animate-pulse rounded bg-surface-container ml-auto" /></td>
                        </tr>
                      ))
                    ) : filteredMembers.length === 0 ? (
                      <tr>
                        <td colSpan={4} className="px-5 py-12 text-center text-body-md text-on-surface-variant">
                          No members found matching your search.
                        </td>
                      </tr>
                    ) : (
                      filteredMembers.map((member) => {
                        const badge = getRoleBadge(member.role);
                        return (
                          <tr key={member.id} className="transition-colors hover:bg-surface-container/30">
                            <td className="px-5 py-4">
                              <div className="flex items-center gap-3">
                                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10 text-label-md font-extrabold text-primary">
                                  {getInitials(member.name)}
                                </div>
                                <div>
                                  <div className="flex items-center gap-2">
                                    <p className="text-body-sm font-bold text-on-surface">{member.name}</p>
                                    {member.role === "Owner" && (
                                      <span className="material-symbols-outlined text-[16px] text-amber-500">star</span>
                                    )}
                                  </div>
                                  <p className="text-label-sm text-on-surface-variant">{member.email}</p>
                                  {member.joinedAt && (
                                    <p className="mt-0.5 text-label-xs text-outline">Joined: {new Date(member.joinedAt).toLocaleDateString()}</p>
                                  )}
                                </div>
                              </div>
                            </td>
                            <td className="px-5 py-4">
                              <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[11px] font-bold ${badge.bg} ${badge.color}`}>
                                <span className="material-symbols-outlined text-[14px]">{badge.icon}</span>
                                {badge.label}
                              </span>
                            </td>
                            <td className="px-5 py-4">
                              <span className="inline-flex items-center gap-2 text-body-sm text-on-surface">
                                <span className="h-2 w-2 rounded-full bg-success-green animate-pulse" />
                                Active
                              </span>
                            </td>
                            <td className="px-5 py-4">
                              <div className="flex justify-end gap-1">
                                {isOwner && member.role !== "Owner" && (
                                  <>
                                    <button 
                                      onClick={() => handleTransferOwnership(member.id, member.name)}
                                      className="flex h-8 w-8 items-center justify-center rounded-full text-on-surface-variant transition-all hover:bg-amber-100 hover:text-amber-600" 
                                      title="Transfer Ownership"
                                    >
                                      <span className="material-symbols-outlined text-[18px]">stars</span>
                                    </button>
                                    <button className="flex h-8 w-8 items-center justify-center rounded-full text-on-surface-variant transition-all hover:bg-primary-fixed hover:text-primary" title="Edit role">
                                      <span className="material-symbols-outlined text-[18px]">edit</span>
                                    </button>
                                    <button className="flex h-8 w-8 items-center justify-center rounded-full text-on-surface-variant transition-all hover:bg-error-container hover:text-danger-red" title="Remove member">
                                      <span className="material-symbols-outlined text-[18px]">delete</span>
                                    </button>
                                  </>
                                )}
                              </div>
                            </td>
                          </tr>
                        );
                      })
                    )}
                  </tbody>
                </table>
              </div>
              <div className="flex flex-col gap-3 border-t border-outline-variant/20 px-5 py-4 text-label-sm text-on-surface-variant sm:flex-row sm:items-center sm:justify-between">
                <span>Showing {filteredMembers.length} members</span>
              </div>
            </div>
          </section>

          {/* Advanced Workflows Banner */}
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

      <InviteMemberModal open={inviteOpen} onClose={() => setInviteOpen(false)} />
    </>
  );
}
