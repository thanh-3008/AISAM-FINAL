"use client";

import { useState, useEffect, useMemo } from "react";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { useWorkspaces, getWorkspaceTypeLabel } from "@/hooks/useWorkspaces";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import {
  fetchTeams,
  fetchMembers,
  createTeam,
  updateTeam,
  deleteTeam,
  inviteMember,
  updateMemberRole,
  removeMember,
  type Team,
  type TeamMember,
  type MemberRole,
  type MemberStatus,
  type CreateTeamData,
  type InviteMemberData,
} from "@/services/teamService";
import TeamStatsCards from "@/components/team/TeamStatsCards";
import TeamCard from "@/components/team/TeamCard";
import TeamListView from "@/components/team/TeamListView";
import TeamFilterBar, { type SortOption } from "@/components/team/TeamFilterBar";
import TeamEmptyState from "@/components/team/TeamEmptyState";
import TeamDetailModal from "@/components/team/TeamDetailModal";
import CreateTeamModal from "@/components/team/CreateTeamModal";
import EditTeamModal from "@/components/team/EditTeamModal";
import EditMemberModal from "@/components/team/EditMemberModal";
import MemberDetailModal from "@/components/team/MemberDetailModal";
import DeleteMemberConfirmModal from "@/components/team/DeleteMemberConfirmModal";
import InviteMemberModal from "@/components/team/InviteMemberModal";
import DeleteConfirmModal from "@/components/team/DeleteConfirmModal";
import BulkActionsBar from "@/components/team/BulkActionsBar";
import RoleDonutChart from "@/components/team/RoleDonutChart";
import MemberCard from "@/components/team/MemberCard";
import { calcTimeAgo } from "@/components/team/teamUtils";

