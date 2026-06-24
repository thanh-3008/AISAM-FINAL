"use client";

import { useState } from "react";
import { updateBrand, type BrandPayload } from "@/services/brandService";

interface Brand {
  id: string;
  userId: string;
  name: string;
  description: string | null;
  logoUrl: string | null;
  slogan: string | null;
  usp: string | null;
  targetAudience: string | null;
  workspaceId: string | null;
  createdAt: string;
  updatedAt: string;
  productsCount: number;
  contentsCount: number;
}

interface Props {
  open: boolean;
  onClose: () => void;
  onSuccess: (brand: Brand) => void;
  brand: Brand;
}

export default function EditBrandModal({ open, onClose, onSuccess, brand }: Props) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [form, setForm] = useState({
    name: brand.name,
    description: brand.description || "",
    logoUrl: brand.logoUrl || "",
    slogan: brand.slogan || "",
    usp: brand.usp || "",
    targetAudience: brand.targetAudience || "",
  });

  const updateField = (field: string, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (error) setError(null);
  };

  const handleClose = () => {
    onClose();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim()) { setError("Brand name is required"); return; }

    setLoading(true);
    setError(null);

    try {
      const body: BrandPayload = { name: form.name.trim() };
      if (form.description.trim()) body.description = form.description.trim();
      if (form.logoUrl.trim()) body.logoUrl = form.logoUrl.trim();
      if (form.slogan.trim()) body.slogan = form.slogan.trim();
      if (form.usp.trim()) body.usp = form.usp.trim();
      if (form.targetAudience.trim()) body.targetAudience = form.targetAudience.trim();

      const updated = await updateBrand(brand.id, body);

      if (updated) {
        onSuccess(updated as Brand);
        onClose();
      } else {
        setError("Failed to save brand");
      }
    } catch (err: any) {
      setError(err?.message || "Failed to save brand");
    } finally {
      setLoading(false);
    }
  };

  if (!open) return null;

  const inputClass =
    "w-full rounded-xl border border-outline-variant/60 bg-surface-container-lowest px-4 py-2.5 text-body-md text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all";

  const labelClass = "text-label-md font-label-md text-on-surface-variant";

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150 overflow-y-auto">
      <div className="bg-surface rounded-2xl border border-outline-variant/20 shadow-xl w-full max-w-lg mx-4 animate-in fade-in zoom-in-95 duration-200">
        <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-outline-variant/20">
          <h3 className="text-headline-sm font-headline-sm text-on-surface">Edit Brand</h3>
          <button onClick={handleClose} className="p-2 hover:bg-surface-container rounded-full transition-colors active:scale-[0.97]">
            <span className="material-symbols-outlined text-on-surface-variant text-[20px]">close</span>
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="flex items-center gap-3 rounded-xl border border-danger-red/20 bg-error-container/50 px-4 py-3 text-body-sm text-on-error-container">
              <span className="material-symbols-outlined text-error text-[18px]">error</span>
              <span className="flex-1">{error}</span>
              <button onClick={() => setError(null)} type="button" className="text-on-error-container/50 hover:text-on-error-container">
                <span className="material-symbols-outlined text-[16px]">close</span>
              </button>
            </div>
          )}

          <div className="flex flex-col gap-1.5">
            <label className={labelClass}>Brand Name <span className="text-danger-red">*</span></label>
            <input className={inputClass} placeholder="e.g. Lumina Tech" value={form.name} onChange={(e) => updateField("name", e.target.value)} />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className={labelClass}>Slogan</label>
            <input className={inputClass} placeholder="e.g. Innovate Your Light" value={form.slogan} onChange={(e) => updateField("slogan", e.target.value)} />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className={labelClass}>Unique Selling Proposition</label>
            <input className={inputClass} placeholder="e.g. Smart lighting that adapts to your lifestyle" value={form.usp} onChange={(e) => updateField("usp", e.target.value)} />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className={labelClass}>Target Audience</label>
            <input className={inputClass} placeholder="e.g. Tech-savvy homeowners" value={form.targetAudience} onChange={(e) => updateField("targetAudience", e.target.value)} />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className={labelClass}>Brand Description</label>
            <textarea className={`${inputClass} resize-none`} placeholder="Briefly describe your brand..." rows={3} value={form.description} onChange={(e) => updateField("description", e.target.value)} />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className={labelClass}>Brand Logo URL</label>
            <input className={inputClass} placeholder="https://example.com/logo.png" type="url" value={form.logoUrl} onChange={(e) => updateField("logoUrl", e.target.value)} />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <button type="button" onClick={handleClose} className="px-6 py-2.5 rounded-xl font-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">
              Cancel
            </button>
            <button type="submit" disabled={loading}
              className="bg-primary text-on-primary px-6 py-2.5 rounded-xl font-bold hover:brightness-110 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 shadow-sm active:scale-[0.97]"
            >
              {loading ? (
                <><svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" /></svg> Saving...</>
              ) : "Save Changes"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
