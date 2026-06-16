"use client";

import { useState, useEffect, useMemo } from "react";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import Header from "@/components/layout/Header";
import {
  fetchSocialAccounts,
  getFacebookAuthUrl,
  deleteSocialAccount,
  type SocialAccount,
  type SocialPlatform,
  type AccountStatus,
  getAccountStatus,
} from "@/services/socialAccountService";
import SocialStatsCards from "@/components/social/SocialStatsCards";
import SocialFilterBar, { type SortOption } from "@/components/social/SocialFilterBar";
import SocialAccountCard from "@/components/social/SocialAccountCard";
import SocialEmptyState from "@/components/social/SocialEmptyState";
import BulkActionsBar from "@/components/social/BulkActionsBar";
import ConnectAccountModal from "@/components/social/ConnectAccountModal";
import DisconnectConfirmModal from "@/components/social/DisconnectConfirmModal";
import ManageTargetsModal from "@/components/social/ManageTargetsModal";

export default function SocialAccountsPage() {
  const { activeWorkspace } = useWorkspaces();
  const [accounts, setAccounts] = useState<SocialAccount[]>([]);
  const [loading, setLoading] = useState(true);
  
  // Filters
  const [search, setSearch] = useState("");
  const [platformFilter, setPlatformFilter] = useState<SocialPlatform | "">("");
  const [statusFilter, setStatusFilter] = useState<AccountStatus | "">("");
  const [sortBy, setSortBy] = useState<SortOption>("newest");
  
  // Selection
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  
  // Modals
  const [showConnectModal, setShowConnectModal] = useState(false);
  const [deletingAccount, setDeletingAccount] = useState<SocialAccount | null>(null);
  const [managingTargetsAccount, setManagingTargetsAccount] = useState<SocialAccount | null>(null);
  const [actionLoading, setActionLoading] = useState<string | null>(null);
  
  // Toast
  const [toast, setToast] = useState<{ msg: string; type: "success" | "error" } | null>(null);

  // Load accounts
  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      try {
        const res = await fetchSocialAccounts();
        if (!cancelled) setAccounts(res.data);
      } catch {
        if (!cancelled) setAccounts([]);
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, [activeWorkspace?.id]);

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

  // Filter and sort accounts
  const filteredAccounts = useMemo(() => {
    let result = [...accounts];

    // Search filter
    if (search) {
      const q = search.toLowerCase();
      result = result.filter((a) =>
        a.accountName?.toLowerCase().includes(q) ||
        a.accountHandle?.toLowerCase().includes(q) ||
        a.targets?.some((t) => t.name.toLowerCase().includes(q))
      );
    }

    // Platform filter
    if (platformFilter) {
      result = result.filter((a) => a.provider === platformFilter);
    }

    // Status filter
    if (statusFilter) {
      result = result.filter((a) => getAccountStatus(a) === statusFilter);
    }

    // Sort
    switch (sortBy) {
      case "newest":
        result.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        break;
      case "expiring":
        result.sort((a, b) => {
          const aExp = a.expiresAt ? new Date(a.expiresAt).getTime() : Infinity;
          const bExp = b.expiresAt ? new Date(b.expiresAt).getTime() : Infinity;
          return aExp - bExp;
        });
        break;
      case "followers":
        result.sort((a, b) => (b.followers || 0) - (a.followers || 0));
        break;
      case "targets":
        result.sort((a, b) => (b.targets?.length || 0) - (a.targets?.length || 0));
        break;
    }

    return result;
  }, [accounts, search, platformFilter, statusFilter, sortBy]);

  // Handlers
  const handleConnect = async (platform: SocialPlatform) => {
    if (platform !== "facebook") {
      showToast("Only Facebook is supported in Phase C", "error");
      return;
    }

    setActionLoading("connect");
    try {
      const authResponse = await getFacebookAuthUrl();
      window.location.href = authResponse.authUrl;
    } catch (error) {
      showToast(error instanceof Error ? error.message : "Failed to get auth URL", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleDelete = async () => {
    if (!deletingAccount) return;
    setActionLoading(deletingAccount.id);
    try {
      await deleteSocialAccount(deletingAccount.id);
      setAccounts((prev) => prev.filter((a) => a.id !== deletingAccount.id));
      setSelectedIds((prev) => prev.filter((id) => id !== deletingAccount.id));
      setDeletingAccount(null);
      showToast("Account deleted successfully");
    } catch {
      showToast("Failed to delete account", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleBulkDelete = async () => {
    if (selectedIds.length === 0) return;
    setActionLoading("bulk");
    try {
      for (const id of selectedIds) {
        await deleteSocialAccount(id);
      }
      setAccounts((prev) => prev.filter((a) => !selectedIds.includes(a.id)));
      setSelectedIds([]);
      showToast(`${selectedIds.length} account(s) deleted`);
    } catch {
      showToast("Failed to delete accounts", "error");
    } finally {
      setActionLoading(null);
    }
  };

  const handleManageTargetsSuccess = async () => {
    try {
      const res = await fetchSocialAccounts();
      setAccounts(res.data);
    } catch {
      // ignore
    }
    showToast("Targets linked successfully");
  };

  const handleSelect = (id: string, selected: boolean) => {
    setSelectedIds((prev) =>
      selected ? [...prev, id] : prev.filter((x) => x !== id)
    );
  };

  const handleClearSelection = () => {
    setSelectedIds([]);
  };

  const hasFilters = !!(search || platformFilter || statusFilter);

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

      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Social Accounts" }]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-7xl mx-auto space-y-6">

          {/* Header */}
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 animate-fade-up">
            <div className="flex items-center gap-4">
              <div className="relative w-12 h-12 shrink-0">
                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-primary to-primary/70 animate-float shadow-lg shadow-primary/20" />
                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-white/15 to-transparent" />
                <div className="relative w-full h-full flex items-center justify-center">
                  <span className="material-symbols-outlined text-on-primary text-[24px]">public</span>
                </div>
              </div>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Social Accounts</h1>
                <p className="text-[11px] text-outline">{accounts.length} accounts connected</p>
              </div>
            </div>
            <button
              onClick={() => setShowConnectModal(true)}
              className="bg-primary text-on-primary px-5 py-2.5 rounded-xl text-label-sm font-bold flex items-center gap-1.5 shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95"
            >
              <span className="material-symbols-outlined text-[16px]">add_link</span>
              Connect Account
            </button>
          </div>

          {/* Stats */}
          <SocialStatsCards allAccounts={accounts} />

          {/* Filters */}
          <SocialFilterBar
            search={search}
            onSearchChange={setSearch}
            platformFilter={platformFilter}
            onPlatformFilterChange={setPlatformFilter}
            statusFilter={statusFilter}
            onStatusFilterChange={setStatusFilter}
            sortBy={sortBy}
            onSortChange={setSortBy}
            resultCount={filteredAccounts.length}
            totalCount={accounts.length}
          />

          {/* Bulk Actions */}
          <BulkActionsBar
            selectedCount={selectedIds.length}
            onClearSelection={handleClearSelection}
            onBulkDelete={handleBulkDelete}
            isLoading={actionLoading === "bulk"}
          />

          {/* Content */}
          {loading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="bg-surface-container-lowest border border-outline-variant/10 rounded-2xl p-6 animate-pulse">
                  <div className="flex items-center gap-4 mb-4">
                    <div className="w-14 h-14 rounded-xl bg-surface-container" />
                    <div className="space-y-2 flex-1">
                      <div className="h-4 w-32 bg-surface-container rounded" />
                      <div className="h-3 w-24 bg-surface-container rounded" />
                    </div>
                  </div>
                  <div className="grid grid-cols-3 gap-2 mb-4">
                    <div className="h-12 bg-surface-container rounded-xl" />
                    <div className="h-12 bg-surface-container rounded-xl" />
                    <div className="h-12 bg-surface-container rounded-xl" />
                  </div>
                  <div className="space-y-2">
                    <div className="h-3 w-full bg-surface-container rounded" />
                    <div className="h-3 w-2/3 bg-surface-container rounded" />
                  </div>
                </div>
              ))}
            </div>
          ) : filteredAccounts.length === 0 ? (
            <SocialEmptyState
              hasFilters={hasFilters}
              onConnect={() => setShowConnectModal(true)}
            />
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
              {filteredAccounts.map((account, i) => (
                <SocialAccountCard
                  key={account.id}
                  account={account}
                  index={i}
                  isSelected={selectedIds.includes(account.id)}
                  isLoading={actionLoading === account.id}
                  onDelete={setDeletingAccount}
                  onManageTargets={setManagingTargetsAccount}
                  onSelect={handleSelect}
                />
              ))}
            </div>
          )}
        </div>

        {/* Modals */}
        <ConnectAccountModal
          open={showConnectModal}
          onClose={() => setShowConnectModal(false)}
          onConnect={handleConnect}
          isLoading={actionLoading === "connect"}
        />

        <DisconnectConfirmModal
          account={deletingAccount}
          isLoading={actionLoading === deletingAccount?.id}
          onConfirm={handleDelete}
          onCancel={() => setDeletingAccount(null)}
        />

        <ManageTargetsModal
          account={managingTargetsAccount}
          onClose={() => setManagingTargetsAccount(null)}
          onSuccess={handleManageTargetsSuccess}
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
