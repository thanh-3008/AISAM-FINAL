"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { motion } from "motion/react";
import { getUserIdFromToken } from "@/lib/auth";
import { useWorkspaces, addWorkspaceToCache, WorkspaceData } from "@/hooks/useWorkspaces";
import { useToast } from "@/contexts/ToastContext";
import { apiFetch } from "@/lib/apiClient";

const BUSINESS_WORKSPACE_TYPE = 2;

const BUSINESS_WORKSPACE = {
  value: BUSINESS_WORKSPACE_TYPE,
  label: "Business",
  icon: "business",
  color: "text-purple-500",
  bg: "bg-purple-50",
  ring: "ring-purple-500/20",
  features: [
    "For teams & companies",
    "Requires Business Plus or higher",
    "Team collaboration",
    "Shared workspace & credits",
  ],
};

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
  const { selectWorkspace } = useWorkspaces();
  const { addToast } = useToast();

  const [form, setForm] = useState({
    name: "",
    profileType: String(BUSINESS_WORKSPACE_TYPE),
    companyName: "",
    bio: "",
    avatarUrl: "",
  });

  const updateField = (field: string, value: string) => {
    setForm(prev => ({ ...prev, [field]: value }));
    if (error) setError(null);
  };

  const resetForm = () => {
    setForm({ name: "", profileType: String(BUSINESS_WORKSPACE_TYPE), companyName: "", bio: "", avatarUrl: "" });
    setError(null);
  };

  const handleClose = () => {
    resetForm();
    onClose();
  };

  const avatarPreview = form.avatarUrl && isValidUrl(form.avatarUrl) ? form.avatarUrl : null;
  const selectedType = BUSINESS_WORKSPACE;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim()) { setError("Workspace name is required"); return; }

    setLoading(true);
    setError(null);

    const userId = getUserIdFromToken();
    if (!userId) {
      setError("Authentication required");
      setLoading(false);
      return;
    }

    try {
      const workspaceType = Number(form.profileType);
      const result = await apiFetch("/workspaces", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name: form.name.trim(),
          workspaceType,
        }),
      });

      if (result?.success && result.data) {
        const w = result.data;
        const wsData: WorkspaceData = {
          id: String(w.id),
          userId,
          name: String(w.name || form.name.trim()),
          workspaceType: typeof w.workspaceType === "number" ? w.workspaceType : workspaceType,
          plan: (typeof w.workspaceType === "number" ? w.workspaceType : workspaceType) === 2 ? "Business" : "Personal",
          status: typeof w.status === "number" ? w.status : 1,
          createdAt: String(w.createdAt || new Date().toISOString()),
          updatedAt: String(w.updatedAt || new Date().toISOString()),
          isOwner: w.currentUserRole === 0 || w.currentUserRole === 1 || typeof w.currentUserRole !== "number",
          memberRole: "Owner",
        };
        addWorkspaceToCache(wsData);
        selectWorkspace(wsData);
        handleClose();
        addToast("Workspace created successfully", "check");
        router.push("/pricing?category=business");
      } else {
        setError(result?.message || "Failed to create workspace");
      }
    } catch (err: any) {
      setError(err.message || "Network error");
    } finally {
      setLoading(false);
    }
  };

  if (!open) return null;

  const inputClass =
    "w-full rounded-xl border border-outline-variant/40 bg-surface-container-lowest px-4 py-2.5 text-body-sm text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-2 focus:ring-primary/10 outline-none transition-all";

  const labelClass = "text-label-sm font-semibold text-on-surface";

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center bg-black/50 backdrop-blur-sm animate-in fade-in duration-150 pt-[5vh] overflow-y-auto pb-8">
      <motion.div
        initial={{ opacity: 0, y: 20, scale: 0.98 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        transition={{ duration: 0.3, ease: [0.16, 1, 0.3, 1] }}
        className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-2xl mx-4"
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-outline-variant/10">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-primary/10 to-primary/5 flex items-center justify-center ring-1 ring-primary/20">
              <span className="material-symbols-outlined text-primary text-[20px]">add_circle</span>
            </div>
            <div>
              <h2 className="text-body-lg text-on-surface font-bold">Create Workspace</h2>
              <p className="text-label-xs text-on-surface-variant">Set up your new workspace</p>
            </div>
          </div>
          <button onClick={handleClose} className="w-9 h-9 rounded-xl hover:bg-surface-container flex items-center justify-center transition-colors">
            <span className="material-symbols-outlined text-on-surface-variant text-[20px]">close</span>
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {error && (
            <div className="flex items-center gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-body-sm text-red-800">
              <span className="material-symbols-outlined text-red-500 text-[18px]">error</span>
              <span className="flex-1">{error}</span>
              <button onClick={() => setError(null)} className="text-red-400 hover:text-red-600">
                <span className="material-symbols-outlined text-[16px]">close</span>
              </button>
            </div>
          )}

          {/* Workspace Type Selection */}
          <div className="space-y-3">
            <label className={labelClass}>Workspace Type</label>
            <div className="relative flex flex-col p-5 rounded-xl border-2 border-primary bg-purple-50 text-left shadow-md">
              <div className="absolute top-3 right-3">
                <span className="material-symbols-outlined text-primary text-[20px]">check_circle</span>
              </div>
              <div className={`w-10 h-10 rounded-xl ${BUSINESS_WORKSPACE.bg} flex items-center justify-center mb-3 ring-1 ${BUSINESS_WORKSPACE.ring}`}>
                <span className={`material-symbols-outlined ${BUSINESS_WORKSPACE.color} text-[22px]`}>{BUSINESS_WORKSPACE.icon}</span>
              </div>
              <h3 className="text-body-md font-bold text-on-surface mb-2">{BUSINESS_WORKSPACE.label}</h3>
              <p className="text-label-xs text-on-surface-variant mb-3">
                Personal workspace is created automatically for each account. New workspaces are Business workspaces and require a paid plan.
              </p>
              <ul className="space-y-1.5">
                {BUSINESS_WORKSPACE.features.map((f) => (
                  <li key={f} className="flex items-center gap-1.5 text-label-xs text-on-surface-variant">
                    <span className="material-symbols-outlined text-emerald-500 text-[14px]">check</span>
                    {f}
                  </li>
                ))}
              </ul>
            </div>
          </div>

          {/* Name and Company */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <label className={labelClass}>Workspace Name <span className="text-red-500">*</span></label>
              <input
                className={inputClass}
                placeholder="e.g. My Workspace"
                value={form.name}
                onChange={e => updateField("name", e.target.value)}
              />
            </div>
            <div className="space-y-1.5">
              <label className={labelClass}>Company <span className="text-outline/50 text-label-xs">(optional)</span></label>
              <input
                className={inputClass}
                placeholder="e.g. AISAM"
                value={form.companyName}
                onChange={e => updateField("companyName", e.target.value)}
              />
            </div>
          </div>

          {/* Bio */}
          <div className="space-y-1.5">
            <label className={labelClass}>Description <span className="text-outline/50 text-label-xs">(optional)</span></label>
            <textarea
              className={`${inputClass} resize-none min-h-[80px]`}
              rows={3}
              placeholder="Tell us about your workspace..."
              value={form.bio}
              onChange={e => updateField("bio", e.target.value)}
            />
          </div>

          {/* Avatar URL */}
          <div className="space-y-1.5">
            <label className={labelClass}>Avatar URL <span className="text-outline/50 text-label-xs">(optional)</span></label>
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 rounded-xl border border-outline-variant/20 flex items-center justify-center overflow-hidden bg-surface-container shrink-0">
                {avatarPreview ? (
                  <img src={avatarPreview} alt="Avatar" className="w-full h-full object-cover" />
                ) : form.name ? (
                  <span className="text-body-sm font-bold text-primary/40">{getInitials(form.name)}</span>
                ) : (
                  <span className="material-symbols-outlined text-outline/40 text-[20px]">workspaces</span>
                )}
              </div>
              <input
                className={inputClass}
                placeholder="https://example.com/avatar.png"
                type="url"
                value={form.avatarUrl}
                onChange={e => updateField("avatarUrl", e.target.value)}
              />
            </div>
          </div>

          {/* Preview */}
          {form.name && selectedType && (
            <div className="bg-surface-container/30 rounded-xl p-4 border border-outline-variant/10">
              <p className="text-label-xs text-on-surface-variant font-medium mb-2">Preview</p>
              <div className="flex items-center gap-3">
                <div className={`w-10 h-10 rounded-xl ${selectedType.bg} flex items-center justify-center ring-1 ${selectedType.ring}`}>
                  {avatarPreview ? (
                    <img src={avatarPreview} alt="Avatar" className="w-full h-full object-cover rounded-xl" />
                  ) : (
                    <span className="text-body-sm font-bold text-primary/60">{getInitials(form.name)}</span>
                  )}
                </div>
                <div>
                  <p className="text-body-sm font-semibold text-on-surface">{form.name}</p>
                  <p className="text-label-xs text-on-surface-variant">{selectedType.label} Workspace</p>
                </div>
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={handleClose}
              className="px-5 py-2.5 border border-outline-variant/30 text-on-surface rounded-xl font-semibold text-body-sm hover:bg-surface-container transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading || !form.name.trim()}
              className="px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-body-sm hover:bg-primary/90 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 shadow-sm shadow-primary/20"
            >
              {loading ? (
                <>
                  <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                  </svg>
                  Creating...
                </>
              ) : (
                <>
                  <span className="material-symbols-outlined text-[18px]">add</span>
                  Create Business Workspace
                </>
              )}
            </button>
          </div>
        </form>
      </motion.div>
    </div>
  );
}
