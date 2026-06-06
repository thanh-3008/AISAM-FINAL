"use client";

import { useState, useEffect, useMemo } from "react";
import Header from "@/components/layout/Header";
import {
  fetchCampaigns,
  createCampaign,
  updateCampaign,
  updateCampaignStatus,
  deleteCampaign,
  type Campaign,
  type CampaignStatus,
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

export default function CampaignsPage() {
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [loading, setLoading] = useState(true);

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
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  // Toast
  const [toast, setToast] = useState<{ msg: string; type: "success" | "error" } | null>(null);

  // Load campaigns
  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      try {
        const res = await fetchCampaigns();
        if (!cancelled) setCampaigns(res.data);
      } catch {
        if (!cancelled) setCampaigns([]);
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, []);

  // Toast auto-dismiss
  useEffect(() => {
    if (toast) {
      const timer = setTimeout(() => setToast(null), 3000);
      return () => clearTimeout(timer);
    }
  }, [toast]);

  const showToast = (msg: string, type: "success" | "error" = "success") => {
    setToast({ msg, type });
  };

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
      showToast(`Campaign "${campaign.name}" created successfully`);
    } catch {
      showToast("Failed to create campaign", "error");
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
        showToast(`Campaign "${updated.name}" updated successfully`);
      }
    } catch {
      showToast("Failed to update campaign", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleToggleStatus = async (campaign: Campaign) => {
    setActionLoading(campaign.id);
    try {
      const newStatus: CampaignStatus = campaign.status === "ACTIVE" ? "PAUSED" : "ACTIVE";
      const updated = await updateCampaignStatus(campaign.id, newStatus);
      if (updated) {
        setCampaigns((prev) => prev.map((c) => (c.id === campaign.id ? updated : c)));
        showToast(`Campaign ${newStatus === "ACTIVE" ? "activated" : "paused"}`);
      }
    } catch {
      showToast("Failed to update campaign status", "error");
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

  const handleConfirmDelete = async () => {
    if (deletingCampaigns.length === 0) return;
    setActionLoading("delete");
    try {
      for (const campaign of deletingCampaigns) {
        await deleteCampaign(campaign.id);
      }
      setCampaigns((prev) => prev.filter((c) => !deletingCampaigns.some((d) => d.id === c.id)));
      setSelectedIds((prev) => prev.filter((id) => !deletingCampaigns.some((d) => d.id === id)));
      setDeletingCampaigns([]);
      showToast(`${deletingCampaigns.length} campaign(s) deleted`);
    } catch {
      showToast("Failed to delete campaign(s)", "error");
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

          {/* Stats */}
          <CampaignStatsCards campaigns={campaigns} />

          {/* Filters */}
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
            isLoading={actionLoading === "delete"}
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
                  onDelete={handleDelete}
                />
              ))}
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
        />

        <DeleteConfirmModal
          campaigns={deletingCampaigns}
          isLoading={actionLoading === "delete"}
          onConfirm={handleConfirmDelete}
          onCancel={() => setDeletingCampaigns([])}
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
      </main>
    </>
  );
}
