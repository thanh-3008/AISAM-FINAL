"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { motion, useReducedMotion } from "motion/react";
import { getProfileTypeLabel, useProfiles, addProfileToCache } from "@/hooks/useProfiles";
import ProfileSettingsSidebar, { ProfileSection } from "@/components/layout/ProfileSettingsSidebar";
import { apiFetch } from "@/lib/apiClient";
import {
  changePassword,
  getPaymentHistory,
  getCurrentSubscription,
  createCheckout,
  type PaymentHistoryItem,
  type CurrentSubscription,
} from "@/services/profileSettingsService";

interface Profile {
  id: string;
  userId: string;
  name: string;
  profileType: number;
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
  { value: 1, label: "Basic" },
  { value: 2, label: "Pro" },
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
  const [profile, setProfile] = useState<Profile | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [editing, setEditing] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const initialSection = (searchParams.get("section") as ProfileSection) || "my-profile";
  const [activeSection, setActiveSection] = useState<ProfileSection>(
    ["my-profile", "team", "security", "billing", "subscription"].includes(initialSection) ? initialSection : "my-profile"
  );
  const { selectProfile } = useProfiles();
  const reduceMotion = useReducedMotion();

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

  useEffect(() => {
    if (!id) return;
    const fetchProfile = async () => {
      try {
        const result = await apiFetch(`/profiles/${id}`);
        if (result?.success && result.data) {
          const p = result.data as Profile;
          setProfile(p);
          setForm({
            name: p.name,
            profileType: String(p.profileType),
            companyName: p.companyName || "",
            bio: p.bio || "",
            avatarUrl: p.avatarUrl || "",
          });
        } else {
          setError(result?.message || "Profile not found");
        }
      } catch {
        setError("Network error");
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();
  }, [id]);

  const handleSave = async () => {
    if (!form.name.trim()) { setError("Name is required"); return; }
    if (!form.profileType) { setError("Profile type is required"); return; }
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
        setProfile(result.data);
        addProfileToCache(result.data);
        selectProfile(result.data);
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
        alert("Password changed successfully! Please login again.");
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
    try {
      const data = await getPaymentHistory(1, 10);
      if (data) {
        setPaymentHistory(data.data);
      }
    } catch {
      console.error("Failed to load payment history");
    } finally {
      setLoadingPayments(false);
    }
  };

  const handleDownloadInvoice = (invoiceId: string) => {
    // TODO: Implement invoice download when BE API is available
    alert(`Download invoice ${invoiceId} - Feature coming soon!`);
  };

  // Subscription section handlers
  const handleLoadSubscription = async () => {
    setLoadingSubscription(true);
    try {
      const data = await getCurrentSubscription();
      if (data) {
        setSubscription(data);
      }
    } catch {
      console.error("Failed to load subscription");
    } finally {
      setLoadingSubscription(false);
    }
  };

  const handleUpgradePlan = async (planType: number) => {
    setUpgradingPlan(true);
    try {
      const checkout = await createCheckout({
        planType,
        returnUrl: window.location.origin + "/profiles?payment=success",
        cancelUrl: window.location.origin + "/profiles?payment=cancelled",
      });
      if (checkout?.checkoutUrl) {
        window.location.href = checkout.checkoutUrl;
      } else {
        setError("Failed to create checkout");
      }
    } catch {
      setError("Network error while upgrading plan");
    } finally {
      setUpgradingPlan(false);
    }
  };

  const handleCancelPlan = () => {
    if (confirm("Are you sure you want to cancel your subscription? You will lose access to premium features.")) {
      // TODO: Implement cancel subscription when BE API is available
      alert("Cancel subscription feature coming soon!");
    }
  };

  // Team section handlers
  const handleInviteMember = () => {
    // TODO: Implement invite member when BE API is available
    alert("Invite Member feature coming soon!");
  };

  const handleUpdatePayment = () => {
    // TODO: Implement update payment method when BE API is available
    alert("Update Payment Method feature coming soon!");
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
    }
  }, [activeSection, subscription]);

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

