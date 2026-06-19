"use client";

import { useEffect, useMemo, useState } from "react";
import Header from "@/components/layout/Header";
import { useAdCampaign } from "@/hooks/useAdCampaign";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { fetchBrands } from "@/services/brandService";
import {
  type AdCampaignDto,
  type AdCampaignObjective,
  type AdCampaignStatus,
  type CreateAdCampaignRequest,
  type UpdateAdCampaignRequest,
} from "@/services/adCampaignService";
import CampaignBulkActionBar from "@/components/campaigns/CampaignBulkActionBar";
import CampaignDeleteDialog from "@/components/campaigns/CampaignDeleteDialog";
import CampaignDetailDialog from "@/components/campaigns/CampaignDetailDialog";
import CampaignEditDialog from "@/components/campaigns/CampaignEditDialog";
import CampaignFilters, { type CampaignSortOption } from "@/components/campaigns/CampaignFilters";
import CampaignPagination from "@/components/campaigns/CampaignPagination";
import {
  CAMPAIGN_OBJECTIVES,
  formatDate,
  formatMoney,
  objectiveLabels,
  statusClass,
} from "@/components/campaigns/campaignDisplay";

type BrandOption = { id: string; name: string };

const emptyForm: CreateAdCampaignRequest = {
  brandId: "",
  adAccountId: "",
  name: "",
  objective: "TRAFFIC",
  budget: null,
  startDate: "",
  endDate: "",
};