export default function TeamPage() {
  const [teams, setTeams] = useState<Team[]>([]);
  const [members, setMembers] = useState<TeamMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    const interval = setInterval(() => setNow(Date.now()), 60000);
    return () => clearInterval(interval);
  }, []);

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<MemberStatus | "">("");
  const [sortBy, setSortBy] = useState<SortOption>("newest");
  const [teamView, setTeamView] = useState<"grid" | "list">("list");
  const [memberView, setMemberView] = useState<"grid" | "table">("table");

  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showInviteModal, setShowInviteModal] = useState(false);
  const [detailTeam, setDetailTeam] = useState<Team | null>(null);
  const [editingTeam, setEditingTeam] = useState<Team | null>(null);
  const [editingMember, setEditingMember] = useState<TeamMember | null>(null);
  const [detailMember, setDetailMember] = useState<TeamMember | null>(null);
  const [deletingTeams, setDeletingTeams] = useState<Team[]>([]);
  const [deletingMembers, setDeletingMembers] = useState<TeamMember[]>([]);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const [toast, setToast] = useState<{ msg: string; type: "success" | "error" } | null>(null);
  const featureGate = useFeatureGate();
  const { activeWorkspace } = useWorkspaces();

  const activeMemberCount = members.filter((m) => m.status === "Active").length;
  const maxMembers = featureGate.isBusiness
    ? featureGate.plan === 4 ? 50 : 10
    : Infinity;

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      try {
        const [teamsRes, membersRes] = await Promise.all([fetchTeams(), fetchMembers()]);
        if (!cancelled) {
          setTeams(teamsRes.data);
          setMembers(membersRes.data);
        }
      } catch {
        if (!cancelled) {
          setTeams([]);
          setMembers([]);
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [activeWorkspace?.id]);

  useEffect(() => {
    if (toast) {
      const timer = setTimeout(() => setToast(null), 3000);
      return () => clearTimeout(timer);
    }
  }, [toast]);

  const showToast = (msg: string, type: "success" | "error" = "success") => {
    setToast({ msg, type });
  };

  const handleCreate = async (data: CreateTeamData) => {
    setActionLoading("create");
    try {
      const newTeam = await createTeam(data);
      setTeams((prev) => [newTeam, ...prev]);
      setShowCreateModal(false);
      showToast(`Team "${newTeam.name}" created successfully`);
    } catch {
      showToast("Failed to create team", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleEdit = async (id: string, data: CreateTeamData) => {
    setActionLoading("edit");
    try {
      const updated = await updateTeam(id, data);
      if (updated) {
        setTeams((prev) => prev.map((t) => (t.id === id ? updated : t)));
        setEditingTeam(null);
        showToast(`Team "${updated.name}" updated successfully`);
      }
    } catch {
      showToast("Failed to update team", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleEditMember = async (id: string, role: MemberRole) => {
    setActionLoading("editMember");
    try {
      const updated = await updateMemberRole(id, role);
      if (updated) {
        setMembers((prev) => prev.map((m) => (m.id === id ? updated : m)));
        setEditingMember(null);
        showToast(`Member role updated to ${role}`);
      }
    } catch {
      showToast("Failed to update member role", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleDeleteMember = (member: TeamMember) => {
    setDeletingMembers([member]);
  };

  const handleConfirmDeleteMember = async () => {
    if (deletingMembers.length === 0) return;
    setActionLoading("deleteMember");
    try {
      for (const member of deletingMembers) {
        await removeMember(member.id);
      }
      setMembers((prev) => prev.filter((m) => !deletingMembers.some((d) => d.id === m.id)));
      setDeletingMembers([]);
      showToast(`${deletingMembers.length} member(s) removed`);
    } catch {
      showToast("Failed to remove member(s)", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleInvite = async (data: InviteMemberData) => {
    setActionLoading("invite");
    try {
      const newMember = await inviteMember(data);
      setMembers((prev) => [newMember, ...prev]);
      setShowInviteModal(false);
      showToast(`Invitation sent to ${newMember.email}`);
    } catch {
      showToast("Failed to send invitation", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleDelete = (team: Team) => {
    setDeletingTeams([team]);
  };

  const handleBulkDelete = () => {
    const selected = teams.filter((t) => selectedIds.includes(t.id));
    setDeletingTeams(selected);
  };

  const handleConfirmDelete = async () => {
    if (deletingTeams.length === 0) return;
    setActionLoading("delete");
    try {
      for (const team of deletingTeams) {
        await deleteTeam(team.id);
      }
      setTeams((prev) => prev.filter((t) => !deletingTeams.some((d) => d.id === t.id)));
      setSelectedIds((prev) => prev.filter((id) => !deletingTeams.some((d) => d.id === id)));
      setDeletingTeams([]);
      showToast(`${deletingTeams.length} team(s) deleted`);
    } catch {
      showToast("Failed to delete team(s)", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleSelect = (id: string, selected: boolean) => {
    setSelectedIds((prev) =>
      selected ? [...prev, id] : prev.filter((x) => x !== id)
    );
  };

  const handleClearSelection = () => {
    setSelectedIds([]);
  };

  const filteredMembers = useMemo(() => {
    let result = [...members];
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(
        (m) => m.name.toLowerCase().includes(q) || m.email.toLowerCase().includes(q)
      );
    }
    if (statusFilter) {
      result = result.filter((m) => m.status === statusFilter);
    }
    switch (sortBy) {
      case "newest":
        result.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        break;
      case "oldest":
        result.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
        break;
      case "name":
        result.sort((a, b) => a.name.localeCompare(b.name));
        break;
      case "role":
        result.sort((a, b) => a.role.localeCompare(b.role));
        break;
      case "status":
        result.sort((a, b) => a.status.localeCompare(b.status));
        break;
    }
    return result;
  }, [members, search, statusFilter, sortBy]);

  const hasFilters = !!(search || statusFilter);

  if (!featureGate.canAccess("teamManagement")) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Team Management" }]} />
        <div className="flex-1 flex items-center justify-center p-8">
          <div className="text-center max-w-md">
            <div className="w-16 h-16 mx-auto mb-6 bg-outline/10 rounded-2xl flex items-center justify-center">
              <span className="material-symbols-outlined text-outline text-[32px]">lock</span>
            </div>
            <h2 className="text-headline-md text-on-surface font-bold mb-2">Team Management</h2>
            <p className="text-body-md text-on-surface-variant mb-6">This feature requires a <strong>Business plan</strong>. Upgrade to manage teams and members.</p>
            <Link href="/pricing" className="inline-flex items-center gap-2 px-6 py-3 bg-primary text-on-primary rounded-xl text-label-sm font-bold hover:scale-105 transition-all">
              View Plans
              <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
            </Link>
          </div>
        </div>
      </>
    );
  }

  return (
    <>
      <style>{`
        @keyframes fade-up { from { opacity: 0; transform: translateY(16px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes float { 0%,100% { transform: translateY(0px); } 50% { transform: translateY(-6px); } }
        @keyframes pulse-dot { 0%,100% { opacity: 1; } 50% { opacity: 0.5; } }
        .animate-fade-up { animation: fade-up 0.5s ease-out forwards; opacity: 0; }
        .animate-float { animation: float 4s ease-in-out infinite; }
        .animate-pulse-dot { animation: pulse-dot 2s ease-in-out infinite; }
        .card-hover { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
        .card-hover:hover { transform: translateY(-4px); box-shadow: 0 12px 40px -12px rgba(0,0,0,0.15); }
      `}</style>

      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Team Management" }]} />

      <div className="p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-7xl mx-auto space-y-6">

          {/* Page Header */}
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 animate-fade-up">
            <div className="flex items-center gap-4">
              <div className="relative w-12 h-12 shrink-0">
                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-primary to-primary/70 animate-float shadow-lg shadow-primary/20" />
                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-white/15 to-transparent" />
                <div className="relative w-full h-full flex items-center justify-center">
                  <span className="material-symbols-outlined text-on-primary text-[24px]">group</span>
                </div>
              </div>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Teams &amp; Collaboration</h1>
                <p className="text-label-sm text-outline">{members.length} members · {teams.length} teams · Manage your organization</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <button
                onClick={() => setShowCreateModal(true)}
                className="px-5 py-2.5 rounded-xl border border-outline-variant/20 text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all flex items-center gap-2"
              >
                <span className="material-symbols-outlined text-[16px]">add_circle</span>
                Create Team
              </button>
              <button
                onClick={() => setShowInviteModal(true)}
                className="px-5 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 flex items-center gap-2"
              >
                <span className="material-symbols-outlined text-[16px]">person_add</span>
                Invite Member
              </button>
            </div>
          </div>

          {/* Stats */}
          <TeamStatsCards teams={teams} members={members} />

          {/* Teams Section */}
          <section className="animate-fade-up" style={{ animationDelay: "0.2s" }}>
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-headline-sm text-on-surface font-semibold">Teams</h2>
              <div className="flex items-center gap-2 bg-surface-container-low rounded-lg p-1">
                <button
                  onClick={() => setTeamView("grid")}
                  className={`p-1.5 rounded-md transition-all ${teamView === "grid" ? "bg-surface-container-lowest shadow-sm text-primary" : "text-outline hover:text-on-surface"}`}
                >
                  <span className="material-symbols-outlined text-[18px]">grid_view</span>
                </button>
                <button
                  onClick={() => setTeamView("list")}
                  className={`p-1.5 rounded-md transition-all ${teamView === "list" ? "bg-surface-container-lowest shadow-sm text-primary" : "text-outline hover:text-on-surface"}`}
                >
                  <span className="material-symbols-outlined text-[18px]">view_list</span>
                </button>
              </div>
            </div>

            {loading ? (
              <div className={teamView === "grid" ? "grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6" : ""}>
                {Array.from({ length: 3 }).map((_, i) => (
                  <div key={i} className="bg-surface-container-lowest border border-outline-variant/10 rounded-2xl p-6 animate-pulse">
                    <div className="flex items-center gap-4 mb-4">
                      <div className="w-10 h-10 rounded-xl bg-surface-container" />
                      <div className="space-y-2 flex-1">
                        <div className="h-4 w-32 bg-surface-container rounded" />
                        <div className="h-3 w-24 bg-surface-container rounded" />
                      </div>
                    </div>
                    <div className="h-2 bg-surface-container rounded-full mb-4" />
                    <div className="flex items-center gap-2">
                      <div className="w-6 h-6 rounded-full bg-surface-container" />
                      <div className="w-6 h-6 rounded-full bg-surface-container" />
                    </div>
                  </div>
                ))}
              </div>
            ) : teamView === "grid" ? (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                {teams.map((team, i) => (
                  <TeamCard
                    key={team.id}
                    team={team}
                    index={i}
                    isSelected={selectedIds.includes(team.id)}
                    isLoading={actionLoading === team.id}
                    onSelect={handleSelect}
                    onViewDetail={setDetailTeam}
                    onEdit={setEditingTeam}
                    onDelete={handleDelete}
                  />
                ))}
              </div>
            ) : (
              <TeamListView
                teams={teams}
                selectedIds={selectedIds}
                actionLoading={actionLoading}
                onSelect={handleSelect}
                onViewDetail={setDetailTeam}
                onEdit={setEditingTeam}
                onDelete={handleDelete}
              />
            )}
          </section>

          {/* Bulk Actions */}
          <BulkActionsBar
            selectedCount={selectedIds.length}
            onClearSelection={handleClearSelection}
            onBulkDelete={handleBulkDelete}
            isLoading={actionLoading === "delete"}
          />

          {/* Members Section */}
          <section className="animate-fade-up" style={{ animationDelay: "0.3s" }}>
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-headline-sm text-on-surface font-semibold">Members</h2>
              <div className="flex items-center gap-2 bg-surface-container-low rounded-lg p-1">
                <button
                  onClick={() => setMemberView("grid")}
                  className={`p-1.5 rounded-md transition-all ${memberView === "grid" ? "bg-surface-container-lowest shadow-sm text-primary" : "text-outline hover:text-on-surface"}`}
                >
                  <span className="material-symbols-outlined text-[18px]">grid_view</span>
                </button>
                <button
                  onClick={() => setMemberView("table")}
                  className={`p-1.5 rounded-md transition-all ${memberView === "table" ? "bg-surface-container-lowest shadow-sm text-primary" : "text-outline hover:text-on-surface"}`}
                >
                  <span className="material-symbols-outlined text-[18px]">view_list</span>
                </button>
              </div>
            </div>

            {/* Filter Bar */}
            <TeamFilterBar
              search={search}
              onSearchChange={setSearch}
              statusFilter={statusFilter}
              onStatusFilterChange={setStatusFilter}
              sortBy={sortBy}
              onSortChange={setSortBy}
              resultCount={filteredMembers.length}
              totalCount={members.length}
            />

            {loading ? (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6 mt-6">
                {Array.from({ length: 6 }).map((_, i) => (
                  <div key={i} className="bg-surface-container-lowest border border-outline-variant/10 rounded-2xl p-6 animate-pulse">
                    <div className="flex items-center gap-4 mb-4">
                      <div className="w-14 h-14 rounded-full bg-surface-container" />
                      <div className="space-y-2 flex-1">
                        <div className="h-4 w-32 bg-surface-container rounded" />
                        <div className="h-3 w-40 bg-surface-container rounded" />
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : filteredMembers.length === 0 && teams.length === 0 ? (
              <TeamEmptyState
                hasFilters={hasFilters}
                onCreate={() => setShowCreateModal(true)}
                onInvite={() => setShowInviteModal(true)}
              />
            ) : (
              <>
                {/* Role Distribution Chart */}
                {!hasFilters && filteredMembers.length > 0 && (
                  <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-6 shadow-sm mb-6">
                    <h3 className="text-label-sm font-bold text-on-surface mb-4">Role Distribution</h3>
                    <RoleDonutChart members={filteredMembers} />
                  </div>
                )}

                {/* Members Grid */}
                {memberView === "grid" ? (
                  <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                    {filteredMembers.map((member) => (
                      <MemberCard
                        key={member.id}
                        member={member}
                        onEdit={setEditingMember}
                        onDelete={handleDeleteMember}
                        onViewDetail={setDetailMember}
                      />
                    ))}
                  </div>
                ) : (
                  <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-xl overflow-hidden shadow-sm">
                    <div className="overflow-x-auto">
                      <table className="w-full">
                        <thead>
                          <tr className="bg-surface-container/50">
                            <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Member</th>
                            <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Role</th>
                            <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Status</th>
                            <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Last Active</th>
                            <th className="px-6 py-3.5 text-right text-label-xs text-outline font-bold uppercase tracking-wider">Actions</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-outline-variant/10">
                          {filteredMembers.map((member) => (
                            <tr key={member.id} className="hover:bg-primary-fixed/10 transition-colors group cursor-pointer" onClick={() => setDetailMember(member)}>
                              <td className="px-6 py-4">
                                <div className="flex items-center gap-3">
                                  {member.avatar ? (
                                    <div className="relative">
                                      <img src={member.avatar} alt={member.name} className="w-9 h-9 rounded-full object-cover ring-2 ring-white shadow-sm" />
                                      {member.status === "Active" && (
                                        <span className="absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full bg-success-green border-2 border-white animate-pulse-dot" />
                                      )}
                                    </div>
                                  ) : (
                                    <div className="relative">
                                      <div className="w-9 h-9 rounded-full bg-gradient-to-br from-primary/20 to-primary/5 flex items-center justify-center text-label-sm font-bold text-primary ring-2 ring-white shadow-sm">
                                        {member.name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2)}
                                      </div>
                                      {member.status === "Active" && (
                                        <span className="absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full bg-success-green border-2 border-white animate-pulse-dot" />
                                      )}
                                    </div>
                                  )}
                                  <div>
                                    <p className="text-body-sm text-on-surface font-semibold">{member.name}</p>
                                    <p className="text-label-xs text-outline">{member.email}</p>
                                  </div>
                                </div>
                              </td>
                              <td className="px-6 py-4">
                                <span className={`px-2.5 py-1 rounded-full text-label-2xs font-bold uppercase tracking-wider ${
                                  member.role === "Owner" ? "bg-primary-fixed text-primary" :
                                  member.role === "Manager" ? "bg-secondary-fixed text-secondary" :
                                  member.role === "ContentCreator" ? "bg-tertiary-fixed text-tertiary" :
                                  "bg-surface-container text-outline"
                                }`}>
                                  {member.role === "ContentCreator" ? "Content Creator" : member.role}
                                </span>
                              </td>
                              <td className="px-6 py-4">
                                <div className="flex items-center gap-2">
                                  <span className={`w-2 h-2 rounded-full ${member.status === "Active" ? "bg-success-green" : member.status === "Pending" ? "bg-warning-amber" : "bg-outline"} ${member.status === "Active" ? "animate-pulse-dot" : ""}`} />
                                  <span className="text-body-sm">{member.status}</span>
                                </div>
                              </td>
                              <td className="px-6 py-4">
                                <span className="text-label-xs text-outline">{calcTimeAgo(now, member.lastActive)}</span>
                              </td>
                              <td className="px-6 py-4 text-right">
                                <div className="flex justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                  <button
                                    onClick={(e) => { e.stopPropagation(); setEditingMember(member); }}
                                    className="p-1.5 rounded-lg text-outline hover:text-primary hover:bg-primary/10 transition-all"
                                    title="Edit member"
                                  >
                                    <span className="material-symbols-outlined text-[16px]">edit</span>
                                  </button>
                                  <button
                                    onClick={(e) => { e.stopPropagation(); handleDeleteMember(member); }}
                                    className="p-1.5 rounded-lg text-outline hover:text-danger-red hover:bg-danger-red/10 transition-all"
                                    title="Remove member"
                                  >
                                    <span className="material-symbols-outlined text-[16px]">delete</span>
                                  </button>
                                </div>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                )}
              </>
            )}
          </section>
        </div>

        {/* Modals */}
        <MemberDetailModal
          member={detailMember}
          teams={teams}
          onClose={() => setDetailMember(null)}
          onEdit={setEditingMember}
          onDelete={handleDeleteMember}
        />

        <CreateTeamModal
          open={showCreateModal}
          onClose={() => setShowCreateModal(false)}
          onCreate={handleCreate}
          isLoading={actionLoading === "create"}
        />

        <EditTeamModal
          team={editingTeam}
          onClose={() => setEditingTeam(null)}
          onUpdate={handleEdit}
          isLoading={actionLoading === "edit"}
        />

        <EditMemberModal
          member={editingMember}
          onClose={() => setEditingMember(null)}
          onUpdate={handleEditMember}
          isLoading={actionLoading === "editMember"}
        />

        <DeleteMemberConfirmModal
          members={deletingMembers}
          isLoading={actionLoading === "deleteMember"}
          onConfirm={handleConfirmDeleteMember}
          onCancel={() => setDeletingMembers([])}
        />

        <InviteMemberModal
          open={showInviteModal}
          onClose={() => setShowInviteModal(false)}
          onInvite={handleInvite}
          isLoading={actionLoading === "invite"}
          teams={teams}
          currentMemberCount={activeMemberCount}
          maxMembers={maxMembers}
        />

        <TeamDetailModal
          team={detailTeam}
          members={members}
          onClose={() => setDetailTeam(null)}
        />

        <DeleteConfirmModal
          teams={deletingTeams}
          isLoading={actionLoading === "delete"}
          onConfirm={handleConfirmDelete}
          onCancel={() => setDeletingTeams([])}
        />

        {/* Toast */}
        {toast && (
          <div className={`fixed bottom-6 right-6 z-[100] flex items-center gap-3 px-5 py-3 rounded-xl shadow-2xl animate-in fade-in slide-in-from-right-2 duration-200 ${
            toast.type === "success" ? "bg-emerald-600 text-white" : "bg-danger-red text-white"
          }`}>
            <span className="material-symbols-outlined text-[18px]">{toast.type === "success" ? "check_circle" : "error"}</span>
            <p className="text-label-sm font-bold">{toast.msg}</p>
            <button onClick={() => setToast(null)} className="ml-2 p-0.5 hover:bg-white/20 rounded-full transition-colors">
              <span className="material-symbols-outlined text-[14px]">close</span>
            </button>
          </div>
        )}
      </div>
    </>
  );
}
