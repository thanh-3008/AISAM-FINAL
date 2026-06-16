"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import { fetchWorkspaceMembers, type WorkspaceMember, type WorkspaceMemberRole } from "@/services/workspaceService";

function getRoleBadge(role: WorkspaceMemberRole): { label: string; color: string; bg: string; icon: string } {
  switch (role) {
    case "Owner":
      return { label: "Owner", color: "text-amber-700", bg: "bg-amber-50 border-amber-200/50", icon: "star" };
    case "Manager":
      return { label: "Manager", color: "text-blue-700", bg: "bg-blue-50 border-blue-200/50", icon: "manage_accounts" };
    case "ContentCreator":
      return { label: "Content Creator", color: "text-emerald-700", bg: "bg-emerald-50 border-emerald-200/50", icon: "edit_note" };
    case "Viewer":
      return { label: "Viewer", color: "text-outline", bg: "bg-surface-container border-outline-variant/20", icon: "visibility" };
  }
}

function getStatusBadge(_status: string): { label: string; color: string; bg: string; dot: string } {
  return { label: "Active", color: "text-emerald-700", bg: "bg-emerald-50 border-emerald-200/50", dot: "bg-emerald-500" };
}

function getInitials(name: string): string {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

export default function WorkspaceMembersPage() {
  const { activeWorkspace } = useWorkspaces();
  const featureGate = useFeatureGate();
  const [members, setMembers] = useState<WorkspaceMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<"all" | "active" | "pending">("all");
  const [showInviteModal, setShowInviteModal] = useState(false);

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

  const filteredMembers = members.filter((member) => {
    if (filter === "all") return true;
    return "active" === filter;
  });

  const activeCount = members.length;
  const pendingCount = 0;

  const isOwner = members.some((m) => m.role === "Owner");

  if (!featureGate.canAccess("teamManagement")) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Team Members" }]} />
        <main className="ml-0 p-6 h-[calc(100vh-64px)] overflow-y-auto">
          <div className="max-w-5xl mx-auto flex items-center justify-center min-h-[60vh]">
            <div className="text-center max-w-md">
              <div className="w-16 h-16 mx-auto mb-6 bg-outline/10 rounded-2xl flex items-center justify-center">
                <span className="material-symbols-outlined text-outline text-[32px]">lock</span>
              </div>
              <h2 className="text-headline-md text-on-surface font-bold mb-2">Team Members</h2>
              <p className="text-body-md text-on-surface-variant mb-6">This feature requires a <strong>Business plan</strong>. Upgrade to manage team members.</p>
              <Link href="/pricing" className="inline-flex items-center gap-2 px-6 py-3 bg-primary text-on-primary rounded-xl text-label-sm font-bold hover:scale-105 transition-all">
                View Plans
                <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
              </Link>
            </div>
          </div>
        </main>
      </>
    );
  }

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Team Members" },
      ]} />
      <main className="ml-0 p-6 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-5xl mx-auto space-y-6">
          {/* Header */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <span className="w-10 h-10 rounded-xl bg-gradient-to-br from-purple-500/10 to-purple-600/10 text-purple-500 flex items-center justify-center">
                <span className="material-symbols-outlined text-[22px]">group</span>
              </span>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Team Members</h1>
                <p className="text-body-sm text-on-surface-variant">
                  {activeWorkspace?.name || "Workspace"} - {members.length} members
                </p>
              </div>
            </div>
            <button
              onClick={() => setShowInviteModal(true)}
              className="px-4 py-2.5 rounded-xl bg-primary text-on-primary text-body-sm font-semibold hover:bg-primary/90 transition-all shadow-sm shadow-primary/20 flex items-center gap-2"
            >
              <span className="material-symbols-outlined text-[18px]">person_add</span>
              Invite Member
            </button>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-5">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-primary/5 flex items-center justify-center">
                  <span className="material-symbols-outlined text-primary text-[20px]">group</span>
                </div>
                <div>
                  <p className="text-label-sm text-on-surface-variant">Total Members</p>
                  <p className="text-body-lg font-bold text-on-surface">{members.length}</p>
                </div>
              </div>
            </div>
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-5">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-emerald-50 flex items-center justify-center">
                  <span className="material-symbols-outlined text-emerald-600 text-[20px]">check_circle</span>
                </div>
                <div>
                  <p className="text-label-sm text-on-surface-variant">Active</p>
                  <p className="text-body-lg font-bold text-emerald-600">{activeCount}</p>
                </div>
              </div>
            </div>
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-5">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-amber-50 flex items-center justify-center">
                  <span className="material-symbols-outlined text-amber-600 text-[20px]">schedule</span>
                </div>
                <div>
                  <p className="text-label-sm text-on-surface-variant">Pending</p>
                  <p className="text-body-lg font-bold text-amber-600">{pendingCount}</p>
                </div>
              </div>
            </div>
          </div>

          {/* Filters */}
          <div className="flex items-center gap-2">
            {[
              { key: "all" as const, label: "All", count: members.length },
              { key: "active" as const, label: "Active", count: activeCount },
              { key: "pending" as const, label: "Pending", count: pendingCount },
            ].map((f) => (
              <button
                key={f.key}
                onClick={() => setFilter(f.key)}
                className={`px-4 py-2 rounded-xl text-label-sm font-medium transition-all ${
                  filter === f.key
                    ? "bg-primary text-on-primary shadow-sm"
                    : "bg-surface-container-lowest text-on-surface-variant hover:bg-surface-container border border-outline-variant/20"
                }`}
              >
                {f.label}
                <span className={`ml-2 px-1.5 py-0.5 rounded-full text-label-xs ${
                  filter === f.key ? "bg-white/20" : "bg-surface-container"
                }`}>
                  {f.count}
                </span>
              </button>
            ))}
          </div>

          {/* Members List */}
          {loading ? (
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6">
              <div className="space-y-4">
                {[1, 2, 3, 4].map((i) => (
                  <div key={i} className="flex items-center gap-4 animate-pulse">
                    <div className="w-12 h-12 rounded-full bg-surface-container" />
                    <div className="flex-1 space-y-2">
                      <div className="h-4 w-48 bg-surface-container rounded" />
                      <div className="h-3 w-32 bg-surface-container rounded" />
                    </div>
                    <div className="h-6 w-20 bg-surface-container rounded" />
                  </div>
                ))}
              </div>
            </div>
          ) : filteredMembers.length === 0 ? (
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-12 text-center">
              <span className="material-symbols-outlined text-outline/40 text-5xl mb-4 block">group_off</span>
              <p className="text-body-md text-on-surface font-semibold mb-2">No members found</p>
              <p className="text-body-sm text-on-surface-variant">
                {filter !== "all" ? "Try changing the filter" : "Invite team members to collaborate"}
              </p>
            </div>
          ) : (
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 overflow-hidden">
              <div className="divide-y divide-outline-variant/10">
                {filteredMembers.map((member) => {
                  const roleBadge = getRoleBadge(member.role);
                  const statusBadge = getStatusBadge("Active");
                  return (
                    <div
                      key={member.id}
                      className="flex items-center gap-4 px-6 py-4 hover:bg-surface-container/30 transition-colors"
                    >
                      {/* Avatar */}
                      <div className="w-12 h-12 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 flex items-center justify-center text-body-md font-bold text-primary shrink-0">
                        {getInitials(member.name)}
                      </div>

                      {/* Info */}
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <p className="text-body-sm text-on-surface font-semibold truncate">{member.name}</p>
                          {member.role === "Owner" && (
                            <span className="material-symbols-outlined text-amber-500 text-[16px]">star</span>
                          )}
                        </div>
                        <p className="text-label-sm text-on-surface-variant truncate">{member.email}</p>
                        {member.joinedAt && (
                          <p className="text-label-xs text-outline mt-0.5">
                            Joined: {new Date(member.joinedAt).toLocaleDateString("en-US", {
                              month: "short",
                              day: "numeric",
                              hour: "2-digit",
                              minute: "2-digit",
                            })}
                          </p>
                        )}
                      </div>

                      {/* Role Badge */}
                      <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-label-xs font-semibold border ${roleBadge.bg} ${roleBadge.color}`}>
                        <span className="material-symbols-outlined text-[12px]">{roleBadge.icon}</span>
                        {roleBadge.label}
                      </span>

                      {/* Status Badge */}
                      <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-xs font-medium border ${statusBadge.bg} ${statusBadge.color}`}>
                        <span className={`w-1.5 h-1.5 rounded-full ${statusBadge.dot} ${"Active" === "Active" ? "animate-pulse" : ""}`} />
                        {statusBadge.label}
                      </span>

                      {/* Actions */}
                      {isOwner && member.role !== "Owner" && (
                        <button className="p-2 rounded-lg hover:bg-surface-container transition-colors">
                          <span className="material-symbols-outlined text-on-surface-variant text-[20px]">more_vert</span>
                        </button>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* Role Permissions Info */}
          <div className="bg-gradient-to-br from-primary/5 to-secondary/5 rounded-2xl border border-primary/10 p-6">
            <div className="flex items-start gap-4 mb-4">
              <div className="w-12 h-12 rounded-xl bg-white/80 flex items-center justify-center shrink-0">
                <span className="material-symbols-outlined text-primary text-[24px]">admin_panel_settings</span>
              </div>
              <div>
                <h4 className="text-body-md font-semibold text-on-surface mb-1">Role Permissions</h4>
                <p className="text-body-sm text-on-surface-variant">
                  Understanding what each role can do
                </p>
              </div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {[
                { role: "Owner", desc: "Full access, billing, subscription, invite/remove members, assign quota", icon: "star", color: "text-amber-600" },
                { role: "Manager", desc: "Brand, Product, Content, Campaign management, view team usage", icon: "manage_accounts", color: "text-blue-600" },
                { role: "Content Creator", desc: "Generate content, create drafts, publish", icon: "edit_note", color: "text-emerald-600" },
                { role: "Viewer", desc: "View dashboard and analytics only", icon: "visibility", color: "text-outline" },
              ].map((r) => (
                <div key={r.role} className="bg-white/60 rounded-xl p-4 border border-outline-variant/10">
                  <div className="flex items-center gap-2 mb-2">
                    <span className={`material-symbols-outlined ${r.color} text-[20px]`}>{r.icon}</span>
                    <span className="text-body-sm font-semibold text-on-surface">{r.role}</span>
                  </div>
                  <p className="text-label-sm text-on-surface-variant leading-relaxed">{r.desc}</p>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Invite Modal */}
        {showInviteModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-md mx-4 p-6">
              <div className="flex items-center gap-3 mb-6">
                <div className="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center">
                  <span className="material-symbols-outlined text-primary text-[24px]">person_add</span>
                </div>
                <div>
                  <h3 className="text-body-lg font-bold text-on-surface">Invite Member</h3>
                  <p className="text-label-sm text-on-surface-variant">Add a new team member</p>
                </div>
              </div>

              <div className="space-y-4 mb-6">
                <div>
                  <label className="text-label-sm font-semibold text-on-surface mb-1.5 block">Email Address</label>
                  <input
                    type="email"
                    placeholder="colleague@company.com"
                    className="w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest px-4 py-2.5 text-body-sm text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all"
                  />
                </div>
                <div>
                  <label className="text-label-sm font-semibold text-on-surface mb-1.5 block">Role</label>
                  <select className="w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest px-4 py-2.5 text-body-sm text-on-surface focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all">
                    <option value="Viewer">Viewer - View only</option>
                    <option value="ContentCreator">Content Creator - Create & publish</option>
                    <option value="Manager">Manager - Manage content & campaigns</option>
                  </select>
                </div>
              </div>

              <div className="flex gap-3">
                <button
                  onClick={() => setShowInviteModal(false)}
                  className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-colors"
                >
                  Cancel
                </button>
                <button
                  onClick={() => setShowInviteModal(false)}
                  className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold bg-primary text-on-primary hover:bg-primary/90 transition-all shadow-sm shadow-primary/20"
                >
                  Send Invitation
                </button>
              </div>
            </div>
          </div>
        )}
      </main>
    </>
  );
}