  if (error && !profile) {
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
                  Back to Profiles
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
  const planLabel = profile ? getProfileTypeLabel(profile.profileType) : "";
  const initials = profile ? getInitials(profile.name) : "?";
  const statusInfo = profile ? statusConfig[profile.status] || statusConfig[0] : statusConfig[0];

  return (
    <div className="min-h-[100dvh] bg-surface flex">
      <div className="flex-1 flex flex-col">
        <div className="flex-1 flex overflow-hidden">
          <ProfileSettingsSidebar
            activeSection={activeSection}
            onSectionChange={setActiveSection}
            profileName={profile?.name}
            profileInitials={initials}
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
                {/* ===== MY PROFILE ===== */}
                {activeSection === "my-profile" && (
                  <motion.div variants={reduceMotion ? undefined : container} initial={reduceMotion ? undefined : "hidden"} animate="show">
                    {editing ? (
                      <div className="space-y-6">
                        <motion.div variants={reduceMotion ? undefined : item}>
                          <h2 className="text-2xl font-bold text-on-surface tracking-tight">Edit Profile</h2>
                          <p className="text-body-sm text-on-surface-variant mt-1.5">Update your business profile information below</p>
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
                            <h3 className="text-xl text-on-surface font-bold">{profile?.name}</h3>
                            <p className="text-label-sm text-on-surface-variant mt-0.5">{planLabel}</p>
                          </div>
                        </motion.div>

                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                          <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-6 shadow-sm space-y-5">
                            <div className="flex items-center gap-2.5 mb-2">
                              <div className="w-9 h-9 rounded-xl bg-primary/5 flex items-center justify-center">
                                <span className="material-symbols-outlined text-primary text-[18px]">business</span>
                              </div>
                              <h3 className="text-body-lg font-semibold text-on-surface">Business</h3>
                            </div>
                            <div className="space-y-1.5">
                              <label className={labelClass}>Name <span className="text-red-500">*</span></label>
                              <input className={inputClass} value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} />
                            </div>
                            <div className="space-y-1.5">
                              <label className={labelClass}>Plan <span className="text-red-500">*</span></label>
                              <div className="relative">
                                <select className={`${inputClass} appearance-none pr-10`} value={form.profileType} onChange={e => setForm(f => ({ ...f, profileType: e.target.value }))}>
                                  <option value="" disabled>Select plan</option>
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
                              <label className={labelClass}>Bio</label>
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
                            <h2 className="text-2xl font-bold text-on-surface tracking-tight">My Profile</h2>
                            <p className="text-body-sm text-on-surface-variant mt-1.5">Manage your profile information</p>
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
                            <h3 className="text-xl text-on-surface font-bold">{profile?.name}</h3>
                            <div className="flex items-center justify-center sm:justify-start flex-wrap gap-x-3 gap-y-1.5 mt-2">
                              <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-xs font-medium border ${statusInfo.class}`}>
                                <span className={`w-1.5 h-1.5 rounded-full ${statusInfo.dot} ${profile?.status === 1 ? "animate-pulse" : ""}`} />
                                {statusInfo.label}
                              </span>
                              <span className="text-label-sm text-outline">{planLabel}</span>
                              {profile?.isOwner && <span className="text-label-sm text-amber-600 font-semibold flex items-center gap-1"><span className="material-symbols-outlined text-[14px]">star</span>Owner</span>}
                            </div>
                            {profile?.companyName && <p className="text-body-sm text-outline mt-2">{profile.companyName}</p>}
                          </div>
                        </motion.div>

                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                          <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 p-6 shadow-sm">
                            <div className="flex items-center gap-2.5 mb-5">
                              <div className="w-9 h-9 rounded-xl bg-primary/5 flex items-center justify-center">
                                <span className="material-symbols-outlined text-primary text-[18px]">business</span>
                              </div>
                              <h3 className="text-body-lg font-semibold text-on-surface">Business</h3>
                            </div>
                            <dl className="space-y-2">
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl bg-surface-container/40">
                                <dt className="text-label-sm text-outline">Name</dt>
                                <dd className="text-body-sm text-on-surface font-medium">{profile?.name}</dd>
                              </div>
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                                <dt className="text-label-sm text-outline">Plan</dt>
                                <dd className="text-body-sm text-on-surface font-medium">{planLabel}</dd>
                              </div>
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                                <dt className="text-label-sm text-outline">Company</dt>
                                <dd className="text-body-sm text-on-surface">{profile?.companyName || "—"}</dd>
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
                                <dt className="text-label-sm text-outline">Bio</dt>
                                <dd className="text-body-sm text-on-surface">{profile?.bio || "—"}</dd>
                              </div>
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                                <dt className="text-label-sm text-outline">Avatar</dt>
                                <dd className="text-body-sm text-on-surface break-all">{profile?.avatarUrl || "—"}</dd>
                              </div>
                              <div className="grid grid-cols-[110px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                                <dt className="text-label-sm text-outline">Created</dt>
                                <dd className="text-body-sm text-on-surface">{profile ? new Date(profile.createdAt).toLocaleDateString() : "—"}</dd>
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
                            Delete Profile
                          </motion.button>
                        </motion.div>
                      </div>
                    )}
                  </motion.div>
                )}

                {/* ===== TEAM ===== */}
                {activeSection === "team" && (
                  <motion.div variants={reduceMotion ? undefined : container} initial={reduceMotion ? undefined : "hidden"} animate="show" className="space-y-6">
                    <motion.div variants={reduceMotion ? undefined : item} className="flex items-center justify-between">
                      <div>
                        <h2 className="text-2xl font-bold text-on-surface tracking-tight">Team Members</h2>
                        <p className="text-body-sm text-on-surface-variant mt-1.5">Manage your team members and their permissions</p>
                      </div>
                      <motion.button
                        whileTap={reduceMotion ? undefined : { scale: 0.97 }}
                        onClick={handleInviteMember}
                        className="px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:bg-primary/90 transition-all shadow-sm shadow-primary/20 inline-flex items-center gap-2"
                      >
                        <span className="material-symbols-outlined text-[18px]">person_add</span>
                        Invite Member
                      </motion.button>
                    </motion.div>

                    {/* Team Stats */}
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                      {[
                        { label: "Total Members", value: "1", icon: "group", color: "text-primary", bg: "bg-primary/5" },
                        { label: "Active", value: "1", icon: "check_circle", color: "text-emerald-600", bg: "bg-emerald-50" },
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

                    {/* Team Members List */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/15 shadow-sm overflow-hidden">
                      <div className="px-6 py-4 border-b border-outline-variant/10 bg-surface-container/30">
                        <h3 className="text-body-md font-semibold text-on-surface">Members</h3>
                      </div>
                      <div className="divide-y divide-outline-variant/10">
                        {profile && (
                          <div className="px-6 py-4 flex items-center gap-4 hover:bg-surface-container/30 transition-colors">
                            <div className="w-12 h-12 rounded-full bg-gradient-to-br from-primary to-primary-container flex items-center justify-center text-on-primary font-bold text-body-md">
                              {getInitials(profile.name)}
                            </div>
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center gap-2">
                                <p className="text-body-sm font-semibold text-on-surface truncate">{profile.name}</p>
                                {profile.isOwner && (
                                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-amber-50 text-amber-700 text-label-xs font-medium border border-amber-200/30">
                                    <span className="material-symbols-outlined text-[12px]">star</span>
                                    Owner
                                  </span>
                                )}
                              </div>
                              <p className="text-label-sm text-on-surface-variant mt-0.5">
                                {profile.isOwner ? "Full access to all features" : profile.memberRole || "Member"}
                              </p>
                            </div>
                            <div className="flex items-center gap-2">
                              <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-700 text-label-xs font-medium border border-emerald-200/50">
                                <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
                                Active
                              </span>
                              {profile.isOwner && (
                                <button className="p-2 rounded-lg hover:bg-surface-container transition-colors" title="Edit member">
                                  <span className="material-symbols-outlined text-[18px] text-outline">more_vert</span>
                                </button>
                              )}
                            </div>
                          </div>
                        )}
                      </div>
                    </motion.div>

                    {/* Permissions Info */}
                    <motion.div variants={reduceMotion ? undefined : item} className="bg-gradient-to-br from-primary/5 to-secondary/5 rounded-2xl border border-primary/10 p-6">
                      <div className="flex items-start gap-4">
                        <div className="w-12 h-12 rounded-xl bg-white/80 flex items-center justify-center shrink-0">
                          <span className="material-symbols-outlined text-primary text-[24px]">admin_panel_settings</span>
                        </div>
                        <div className="flex-1">
                          <h4 className="text-body-md font-semibold text-on-surface mb-1">Role Permissions</h4>
                          <p className="text-body-sm text-on-surface-variant mb-4">
                            As the owner, you have full control over this profile including billing, team management, and all settings.
                          </p>
                          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                            {[
                              "Manage team members",
                              "Edit profile settings",
                              "Access billing & payments",
                              "View all analytics",
                            ].map((permission) => (
                              <div key={permission} className="flex items-center gap-2">
                                <span className="material-symbols-outlined text-emerald-600 text-[16px]">check_circle</span>
                                <span className="text-label-sm text-on-surface">{permission}</span>
                              </div>
                            ))}
                          </div>
                        </div>
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
                        <h2 className="text-2xl font-bold text-on-surface tracking-tight">Billing & Usage</h2>
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

                    {/* Usage Quotas */}
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                      {[
                        { label: "API Calls", used: "8,452", total: "10,000", pct: 85, icon: "api", color: "text-primary", bg: "bg-primary/5", bar: "bg-gradient-to-r from-primary to-primary-container", warning: true },
                        { label: "Storage", used: "2.4 GB", total: "5 GB", pct: 48, icon: "storage", color: "text-secondary", bg: "bg-secondary/5", bar: "bg-gradient-to-r from-secondary to-secondary-container", warning: false },
                        { label: "Team Members", used: "1", total: "5", pct: 20, icon: "group", color: "text-emerald-600", bg: "bg-emerald-50", bar: "bg-gradient-to-r from-emerald-500 to-emerald-400", warning: false },
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
                                  <p className="text-body-sm font-semibold text-on-surface">
                                    {new Date(invoice.createdAt).toLocaleDateString("en-US", { month: "short", year: "numeric" })}
                                  </p>
                                  <p className="text-label-sm text-on-surface-variant">{invoice.description || invoice.paymentMethod}</p>
                                </div>
                              </div>
                              <div className="flex items-center gap-4">
                                <p className="text-body-sm font-semibold text-on-surface">
                                  {new Intl.NumberFormat("en-US", { style: "currency", currency: invoice.currency || "USD" }).format(invoice.amount)}
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
                  </motion.div>
                )}

                {/* ===== SUBSCRIPTION ===== */}
                {activeSection === "subscription" && (
                  <motion.div variants={reduceMotion ? undefined : container} initial={reduceMotion ? undefined : "hidden"} animate="show" className="space-y-6">
                    <motion.div variants={reduceMotion ? undefined : item}>
                      <h2 className="text-2xl font-bold text-on-surface tracking-tight">Subscription</h2>
                      <p className="text-body-sm text-on-surface-variant mt-1.5">Manage your plan and billing details</p>
                    </motion.div>

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
                                      <span className={`w-1.5 h-1.5 rounded-full ${statusInfo.dot} ${profile?.status === 1 ? "animate-pulse" : ""}`} />
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
                                  {subscription?.autoRenew ? "Renews automatically" : "Manual renewal"}
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
                                  {subscription?.amount 
                                    ? `${subscription.currency} ${subscription.amount.toFixed(2)}`
                                    : planLabel === "Free" ? "Free" : "$29.00 USD"}
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
                      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        {[
                          {
                            name: "Free",
                            planType: 0,
                            price: "$0",
                            period: "/month",
                            current: planLabel === "Free",
                            features: ["1 Profile", "100 API Calls/month", "1 GB Storage", "Community Support"],
                            cta: "Current Plan",
                          },
                          {
                            name: "Basic",
                            planType: 1,
                            price: "$29",
                            period: "/month",
                            current: planLabel === "Basic",
                            popular: true,
                            features: ["5 Profiles", "10,000 API Calls/month", "5 GB Storage", "5 Team Members", "Email Support"],
                            cta: planLabel === "Basic" ? "Current Plan" : "Upgrade to Basic",
                          },
                          {
                            name: "Pro",
                            planType: 2,
                            price: "$79",
                            period: "/month",
                            current: planLabel === "Pro",
                            features: ["Unlimited Profiles", "Unlimited API Calls", "50 GB Storage", "Unlimited Team Members", "Priority Support", "Advanced Analytics"],
                            cta: planLabel === "Pro" ? "Current Plan" : "Upgrade to Pro",
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
                        <h3 className="text-body-lg text-on-surface font-semibold">Delete Profile</h3>
                        <p className="text-body-sm text-on-surface-variant">This action cannot be undone</p>
                      </div>
                    </div>
                    <p className="text-body-sm text-on-surface-variant mb-6">
                      Are you sure you want to delete <span className="font-semibold text-on-surface">{profile?.name}</span>? All associated data will be permanently removed.
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
                </motion.div>
              )}
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
