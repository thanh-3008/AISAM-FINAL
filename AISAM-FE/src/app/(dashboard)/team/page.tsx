"use client";

import { useState, useEffect, useMemo, useCallback } from "react";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { useWorkspaces, getWorkspaceTypeLabel, invalidateWorkspaceCache } from "@/hooks/useWorkspaces";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import {
  fetchMembers,
  fetchTeams,
  createTeam,
  updateTeam,
  deleteTeam,
  inviteMember,
  updateMemberRole,
  transferWorkspaceOwnership,
  removeMember,
  updateMemberQuota,
  syncWorkspaceTeams,
  type Team,
  type CreateTeamData,
  type TeamMember,
  type MemberRole,
  type MemberStatus,
  type QuotaMode,
  type InviteMemberData,
} from "@/services/teamService";
import TeamFilterBar, { type SortOption } from "@/components/team/TeamFilterBar";
import TeamEmptyState from "@/components/team/TeamEmptyState";
import EditMemberModal from "@/components/team/EditMemberModal";
import MemberDetailModal from "@/components/team/MemberDetailModal";
import DeleteMemberConfirmModal from "@/components/team/DeleteMemberConfirmModal";
import TransferOwnershipConfirmModal from "@/components/team/TransferOwnershipConfirmModal";
import InviteMemberModal from "@/components/team/InviteMemberModal";
import RoleDonutChart from "@/components/team/RoleDonutChart";
import MemberCard from "@/components/team/MemberCard";
import TeamCard from "@/components/team/TeamCard";
import TeamListView from "@/components/team/TeamListView";
import CreateTeamModal from "@/components/team/CreateTeamModal";
import EditTeamModal from "@/components/team/EditTeamModal";
import TeamDetailModal from "@/components/team/TeamDetailModal";
import DeleteConfirmModal from "@/components/team/DeleteConfirmModal";
import BulkActionsBar from "@/components/team/BulkActionsBar";
import TeamStatsCards from "@/components/team/TeamStatsCards";
import { calcTimeAgo } from "@/components/team/teamUtils";
import { getStoredActiveTeam, storeActiveTeam, type ActiveTeam } from "@/stores/team-store";
import OwnerTeamSwitcher from "@/components/team/OwnerTeamSwitcher";

