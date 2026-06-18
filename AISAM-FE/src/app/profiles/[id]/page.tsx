"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { motion, useReducedMotion } from "motion/react";
import { useWorkspaces, getWorkspaceTypeLabel } from "@/hooks/useWorkspaces";
import { useToast } from "@/contexts/ToastContext";
import WorkspaceSettingsSidebar, { WorkspaceSection } from "@/components/layout/WorkspaceSettingsSidebar";
import ConfirmationModal from "@/components/ui/ConfirmationModal";
import { apiFetch } from "@/lib/apiClient";
import {
  changePassword,
  getPaymentHistory,
  getCurrentSubscription,
  createCheckout,
  createCreditPackCheckout,
  type PaymentHistoryItem,
  type CurrentSubscription,
} from "@/services/profileSettingsService";
import {
  fetchWorkspaceMembers,
  fetchCreditUsageHistory,
  fetchCreditWallet,
  fetchWorkspaceDashboard,
  fetchPostQuota,
  type WorkspaceMember,
  type WorkspaceMemberRole,
  type CreditUsageRecord,
  type CreditWallet,
  type WorkspaceDashboard,
} from "@/services/workspaceService";
import { inviteMember, getWorkspaceInvitations, type WorkspaceInvitation, type WorkspaceMemberRole as InvitationRole } from "@/services/workspaceInvitationService";

interface Workspace {
  id: string;
  userId: string;
  name: string;
  workspaceType: number;
  companyName: string | null;
  bio: string | null;
  avatarUrl: string | null;
  status: number;
  createdAt: string;
  updatedAt: string;
  isOwner: boolean;
  memberRole: string | null;
}

