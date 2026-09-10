"use client";

import { useState, useEffect, useCallback } from "react";
import {
  getTeamById,
  updateTeam,
  deleteTeam,
  addTeamMember,
  removeTeamMember,
  fetchMembers,
  type TeamDetail,
  type TeamMember,
} from "@/services/teamService";

interface TeamDetailPanelProps {
  teamId: string | null;
  onClose: () => void;
  onUpdated: () => void;
  isOwner: boolean;
}

export default function TeamDetailPanel({ teamId, onClose, onUpdated, isOwner }: TeamDetailPanelProps) {
  const [team, setTeam] = useState<TeamDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState(false);
  const [editName, setEditName] = useState("");
  const [editDesc, setEditDesc] = useState("");
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [confirmDelete, setConfirmDelete] = useState(false);

  // Add member state
  const [showAddMember, setShowAddMember] = useState(false);
  const [workspaceMembers, setWorkspaceMembers] = useState<TeamMember[]>([]);
  const [addingMember, setAddingMember] = useState(false);

  useEffect(() => {
    if (!teamId) {
      setTeam(null);
      return;
    }
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await getTeamById(teamId);
        setTeam(data);
        if (data) {
          setEditName(data.name);
          setEditDesc(data.description || "");
        }
      } catch {
        setError("Failed to load team");
      }
      setLoading(false);
    };
    load();
  }, [teamId]);

  const loadWorkspaceMembers = useCallback(async () => {
    try {
      const { data } = await fetchMembers();
      setWorkspaceMembers(data.filter((m) => m.status === "Active"));
    } catch {
      // ignore
    }
  }, []);

  const handleSave = async () => {
    if (!team || !editName.trim()) return;
    setSaving(true);
    setError(null);
    try {
      const updated = await updateTeam(team.id, { name: editName.trim(), description: editDesc.trim() });
      setTeam(updated);
      setEditing(false);
      onUpdated();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update team");
    }
    setSaving(false);
  };

  const handleDelete = async () => {
    if (!team) return;
    setSaving(true);
    try {
      await deleteTeam(team.id);
      onClose();
      onUpdated();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete team");
    }
    setSaving(false);
  };

  const handleAddMember = async (userId: string, role: string) => {
    if (!team) return;
    setAddingMember(true);
    try {
      await addTeamMember(team.id, userId, role);
      // Reload team detail
      const updated = await getTeamById(team.id);
      setTeam(updated);
      setShowAddMember(false);
      onUpdated();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to add member");
    }
    setAddingMember(false);
  };

  const handleRemoveMember = async (userId: string) => {
    if (!team) return;
    try {
      await removeTeamMember(team.id, userId);
      const updated = await getTeamById(team.id);
      setTeam(updated);
      onUpdated();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to remove member");
    }
  };

  if (!teamId) return null;

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div
          className="w-full max-w-xl bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[85vh] flex flex-col"
          onClick={(e) => e.stopPropagation()}
        >
          {/* Header */}
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between shrink-0">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">groups</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">
                  {team?.name || "Team Details"}
                </h2>
                <p className="text-label-xs text-outline">{team?.status || "Loading..."}</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          {/* Content */}
          <div className="flex-1 overflow-y-auto p-6 space-y-5">
            {loading ? (
              <div className="flex items-center justify-center py-12">
                <div className="w-8 h-8 border-3 border-primary/20 border-t-primary rounded-full animate-spin" />
              </div>
            ) : !team ? (
              <p className="text-body-sm text-outline text-center py-8">Team not found.</p>
            ) : (
              <>
                {error && (
                  <div className="p-3 bg-danger-red/10 border border-danger-red/20 rounded-xl text-body-sm text-danger-red flex items-center gap-2">
                    <span className="material-symbols-outlined text-[16px]">error</span>
                    {error}
                    <button onClick={() => setError(null)} className="ml-auto">
                      <span className="material-symbols-outlined text-[14px]">close</span>
                    </button>
                  </div>
                )}

                {/* Team Info */}
                <div className="bg-surface-container-low rounded-xl p-4">
                  {editing ? (
                    <div className="space-y-3">
                      <input
                        type="text"
                        value={editName}
                        onChange={(e) => setEditName(e.target.value)}
                        className="w-full p-2.5 bg-surface-container-lowest border border-outline-variant/20 rounded-lg text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20"
                        maxLength={255}
                      />
                      <textarea
                        value={editDesc}
                        onChange={(e) => setEditDesc(e.target.value)}
                        rows={2}
                        className="w-full p-2.5 bg-surface-container-lowest border border-outline-variant/20 rounded-lg text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 resize-none"
                        maxLength={1000}
                      />
                      <div className="flex gap-2">
                        <button
                          onClick={handleSave}
                          disabled={saving || !editName.trim()}
                          className="px-4 py-2 bg-primary text-on-primary rounded-lg text-label-sm font-bold disabled:opacity-50"
                        >
                          {saving ? "Saving..." : "Save"}
                        </button>
                        <button
                          onClick={() => { setEditing(false); setEditName(team.name); setEditDesc(team.description || ""); }}
                          className="px-4 py-2 border border-outline-variant/20 rounded-lg text-label-sm text-outline"
                        >
                          Cancel
                        </button>
                      </div>
                    </div>
                  ) : (
                    <div className="flex items-start justify-between">
                      <div>
                        <h3 className="text-body-md font-bold text-on-surface">{team.name}</h3>
                        {team.description && (
                          <p className="text-body-sm text-on-surface-variant mt-1">{team.description}</p>
                        )}
                        <p className="text-label-xs text-outline mt-2">
                          Created {new Date(team.createdAt).toLocaleDateString()}
                        </p>
                      </div>
                      {isOwner && (
                        <button
                          onClick={() => setEditing(true)}
                          className="p-1.5 hover:bg-surface-container rounded-lg text-outline hover:text-primary transition-all"
                        >
                          <span className="material-symbols-outlined text-[16px]">edit</span>
                        </button>
                      )}
                    </div>
                  )}
                </div>

                {/* Members */}
                <div>
                  <div className="flex items-center justify-between mb-3">
                    <h4 className="text-label-sm font-bold text-on-surface flex items-center gap-2">
                      <span className="material-symbols-outlined text-[16px] text-primary">group</span>
                      Members ({team.members.length})
                    </h4>
                    {isOwner && (
                      <button
                        onClick={() => { setShowAddMember(true); loadWorkspaceMembers(); }}
                        className="px-3 py-1.5 text-label-2xs font-bold text-primary bg-primary/10 rounded-lg hover:bg-primary/20 transition-all flex items-center gap-1"
                      >
                        <span className="material-symbols-outlined text-[14px]">person_add</span>
                        Add
                      </button>
                    )}
                  </div>
                  <div className="space-y-2">
                    {team.members.map((m) => (
                      <div
                        key={m.userId}
                        className="flex items-center gap-3 p-3 bg-surface-container-low rounded-xl group"
                      >
                        <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center text-label-2xs font-bold text-primary">
                          {m.name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2)}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-body-sm font-semibold text-on-surface truncate">{m.name}</p>
                          <p className="text-label-xs text-outline truncate">{m.email}</p>
                        </div>
                        <span className="px-2 py-0.5 rounded-full text-label-2xs font-bold bg-surface-container text-outline">
                          {m.role}
                        </span>
                        {isOwner && (
                          <button
                            onClick={() => handleRemoveMember(m.userId)}
                            className="opacity-0 group-hover:opacity-100 p-1 hover:bg-danger-red/10 rounded text-outline hover:text-danger-red transition-all"
                            title="Remove member"
                          >
                            <span className="material-symbols-outlined text-[14px]">close</span>
                          </button>
                        )}
                      </div>
                    ))}
                  </div>
                </div>

                {/* Brands */}
                <div>
                  <h4 className="text-label-sm font-bold text-on-surface flex items-center gap-2 mb-3">
                    <span className="material-symbols-outlined text-[16px] text-primary">branding_watermark</span>
                    Assigned Brands ({team.brands.length})
                  </h4>
                  {team.brands.length === 0 ? (
                    <p className="text-body-sm text-outline text-center py-4">
                      No brands assigned. Use the Brand page to assign this team.
                    </p>
                  ) : (
                    <div className="grid grid-cols-2 gap-2">
                      {team.brands.map((b) => (
                        <div
                          key={b.brandId}
                          className="flex items-center gap-2 p-3 bg-surface-container-low rounded-xl"
                        >
                          <div className="w-7 h-7 rounded-lg bg-primary/10 flex items-center justify-center text-label-2xs font-bold text-primary">
                            {b.brandName.charAt(0).toUpperCase()}
                          </div>
                          <span className="text-body-sm font-semibold text-on-surface truncate">{b.brandName}</span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                {/* Delete Team */}
                {isOwner && (
                  <div className="pt-3 border-t border-outline-variant/20">
                    {confirmDelete ? (
                      <div className="p-4 bg-danger-red/5 border border-danger-red/20 rounded-xl">
                        <p className="text-body-sm text-danger-red mb-3">
                          Are you sure you want to delete <strong>{team.name}</strong>? This will revoke all brand
                          assignments and remove all members from this team.
                        </p>
                        <div className="flex gap-2">
                          <button
                            onClick={handleDelete}
                            disabled={saving}
                            className="px-4 py-2 bg-danger-red text-white rounded-lg text-label-sm font-bold disabled:opacity-50"
                          >
                            {saving ? "Deleting..." : "Delete Team"}
                          </button>
                          <button
                            onClick={() => setConfirmDelete(false)}
                            className="px-4 py-2 border border-outline-variant/20 rounded-lg text-label-sm text-outline"
                          >
                            Cancel
                          </button>
                        </div>
                      </div>
                    ) : (
                      <button
                        onClick={() => setConfirmDelete(true)}
                        className="text-label-sm text-danger-red hover:underline flex items-center gap-1"
                      >
                        <span className="material-symbols-outlined text-[14px]">delete</span>
                        Delete this team
                      </button>
                    )}
                  </div>
                )}
              </>
            )}
          </div>

          {/* Add Member Overlay */}
          {showAddMember && (
            <div className="absolute inset-0 bg-surface-container-lowest/95 z-10 flex flex-col rounded-2xl">
              <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between">
                <h3 className="text-body-lg font-bold text-on-surface">Add Member to Team</h3>
                <button onClick={() => setShowAddMember(false)} className="p-2 hover:bg-surface-container rounded-full">
                  <span className="material-symbols-outlined text-[18px]">close</span>
                </button>
              </div>
              <div className="flex-1 overflow-y-auto p-6 space-y-2">
                {workspaceMembers
                  .filter((wm) => !team?.members.some((tm) => tm.userId === wm.id))
                  .map((wm) => (
                    <button
                      key={wm.id}
                      onClick={() => handleAddMember(wm.id, wm.role)}
                      disabled={addingMember}
                      className="w-full flex items-center gap-3 p-3 bg-surface-container-low rounded-xl hover:bg-primary/5 transition-all text-left disabled:opacity-50"
                    >
                      <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center text-label-2xs font-bold text-primary">
                        {wm.name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2)}
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-body-sm font-semibold text-on-surface truncate">{wm.name}</p>
                        <p className="text-label-xs text-outline truncate">{wm.email}</p>
                      </div>
                      <span className="px-2 py-0.5 rounded-full text-label-2xs font-bold bg-surface-container text-outline">
                        {wm.role === "ContentCreator" ? "Creator" : wm.role}
                      </span>
                    </button>
                  ))}
                {workspaceMembers.filter((wm) => !team?.members.some((tm) => tm.userId === wm.id)).length ===
                  0 && (
                  <p className="text-body-sm text-outline text-center py-8">
                    All workspace members are already in this team.
                  </p>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
}
