"use client";

import { useState, useRef } from "react";
import { apiFetch } from "@/lib/apiClient";

export interface Product {
  id: string;
  brandId: string;
  name: string;
  description: string;
  price: number;
  images?: string[];
  stock?: number;
  createdAt: string;
}

interface Props {
  open: boolean;
  mode: "add" | "edit";
  onClose: () => void;
  onSuccess: (product: Product) => void;
  brandId: string;
  product?: Product;
}

export default function ProductModal({ open, mode, onClose, onSuccess, brandId, product }: Props) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [form, setForm] = useState({
    name: product?.name || "",
    description: product?.description || "",
    price: product?.price?.toString() || "",
  });
  const [files, setFiles] = useState<File[]>([]);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const updateField = (field: string, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (error) setError(null);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = Array.from(e.target.files || []);
    setFiles((prev) => [...prev, ...selected]);
    if (error) setError(null);
  };

  const removeFile = (index: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const handleClose = () => {
    setFiles([]);
    onClose();
  };

  const buildApiBody = () => {
    const fd = new FormData();
    fd.append("name", form.name.trim());
    fd.append("brandId", brandId);
    fd.append("description", form.description.trim());
    fd.append("price", form.price);
    files.forEach((file) => fd.append("ImageFiles", file));
    return fd;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim()) { setError("Product name is required"); return; }
    if (!form.price.trim() || isNaN(Number(form.price))) { setError("Valid price is required"); return; }

    setLoading(true);
    setError(null);

    const endpoint = mode === "edit" && product ? `/products/${product.id}` : "/products";
    const method = mode === "edit" ? "PUT" : "POST";

    try {
      const result = await apiFetch(endpoint, { method, body: buildApiBody() });
      if (result?.success && result.data) {
        onSuccess(result.data);
        handleClose();
        return;
      }
      setError(result?.message || `Failed to ${mode === "edit" ? "update" : "create"} product`);
    } catch (err: any) {
      setError(err?.message || `Failed to ${mode === "edit" ? "update" : "create"} product`);
    } finally {
      setLoading(false);
    }
  };

  if (!open) return null;

  const inputClass =
    "w-full rounded-xl border border-outline-variant/60 bg-surface-container-lowest px-4 py-2.5 text-body-md text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all";

  const labelClass = "text-label-md font-bold text-on-surface-variant uppercase";

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150 overflow-y-auto">
      <div className="bg-surface rounded-2xl border border-outline-variant/20 shadow-xl w-full max-w-lg mx-4 animate-in fade-in zoom-in-95 duration-200">
        <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-outline-variant/20">
          <h3 className="text-headline-sm text-on-surface font-bold">{mode === "edit" ? "Edit Product" : "Create New Product"}</h3>
          <button onClick={handleClose} className="text-outline hover:text-primary transition-colors active:scale-[0.97]">
            <span className="material-symbols-outlined text-[20px]">close</span>
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="p-6 space-y-4">
            {error && (
              <div className="flex items-center gap-3 rounded-xl border border-danger-red/20 bg-error-container/50 px-4 py-3 text-body-sm text-on-error-container">
                <span className="material-symbols-outlined text-error text-[18px]">error</span>
                <span className="flex-1">{error}</span>
                <button onClick={() => setError(null)} type="button" className="text-on-error-container/50 hover:text-on-error-container">
                  <span className="material-symbols-outlined text-[16px]">close</span>
                </button>
              </div>
            )}

            <div className="space-y-1">
              <label className={labelClass}>Product Name <span className="text-danger-red">*</span></label>
              <input className={inputClass} placeholder="e.g. Radiance Glow Serum" value={form.name} onChange={(e) => updateField("name", e.target.value)} />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1">
                <label className={labelClass}>Price <span className="text-danger-red">*</span></label>
                <div className="relative">
                  <span className="absolute inset-y-0 left-0 pl-3 flex items-center text-outline/40 text-body-md">$</span>
                  <input className={`${inputClass} pl-7`} placeholder="0.00" type="number" step="0.01" min="0" value={form.price} onChange={(e) => updateField("price", e.target.value)} />
                </div>
              </div>
              <div className="space-y-1">
                <label className={labelClass}>Product Media</label>
                <input ref={fileInputRef} type="file" multiple accept="image/*" className="hidden" onChange={handleFileChange} />
                <div onClick={() => fileInputRef.current?.click()} className="border-2 border-dashed border-outline-variant/50 rounded-xl p-4 flex flex-col items-center justify-center gap-1 hover:border-primary hover:bg-primary/5 transition-all cursor-pointer min-h-[52px]">
                  {files.length > 0 ? (
                    <div className="text-center w-full">
                      <span className="material-symbols-outlined text-primary text-2xl">check_circle</span>
                      <p className="text-label-sm text-primary">{files.length} file(s) selected</p>
                      <div className="flex flex-wrap gap-1 mt-1.5 justify-center">
                        {files.map((f, i) => (
                          <span key={i} className="inline-flex items-center gap-0.5 px-2 py-0.5 rounded-full bg-primary/8 text-primary text-label-xs">
                            {f.name.length > 12 ? f.name.slice(0, 10) + ".." : f.name}
                            <button type="button" onClick={(e) => { e.stopPropagation(); removeFile(i); }} className="hover:text-primary/60">
                              <span className="material-symbols-outlined text-label-xs">close</span>
                            </button>
                          </span>
                        ))}
                      </div>
                    </div>
                  ) : (
                    <>
                      <span className="material-symbols-outlined text-outline text-2xl">upload_file</span>
                      <span className="text-label-sm text-outline">Click to upload images</span>
                    </>
                  )}
                </div>
              </div>
            </div>

            <div className="space-y-1">
              <label className={labelClass}>Product Description</label>
              <textarea className={`${inputClass} resize-none`} placeholder="Describe the product benefits and key ingredients..." rows={3} value={form.description} onChange={(e) => updateField("description", e.target.value)} />
            </div>
          </div>

          <div className="bg-surface-container-low px-6 py-4 flex items-center justify-end gap-3 rounded-b-2xl">
            <button type="button" onClick={handleClose} className="px-6 py-2 text-label-md font-bold text-on-surface-variant hover:bg-surface-container transition-colors rounded-xl active:scale-[0.97]">
              Cancel
            </button>
            <button type="submit" disabled={loading}
              className="px-6 py-2 bg-primary text-on-primary text-label-md font-bold rounded-xl shadow-md hover:opacity-90 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 active:scale-[0.97]"
            >
              {loading ? (
                <><svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" /></svg> {mode === "edit" ? "Saving..." : "Adding..."}</>
              ) : mode === "edit" ? "Save Changes" : "Create Product"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