function getInitials(name: string) {
  return name.split(" ").map(w => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

const PROFILE_TYPES = [
  { value: 0, label: "Free" },
  { value: 1, label: "Personal Plus" },
  { value: 2, label: "Personal Pro" },
  { value: 3, label: "Business Plus" },
  { value: 4, label: "Business Pro" },
];

const inputClass =
  "w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest px-4 py-2.5 text-body-sm text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all";

const labelClass = "text-label-sm font-semibold text-on-surface";

const statusConfig: Record<number, { label: string; class: string; dot: string }> = {
  0: { label: "Pending", class: "bg-amber-50 text-amber-700 border-amber-200/50", dot: "bg-amber-500" },
  1: { label: "Active", class: "bg-emerald-50 text-emerald-700 border-emerald-200/50", dot: "bg-emerald-500" },
  2: { label: "Suspended", class: "bg-red-50 text-red-700 border-red-200/50", dot: "bg-red-500" },
  3: { label: "Cancelled", class: "bg-surface-container-high text-on-surface-variant border-outline-variant/20", dot: "bg-outline" },
};

const container = {
  hidden: { opacity: 0 },
  show: { opacity: 1, transition: { staggerChildren: 0.08 } },
};

const item = {
  hidden: { opacity: 0, y: 12 },
  show: { opacity: 1, y: 0, transition: { duration: 0.4, ease: [0.16, 1, 0.3, 1] as const } },
};

export default function ProfileDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [workspace, setWorkspace] = useState<Workspace | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [editing, setEditing] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const initialSection = (searchParams.get("section") as WorkspaceSection) || "overview";
  const [activeSection, setActiveSection] = useState<WorkspaceSection>(
    ["overview", "my-profile", "team", "security", "billing", "subscription"].includes(initialSection) ? initialSection : "overview"
  );
  const { selectWorkspace, activeWorkspace } = useWorkspaces();
  const reduceMotion = useReducedMotion();
  const { showToast } = useToast();

  // Confirmation modal state
  const [confirmModal, setConfirmModal] = useState<{
    isOpen: boolean;
    title: string;
    message: string;
    onConfirm: () => void;
    type?: "danger" | "warning" | "info";
    confirmText?: string;
    isLoading?: boolean;
  }>({
    isOpen: false,
    title: "",
    message: "",
    onConfirm: () => {},
  });

  // Search state for members
  const [memberSearch, setMemberSearch] = useState("");

  const [form, setForm] = useState({ name: "", profileType: "", companyName: "", bio: "", avatarUrl: "" });
  
  // Security section state
  const [passwordForm, setPasswordForm] = useState({ currentPassword: "", newPassword: "", confirmPassword: "" });
  const [changingPassword, setChangingPassword] = useState(false);
  
  // Billing section state
  const [paymentHistory, setPaymentHistory] = useState<PaymentHistoryItem[]>([]);
  const [loadingPayments, setLoadingPayments] = useState(false);
  
  // Subscription section state
  const [subscription, setSubscription] = useState<CurrentSubscription | null>(null);
  const [loadingSubscription, setLoadingSubscription] = useState(false);
  const [upgradingPlan, setUpgradingPlan] = useState(false);

  // Team members state
  const [members, setMembers] = useState<WorkspaceMember[]>([]);
  const [loadingMembers, setLoadingMembers] = useState(false);
  const [invitations, setInvitations] = useState<WorkspaceInvitation[]>([]);
  const [loadingInvitations, setLoadingInvitations] = useState(false);
  const [memberFilter, setMemberFilter] = useState<"all" | "active" | "pending">("all");
  const [showInviteModal, setShowInviteModal] = useState(false);
  const [inviteForm, setInviteForm] = useState({ email: "", role: "Viewer" as InvitationRole });
  const [sendingInvite, setSendingInvite] = useState(false);

  // Credit usage state
  const [creditHistory, setCreditHistory] = useState<CreditUsageRecord[]>([]);
  const [loadingCreditHistory, setLoadingCreditHistory] = useState(false);
  const [creditPage, setCreditPage] = useState(1);
  const [creditTotalPages, setCreditTotalPages] = useState(0);
  const [creditTotalCount, setCreditTotalCount] = useState(0);
  const [creditFilter, setCreditFilter] = useState<"all" | "success" | "failed">("all");

  // Credit wallet state
  const [creditWallet, setCreditWallet] = useState<CreditWallet | null>(null);
  const [selectedCreditPack, setSelectedCreditPack] = useState<{ name: string; credits: number; price: string } | null>(null);
  const [showPurchaseConfirm, setShowPurchaseConfirm] = useState(false);
  const [purchasing, setPurchasing] = useState(false);
  const [purchaseSuccess, setPurchaseSuccess] = useState(false);

  // Role management state
  const [memberActionMenu, setMemberActionMenu] = useState<string | null>(null);
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [selectedMember, setSelectedMember] = useState<WorkspaceMember | null>(null);
  const [newRole, setNewRole] = useState<WorkspaceMemberRole>("Viewer");
  const [changingRole, setChangingRole] = useState(false);

  // Ownership transfer state
  const [showTransferModal, setShowTransferModal] = useState(false);
  const [selectedNewOwner, setSelectedNewOwner] = useState<WorkspaceMember | null>(null);
  const [transferring, setTransferring] = useState(false);

  // Member quota management state
  const [showQuotaModal, setShowQuotaModal] = useState(false);
  const [quotaMember, setQuotaMember] = useState<WorkspaceMember | null>(null);
  const [quotaMode, setQuotaMode] = useState<"SharedPool" | "LifetimeAssigned" | "MonthlyAssigned">("SharedPool");
  const [quotaLimit, setQuotaLimit] = useState<number>(1000);
  const [savingQuota, setSavingQuota] = useState(false);

  // Member detail view state
  const [selectedMemberDetail, setSelectedMemberDetail] = useState<WorkspaceMember | null>(null);

  // Bulk actions state
  const [selectedMembers, setSelectedMembers] = useState<Set<string>>(new Set());
  const [showBulkActions, setShowBulkActions] = useState(false);

  // Pagination state
  const [memberPage, setMemberPage] = useState(1);
  const membersPerPage = 10;

  // Billing tab state
  const [billingTab, setBillingTab] = useState<"overview" | "usage">("overview");

  // Overview section state
  const [dashboardData, setDashboardData] = useState<WorkspaceDashboard | null>(null);
  const [overviewPostQuota, setOverviewPostQuota] = useState<{ used: number; total: number } | null>(null);
  const [overviewCreditWallet, setOverviewCreditWallet] = useState<CreditWallet | null>(null);
  const [loadingOverview, setLoadingOverview] = useState(false);

  // Subscription expired state
  const [showExpiredBanner, setShowExpiredBanner] = useState(false);

  // Limited mode state (subscription expired < 90 days)
  const [isLimitedMode, setIsLimitedMode] = useState(false);
  const [showLimitedModeBanner, setShowLimitedModeBanner] = useState(false);

  // Archived workspace state (subscription expired 90-180 days)
  const [isArchived, setIsArchived] = useState(false);
  const [showArchivedBanner, setShowArchivedBanner] = useState(false);

  useEffect(() => {
    if (!id) return;
    const fetchWorkspace = async () => {
      try {
        const result = await apiFetch(`/profiles/${id}`);
        if (result?.success && result.data) {
          const w = result.data as Workspace;
          setWorkspace(w);
          setForm({
            name: w.name,
            profileType: String(w.workspaceType),
            companyName: w.companyName || "",
            bio: w.bio || "",
            avatarUrl: w.avatarUrl || "",
          });
        } else {
          setError(result?.message || "Workspace not found");
        }
      } catch {
        setError("Network error");
      } finally {
        setLoading(false);
      }
    };
    fetchWorkspace();
  }, [id]);

  const handleSave = async () => {
    if (!form.name.trim()) { setError("Name is required"); return; }
    if (!form.profileType) { setError("Workspace type is required"); return; }
    setSaving(true);
    setError(null);
    try {
      const formBody = new FormData();
      formBody.append("name", form.name.trim());
      formBody.append("profileType", form.profileType);
      if (form.companyName.trim()) formBody.append("companyName", form.companyName.trim());
      if (form.bio.trim()) formBody.append("bio", form.bio.trim());
      if (form.avatarUrl.trim()) formBody.append("avatarUrl", form.avatarUrl.trim());

      const result = await apiFetch(`/profiles/${id}`, {
        method: "PUT",
        body: formBody,
      });
      if (result?.success && result.data) {
        setWorkspace(result.data);
        setEditing(false);
      } else {
        setError(result?.message || "Update failed");
      }
    } catch {
      setError("Network error");
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setShowDeleteDialog(false);
    try {
      await apiFetch(`/profiles/${id}`, { method: "DELETE" });
      router.push("/profiles");
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : "Delete failed";
      setError(message);
    }
  };

  // Security section handlers
  const handleUpdatePassword = async () => {
    if (!passwordForm.currentPassword || !passwordForm.newPassword || !passwordForm.confirmPassword) {
      setError("All password fields are required");
      return;
    }
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setError("New passwords do not match");
      return;
    }
    if (passwordForm.newPassword.length < 8) {
      setError("Password must be at least 8 characters");
      return;
    }
    
    setChangingPassword(true);
    setError(null);
    try {
      const success = await changePassword({
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
        confirmPassword: passwordForm.confirmPassword,
      });
      if (success) {
        setPasswordForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
        setError(null);
        showToast({ type: "success", title: "Password changed", message: "Please login again with your new password." });
        // Optionally redirect to login
        // router.push("/login");
      } else {
        setError("Failed to change password. Please check your current password.");
      }
    } catch {
      setError("Network error while changing password");
    } finally {
      setChangingPassword(false);
    }
  };

  // Billing section handlers
  const handleLoadPaymentHistory = async () => {
    setLoadingPayments(true);
    const data = await getPaymentHistory(1, 10);
    if (data) {
      setPaymentHistory(data.data);
    } else {
      setError("Failed to load payment history.");
    }
    setLoadingPayments(false);
  };

  const handleDownloadInvoice = (invoiceId: string) => {
    // TODO: Implement invoice download when BE API is available
    showToast({ type: "info", title: "Download invoice", message: "Feature coming soon!" });
  };

  // Subscription section handlers
  const handleLoadSubscription = async () => {
    setLoadingSubscription(true);
    const data = await getCurrentSubscription();
    if (data) {
      setSubscription(data);

      if (data.status === "Expired" || data.status === "Cancelled") {
        setShowExpiredBanner(true);

        const now = Date.now();
        const endDate = data.endDate ? new Date(data.endDate).getTime() : now;
        const daysSinceExpiry = Math.floor((now - endDate) / (1000 * 60 * 60 * 24));

        if (daysSinceExpiry < 90) {
          setIsLimitedMode(true);
          setShowLimitedModeBanner(true);
          setIsArchived(false);
          setShowArchivedBanner(false);
        } else if (daysSinceExpiry >= 90 && daysSinceExpiry <= 180) {
          setIsLimitedMode(true);
          setShowLimitedModeBanner(false);
          setIsArchived(true);
          setShowArchivedBanner(true);
        }
      } else {
        setShowExpiredBanner(false);
        setIsLimitedMode(false);
        setShowLimitedModeBanner(false);
        setIsArchived(false);
        setShowArchivedBanner(false);
      }
    } else {
      setError("Failed to load subscription.");
    }
    setLoadingSubscription(false);
  };

  const handleUpgradePlan = async (planType: number) => {
    setUpgradingPlan(true);
    try {
      const planCodes = ["", "basic_monthly", "pro_monthly", "business_monthly"];
      const checkout = await createCheckout({
        planCode: planCodes[planType] || "basic_monthly",
        returnUrl: window.location.origin + "/profiles?payment=success",
        cancelUrl: window.location.origin + "/profiles?payment=cancelled",
      });
      if (checkout?.checkoutUrl) {
        window.location.href = checkout.checkoutUrl;
      } else {
        showToast({ type: "info", title: "Upgrade", message: "PayOS checkout will be available when backend is connected." });
      }
    } catch {
      setError("Network error while upgrading plan");
    } finally {
      setUpgradingPlan(false);
    }
  };

  const handleCancelPlan = () => {
    setConfirmModal({
      isOpen: true,
      title: "Cancel Subscription",
      message: "Are you sure you want to cancel your subscription? You will lose access to premium features immediately when the current period ends.",
      type: "danger",
      confirmText: "Cancel Subscription",
      onConfirm: async () => {
        setConfirmModal(prev => ({ ...prev, isOpen: false }));
        try {
          showToast({ type: "success", title: "Cancelled", message: "Subscription has been cancelled." });
          handleLoadSubscription();
        } catch {
          showToast({ type: "error", title: "Error", message: "Network error while cancelling subscription." });
        }
      },
    });
  };

  // Team section handlers
  const handleLoadMembers = async () => {
    setLoadingMembers(true);
    const data = await fetchWorkspaceMembers();
    if (data) {
      setMembers(data.data);
    } else {
      setError("Failed to load team members.");
    }
    setLoadingMembers(false);

    setLoadingInvitations(true);
    const invites = await getWorkspaceInvitations();
    setInvitations(invites);
    setLoadingInvitations(false);
  };

  const handleInviteMember = () => {
    setShowInviteModal(true);
  };

  const handleSendInvite = async () => {
    if (!inviteForm.email.trim()) {
      showToast({ type: "error", title: "Invalid email", message: "Please enter an email address." });
      return;
    }

    setSendingInvite(true);
    try {
      const result = await inviteMember({
        email: inviteForm.email.trim(),
        role: inviteForm.role,
      });

      if (result?.data) {
        setShowInviteModal(false);
        setInviteForm({ email: "", role: "Viewer" });
        showToast({ type: "success", title: "Invitation sent", message: `Invitation sent to ${inviteForm.email}` });
        handleLoadMembers();
      } else {
        showToast({ type: "error", title: "Invitation failed", message: result?.error || "Failed to send invitation. Please try again." });
      }
    } catch {
      showToast({ type: "error", title: "Network error", message: "Please check your connection and try again." });
    } finally {
      setSendingInvite(false);
    }
  };

  // Role management handlers
  const handleOpenRoleModal = (member: WorkspaceMember) => {
    setSelectedMember(member);
    setNewRole(member.role);
    setShowRoleModal(true);
    setMemberActionMenu(null);
  };

  const handleChangeRole = async () => {
    if (!selectedMember) return;
    setChangingRole(true);
    try {
      // TODO: Call API to change role when BE is ready
      // await updateMemberRole(selectedMember.id, newRole);
      await new Promise(resolve => setTimeout(resolve, 1000)); // Mock delay
      
      // Update local state
      setMembers(prev => prev.map(m => 
        m.id === selectedMember.id ? { ...m, role: newRole } : m
      ));
      
      setShowRoleModal(false);
      setSelectedMember(null);
      showToast({ type: "success", title: "Role updated", message: `Role updated to ${newRole} successfully!` });
    } catch {
      showToast({ type: "error", title: "Update failed", message: "Failed to update role. Please try again." });
    } finally {
      setChangingRole(false);
    }
  };

  const handleRemoveMember = (member: WorkspaceMember) => {
    setConfirmModal({
      isOpen: true,
      title: "Remove Member",
      message: `Are you sure you want to remove ${member.name} from the workspace? This action cannot be undone.`,
      type: "danger",
      confirmText: "Remove",
      onConfirm: () => {
        setMembers(prev => prev.filter(m => m.id !== member.id));
        setMemberActionMenu(null);
        showToast({ type: "success", title: "Member removed", message: `${member.name} has been removed from the workspace.` });
        setConfirmModal(prev => ({ ...prev, isOpen: false }));
      },
    });
  };

  // Ownership transfer handlers
  const handleOpenTransferModal = () => {
    setShowTransferModal(true);
    setSelectedNewOwner(null);
  };

  const handleTransferOwnership = async () => {
    if (!selectedNewOwner) return;
    setTransferring(true);
    try {
      await new Promise(resolve => setTimeout(resolve, 1500));
      setMembers(prev => prev.map(m => {
        if (m.id === selectedNewOwner.id) return { ...m, role: "Owner" as WorkspaceMemberRole };
        if (m.role === "Owner") return { ...m, role: "Manager" as WorkspaceMemberRole };
        return m;
      }));
      setShowTransferModal(false);
      setSelectedNewOwner(null);
      showToast({ type: "success", title: "Ownership transferred", message: `Ownership has been transferred to ${selectedNewOwner.name}.` });
    } catch {
      showToast({ type: "error", title: "Transfer failed", message: "Failed to transfer ownership. Please try again." });
    } finally {
      setTransferring(false);
    }
  };

  // Member quota management handlers
  const handleOpenQuotaModal = (member: WorkspaceMember) => {
    setQuotaMember(member);
    setQuotaMode("SharedPool");
    setQuotaLimit(1000);
    setShowQuotaModal(true);
    setMemberActionMenu(null);
  };

  const handleSaveQuota = async () => {
    if (!quotaMember) return;
    setSavingQuota(true);
    try {
      await new Promise(resolve => setTimeout(resolve, 1000));
      showToast({ 
        type: "success", 
        title: "Quota updated", 
        message: `Quota updated for ${quotaMember.name}: ${quotaMode}${quotaMode !== "SharedPool" ? ` - ${quotaLimit} credits` : ""}` 
      });
      setShowQuotaModal(false);
      setQuotaMember(null);
    } catch {
      showToast({ type: "error", title: "Update failed", message: "Failed to update quota. Please try again." });
    } finally {
      setSavingQuota(false);
    }
  };

  // Credit usage handlers
  const handleLoadCreditHistory = async (pg: number) => {
    setLoadingCreditHistory(true);
    try {
      const data = await fetchCreditUsageHistory(pg, 10);
      if (data) {
        setCreditHistory(data.data);
        setCreditTotalPages(data.totalPages);
        setCreditTotalCount(data.totalCount);
      }
    } catch {
      console.error("Failed to load credit history");
    } finally {
      setLoadingCreditHistory(false);
    }
  };

  const handleLoadCreditWallet = async () => {
    const wallet = await fetchCreditWallet();
    setCreditWallet(wallet);
  };

  const handlePurchaseCredits = async () => {
    if (!selectedCreditPack) return;
    setPurchasing(true);
    try {
      const creditPackCodes: Record<string, number> = { Starter: 1, Standard: 2, Growth: 3, Business: 4 };
      const checkout = await createCreditPackCheckout({
        creditPackCode: creditPackCodes[selectedCreditPack.name] || 1,
        returnUrl: window.location.origin + "/profiles?payment=success",
        cancelUrl: window.location.origin + "/profiles?payment=cancelled",
      });
      if (checkout?.checkoutUrl) {
        window.location.href = checkout.checkoutUrl;
      } else {
        await new Promise((resolve) => setTimeout(resolve, 2000));
        setPurchaseSuccess(true);
        if (creditWallet) {
          setCreditWallet({ ...creditWallet, balance: creditWallet.balance + selectedCreditPack.credits });
        }
        showToast({ type: "success", title: "Purchase successful", message: `${selectedCreditPack.credits.toLocaleString()} credits added to your workspace.` });
        setTimeout(() => setPurchaseSuccess(false), 3000);
      }
    } catch {
      showToast({ type: "error", title: "Payment failed", message: "Failed to process credit pack purchase." });
    } finally {
      setPurchasing(false);
      setShowPurchaseConfirm(false);
    }
  };

  // Overview section handlers
  const handleLoadOverview = async () => {
    setLoadingOverview(true);
    const [dashData, walletData, quotaData] = await Promise.all([
      fetchWorkspaceDashboard(),
      fetchCreditWallet(),
      fetchPostQuota(),
    ]);
    if (!dashData && !walletData && !quotaData) {
      setError("Failed to load overview data. Check your connection.");
    } else {
      setError(null);
    }
    setDashboardData(dashData);
    setOverviewCreditWallet(walletData);
    setOverviewPostQuota(quotaData);
    setLoadingOverview(false);
  };

  const handleUpdatePayment = () => {
    // TODO: Implement update payment method when BE API is available
    showToast({ type: "info", title: "Update Payment Method", message: "Feature coming soon!" });
  };

  // Load payment history when billing section is active
  useEffect(() => {
    if (activeSection === "billing" && paymentHistory.length === 0) {
      handleLoadPaymentHistory();
    }
  }, [activeSection, paymentHistory.length]);

  // Load subscription when subscription section is active
  useEffect(() => {
    if (activeSection === "subscription" && !subscription) {
      handleLoadSubscription();
      handleLoadCreditWallet();
    }
  }, [activeSection, subscription]);

  // Load members when team section is active
  useEffect(() => {
    if (activeSection === "team" && members.length === 0) {
      handleLoadMembers();
    }
  }, [activeSection, members.length]);

  // Load credit history when billing tab is usage
  useEffect(() => {
    if (activeSection === "billing" && billingTab === "usage" && creditHistory.length === 0) {
      handleLoadCreditHistory(1);
    }
  }, [activeSection, billingTab, creditHistory.length]);

  // Load overview data when overview section is active
  useEffect(() => {
    if (activeSection === "overview" && !dashboardData) {
      handleLoadOverview();
    }
  }, [activeSection, dashboardData]);

  const nextPaymentDate = "2026-07-08";

  if (loading) {
    return (
      <div className="min-h-[100dvh] bg-surface flex">
        <div className="flex-1 flex flex-col">
          <div className="flex-1 flex overflow-hidden">
            <div className="w-64 shrink-0 border-r border-outline-variant/20 bg-surface-container-low/30 p-5 space-y-2">
              <div className="h-10 bg-surface-container rounded-xl animate-pulse" />
              <div className="h-8 bg-surface-container rounded-lg animate-pulse mt-6" />
              <div className="h-8 bg-surface-container rounded-lg animate-pulse" />
              <div className="h-8 bg-surface-container rounded-lg animate-pulse" />
              <div className="h-8 bg-surface-container rounded-lg animate-pulse" />
              <div className="h-8 bg-surface-container rounded-lg animate-pulse" />
            </div>
            <main className="flex-1 p-8 space-y-6">
              <div className="h-6 w-48 bg-surface-container rounded-lg animate-pulse" />
              <div className="h-4 w-72 bg-surface-container rounded-lg animate-pulse" />
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-8">
                <div className="h-64 bg-surface-container rounded-2xl animate-pulse" />
                <div className="h-64 bg-surface-container rounded-2xl animate-pulse" />
              </div>
            </main>
          </div>
        </div>
      </div>
    );
  }

  if (error && !workspace) {
    return (
      <div className="min-h-[100dvh] bg-surface flex">
        <div className="flex-1 flex flex-col">
          <div className="flex-1 flex overflow-hidden">
            <div className="w-64 shrink-0 border-r border-outline-variant/20 bg-surface-container-low/30" />
            <main className="flex-1 flex items-center justify-center">
              <motion.div
                initial={reduceMotion ? undefined : { opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                className="text-center space-y-4"
              >
                <div className="w-14 h-14 mx-auto rounded-2xl bg-red-50 flex items-center justify-center">
                  <span className="material-symbols-outlined text-red-500 text-3xl">error_outline</span>
                </div>
                <p className="text-body-md text-red-600 font-semibold">{error}</p>
                <motion.button
                  whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                  onClick={() => router.push("/profiles")}
                  className="px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:bg-primary/90 transition-all shadow-sm shadow-primary/20"
                >
                  Back to Workspaces
                </motion.button>
              </motion.div>
            </main>
          </div>
        </div>
      </div>
    );
  }

  const avatarPreview = form.avatarUrl && (() => { try { new URL(form.avatarUrl); return true; } catch { return false; } })()
    ? form.avatarUrl
    : null;
  const planLabel = workspace ? getWorkspaceTypeLabel(workspace.workspaceType) : "";
  const initials = workspace ? getInitials(workspace.name) : "?";
  const statusInfo = workspace ? statusConfig[workspace.status] || statusConfig[0] : statusConfig[0];

  return (
    <div className="min-h-[100dvh] bg-surface flex">
      <div className="flex-1 flex flex-col">
        <div className="flex-1 flex overflow-hidden">
          <WorkspaceSettingsSidebar
            activeSection={activeSection}
            onSectionChange={setActiveSection}
            workspaceName={workspace?.name}
            workspaceInitials={initials}
          />

          <main className="flex-1 overflow-auto">
            <div className="p-6 md:p-8">
              {error && (
                <motion.div
                  initial={reduceMotion ? undefined : { opacity: 0, y: -10 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="flex items-center gap-3 rounded-xl border border-red-200 bg-red-50 px-5 py-4 text-body-sm text-red-800 mb-6"
                >
                  <span className="material-symbols-outlined text-red-500 text-[20px]">error</span>
                  <span className="flex-1">{error}</span>
                  <button onClick={() => setError(null)} className="text-red-400 hover:text-red-600 transition-colors">
                    <span className="material-symbols-outlined text-[18px]">close</span>
                  </button>
                </motion.div>
              )}

              <motion.div
                key={activeSection}
                initial={reduceMotion ? undefined : { opacity: 0, x: 10 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ duration: 0.3 }}
                className="space-y-6"
              >
                {/* ===== OVERVIEW ===== */}
                {activeSection === "overview" && (
                  <motion.div variants={reduceMotion ? undefined : container} initial={reduceMotion ? undefined : "hidden"} animate="show" className="space-y-8">
                    <motion.div variants={reduceMotion ? undefined : item} className="flex items-center justify-between">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 rounded-2xl bg-gradient-to-br from-primary/10 to-secondary/10 flex items-center justify-center ring-1 ring-primary/20">
                          <span className="material-symbols-outlined text-primary text-[24px]">dashboard</span>
                        </div>
                        <div>
                          <h2 className="text-2xl font-bold text-on-surface tracking-tight">Workspace Overview</h2>
                          <p className="text-body-sm text-on-surface-variant mt-0.5">{workspace?.name || "Workspace"} - Analytics & Insights</p>
                        </div>
                      </div>
                      <motion.button
                        whileHover={reduceMotion ? undefined : { scale: 1.02 }}
                        whileTap={reduceMotion ? undefined : { scale: 0.98 }}
                        onClick={() => handleLoadOverview()}
                        disabled={loadingOverview}
                        className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-surface-container-lowest border border-outline-variant/20 text-body-sm font-medium text-on-surface-variant hover:text-on-surface hover:bg-surface-container transition-all disabled:opacity-50"
                      >
                        <span className={`material-symbols-outlined text-[18px] ${loadingOverview ? "animate-spin" : ""}`}>refresh</span>
                        Refresh
                      </motion.button>
                    </motion.div>

                    {loadingOverview && !dashboardData ? (
                      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-5">
                        {[1, 2, 3, 4].map((i) => (
                          <motion.div key={i} variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/10 p-6 shadow-sm">
                            <div className="flex items-center gap-3 mb-4">
                              <div className="w-10 h-10 rounded-xl bg-surface-container animate-pulse" />
                              <div className="h-4 w-20 bg-surface-container rounded animate-pulse" />
                            </div>
                            <div className="h-8 w-24 bg-surface-container rounded animate-pulse mb-3" />
                            <div className="h-2 bg-surface-container rounded-full" />
                          </motion.div>
                        ))}
                      </div>
                    ) : (
                      <>
                        {/* KPI Cards */}
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-5">
                          {/* Credits Remaining */}
                          <motion.div
                            variants={reduceMotion ? undefined : item}
                            whileHover={reduceMotion ? undefined : { y: -4, transition: { duration: 0.2 } }}
                            className="group bg-surface-container-lowest rounded-2xl border border-outline-variant/10 p-6 shadow-sm hover:shadow-lg hover:border-emerald-200/50 transition-all duration-300"
                          >
                            <div className="flex items-center justify-between mb-4">
                              <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-emerald-500/10 to-emerald-600/5 flex items-center justify-center ring-1 ring-emerald-500/20 group-hover:ring-emerald-500/40 transition-all">
                                  <span className="material-symbols-outlined text-emerald-500 text-[20px]">token</span>
                                </div>
                                <span className="text-label-sm text-on-surface-variant font-medium">Credits</span>
                              </div>
                              <span className="text-label-xs text-emerald-600 font-semibold bg-emerald-50 px-2 py-0.5 rounded-full">
                                Active
                              </span>
                            </div>
                            <div className="space-y-3">
                              <div className="flex items-baseline gap-2">
                                <span className="text-3xl font-bold text-on-surface tabular-nums">{overviewCreditWallet?.balance.toLocaleString() || 0}</span>
                                <span className="text-label-sm text-outline">/ {overviewCreditWallet?.maxBalance.toLocaleString() || 0}</span>
                              </div>
                              <div className="relative h-2 bg-surface-container rounded-full overflow-hidden">
                                <motion.div
                                  initial={reduceMotion ? undefined : { width: 0 }}
                                  animate={{ width: `${overviewCreditWallet ? (overviewCreditWallet.balance / overviewCreditWallet.maxBalance) * 100 : 0}%` }}
                                  transition={{ duration: 1, ease: "easeOut" }}
                                  className="absolute inset-y-0 left-0 bg-gradient-to-r from-emerald-400 to-emerald-500 rounded-full"
                                />
                              </div>
                              <p className="text-label-xs text-outline flex items-center gap-1">
                                <span className="material-symbols-outlined text-[14px] text-emerald-500">trending_up</span>
                                {overviewCreditWallet ? Math.round((overviewCreditWallet.balance / overviewCreditWallet.maxBalance) * 100) : 0}% remaining
                              </p>
                            </div>
                          </motion.div>

                          {/* Posts This Month */}
                          <motion.div
                            variants={reduceMotion ? undefined : item}
                            whileHover={reduceMotion ? undefined : { y: -4, transition: { duration: 0.2 } }}
                            className="group bg-surface-container-lowest rounded-2xl border border-outline-variant/10 p-6 shadow-sm hover:shadow-lg hover:border-blue-200/50 transition-all duration-300"
                          >
                            <div className="flex items-center justify-between mb-4">
                              <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-blue-500/10 to-blue-600/5 flex items-center justify-center ring-1 ring-blue-500/20 group-hover:ring-blue-500/40 transition-all">
                                  <span className="material-symbols-outlined text-blue-500 text-[20px]">send</span>
                                </div>
                                <span className="text-label-sm text-on-surface-variant font-medium">Posts</span>
                              </div>
                              <span className="text-label-xs text-blue-600 font-semibold bg-blue-50 px-2 py-0.5 rounded-full">
                                Monthly
                              </span>
                            </div>
                            <div className="space-y-3">
                              <div className="flex items-baseline gap-2">
                                <span className="text-3xl font-bold text-on-surface tabular-nums">{overviewPostQuota?.used.toLocaleString() || 0}</span>
                                <span className="text-label-sm text-outline">/ {overviewPostQuota?.total.toLocaleString() || 0}</span>
                              </div>
                              <div className="relative h-2 bg-surface-container rounded-full overflow-hidden">
                                <motion.div
                                  initial={reduceMotion ? undefined : { width: 0 }}
                                  animate={{ width: `${overviewPostQuota ? (overviewPostQuota.used / overviewPostQuota.total) * 100 : 0}%` }}
                                  transition={{ duration: 1, ease: "easeOut" }}
                                  className="absolute inset-y-0 left-0 bg-gradient-to-r from-blue-400 to-blue-500 rounded-full"
                                />
                              </div>
                              <p className="text-label-xs text-outline flex items-center gap-1">
                                <span className="material-symbols-outlined text-[14px] text-blue-500">schedule</span>
                                {overviewPostQuota ? 100 - Math.round((overviewPostQuota.used / overviewPostQuota.total) * 100) : 100}% remaining
                              </p>
                            </div>
                          </motion.div>

                          {/* Total AI Usage */}
                          <motion.div
                            variants={reduceMotion ? undefined : item}
                            whileHover={reduceMotion ? undefined : { y: -4, transition: { duration: 0.2 } }}
                            className="group bg-surface-container-lowest rounded-2xl border border-outline-variant/10 p-6 shadow-sm hover:shadow-lg hover:border-purple-200/50 transition-all duration-300"
                          >
                            <div className="flex items-center justify-between mb-4">
                              <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-purple-500/10 to-purple-600/5 flex items-center justify-center ring-1 ring-purple-500/20 group-hover:ring-purple-500/40 transition-all">
                                  <span className="material-symbols-outlined text-purple-500 text-[20px]">auto_awesome</span>
                                </div>
                                <span className="text-label-sm text-on-surface-variant font-medium">AI Usage</span>
                              </div>
                              <span className="text-label-xs text-purple-600 font-semibold bg-purple-50 px-2 py-0.5 rounded-full">
                                This Month
                              </span>
                            </div>
                            <div className="space-y-3">
                              <span className="text-3xl font-bold text-on-surface tabular-nums">{dashboardData?.totalAiUsage.toLocaleString() || 0}</span>
                              <p className="text-label-xs text-outline flex items-center gap-1">
                                <span className="material-symbols-outlined text-[14px] text-purple-500">insights</span>
                                Total generations
                              </p>
                            </div>
                          </motion.div>

                          {/* Workspace Type */}
                          <motion.div
                            variants={reduceMotion ? undefined : item}
                            whileHover={reduceMotion ? undefined : { y: -4, transition: { duration: 0.2 } }}
                            className="group bg-surface-container-lowest rounded-2xl border border-outline-variant/10 p-6 shadow-sm hover:shadow-lg hover:border-amber-200/50 transition-all duration-300"
                          >
                            <div className="flex items-center justify-between mb-4">
                              <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-amber-500/10 to-amber-600/5 flex items-center justify-center ring-1 ring-amber-500/20 group-hover:ring-amber-500/40 transition-all">
                                  <span className="material-symbols-outlined text-amber-500 text-[20px]">
                                    {workspace && workspace.workspaceType >= 3 ? "business" : "person"}
                                  </span>
                                </div>
                                <span className="text-label-sm text-on-surface-variant font-medium">Type</span>
                              </div>
                              <span className="text-label-xs text-amber-600 font-semibold bg-amber-50 px-2 py-0.5 rounded-full">
                                {workspace && workspace.workspaceType >= 3 ? "Team" : "Personal"}
                              </span>
                            </div>
                            <div className="space-y-3">
                              <span className="text-xl font-bold text-on-surface">{planLabel}</span>
                              <p className="text-label-xs text-outline flex items-center gap-1">
                                <span className="material-symbols-outlined text-[14px] text-amber-500">info</span>
                                {workspace && workspace.workspaceType >= 3 ? "Team workspace" : "Individual workspace"}
                              </p>
                            </div>
                          </motion.div>
                        </div>

                        {/* Top Members & Usage Chart */}
                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                          {/* Top Members by Usage */}
                          <motion.div
                            variants={reduceMotion ? undefined : item}
                            className="bg-surface-container-lowest rounded-2xl border border-outline-variant/10 p-6 shadow-sm"
                          >
                            <div className="flex items-center justify-between mb-6">
                              <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary/10 to-primary/5 flex items-center justify-center ring-1 ring-primary/20">
                                  <span className="material-symbols-outlined text-primary text-[20px]">leaderboard</span>
                                </div>
                                <div>
                                  <h3 className="text-body-lg font-bold text-on-surface">Top Members</h3>
                                  <p className="text-label-xs text-on-surface-variant">By AI Usage</p>
                                </div>
                              </div>
                            </div>
                            <div className="space-y-4">
                              {dashboardData?.topMembers && dashboardData.topMembers.length > 0 ? (
                                dashboardData.topMembers.map((member, index) => {
                                  const maxUsage = dashboardData.topMembers[0]?.usage || 1;
                                  const pct = Math.round((member.usage / maxUsage) * 100);
                                  const medals = ["🥇", "🥈", "🥉"];
                                  return (
                                    <motion.div
                                      key={member.userId}
                                      initial={reduceMotion ? undefined : { opacity: 0, x: -20 }}
                                      animate={{ opacity: 1, x: 0 }}
                                      transition={{ delay: index * 0.1 }}
                                      className="flex items-center gap-4"
                                    >
                                      <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 flex items-center justify-center text-body-sm font-bold text-primary ring-2 ring-primary/20">
                                        {index < 3 ? medals[index] : index + 1}
                                      </div>
                                      <div className="flex-1 min-w-0">
                                        <div className="flex items-center justify-between mb-2">
                                          <span className="text-body-sm text-on-surface font-semibold truncate">{member.name}</span>
                                          <span className="text-label-sm text-on-surface-variant font-medium tabular-nums">{member.usage} credits</span>
                                        </div>
                                        <div className="relative h-2 bg-surface-container rounded-full overflow-hidden">
                                          <motion.div
                                            initial={reduceMotion ? undefined : { width: 0 }}
                                            animate={{ width: `${pct}%` }}
                                            transition={{ duration: 0.8, delay: index * 0.1, ease: "easeOut" }}
                                            className="absolute inset-y-0 left-0 bg-gradient-to-r from-primary to-primary-container rounded-full"
                                          />
                                        </div>
                                      </div>
                                    </motion.div>
                                  );
                                })
                              ) : (
                                <div className="text-center py-12">
                                  <div className="w-16 h-16 rounded-2xl bg-surface-container flex items-center justify-center mx-auto mb-4">
                                    <span className="material-symbols-outlined text-outline/40 text-3xl">group_off</span>
                                  </div>
                                  <p className="text-body-sm text-on-surface-variant">No member data yet</p>
                                  <p className="text-label-xs text-outline mt-1">Invite team members to see usage stats</p>
                                </div>
                              )}
                            </div>
                          </motion.div>

                          {/* Usage Breakdown */}
                          <motion.div
                            variants={reduceMotion ? undefined : item}
                            className="bg-surface-container-lowest rounded-2xl border border-outline-variant/10 p-6 shadow-sm"
                          >
                            <div className="flex items-center justify-between mb-6">
                              <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-secondary/10 to-secondary/5 flex items-center justify-center ring-1 ring-secondary/20">
                                  <span className="material-symbols-outlined text-secondary text-[20px]">pie_chart</span>
                                </div>
                                <div>
                                  <h3 className="text-body-lg font-bold text-on-surface">Usage Breakdown</h3>
                                  <p className="text-label-xs text-on-surface-variant">By content type</p>
                                </div>
                              </div>
                            </div>
                            <div className="space-y-5">
                              {[
                                { label: "Text Generation", value: 45, color: "from-blue-400 to-blue-500", icon: "text_fields", bgColor: "bg-blue-50", textColor: "text-blue-500" },
                                { label: "Image Generation", value: 30, color: "from-purple-400 to-purple-500", icon: "image", bgColor: "bg-purple-50", textColor: "text-purple-500" },
                                { label: "Video Generation", value: 15, color: "from-pink-400 to-pink-500", icon: "videocam", bgColor: "bg-pink-50", textColor: "text-pink-500" },
                                { label: "Other", value: 10, color: "from-gray-400 to-gray-500", icon: "more_horiz", bgColor: "bg-gray-50", textColor: "text-gray-500" },
                              ].map((item, idx) => (
                                <motion.div
                                  key={item.label}
                                  initial={reduceMotion ? undefined : { opacity: 0, x: -20 }}
                                  animate={{ opacity: 1, x: 0 }}
                                  transition={{ delay: idx * 0.1 }}
                                  className="flex items-center gap-4"
                                >
                                  <div className={`w-10 h-10 rounded-xl ${item.bgColor} flex items-center justify-center shrink-0`}>
                                    <span className={`material-symbols-outlined ${item.textColor} text-[20px]`}>{item.icon}</span>
                                  </div>
                                  <div className="flex-1 min-w-0">
                                    <div className="flex items-center justify-between mb-2">
                                      <span className="text-body-sm text-on-surface font-medium">{item.label}</span>
                                      <span className="text-label-sm text-on-surface-variant font-semibold tabular-nums">{item.value}%</span>
                                    </div>
                                    <div className="relative h-2 bg-surface-container rounded-full overflow-hidden">
                                      <motion.div
                                        initial={reduceMotion ? undefined : { width: 0 }}
                                        animate={{ width: `${item.value}%` }}
                                        transition={{ duration: 0.8, delay: idx * 0.1, ease: "easeOut" }}
                                        className={`absolute inset-y-0 left-0 bg-gradient-to-r ${item.color} rounded-full`}
                                      />
                                    </div>
                                  </div>
                                </motion.div>
                              ))}
                            </div>
                          </motion.div>
                        </div>

                        {/* Quick Actions */}
                        <motion.div
                          variants={reduceMotion ? undefined : item}
                          className="bg-gradient-to-br from-surface-container-lowest to-surface-container-low rounded-2xl border border-outline-variant/10 p-6 shadow-sm"
                        >
                          <div className="flex items-center gap-3 mb-6">
                            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary/10 to-secondary/10 flex items-center justify-center ring-1 ring-primary/20">
                              <span className="material-symbols-outlined text-primary text-[20px]">bolt</span>
                            </div>
                            <div>
                              <h3 className="text-body-lg font-bold text-on-surface">Quick Actions</h3>
                              <p className="text-label-xs text-on-surface-variant">Frequently used features</p>
                            </div>
                          </div>
                          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                            {[
                              { href: "/content/ai-generate", icon: "auto_awesome", label: "Generate Content", color: "text-primary", bg: "bg-primary/5", ring: "ring-primary/20" },
                              { href: "/posts", icon: "send", label: "View Posts", color: "text-blue-500", bg: "bg-blue-50", ring: "ring-blue-500/20" },
                              { onClick: () => setActiveSection("billing"), icon: "token", label: "Buy Credits", color: "text-emerald-500", bg: "bg-emerald-50", ring: "ring-emerald-500/20" },
                              { onClick: () => setActiveSection("team"), icon: "group", label: "Manage Team", color: "text-purple-500", bg: "bg-purple-50", ring: "ring-purple-500/20" },
                            ].map((action, idx) => (
                              <motion.button
                                key={idx}
                                whileHover={reduceMotion ? undefined : { scale: 1.05, y: -2 }}
                                whileTap={reduceMotion ? undefined : { scale: 0.95 }}
                                onClick={action.onClick as any}
                                {...(action.href && !action.onClick ? { as: "a", href: action.href } : {})}
                                className="flex flex-col items-center gap-3 p-5 rounded-xl bg-surface-container-lowest border border-outline-variant/10 hover:border-outline-variant/30 hover:shadow-md transition-all duration-200 group"
                              >
                                <div className={`w-12 h-12 rounded-xl ${action.bg} flex items-center justify-center ring-1 ${action.ring} group-hover:ring-2 transition-all`}>
                                  <span className={`material-symbols-outlined ${action.color} text-[24px]`}>{action.icon}</span>
                                </div>
                                <span className="text-label-sm text-on-surface font-medium text-center">{action.label}</span>
                              </motion.button>
                            ))}
                          </div>
                        </motion.div>
                      </>
                    )}
                  </motion.div>
                )}

                {/* ===== MY PROFILE ===== */}
                {activeSection === "my-profile" && (
                  <motion.div variants={reduceMotion ? undefined : container} initial={reduceMotion ? undefined : "hidden"} animate="show">
                    {editing ? (
                      <div className="space-y-6">
                        <motion.div variants={reduceMotion ? undefined : item}>
                          <h2 className="text-2xl font-bold text-on-surface tracking-tight">Edit Workspace</h2>
                          <p className="text-body-sm text-on-surface-variant mt-1.5">Update your workspace information below</p>
                        </motion.div>

                        {/* Profile Summary */}
                        <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-6 shadow-sm flex flex-col sm:flex-row items-center gap-5">
                          <div className="w-20 h-20 rounded-2xl flex items-center justify-center overflow-hidden bg-gradient-to-br from-primary/10 to-primary/5 shrink-0 border border-primary/10">
                            {avatarPreview ? (
                              <img src={avatarPreview} alt="Avatar" className="w-full h-full object-cover" />
                            ) : (
                              <span className="text-2xl text-primary/40 font-semibold">{initials}</span>
                            )}
                          </div>
                          <div className="text-center sm:text-left min-w-0 flex-1">
                            <h3 className="text-xl text-on-surface font-bold">{workspace?.name}</h3>
                            <p className="text-label-sm text-on-surface-variant mt-0.5">{planLabel}</p>
                          </div>
                        </motion.div>

                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                          <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-6 shadow-sm space-y-5">
                            <div className="flex items-center gap-2.5 mb-2">
                              <div className="w-9 h-9 rounded-xl bg-primary/5 flex items-center justify-center">
                                <span className="material-symbols-outlined text-primary text-[18px]">workspaces</span>
                              </div>
                              <h3 className="text-body-lg font-semibold text-on-surface">Workspace</h3>
                            </div>
                            <div className="space-y-1.5">
                              <label className={labelClass}>Name <span className="text-red-500">*</span></label>
                              <input className={inputClass} value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} />
                            </div>
                            <div className="space-y-1.5">
                              <label className={labelClass}>Workspace Type <span className="text-red-500">*</span></label>
                              <div className="relative">
                                <select className={`${inputClass} appearance-none pr-10`} value={form.profileType} onChange={e => setForm(f => ({ ...f, profileType: e.target.value }))}>
                                  <option value="" disabled>Select workspace type</option>
                                  {PROFILE_TYPES.map(pt => (
                                    <option key={pt.value} value={pt.value}>{pt.label}</option>
                                  ))}
                                </select>
                                <span className="absolute inset-y-0 right-3 flex items-center pointer-events-none text-outline">
                                  <span className="material-symbols-outlined text-[18px]">unfold_more</span>
                                </span>
                              </div>
                            </div>
                            <div className="space-y-1.5">
                              <label className={labelClass}>Company</label>
                              <input className={inputClass} value={form.companyName} onChange={e => setForm(f => ({ ...f, companyName: e.target.value }))} />
                            </div>
                          </motion.div>
                          <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-6 shadow-sm space-y-5">
                            <div className="flex items-center gap-2.5 mb-2">
                              <div className="w-9 h-9 rounded-xl bg-secondary/5 flex items-center justify-center">
                                <span className="material-symbols-outlined text-secondary text-[18px]">info</span>
                              </div>
                              <h3 className="text-body-lg font-semibold text-on-surface">Details</h3>
                            </div>
                            <div className="space-y-1.5">
                              <label className={labelClass}>Description</label>
                              <textarea className={`${inputClass} resize-none min-h-[100px]`} rows={4} value={form.bio} onChange={e => setForm(f => ({ ...f, bio: e.target.value }))} />
                            </div>
                            <div className="space-y-1.5">
                              <label className={labelClass}>Avatar URL</label>
                              <input className={inputClass} placeholder="https://example.com/avatar.png" type="url" value={form.avatarUrl} onChange={e => setForm(f => ({ ...f, avatarUrl: e.target.value }))} />
                            </div>
                          </motion.div>
                        </div>
                        <motion.div variants={reduceMotion ? undefined : item} className="flex justify-end gap-3">
                          <motion.button
                            whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                            onClick={() => setEditing(false)}
                            className="px-5 py-2.5 border border-outline-variant/40 text-on-surface rounded-xl font-semibold text-body-sm hover:bg-surface-container transition-colors"
                          >
                            Cancel
                          </motion.button>
                          <motion.button
                            whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                            onClick={handleSave}
                            disabled={saving}
                            className="px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:bg-primary/90 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 shadow-sm shadow-primary/20"
                          >
                            {saving ? (
                              <><svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" /></svg> Saving...</>
                            ) : "Save Changes"}
                          </motion.button>
                        </motion.div>
                      </div>
                    ) : (
                      <div className="space-y-6">
                        <motion.div variants={reduceMotion ? undefined : item} className="flex items-center justify-between">
                          <div>
                            <h2 className="text-2xl font-bold text-on-surface tracking-tight">Workspace Info</h2>
                            <p className="text-body-sm text-on-surface-variant mt-1.5">Manage your workspace information</p>
                          </div>
                          <motion.button
                            whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                            onClick={() => setEditing(true)}
                            className="inline-flex items-center gap-1.5 px-5 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-semibold hover:bg-primary/90 transition-all shadow-sm shadow-primary/20"
                          >
                            <span className="material-symbols-outlined text-[16px]">edit</span>
                            Edit
                          </motion.button>
                        </motion.div>

                        <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-6 shadow-sm flex flex-col sm:flex-row items-center gap-5">
                          <div className="w-20 h-20 rounded-2xl flex items-center justify-center overflow-hidden bg-gradient-to-br from-primary/10 to-primary/5 shrink-0 border border-primary/10">
                            {avatarPreview ? (
                              <img src={avatarPreview} alt="Avatar" className="w-full h-full object-cover" />
                            ) : (
                              <span className="text-2xl text-primary/40 font-semibold">{initials}</span>
                            )}
                          </div>
                          <div className="text-center sm:text-left min-w-0 flex-1">
                            <h3 className="text-xl text-on-surface font-bold">{workspace?.name}</h3>
                            <div className="flex items-center justify-center sm:justify-start flex-wrap gap-x-3 gap-y-1.5 mt-2">
                              <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-xs font-medium border ${statusInfo.class}`}>
                                <span className={`w-1.5 h-1.5 rounded-full ${statusInfo.dot} ${workspace?.status === 1 ? "animate-pulse" : ""}`} />
                                {statusInfo.label}
                              </span>
                              <span className="text-label-sm text-outline">{planLabel}</span>
                              {workspace?.isOwner && <span className="text-label-sm text-amber-600 font-semibold flex items-center gap-1"><span className="material-symbols-outlined text-[14px]">star</span>Owner</span>}
                            </div>
                            {workspace?.companyName && <p className="text-body-sm text-outline mt-2">{workspace.companyName}</p>}
                          </div>
                        </motion.div>

                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                          <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-6 shadow-sm">
                            <div className="flex items-center gap-2.5 mb-5">
                              <div className="w-9 h-9 rounded-xl bg-primary/5 flex items-center justify-center">
                                <span className="material-symbols-outlined text-primary text-[18px]">workspaces</span>
                              </div>
                              <h3 className="text-body-lg font-semibold text-on-surface">Workspace</h3>
                            </div>
                            <dl className="space-y-2">
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl bg-surface-container/40">
                                <dt className="text-label-sm text-outline">Name</dt>
                                <dd className="text-body-sm text-on-surface font-medium">{workspace?.name}</dd>
                              </div>
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                                <dt className="text-label-sm text-outline">Type</dt>
                                <dd className="text-body-sm text-on-surface font-medium">{planLabel}</dd>
                              </div>
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                                <dt className="text-label-sm text-outline">Company</dt>
                                <dd className="text-body-sm text-on-surface">{workspace?.companyName || "—"}</dd>
                              </div>
                            </dl>
                          </motion.div>
                          <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-6 shadow-sm">
                            <div className="flex items-center gap-2.5 mb-5">
                              <div className="w-9 h-9 rounded-xl bg-secondary/5 flex items-center justify-center">
                                <span className="material-symbols-outlined text-secondary text-[18px]">info</span>
                              </div>
                              <h3 className="text-body-lg font-semibold text-on-surface">Details</h3>
                            </div>
                            <dl className="space-y-2">
                              <div className="grid grid-cols-[110px_1fr] items-start py-2.5 px-3 -mx-3 rounded-xl bg-surface-container/40">
                                <dt className="text-label-sm text-outline">Description</dt>
                                <dd className="text-body-sm text-on-surface">{workspace?.bio || "—"}</dd>
                              </div>
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                                <dt className="text-label-sm text-outline">Avatar</dt>
                                <dd className="text-body-sm text-on-surface break-all">{workspace?.avatarUrl || "—"}</dd>
                              </div>
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                                <dt className="text-label-sm text-outline">Created</dt>
                                <dd className="text-body-sm text-on-surface">{workspace ? new Date(workspace.createdAt).toLocaleDateString() : "—"}</dd>
                              </div>
                            </dl>
                          </motion.div>
                        </div>

                        <motion.div variants={reduceMotion ? undefined : item} className="flex justify-end">
                          <motion.button
                            whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                            onClick={() => setShowDeleteDialog(true)}
                            className="inline-flex items-center gap-1.5 px-4 py-2 border border-red-200 text-red-600 rounded-xl text-body-sm font-medium hover:bg-red-50 hover:border-red-300 transition-colors"
                          >
                            <span className="material-symbols-outlined text-[16px]">delete</span>
                            Delete Workspace
                          </motion.button>
                        </motion.div>
                      </div>
                    )}
                  </motion.div>
                )}

                {/* ===== TEAM ===== */}
                {activeSection === "team" && (
                  <motion.div variants={reduceMotion ? undefined : container} initial={reduceMotion ? undefined : "hidden"} animate="show" className="space-y-6">
                    <motion.div variants={reduceMotion ? undefined : item} className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                      <div>
                        <h2 className="text-2xl font-bold text-on-surface tracking-tight">Team Members</h2>
                        <p className="text-body-sm text-on-surface-variant mt-1.5">Manage your team members and their permissions</p>
                      </div>
                      <div className="flex flex-wrap items-center gap-2">
                        {workspace?.isOwner && (
                          <motion.button
                            whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                            onClick={handleOpenTransferModal}
                            disabled={isLimitedMode}
                            className="px-3 sm:px-4 py-2 sm:py-2.5 bg-surface-container border border-outline-variant/30 text-on-surface rounded-xl text-label-sm sm:text-body-sm font-semibold hover:bg-surface-container-high transition-all inline-flex items-center gap-1.5 sm:gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <span className="material-symbols-outlined text-[16px] sm:text-[18px]">swap_horiz</span>
                            <span className="hidden sm:inline">Transfer Ownership</span>
                            <span className="sm:hidden">Transfer</span>
                          </motion.button>
                        )}
                        <motion.button
                          whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                          onClick={handleInviteMember}
                          disabled={isLimitedMode}
                          className="px-3 sm:px-5 py-2 sm:py-2.5 bg-primary text-on-primary rounded-xl text-label-sm sm:text-body-sm font-semibold hover:bg-primary/90 transition-all shadow-sm shadow-primary/20 inline-flex items-center gap-1.5 sm:gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          <span className="material-symbols-outlined text-[16px] sm:text-[18px]">person_add</span>
                          {isLimitedMode ? "Limited" : <span className="hidden sm:inline">Invite Member</span>}
                          {isLimitedMode ? "" : <span className="sm:hidden">Invite</span>}
                        </motion.button>
                      </div>
                    </motion.div>

                    {/* Team Stats */}
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                      {[
                        { label: "Total Members", value: String(members.length), icon: "group", color: "text-primary", bg: "bg-primary/5" },
                        { label: "Active", value: String(members.length), icon: "check_circle", color: "text-emerald-600", bg: "bg-emerald-50" },
                        { label: "Pending Invites", value: "0", icon: "schedule", color: "text-amber-600", bg: "bg-amber-50" },
                      ].map((stat) => (
                        <motion.div
                          key={stat.label}
                          variants={reduceMotion ? undefined : item}
                          className={`${stat.bg} rounded-2xl border border-outline-variant/10 p-5`}
                        >
                          <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-xl bg-white/60 flex items-center justify-center">
                              <span className={`material-symbols-outlined ${stat.color} text-[20px]`}>{stat.icon}</span>
                            </div>
                            <div>
                              <p className="text-label-sm text-on-surface-variant">{stat.label}</p>
                              <p className={`text-body-lg font-bold ${stat.color}`}>{stat.value}</p>
                            </div>
                          </div>
                        </motion.div>
                      ))}
                    </div>

                    {/* Filters and Search */}
                    <motion.div variants={reduceMotion ? undefined : item} className="flex flex-col sm:flex-row items-start sm:items-center gap-3">
                      <div className="flex items-center gap-2">
                        {[
                          { key: "all" as const, label: "All", count: members.length },
                          { key: "active" as const, label: "Active", count: members.length },
                          { key: "pending" as const, label: "Pending", count: 0 },
                        ].map((f) => (
                          <button
                            key={f.key}
                            onClick={() => setMemberFilter(f.key)}
                            className={`px-4 py-2 rounded-xl text-label-sm font-medium transition-all ${
                              memberFilter === f.key
                                ? "bg-primary text-on-primary shadow-sm"
                                : "bg-surface-container-lowest text-on-surface-variant hover:bg-surface-container border border-outline-variant/20"
                            }`}
                          >
                            {f.label}
                            <span className={`ml-2 px-1.5 py-0.5 rounded-full text-label-xs ${
                              memberFilter === f.key ? "bg-white/20" : "bg-surface-container"
                            }`}>
                              {f.count}
                            </span>
                          </button>
                        ))}
                      </div>
                      <div className="relative flex-1 w-full sm:w-auto sm:max-w-xs">
                        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-[18px]">search</span>
                        <input
                          type="text"
                          placeholder="Search members..."
                          value={memberSearch}
                          onChange={(e) => setMemberSearch(e.target.value)}
                          className="w-full pl-10 pr-4 py-2 rounded-xl border border-outline-variant/20 bg-surface-container-lowest text-body-sm text-on-surface placeholder:text-outline focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all"
                        />
                        {memberSearch && (
                          <button
                            onClick={() => setMemberSearch("")}
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors"
                          >
                            <span className="material-symbols-outlined text-[16px]">close</span>
                          </button>
                        )}
                      </div>
                    </motion.div>

                    {/* Team Members List */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 shadow-sm overflow-hidden">
                      <div className="px-6 py-4 border-b border-outline-variant/10 bg-surface-container/30">
                        <h3 className="text-body-md font-semibold text-on-surface">Members</h3>
                      </div>
                      <div className="divide-y divide-outline-variant/10">
                        {loadingMembers ? (
                          <div className="divide-y divide-outline-variant/10">
                            {[1, 2, 3, 4, 5].map((i) => (
                              <div key={i} className="px-6 py-4 flex items-center gap-4 animate-pulse">
                                <div className="w-12 h-12 rounded-full bg-surface-container-high shrink-0" />
                                <div className="flex-1 min-w-0 space-y-2">
                                  <div className="h-4 bg-surface-container-high rounded w-32" />
                                  <div className="h-3 bg-surface-container-high rounded w-48" />
                                </div>
                                <div className="h-6 bg-surface-container-high rounded-full w-20" />
                                <div className="h-6 bg-surface-container-high rounded-full w-16" />
                              </div>
                            ))}
                          </div>
                        ) : members.filter(m => {
                          const matchesFilter = memberFilter === "all" || memberFilter === "active";
                          const matchesSearch = !memberSearch || 
                            m.name.toLowerCase().includes(memberSearch.toLowerCase()) ||
                            m.email.toLowerCase().includes(memberSearch.toLowerCase());
                          return matchesFilter && matchesSearch;
                        }).length === 0 ? (
                          <div className="px-6 py-16 text-center">
                            <div className="w-20 h-20 rounded-full bg-surface-container flex items-center justify-center mx-auto mb-4">
                              <span className="material-symbols-outlined text-outline/40 text-4xl">group_off</span>
                            </div>
                            <h4 className="text-body-md font-semibold text-on-surface mb-2">
                              {memberSearch ? "No members match your search" : "No members found"}
                            </h4>
                            <p className="text-body-sm text-on-surface-variant max-w-sm mx-auto mb-4">
                              {memberSearch 
                                ? "Try adjusting your search terms or filters to find what you're looking for."
                                : "Start building your team by inviting members to collaborate on this workspace."}
                            </p>
                            {memberSearch ? (
                              <button
                                onClick={() => setMemberSearch("")}
                                className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:bg-primary/90 transition-all"
                              >
                                <span className="material-symbols-outlined text-[16px]">close</span>
                                Clear search
                              </button>
                            ) : (
                              <button
                                onClick={handleInviteMember}
                                disabled={isLimitedMode}
                                className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:bg-primary/90 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                              >
                                <span className="material-symbols-outlined text-[16px]">person_add</span>
                                Invite first member
                              </button>
                            )}
                          </div>
                        ) : (
                          (() => {
                            const filteredMembers = members.filter(m => {
                            const matchesFilter = memberFilter === "all" || memberFilter === "active";
                            const matchesSearch = !memberSearch || 
                              m.name.toLowerCase().includes(memberSearch.toLowerCase()) ||
                              m.email.toLowerCase().includes(memberSearch.toLowerCase());
                            return matchesFilter && matchesSearch;
                          });
                            const totalPages = Math.ceil(filteredMembers.length / membersPerPage);
                            const paginatedMembers = filteredMembers.slice((memberPage - 1) * membersPerPage, memberPage * membersPerPage);
                            
                            return (
                              <>
                                {paginatedMembers.map((member) => {
                            const roleConfig: Record<WorkspaceMemberRole, { label: string; color: string; bg: string; icon: string }> = {
                              Owner: { label: "Owner", color: "text-amber-700", bg: "bg-amber-50 border-amber-200/50", icon: "star" },
                              Manager: { label: "Manager", color: "text-blue-700", bg: "bg-blue-50 border-blue-200/50", icon: "manage_accounts" },
                              ContentCreator: { label: "Content Creator", color: "text-emerald-700", bg: "bg-emerald-50 border-emerald-200/50", icon: "edit_note" },
                              Viewer: { label: "Viewer", color: "text-outline", bg: "bg-surface-container border-outline-variant/20", icon: "visibility" },
                            };
                            const statusConfigMember: Record<string, { label: string; color: string; bg: string; dot: string }> = {
                              Active: { label: "Active", color: "text-emerald-700", bg: "bg-emerald-50 border-emerald-200/50", dot: "bg-emerald-500" },
                              Pending: { label: "Pending", color: "text-amber-700", bg: "bg-amber-50 border-amber-200/50", dot: "bg-amber-500" },
                              Invited: { label: "Invited", color: "text-blue-700", bg: "bg-blue-50 border-blue-200/50", dot: "bg-blue-500" },
                            };
                            const rb = roleConfig[member.role];
                            const sb = statusConfigMember["Active"];
                            const isSelected = selectedMembers.has(member.id);
                            return (
                              <div key={member.id} className={`px-6 py-4 flex items-center gap-4 hover:bg-surface-container/30 transition-colors ${isSelected ? "bg-primary/5" : ""}`}>
                                <input
                                  type="checkbox"
                                  checked={isSelected}
                                  onChange={(e) => {
                                    e.stopPropagation();
                                    const newSelected = new Set(selectedMembers);
                                    if (isSelected) {
                                      newSelected.delete(member.id);
                                    } else {
                                      newSelected.add(member.id);
                                    }
                                    setSelectedMembers(newSelected);
                                    setShowBulkActions(newSelected.size > 0);
                                  }}
                                  onClick={(e) => e.stopPropagation()}
                                  className="w-4 h-4 rounded border-outline-variant/40 text-primary focus:ring-primary/20 cursor-pointer"
                                />
                                <div 
                                  className="w-12 h-12 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 flex items-center justify-center text-body-md font-bold text-primary shrink-0 cursor-pointer hover:ring-2 hover:ring-primary/30 transition-all"
                                  onClick={() => setSelectedMemberDetail(member)}
                                >
                                  {getInitials(member.name)}
                                </div>
                                <div className="flex-1 min-w-0 cursor-pointer" onClick={() => setSelectedMemberDetail(member)}>
                                  <div className="flex items-center gap-2">
                                    <p className="text-body-sm text-on-surface font-semibold truncate">{member.name}</p>
                                    {member.role === "Owner" && (
                                      <span className="material-symbols-outlined text-amber-500 text-[16px]">star</span>
                                    )}
                                  </div>
                                  <p className="text-label-sm text-on-surface-variant truncate">{member.email}</p>
                                  {member.joinedAt && (
                                    <p className="text-label-xs text-outline mt-0.5">
                                      Joined: {new Date(member.joinedAt).toLocaleDateString("en-US", { month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" })}
                                    </p>
                                  )}
                                </div>
                                <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-label-xs font-semibold border ${rb.bg} ${rb.color}`}>
                                  <span className="material-symbols-outlined text-[12px]">{rb.icon}</span>
                                  {rb.label}
                                </span>
                                <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-xs font-medium border ${sb.bg} ${sb.color}`}>
                                  <span className={`w-1.5 h-1.5 rounded-full ${sb.dot} animate-pulse`} />
                                  {sb.label}
                                </span>
                                {member.role !== "Owner" && (
                                  <div className="relative">
                                    <button 
                                      onClick={() => setMemberActionMenu(memberActionMenu === member.id ? null : member.id)}
                                      className="p-2 rounded-lg hover:bg-surface-container transition-colors"
                                    >
                                      <span className="material-symbols-outlined text-on-surface-variant text-[20px]">more_vert</span>
                                    </button>
                                    {memberActionMenu === member.id && (
                                      <>
                                        <div className="fixed inset-0 z-10" onClick={() => setMemberActionMenu(null)} />
                                        <div className="absolute right-0 top-full mt-1 w-48 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-lg z-20 py-1.5 animate-in fade-in slide-in-from-top-2 duration-200">
                                          <button
                                            onClick={() => handleOpenRoleModal(member)}
                                            className="w-full flex items-center gap-3 px-4 py-2.5 text-body-sm text-on-surface hover:bg-surface-container transition-colors text-left"
                                          >
                                            <span className="material-symbols-outlined text-[18px] text-blue-500">swap_horiz</span>
                                            Change Role
                                          </button>
                                          <button
                                            onClick={() => handleOpenQuotaModal(member)}
                                            className="w-full flex items-center gap-3 px-4 py-2.5 text-body-sm text-on-surface hover:bg-surface-container transition-colors text-left"
                                          >
                                            <span className="material-symbols-outlined text-[18px] text-purple-500">data_thresholding</span>
                                            Assign Quota
                                          </button>
                                          <button
                                            onClick={() => handleRemoveMember(member)}
                                            className="w-full flex items-center gap-3 px-4 py-2.5 text-body-sm text-danger-red hover:bg-danger-red/5 transition-colors text-left"
                                          >
                                            <span className="material-symbols-outlined text-[18px]">person_remove</span>
                                            Remove Member
                                          </button>
                                        </div>
                                      </>
                                    )}
                                  </div>
                                )}
                              </div>
                            );
                          })}
                          
                          {/* Pagination */}
                          {totalPages > 1 && (
                            <div className="px-6 py-4 border-t border-outline-variant/10 flex items-center justify-between bg-surface-container/20">
                              <p className="text-body-sm text-on-surface-variant">
                                Showing {(memberPage - 1) * membersPerPage + 1} to {Math.min(memberPage * membersPerPage, filteredMembers.length)} of {filteredMembers.length} members
                              </p>
                              <div className="flex items-center gap-2">
                                <button
                                  onClick={() => setMemberPage(p => Math.max(1, p - 1))}
                                  disabled={memberPage === 1}
                                  className="px-3 py-1.5 rounded-lg text-label-sm font-medium bg-surface-container hover:bg-surface-container-high disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                >
                                  Previous
                                </button>
                                <span className="px-3 py-1.5 text-label-sm text-on-surface-variant">
                                  Page {memberPage} of {totalPages}
                                </span>
                                <button
                                  onClick={() => setMemberPage(p => Math.min(totalPages, p + 1))}
                                  disabled={memberPage === totalPages}
                                  className="px-3 py-1.5 rounded-lg text-label-sm font-medium bg-surface-container hover:bg-surface-container-high disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                >
                                  Next
                                </button>
                              </div>
                            </div>
                          )}
                        </>
                      );
                    })()
                  )}
                </div>
              </motion.div>

              {/* Pending Invitations */}
              {invitations.length > 0 && (
                <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 shadow-sm overflow-hidden">
                  <div className="px-6 py-4 border-b border-outline-variant/10 bg-surface-container/30">
                    <h3 className="text-body-md font-semibold text-on-surface">Pending Invitations</h3>
                  </div>
                  <div className="divide-y divide-outline-variant/10">
                    {loadingInvitations ? (
                      <div className="px-6 py-4 text-center text-body-sm text-on-surface-variant">Loading...</div>
                    ) : (
                      invitations.map((inv) => (
                        <div key={inv.id} className="px-6 py-4 flex items-center gap-4">
                          <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center shrink-0">
                            <span className="material-symbols-outlined text-[18px] text-blue-500">mail</span>
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="text-body-sm font-medium text-on-surface">{inv.email}</p>
                            <p className="text-label-xs text-outline">
                              Invited by {inv.invitedByName} · {new Date(inv.createdAt).toLocaleDateString()}
                            </p>
                          </div>
                          <span className="text-label-xs px-2.5 py-1 rounded-full bg-blue-50 text-blue-700 border border-blue-200/50 font-medium">
                            Pending
                          </span>
                        </div>
                      ))
                    )}
                  </div>
                </motion.div>
              )}

              {/* Bulk Actions Bar */}
              {showBulkActions && selectedMembers.size > 0 && (
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="fixed bottom-4 sm:bottom-6 left-4 right-4 sm:left-1/2 sm:-translate-x-1/2 sm:w-auto z-40 bg-surface-container-lowest border border-outline-variant/20 rounded-2xl shadow-2xl px-4 sm:px-6 py-3 sm:py-4 flex flex-col sm:flex-row items-stretch sm:items-center gap-3 sm:gap-4"
                >
                  <span className="text-body-sm font-semibold text-on-surface text-center sm:text-left">
                    {selectedMembers.size} member{selectedMembers.size > 1 ? "s" : ""} selected
                  </span>
                  <div className="hidden sm:block h-6 w-px bg-outline-variant/20" />
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => {
                        setConfirmModal({
                          isOpen: true,
                          title: "Remove Members",
                          message: `Are you sure you want to remove ${selectedMembers.size} member${selectedMembers.size > 1 ? "s" : ""}? This action cannot be undone.`,
                          type: "danger",
                          confirmText: "Remove All",
                          onConfirm: () => {
                            setMembers(prev => prev.filter(m => !selectedMembers.has(m.id)));
                            setSelectedMembers(new Set());
                            setShowBulkActions(false);
                            showToast({ type: "success", title: "Members removed", message: `${selectedMembers.size} member${selectedMembers.size > 1 ? "s" : ""} removed successfully.` });
                            setConfirmModal(prev => ({ ...prev, isOpen: false }));
                          },
                        });
                      }}
                      className="flex-1 sm:flex-none px-4 py-2 rounded-xl text-body-sm font-semibold text-danger-red hover:bg-danger-red/5 transition-colors flex items-center justify-center sm:justify-start gap-2"
                    >
                      <span className="material-symbols-outlined text-[16px]">person_remove</span>
                      Remove
                    </button>
                    <button
                      onClick={() => {
                        setSelectedMembers(new Set());
                        setShowBulkActions(false);
                      }}
                      className="flex-1 sm:flex-none px-4 py-2 rounded-xl text-body-sm font-semibold text-on-surface-variant hover:bg-surface-container transition-colors"
                    >
                      Cancel
                    </button>
                  </div>
                </motion.div>
              )}

              {/* Member Detail Modal */}
              {selectedMemberDetail && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
                  <motion.div
                    initial={reduceMotion ? undefined : { opacity: 0, scale: 0.95 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-md mx-4 p-6"
                  >
                    <div className="flex items-center justify-between mb-6">
                      <h3 className="text-body-lg font-bold text-on-surface">Member Details</h3>
                      <button
                        onClick={() => setSelectedMemberDetail(null)}
                        className="w-8 h-8 rounded-lg hover:bg-surface-container flex items-center justify-center transition-colors"
                      >
                        <span className="material-symbols-outlined text-on-surface-variant text-[20px]">close</span>
                      </button>
                    </div>

                    <div className="flex items-center gap-4 mb-6">
                      <div className="w-16 h-16 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 flex items-center justify-center text-body-lg font-bold text-primary">
                        {getInitials(selectedMemberDetail.name)}
                      </div>
                      <div>
                        <h4 className="text-body-md font-semibold text-on-surface">{selectedMemberDetail.name}</h4>
                        <p className="text-body-sm text-on-surface-variant">{selectedMemberDetail.email}</p>
                      </div>
                    </div>

                    <div className="space-y-4">
                      <div className="flex items-center justify-between p-3 rounded-xl bg-surface-container/50">
                        <span className="text-body-sm text-on-surface-variant">Role</span>
                        <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-label-xs font-semibold border ${
                          selectedMemberDetail.role === "Owner" ? "bg-amber-50 text-amber-700 border-amber-200/50" :
                          selectedMemberDetail.role === "Manager" ? "bg-blue-50 text-blue-700 border-blue-200/50" :
                          selectedMemberDetail.role === "ContentCreator" ? "bg-emerald-50 text-emerald-700 border-emerald-200/50" :
                          "bg-surface-container text-on-surface-variant border-outline-variant/20"
                        }`}>
                          <span className="material-symbols-outlined text-[12px]">
                            {selectedMemberDetail.role === "Owner" ? "star" :
                             selectedMemberDetail.role === "Manager" ? "manage_accounts" :
                             selectedMemberDetail.role === "ContentCreator" ? "edit_note" : "visibility"}
                          </span>
                          {selectedMemberDetail.role === "ContentCreator" ? "Content Creator" : selectedMemberDetail.role}
                        </span>
                      </div>

                      <div className="flex items-center justify-between p-3 rounded-xl bg-surface-container/50">
                        <span className="text-body-sm text-on-surface-variant">Status</span>
                        <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-xs font-medium border bg-emerald-50 text-emerald-700 border-emerald-200/50">
                          <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
                          Active
                        </span>
                      </div>

                      <div className="flex items-center justify-between p-3 rounded-xl bg-surface-container/50">
                        <span className="text-body-sm text-on-surface-variant">Joined</span>
                        <span className="text-body-sm text-on-surface">
                          {new Date(selectedMemberDetail.joinedAt).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}
                        </span>
                      </div>

                      {selectedMemberDetail.joinedAt && (
                        <div className="flex items-center justify-between p-3 rounded-xl bg-surface-container/50">
                          <span className="text-body-sm text-on-surface-variant">Last Active</span>
                          <span className="text-body-sm text-on-surface">
                            {new Date(selectedMemberDetail.joinedAt).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}
                          </span>
                        </div>
                      )}
                    </div>

                    <div className="flex gap-3 mt-6">
                      <button
                        onClick={() => setSelectedMemberDetail(null)}
                        className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-colors"
                      >
                        Close
                      </button>
                      {selectedMemberDetail.role !== "Owner" && (
                        <button
                          onClick={() => {
                            setSelectedMemberDetail(null);
                            handleOpenRoleModal(selectedMemberDetail);
                          }}
                          className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold bg-primary text-on-primary hover:bg-primary/90 transition-all shadow-sm shadow-primary/20"
                        >
                          Change Role
                        </button>
                      )}
                    </div>
                  </motion.div>
                </div>
              )}

                    {/* Workspace Roles Info */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-gradient-to-br from-primary/5 to-secondary/5 rounded-2xl border border-primary/10 p-6">
                      <div className="flex items-start gap-4 mb-4">
                        <div className="w-12 h-12 rounded-xl bg-white/80 flex items-center justify-center shrink-0">
                          <span className="material-symbols-outlined text-primary text-[24px]">admin_panel_settings</span>
                        </div>
                        <div className="flex-1">
                          <h4 className="text-body-md font-semibold text-on-surface mb-1">Workspace Roles</h4>
                          <p className="text-body-sm text-on-surface-variant">
                            Manage team members with role-based permissions
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
                    </motion.div>
                  </motion.div>
                )}

                {/* ===== SECURITY ===== */}
                {activeSection === "security" && (
                  <motion.div variants={reduceMotion ? undefined : container} initial={reduceMotion ? undefined : "hidden"} animate="show" className="space-y-6">
                    <motion.div variants={reduceMotion ? undefined : item}>
                      <h2 className="text-2xl font-bold text-on-surface tracking-tight">Security Settings</h2>
                      <p className="text-body-sm text-on-surface-variant mt-1.5">Manage your password and security preferences</p>
                    </motion.div>

                    {/* Security Status Card */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-gradient-to-br from-emerald-50 to-emerald-50/50 rounded-2xl border border-emerald-200/30 p-6">
                      <div className="flex items-center gap-4">
                        <div className="w-14 h-14 rounded-2xl bg-emerald-100 flex items-center justify-center">
                          <span className="material-symbols-outlined text-emerald-600 text-[28px]">shield</span>
                        </div>
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-1">
                            <h3 className="text-body-lg font-semibold text-on-surface">Account Security</h3>
                            <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-500 text-white text-label-xs font-semibold">
                              <span className="material-symbols-outlined text-[12px]">check</span>
                              Secure
                            </span>
                          </div>
                          <p className="text-body-sm text-on-surface-variant">Your account is protected with industry-standard security measures</p>
                        </div>
                      </div>
                    </motion.div>

                    {/* Change Password Form */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-6 shadow-sm">
                      <div className="flex items-center gap-3 mb-6">
                        <div className="w-10 h-10 rounded-xl bg-primary/5 flex items-center justify-center">
                          <span className="material-symbols-outlined text-primary text-[20px]">lock</span>
                        </div>
                        <div>
                          <h3 className="text-body-lg font-semibold text-on-surface">Change Password</h3>
                          <p className="text-label-sm text-on-surface-variant">Update your password to keep your account secure</p>
                        </div>
                      </div>
                      
                      <div className="max-w-md space-y-5">
                        <div className="space-y-2">
                          <label className={labelClass}>Current Password</label>
                          <div className="relative">
                            <input 
                              className={`${inputClass} pr-12`} 
                              type="password" 
                              placeholder="Enter current password"
                              value={passwordForm.currentPassword}
                              onChange={(e) => setPasswordForm({ ...passwordForm, currentPassword: e.target.value })}
                            />
                            <button type="button" className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors">
                              <span className="material-symbols-outlined text-[20px]">visibility</span>
                            </button>
                          </div>
                        </div>
                        
                        <div className="space-y-2">
                          <label className={labelClass}>New Password</label>
                          <div className="relative">
                            <input 
                              className={`${inputClass} pr-12`} 
                              type="password" 
                              placeholder="Enter new password"
                              value={passwordForm.newPassword}
                              onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })}
                            />
                            <button type="button" className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors">
                              <span className="material-symbols-outlined text-[20px]">visibility</span>
                            </button>
                          </div>
                          <div className="flex items-center gap-2 mt-2">
                            <div className="flex-1 h-1.5 bg-surface-container rounded-full overflow-hidden">
                              <div className={`h-full rounded-full transition-all ${
                                passwordForm.newPassword.length === 0 ? "w-0 bg-surface-container" :
                                passwordForm.newPassword.length < 8 ? "w-1/3 bg-red-500" :
                                passwordForm.newPassword.length < 12 ? "w-2/3 bg-gradient-to-r from-amber-400 to-emerald-500" :
                                "w-full bg-emerald-500"
                              }`} />
                            </div>
                            <span className={`text-label-xs font-medium ${
                              passwordForm.newPassword.length === 0 ? "text-outline" :
                              passwordForm.newPassword.length < 8 ? "text-red-600" :
                              passwordForm.newPassword.length < 12 ? "text-emerald-600" :
                              "text-emerald-600"
                            }`}>
                              {passwordForm.newPassword.length === 0 ? "—" :
                               passwordForm.newPassword.length < 8 ? "Weak" :
                               passwordForm.newPassword.length < 12 ? "Medium" : "Strong"}
                            </span>
                          </div>
                        </div>
                        
                        <div className="space-y-2">
                          <label className={labelClass}>Confirm New Password</label>
                          <div className="relative">
                            <input 
                              className={`${inputClass} pr-12`} 
                              type="password" 
                              placeholder="Confirm new password"
                              value={passwordForm.confirmPassword}
                              onChange={(e) => setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })}
                            />
                            <button type="button" className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-on-surface transition-colors">
                              <span className="material-symbols-outlined text-[20px]">visibility</span>
                            </button>
                          </div>
                          {passwordForm.confirmPassword && passwordForm.newPassword !== passwordForm.confirmPassword && (
                            <p className="text-label-xs text-red-600 mt-1">Passwords do not match</p>
                          )}
                        </div>

                        <div className="pt-2">
                          <motion.button
                            whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                            onClick={handleUpdatePassword}
                            disabled={changingPassword}
                            className="px-6 py-3 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:bg-primary/90 transition-all shadow-sm shadow-primary/20 inline-flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            {changingPassword ? (
                              <>
                                <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                                </svg>
                                Updating...
                              </>
                            ) : (
                              <>
                                <span className="material-symbols-outlined text-[18px]">save</span>
                                Update Password
                              </>
                            )}
                          </motion.button>
                        </div>
                      </div>
                    </motion.div>

                    {/* Security Tips */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container/30 rounded-2xl border border-outline-variant/10 p-6">
                      <div className="flex items-center gap-2.5 mb-4">
                        <span className="material-symbols-outlined text-primary text-[20px]">lightbulb</span>
                        <h4 className="text-body-md font-semibold text-on-surface">Security Tips</h4>
                      </div>
                      <ul className="space-y-3">
                        {[
                          "Use at least 12 characters with a mix of letters, numbers, and symbols",
                          "Avoid using personal information like birthdays or names",
                          "Enable two-factor authentication for extra security",
                          "Change your password every 3-6 months",
                        ].map((tip, i) => (
                          <li key={i} className="flex items-start gap-2.5">
                            <span className="material-symbols-outlined text-primary text-[16px] mt-0.5">check_circle</span>
                            <span className="text-body-sm text-on-surface-variant">{tip}</span>
                          </li>
                        ))}
                      </ul>
                    </motion.div>
                  </motion.div>
                )}

                {/* ===== BILLING & QUOTA ===== */}
                {activeSection === "billing" && (
                  <motion.div variants={reduceMotion ? undefined : container} initial={reduceMotion ? undefined : "hidden"} animate="show" className="space-y-6">
                    <motion.div variants={reduceMotion ? undefined : item} className="flex items-center justify-between">
                      <div>
                        <h2 className="text-2xl font-bold text-on-surface tracking-tight">Billing & Credits</h2>
                        <p className="text-body-sm text-on-surface-variant mt-1.5">Monitor your usage and manage billing</p>
                      </div>
                      <motion.button
                        whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                        onClick={() => handleDownloadInvoice("current")}
                        className="px-5 py-2.5 bg-surface-container border border-outline-variant/30 text-on-surface rounded-xl text-body-sm font-semibold hover:bg-surface-container-high transition-all inline-flex items-center gap-2"
                      >
                        <span className="material-symbols-outlined text-[18px]">download</span>
                        Download Invoice
                      </motion.button>
                    </motion.div>

                    {/* Billing Tabs */}
                    <motion.div variants={reduceMotion ? undefined : item} className="flex items-center gap-1 bg-surface-container/50 rounded-xl p-1">
                      {[
                        { key: "overview" as const, label: "Overview", icon: "receipt_long" },
                        { key: "usage" as const, label: "Usage", icon: "monitoring" },
                      ].map((tab) => (
                        <button
                          key={tab.key}
                          onClick={() => setBillingTab(tab.key)}
                          className={`flex-1 flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg text-body-sm font-semibold transition-all ${
                            billingTab === tab.key
                              ? "bg-surface-container-lowest text-primary shadow-sm"
                              : "text-on-surface-variant hover:text-on-surface"
                          }`}
                        >
                          <span className="material-symbols-outlined text-[18px]">{tab.icon}</span>
                          {tab.label}
                        </button>
                      ))}
                    </motion.div>

                    {billingTab === "overview" && (
                      <>
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-gradient-to-br from-emerald-50 to-emerald-50/50 rounded-2xl border border-emerald-200/30 p-6">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                          <div className="w-14 h-14 rounded-2xl bg-emerald-100 flex items-center justify-center">
                            <span className="material-symbols-outlined text-emerald-600 text-[28px]">token</span>
                          </div>
                          <div>
                            <div className="flex items-center gap-2 mb-1">
                              <h3 className="text-body-lg font-semibold text-on-surface">Credit Wallet</h3>
                              <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-emerald-500 text-white text-label-xs font-semibold">
                                <span className="material-symbols-outlined text-[12px]">check</span>
                                Active
                              </span>
                            </div>
                            <p className="text-body-sm text-on-surface-variant">Workspace AI credits balance</p>
                          </div>
                        </div>
                        <div className="text-right">
                          <p className="text-3xl font-bold text-emerald-600">850</p>
                          <p className="text-label-sm text-outline">Credits remaining</p>
                        </div>
                      </div>
                    </motion.div>

                    {/* Workspace Credits & Usage */}
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                      {[
                        { label: "AI Credits", used: "850", total: "2,000", pct: 43, icon: "token", color: "text-primary", bg: "bg-primary/5", bar: "bg-gradient-to-r from-primary to-primary-container", warning: false },
                        { label: "Posts This Month", used: "124", total: "1,000", pct: 12, icon: "send", color: "text-secondary", bg: "bg-secondary/5", bar: "bg-gradient-to-r from-secondary to-secondary-container", warning: false },
                        { label: "Team Members", used: "3", total: "10", pct: 30, icon: "group", color: "text-emerald-600", bg: "bg-emerald-50", bar: "bg-gradient-to-r from-emerald-500 to-emerald-400", warning: false },
                      ].map((q) => (
                        <motion.div
                          key={q.label}
                          variants={reduceMotion ? undefined : item}
                          className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-5 shadow-sm hover:shadow-md transition-shadow"
                        >
                          <div className="flex items-center justify-between mb-3">
                            <div className="flex items-center gap-2.5">
                              <div className={`w-9 h-9 rounded-xl ${q.bg} flex items-center justify-center`}>
                                <span className={`material-symbols-outlined ${q.color} text-[18px]`}>{q.icon}</span>
                              </div>
                              <p className="text-label-sm text-on-surface-variant font-medium">{q.label}</p>
                            </div>
                            {q.warning && (
                              <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-amber-50 text-amber-700 text-label-2xs font-semibold border border-amber-200/50">
                                <span className="material-symbols-outlined text-[10px]">warning</span>
                                High
                              </span>
                            )}
                          </div>
                          <div className="flex items-baseline gap-1 mb-3">
                            <p className="text-body-xl font-bold text-on-surface">{q.used}</p>
                            <p className="text-body-sm text-outline">/ {q.total}</p>
                          </div>
                          <div className="h-2 bg-surface-container rounded-full overflow-hidden">
                            <motion.div
                              initial={reduceMotion ? undefined : { width: 0 }}
                              animate={{ width: `${q.pct}%` }}
                              transition={{ duration: 1, ease: "easeOut" }}
                              className={`h-full rounded-full ${q.bar}`}
                            />
                          </div>
                          <p className="text-label-xs text-outline mt-2">{q.pct}% used</p>
                        </motion.div>
                      ))}
                    </div>

                    {/* Billing History */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 shadow-sm overflow-hidden">
                      <div className="px-6 py-4 border-b border-outline-variant/10 bg-surface-container/30 flex items-center justify-between">
                        <h3 className="text-body-md font-semibold text-on-surface">Billing History</h3>
                        <span className="text-label-sm text-outline">Recent transactions</span>
                      </div>
                      <div className="divide-y divide-outline-variant/10">
                        {loadingPayments ? (
                          <div className="px-6 py-8 text-center">
                            <div className="w-8 h-8 border-2 border-primary/20 border-t-primary rounded-full animate-spin mx-auto mb-3" />
                            <p className="text-body-sm text-on-surface-variant">Loading payment history...</p>
                          </div>
                        ) : paymentHistory.length === 0 ? (
                          <div className="px-6 py-12 text-center">
                            <span className="material-symbols-outlined text-outline/40 text-4xl mb-3 block">receipt_long</span>
                            <p className="text-body-sm text-on-surface-variant">No payment history yet</p>
                          </div>
                        ) : (
                          paymentHistory.map((invoice) => (
                            <div key={invoice.id} className="px-6 py-4 flex items-center justify-between hover:bg-surface-container/30 transition-colors">
                              <div className="flex items-center gap-4">
                                <div className="w-10 h-10 rounded-xl bg-primary/5 flex items-center justify-center">
                                  <span className="material-symbols-outlined text-primary text-[20px]">receipt</span>
                                </div>
                                <div>
                                  <div className="flex items-center gap-2">
                                    <p className="text-body-sm font-semibold text-on-surface">
                                      {new Date(invoice.createdAt).toLocaleDateString("en-US", { month: "short", year: "numeric" })}
                                    </p>
                                    <span className="px-2 py-0.5 rounded-full text-label-2xs font-semibold bg-primary/5 text-primary border border-primary/20">
                                      Payment
                                    </span>
                                  </div>
                                  <p className="text-label-sm text-on-surface-variant">{invoice.paymentMethod}</p>
                                </div>
                              </div>
                              <div className="flex items-center gap-4">
                                <p className="text-body-sm font-semibold text-on-surface">
                                  {new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(invoice.amount)}
                                </p>
                                <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-label-xs font-medium border ${
                                  invoice.status === "Completed" || invoice.status === "Success"
                                    ? "bg-emerald-50 text-emerald-700 border-emerald-200/50"
                                    : invoice.status === "Pending"
                                    ? "bg-amber-50 text-amber-700 border-amber-200/50"
                                    : "bg-red-50 text-red-700 border-red-200/50"
                                }`}>
                                  <span className="material-symbols-outlined text-[12px]">
                                    {invoice.status === "Completed" || invoice.status === "Success" ? "check_circle" : 
                                     invoice.status === "Pending" ? "schedule" : "error"}
                                  </span>
                                  {invoice.status}
                                </span>
                              </div>
                            </div>
                          ))
                        )}
                      </div>
                    </motion.div>

                    {/* Payment Method */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-gradient-to-br from-primary/5 to-secondary/5 rounded-2xl border border-primary/10 p-6">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                          <div className="w-14 h-10 rounded-lg bg-white shadow-sm flex items-center justify-center">
                            <div className="flex items-center gap-1">
                              <div className="w-6 h-6 rounded-full bg-red-500" />
                              <div className="w-6 h-6 rounded-full bg-amber-400 -ml-3" />
                            </div>
                          </div>
                          <div>
                            <p className="text-body-sm font-semibold text-on-surface">Visa ending in 4242</p>
                            <p className="text-label-sm text-on-surface-variant">Expires 12/2025</p>
                          </div>
                        </div>
                        <motion.button
                          whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                          onClick={handleUpdatePayment}
                          className="px-4 py-2 bg-white border border-outline-variant/30 text-on-surface rounded-xl text-label-sm font-semibold hover:bg-surface-container transition-all"
                        >
                          Update
                        </motion.button>
                      </div>
                    </motion.div>
                      </>
                    )}

                    {billingTab === "usage" && (
                      <>
                        {/* Credit Usage Summary */}
                        <motion.div variants={reduceMotion ? undefined : item} className="flex items-center justify-between bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-5 shadow-sm">
                          <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-xl bg-emerald-50 flex items-center justify-center">
                              <span className="material-symbols-outlined text-emerald-600 text-[20px]">token</span>
                            </div>
                            <div>
                              <p className="text-label-sm text-on-surface-variant">Total Credits Used</p>
                              <p className="text-body-lg font-bold text-emerald-600">
                                {creditHistory.filter(r => r.status === "Success").reduce((sum, r) => sum + r.credits, 0)}
                              </p>
                            </div>
                          </div>
                          <div className="flex items-center gap-2">
                            {[
                              { key: "all" as const, label: "All", count: creditTotalCount },
                              { key: "success" as const, label: "Success", count: creditHistory.filter(r => r.status === "Success").length },
                              { key: "failed" as const, label: "Failed", count: creditHistory.filter(r => r.status === "Failed").length },
                            ].map((f) => (
                              <button
                                key={f.key}
                                onClick={() => setCreditFilter(f.key)}
                                className={`px-3 py-1.5 rounded-lg text-label-xs font-medium transition-all ${
                                  creditFilter === f.key
                                    ? "bg-primary text-on-primary shadow-sm"
                                    : "bg-surface-container text-on-surface-variant hover:bg-surface-container-high"
                                }`}
                              >
                                {f.label} ({f.count})
                              </button>
                            ))}
                          </div>
                        </motion.div>

                        {/* Credit Usage Table */}
                        <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 shadow-sm overflow-hidden">
                          {loadingCreditHistory ? (
                            <div className="px-6 py-12 text-center">
                              <div className="w-8 h-8 border-2 border-primary/20 border-t-primary rounded-full animate-spin mx-auto mb-3" />
                              <p className="text-body-sm text-on-surface-variant">Loading credit history...</p>
                            </div>
                          ) : creditHistory.filter(r => {
                            if (creditFilter === "all") return true;
                            return r.status.toLowerCase() === creditFilter;
                          }).length === 0 ? (
                            <div className="px-6 py-12 text-center">
                              <span className="material-symbols-outlined text-outline/40 text-4xl mb-3 block">history</span>
                              <p className="text-body-sm text-on-surface-variant">No credit usage yet</p>
                            </div>
                          ) : (
                            <>
                              <div className="grid grid-cols-12 gap-4 px-6 py-3 bg-surface-container/50 border-b border-outline-variant/10 text-label-sm font-semibold text-outline">
                                <div className="col-span-5">Action</div>
                                <div className="col-span-3">Feature</div>
                                <div className="col-span-2 text-center">Credits</div>
                                <div className="col-span-2 text-right">Time</div>
                              </div>
                              <div className="divide-y divide-outline-variant/10">
                                {creditHistory.filter(r => {
                                  if (creditFilter === "all") return true;
                                  return r.status.toLowerCase() === creditFilter;
                                }).map((record) => {
                                  const actionIcons: Record<string, string> = {
                                    "generate text": "text_fields", "generate image": "image", "generate video": "videocam",
                                    "regenerate": "refresh", "refine": "refresh", "trend analysis": "trending_up",
                                    "campaign recommendation": "campaign",
                                  };
                                  const actionColors: Record<string, string> = {
                                    "generate text": "text-blue-500 bg-blue-50", "generate image": "text-purple-500 bg-purple-50",
                                    "generate video": "text-pink-500 bg-pink-50", "regenerate": "text-amber-500 bg-amber-50",
                                    "refine": "text-amber-500 bg-amber-50", "trend analysis": "text-emerald-500 bg-emerald-50",
                                    "campaign recommendation": "text-indigo-500 bg-indigo-50",
                                  };
                                  const icon = actionIcons[record.action.toLowerCase()] || "auto_awesome";
                                  const color = actionColors[record.action.toLowerCase()] || "text-primary bg-primary/5";
                                  return (
                                    <div key={record.id} className="grid grid-cols-12 gap-4 px-6 py-4 hover:bg-surface-container/30 transition-colors">
                                      <div className="col-span-5 flex items-center gap-3">
                                        <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${color}`}>
                                          <span className="material-symbols-outlined text-[20px]">{icon}</span>
                                        </div>
                                        <div>
                                          <p className="text-body-sm text-on-surface font-medium">{record.action}</p>
                                          <p className="text-label-xs text-outline">{record.userName}</p>
                                        </div>
                                      </div>
                                      <div className="col-span-3 flex items-center">
                                        <span className="text-body-sm text-on-surface-variant">{record.featureUsed}</span>
                                      </div>
                                      <div className="col-span-2 flex items-center justify-center">
                                        <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-label-sm font-semibold ${
                                          record.status === "Success" ? "bg-emerald-50 text-emerald-700" : "bg-red-50 text-red-700"
                                        }`}>
                                          {record.status === "Failed" && <span className="material-symbols-outlined text-[14px]">close</span>}
                                          {record.status === "Success" ? `-${record.credits}` : "0"}
                                        </span>
                                      </div>
                                      <div className="col-span-2 flex items-center justify-end">
                                        <span className="text-label-sm text-outline">
                                          {new Date(record.createdAt).toLocaleDateString("en-US", { month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" })}
                                        </span>
                                      </div>
                                    </div>
                                  );
                                })}
                              </div>
                              {creditTotalPages > 1 && (
                                <div className="flex items-center justify-between px-6 py-4 border-t border-outline-variant/10">
                                  <span className="text-label-sm text-outline">Page {creditPage} of {creditTotalPages}</span>
                                  <div className="flex items-center gap-2">
                                    <button
                                      onClick={() => { setCreditPage(p => Math.max(1, p - 1)); handleLoadCreditHistory(Math.max(1, creditPage - 1)); }}
                                      disabled={creditPage === 1}
                                      className="px-3 py-1.5 rounded-lg text-label-sm font-medium bg-surface-container hover:bg-surface-container-high disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                    >
                                      Previous
                                    </button>
                                    <button
                                      onClick={() => { setCreditPage(p => Math.min(creditTotalPages, p + 1)); handleLoadCreditHistory(Math.min(creditTotalPages, creditPage + 1)); }}
                                      disabled={creditPage === creditTotalPages}
                                      className="px-3 py-1.5 rounded-lg text-label-sm font-medium bg-surface-container hover:bg-surface-container-high disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                                    >
                                      Next
                                    </button>
                                  </div>
                                </div>
                              )}
                            </>
                          )}
                        </motion.div>

                        {/* About Credit Usage */}
                        <motion.div variants={reduceMotion ? undefined : item} className="bg-gradient-to-br from-primary/5 to-secondary/5 rounded-2xl border border-primary/10 p-6">
                          <div className="flex items-start gap-4">
                            <div className="w-12 h-12 rounded-xl bg-white/80 flex items-center justify-center shrink-0">
                              <span className="material-symbols-outlined text-primary text-[24px]">info</span>
                            </div>
                            <div>
                              <h4 className="text-body-md font-semibold text-on-surface mb-1">About Credit Usage</h4>
                              <ul className="space-y-1.5 text-body-sm text-on-surface-variant">
                                <li className="flex items-center gap-2">
                                  <span className="material-symbols-outlined text-[14px] text-primary">check_circle</span>
                                  Text generation costs 1 credit per request
                                </li>
                                <li className="flex items-center gap-2">
                                  <span className="material-symbols-outlined text-[14px] text-primary">check_circle</span>
                                  Image generation costs 5 credits per request
                                </li>
                                <li className="flex items-center gap-2">
                                  <span className="material-symbols-outlined text-[14px] text-primary">check_circle</span>
                                  Video generation costs 20 credits per request
                                </li>
                                <li className="flex items-center gap-2">
                                  <span className="material-symbols-outlined text-[14px] text-primary">check_circle</span>
                                  Failed requests do not consume credits
                                </li>
                              </ul>
                            </div>
                          </div>
                        </motion.div>
                      </>
                    )}
                  </motion.div>
                )}
                {activeSection === "subscription" && (
                  <motion.div variants={reduceMotion ? undefined : container} initial={reduceMotion ? undefined : "hidden"} animate="show" className="space-y-6">
                    <motion.div variants={reduceMotion ? undefined : item} className="flex items-center justify-between">
                      <div>
                        <h2 className="text-2xl font-bold text-on-surface tracking-tight">Subscription</h2>
                        <p className="text-body-sm text-on-surface-variant mt-1.5">Manage your plan and billing details</p>
                      </div>
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => setShowExpiredBanner(!showExpiredBanner)}
                          className="px-4 py-2 bg-amber-100 text-amber-800 rounded-lg text-body-sm font-medium hover:bg-amber-200 transition-colors inline-flex items-center gap-2"
                        >
                          <span className="material-symbols-outlined text-[16px]">warning</span>
                          {showExpiredBanner ? "Hide" : "Test"} Expired
                        </button>
                        <button
                          onClick={() => {
                            setShowLimitedModeBanner(!showLimitedModeBanner);
                            setIsLimitedMode(!isLimitedMode);
                          }}
                          className="px-4 py-2 bg-red-100 text-red-800 rounded-lg text-body-sm font-medium hover:bg-red-200 transition-colors inline-flex items-center gap-2"
                        >
                          <span className="material-symbols-outlined text-[16px]">lock</span>
                          {showLimitedModeBanner ? "Hide" : "Test"} Limited
                        </button>
                        <button
                          onClick={() => {
                            setShowArchivedBanner(!showArchivedBanner);
                            setIsArchived(!isArchived);
                          }}
                          className="px-4 py-2 bg-gray-100 text-gray-800 rounded-lg text-body-sm font-medium hover:bg-gray-200 transition-colors inline-flex items-center gap-2"
                        >
                          <span className="material-symbols-outlined text-[16px]">archive</span>
                          {showArchivedBanner ? "Hide" : "Test"} Archived
                        </button>
                      </div>
                    </motion.div>

                    {/* Test Banners */}
                    {showExpiredBanner && (
                      <motion.div
                        initial={reduceMotion ? undefined : { opacity: 0, y: -10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="rounded-xl border border-amber-200 bg-gradient-to-r from-amber-50 to-orange-50 px-5 py-4"
                      >
                        <div className="flex items-start gap-3">
                          <div className="w-10 h-10 rounded-xl bg-amber-100 flex items-center justify-center shrink-0">
                            <span className="material-symbols-outlined text-amber-600 text-[20px]">warning</span>
                          </div>
                          <div className="flex-1">
                            <h4 className="text-body-md font-semibold text-amber-900 mb-1">Subscription Expired</h4>
                            <p className="text-body-sm text-amber-800 mb-3">
                              Your subscription has expired. You still have <span className="font-bold">850 credits</span> remaining, but premium features are locked.
                            </p>
                            <div className="flex flex-wrap gap-2">
                              <button 
                                className="px-4 py-2 bg-amber-600 text-white rounded-lg text-body-sm font-semibold hover:bg-amber-700 transition-colors inline-flex items-center gap-2"
                              >
                                <span className="material-symbols-outlined text-[16px]">credit_card</span>
                                Renew Subscription
                              </button>
                              <button 
                                onClick={() => setShowExpiredBanner(false)}
                                className="px-4 py-2 bg-white border border-amber-300 text-amber-800 rounded-lg text-body-sm font-medium hover:bg-amber-50 transition-colors"
                              >
                                Dismiss
                              </button>
                            </div>
                          </div>
                        </div>
                      </motion.div>
                    )}

                    {showLimitedModeBanner && (
                      <motion.div
                        initial={reduceMotion ? undefined : { opacity: 0, y: -10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="rounded-xl border border-red-200 bg-gradient-to-r from-red-50 to-orange-50 px-5 py-4"
                      >
                        <div className="flex items-start gap-3">
                          <div className="w-10 h-10 rounded-xl bg-red-100 flex items-center justify-center shrink-0">
                            <span className="material-symbols-outlined text-red-600 text-[20px]">lock</span>
                          </div>
                          <div className="flex-1">
                            <h4 className="text-body-md font-semibold text-red-900 mb-1">Limited Mode Active</h4>
                            <p className="text-body-sm text-red-800 mb-2">
                              Your workspace subscription expired <span className="font-bold">45 days ago</span>. Workspace is in read-only mode.
                            </p>
                            <ul className="text-body-sm text-red-700 space-y-1 mb-3">
                              <li className="flex items-center gap-2">
                                <span className="material-symbols-outlined text-[14px]">check_circle</span>
                                Members can still login and view data
                              </li>
                              <li className="flex items-center gap-2">
                                <span className="material-symbols-outlined text-[14px]">cancel</span>
                                Creating, publishing, and inviting members are disabled
                              </li>
                              <li className="flex items-center gap-2">
                                <span className="material-symbols-outlined text-[14px]">cancel</span>
                                Premium and Business features are locked
                              </li>
                            </ul>
                            <div className="flex flex-wrap gap-2">
                              <button 
                                className="px-4 py-2 bg-red-600 text-white rounded-lg text-body-sm font-semibold hover:bg-red-700 transition-colors inline-flex items-center gap-2"
                              >
                                <span className="material-symbols-outlined text-[16px]">credit_card</span>
                                Renew Now
                              </button>
                              <button 
                                onClick={() => {
                                  setShowLimitedModeBanner(false);
                                  setIsLimitedMode(false);
                                }}
                                className="px-4 py-2 bg-white border border-red-300 text-red-800 rounded-lg text-body-sm font-medium hover:bg-red-50 transition-colors"
                              >
                                Dismiss
                              </button>
                            </div>
                          </div>
                        </div>
                      </motion.div>
                    )}

                    {showArchivedBanner && (
                      <motion.div
                        initial={reduceMotion ? undefined : { opacity: 0, y: -10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="rounded-xl border border-gray-300 bg-gradient-to-r from-gray-50 to-slate-50 px-5 py-4"
                      >
                        <div className="flex items-start gap-3">
                          <div className="w-10 h-10 rounded-xl bg-gray-200 flex items-center justify-center shrink-0">
                            <span className="material-symbols-outlined text-gray-600 text-[20px]">archive</span>
                          </div>
                          <div className="flex-1">
                            <h4 className="text-body-md font-semibold text-gray-900 mb-1">Workspace Archived</h4>
                            <p className="text-body-sm text-gray-700 mb-2">
                              Your workspace subscription expired <span className="font-bold">120 days ago</span>. Workspace is now archived.
                            </p>
                            <ul className="text-body-sm text-gray-600 space-y-1 mb-3">
                              <li className="flex items-center gap-2">
                                <span className="material-symbols-outlined text-[14px] text-gray-500">info</span>
                                As Owner, you can view, export data, or renew subscription
                              </li>
                              <li className="flex items-center gap-2">
                                <span className="material-symbols-outlined text-[14px] text-gray-500">info</span>
                                Workspace will be eligible for deletion after 180 days
                              </li>
                            </ul>
                            <div className="flex flex-wrap gap-2">
                              <button 
                                className="px-4 py-2 bg-gray-700 text-white rounded-lg text-body-sm font-semibold hover:bg-gray-800 transition-colors inline-flex items-center gap-2"
                              >
                                <span className="material-symbols-outlined text-[16px]">credit_card</span>
                                Renew Subscription
                              </button>
                              <button 
                                onClick={() => showToast({ type: "info", title: "Export Data", message: "Feature coming soon!" })}
                                className="px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg text-body-sm font-medium hover:bg-gray-50 transition-colors inline-flex items-center gap-2"
                              >
                                <span className="material-symbols-outlined text-[16px]">download</span>
                                Export Data
                              </button>
                              <button 
                                onClick={() => {
                                  setShowArchivedBanner(false);
                                  setIsArchived(false);
                                }}
                                className="px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg text-body-sm font-medium hover:bg-gray-50 transition-colors"
                              >
                                Dismiss
                              </button>
                            </div>
                          </div>
                        </div>
                      </motion.div>
                    )}

                    {/* Current Plan Card */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-gradient-to-br from-primary via-primary-container to-secondary rounded-2xl p-[1px] shadow-lg shadow-primary/20">
                      <div className="bg-surface-container-lowest rounded-2xl p-6">
                        {loadingSubscription ? (
                          <div className="flex items-center justify-center py-8">
                            <div className="w-8 h-8 border-2 border-primary/20 border-t-primary rounded-full animate-spin mr-3" />
                            <p className="text-body-sm text-on-surface-variant">Loading subscription...</p>
                          </div>
                        ) : (
                          <>
                            <div className="flex items-start justify-between gap-4 mb-6">
                              <div className="flex items-center gap-4">
                                <div className="w-16 h-16 rounded-2xl bg-gradient-to-br from-primary to-secondary flex items-center justify-center shadow-lg shadow-primary/30">
                                  <span className="material-symbols-outlined text-white text-[32px]">workspace_premium</span>
                                </div>
                                <div>
                                  <div className="flex items-center gap-2 mb-1">
                                    <h3 className="text-xl font-bold text-on-surface">
                                      {subscription?.planName || planLabel} Plan
                                    </h3>
                                    <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-label-xs font-semibold border ${statusInfo.class}`}>
                                      <span className={`w-1.5 h-1.5 rounded-full ${statusInfo.dot} ${workspace?.status === 1 ? "animate-pulse" : ""}`} />
                                      {subscription?.status || statusInfo.label}
                                    </span>
                                  </div>
                                  <p className="text-body-sm text-on-surface-variant">
                                    {subscription?.endDate 
                                      ? `Active until ${new Date(subscription.endDate).toLocaleDateString()}`
                                      : "Your current subscription plan"}
                                  </p>
                                </div>
                              </div>
                              {planLabel === "Free" && (
                                <motion.button
                                  whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                                  onClick={() => handleUpgradePlan(1)}
                                  disabled={upgradingPlan}
                                  className="px-5 py-2.5 bg-gradient-to-r from-primary to-secondary text-white rounded-xl text-body-sm font-semibold hover:opacity-90 transition-all shadow-md shadow-primary/20 shrink-0 disabled:opacity-50"
                                >
                                  {upgradingPlan ? "Processing..." : "Upgrade Plan"}
                                </motion.button>
                              )}
                            </div>

                            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 pt-6 border-t border-outline-variant/15">
                              <div className="p-4 rounded-xl bg-surface-container/40">
                                <div className="flex items-center gap-2 mb-2">
                                  <span className="material-symbols-outlined text-primary text-[18px]">autorenew</span>
                                  <p className="text-label-sm text-on-surface-variant">Billing Cycle</p>
                                </div>
                                <p className="text-body-md text-on-surface font-semibold">Monthly</p>
                                <p className="text-label-xs text-outline mt-1">
                                  Manual renewal
                                </p>
                              </div>
                              <div className="p-4 rounded-xl bg-surface-container/40">
                                <div className="flex items-center gap-2 mb-2">
                                  <span className="material-symbols-outlined text-primary text-[18px]">event</span>
                                  <p className="text-label-sm text-on-surface-variant">Next Payment</p>
                                </div>
                                <p className="text-body-md text-on-surface font-semibold">
                                  {subscription?.endDate
                                    ? new Date(subscription.endDate).toLocaleDateString()
                                    : nextPaymentDate}
                                </p>
                                <p className="text-label-xs text-outline mt-1">
                                  {planLabel === "Free" ? "Free" : "$29.00 USD"}
                                </p>
                              </div>
                              <div className="p-4 rounded-xl bg-surface-container/40">
                                <div className="flex items-center gap-2 mb-2">
                                  <span className="material-symbols-outlined text-primary text-[18px]">calendar_month</span>
                                  <p className="text-label-sm text-on-surface-variant">Start Date</p>
                                </div>
                                <p className="text-body-md text-on-surface font-semibold">
                                  {subscription?.startDate 
                                    ? new Date(subscription.startDate).toLocaleDateString()
                                    : "—"}
                                </p>
                                <p className="text-label-xs text-outline mt-1">Subscription start</p>
                              </div>
                            </div>
                          </>
                        )}
                      </div>
                    </motion.div>

                    {/* Plan Comparison */}
                    <motion.div variants={reduceMotion ? undefined : item}>
                      <h3 className="text-body-lg font-semibold text-on-surface mb-4">Compare Plans</h3>
                      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                        {[
                          {
                            name: "Free",
                            planType: 0,
                            price: "$0",
                            period: "/month",
                            current: planLabel === "Free",
                            features: ["Generate Text", "Manual Post", "Basic Analytics", "50 AI Credits/7 days", "20 Posts/week"],
                            cta: "Current Plan",
                          },
                          {
                            name: "Personal Plus",
                            planType: 1,
                            price: "$29",
                            period: "/month",
                            current: planLabel === "Personal Plus",
                            popular: true,
                            features: ["All Free features", "AI Image", "Content Calendar", "Schedule Post", "Multi Platform Publish", "500 Credits"],
                            cta: planLabel === "Personal Plus" ? "Current Plan" : "Upgrade",
                          },
                          {
                            name: "Personal Pro",
                            planType: 2,
                            price: "$79",
                            period: "/month",
                            current: planLabel === "Personal Pro",
                            features: ["All Personal Plus features", "Trend Analysis", "AI Video", "Advanced Analytics", "2,000 Credits", "1,000 Posts/month"],
                            cta: planLabel === "Personal Pro" ? "Current Plan" : "Upgrade",
                          },
                        ].map((plan) => (
                          <div
                            key={plan.name}
                            className={`relative rounded-2xl border p-6 ${
                              plan.popular
                                ? "border-primary shadow-lg shadow-primary/10 bg-gradient-to-b from-primary/5 to-transparent"
                                : plan.current
                                ? "border-primary/30 bg-primary/5"
                                : "border-outline-variant/20 bg-surface-container-lowest"
                            }`}
                          >
                            {plan.popular && (
                              <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                                <span className="px-3 py-1 bg-gradient-to-r from-primary to-secondary text-white text-label-xs font-bold rounded-full shadow-md">
                                  Most Popular
                                </span>
                              </div>
                            )}
                            <div className="mb-4">
                              <h4 className="text-body-lg font-bold text-on-surface">{plan.name}</h4>
                              <div className="flex items-baseline gap-1 mt-2">
                                <span className="text-3xl font-bold text-on-surface">{plan.price}</span>
                                <span className="text-body-sm text-outline">{plan.period}</span>
                              </div>
                            </div>
                            <ul className="space-y-2.5 mb-6">
                              {plan.features.map((feature) => (
                                <li key={feature} className="flex items-center gap-2">
                                  <span className="material-symbols-outlined text-emerald-500 text-[16px]">check_circle</span>
                                  <span className="text-label-sm text-on-surface-variant">{feature}</span>
                                </li>
                              ))}
                            </ul>
                            <motion.button
                              whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                              onClick={() => !plan.current && handleUpgradePlan(plan.planType)}
                              disabled={plan.current || upgradingPlan}
                              className={`w-full py-2.5 rounded-xl text-body-sm font-semibold transition-all ${
                                plan.popular
                                  ? "bg-gradient-to-r from-primary to-secondary text-white hover:opacity-90 shadow-md shadow-primary/20"
                                  : plan.current
                                  ? "bg-primary/10 text-primary border border-primary/20 cursor-default"
                                  : "bg-surface-container border border-outline-variant/30 text-on-surface hover:bg-surface-container-high"
                              } disabled:opacity-60 disabled:cursor-not-allowed`}
                            >
                              {upgradingPlan && !plan.current ? "Processing..." : plan.cta}
                            </motion.button>
                          </div>
                        ))}
                      </div>
                    </motion.div>

                    {/* Credit Pack */}
                    <motion.div variants={reduceMotion ? undefined : item}>
                      <div className="flex items-center justify-between mb-4">
                        <div>
                          <h3 className="text-body-lg font-semibold text-on-surface">Buy Credits</h3>
                          <p className="text-body-sm text-on-surface-variant">Purchase additional AI credits. Credits never expire.</p>
                        </div>
                        {creditWallet && (
                          <div className="flex items-center gap-2 px-4 py-2 rounded-xl bg-emerald-50 border border-emerald-200/30">
                            <span className="material-symbols-outlined text-emerald-500 text-[20px]">account_balance_wallet</span>
                            <span className="text-body-sm font-semibold text-emerald-700">{creditWallet.balance.toLocaleString()} credits</span>
                          </div>
                        )}
                      </div>

                      {purchaseSuccess && (
                        <div className="flex items-center gap-3 px-5 py-3 rounded-xl bg-emerald-600 text-white mb-4 animate-in slide-in-from-top-2">
                          <span className="material-symbols-outlined text-[20px]">check_circle</span>
                          <span className="text-body-sm font-semibold">Credits added successfully!</span>
                        </div>
                      )}

                      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                        {[
                          { name: "Starter", credits: 100, price: "29,000₫", icon: "bolt" },
                          { name: "Standard", credits: 500, price: "99,000₫", icon: "electric_bolt", popular: true },
                          { name: "Growth", credits: 1500, price: "249,000₫", icon: "local_fire_department" },
                          { name: "Business", credits: 5000, price: "699,000₫", icon: "whatshot" },
                        ].map((pack) => (
                          <div
                            key={pack.name}
                            className={`relative rounded-2xl border p-5 ${
                              pack.popular
                                ? "border-primary shadow-lg shadow-primary/10 bg-gradient-to-b from-primary/5 to-transparent"
                                : "border-outline-variant/20 bg-surface-container-lowest"
                            }`}
                          >
                            {pack.popular && (
                              <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                                <span className="px-3 py-1 bg-gradient-to-r from-primary to-secondary text-white text-label-xs font-bold rounded-full shadow-md">
                                  Best Value
                                </span>
                              </div>
                            )}
                            <div className="flex items-center gap-2 mb-3">
                              <span className={`material-symbols-outlined text-[24px] ${pack.popular ? "text-primary" : "text-outline"}`}>{pack.icon}</span>
                              <h4 className="text-body-md font-bold text-on-surface">{pack.name}</h4>
                            </div>
                            <div className="mb-3">
                              <span className="text-2xl font-bold text-on-surface">{pack.credits.toLocaleString()}</span>
                              <span className="text-label-sm text-outline ml-1">Credits</span>
                            </div>
                            <p className="text-body-lg font-semibold text-primary mb-4">{pack.price}</p>
                            <motion.button
                              whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                              onClick={() => { setSelectedCreditPack(pack); setShowPurchaseConfirm(true); }}
                              className={`w-full py-2.5 rounded-xl text-body-sm font-semibold transition-all ${
                                pack.popular
                                  ? "bg-gradient-to-r from-primary to-secondary text-white hover:opacity-90 shadow-md shadow-primary/20"
                                  : "bg-surface-container border border-outline-variant/30 text-on-surface hover:bg-surface-container-high"
                              }`}
                            >
                              Purchase
                            </motion.button>
                          </div>
                        ))}
                      </div>
                    </motion.div>

                    {/* Cancel Subscription */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-red-50/50 rounded-2xl border border-red-200/30 p-6">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                          <div className="w-12 h-12 rounded-xl bg-red-100 flex items-center justify-center">
                            <span className="material-symbols-outlined text-red-500 text-[24px]">cancel_schedule_send</span>
                          </div>
                          <div>
                            <h4 className="text-body-md font-semibold text-on-surface">Cancel Subscription</h4>
                            <p className="text-label-sm text-on-surface-variant">Stop your subscription and downgrade to Free plan</p>
                          </div>
                        </div>
                        <motion.button
                          whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                          onClick={handleCancelPlan}
                          className="px-5 py-2.5 border border-red-200 text-red-600 rounded-xl text-body-sm font-semibold hover:bg-red-50 hover:border-red-300 transition-all"
                        >
                          Cancel Plan
                        </motion.button>
                      </div>
                    </motion.div>
                  </motion.div>
                )}
              </motion.div>

              {/* Delete Confirmation Dialog */}
              {showDeleteDialog && (
                <motion.div
                  initial={reduceMotion ? undefined : { opacity: 0 }}
                  animate={{ opacity: 1 }}
                  className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
                >
                  <motion.div
                    initial={reduceMotion ? undefined : { opacity: 0, scale: 0.95 }}
                    animate={{ opacity: 1, scale: 1 }}
                    transition={{ duration: 0.2 }}
                    className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 shadow-xl p-6 w-full max-w-sm mx-4"
                  >
                    <div className="flex items-center gap-3 mb-4">
                      <div className="w-10 h-10 rounded-xl bg-red-50 flex items-center justify-center">
                        <span className="material-symbols-outlined text-red-500 text-[22px]">delete</span>
                      </div>
                      <div>
                        <h3 className="text-body-lg text-on-surface font-semibold">Delete Workspace</h3>
                        <p className="text-body-sm text-on-surface-variant">This action cannot be undone</p>
                      </div>
                    </div>
                    <p className="text-body-sm text-on-surface-variant mb-6">
                      Are you sure you want to delete <span className="font-semibold text-on-surface">{workspace?.name}</span>? All associated data will be permanently removed.
                    </p>
                    <div className="flex justify-end gap-3">
                      <motion.button
                        whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                        onClick={() => setShowDeleteDialog(false)}
                        className="px-5 py-2.5 border border-outline-variant/40 text-on-surface rounded-xl font-semibold text-body-sm hover:bg-surface-container transition-colors"
                      >
                        Cancel
                      </motion.button>
                      <motion.button
                        whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                        onClick={handleDelete}
                        className="px-5 py-2.5 bg-red-500 text-white rounded-xl font-semibold text-body-sm hover:bg-red-600 transition-all shadow-sm flex items-center gap-2"
                      >
                        Delete
                      </motion.button>
                      </div>
                    </motion.div>

                    {/* Change Role Modal */}
                    {showRoleModal && selectedMember && (
                      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
                        <motion.div
                          initial={reduceMotion ? undefined : { opacity: 0, scale: 0.95 }}
                          animate={{ opacity: 1, scale: 1 }}
                          className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-md mx-4 p-6"
                        >
                          <div className="flex items-center gap-3 mb-6">
                            <div className="w-12 h-12 rounded-xl bg-blue-50 flex items-center justify-center">
                              <span className="material-symbols-outlined text-blue-500 text-[24px]">swap_horiz</span>
                            </div>
                            <div>
                              <h3 className="text-body-lg font-bold text-on-surface">Change Role</h3>
                              <p className="text-label-sm text-on-surface-variant">Update role for {selectedMember.name}</p>
                            </div>
                          </div>

                          <div className="mb-6">
                            <label className="text-label-sm font-semibold text-on-surface mb-3 block">Select New Role</label>
                            <div className="space-y-2">
                              {[
                                { value: "Manager", label: "Manager", icon: "manage_accounts", color: "text-blue-600", bg: "bg-blue-50", desc: "Manage brands, content, campaigns" },
                                { value: "ContentCreator", label: "Content Creator", icon: "edit_note", color: "text-emerald-600", bg: "bg-emerald-50", desc: "Create and publish content" },
                                { value: "Viewer", label: "Viewer", icon: "visibility", color: "text-outline", bg: "bg-surface-container", desc: "View dashboard and analytics only" },
                              ].map((role) => (
                                <button
                                  key={role.value}
                                  onClick={() => setNewRole(role.value as WorkspaceMemberRole)}
                                  className={`w-full flex items-center gap-3 p-4 rounded-xl border-2 transition-all ${
                                    newRole === role.value
                                      ? "border-primary bg-primary/5"
                                      : "border-outline-variant/20 hover:border-outline-variant/40 hover:bg-surface-container/50"
                                  }`}
                                >
                                  <div className={`w-10 h-10 rounded-lg ${role.bg} flex items-center justify-center`}>
                                    <span className={`material-symbols-outlined ${role.color} text-[20px]`}>{role.icon}</span>
                                  </div>
                                  <div className="flex-1 text-left">
                                    <p className="text-body-sm font-semibold text-on-surface">{role.label}</p>
                                    <p className="text-label-xs text-on-surface-variant">{role.desc}</p>
                                  </div>
                                  {newRole === role.value && (
                                    <span className="material-symbols-outlined text-primary text-[20px]">check_circle</span>
                                  )}
                                </button>
                              ))}
                            </div>
                          </div>

                          <div className="flex gap-3">
                            <button
                              onClick={() => { setShowRoleModal(false); setSelectedMember(null); }}
                              disabled={changingRole}
                              className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-colors disabled:opacity-50"
                            >
                              Cancel
                            </button>
                            <button
                              onClick={handleChangeRole}
                              disabled={changingRole || newRole === selectedMember.role}
                              className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold bg-primary text-on-primary hover:bg-primary/90 transition-all shadow-sm shadow-primary/20 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                            >
                              {changingRole ? (
                                <>
                                  <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                                  </svg>
                                  Updating...
                                </>
                              ) : (
                                "Update Role"
                              )}
                            </button>
                          </div>
                        </motion.div>
                      </div>
                    )}

                    {/* Transfer Ownership Modal */}
                    {showTransferModal && (
                      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
                        <motion.div
                          initial={reduceMotion ? undefined : { opacity: 0, scale: 0.95 }}
                          animate={{ opacity: 1, scale: 1 }}
                          className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-lg mx-4 p-6"
                        >
                          <div className="flex items-center gap-3 mb-6">
                            <div className="w-12 h-12 rounded-xl bg-amber-50 flex items-center justify-center">
                              <span className="material-symbols-outlined text-amber-500 text-[24px]">admin_panel_settings</span>
                            </div>
                            <div>
                              <h3 className="text-body-lg font-bold text-on-surface">Transfer Ownership</h3>
                              <p className="text-label-sm text-on-surface-variant">Select a Manager to become the new Owner</p>
                            </div>
                          </div>

                          <div className="mb-4 p-4 rounded-xl bg-amber-50 border border-amber-200/50">
                            <div className="flex items-start gap-2">
                              <span className="material-symbols-outlined text-amber-600 text-[18px] mt-0.5">warning</span>
                              <div className="text-body-sm text-amber-800">
                                <p className="font-semibold mb-1">Important:</p>
                                <ul className="space-y-1 text-amber-700">
                                  <li>• You will become a Manager after transfer</li>
                                  <li>• The new Owner will have full access to billing and settings</li>
                                  <li>• This action can be reversed by the new Owner</li>
                                </ul>
                              </div>
                            </div>
                          </div>

                          <div className="mb-6">
                            <label className="text-label-sm font-semibold text-on-surface mb-3 block">Select New Owner (Manager only)</label>
                            <div className="space-y-2 max-h-60 overflow-y-auto">
                              {members.filter(m => m.role === "Manager").length === 0 ? (
                                <div className="text-center py-8">
                                  <span className="material-symbols-outlined text-outline/40 text-4xl mb-2 block">person_off</span>
                                  <p className="text-body-sm text-on-surface-variant">No Manager members found</p>
                                  <p className="text-label-xs text-outline mt-1">You need to have at least one Manager to transfer ownership</p>
                                </div>
                              ) : (
                                members.filter(m => m.role === "Manager").map((member) => (
                                  <button
                                    key={member.id}
                                    onClick={() => setSelectedNewOwner(member)}
                                    className={`w-full flex items-center gap-3 p-4 rounded-xl border-2 transition-all ${
                                      selectedNewOwner?.id === member.id
                                        ? "border-primary bg-primary/5"
                                        : "border-outline-variant/20 hover:border-outline-variant/40 hover:bg-surface-container/50"
                                    }`}
                                  >
                                    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 flex items-center justify-center text-body-sm font-bold text-primary">
                                      {getInitials(member.name)}
                                    </div>
                                    <div className="flex-1 text-left">
                                      <p className="text-body-sm font-semibold text-on-surface">{member.name}</p>
                                      <p className="text-label-xs text-on-surface-variant">{member.email}</p>
                                    </div>
                                    {selectedNewOwner?.id === member.id && (
                                      <span className="material-symbols-outlined text-primary text-[20px]">check_circle</span>
                                    )}
                                  </button>
                                ))
                              )}
                            </div>
                          </div>

                          <div className="flex gap-3">
                            <button
                              onClick={() => { setShowTransferModal(false); setSelectedNewOwner(null); }}
                              disabled={transferring}
                              className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-colors disabled:opacity-50"
                            >
                              Cancel
                            </button>
                            <button
                              onClick={handleTransferOwnership}
                              disabled={transferring || !selectedNewOwner}
                              className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold bg-gradient-to-r from-amber-500 to-amber-600 text-white hover:opacity-90 transition-all shadow-sm shadow-amber-500/20 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                            >
                              {transferring ? (
                                <>
                                  <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                                  </svg>
                                  Transferring...
                                </>
                              ) : (
                                <>
                                  <span className="material-symbols-outlined text-[18px]">check</span>
                                  Confirm Transfer
                                </>
                              )}
                            </button>
                          </div>
                        </motion.div>
                      </div>
                    )}

                    {/* Member Quota Modal */}
                    {showQuotaModal && quotaMember && (
                      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
                        <motion.div
                          initial={reduceMotion ? undefined : { opacity: 0, scale: 0.95 }}
                          animate={{ opacity: 1, scale: 1 }}
                          className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-lg mx-4 p-6"
                        >
                          <div className="flex items-center gap-3 mb-6">
                            <div className="w-12 h-12 rounded-xl bg-purple-50 flex items-center justify-center">
                              <span className="material-symbols-outlined text-purple-500 text-[24px]">data_thresholding</span>
                            </div>
                            <div>
                              <h3 className="text-body-lg font-bold text-on-surface">Assign Quota</h3>
                              <p className="text-label-sm text-on-surface-variant">Set credit quota for {quotaMember.name}</p>
                            </div>
                          </div>

                          <div className="mb-6">
                            <label className="text-label-sm font-semibold text-on-surface mb-3 block">Quota Mode</label>
                            <div className="space-y-2">
                              {[
                                { value: "SharedPool", label: "Shared Pool", icon: "pool", color: "text-blue-600", bg: "bg-blue-50", desc: "Use workspace's shared credit pool. No individual limit." },
                                { value: "LifetimeAssigned", label: "Lifetime Assigned", icon: "lock_clock", color: "text-purple-600", bg: "bg-purple-50", desc: "Fixed credit limit that never resets. Owner must increase when used up." },
                                { value: "MonthlyAssigned", label: "Monthly Assigned", icon: "calendar_month", color: "text-emerald-600", bg: "bg-emerald-50", desc: "Monthly credit limit that resets on the 1st of each month." },
                              ].map((mode) => (
                                <button
                                  key={mode.value}
                                  onClick={() => setQuotaMode(mode.value as typeof quotaMode)}
                                  className={`w-full flex items-start gap-3 p-4 rounded-xl border-2 transition-all ${
                                    quotaMode === mode.value
                                      ? "border-primary bg-primary/5"
                                      : "border-outline-variant/20 hover:border-outline-variant/40 hover:bg-surface-container/50"
                                  }`}
                                >
                                  <div className={`w-10 h-10 rounded-lg ${mode.bg} flex items-center justify-center shrink-0`}>
                                    <span className={`material-symbols-outlined ${mode.color} text-[20px]`}>{mode.icon}</span>
                                  </div>
                                  <div className="flex-1 text-left">
                                    <p className="text-body-sm font-semibold text-on-surface">{mode.label}</p>
                                    <p className="text-label-xs text-on-surface-variant mt-0.5">{mode.desc}</p>
                                  </div>
                                  {quotaMode === mode.value && (
                                    <span className="material-symbols-outlined text-primary text-[20px]">check_circle</span>
                                  )}
                                </button>
                              ))}
                            </div>
                          </div>

                          {quotaMode !== "SharedPool" && (
                            <div className="mb-6">
                              <label className="text-label-sm font-semibold text-on-surface mb-2 block">
                                Credit Limit
                              </label>
                              <div className="relative">
                                <input
                                  type="number"
                                  value={quotaLimit}
                                  onChange={(e) => setQuotaLimit(Number(e.target.value))}
                                  min={100}
                                  step={100}
                                  className="w-full px-4 py-3 pr-16 rounded-xl border border-outline-variant/40 bg-surface-container-lowest text-body-sm text-on-surface focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all"
                                  placeholder="Enter credit limit"
                                />
                                <span className="absolute right-4 top-1/2 -translate-y-1/2 text-label-sm text-on-surface-variant font-medium">credits</span>
                              </div>
                              <p className="text-label-xs text-on-surface-variant mt-2">
                                {quotaMode === "LifetimeAssigned" 
                                  ? "This limit will never reset. Member will be blocked when reached."
                                  : "This limit will reset to 0 on the 1st of each month."}
                              </p>
                            </div>
                          )}

                          <div className="flex gap-3">
                            <button
                              onClick={() => { setShowQuotaModal(false); setQuotaMember(null); }}
                              disabled={savingQuota}
                              className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-colors disabled:opacity-50"
                            >
                              Cancel
                            </button>
                            <button
                              onClick={handleSaveQuota}
                              disabled={savingQuota || (quotaMode !== "SharedPool" && quotaLimit < 100)}
                              className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold bg-purple-600 text-white hover:bg-purple-700 transition-all shadow-sm shadow-purple-500/20 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                            >
                              {savingQuota ? (
                                <>
                                  <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                                  </svg>
                                  Saving...
                                </>
                              ) : (
                                <>
                                  <span className="material-symbols-outlined text-[18px]">check</span>
                                  Save Quota
                                </>
                              )}
                            </button>
                          </div>
                        </motion.div>
                      </div>
                    )}
                  </motion.div>
                )}
              {showInviteModal && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
                  <motion.div
                    initial={reduceMotion ? undefined : { opacity: 0, scale: 0.95 }}
                    animate={{ opacity: 1, scale: 1 }}
                    className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-md mx-4 p-6"
                  >
                    <div className="flex items-center gap-3 mb-6">
                      <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-primary/10 to-primary/5 flex items-center justify-center ring-1 ring-primary/20">
                        <span className="material-symbols-outlined text-primary text-[24px]">person_add</span>
                      </div>
                      <div>
                        <h3 className="text-body-lg font-bold text-on-surface">Invite Member</h3>
                        <p className="text-label-sm text-on-surface-variant">Add a new team member to workspace</p>
                      </div>
                    </div>

                    <div className="space-y-4 mb-6">
                      <div>
                        <label className="text-label-sm font-semibold text-on-surface mb-1.5 block">Email Address</label>
                        <input
                          type="email"
                          placeholder="colleague@company.com"
                          value={inviteForm.email}
                          onChange={(e) => setInviteForm({ ...inviteForm, email: e.target.value })}
                          className="w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest px-4 py-2.5 text-body-sm text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all"
                        />
                      </div>
                      <div>
                        <label className="text-label-sm font-semibold text-on-surface mb-1.5 block">Role</label>
                        <div className="grid grid-cols-2 gap-2">
                          {[
                            { value: "Viewer", label: "Viewer", icon: "visibility", desc: "View only" },
                            { value: "ContentCreator", label: "Creator", icon: "edit_note", desc: "Create & publish" },
                            { value: "Manager", label: "Manager", icon: "manage_accounts", desc: "Manage content" },
                          ].map((r) => (
                            <button
                              key={r.value}
                              type="button"
                              onClick={() => setInviteForm({ ...inviteForm, role: r.value as InvitationRole })}
                              className={`flex flex-col items-center gap-1 p-3 rounded-xl border-2 transition-all ${
                                inviteForm.role === r.value
                                  ? "border-primary bg-primary/5 text-primary"
                                  : "border-outline-variant/30 text-on-surface-variant hover:border-outline-variant/60 hover:bg-surface-container"
                              }`}
                            >
                              <span className={`material-symbols-outlined text-[20px] ${inviteForm.role === r.value ? "text-primary" : ""}`}>{r.icon}</span>
                              <span className="text-label-sm font-medium">{r.label}</span>
                              <span className="text-label-2xs text-on-surface-variant">{r.desc}</span>
                            </button>
                          ))}
                        </div>
                      </div>
                    </div>

                    <div className="flex gap-3">
                      <button
                        onClick={() => { setShowInviteModal(false); setInviteForm({ email: "", role: "Viewer" }); }}
                        disabled={sendingInvite}
                        className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-colors disabled:opacity-50"
                      >
                        Cancel
                      </button>
                      <button
                        onClick={handleSendInvite}
                        disabled={sendingInvite || !inviteForm.email.trim()}
                        className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold bg-primary text-on-primary hover:bg-primary/90 transition-all shadow-sm shadow-primary/20 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                      >
                        {sendingInvite ? (
                          <>
                            <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                            </svg>
                            Sending...
                          </>
                        ) : (
                          <>
                            <span className="material-symbols-outlined text-[18px]">send</span>
                            Send Invite
                          </>
                        )}
                      </button>
                    </div>
                  </motion.div>
                </div>
              )}

              {/* Purchase Credits Confirm Dialog */}
              {showPurchaseConfirm && selectedCreditPack && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
                  <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-md mx-4 p-6">
                    <div className="flex items-center gap-3 mb-6">
                      <div className="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center">
                        <span className="material-symbols-outlined text-primary text-[24px]">shopping_cart</span>
                      </div>
                      <div>
                        <h3 className="text-body-lg font-bold text-on-surface">Confirm Purchase</h3>
                        <p className="text-label-sm text-on-surface-variant">Review your order</p>
                      </div>
                    </div>
                    <div className="space-y-4 mb-6">
                      <div className="flex items-center justify-between p-4 rounded-xl bg-surface-container/50">
                        <span className="text-body-sm font-semibold text-on-surface">{selectedCreditPack.name} Pack</span>
                        <span className="text-body-sm font-bold text-on-surface">{selectedCreditPack.credits.toLocaleString()} credits</span>
                      </div>
                      <div className="flex items-center justify-between p-4 rounded-xl bg-surface-container/50">
                        <span className="text-body-sm text-on-surface-variant">Current Balance</span>
                        <span className="text-body-sm font-semibold text-on-surface">{creditWallet?.balance.toLocaleString() || 0} credits</span>
                      </div>
                      <div className="flex items-center justify-between p-4 rounded-xl bg-emerald-50 border border-emerald-200/30">
                        <span className="text-body-sm font-semibold text-emerald-700">New Balance</span>
                        <span className="text-body-sm font-bold text-emerald-700">{((creditWallet?.balance || 0) + selectedCreditPack.credits).toLocaleString()} credits</span>
                      </div>
                      <div className="pt-4 border-t border-outline-variant/20">
                        <div className="flex items-center justify-between">
                          <span className="text-body-md font-semibold text-on-surface">Total</span>
                          <span className="text-2xl font-bold text-primary">{selectedCreditPack.price}</span>
                        </div>
                      </div>
                    </div>
                    <div className="flex gap-3">
                      <button
                        onClick={() => setShowPurchaseConfirm(false)}
                        disabled={purchasing}
                        className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold border border-outline-variant/30 text-on-surface hover:bg-surface-container transition-colors disabled:opacity-50"
                      >
                        Cancel
                      </button>
                      <button
                        onClick={handlePurchaseCredits}
                        disabled={purchasing}
                        className="flex-1 px-4 py-3 rounded-xl text-body-sm font-semibold bg-gradient-to-r from-primary to-secondary text-white hover:opacity-90 transition-all disabled:opacity-50 flex items-center justify-center gap-2"
                      >
                        {purchasing ? (
                          <>
                            <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                            </svg>
                            Processing...
                          </>
                        ) : "Confirm & Pay"}
                      </button>
                    </div>
                  </div>
                </div>
              )}

              {/* Confirmation Modal */}
              <ConfirmationModal
                isOpen={confirmModal.isOpen}
                onClose={() => setConfirmModal(prev => ({ ...prev, isOpen: false }))}
                onConfirm={confirmModal.onConfirm}
                title={confirmModal.title}
                message={confirmModal.message}
                type={confirmModal.type}
                confirmText={confirmModal.confirmText}
                isLoading={confirmModal.isLoading}
              />
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
