"use client";

import { useState, useEffect, useMemo, useCallback } from "react";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { useToast } from "@/contexts/ToastContext";
import Header from "@/components/layout/Header";
import {
  fetchCampaigns,
  createCampaign,
  updateCampaign,
  updateCampaignStatus,
  restartCampaign,
  deleteCampaign,
  restoreCampaign,
  deployCampaignToFacebook,
  activateCampaign,
  cleanupCampaignDeployment,
  duplicateCampaign,
  type Campaign,
  type CampaignStatus,
  type DeploymentStatus,
  type CampaignObjective,
  type CreateCampaignData,
} from "@/services/campaignService";
import CampaignStatsCards from "@/components/campaigns/CampaignStatsCards";
import CampaignFilterBar, { type SortOption } from "@/components/campaigns/CampaignFilterBar";
import CampaignCard from "@/components/campaigns/CampaignCard";
import CampaignEmptyState from "@/components/campaigns/CampaignEmptyState";
import BulkActionsBar from "@/components/campaigns/BulkActionsBar";
import CreateCampaignModal from "@/components/campaigns/CreateCampaignModal";
import EditCampaignModal from "@/components/campaigns/EditCampaignModal";
import CampaignDetailModal from "@/components/campaigns/CampaignDetailModal";
import DeleteConfirmModal from "@/components/campaigns/DeleteConfirmModal";
import StartConfirmModal from "@/components/campaigns/StartConfirmModal";
import { fetchBrands } from "@/services/brandService";
import { setCachedBrands, OBJECTIVE_CONFIG, STATUS_CONFIG } from "@/components/campaigns/campaignUtils";