export default function CampaignsPage() {
  const { activeWorkspace } = useWorkspaces();
  const {
    campaigns,
    isLoading,
    error,
    refresh,
    createCampaign,
    updateCampaign,
    deleteCampaign,
    syncCampaign,
    query,
    setQuery,
    totalCount,
    page,
    pageSize,
    totalPages,
  } = useAdCampaign({ page: 1, pageSize: 50, sortBy: "createdAt", sortDescending: true });

  const [brands, setBrands] = useState<BrandOption[]>([]);
  const [form, setForm] = useState<CreateAdCampaignRequest>(emptyForm);
  const [formOpen, setFormOpen] = useState(false);
  const [actionId, setActionId] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [editError, setEditError] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<AdCampaignStatus | "">("");
  const [objectiveFilter, setObjectiveFilter] = useState<AdCampaignObjective | "">("");
  const [sortOption, setSortOption] = useState<CampaignSortOption>("newest");
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [detailCampaign, setDetailCampaign] = useState<AdCampaignDto | null>(null);
  const [editCampaign, setEditCampaign] = useState<AdCampaignDto | null>(null);
  const [deleteTargets, setDeleteTargets] = useState<AdCampaignDto[]>([]);

  useEffect(() => {
    let cancelled = false;
    fetchBrands().then((items) => {
      if (!cancelled) setBrands(items);
    });
    return () => {
      cancelled = true;
    };
  }, [activeWorkspace?.id]);

  useEffect(() => {
    setSelectedIds((prev) => prev.filter((id) => campaigns.some((campaign) => campaign.id === id)));
  }, [campaigns]);

  const stats = useMemo(() => {
    const active = campaigns.filter((item) => item.status === "ACTIVE").length;
    const draft = campaigns.filter((item) => item.status === "DRAFT").length;
    const synced = campaigns.filter((item) => !!item.facebookCampaignId).length;
    return { active, draft, synced };
  }, [campaigns]);

  const visibleCampaigns = useMemo(() => {
    return campaigns.filter((campaign) => {
      if (statusFilter && campaign.status !== statusFilter) return false;
      if (objectiveFilter && campaign.objective !== objectiveFilter) return false;
      return true;
    });
  }, [campaigns, objectiveFilter, statusFilter]);

  const selectedCampaigns = useMemo(
    () => campaigns.filter((campaign) => selectedIds.includes(campaign.id)),
    [campaigns, selectedIds]
  );

  const allVisibleSelected = visibleCampaigns.length > 0 && visibleCampaigns.every((campaign) => selectedIds.includes(campaign.id));

  const updateForm = (partial: Partial<CreateAdCampaignRequest>) => {
    setForm((prev) => ({ ...prev, ...partial }));
  };

  const validateForm = () => {
    if (!form.brandId) return "Brand is required.";
    if (!form.adAccountId.trim()) return "Facebook ad account is required.";
    if (!form.name.trim()) return "Campaign name is required.";
    if (form.budget !== null && form.budget !== undefined && Number(form.budget) <= 0) return "Budget must be positive.";
    if (form.startDate && form.endDate && form.endDate <= form.startDate) return "End date must be after start date.";
    return null;
  };

  const handleCreate = async () => {
    const validation = validateForm();
    if (validation) {
      setFormError(validation);
      return;
    }

    setActionId("create");
    setFormError(null);
    try {
      const created = await createCampaign({
        ...form,
        name: form.name.trim(),
        adAccountId: form.adAccountId.trim(),
        budget: form.budget === null || form.budget === undefined ? null : Number(form.budget),
        startDate: form.startDate || null,
        endDate: form.endDate || null,
      });
      if (created) {
        setForm(emptyForm);
        setFormOpen(false);
      }
    } catch (err) {
      setFormError(err instanceof Error ? err.message : "Failed to create campaign.");
    } finally {
      setActionId(null);
    }
  };

  const handleSearchChange = (value: string) => {
    setQuery((prev) => ({ ...prev, page: 1, searchTerm: value || undefined }));
  };

  const handleSortChange = (value: CampaignSortOption) => {
    setSortOption(value);
    const next = (() => {
      switch (value) {
        case "oldest":
          return { sortBy: "createdAt", sortDescending: false };
        case "updated":
          return { sortBy: "updatedAt", sortDescending: true };
        case "name_asc":
          return { sortBy: "name", sortDescending: false };
        case "name_desc":
          return { sortBy: "name", sortDescending: true };
        default:
          return { sortBy: "createdAt", sortDescending: true };
      }
    })();
    setQuery((prev) => ({ ...prev, page: 1, ...next }));
  };

  const clearFilters = () => {
    setStatusFilter("");
    setObjectiveFilter("");
    setQuery((prev) => ({ ...prev, page: 1, searchTerm: undefined }));
  };

  const toggleSelected = (id: string, selected: boolean) => {
    setSelectedIds((prev) => selected ? [...new Set([...prev, id])] : prev.filter((item) => item !== id));
  };

  const toggleAllVisible = (selected: boolean) => {
    if (!selected) {
      setSelectedIds((prev) => prev.filter((id) => !visibleCampaigns.some((campaign) => campaign.id === id)));
      return;
    }
    setSelectedIds((prev) => [...new Set([...prev, ...visibleCampaigns.map((campaign) => campaign.id)])]);
  };

  const handleUpdate = async (id: string, payload: UpdateAdCampaignRequest) => {
    setActionId(id);
    setEditError(null);
    try {
      const updated = await updateCampaign(id, payload);
      if (updated) {
        setEditCampaign(null);
      }
    } catch (err) {
      setEditError(err instanceof Error ? err.message : "Failed to update campaign.");
    } finally {
      setActionId(null);
    }
  };

  const handleStatusChange = async (campaign: AdCampaignDto, status: AdCampaignStatus) => {
    setActionId(campaign.id);
    try {
      await updateCampaign(campaign.id, { status });
    } finally {
      setActionId(null);
    }
  };

  const handleApply = async (campaign: AdCampaignDto) => {
    setActionId(campaign.id);
    try {
      await syncCampaign(campaign.id);
      setDetailCampaign(null);
    } finally {
      setActionId(null);
    }
  };

  const handleRestart = async (campaign: AdCampaignDto) => {
    const today = new Date();
    const end = new Date(today);
    end.setDate(end.getDate() + 30);

    setActionId(campaign.id);
    try {
      await updateCampaign(campaign.id, {
        status: "ACTIVE",
        startDate: today.toISOString().slice(0, 10),
        endDate: end.toISOString().slice(0, 10),
      });
      setDetailCampaign(null);
    } finally {
      setActionId(null);
    }
  };

  const handleSync = async (id: string) => {
    setActionId(id);
    try {
      await syncCampaign(id);
    } finally {
      setActionId(null);
    }
  };

  const handleConfirmDelete = async () => {
    if (deleteTargets.length === 0) return;
    setActionId("delete");
    try {
      for (const campaign of deleteTargets) {
        await deleteCampaign(campaign.id);
      }
      setSelectedIds((prev) => prev.filter((id) => !deleteTargets.some((campaign) => campaign.id === id)));
      setDeleteTargets([]);
    } finally {
      setActionId(null);
    }
  };

  if (!activeWorkspace?.id) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Campaigns" }]} />
        <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
          <div className="max-w-5xl mx-auto border border-outline-variant/20 bg-surface-container-lowest rounded-2xl p-8">
            <h1 className="text-headline-sm font-bold text-on-surface">Campaigns</h1>
            <p className="text-body-sm text-on-surface-variant mt-2">Select a workspace before managing ad campaigns.</p>
          </div>
        </main>
      </>
    );
  }

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Campaigns" }]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-7xl mx-auto space-y-6">
          <div className="flex flex-col lg:flex-row lg:items-end justify-between gap-4">
            <div>
              <h1 className="text-headline-sm font-bold text-on-surface">Ad Campaigns</h1>
              <p className="text-body-sm text-on-surface-variant mt-1">Create local campaign records before Facebook Marketing API sync.</p>
            </div>
            <div className="flex items-center gap-3">
              <button
                onClick={() => void refresh()}
                className="h-10 px-4 rounded-xl border border-outline-variant/30 text-label-sm font-semibold text-on-surface-variant hover:bg-surface-container transition-colors flex items-center gap-2"
              >
                <span className="material-symbols-outlined text-[18px]">refresh</span>
                Refresh
              </button>
              <button
                onClick={() => setFormOpen((prev) => !prev)}
                className="h-10 px-4 rounded-xl bg-primary text-on-primary text-label-sm font-semibold hover:shadow-lg transition-all flex items-center gap-2"
              >
                <span className="material-symbols-outlined text-[18px]">add</span>
                New Campaign
              </button>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Metric label="Total" value={campaigns.length} icon="campaign" />
            <Metric label="Active" value={stats.active} icon="play_circle" />
            <Metric label="Draft" value={stats.draft} icon="edit_note" />
            <Metric label="Local Sync" value={stats.synced} icon="sync" />
          </div>

          <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-amber-900 flex items-start gap-3">
            <span className="material-symbols-outlined text-[20px] mt-0.5">info</span>
            <p className="text-body-sm">Campaign CRUD is active locally. Sync currently stores a local pending marker; it does not publish ads to Facebook yet.</p>
          </div>

          {formOpen && (
            <section className="rounded-2xl border border-outline-variant/20 bg-surface-container-lowest p-5">
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
                <Field label="Brand">
                  <select
                    value={form.brandId}
                    onChange={(event) => updateForm({ brandId: event.target.value })}
                    className="w-full h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary"
                  >
                    <option value="">Select brand</option>
                    {brands.map((brand) => <option key={brand.id} value={brand.id}>{brand.name}</option>)}
                  </select>
                </Field>
                <Field label="Ad account">
                  <input
                    value={form.adAccountId}
                    onChange={(event) => updateForm({ adAccountId: event.target.value })}
                    placeholder="act_123456789"
                    className="w-full h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary"
                  />
                </Field>
                <Field label="Objective">
                  <select
                    value={form.objective}
                    onChange={(event) => updateForm({ objective: event.target.value })}
                    className="w-full h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary"
                  >
                    {CAMPAIGN_OBJECTIVES.map((objective) => <option key={objective} value={objective}>{objectiveLabels[objective]}</option>)}
                  </select>
                </Field>
                <Field label="Name">
                  <input
                    value={form.name}
                    onChange={(event) => updateForm({ name: event.target.value })}
                    placeholder="Summer promotion"
                    className="w-full h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary"
                  />
                </Field>
                <Field label="Budget">
                  <input
                    type="number"
                    min="0"
                    value={form.budget ?? ""}
                    onChange={(event) => updateForm({ budget: event.target.value ? Number(event.target.value) : null })}
                    placeholder="1000000"
                    className="w-full h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary"
                  />
                </Field>
                <div className="grid grid-cols-2 gap-3">
                  <Field label="Start">
                    <input
                      type="date"
                      value={form.startDate ?? ""}
                      onChange={(event) => updateForm({ startDate: event.target.value })}
                      className="w-full h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary"
                    />
                  </Field>
                  <Field label="End">
                    <input
                      type="date"
                      value={form.endDate ?? ""}
                      onChange={(event) => updateForm({ endDate: event.target.value })}
                      className="w-full h-10 rounded-xl border border-outline-variant/30 bg-surface-container-lowest px-3 text-body-sm outline-none focus:border-primary"
                    />
                  </Field>
                </div>
              </div>
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mt-4">
                {formError ? <p className="text-label-sm text-danger-red font-semibold">{formError}</p> : <span />}
                <button
                  onClick={handleCreate}
                  disabled={actionId === "create"}
                  className="h-10 px-5 rounded-xl bg-primary text-on-primary text-label-sm font-semibold disabled:opacity-50 flex items-center justify-center gap-2"
                >
                  {actionId === "create" ? <span className="w-4 h-4 rounded-full border-2 border-white/40 border-t-white animate-spin" /> : <span className="material-symbols-outlined text-[18px]">check</span>}
                  Create Campaign
                </button>
              </div>
            </section>
          )}

          <CampaignBulkActionBar
            selectedCount={selectedIds.length}
            isLoading={actionId === "delete"}
            onClear={() => setSelectedIds([])}
            onDelete={() => setDeleteTargets(selectedCampaigns)}
          />

          <section className="rounded-2xl border border-outline-variant/20 bg-surface-container-lowest overflow-hidden">
            <CampaignFilters
              search={query.searchTerm ?? ""}
              status={statusFilter}
              objective={objectiveFilter}
              sort={sortOption}
              resultCount={visibleCampaigns.length}
              totalCount={totalCount}
              onSearchChange={handleSearchChange}
              onStatusChange={(value) => setStatusFilter(value)}
              onObjectiveChange={(value) => setObjectiveFilter(value)}
              onSortChange={handleSortChange}
              onClear={clearFilters}
            />

            {error && (
              <div className="m-4 rounded-xl border border-danger-red/20 bg-danger-red/10 px-4 py-3 text-danger-red text-body-sm">
                {error.message}
              </div>
            )}

            {isLoading ? (
              <div className="p-8 text-center text-body-sm text-outline">Loading campaigns...</div>
            ) : visibleCampaigns.length === 0 ? (
              <div className="p-10 text-center">
                <span className="material-symbols-outlined text-[40px] text-outline/40">campaign</span>
                <h3 className="text-title-md font-bold text-on-surface mt-3">{campaigns.length === 0 ? "No campaigns yet" : "No matching campaigns"}</h3>
                <p className="text-body-sm text-on-surface-variant mt-1">{campaigns.length === 0 ? "Create a local campaign record to start the ads workflow." : "Adjust filters or search to see more results."}</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full min-w-[1060px] text-left">
                  <thead className="bg-surface-container text-label-sm text-on-surface-variant">
                    <tr>
                      <th className="px-5 py-3 font-semibold w-12">
                        <input
                          type="checkbox"
                          checked={allVisibleSelected}
                          onChange={(event) => toggleAllVisible(event.target.checked)}
                          className="w-4 h-4 rounded border-outline-variant/30"
                          aria-label="Select all visible campaigns"
                        />
                      </th>
                      <th className="px-5 py-3 font-semibold">Campaign</th>
                      <th className="px-5 py-3 font-semibold">Brand</th>
                      <th className="px-5 py-3 font-semibold">Objective</th>
                      <th className="px-5 py-3 font-semibold">Budget</th>
                      <th className="px-5 py-3 font-semibold">Dates</th>
                      <th className="px-5 py-3 font-semibold">Status</th>
                      <th className="px-5 py-3 font-semibold text-right">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-outline-variant/10">
                    {visibleCampaigns.map((campaign) => (
                      <tr key={campaign.id} className="hover:bg-surface-container/50">
                        <td className="px-5 py-4">
                          <input
                            type="checkbox"
                            checked={selectedIds.includes(campaign.id)}
                            onChange={(event) => toggleSelected(campaign.id, event.target.checked)}
                            className="w-4 h-4 rounded border-outline-variant/30"
                            aria-label={`Select ${campaign.name}`}
                          />
                        </td>
                        <td className="px-5 py-4">
                          <p className="text-body-sm font-semibold text-on-surface">{campaign.name}</p>
                          <p className="text-label-xs text-outline">{campaign.facebookCampaignId || campaign.adAccountId}</p>
                        </td>
                        <td className="px-5 py-4 text-body-sm text-on-surface-variant">{campaign.brandName || "Unknown"}</td>
                        <td className="px-5 py-4 text-body-sm text-on-surface-variant">{objectiveLabels[campaign.objective] || campaign.objective}</td>
                        <td className="px-5 py-4 text-body-sm text-on-surface-variant">{formatMoney(campaign.budget)}</td>
                        <td className="px-5 py-4 text-body-sm text-on-surface-variant">
                          {formatDate(campaign.startDate)} {campaign.endDate ? `to ${formatDate(campaign.endDate)}` : ""}
                        </td>
                        <td className="px-5 py-4">
                          <span className={`inline-flex items-center px-2.5 py-1 rounded-full border text-label-xs font-semibold ${statusClass(campaign.status)}`}>
                            {campaign.status}
                          </span>
                        </td>
                        <td className="px-5 py-4">
                          <div className="flex items-center justify-end gap-1.5">
                            <button
                              onClick={() => setDetailCampaign(campaign)}
                              className="w-9 h-9 rounded-lg border border-outline-variant/30 text-on-surface-variant hover:bg-surface-container"
                              title="View details"
                            >
                              <span className="material-symbols-outlined text-[18px]">visibility</span>
                            </button>
                            {campaign.status !== "COMPLETED" && (
                              <button
                                onClick={() => {
                                  setEditError(null);
                                  setEditCampaign(campaign);
                                }}
                                className="w-9 h-9 rounded-lg border border-outline-variant/30 text-on-surface-variant hover:bg-surface-container"
                                title="Edit"
                              >
                                <span className="material-symbols-outlined text-[18px]">edit</span>
                              </button>
                            )}
                            {campaign.status === "DRAFT" && (
                              <button
                                onClick={() => void handleApply(campaign)}
                                disabled={actionId === campaign.id}
                                className="w-9 h-9 rounded-lg border border-emerald-200 text-emerald-700 hover:bg-emerald-50 disabled:opacity-50"
                                title="Apply campaign"
                              >
                                <span className="material-symbols-outlined text-[18px]">rocket_launch</span>
                              </button>
                            )}
                            {(campaign.status === "ACTIVE" || campaign.status === "PAUSED") && (
                              <button
                                onClick={() => void handleStatusChange(campaign, campaign.status === "ACTIVE" ? "PAUSED" : "ACTIVE")}
                                disabled={actionId === campaign.id}
                                className="w-9 h-9 rounded-lg border border-outline-variant/30 text-on-surface-variant hover:bg-surface-container disabled:opacity-50"
                                title={campaign.status === "ACTIVE" ? "Pause" : "Activate"}
                              >
                                <span className="material-symbols-outlined text-[18px]">{campaign.status === "ACTIVE" ? "pause" : "play_arrow"}</span>
                              </button>
                            )}
                            {campaign.status === "COMPLETED" && (
                              <button
                                onClick={() => void handleRestart(campaign)}
                                disabled={actionId === campaign.id}
                                className="w-9 h-9 rounded-lg border border-blue-200 text-blue-700 hover:bg-blue-50 disabled:opacity-50"
                                title="Restart"
                              >
                                <span className="material-symbols-outlined text-[18px]">replay</span>
                              </button>
                            )}
                            <button
                              onClick={() => void handleSync(campaign.id)}
                              disabled={actionId === campaign.id}
                              className="w-9 h-9 rounded-lg border border-outline-variant/30 text-on-surface-variant hover:bg-surface-container disabled:opacity-50"
                              title="Sync locally"
                            >
                              <span className="material-symbols-outlined text-[18px]">sync</span>
                            </button>
                            <button
                              onClick={() => setDeleteTargets([campaign])}
                              disabled={actionId === campaign.id}
                              className="w-9 h-9 rounded-lg border border-outline-variant/30 text-danger-red hover:bg-danger-red/10 disabled:opacity-50"
                              title="Delete"
                            >
                              <span className="material-symbols-outlined text-[18px]">delete</span>
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
            <CampaignPagination
              page={page}
              pageSize={pageSize}
              totalCount={totalCount}
              totalPages={totalPages}
              isLoading={isLoading}
              onPageChange={(nextPage) => setQuery((prev) => ({ ...prev, page: nextPage }))}
              onPageSizeChange={(nextPageSize) => setQuery((prev) => ({ ...prev, page: 1, pageSize: nextPageSize }))}
            />
          </section>
        </div>

        <CampaignDetailDialog
          campaign={detailCampaign}
          isLoading={actionId === detailCampaign?.id}
          onClose={() => setDetailCampaign(null)}
          onApply={(campaign) => void handleApply(campaign)}
          onRestart={(campaign) => void handleRestart(campaign)}
        />
        <CampaignEditDialog
          campaign={editCampaign}
          brands={brands}
          isLoading={actionId === editCampaign?.id}
          error={editError}
          onClose={() => setEditCampaign(null)}
          onSave={(id, payload) => void handleUpdate(id, payload)}
        />
        <CampaignDeleteDialog
          campaigns={deleteTargets}
          isLoading={actionId === "delete"}
          onCancel={() => setDeleteTargets([])}
          onConfirm={() => void handleConfirmDelete()}
        />
      </main>
    </>
  );
}

function Metric({ label, value, icon }: { label: string; value: number; icon: string }) {
  return (
    <div className="rounded-2xl border border-outline-variant/20 bg-surface-container-lowest p-4">
      <div className="flex items-center justify-between">
        <span className="text-label-sm text-on-surface-variant">{label}</span>
        <span className="material-symbols-outlined text-[20px] text-primary">{icon}</span>
      </div>
      <p className="text-headline-sm font-bold text-on-surface mt-2">{value}</p>
    </div>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block">
      <span className="text-label-sm font-semibold text-on-surface-variant mb-1.5 block">{label}</span>
      {children}
    </label>
  );
}