export default function TeamPage() {
  const [members, setMembers] = useState<TeamMember[]>([]);
  const [teams, setTeams] = useState<Team[]>([]);
  const [loading, setLoading] = useState(true);
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    const interval = setInterval(() => setNow(Date.now()), 60000);
    return () => clearInterval(interval);
  }, []);

  // Teams state
  const [teamView, setTeamView] = useState<"grid" | "table">("grid");
  const [selectedTeamIds, setSelectedTeamIds] = useState<string[]>([]);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingTeam, setEditingTeam] = useState<Team | null>(null);
  const [detailTeam, setDetailTeam] = useState<Team | null>(null);
  const [deletingTeams, setDeletingTeams] = useState<Team[]>([]);

  // Members state
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<MemberStatus | "">("");
  const [sortBy, setSortBy] = useState<SortOption>("newest");
  const [memberView, setMemberView] = useState<"grid" | "table">("table");

  const [showInviteModal, setShowInviteModal] = useState(false);
  const [editingMember, setEditingMember] = useState<TeamMember | null>(null);
  const [detailMember, setDetailMember] = useState<TeamMember | null>(null);
  const [deletingMembers, setDeletingMembers] = useState<TeamMember[]>([]);
  const [transferringOwnerMember, setTransferringOwnerMember] = useState<TeamMember | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [quotaLoading, setQuotaLoading] = useState(false);
  const [syncing, setSyncing] = useState(false);

  const [toast, setToast] = useState<{ msg: string; type: "success" | "error" } | null>(null);
  const featureGate = useFeatureGate();
  const { activeWorkspace, refetch: refetchWorkspaces } = useWorkspaces();

  const activeMemberCount = members.filter((m) => m.status === "Active").length;
  const canAssignQuota = featureGate.canAccess("lifetimeAssignedLimit") || featureGate.canAccess("monthlyAssignedLimit");
  const isOwner = activeWorkspace?.isOwner === true;

  const [activeTeam, setActiveTeam] = useState<ActiveTeam | null>(() => getStoredActiveTeam(activeWorkspace?.id));

  useEffect(() => {
    const handleSync = () => {
      setActiveTeam(getStoredActiveTeam(activeWorkspace?.id));
    };
    handleSync();
    window.addEventListener("aisam_active_team_changed", handleSync);
    return () => window.removeEventListener("aisam_active_team_changed", handleSync);
  }, [activeWorkspace?.id]);

  const handleSwitchTeam = (team: Team) => {
    const next: ActiveTeam = { id: team.id, name: team.name, workspaceId: activeWorkspace?.id };
    storeActiveTeam(next);
    setActiveTeam(next);
    showToast(`Đã chuyển sang team "${team.name}"`);
  };

  const showToast = (msg: string, type: "success" | "error" = "success") => {
    setToast({ msg, type });
  };

  const loadData = useCallback(async () => {
    try {
      const teamsRes = await fetchTeams().catch(() => ({ data: [], total: 0 }));
      const membersRes = await fetchMembers(teamsRes.data);
      setTeams(teamsRes.data);
      setMembers(membersRes.data);

      const stored = getStoredActiveTeam(activeWorkspace?.id);
      if ((!stored || !teamsRes.data.some((t) => t.id === stored.id)) && teamsRes.data.length > 0) {
        const firstTeam: ActiveTeam = { id: teamsRes.data[0].id, name: teamsRes.data[0].name, workspaceId: activeWorkspace?.id };
        storeActiveTeam(firstTeam);
        setActiveTeam(firstTeam);
      } else if (stored) {
        setActiveTeam(stored);
      }
    } catch {
      showToast("Failed to load team data", "error");
      setMembers([]);
      setTeams([]);
    }
  }, [activeWorkspace?.id]);

  const handleSyncTeam = async () => {
    setSyncing(true);
    try {
      const res = await syncWorkspaceTeams();
      if (res.success) {
        showToast(res.message || "Đã đồng bộ toàn bộ thành viên và thương hiệu vào nhóm!", "success");
        await loadData();
      } else {
        showToast(res.message || "Đồng bộ team thất bại", "error");
      }
    } catch {
      showToast("Đồng bộ team thất bại", "error");
    } finally {
      setSyncing(false);
    }
  };

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      await loadData();
      if (cancelled) return;
      setLoading(false);
    };
    load();
    return () => { cancelled = true; };
  }, [activeWorkspace?.id, loadData]);

  useEffect(() => {
    const interval = setInterval(() => loadData(), 60000);
    return () => clearInterval(interval);
  }, [loadData]);

  useEffect(() => {
    if (toast) {
      const timer = setTimeout(() => setToast(null), 3000);
      return () => clearTimeout(timer);
    }
  }, [toast]);

  // Team action handlers
  const handleCreateTeam = async (data: CreateTeamData) => {
    setActionLoading("createTeam");
    try {
      const newTeam = await createTeam(data);
      setTeams((prev) => [newTeam, ...prev]);
      setShowCreateModal(false);
      showToast(`Team "${newTeam.name}" created successfully`);
      handleSwitchTeam(newTeam);
      await loadData();
    } catch (err: any) {
      showToast(err?.message || "Failed to create team", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleEditTeam = async (id: string, data: CreateTeamData) => {
    setActionLoading("editTeam");
    try {
      const updated = await updateTeam(id, data);
      if (updated) {
        setTeams((prev) => prev.map((t) => (t.id === id ? updated : t)));
        setEditingTeam(null);
        showToast(`Team "${updated.name}" updated`);
        await loadData();
      } else {
        showToast("Failed to update team", "error");
      }
    } catch (err: any) {
      showToast(err?.message || "Failed to update team", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleDeleteTeam = (team: Team) => {
    setDeletingTeams([team]);
  };

  const handleConfirmDeleteTeams = async () => {
    if (deletingTeams.length === 0) return;
    setActionLoading("deleteTeam");
    try {
      for (const t of deletingTeams) {
        await deleteTeam(t.id);
      }
      setTeams((prev) => prev.filter((t) => !deletingTeams.some((d) => d.id === t.id)));
      setDeletingTeams([]);
      setSelectedTeamIds([]);
      showToast(`${deletingTeams.length} team(s) deleted`);
      await loadData();
    } catch (err: any) {
      showToast(err?.message || "Failed to delete team(s)", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleSelectTeam = (id: string, selected: boolean) => {
    setSelectedTeamIds((prev) => (selected ? [...prev, id] : prev.filter((x) => x !== id)));
  };

  const handleBulkDeleteTeams = () => {
    const toDelete = teams.filter((t) => selectedTeamIds.includes(t.id));
    setDeletingTeams(toDelete);
  };

  // Member action handlers
  const handleEditMember = async (id: string, role: MemberRole) => {
    const member = members.find((m) => m.id === id);
    if (member?.status === "Pending") {
      showToast("Cannot change role of a pending invitation", "error");
      return;
    }
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
        await removeMember(member.id, member.status);
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
      setMembers((prev) => {
        if (prev.some((m) => m.id === newMember.id)) return prev;
        return [newMember, ...prev];
      });
      setShowInviteModal(false);
      showToast(`Invitation sent to ${newMember.email}`);
    } catch (err) {
      const message = err instanceof Error ? err.message : "Failed to send invitation";
      showToast(message, "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleUpdateQuota = async (memberId: string, mode: QuotaMode, limit: number | null) => {
    setQuotaLoading(true);
    try {
      const result = await updateMemberQuota(memberId, mode, limit);
      if (result.success) {
        setMembers((prev) => prev.map((m) => (m.id === memberId ? { ...m, quotaMode: mode, creditLimit: limit } : m)));
        showToast("Member quota updated");
      } else {
        showToast(result.message || "Failed to update quota", "error");
      }
    } catch {
      showToast("Failed to update quota", "error");
    } finally {
      setQuotaLoading(false);
    }
  };

  const handleOpenTransferOwnership = (member: TeamMember) => {
    if (member.status !== "Active" || member.role !== "Manager") {
      showToast("Ownership can only be transferred to an active manager", "error");
      return;
    }
    setTransferringOwnerMember(member);
  };

  const handleConfirmTransferOwnership = async () => {
    if (!transferringOwnerMember) return;
    setActionLoading("transferOwnership");
    try {
      await transferWorkspaceOwnership(transferringOwnerMember.id);
      setDetailMember(null);
      setTransferringOwnerMember(null);
      invalidateWorkspaceCache();
      await Promise.all([loadData(), refetchWorkspaces()]);
      showToast(`Ownership transferred to ${transferringOwnerMember.name}`);
    } catch (err) {
      const message = err instanceof Error ? err.message : "Failed to transfer ownership";
      showToast(message, "error");
    } finally {
      setActionLoading(null);
    }
  };

  const currentTeam = useMemo(() => {
    if (!teams || teams.length === 0) return null;
    return teams.find((t) => t.id === activeTeam?.id) || teams[0];
  }, [teams, activeTeam?.id]);

  const teamMembers = useMemo(() => {
    if (!currentTeam) return [];
    return members.filter((m) => m.teamIds.includes(currentTeam.id) || currentTeam.memberIds.includes(m.id));
  }, [members, currentTeam]);

  const filteredMembers = useMemo(() => {
    let result = [...teamMembers];
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
        result.sort((a, b) => {
          if (a.role === "Owner") return -1;
          if (b.role === "Owner") return 1;
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        });
        break;
      case "oldest":
        result.sort((a, b) => {
          if (a.role === "Owner") return -1;
          if (b.role === "Owner") return 1;
          return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
        });
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
  }, [teamMembers, search, statusFilter, sortBy]);

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
            <p className="text-body-md text-on-surface-variant mb-6">This feature requires a <strong>Business plan</strong>. Upgrade to manage team members.</p>
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
        <div className="max-w-7xl mx-auto space-y-8">

          {/* Page Header */}
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 animate-fade-up">
            <div className="flex items-center gap-4">
              <div className="relative w-12 h-12 shrink-0">
                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-primary to-primary/70 animate-float shadow-lg shadow-primary/20" />
                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-white/15 to-transparent" />
                <div className="relative w-full h-full flex items-center justify-center">
                  <span className="material-symbols-outlined text-on-primary text-[24px]">group</span>
                </div>
              </div>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">
                  {activeWorkspace?.name || "Workspace"} Teams &amp; Members
                </h1>
                <p className="text-label-sm text-outline">
                  {teams.length} teams · {activeMemberCount} active members · {getWorkspaceTypeLabel(activeWorkspace?.workspaceType || 0)}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <button
                onClick={() => loadData()}
                className="px-4 py-2.5 rounded-xl border border-outline-variant/20 text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all flex items-center gap-2"
                title="Refresh data"
              >
                <span className="material-symbols-outlined text-[16px]">refresh</span>
              </button>
              {isOwner && (
                <>
                  <button
                    onClick={handleSyncTeam}
                    disabled={syncing}
                    className="px-4 py-2.5 rounded-xl border border-outline-variant/30 text-label-sm font-semibold text-on-surface hover:bg-surface-container hover:scale-105 transition-all flex items-center gap-2 disabled:opacity-50"
                    title="Đồng bộ tất cả thành viên & thương hiệu vào team để chia sẻ chung"
                  >
                    <span className={`material-symbols-outlined text-[16px] ${syncing ? "animate-spin" : ""}`}>sync</span>
                    {syncing ? "Đang đồng bộ..." : "Đồng bộ team"}
                  </button>
                  <button
                    onClick={() => setShowCreateModal(true)}
                    className="px-5 py-2.5 rounded-xl bg-surface-container-high border border-outline-variant/30 text-on-surface text-label-sm font-bold hover:bg-surface-container hover:scale-105 transition-all flex items-center gap-2"
                  >
                    <span className="material-symbols-outlined text-[16px]">group_add</span>
                    Create Team
                  </button>
                  <button
                    onClick={() => setShowInviteModal(true)}
                    className="px-5 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 flex items-center gap-2"
                  >
                    <span className="material-symbols-outlined text-[16px]">person_add</span>
                    Invite Member
                  </button>
                </>
              )}
            </div>
          </div>

          {/* Stats Overview */}
          <TeamStatsCards teams={teams} members={members} />

          {/* Teams Section */}
          <section className="animate-fade-up" style={{ animationDelay: "0.15s" }}>
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-3">
                <div className="flex items-center gap-2.5">
                  <h2 className="text-headline-sm text-on-surface font-semibold">Teams</h2>
                  <span className="px-2 py-0.5 rounded-full text-label-xs font-bold bg-secondary/10 text-secondary">
                    {teams.length}
                  </span>
                </div>

                {/* Team Switcher - Visible to all roles if teams exist */}
                {teams.length > 0 && (
                  <OwnerTeamSwitcher
                    workspaceId={activeWorkspace?.id}
                    isOwner={isOwner}
                    role={activeWorkspace?.memberRole || (isOwner ? "Owner" : undefined)}
                    teams={teams}
                    onTeamSwitched={(t) => {
                      setActiveTeam(t);
                      showToast(`Đã chuyển sang team "${t.name}"`);
                    }}
                  />
                )}

                {/* Quick Create Team Button for Owner */}
                {isOwner && (
                  <button
                    type="button"
                    onClick={() => setShowCreateModal(true)}
                    className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-primary text-on-primary text-label-xs font-bold shadow-xs hover:scale-105 active:scale-95 transition-all"
                    title="Tạo team mới trong workspace"
                  >
                    <span className="material-symbols-outlined text-[15px]">group_add</span>
                    <span>Tạo team</span>
                  </button>
                )}
              </div>
              <div className="flex items-center gap-2 bg-surface-container-low rounded-lg p-1">
                <button
                  onClick={() => setTeamView("grid")}
                  className={`p-1.5 rounded-md transition-all ${
                    teamView === "grid" ? "bg-surface-container text-primary shadow-sm" : "text-outline hover:text-on-surface"
                  }`}
                  title="Grid view"
                >
                  <span className="material-symbols-outlined text-[18px]">grid_view</span>
                </button>
                <button
                  onClick={() => setTeamView("table")}
                  className={`p-1.5 rounded-md transition-all ${
                    teamView === "table" ? "bg-surface-container text-primary shadow-sm" : "text-outline hover:text-on-surface"
                  }`}
                  title="Table view"
                >
                  <span className="material-symbols-outlined text-[18px]">view_list</span>
                </button>
              </div>
            </div>

            {loading ? (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
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
                  </div>
                ))}
              </div>
            ) : teams.length === 0 ? (
              <div className="bg-surface-container-lowest/80 border border-outline-variant/30 rounded-2xl p-8 text-center">
                <div className="w-12 h-12 rounded-2xl bg-secondary/10 text-secondary flex items-center justify-center mx-auto mb-3">
                  <span className="material-symbols-outlined text-[24px]">schema</span>
                </div>
                <h3 className="text-body-md font-bold text-on-surface mb-1">No teams yet</h3>
                <p className="text-body-sm text-outline mb-4 max-w-md mx-auto">
                  Create teams to group workspace members and assign specific brand permissions to each team.
                </p>
                {isOwner && (
                  <button
                    onClick={() => setShowCreateModal(true)}
                    className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-md hover:scale-105 transition-all"
                  >
                    <span className="material-symbols-outlined text-[16px]">group_add</span>
                    Create First Team
                  </button>
                )}
              </div>
            ) : teamView === "grid" ? (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                {teams.map((team, i) => (
                  <TeamCard
                    key={team.id}
                    team={team}
                    index={i}
                    isSelected={selectedTeamIds.includes(team.id)}
                    isLoading={actionLoading === team.id}
                    isActiveTeam={activeTeam?.id === team.id}
                    isOwner={isOwner}
                    onSelect={handleSelectTeam}
                    onViewDetail={setDetailTeam}
                    onEdit={setEditingTeam}
                    onDelete={handleDeleteTeam}
                    onSwitchTeam={handleSwitchTeam}
                  />
                ))}
              </div>
            ) : (
              <TeamListView
                teams={teams}
                selectedIds={selectedTeamIds}
                actionLoading={actionLoading}
                activeTeamId={activeTeam?.id}
                isOwner={isOwner}
                onSelect={handleSelectTeam}
                onViewDetail={setDetailTeam}
                onEdit={setEditingTeam}
                onDelete={handleDeleteTeam}
                onSwitchTeam={handleSwitchTeam}
              />
            )}

            {/* Bulk Actions for Teams */}
            <BulkActionsBar
              selectedCount={selectedTeamIds.length}
              onClearSelection={() => setSelectedTeamIds([])}
              onBulkDelete={handleBulkDeleteTeams}
              isLoading={actionLoading === "deleteTeam"}
            />
          </section>

          {/* Members Section */}
          <section className="animate-fade-up" style={{ animationDelay: "0.25s" }}>
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-4">
              <div className="flex items-center gap-2.5 flex-wrap">
                <h2 className="text-headline-sm text-on-surface font-semibold">Thành viên làm việc</h2>
                {currentTeam && (
                  <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-label-xs font-bold bg-primary/10 text-primary border border-primary/20 shadow-xs">
                    <span className="w-2 h-2 rounded-full bg-primary animate-pulse-dot" />
                    Team: {currentTeam.name}
                  </span>
                )}
                <span className="px-2 py-0.5 rounded-full text-label-xs font-bold bg-surface-container-high text-on-surface-variant">
                  {teamMembers.length} thành viên
                </span>
              </div>
              <div className="flex items-center gap-2 bg-surface-container-low rounded-lg p-1 self-end sm:self-auto">
                <button
                  onClick={() => setMemberView("grid")}
                  className={`p-1.5 rounded-md transition-all ${
                    memberView === "grid" ? "bg-surface-container text-primary shadow-sm" : "text-outline hover:text-on-surface"
                  }`}
                  title="Grid view"
                >
                  <span className="material-symbols-outlined text-[18px]">grid_view</span>
                </button>
                <button
                  onClick={() => setMemberView("table")}
                  className={`p-1.5 rounded-md transition-all ${
                    memberView === "table" ? "bg-surface-container text-primary shadow-sm" : "text-outline hover:text-on-surface"
                  }`}
                  title="Table view"
                >
                  <span className="material-symbols-outlined text-[18px]">view_list</span>
                </button>
              </div>
            </div>

            <TeamFilterBar
              search={search}
              onSearchChange={setSearch}
              statusFilter={statusFilter}
              onStatusFilterChange={setStatusFilter}
              sortBy={sortBy}
              onSortChange={setSortBy}
              resultCount={filteredMembers.length}
              totalCount={teamMembers.length}
              teams={teams}
              selectedTeamId={currentTeam?.id}
              onTeamChange={(teamId) => {
                const target = teams.find((t) => t.id === teamId);
                if (target) handleSwitchTeam(target);
              }}
            />

            {loading ? (
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
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
                  </div>
                ))}
              </div>
            ) : teams.length === 0 ? (
              <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-8 text-center animate-fade-up">
                <div className="w-14 h-14 rounded-2xl bg-primary/10 text-primary flex items-center justify-center mx-auto mb-3">
                  <span className="material-symbols-outlined text-[28px]">groups</span>
                </div>
                <h3 className="text-body-lg font-bold text-on-surface mb-1">Chưa có team làm việc nào</h3>
                <p className="text-label-sm text-outline max-w-md mx-auto mb-5">
                  Tạo team để phân quyền và quản lý các thành viên làm việc theo từng nhóm. Mặc định bạn sẽ là thành viên đầu tiên.
                </p>
                {isOwner && (
                  <button
                    onClick={() => setShowCreateModal(true)}
                    className="inline-flex items-center gap-2 px-5 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-md shadow-primary/20 hover:scale-105 transition-all"
                  >
                    <span className="material-symbols-outlined text-[18px]">group_add</span>
                    Tạo Team ngay
                  </button>
                )}
              </div>
            ) : filteredMembers.length === 0 ? (
              <TeamEmptyState
                hasFilters={hasFilters}
                onCreate={() => (currentTeam ? setEditingTeam(currentTeam) : setShowCreateModal(true))}
                onInvite={() => setShowInviteModal(true)}
                isOwner={isOwner}
              />
            ) : (
              <>
                {!hasFilters && filteredMembers.length > 0 && (
                  <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 p-6 shadow-sm mb-6">
                    <h3 className="text-label-sm font-bold text-on-surface mb-4">Role Distribution</h3>
                    <RoleDonutChart members={filteredMembers} />
                  </div>
                )}

                {memberView === "grid" ? (
                  <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                    {filteredMembers.map((member) => (
                      <MemberCard
                        key={member.id}
                        member={member}
                        isOwner={isOwner}
                        onViewDetail={setDetailMember}
                        onEdit={setEditingMember}
                        onDelete={handleDeleteMember}
                      />
                    ))}
                  </div>
                ) : (
                  <div className="bg-surface-container-lowest/80 backdrop-blur-sm rounded-2xl border border-outline-variant/30 shadow-sm overflow-hidden">
                    <div className="overflow-x-auto">
                      <table className="w-full">
                        <thead>
                          <tr className="border-b border-outline-variant/20 bg-surface-container-low/50">
                            <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Member</th>
                            <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Role</th>
                            <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Quota / Usage</th>
                            <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Status</th>
                            <th className="px-6 py-3.5 text-left text-label-xs text-outline font-bold uppercase tracking-wider">Joined</th>
                            {isOwner && (
                              <th className="px-6 py-3.5 text-right text-label-xs text-outline font-bold uppercase tracking-wider">Actions</th>
                            )}
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
                                {member.quotaMode !== "SharedPool" ? (
                                  <div className="flex flex-col">
                                    <div className="flex items-center gap-1.5">
                                      <span className="text-label-xs text-outline">
                                        <span className="text-body-sm text-on-surface font-semibold">{member.creditUsed.toLocaleString()}</span>
                                        {member.creditLimit != null ? ` / ${member.creditLimit.toLocaleString()} credits` : " credits"}
                                      </span>
                                    </div>
                                    {member.creditLimit != null && (
                                      <div className="w-20 h-1.5 bg-surface-container rounded-full overflow-hidden mt-1">
                                        <div
                                          className="h-full bg-primary rounded-full transition-all"
                                          style={{ width: `${Math.min(100, (member.creditUsed / member.creditLimit) * 100)}%` }}
                                        />
                                      </div>
                                    )}
                                    <span className="text-label-2xs text-outline mt-0.5">
                                      {member.quotaMode === "LifetimeAssigned" ? "Lifetime" : "Monthly"}
                                    </span>
                                  </div>
                                ) : (
                                  <span className="text-label-sm text-outline">Shared</span>
                                )}
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
                              {isOwner && (
                                <td className="px-6 py-4 text-right">
                                  <div className="flex justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                    {member.role === "Manager" && member.status === "Active" && (
                                      <button
                                        onClick={(e) => { e.stopPropagation(); handleOpenTransferOwnership(member); }}
                                        className="p-1.5 rounded-lg text-outline hover:text-amber-600 hover:bg-amber-50 transition-all"
                                        title="Transfer ownership"
                                      >
                                        <span className="material-symbols-outlined text-[16px]">crown</span>
                                      </button>
                                    )}
                                    <button
                                      onClick={(e) => { e.stopPropagation(); setEditingMember(member); }}
                                      className={`p-1.5 rounded-lg transition-all ${
                                        member.status === "Pending"
                                          ? "text-outline/30 cursor-not-allowed"
                                          : "text-outline hover:text-primary hover:bg-primary/10"
                                      }`}
                                      title={member.status === "Pending" ? "Role can be changed after acceptance" : "Edit member"}
                                      disabled={member.status === "Pending"}
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
                              )}
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
          onTransferOwnership={handleOpenTransferOwnership}
          isOwner={isOwner}
          isTransferringOwnership={actionLoading === "transferOwnership"}
        />

        <CreateTeamModal
          open={showCreateModal}
          onClose={() => setShowCreateModal(false)}
          onCreate={handleCreateTeam}
          isLoading={actionLoading === "createTeam"}
        />

        <EditTeamModal
          team={editingTeam}
          onClose={() => setEditingTeam(null)}
          onUpdate={handleEditTeam}
          isLoading={actionLoading === "editTeam"}
        />

        <TeamDetailModal
          team={detailTeam}
          members={members}
          onClose={() => setDetailTeam(null)}
        />

        <DeleteConfirmModal
          teams={deletingTeams}
          isLoading={actionLoading === "deleteTeam"}
          onConfirm={handleConfirmDeleteTeams}
          onCancel={() => setDeletingTeams([])}
        />

        <EditMemberModal
          member={editingMember}
          onClose={() => setEditingMember(null)}
          onUpdate={handleEditMember}
          isLoading={actionLoading === "editMember"}
          canAssignQuota={canAssignQuota}
          onUpdateQuota={handleUpdateQuota}
          isUpdatingQuota={quotaLoading}
        />

        <DeleteMemberConfirmModal
          members={deletingMembers}
          isLoading={actionLoading === "deleteMember"}
          onConfirm={handleConfirmDeleteMember}
          onCancel={() => setDeletingMembers([])}
        />

        <TransferOwnershipConfirmModal
          member={transferringOwnerMember}
          isLoading={actionLoading === "transferOwnership"}
          onConfirm={handleConfirmTransferOwnership}
          onCancel={() => setTransferringOwnerMember(null)}
        />

        <InviteMemberModal
          open={showInviteModal}
          onClose={() => setShowInviteModal(false)}
          onInvite={handleInvite}
          isLoading={actionLoading === "invite"}
          currentMemberCount={activeMemberCount}
          maxMembers={activeWorkspace?.memberLimit ?? 1}
          canAssignQuota={canAssignQuota}
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