export default function CampaignsPage() {
  const { activeWorkspace } = useWorkspaces();
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);

  const PAGE_SIZE = 20;

  // Filters
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<CampaignStatus | "">("");
  const [objectiveFilter, setObjectiveFilter] = useState<CampaignObjective | "">("");
  const [sortBy, setSortBy] = useState<SortOption>("newest");

  // Selection
  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  // Modals
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editCampaign, setEditCampaign] = useState<Campaign | null>(null);
  const [detailCampaign, setDetailCampaign] = useState<Campaign | null>(null);
  const [deletingCampaigns, setDeletingCampaigns] = useState<Campaign[]>([]);
  const [startingCampaign, setStartingCampaign] = useState<Campaign | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const { addToast } = useToast();

  // Load campaigns
  const loadCampaigns = useCallback(async (pageNum: number, append = false) => {
    if (append) {
      setLoadingMore(true);
    } else {
      setLoading(true);
    }
    try {
      const res = await fetchCampaigns({ page: pageNum, pageSize: PAGE_SIZE });
      if (append) {
        setCampaigns((prev) => [...prev, ...res.data]);
      } else {
        setCampaigns(res.data);
      }
      setHasMore(res.data.length === PAGE_SIZE);
      setPage(pageNum);
    } catch (err) {
      console.error("Failed to load campaigns:", err);
      addToast("Failed to load campaigns", "error");
      if (!append) setCampaigns([]);
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  }, [addToast]);

  useEffect(() => {
    loadCampaigns(1);
  }, [activeWorkspace?.id, loadCampaigns]);

  const handleLoadMore = () => {
    loadCampaigns(page + 1, true);
  };

  useEffect(() => {
    fetchBrands().then((brands) => {
      setCachedBrands(brands.map(b => ({ id: b.id, name: b.name })));
    });
  }, [activeWorkspace?.id]);

  // Filter and sort campaigns
  const filteredCampaigns = useMemo(() => {
    let result = [...campaigns];

    // Search filter
    if (search) {
      const q = search.toLowerCase();
      result = result.filter((c) =>
        c.name.toLowerCase().includes(q) ||
        c.brandName.toLowerCase().includes(q)
      );
    }

    // Status filter
    if (statusFilter) {
      result = result.filter((c) => c.status === statusFilter);
    }

    // Objective filter
    if (objectiveFilter) {
      result = result.filter((c) => c.objective === objectiveFilter);
    }

    // Sort
    switch (sortBy) {
      case "newest":
        result.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        break;
      case "oldest":
        result.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
        break;
      case "budget_high":
        result.sort((a, b) => (b.budget || 0) - (a.budget || 0));
        break;
      case "budget_low":
        result.sort((a, b) => (a.budget || 0) - (b.budget || 0));
        break;
      case "spend_high":
        result.sort((a, b) => b.spend - a.spend);
        break;
      case "name":
        result.sort((a, b) => a.name.localeCompare(b.name));
        break;
    }

    return result;
  }, [campaigns, search, statusFilter, objectiveFilter, sortBy]);

  // Handlers
  const handleCreate = async (data: CreateCampaignData) => {
    setActionLoading("create");
    try {
      const campaign = await createCampaign(data);
      setCampaigns((prev) => [campaign, ...prev]);
      setShowCreateModal(false);
      addToast(`Campaign "${campaign.name}" created successfully`);
    } catch {
      addToast("Failed to create campaign", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleEdit = async (id: string, data: CreateCampaignData) => {
    setActionLoading("edit");
    try {
      const updated = await updateCampaign(id, data);
      if (updated) {
        setCampaigns((prev) => prev.map((c) => (c.id === id ? updated : c)));
        setEditCampaign(null);
        addToast(`Campaign "${updated.name}" updated successfully`);
      }
    } catch {
      addToast("Failed to update campaign", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleToggleStatus = async (campaign: Campaign) => {
    if (campaign.status === "PAUSED" && campaign.facebookCampaignId) {
      setStartingCampaign(campaign);
      return;
    }
    await doToggleStatus(campaign, "PAUSED");
  };

  const doToggleStatus = async (campaign: Campaign, newStatus: CampaignStatus) => {
    setActionLoading(campaign.id);
    try {
      const updated = await updateCampaignStatus(campaign.id, newStatus);
      if (updated) {
        setCampaigns((prev) => prev.map((c) => (c.id === campaign.id ? updated : c)));
        addToast(`Campaign ${newStatus === "ACTIVE" ? "activated" : "paused"}`);
      }
    } catch {
      addToast("Failed to update campaign status", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleRestart = async (campaign: Campaign) => {
    setActionLoading(campaign.id);
    try {
      const updated = await restartCampaign(campaign.id);
      if (updated) {
        setCampaigns((prev) => prev.map((c) => (c.id === campaign.id ? updated : c)));
        addToast(`Campaign "${updated.name}" restarted successfully`);
      }
    } catch {
      addToast("Failed to restart campaign", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleDelete = (campaign: Campaign) => {
    setDeletingCampaigns([campaign]);
  };

  const handleBulkDelete = () => {
    const selected = campaigns.filter((c) => selectedIds.includes(c.id));
    setDeletingCampaigns(selected);
  };

  const handleBulkDuplicate = async () => {
    const selected = campaigns.filter((c) => selectedIds.includes(c.id));
    if (selected.length === 0) return;
    setActionLoading("duplicate");
    try {
      const results = await Promise.all(
        selected.map((campaign) => duplicateCampaign(campaign.id))
      );
      const duplicated = results.filter(Boolean);
      setCampaigns((prev) => [...duplicated, ...prev]);
      setSelectedIds([]);
      addToast(`${duplicated.length} campaign(s) duplicated`);
    } catch (err: any) {
      addToast(err?.message || "Failed to duplicate", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleConfirmDelete = async () => {
    if (deletingCampaigns.length === 0) return;
    setActionLoading("delete");
    try {
      await Promise.all(
        deletingCampaigns.map((campaign) => deleteCampaign(campaign.id))
      );
      setCampaigns((prev) => prev.filter((c) => !deletingCampaigns.some((d) => d.id === c.id)));
      setSelectedIds((prev) => prev.filter((id) => !deletingCampaigns.some((d) => d.id === id)));
      setDeletingCampaigns([]);
      addToast(`${deletingCampaigns.length} campaign(s) deleted`);
    } catch {
      addToast("Failed to delete campaign(s)", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleSelect = (id: string, selected: boolean) => {
    setSelectedIds((prev) =>
      selected ? [...prev, id] : prev.filter((x) => x !== id)
    );
  };

  const handleConfirmStart = async (campaign: Campaign) => {
    setStartingCampaign(null);
    await doToggleStatus(campaign, "ACTIVE");
  };

  const handleClearSelection = () => {
    setSelectedIds([]);
  };

  const handleDeploy = async (campaign: Campaign) => {
    setActionLoading(campaign.id);
    try {
      const updated = await deployCampaignToFacebook(campaign.id);
      if (updated) {
        setCampaigns((prev) => prev.map((c) => (c.id === campaign.id ? updated : c)));
        addToast("Campaign sent to Meta. AISAM will mark it ready once review checks pass.");
      }
    } catch (err: any) {
      addToast(err?.message || "Failed to deploy", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleActivate = async (campaign: Campaign) => {
    setActionLoading(campaign.id);
    try {
      const updated = await activateCampaign(campaign.id);
      if (updated) {
        setCampaigns((prev) => prev.map((c) => (c.id === campaign.id ? updated : c)));
        addToast("Campaign activated successfully.");
      }
    } catch (err: any) {
      addToast(err?.message || "Failed to activate campaign", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleCleanup = async (campaign: Campaign) => {
    setActionLoading(campaign.id);
    try {
      await cleanupCampaignDeployment(campaign.id);
      setCampaigns((prev) => prev.map((c) => (c.id === campaign.id ? { ...c, deploymentStatus: 0 as DeploymentStatus, deploymentStep: 0, status: "DRAFT" as CampaignStatus, facebookCampaignId: null } : c)));
      addToast("Failed deployment cleaned up. Campaign reset to Draft.");
    } catch (err: any) {
      addToast(err?.message || "Failed to clean up deployment", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleDuplicate = async (campaign: Campaign) => {
    setActionLoading(campaign.id);
    try {
      const dup = await duplicateCampaign(campaign.id);
      if (dup) {
        setCampaigns((prev) => [dup, ...prev]);
        addToast("Campaign duplicated");
      }
    } catch (err: any) {
      addToast(err?.message || "Failed to duplicate", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const hasFilters = !!(search || statusFilter || objectiveFilter);

  return (
    <>
      <style>{`
        @keyframes fade-up { from { opacity: 0; transform: translateY(16px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes float { 0%,100% { transform: translateY(0px); } 50% { transform: translateY(-6px); } }
        .animate-fade-up { animation: fade-up 0.5s ease-out forwards; opacity: 0; }
        .animate-float { animation: float 4s ease-in-out infinite; }
        .card-hover { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
        .card-hover:hover { transform: translateY(-4px); box-shadow: 0 12px 40px -12px rgba(0,0,0,0.15); }
      `}</style>

      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Campaigns" }]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-7xl mx-auto space-y-6">

          {/* Header */}
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 animate-fade-up">
            <div className="flex items-center gap-4">
              <div className="relative w-12 h-12 shrink-0">
                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-primary to-primary/70 animate-float shadow-lg shadow-primary/20" />
                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-white/15 to-transparent" />
                <div className="relative w-full h-full flex items-center justify-center">
                  <span className="material-symbols-outlined text-on-primary text-[24px]">campaign</span>
                </div>
              </div>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Ad Campaigns</h1>
                <p className="text-[11px] text-outline">{campaigns.length} campaigns · Manage your advertising</p>
              </div>
            </div>
            <button
              onClick={() => setShowCreateModal(true)}
              className="bg-primary text-on-primary px-5 py-2.5 rounded-xl text-label-sm font-bold flex items-center gap-1.5 shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95"
            >
              <span className="material-symbols-outlined text-[16px]">add</span>
              Create Campaign
            </button>
          </div>

          <CampaignStatsCards campaigns={campaigns} />

          <CampaignFilterBar
            search={search}
            onSearchChange={setSearch}
            statusFilter={statusFilter}
            onStatusFilterChange={setStatusFilter}
            objectiveFilter={objectiveFilter}
            onObjectiveFilterChange={setObjectiveFilter}
            sortBy={sortBy}
            onSortChange={setSortBy}
            resultCount={filteredCampaigns.length}
            totalCount={campaigns.length}
          />

          {/* Bulk Actions */}
            <BulkActionsBar
              selectedCount={selectedIds.length}
              onClearSelection={handleClearSelection}
              onBulkDelete={handleBulkDelete}
              onBulkDuplicate={handleBulkDuplicate}
              isLoading={actionLoading === "delete" || actionLoading === "duplicate"}
            />

          {/* Content */}
          {loading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="bg-surface-container-lowest border border-outline-variant/10 rounded-2xl p-6 animate-pulse">
                  <div className="flex items-center gap-4 mb-4">
                    <div className="w-10 h-10 rounded-xl bg-surface-container" />
                    <div className="space-y-2 flex-1">
                      <div className="h-4 w-32 bg-surface-container rounded" />
                      <div className="h-3 w-24 bg-surface-container rounded" />
                    </div>
                  </div>
                  <div className="grid grid-cols-4 gap-2 mb-4">
                    <div className="h-12 bg-surface-container rounded-lg" />
                    <div className="h-12 bg-surface-container rounded-lg" />
                    <div className="h-12 bg-surface-container rounded-lg" />
                    <div className="h-12 bg-surface-container rounded-lg" />
                  </div>
                  <div className="h-2 bg-surface-container rounded-full" />
                </div>
              ))}
            </div>
          ) : filteredCampaigns.length === 0 ? (
            <CampaignEmptyState
              hasFilters={hasFilters}
              onCreate={() => setShowCreateModal(true)}
            />
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
              {filteredCampaigns.map((campaign, i) => (
                <CampaignCard
                  key={campaign.id}
                  campaign={campaign}
                  index={i}
                  isSelected={selectedIds.includes(campaign.id)}
                  isLoading={actionLoading === campaign.id}
                  onSelect={handleSelect}
                  onViewDetail={setDetailCampaign}
                  onEdit={setEditCampaign}
                  onToggleStatus={handleToggleStatus}
                  onRestart={handleRestart}
                  onDeploy={handleDeploy}
                  onActivate={handleActivate}
                  onCleanup={handleCleanup}
                  onDelete={handleDelete}
                />
              ))}
            </div>
          )}

          {hasMore && !loading && filteredCampaigns.length > 0 && (
            <div className="flex justify-center mt-4">
              <button
                onClick={handleLoadMore}
                disabled={loadingMore}
                className="px-6 py-3 border border-outline-variant/30 hover:bg-surface-container-high rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface transition-all disabled:opacity-50 flex items-center gap-2"
              >
                {loadingMore ? (
                  <span className="w-4 h-4 border-2 border-outline/30 border-t-outline rounded-full animate-spin" />
                ) : (
                  <span className="material-symbols-outlined text-[16px]">expand_more</span>
                )}
                {loadingMore ? "Loading..." : "Load More"}
              </button>
            </div>
          )}

        </div>

        {/* Modals */}
        <CreateCampaignModal
          open={showCreateModal}
          onClose={() => setShowCreateModal(false)}
          onCreate={handleCreate}
          isLoading={actionLoading === "create"}
        />

        <EditCampaignModal
          key={editCampaign?.id || "new"}
          campaign={editCampaign}
          onClose={() => setEditCampaign(null)}
          onUpdate={handleEdit}
          isLoading={actionLoading === "edit"}
        />

        <CampaignDetailModal
          campaign={detailCampaign}
          onClose={() => setDetailCampaign(null)}
          onDeploy={handleDeploy}
          onActivate={handleActivate}
          onCleanup={handleCleanup}
          onRestart={handleRestart}
          isLoading={actionLoading === detailCampaign?.id}
        />

        <StartConfirmModal
          campaign={startingCampaign}
          isLoading={actionLoading === startingCampaign?.id}
          onConfirm={handleConfirmStart}
          onCancel={() => setStartingCampaign(null)}
        />

        <DeleteConfirmModal
          campaigns={deletingCampaigns}
          isLoading={actionLoading === "delete"}
          onConfirm={handleConfirmDelete}
          onCancel={() => setDeletingCampaigns([])}
        />

      </main>
    </>
  );
}
