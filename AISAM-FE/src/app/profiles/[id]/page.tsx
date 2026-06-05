"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { getProfileTypeLabel, useProfiles, addProfileToCache } from "@/hooks/useProfiles";
import ProfileSettingsSidebar, { ProfileSection } from "@/components/layout/ProfileSettingsSidebar";
import { apiFetch } from "@/lib/apiClient";

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
  "w-full rounded-xl border border-outline-variant/60 bg-surface-container-lowest px-4 py-2.5 text-body-md text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all";

const labelClass = "text-label-sm font-semibold text-on-surface";

const statusConfig: Record<number, { label: string; class: string }> = {
  0: { label: "Pending", class: "bg-amber-50 text-amber-600" },
  1: { label: "Active", class: "bg-emerald-50 text-emerald-600" },
  2: { label: "Suspended", class: "bg-red-50 text-red-500" },
  3: { label: "Cancelled", class: "bg-surface-container-high text-on-surface-variant" },
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
  const [activeSection, setActiveSection] = useState<ProfileSection>(
    (searchParams.get("section") as ProfileSection) || "my-profile"
  );
  const { selectProfile } = useProfiles();

  const [form, setForm] = useState({ name: "", profileType: "", companyName: "", bio: "", avatarUrl: "" });

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

  useEffect(() => {
    const section = searchParams.get("section") as ProfileSection | null;
    if (section && ["my-profile", "team", "security", "billing", "subscription"].includes(section)) {
      setActiveSection(section);
    }
  }, [searchParams]);

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
    } catch (err: any) {
      setError(err.message || "Delete failed");
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-surface flex">
        <div className="flex-1 flex flex-col">
          <div className="flex-1 flex overflow-hidden">
            <div className="w-64 shrink-0 border-r border-outline-variant/30 bg-surface-container-low/50 p-5 space-y-2">
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
      <div className="min-h-screen bg-surface flex">
        <div className="flex-1 flex flex-col">
          <div className="flex-1 flex overflow-hidden">
            <div className="w-64 shrink-0 border-r border-outline-variant/30 bg-surface-container-low/50" />
            <main className="flex-1 flex items-center justify-center">
              <div className="text-center space-y-4">
                <div className="w-14 h-14 mx-auto rounded-2xl bg-error-container/30 flex items-center justify-center">
                  <span className="material-symbols-outlined text-danger-red text-3xl">error_outline</span>
                </div>
                <p className="text-body-md text-danger-red font-semibold">{error}</p>
                <button onClick={() => router.push("/profiles")} className="px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:opacity-90 transition-all shadow-sm">
                  Back to Profiles
                </button>
              </div>
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
    <div className="min-h-screen bg-surface flex">
      <div className="flex-1 flex flex-col">
        <div className="flex-1 flex overflow-hidden">
          <ProfileSettingsSidebar
            activeSection={activeSection}
            onSectionChange={setActiveSection}
            profileName={profile?.name}
            profileInitials={initials}
          />

          <main className="flex-1 overflow-auto">
            <div className="p-8 space-y-8 animate-in fade-in duration-300">
              {error && (
                <div className="flex items-center gap-3 rounded-xl border border-danger-red/20 bg-error-container/50 px-5 py-4 text-body-sm text-on-error-container">
                  <span className="material-symbols-outlined text-error text-[20px]">error</span>
                  <span className="flex-1">{error}</span>
                  <button onClick={() => setError(null)} className="text-on-error-container/50 hover:text-on-error-container">
                    <span className="material-symbols-outlined text-[18px]">close</span>
                  </button>
                </div>
              )}

              <div key={activeSection} className="animate-in fade-in slide-in-from-right-2 duration-200">
              {/* ===== MY PROFILE ===== */}
              {activeSection === "my-profile" && (
                <div className="space-y-6">
                  {editing ? (
                    <>
                      <div>
                        <h2 className="text-headline-sm text-on-surface">Edit Profile</h2>
                        <p className="text-body-sm text-on-surface-variant mt-1">Update your business profile information below</p>
                      </div>

                      {/* Profile Summary */}
                      <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm flex flex-col sm:flex-row items-center gap-5">
                        <div className="w-20 h-20 rounded-2xl flex items-center justify-center overflow-hidden bg-gradient-to-br from-primary/10 to-primary/5 shrink-0">
                          {avatarPreview ? (
                            <img src={avatarPreview} alt="Avatar" className="w-full h-full object-cover" />
                          ) : (
                            <span className="text-headline-lg-mobile text-primary/40">{initials}</span>
                          )}
                        </div>
                        <div className="text-center sm:text-left min-w-0 flex-1">
                          <h3 className="text-headline-sm text-on-surface font-bold">{profile?.name}</h3>
                          <p className="text-label-sm text-on-surface-variant mt-0.5">{planLabel}</p>
                        </div>
                      </div>

                      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm space-y-5">
                          <div className="flex items-center gap-2.5 mb-2">
                            <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary/10 to-primary/5 flex items-center justify-center">
                              <span className="material-symbols-outlined text-primary text-[18px]">business</span>
                            </div>
                            <h3 className="text-headline-sm font-semibold text-on-surface">Business</h3>
                          </div>
                          <div className="space-y-1.5">
                            <label className={labelClass}>Name <span className="text-danger-red">*</span></label>
                            <input className={inputClass} value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} />
                          </div>
                          <div className="space-y-1.5">
                            <label className={labelClass}>Plan <span className="text-danger-red">*</span></label>
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
                        </div>
                        <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm space-y-5">
                          <div className="flex items-center gap-2.5 mb-2">
                            <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-secondary/10 to-secondary/5 flex items-center justify-center">
                              <span className="material-symbols-outlined text-secondary text-[18px]">info</span>
                            </div>
                            <h3 className="text-headline-sm font-semibold text-on-surface">Details</h3>
                          </div>
                          <div className="space-y-1.5">
                            <label className={labelClass}>Bio</label>
                            <textarea className={`${inputClass} resize-none min-h-[100px]`} rows={4} value={form.bio} onChange={e => setForm(f => ({ ...f, bio: e.target.value }))} />
                          </div>
                          <div className="space-y-1.5">
                            <label className={labelClass}>Avatar URL</label>
                            <input className={inputClass} placeholder="https://example.com/avatar.png" type="url" value={form.avatarUrl} onChange={e => setForm(f => ({ ...f, avatarUrl: e.target.value }))} />
                          </div>
                        </div>
                      </div>
                      <div className="flex justify-end gap-3">
                        <button onClick={() => setEditing(false)} className="px-5 py-2.5 border-2 border-on-surface/15 text-on-surface rounded-xl font-semibold text-body-sm hover:bg-surface-container transition-colors">Cancel</button>
                        <button onClick={handleSave} disabled={saving} className="px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:opacity-90 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 shadow-sm">
                          {saving ? (
                            <><svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" /></svg> Saving...</>
                          ) : "Save"}
                        </button>
                      </div>
                    </>
                  ) : (
                    <>
                      <div className="flex items-center justify-between">
                        <div>
                          <h2 className="text-headline-sm text-on-surface">My Profile</h2>
                          <p className="text-body-sm text-on-surface-variant mt-1">Manage your profile information</p>
                        </div>
                        <button onClick={() => setEditing(true)} className="inline-flex items-center gap-1.5 px-5 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-semibold hover:opacity-90 transition-all shadow-sm">
                          <span className="material-symbols-outlined text-[16px]">edit</span>
                          Edit
                        </button>
                      </div>

                      <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm flex flex-col sm:flex-row items-center gap-5">
                        <div className="w-20 h-20 rounded-2xl flex items-center justify-center overflow-hidden bg-gradient-to-br from-primary/10 to-primary/5 shrink-0">
                          {avatarPreview ? (
                            <img src={avatarPreview} alt="Avatar" className="w-full h-full object-cover" />
                          ) : (
                            <span className="text-headline-lg-mobile text-primary/40">{initials}</span>
                          )}
                        </div>
                        <div className="text-center sm:text-left min-w-0 flex-1">
                          <h3 className="text-headline-sm text-on-surface font-bold">{profile?.name}</h3>
                          <div className="flex items-center justify-center sm:justify-start flex-wrap gap-x-3 gap-y-1 mt-1">
                            <span className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-label-sm font-semibold ${statusInfo.class}`}>
                              <span className={`w-1.5 h-1.5 rounded-full ${profile?.status === 1 ? "bg-success-green animate-pulse" : "bg-current"}`} />
                              {statusInfo.label}
                            </span>
                            <span className="text-label-sm text-outline">{planLabel}</span>
                            {profile?.isOwner && <span className="text-label-sm text-primary font-semibold">Owner</span>}
                          </div>
                          {profile?.companyName && <p className="text-body-sm text-outline mt-2">{profile.companyName}</p>}
                        </div>
                      </div>

                      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm">
                          <div className="flex items-center gap-2.5 mb-6">
                            <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary/10 to-primary/5 flex items-center justify-center">
                              <span className="material-symbols-outlined text-primary text-[18px]">business</span>
                            </div>
                            <h3 className="text-headline-sm font-semibold text-on-surface">Business</h3>
                          </div>
                          <dl className="space-y-2">
                            <div className="grid grid-cols-[120px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl bg-surface-container/50">
                              <dt className="text-label-sm text-outline">Name</dt>
                              <dd className="text-body-sm text-on-surface font-medium">{profile?.name}</dd>
                            </div>
                            <div className="grid grid-cols-[120px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                              <dt className="text-label-sm text-outline">Plan</dt>
                              <dd className="text-body-sm text-on-surface font-medium">{planLabel}</dd>
                            </div>
                            <div className="grid grid-cols-[120px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                              <dt className="text-label-sm text-outline">Company</dt>
                              <dd className="text-body-sm text-on-surface">{profile?.companyName || "—"}</dd>
                            </div>
                          </dl>
                        </div>
                        <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm">
                          <div className="flex items-center gap-2.5 mb-6">
                            <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-secondary/10 to-secondary/5 flex items-center justify-center">
                              <span className="material-symbols-outlined text-secondary text-[18px]">info</span>
                            </div>
                            <h3 className="text-headline-sm font-semibold text-on-surface">Details</h3>
                          </div>
                          <dl className="space-y-2">
                            <div className="grid grid-cols-[120px_1fr] items-start py-2.5 px-3 -mx-3 rounded-xl bg-surface-container/50">
                              <dt className="text-label-sm text-outline">Bio</dt>
                              <dd className="text-body-sm text-on-surface">{profile?.bio || "—"}</dd>
                            </div>
                            <div className="grid grid-cols-[120px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                              <dt className="text-label-sm text-outline">Avatar</dt>
                              <dd className="text-body-sm text-on-surface break-all">{profile?.avatarUrl || "—"}</dd>
                            </div>
                            <div className="grid grid-cols-[120px_1fr] items-center py-2.5 px-3 -mx-3 rounded-xl">
                              <dt className="text-label-sm text-outline">Created</dt>
                              <dd className="text-body-sm text-on-surface">{profile ? new Date(profile.createdAt).toLocaleDateString() : "—"}</dd>
                            </div>
                          </dl>
                        </div>
                      </div>

                      <div className="flex justify-end">
                        <button onClick={() => setShowDeleteDialog(true)} className="inline-flex items-center gap-1.5 px-4 py-2 border border-danger-red/30 text-danger-red rounded-xl text-body-sm font-medium hover:bg-danger-red/5 hover:border-danger-red/50 transition-colors">
                          <span className="material-symbols-outlined text-[16px]">delete</span>
                          Delete Profile
                        </button>
                      </div>
                    </>
                  )}
                </div>
              )}

              {/* ===== TEAM ===== */}
              {activeSection === "team" && (
                <div className="space-y-6">
                  <div>
                    <h2 className="text-headline-sm text-on-surface">Team</h2>
                    <p className="text-body-sm text-on-surface-variant mt-1">Manage your team members and permissions</p>
                  </div>
                  <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-12 shadow-sm flex flex-col items-center justify-center text-center gap-4">
                    <div className="w-16 h-16 rounded-2xl bg-secondary/10 flex items-center justify-center">
                      <span className="material-symbols-outlined text-secondary text-3xl">group</span>
                    </div>
                    <div>
                      <h3 className="text-headline-sm text-on-surface font-semibold">No team members yet</h3>
                      <p className="text-body-sm text-on-surface-variant mt-1 max-w-sm">Invite colleagues to collaborate on campaigns and content</p>
                    </div>
                    <button className="px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:opacity-90 transition-all shadow-sm inline-flex items-center gap-2 active:scale-[0.98]">
                      <span className="material-symbols-outlined text-[18px]">person_add</span>
                      Invite Members
                    </button>
                  </div>
                </div>
              )}

              {/* ===== SECURITY ===== */}
              {activeSection === "security" && (
                <div className="space-y-6">
                  <div>
                    <h2 className="text-headline-sm text-on-surface">Security</h2>
                    <p className="text-body-sm text-on-surface-variant mt-1">Manage your password and security settings</p>
                  </div>
                  <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm space-y-6">
                    <div>
                      <div className="flex items-center gap-2.5 mb-4">
                        <div className="w-8 h-8 rounded-xl bg-primary/10 flex items-center justify-center">
                          <span className="material-symbols-outlined text-primary text-[18px]">lock</span>
                        </div>
                        <h3 className="text-headline-sm font-semibold text-on-surface">Change Password</h3>
                      </div>
                      <p className="text-body-sm text-on-surface-variant mb-5">Update your account password to keep your account secure</p>
                      <div className="space-y-4 max-w-sm">
                        <div className="space-y-1.5">
                          <label className={labelClass}>Current Password</label>
                          <input className={inputClass} type="password" placeholder="Enter current password" />
                        </div>
                        <div className="space-y-1.5">
                          <label className={labelClass}>New Password</label>
                          <input className={inputClass} type="password" placeholder="Enter new password" />
                        </div>
                        <div className="space-y-1.5">
                          <label className={labelClass}>Confirm New Password</label>
                          <input className={inputClass} type="password" placeholder="Confirm new password" />
                        </div>
                        <button className="px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:opacity-90 transition-all shadow-sm">
                          Update Password
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {/* ===== BILLING & QUOTA ===== */}
              {activeSection === "billing" && (
                <div className="space-y-6">
                  <div>
                    <h2 className="text-headline-sm text-on-surface">Billing & Quota</h2>
                    <p className="text-body-sm text-on-surface-variant mt-1">View your billing details and usage quotas</p>
                  </div>
                  <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                    {[
                      { label: "API Calls", used: "8,452", total: "10,000", pct: 85, bar: "bg-primary", dot: "bg-primary" },
                      { label: "Storage", used: "2.4 GB", total: "5 GB", pct: 48, bar: "bg-secondary", dot: "bg-secondary" },
                      { label: "Team Members", used: "1", total: "5", pct: 20, bar: "bg-emerald-500", dot: "bg-emerald-500" },
                    ].map((q) => (
                      <div key={q.label} className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-5 shadow-sm">
                        <div className="flex items-center justify-between mb-2">
                          <p className="text-label-sm text-outline">{q.label}</p>
                          <span className={`w-2 h-2 rounded-full ${q.dot}`} />
                        </div>
                        <p className="text-headline-sm text-on-surface font-bold">{q.used} <span className="text-body-sm text-outline font-normal">/ {q.total}</span></p>
                        <div className="mt-3 h-1.5 bg-surface-container rounded-full overflow-hidden">
                          <div className={`h-full rounded-full transition-all duration-500 ${q.bar}`} style={{ width: `${q.pct}%` }} />
                        </div>
                      </div>
                    ))}
                  </div>
                  <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm">
                    <h3 className="text-headline-sm text-on-surface font-semibold mb-4">Billing History</h3>
                    <div className="flex flex-col items-center justify-center py-8 text-center gap-3">
                      <span className="material-symbols-outlined text-outline text-3xl">receipt_long</span>
                      <p className="text-body-sm text-on-surface-variant">No billing records yet</p>
                    </div>
                  </div>
                </div>
              )}

              {/* ===== SUBSCRIPTION ===== */}
              {activeSection === "subscription" && (
                <div className="space-y-6">
                  <div>
                    <h2 className="text-headline-sm text-on-surface">Subscription</h2>
                    <p className="text-body-sm text-on-surface-variant mt-1">Manage your subscription plan and billing</p>
                  </div>
                  <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm">
                    <div className="flex items-start justify-between gap-4">
                      <div className="flex items-center gap-4">
                        <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-primary/10 to-primary/5 flex items-center justify-center">
                          <span className="material-symbols-outlined text-primary text-3xl">workspace_premium</span>
                        </div>
                        <div>
                          <div className="flex items-center gap-2 mb-0.5">
                            <h3 className="text-headline-sm text-on-surface font-bold">{planLabel} Plan</h3>
                            <span className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-label-sm font-semibold ${statusInfo.class}`}>
                              <span className={`w-1.5 h-1.5 rounded-full ${profile?.status === 1 ? "bg-success-green animate-pulse" : "bg-current"}`} />
                              {statusInfo.label}
                            </span>
                          </div>
                          <p className="text-body-sm text-on-surface-variant">Your current subscription plan</p>
                        </div>
                      </div>
                      <button className="px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:opacity-90 transition-all shadow-sm shrink-0">
                        Upgrade Plan
                      </button>
                    </div>
                    <div className="mt-6 grid grid-cols-1 sm:grid-cols-3 gap-4 pt-6 border-t border-outline-variant/20">
                      <div className="p-4 rounded-xl bg-surface-container/50">
                        <p className="text-label-sm text-outline">Billing Cycle</p>
                        <p className="text-body-sm text-on-surface font-semibold mt-1">Monthly</p>
                      </div>
                      <div className="p-4 rounded-xl bg-surface-container/50">
                        <p className="text-label-sm text-outline">Next Payment</p>
                        <p className="text-body-sm text-on-surface font-semibold mt-1">{new Date(Date.now() + 30 * 86400000).toLocaleDateString()}</p>
                      </div>
                      <div className="p-4 rounded-xl bg-surface-container/50">
                        <p className="text-label-sm text-outline">Payment Method</p>
                        <p className="text-body-sm text-on-surface font-semibold mt-1">—</p>
                      </div>
                    </div>
                  </div>
                </div>
              )}
              </div>

            {/* Delete Confirmation Dialog */}
            {showDeleteDialog && (
              <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150">
                <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-xl p-6 w-full max-w-sm mx-4 animate-in fade-in zoom-in-95 duration-200">
                  <div className="flex items-center gap-3 mb-4">
                    <div className="w-10 h-10 rounded-xl bg-danger-red/10 flex items-center justify-center">
                      <span className="material-symbols-outlined text-danger-red text-[22px]">delete</span>
                    </div>
                    <div>
                      <h3 className="text-headline-sm text-on-surface font-semibold">Delete Profile</h3>
                      <p className="text-body-sm text-on-surface-variant">This action cannot be undone</p>
                    </div>
                  </div>
                  <p className="text-body-sm text-on-surface-variant mb-6">
                    Are you sure you want to delete <span className="font-semibold text-on-surface">{profile?.name}</span>? All associated data will be permanently removed.
                  </p>
                  <div className="flex justify-end gap-3">
                    <button onClick={() => setShowDeleteDialog(false)} className="px-5 py-2.5 border-2 border-on-surface/15 text-on-surface rounded-xl font-semibold text-body-sm hover:bg-surface-container transition-colors">Cancel</button>
                    <button onClick={handleDelete} className="px-5 py-2.5 bg-danger-red text-white rounded-xl font-semibold text-body-sm hover:opacity-90 transition-all shadow-sm flex items-center gap-2">
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
          </main>
        </div>
      </div>
    </div>
  );
}
