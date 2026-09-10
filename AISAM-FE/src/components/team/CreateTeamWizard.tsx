"use client";

import { useState, useEffect, useCallback } from "react";
import {
  createTeam,
  fetchMembers,
  type TeamMember,
  type TeamDetail,
} from "@/services/teamService";
import { fetchBrands } from "@/services/brandService";
import {
  readAssignments,
  changeAssignment,
  type AssignmentSnapshot,
} from "@/services/permissionService";
import { apiClient } from "@/lib/apiClient";

interface CreateTeamWizardProps {
  open: boolean;
  onClose: () => void;
  onCreated: (team: TeamDetail) => void;
}

interface SocialIntegration {
  id: string;
  platform: string;
  accountName: string;
  brandId: string;
}

type Step = "info" | "members" | "brands" | "channels" | "review";
const STEPS: { key: Step; label: string; icon: string }[] = [
  { key: "info", label: "Team Info", icon: "edit" },
  { key: "members", label: "Members", icon: "group" },
  { key: "brands", label: "Brands", icon: "branding_watermark" },
  { key: "channels", label: "Channels", icon: "share" },
  { key: "review", label: "Review", icon: "checklist" },
];

export default function CreateTeamWizard({ open, onClose, onCreated }: CreateTeamWizardProps) {
  const [step, setStep] = useState<Step>("info");
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Step 1: Team Info
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");

  // Step 2: Members
  const [workspaceMembers, setWorkspaceMembers] = useState<TeamMember[]>([]);
  const [selectedMembers, setSelectedMembers] = useState<{ userId: string; role: string }[]>([]);

  // Step 3: Brands
  const [brands, setBrands] = useState<{ id: string; name: string }[]>([]);
  const [selectedBrandIds, setSelectedBrandIds] = useState<string[]>([]);

  // Step 4: Channels
  const [integrations, setIntegrations] = useState<SocialIntegration[]>([]);
  const [channelPermissions, setChannelPermissions] = useState<
    Record<string, { canView: boolean; canPublish: boolean; canManage: boolean }>
  >({});

  // Load data
  useEffect(() => {
    if (!open) return;
    setStep("info");
    setName("");
    setDescription("");
    setSelectedMembers([]);
    setSelectedBrandIds([]);
    setChannelPermissions({});
    setError(null);

    const load = async () => {
      setLoading(true);
      try {
        const [membersRes, brandsRes] = await Promise.all([fetchMembers(), fetchBrands()]);
        setWorkspaceMembers(membersRes.data.filter((m) => m.status === "Active"));
        setBrands(brandsRes);
      } catch {
        setError("Failed to load workspace data");
      }
      setLoading(false);
    };
    load();
  }, [open]);

  // Load integrations when brands change
  useEffect(() => {
    if (selectedBrandIds.length === 0) {
      setIntegrations([]);
      return;
    }
    const load = async () => {
      try {
        const res = await apiClient("/social/integrations?pageSize=200");
        if (res?.data) {
          const items = (Array.isArray(res.data) ? res.data : res.data.data || []) as SocialIntegration[];
          setIntegrations(items.filter((i) => selectedBrandIds.includes(i.brandId)));
        }
      } catch {
        // ignore
      }
    };
    load();
  }, [selectedBrandIds]);

  const toggleMember = useCallback((userId: string, role: string) => {
    setSelectedMembers((prev) => {
      if (prev.some((m) => m.userId === userId)) {
        return prev.filter((m) => m.userId !== userId);
      }
      return [...prev, { userId, role }];
    });
  }, []);

  const toggleBrand = useCallback((id: string) => {
    setSelectedBrandIds((prev) =>
      prev.includes(id) ? prev.filter((b) => b !== id) : [...prev, id]
    );
  }, []);

  const setChannelPerm = useCallback(
    (integrationId: string, field: "canView" | "canPublish" | "canManage", value: boolean) => {
      setChannelPermissions((prev) => ({
        ...prev,
        [integrationId]: {
          canView: prev[integrationId]?.canView ?? false,
          canPublish: prev[integrationId]?.canPublish ?? false,
          canManage: prev[integrationId]?.canManage ?? false,
          [field]: value,
          // enforce: publish/manage requires view
          ...(field === "canView" && !value ? { canPublish: false, canManage: false } : {}),
        },
      }));
    },
    []
  );

  const currentStepIndex = STEPS.findIndex((s) => s.key === step);

  const canNext = () => {
    switch (step) {
      case "info":
        return name.trim().length > 0;
      case "members":
        return true; // optional
      case "brands":
        return true; // optional
      case "channels":
        return true; // optional
      case "review":
        return true;
      default:
        return false;
    }
  };

  const goNext = () => {
    const idx = currentStepIndex;
    if (idx < STEPS.length - 1) setStep(STEPS[idx + 1].key);
  };

  const goPrev = () => {
    const idx = currentStepIndex;
    if (idx > 0) setStep(STEPS[idx - 1].key);
  };

  const handleSave = async () => {
    setSaving(true);
    setError(null);
    try {
      // Step 1: Create team with members
      const team = await createTeam({
        name: name.trim(),
        description: description.trim() || undefined,
        members: selectedMembers,
      });

      // Step 2: Assign brands via existing assignment API
      for (const brandId of selectedBrandIds) {
        try {
          const snapshot = await readAssignments(brandId);
          await changeAssignment(brandId, team.id, snapshot.revision, true);
        } catch (err) {
          console.error(`Failed to assign brand ${brandId}:`, err);
        }
      }

      // Step 3: Set channel permissions
      for (const [integrationId, perms] of Object.entries(channelPermissions)) {
        if (!perms.canView && !perms.canPublish && !perms.canManage) continue;
        const integration = integrations.find((i) => i.id === integrationId);
        if (!integration) continue;
        try {
          const snapshot = await readAssignments(integration.brandId);
          await changeAssignment(integration.brandId, team.id, snapshot.revision, true, {
            id: integrationId,
            canView: perms.canView,
            canPublish: perms.canPublish,
            canManage: perms.canManage,
          });
        } catch (err) {
          console.error(`Failed to set channel ${integrationId}:`, err);
        }
      }

      onCreated(team);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to create team");
    }
    setSaving(false);
  };

  if (!open) return null;

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div
        className="fixed inset-0 z-50 flex items-center justify-center p-4"
        onClick={onClose}
      >
        <div
          className="w-full max-w-2xl bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[90vh] flex flex-col"
          onClick={(e) => e.stopPropagation()}
        >
          {/* Header */}
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between shrink-0">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">group_add</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Create New Team</h2>
                <p className="text-label-xs text-outline">
                  Step {currentStepIndex + 1} of {STEPS.length}: {STEPS[currentStepIndex].label}
                </p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          {/* Step indicator */}
          <div className="px-6 pt-4 flex gap-1 shrink-0">
            {STEPS.map((s, i) => (
              <button
                key={s.key}
                onClick={() => i <= currentStepIndex && setStep(s.key)}
                className={`flex-1 h-1.5 rounded-full transition-all ${
                  i <= currentStepIndex ? "bg-primary" : "bg-outline-variant/20"
                } ${i < currentStepIndex ? "cursor-pointer" : "cursor-default"}`}
              />
            ))}
          </div>

          {/* Content */}
          <div className="flex-1 overflow-y-auto p-6 space-y-4">
            {error && (
              <div className="p-3 bg-danger-red/10 border border-danger-red/20 rounded-xl text-body-sm text-danger-red flex items-center gap-2">
                <span className="material-symbols-outlined text-[16px]">error</span>
                {error}
              </div>
            )}

            {loading ? (
              <div className="flex items-center justify-center py-12">
                <div className="w-8 h-8 border-3 border-primary/20 border-t-primary rounded-full animate-spin" />
              </div>
            ) : (
              <>
                {/* Step 1: Info */}
                {step === "info" && (
                  <div className="space-y-4">
                    <div>
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">
                        Team Name *
                      </label>
                      <input
                        type="text"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        placeholder="e.g., Content Team Alpha"
                        className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 placeholder:text-outline/40 transition-all"
                        maxLength={255}
                        autoFocus
                      />
                    </div>
                    <div>
                      <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">
                        Description
                      </label>
                      <textarea
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        rows={3}
                        placeholder="Describe the team's focus and goals..."
                        className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 placeholder:text-outline/40 resize-none transition-all"
                        maxLength={1000}
                      />
                    </div>
                  </div>
                )}

                {/* Step 2: Members */}
                {step === "members" && (
                  <div className="space-y-3">
                    <p className="text-body-sm text-on-surface-variant">
                      Select workspace members to add to this team. You (the creator) are added automatically.
                    </p>
                    {workspaceMembers.length === 0 ? (
                      <p className="text-body-sm text-outline py-8 text-center">No workspace members available.</p>
                    ) : (
                      <div className="grid gap-2">
                        {workspaceMembers.map((m) => {
                          const selected = selectedMembers.some((s) => s.userId === m.id);
                          return (
                            <button
                              key={m.id}
                              type="button"
                              onClick={() => toggleMember(m.id, m.role)}
                              className={`flex items-center gap-3 p-3 rounded-xl border-2 transition-all text-left ${
                                selected
                                  ? "border-primary bg-primary/5"
                                  : "border-outline-variant/20 hover:border-outline-variant/40"
                              }`}
                            >
                              <div
                                className={`w-9 h-9 rounded-full flex items-center justify-center text-label-2xs font-bold ${
                                  selected
                                    ? "bg-primary text-on-primary"
                                    : "bg-surface-container-high text-outline"
                                }`}
                              >
                                {m.name
                                  .split(" ")
                                  .map((w) => w[0])
                                  .join("")
                                  .toUpperCase()
                                  .slice(0, 2)}
                              </div>
                              <div className="flex-1 min-w-0">
                                <p className="text-body-sm font-semibold text-on-surface truncate">{m.name}</p>
                                <p className="text-label-xs text-outline truncate">{m.email}</p>
                              </div>
                              <span
                                className={`px-2 py-0.5 rounded-full text-label-2xs font-bold ${
                                  m.role === "Owner"
                                    ? "bg-primary-fixed text-primary"
                                    : m.role === "Manager"
                                    ? "bg-secondary-fixed text-secondary"
                                    : m.role === "ContentCreator"
                                    ? "bg-tertiary-fixed text-tertiary"
                                    : "bg-surface-container text-outline"
                                }`}
                              >
                                {m.role === "ContentCreator" ? "Creator" : m.role}
                              </span>
                              {selected && (
                                <span className="material-symbols-outlined text-primary text-[20px]">
                                  check_circle
                                </span>
                              )}
                            </button>
                          );
                        })}
                      </div>
                    )}
                  </div>
                )}

                {/* Step 3: Brands */}
                {step === "brands" && (
                  <div className="space-y-3">
                    <p className="text-body-sm text-on-surface-variant">
                      Select brands this team will manage. Brand assignment determines data scope.
                    </p>
                    {brands.length === 0 ? (
                      <p className="text-body-sm text-outline py-8 text-center">No brands in this workspace.</p>
                    ) : (
                      <div className="grid grid-cols-2 gap-2">
                        {brands.map((brand) => {
                          const selected = selectedBrandIds.includes(brand.id);
                          return (
                            <button
                              key={brand.id}
                              type="button"
                              onClick={() => toggleBrand(brand.id)}
                              className={`flex items-center gap-2 p-3 rounded-xl border-2 transition-all text-left ${
                                selected
                                  ? "border-primary bg-primary/5"
                                  : "border-outline-variant/20 hover:border-outline-variant/40"
                              }`}
                            >
                              <div
                                className={`w-8 h-8 rounded-lg flex items-center justify-center text-label-2xs font-bold ${
                                  selected ? "bg-primary text-on-primary" : "bg-surface-container-high text-outline"
                                }`}
                              >
                                {brand.name.charAt(0).toUpperCase()}
                              </div>
                              <span className="text-label-sm font-semibold text-on-surface flex-1 truncate">
                                {brand.name}
                              </span>
                              {selected && (
                                <span className="material-symbols-outlined text-primary text-[18px]">check</span>
                              )}
                            </button>
                          );
                        })}
                      </div>
                    )}
                  </div>
                )}

                {/* Step 4: Channels */}
                {step === "channels" && (
                  <div className="space-y-3">
                    <p className="text-body-sm text-on-surface-variant">
                      Configure channel permissions for each brand&apos;s social accounts.
                    </p>
                    {selectedBrandIds.length === 0 ? (
                      <p className="text-body-sm text-outline py-8 text-center">
                        No brands selected. Go back to assign brands first.
                      </p>
                    ) : integrations.length === 0 ? (
                      <p className="text-body-sm text-outline py-8 text-center">
                        No social accounts connected to the selected brands.
                      </p>
                    ) : (
                      <div className="space-y-3">
                        {selectedBrandIds.map((brandId) => {
                          const brand = brands.find((b) => b.id === brandId);
                          const brandIntegrations = integrations.filter((i) => i.brandId === brandId);
                          if (brandIntegrations.length === 0) return null;
                          return (
                            <div key={brandId} className="border border-outline-variant/20 rounded-xl p-4">
                              <h4 className="text-label-sm font-bold text-on-surface mb-3">
                                {brand?.name || "Brand"}
                              </h4>
                              <div className="space-y-2">
                                {brandIntegrations.map((integration) => {
                                  const perms = channelPermissions[integration.id] || {
                                    canView: false,
                                    canPublish: false,
                                    canManage: false,
                                  };
                                  return (
                                    <div
                                      key={integration.id}
                                      className="flex items-center gap-3 p-2 bg-surface-container-low rounded-lg"
                                    >
                                      <div className="flex items-center gap-2 flex-1 min-w-0">
                                        <span className="material-symbols-outlined text-[16px] text-outline">
                                          {integration.platform === "facebook"
                                            ? "facebook"
                                            : integration.platform === "instagram"
                                            ? "photo_camera"
                                            : integration.platform === "tiktok"
                                            ? "music_note"
                                            : "share"}
                                        </span>
                                        <span className="text-body-sm text-on-surface truncate">
                                          {integration.accountName || integration.platform}
                                        </span>
                                      </div>
                                      <div className="flex items-center gap-3">
                                        <label className="flex items-center gap-1 cursor-pointer">
                                          <input
                                            type="checkbox"
                                            checked={perms.canView}
                                            onChange={(e) =>
                                              setChannelPerm(integration.id, "canView", e.target.checked)
                                            }
                                            className="w-4 h-4 rounded accent-primary"
                                          />
                                          <span className="text-label-2xs text-outline">View</span>
                                        </label>
                                        <label className="flex items-center gap-1 cursor-pointer">
                                          <input
                                            type="checkbox"
                                            checked={perms.canPublish}
                                            disabled={!perms.canView}
                                            onChange={(e) =>
                                              setChannelPerm(integration.id, "canPublish", e.target.checked)
                                            }
                                            className="w-4 h-4 rounded accent-primary disabled:opacity-40"
                                          />
                                          <span className="text-label-2xs text-outline">Publish</span>
                                        </label>
                                        <label className="flex items-center gap-1 cursor-pointer">
                                          <input
                                            type="checkbox"
                                            checked={perms.canManage}
                                            disabled={!perms.canView}
                                            onChange={(e) =>
                                              setChannelPerm(integration.id, "canManage", e.target.checked)
                                            }
                                            className="w-4 h-4 rounded accent-primary disabled:opacity-40"
                                          />
                                          <span className="text-label-2xs text-outline">Manage</span>
                                        </label>
                                      </div>
                                    </div>
                                  );
                                })}
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                )}

                {/* Step 5: Review */}
                {step === "review" && (
                  <div className="space-y-4">
                    <div className="bg-surface-container-low rounded-xl p-4">
                      <h4 className="text-label-sm font-bold text-on-surface mb-2 flex items-center gap-2">
                        <span className="material-symbols-outlined text-[16px] text-primary">info</span>
                        Team Info
                      </h4>
                      <p className="text-body-sm text-on-surface">
                        <strong>{name}</strong>
                        {description && ` — ${description}`}
                      </p>
                    </div>

                    <div className="bg-surface-container-low rounded-xl p-4">
                      <h4 className="text-label-sm font-bold text-on-surface mb-2 flex items-center gap-2">
                        <span className="material-symbols-outlined text-[16px] text-primary">group</span>
                        Members ({selectedMembers.length + 1})
                      </h4>
                      <p className="text-body-sm text-on-surface-variant">
                        You (creator) + {selectedMembers.length} additional member
                        {selectedMembers.length !== 1 ? "s" : ""}
                      </p>
                      {selectedMembers.length > 0 && (
                        <div className="mt-2 flex flex-wrap gap-1">
                          {selectedMembers.map((m) => {
                            const member = workspaceMembers.find((wm) => wm.id === m.userId);
                            return (
                              <span
                                key={m.userId}
                                className="px-2 py-0.5 bg-surface-container rounded-full text-label-2xs text-on-surface"
                              >
                                {member?.name || m.userId}
                              </span>
                            );
                          })}
                        </div>
                      )}
                    </div>

                    <div className="bg-surface-container-low rounded-xl p-4">
                      <h4 className="text-label-sm font-bold text-on-surface mb-2 flex items-center gap-2">
                        <span className="material-symbols-outlined text-[16px] text-primary">
                          branding_watermark
                        </span>
                        Brands ({selectedBrandIds.length})
                      </h4>
                      {selectedBrandIds.length === 0 ? (
                        <p className="text-body-sm text-outline">No brands assigned. You can assign later.</p>
                      ) : (
                        <div className="flex flex-wrap gap-1">
                          {selectedBrandIds.map((id) => {
                            const brand = brands.find((b) => b.id === id);
                            return (
                              <span
                                key={id}
                                className="px-2 py-0.5 bg-primary/10 text-primary rounded-full text-label-2xs font-semibold"
                              >
                                {brand?.name || id}
                              </span>
                            );
                          })}
                        </div>
                      )}
                    </div>

                    {Object.entries(channelPermissions).filter(
                      ([, p]) => p.canView || p.canPublish || p.canManage
                    ).length > 0 && (
                      <div className="bg-surface-container-low rounded-xl p-4">
                        <h4 className="text-label-sm font-bold text-on-surface mb-2 flex items-center gap-2">
                          <span className="material-symbols-outlined text-[16px] text-primary">share</span>
                          Channel Permissions
                        </h4>
                        <div className="space-y-1">
                          {Object.entries(channelPermissions)
                            .filter(([, p]) => p.canView || p.canPublish || p.canManage)
                            .map(([id, perms]) => {
                              const integration = integrations.find((i) => i.id === id);
                              return (
                                <div key={id} className="flex items-center gap-2 text-body-sm">
                                  <span className="text-on-surface">{integration?.accountName || id}</span>
                                  <span className="text-outline">—</span>
                                  {perms.canView && (
                                    <span className="px-1.5 py-0.5 bg-emerald-100 text-emerald-700 rounded text-label-2xs">
                                      View
                                    </span>
                                  )}
                                  {perms.canPublish && (
                                    <span className="px-1.5 py-0.5 bg-blue-100 text-blue-700 rounded text-label-2xs">
                                      Publish
                                    </span>
                                  )}
                                  {perms.canManage && (
                                    <span className="px-1.5 py-0.5 bg-purple-100 text-purple-700 rounded text-label-2xs">
                                      Manage
                                    </span>
                                  )}
                                </div>
                              );
                            })}
                        </div>
                      </div>
                    )}
                  </div>
                )}
              </>
            )}
          </div>

          {/* Footer */}
          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-between shrink-0">
            <button
              onClick={currentStepIndex === 0 ? onClose : goPrev}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all"
            >
              {currentStepIndex === 0 ? "Cancel" : "Back"}
            </button>
            <div className="flex items-center gap-3">
              {step === "review" ? (
                <button
                  onClick={handleSave}
                  disabled={saving || !name.trim()}
                  className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 disabled:opacity-50 disabled:hover:scale-100 flex items-center gap-2"
                >
                  {saving ? (
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  ) : (
                    <span className="material-symbols-outlined text-[16px]">check</span>
                  )}
                  Create Team
                </button>
              ) : (
                <button
                  onClick={goNext}
                  disabled={!canNext()}
                  className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 disabled:opacity-50 disabled:hover:scale-100 flex items-center gap-2"
                >
                  Next
                  <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
