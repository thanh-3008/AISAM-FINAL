"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { getUserIdFromToken } from "@/lib/auth";
import { useProfiles, addProfileToCache } from "@/hooks/useProfiles";
import { apiFetch } from "@/lib/apiClient";

const PROFILE_TYPES = [
  { value: 0, label: "Free", icon: "person" },
  { value: 1, label: "Basic", icon: "groups" },
  { value: 2, label: "Pro", icon: "workspace_premium" },
];

function getInitials(name: string) {
  return name.split(" ").map(w => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

function isValidUrl(url: string) {
  try { new URL(url); return true; }
  catch { return false; }
}

interface Props {
  open: boolean;
  onClose: () => void;
}

export default function CreateProfileModal({ open, onClose }: Props) {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { selectProfile } = useProfiles();

  const [form, setForm] = useState({
    name: "",
    profileType: "",
    companyName: "",
    bio: "",
    avatarUrl: "",
  });

  const updateField = (field: string, value: string) => {
    setForm(prev => ({ ...prev, [field]: value }));
    if (error) setError(null);
  };

  const resetForm = () => {
    setForm({ name: "", profileType: "", companyName: "", bio: "", avatarUrl: "" });
    setError(null);
  };

  const handleClose = () => {
    resetForm();
    onClose();
  };

  const avatarPreview = form.avatarUrl && isValidUrl(form.avatarUrl) ? form.avatarUrl : null;
  const selectedPlan = PROFILE_TYPES.find(t => t.value === Number(form.profileType));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim()) { setError("Profile name is required"); return; }
    if (!form.profileType) { setError("Please select a plan type"); return; }

    setLoading(true);
    setError(null);

    const userId = getUserIdFromToken();
    if (!userId) {
      setError("Authentication required");
      setLoading(false);
      return;
    }

    try {
      const formBody = new FormData();
      formBody.append("name", form.name.trim());
      formBody.append("profileType", form.profileType);
      if (form.companyName.trim()) formBody.append("companyName", form.companyName.trim());
      if (form.bio.trim()) formBody.append("bio", form.bio.trim());
      if (form.avatarUrl.trim()) formBody.append("avatarUrl", form.avatarUrl.trim());

      const result = await apiFetch(`/profiles/user/${userId}`, {
        method: "POST",
        body: formBody,
      });

      if (result?.success && result.data) {
        addProfileToCache(result.data);
        selectProfile(result.data);
        handleClose();
        router.push("/dashboard");
      } else {
        setError(result?.message || "An unexpected error occurred");
      }
    } catch (err: any) {
      setError(err.message || "Network error");
    } finally {
      setLoading(false);
    }
  };

  if (!open) return null;

  const inputClass =
    "w-full rounded-xl border border-outline-variant/60 bg-surface-container-lowest px-4 py-2.5 text-body-md text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all";

  const labelClass = "text-label-sm font-semibold text-on-surface";

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150 pt-[10vh] overflow-y-auto">
      <div className="bg-surface rounded-2xl border border-outline-variant/20 shadow-xl w-full max-w-xl mx-4 animate-in fade-in zoom-in-95 duration-200">
        {/* Header */}
        <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-outline-variant/20">
          <div>
            <h2 className="text-headline-sm text-on-surface font-bold">Create Profile</h2>
            <p className="text-body-sm text-on-surface-variant mt-0.5">Set up a new business profile</p>
          </div>
          <button onClick={handleClose} className="w-9 h-9 rounded-xl hover:bg-surface-container flex items-center justify-center transition-colors">
            <span className="material-symbols-outlined text-on-surface-variant text-[20px]">close</span>
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {error && (
            <div className="flex items-center gap-3 rounded-xl border border-danger-red/20 bg-error-container/50 px-4 py-3 text-body-sm text-on-error-container">
              <span className="material-symbols-outlined text-error text-[18px]">error</span>
              <span className="flex-1">{error}</span>
              <button onClick={() => setError(null)} className="text-on-error-container/50 hover:text-on-error-container">
                <span className="material-symbols-outlined text-[16px]">close</span>
              </button>
            </div>
          )}

          {/* Avatar + Name preview */}
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 rounded-2xl border-2 border-surface-container-lowest shadow-sm flex items-center justify-center overflow-hidden bg-gradient-to-br from-surface-container to-surface-container-high shrink-0">
              {avatarPreview ? (
                <img src={avatarPreview} alt="Avatar" className="w-full h-full object-cover" />
              ) : form.name ? (
                <span className="text-[22px] font-bold text-primary/40">{getInitials(form.name)}</span>
              ) : (
                <span className="material-symbols-outlined text-outline/40 text-2xl">person</span>
              )}
            </div>
            <div className="min-w-0">
              <h3 className="text-body-md font-bold text-on-surface">{form.name || "Profile Name"}</h3>
              <p className="text-label-sm text-on-surface-variant">{selectedPlan?.label || "Select a plan"}</p>
            </div>
          </div>

          {/* Form grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="space-y-4">
              <div className="space-y-1.5">
                <label className={labelClass}>Name <span className="text-danger-red">*</span></label>
                <input className={inputClass} placeholder="e.g. My Business" value={form.name} onChange={e => updateField("name", e.target.value)} />
              </div>
              <div className="space-y-1.5">
                <label className={labelClass}>Plan <span className="text-danger-red">*</span></label>
                <div className="grid grid-cols-3 gap-1.5">
                  {PROFILE_TYPES.map(pt => {
                    const sel = form.profileType === String(pt.value);
                    return (
                      <button key={pt.value} type="button" onClick={() => updateField("profileType", String(pt.value))}
                        className={`flex flex-col items-center gap-1 p-2.5 rounded-xl border-2 text-center transition-all duration-200 ${
                          sel ? "border-primary bg-primary/5 text-primary shadow-sm scale-[1.02]" : "border-outline-variant/30 text-on-surface-variant hover:border-outline-variant/60 hover:bg-surface-container"
                        }`}
                      >
                        <span className={`material-symbols-outlined text-[20px] transition-all duration-200 ${sel ? "text-primary scale-110" : ""}`}>{pt.icon}</span>
                        <span className="text-label-sm">{pt.label}</span>
                      </button>
                    );
                  })}
                </div>
              </div>
              <div className="space-y-1.5">
                <label className={labelClass}>Company</label>
                <input className={inputClass} placeholder="e.g. AISAM Inc." value={form.companyName} onChange={e => updateField("companyName", e.target.value)} />
              </div>
            </div>
            <div className="space-y-4">
              <div className="space-y-1.5">
                <label className={labelClass}>Bio</label>
                <textarea className={`${inputClass} resize-none min-h-[80px]`} rows={3} placeholder="Tell us about your business..." value={form.bio} onChange={e => updateField("bio", e.target.value)} />
              </div>
              <div className="space-y-1.5">
                <label className={labelClass}>Avatar URL</label>
                <div className="relative">
                  <span className="absolute inset-y-0 left-0 pl-3.5 flex items-center text-outline">
                    <span className="material-symbols-outlined text-[16px]">link</span>
                  </span>
                  <input className={`${inputClass} pl-9`} placeholder="https://example.com/avatar.png" type="url" value={form.avatarUrl} onChange={e => updateField("avatarUrl", e.target.value)} />
                </div>
              </div>
            </div>
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={handleClose} className="px-5 py-2.5 border-2 border-on-surface/15 text-on-surface rounded-xl font-semibold text-body-sm hover:bg-surface-container transition-colors">
              Cancel
            </button>
            <button type="submit" disabled={loading}
              className="px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:opacity-90 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 shadow-sm"
            >
              {loading ? (
                <><svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" /></svg> Creating...</>
              ) : "Create Profile"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
