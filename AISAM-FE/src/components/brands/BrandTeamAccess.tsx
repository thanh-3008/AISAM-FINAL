"use client";

import { useState, useEffect, useCallback } from "react";
import {
  readAssignments,
  changeAssignment,
  type AssignmentSnapshot,
} from "@/services/permissionService";
import { fetchTeams, type Team } from "@/services/teamService";
import { fetchSocialIntegrations } from "@/services/socialAccountService";

interface BrandTeamAccessProps {
  brandId: string;
  brandName: string;
  isOwner: boolean;
}

interface Integration {
  id: string;
  platform: string;
  accountName: string;
  isActive: boolean;
}

interface TeamAccessRow {
  teamBrandId: string;
  teamId: string;
  teamName: string;
  isActive: boolean;
  channels: {
    integrationId: string;
    canView: boolean;
    canPublish: boolean;
    canManage: boolean;
  }[];
}

export default function BrandTeamAccess({ brandId, brandName, isOwner }: BrandTeamAccessProps) {
  const [snapshot, setSnapshot] = useState<AssignmentSnapshot | null>(null);
  const [teams, setTeams] = useState<Team[]>([]);
  const [integrations, setIntegrations] = useState<Integration[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showAssign, setShowAssign] = useState(false);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [snap, teamsRes, intItems] = await Promise.all([
        readAssignments(brandId).catch(() => null),
        fetchTeams(),
        fetchSocialIntegrations(brandId).catch(() => []),
      ]);
      setSnapshot(snap);
      setTeams(teamsRes.data);
      setIntegrations(
        intItems.map((i) => ({
          id: i.id,
          platform: i.provider,
          accountName: i.accountName,
          isActive: i.isActive,
        }))
      );
    } catch {
      setError("Failed to load team access data");
    }
    setLoading(false);
  }, [brandId]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Build team access rows from snapshot
  const teamRows: TeamAccessRow[] = (snapshot?.teams || [])
    .filter((t) => t.isActive)
    .map((t) => {
      const team = teams.find((tm) => tm.id === t.teamId);
      return {
        teamBrandId: t.id,
        teamId: t.teamId,
        teamName: team?.name || "Unknown Team",
        isActive: t.isActive,
        channels: (snapshot?.channels || [])
          .filter((c) => c.teamBrandId === t.id)
          .map((c) => ({
            integrationId: c.integrationId,
            canView: c.canView,
            canPublish: c.canPublish,
            canManage: c.canManage,
          })),
      };
    });

  const assignedTeamIds = new Set(teamRows.map((r) => r.teamId));
  const unassignedTeams = teams.filter((t) => !assignedTeamIds.has(t.id));

  const handleAssignTeam = async (teamId: string) => {
    if (!snapshot) return;
    setSaving(true);
    setError(null);
    try {
      const newSnap = await changeAssignment(brandId, teamId, snapshot.revision, true);
      setSnapshot(newSnap);
      setShowAssign(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to assign team");
    }
    setSaving(false);
  };

  const handleRevokeTeam = async (teamId: string) => {
    if (!snapshot) return;
    setSaving(true);
    setError(null);
    try {
      const newSnap = await changeAssignment(brandId, teamId, snapshot.revision, false);
      setSnapshot(newSnap);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to revoke team");
    }
    setSaving(false);
  };

  const handleChannelChange = async (
    teamId: string,
    integrationId: string,
    field: "canView" | "canPublish" | "canManage",
    value: boolean
  ) => {
    if (!snapshot) return;
    setSaving(true);
    setError(null);
    try {
      const row = teamRows.find((r) => r.teamId === teamId);
      const existing = row?.channels.find((c) => c.integrationId === integrationId);
      const perms = {
        canView: existing?.canView ?? false,
        canPublish: existing?.canPublish ?? false,
        canManage: existing?.canManage ?? false,
        [field]: value,
      };
      // Enforce: publish/manage requires view
      if (field === "canView" && !value) {
        perms.canPublish = false;
        perms.canManage = false;
      }
      const newSnap = await changeAssignment(brandId, teamId, snapshot.revision, true, {
        id: integrationId,
        ...perms,
      });
      setSnapshot(newSnap);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update channel permissions");
    }
    setSaving(false);
  };

  const platformIcon = (platform: string) => {
    switch (platform?.toLowerCase()) {
      case "facebook": return "facebook";
      case "instagram": return "photo_camera";
      case "tiktok": return "music_note";
      default: return "share";
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-body-lg font-bold text-on-surface flex items-center gap-2">
          <span className="material-symbols-outlined text-[20px] text-primary">groups</span>
          Team Access
        </h3>
        {isOwner && (
          <button
            onClick={() => setShowAssign(true)}
            disabled={saving}
            className="px-3 py-1.5 text-label-sm font-bold text-primary bg-primary/10 rounded-lg hover:bg-primary/20 transition-all flex items-center gap-1 disabled:opacity-50"
          >
            <span className="material-symbols-outlined text-[14px]">add</span>
            Assign Team
          </button>
        )}
      </div>

      {error && (
        <div className="p-3 bg-danger-red/10 border border-danger-red/20 rounded-xl text-body-sm text-danger-red flex items-center gap-2">
          <span className="material-symbols-outlined text-[16px]">error</span>
          {error}
          <button onClick={() => setError(null)} className="ml-auto">
            <span className="material-symbols-outlined text-[14px]">close</span>
          </button>
        </div>
      )}

      {loading ? (
        <div className="flex items-center justify-center py-8">
          <div className="w-6 h-6 border-2 border-primary/20 border-t-primary rounded-full animate-spin" />
        </div>
      ) : teamRows.length === 0 ? (
        <div className="text-center py-8 bg-surface-container-low rounded-xl">
          <span className="material-symbols-outlined text-[32px] text-outline mb-2 block">group_off</span>
          <p className="text-body-sm text-outline">No teams assigned to this brand.</p>
          {isOwner && (
            <button
              onClick={() => setShowAssign(true)}
              className="mt-3 px-4 py-2 text-label-sm font-bold text-primary bg-primary/10 rounded-lg hover:bg-primary/20 transition-all"
            >
              Assign a Team
            </button>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {teamRows.map((row) => (
            <div key={row.teamId} className="border border-outline-variant/20 rounded-xl overflow-hidden">
              {/* Team Header */}
              <div className="flex items-center justify-between p-4 bg-surface-container-low">
                <div className="flex items-center gap-2">
                  <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center">
                    <span className="material-symbols-outlined text-[16px] text-primary">groups</span>
                  </div>
                  <span className="text-body-sm font-bold text-on-surface">{row.teamName}</span>
                </div>
                {isOwner && (
                  <button
                    onClick={() => handleRevokeTeam(row.teamId)}
                    disabled={saving}
                    className="px-2.5 py-1 text-label-2xs font-bold text-danger-red bg-danger-red/10 rounded-lg hover:bg-danger-red/20 transition-all disabled:opacity-50"
                  >
                    Revoke
                  </button>
                )}
              </div>

              {/* Channel Permissions */}
              {integrations.length > 0 && (
                <div className="p-4 space-y-2">
                  <p className="text-label-2xs text-outline uppercase font-bold tracking-widest mb-2">
                    Channel Permissions
                  </p>
                  {integrations.map((integration) => {
                    const perm = row.channels.find((c) => c.integrationId === integration.id);
                    return (
                      <div
                        key={integration.id}
                        className="flex items-center gap-3 p-2 bg-surface-container-low/50 rounded-lg"
                      >
                        <span className="material-symbols-outlined text-[16px] text-outline">
                          {platformIcon(integration.platform)}
                        </span>
                        <span className="text-body-sm text-on-surface flex-1 truncate">
                          {integration.accountName || integration.platform}
                        </span>
                        {isOwner ? (
                          <div className="flex items-center gap-3">
                            <label className="flex items-center gap-1 cursor-pointer">
                              <input
                                type="checkbox"
                                checked={perm?.canView ?? false}
                                onChange={(e) =>
                                  handleChannelChange(row.teamId, integration.id, "canView", e.target.checked)
                                }
                                disabled={saving}
                                className="w-3.5 h-3.5 rounded accent-primary"
                              />
                              <span className="text-label-2xs text-outline">View</span>
                            </label>
                            <label className="flex items-center gap-1 cursor-pointer">
                              <input
                                type="checkbox"
                                checked={perm?.canPublish ?? false}
                                disabled={saving || !(perm?.canView)}
                                onChange={(e) =>
                                  handleChannelChange(row.teamId, integration.id, "canPublish", e.target.checked)
                                }
                                className="w-3.5 h-3.5 rounded accent-primary disabled:opacity-40"
                              />
                              <span className="text-label-2xs text-outline">Publish</span>
                            </label>
                            <label className="flex items-center gap-1 cursor-pointer">
                              <input
                                type="checkbox"
                                checked={perm?.canManage ?? false}
                                disabled={saving || !(perm?.canView)}
                                onChange={(e) =>
                                  handleChannelChange(row.teamId, integration.id, "canManage", e.target.checked)
                                }
                                className="w-3.5 h-3.5 rounded accent-primary disabled:opacity-40"
                              />
                              <span className="text-label-2xs text-outline">Manage</span>
                            </label>
                          </div>
                        ) : (
                          <div className="flex items-center gap-2">
                            {perm?.canView && (
                              <span className="px-1.5 py-0.5 bg-emerald-100 text-emerald-700 rounded text-label-2xs">View</span>
                            )}
                            {perm?.canPublish && (
                              <span className="px-1.5 py-0.5 bg-blue-100 text-blue-700 rounded text-label-2xs">Publish</span>
                            )}
                            {perm?.canManage && (
                              <span className="px-1.5 py-0.5 bg-purple-100 text-purple-700 rounded text-label-2xs">Manage</span>
                            )}
                            {!perm?.canView && !perm?.canPublish && !perm?.canManage && (
                              <span className="text-label-2xs text-outline">No access</span>
                            )}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Assign Team Overlay */}
      {showAssign && (
        <>
          <div className="fixed inset-0 bg-black/30 z-[60]" onClick={() => setShowAssign(false)} />
          <div className="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-[60] w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[60vh] flex flex-col">
            <div className="p-5 border-b border-outline-variant/20 flex items-center justify-between">
              <h3 className="text-body-lg font-bold text-on-surface">Assign Team to {brandName}</h3>
              <button onClick={() => setShowAssign(false)} className="p-1.5 hover:bg-surface-container rounded-full">
                <span className="material-symbols-outlined text-[18px]">close</span>
              </button>
            </div>
            <div className="flex-1 overflow-y-auto p-5 space-y-2">
              {unassignedTeams.length === 0 ? (
                <p className="text-body-sm text-outline text-center py-6">All teams are already assigned.</p>
              ) : (
                unassignedTeams.map((team) => {
                  const cannotAssign = saving || team.hasManager === false;
                  return (
                    <button
                      key={team.id}
                      onClick={() => handleAssignTeam(team.id)}
                      disabled={cannotAssign}
                      className="w-full flex items-center gap-3 p-3 bg-surface-container-low rounded-xl hover:bg-primary/5 transition-all text-left disabled:opacity-50"
                    >
                      <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center">
                        <span className="material-symbols-outlined text-[16px] text-primary">groups</span>
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-body-sm font-semibold text-on-surface">{team.name}</p>
                        <p className="text-label-xs text-outline">
                          {team.memberCount} members · {team.brandCount} brands
                          {team.hasManager === false && (
                            <span className="text-warning-amber ml-2 font-medium">
                              (Cần có Manager)
                            </span>
                          )}
                        </p>
                      </div>
                      <span className="material-symbols-outlined text-[18px] text-primary">add_circle</span>
                    </button>
                  );
                })
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
