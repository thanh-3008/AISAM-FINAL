"use client";

import { useEffect, useRef, useState } from "react";
import { apiFetch } from "@/lib/apiClient";

export interface Product {
  id: string;
  brandId: string;
  name: string;
  description: string;
  productUrl?: string | null;
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

interface ExtractedProduct {
  productName: string;
  description?: string | null;
  price?: number | null;
  images: string[];
  sourceUrl: string;
  benefits: string[];
  features: string[];
  targetAudience?: string | null;
  tone?: string | null;
  keywords: string[];
  recommendedCTA?: string | null;
  importStatus?: string;
}

type ImportForm = {
  benefits: string[];
  features: string[];
  targetAudience: string;
  tone: string;
  keywords: string[];
  recommendedCTA: string;
};

const EMPTY_IMPORT_FORM: ImportForm = {
  benefits: [],
  features: [],
  targetAudience: "",
  tone: "",
  keywords: [],
  recommendedCTA: "",
};

function formatVndInput(value: string | number | null | undefined) {
  if (value === null || value === undefined || value === "") return "";

  if (typeof value === "number") {
    return new Intl.NumberFormat("vi-VN", { maximumFractionDigits: 0 }).format(value);
  }

  const digits = value.replace(/\D/g, "");
  if (!digits) return "";

  return new Intl.NumberFormat("vi-VN", { maximumFractionDigits: 0 }).format(Number(digits));
}

function parseVndInput(value: string) {
  const digits = value.replace(/\D/g, "");
  return digits ? Number(digits) : Number.NaN;
}

export default function ProductModal({ open, mode, onClose, onSuccess, brandId, product }: Props) {
  const [loading, setLoading] = useState(false);
  const [extracting, setExtracting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [form, setForm] = useState({
    name: product?.name || "",
    description: product?.description || "",
    productUrl: product?.productUrl || "",
    price: product?.price != null ? formatVndInput(product.price) : "",
    stock: product?.stock != null ? String(product.stock) : "",
  });
  const [files, setFiles] = useState<File[]>([]);
  const [importUrl, setImportUrl] = useState("");
  const [extracted, setExtracted] = useState<ExtractedProduct | null>(null);
  const [selectedImageUrls, setSelectedImageUrls] = useState<string[]>([]);
  const [loadedImageUrls, setLoadedImageUrls] = useState<string[]>([]);
  const [brokenImageUrls, setBrokenImageUrls] = useState<string[]>([]);
  const [importForm, setImportForm] = useState<ImportForm>(EMPTY_IMPORT_FORM);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!open) return;
    setForm({
      name: product?.name || "",
      description: product?.description || "",
      productUrl: product?.productUrl || "",
      price: product?.price != null ? formatVndInput(product.price) : "",
      stock: product?.stock != null ? String(product.stock) : "",
    });
    setFiles([]);
    setImportUrl("");
    setExtracted(null);
    setSelectedImageUrls([]);
    setLoadedImageUrls([]);
    setBrokenImageUrls([]);
    setImportForm(EMPTY_IMPORT_FORM);
    setError(null);
  }, [open, product]);

  const updateField = (field: string, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    if (error) setError(null);
  };

  const updatePriceField = (value: string) => {
    updateField("price", formatVndInput(value));
  };

  const updateImportField = (field: keyof ImportForm, value: string) => {
    setImportForm((prev) => ({ ...prev, [field]: value }));
    if (error) setError(null);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = Array.from(e.target.files || []);
    const allowed = new Set(["image/jpeg", "image/png", "image/webp", "image/gif"]);
    if (files.length + selected.length > 5) {
      setError("A product can contain at most 5 images");
      e.target.value = "";
      return;
    }
    const invalid = selected.find((file) => !allowed.has(file.type) || file.size > 10 * 1024 * 1024);
    if (invalid) {
      setError(`${invalid.name} must be JPEG, PNG, WEBP, or GIF and no larger than 10 MB`);
      e.target.value = "";
      return;
    }
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
    const benefits = cleanList(importForm.benefits);
    const features = cleanList(importForm.features);
    const keywords = cleanList(importForm.keywords);

    fd.append("name", form.name.trim());
    fd.append("brandId", brandId);
    fd.append("description", form.description.trim());
    fd.append("productUrl", form.productUrl.trim());
    fd.append("price", String(parseVndInput(form.price)));
    const parsedStock = parseInt(form.stock, 10);
    if (!isNaN(parsedStock) && parsedStock >= 0) {
      fd.append("stock", String(parsedStock));
    }
    if (mode === "add") {
      if (benefits.length > 0) fd.append("primaryUse", benefits.join("; "));
      if (benefits[0]) fd.append("usp", benefits[0]);
      if (importForm.targetAudience.trim()) fd.append("targetAudience", importForm.targetAudience.trim());
      fd.append(
        "knowledgeProfile",
        JSON.stringify({
          importStatus: "Manual",
          sourceUrl: form.productUrl.trim() || null,
          productName: form.name.trim(),
          description: form.description.trim(),
          price: parseVndInput(form.price),
          benefits,
          features,
          targetAudience: importForm.targetAudience.trim() || null,
          tone: importForm.tone.trim() || null,
          keywords,
          recommendedCTA: importForm.recommendedCTA.trim() || null,
          createdFrom: "manual_product_form",
          createdAt: new Date().toISOString(),
        }),
      );
    }
    files.forEach((file) => fd.append("ImageFiles", file));
    return fd;
  };

  const handleExtractUrl = async () => {
    let parsed: URL;
    try {
      parsed = new URL(importUrl.trim());
      if (!["http:", "https:"].includes(parsed.protocol)) throw new Error();
    } catch {
      setError("URL sản phẩm không hợp lệ.");
      return;
    }

    setExtracting(true);
    setError(null);
    try {
      const result: any = await apiFetch("/products/extract-url", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ url: parsed.toString() }),
      });

      if (!result?.success || !result.data) {
        setError(result?.message || "Không thể trích xuất sản phẩm từ URL này.");
        return;
      }

      const data = result.data as ExtractedProduct;
      const uniqueImages = Array.from(new Set((data.images || []).filter(isLikelyImageUrl)));
      setExtracted({ ...data, images: uniqueImages });
      setLoadedImageUrls([]);
      setBrokenImageUrls([]);
      setSelectedImageUrls(uniqueImages.slice(0, 5));
      setImportForm({
        benefits: data.benefits || [],
        features: data.features || [],
        targetAudience: data.targetAudience || "",
        tone: data.tone || "",
        keywords: data.keywords || [],
        recommendedCTA: data.recommendedCTA || "",
      });
      setForm({
        name: data.productName || "",
        description: data.description || "",
        productUrl: data.sourceUrl || parsed.toString(),
        price: data.price != null ? formatVndInput(data.price) : "",
        stock: "",
      });
      setFiles([]);
    } catch (err: any) {
      setError(err.message || "Không thể đọc dữ liệu từ trang web này. Vui lòng nhập thủ công.");
    } finally {
      setExtracting(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.name.trim()) {
      setError("Product name is required");
      return;
    }
    const parsedPrice = parseVndInput(form.price);
    if (!form.price.trim() || isNaN(parsedPrice)) {
      setError("Valid price is required");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      let result: any;
      if (mode === "add" && extracted) {
        result = await apiFetch("/products/import-reviewed", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            brandId,
            productName: form.name.trim(),
            description: form.description.trim(),
            price: parsedPrice,
            images: selectedImageUrls.filter((url) => !brokenImageUrls.includes(url)),
            sourceUrl: form.productUrl.trim() || extracted.sourceUrl,
            benefits: importForm.benefits,
            features: importForm.features,
            targetAudience: importForm.targetAudience,
            tone: importForm.tone,
            keywords: importForm.keywords,
            recommendedCTA: importForm.recommendedCTA,
            stock: parseInt(form.stock, 10) || 0,
          }),
        });
      } else {
        const endpoint = mode === "edit" && product ? `/products/${product.id}` : "/products";
        const method = mode === "edit" ? "PUT" : "POST";
        result = await apiFetch(endpoint, { method, body: buildApiBody() });
      }

      if (result?.success && result.data) {
        onSuccess(result.data);
        handleClose();
      } else {
        setError(result?.message || `Failed to ${mode === "edit" ? "update" : "create"} product`);
      }
    } catch (err: any) {
      setError(err?.message || "Network error. Please check your connection");
    } finally {
      setLoading(false);
    }
  };

  if (!open) return null;

  const inputClass =
    "w-full rounded-xl border border-outline-variant/60 bg-surface-container-lowest px-4 py-2.5 text-body-md text-on-surface placeholder:text-outline/40 focus:border-primary focus:ring-1 focus:ring-primary outline-none transition-all";

  const labelClass = "text-label-md font-bold text-on-surface-variant uppercase";
  const isImportedReview = mode === "add" && !!extracted;
  const visibleExtractedImages = extracted?.images.filter((url) => !brokenImageUrls.includes(url)) || [];
  const hiddenBrokenImageCount = extracted ? extracted.images.length - visibleExtractedImages.length : 0;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150 overflow-y-auto py-8">
      <div className="bg-surface rounded-2xl border border-outline-variant/20 shadow-xl w-full max-w-2xl mx-4 animate-in fade-in zoom-in-95 duration-200">
        <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-outline-variant/20">
          <div>
            <h3 className="text-headline-sm text-on-surface font-bold">{mode === "edit" ? "Edit Product" : "Create New Product"}</h3>
            {isImportedReview && <p className="text-body-sm text-on-surface-variant">AI extracted product. Review before saving.</p>}
          </div>
          <button onClick={handleClose} className="text-outline hover:text-primary transition-colors active:scale-[0.97]">
            <span className="material-symbols-outlined text-[20px]">close</span>
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="p-6 space-y-4 max-h-[72vh] overflow-y-auto">
            {error && (
              <div className="flex items-center gap-3 rounded-xl border border-danger-red/20 bg-error-container/50 px-4 py-3 text-body-sm text-on-error-container">
                <span className="material-symbols-outlined text-error text-[18px]">error</span>
                <span className="flex-1">{error}</span>
                <button onClick={() => setError(null)} type="button" className="text-on-error-container/50 hover:text-on-error-container">
                  <span className="material-symbols-outlined text-[16px]">close</span>
                </button>
              </div>
            )}

            {mode === "add" && (
              <div className="rounded-2xl border border-primary/20 bg-primary/5 p-4 space-y-3">
                <div className="flex items-start gap-3">
                  <span className="material-symbols-outlined text-primary">auto_awesome</span>
                  <div>
                    <p className="text-title-sm font-bold text-on-surface">AI Import Product from URL</p>
                    <p className="text-body-sm text-on-surface-variant">Dán link Shopee, Tiki hoặc website sản phẩm để AISAM đọc metadata và tạo hồ sơ marketing nháp.</p>
                  </div>
                </div>
                <div className="flex flex-col sm:flex-row gap-2">
                  <input
                    className={inputClass}
                    placeholder="Dán đường dẫn sản phẩm..."
                    value={importUrl}
                    onChange={(e) => setImportUrl(e.target.value)}
                    disabled={extracting || loading}
                  />
                  <button
                    type="button"
                    onClick={handleExtractUrl}
                    disabled={extracting || loading || !importUrl.trim()}
                    className="shrink-0 rounded-xl bg-primary px-4 py-2.5 text-label-md font-bold text-on-primary shadow-md transition-all hover:opacity-90 disabled:opacity-50"
                  >
                    {extracting ? "Đang phân tích..." : "Trích xuất bằng AI"}
                  </button>
                </div>
              </div>
            )}

            <div className="space-y-1">
              <label className={labelClass}>Product Name <span className="text-danger-red">*</span></label>
              <input className={inputClass} placeholder="e.g. Radiance Glow Serum" value={form.name} onChange={(e) => updateField("name", e.target.value)} />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1">
                <label className={labelClass}>Price <span className="text-danger-red">*</span></label>
                <div className="relative">
                  <input className={`${inputClass} pr-14`} placeholder="0" type="text" inputMode="numeric" value={form.price} onChange={(e) => updatePriceField(e.target.value)} />
                  <span className="absolute inset-y-0 right-0 pr-3 flex items-center text-outline/50 text-label-sm font-semibold">VND</span>
                </div>
              </div>
              <div className="space-y-1">
                <label className={labelClass}>Stock</label>
                <input className={inputClass} placeholder="0" type="number" min="0" value={form.stock} onChange={(e) => updateField("stock", e.target.value)} />
              </div>

              {!isImportedReview && (
                <div className="space-y-1">
                  <label className={labelClass}>Product Media</label>
                  <input ref={fileInputRef} type="file" multiple accept="image/jpeg,image/png,image/webp,image/gif" className="hidden" onChange={handleFileChange} />
                  <div onClick={() => fileInputRef.current?.click()} className="border-2 border-dashed border-outline-variant/50 rounded-xl p-4 flex flex-col items-center justify-center gap-1 hover:border-primary hover:bg-primary/5 transition-all cursor-pointer min-h-[52px]">
                    {files.length > 0 ? (
                      <div className="text-center w-full">
                        <span className="material-symbols-outlined text-primary text-2xl">check_circle</span>
                        <p className="text-label-sm text-primary">{files.length} file(s) selected</p>
                        <div className="flex flex-wrap gap-1 mt-1.5 justify-center">
                          {files.map((f, i) => (
                            <span key={`${f.name}-${i}`} className="inline-flex items-center gap-0.5 px-2 py-0.5 rounded-full bg-primary/8 text-primary text-label-xs">
                              {f.name.length > 12 ? f.name.slice(0, 10) + ".." : f.name}
                              <button type="button" onClick={(event) => { event.stopPropagation(); removeFile(i); }} className="hover:text-primary/60">
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
              )}
            </div>

            <div className="space-y-1">
              <label className={labelClass}>Product Description</label>
              <textarea className={`${inputClass} resize-none`} placeholder="Describe the product benefits and key ingredients..." rows={3} value={form.description} onChange={(e) => updateField("description", e.target.value)} />
            </div>

            <div className="space-y-1">
              <label className={labelClass}>Product URL</label>
              <input
                className={inputClass}
                placeholder="https://example.com/product"
                value={form.productUrl}
                onChange={(e) => updateField("productUrl", e.target.value)}
              />
              <p className="text-body-xs text-on-surface-variant">
                Nếu có link, AI sẽ tự gắn CTA/link này vào caption quảng cáo, trừ khi bạn yêu cầu không chèn link.
              </p>
            </div>

            {mode === "add" && (
              <div className="space-y-4 rounded-2xl border border-outline-variant/30 bg-surface-container-low p-4">
                <div>
                  <p className="text-title-sm font-bold text-on-surface">Marketing Profile</p>
                  <p className="text-body-xs text-on-surface-variant">
                    {isImportedReview
                      ? "AI đã trích xuất hồ sơ sản phẩm. Bạn có thể sửa lại trước khi lưu."
                      : "Nhập thêm thông tin marketing để AI tạo caption, ảnh và quảng cáo chính xác hơn."}
                  </p>
                </div>

                {isImportedReview && (
                <div>
                  <label className={labelClass}>Extracted Images</label>
                  <p className="text-body-xs text-on-surface-variant mb-2">Chọn ảnh muốn giữ lại cho product. Tối đa 5 ảnh.</p>
                  {hiddenBrokenImageCount > 0 && (
                    <p className="mb-2 text-body-xs text-amber-600">
                      Đã tự ẩn {hiddenBrokenImageCount} ảnh bị lỗi hoặc không tải được.
                    </p>
                  )}
                  {visibleExtractedImages.length > 0 ? (
                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                      {visibleExtractedImages.map((url) => {
                        const checked = selectedImageUrls.includes(url);
                        const loaded = loadedImageUrls.includes(url);
                        return (
                          <button
                            type="button"
                            key={url}
                            onClick={() => {
                              setSelectedImageUrls((prev) =>
                                checked ? prev.filter((item) => item !== url) : prev.length >= 5 ? prev : [...prev, url],
                              );
                            }}
                            className={`relative overflow-hidden rounded-xl border text-left transition-all ${checked ? "border-primary ring-2 ring-primary/30" : "border-outline-variant/40 opacity-70 hover:opacity-100"}`}
                          >
                            {!loaded && (
                              <div className="absolute inset-0 flex items-center justify-center bg-surface-container-high">
                                <span className="h-6 w-6 animate-spin rounded-full border-2 border-outline-variant border-t-primary" />
                              </div>
                            )}
                            <img
                              src={url}
                              alt=""
                              className={`h-28 w-full object-cover transition-opacity duration-200 ${loaded ? "opacity-100" : "opacity-0"}`}
                              loading="lazy"
                              onLoad={() => {
                                setLoadedImageUrls((prev) => (prev.includes(url) ? prev : [...prev, url]));
                              }}
                              onError={() => {
                                setBrokenImageUrls((prev) => (prev.includes(url) ? prev : [...prev, url]));
                                setSelectedImageUrls((prev) => prev.filter((item) => item !== url));
                              }}
                            />
                            <span className={`absolute right-2 top-2 rounded-full px-2 py-1 text-label-xs font-bold ${checked ? "bg-primary text-on-primary" : "bg-black/50 text-white"}`}>
                              {checked ? "Keep" : "Skip"}
                            </span>
                          </button>
                        );
                      })}
                    </div>
                  ) : (
                    <div className="rounded-xl border border-outline-variant/40 p-4 text-body-sm text-on-surface-variant">Không tìm thấy ảnh từ URL.</div>
                  )}
                </div>

                )}

                <ChipEditor label="Benefits" values={importForm.benefits} onChange={(values) => setImportForm((prev) => ({ ...prev, benefits: values }))} placeholder="Nhập lợi ích rồi Enter" />
                <ChipEditor label="Features" values={importForm.features} onChange={(values) => setImportForm((prev) => ({ ...prev, features: values }))} placeholder="Nhập tính năng rồi Enter" />
                <ChipEditor label="Keywords" values={importForm.keywords} onChange={(values) => setImportForm((prev) => ({ ...prev, keywords: values }))} placeholder="Nhập keyword rồi Enter" />

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div className="space-y-1">
                    <label className={labelClass}>Target Audience</label>
                    <input className={inputClass} value={importForm.targetAudience} onChange={(e) => updateImportField("targetAudience", e.target.value)} />
                  </div>
                  <div className="space-y-1">
                    <label className={labelClass}>Tone</label>
                    <input className={inputClass} maxLength={200} value={importForm.tone} onChange={(e) => updateImportField("tone", e.target.value)} />
                  </div>
                </div>
                <div className="space-y-1">
                  <label className={labelClass}>Recommended CTA</label>
                  <input className={inputClass} value={importForm.recommendedCTA} onChange={(e) => updateImportField("recommendedCTA", e.target.value)} />
                </div>
              </div>
            )}
          </div>

          <div className="bg-surface-container-low px-6 py-4 flex items-center justify-end gap-3 rounded-b-2xl">
            <button type="button" onClick={handleClose} className="px-6 py-2 text-label-md font-bold text-on-surface-variant hover:bg-surface-container transition-colors rounded-xl active:scale-[0.97]">
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading || extracting}
              className="px-6 py-2 bg-primary text-on-primary text-label-md font-bold rounded-xl shadow-md hover:opacity-90 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 active:scale-[0.97]"
            >
              {loading ? (
                <><svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" /></svg> {mode === "edit" ? "Saving..." : "Adding..."}</>
              ) : isImportedReview ? "Save Reviewed Product" : mode === "edit" ? "Save Changes" : "Create Product"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

function isLikelyImageUrl(value: string) {
  try {
    const url = new URL(value);
    if (!["http:", "https:"].includes(url.protocol)) return false;
    const cleanPath = url.pathname.toLowerCase();
    return (
      cleanPath.endsWith(".jpg") ||
      cleanPath.endsWith(".jpeg") ||
      cleanPath.endsWith(".png") ||
      cleanPath.endsWith(".webp") ||
      cleanPath.endsWith(".gif") ||
      cleanPath.endsWith(".avif") ||
      cleanPath.includes("/image") ||
      cleanPath.includes("/images") ||
      cleanPath.includes("/photo") ||
      cleanPath.includes("/photos") ||
      cleanPath.includes("/product")
    );
  } catch {
    return false;
  }
}

function cleanList(values: string[]) {
  return Array.from(new Set(values.map((value) => value.trim()).filter(Boolean)));
}

function ChipEditor({
  label,
  values,
  onChange,
  placeholder,
}: {
  label: string;
  values: string[];
  onChange: (values: string[]) => void;
  placeholder: string;
}) {
  const [draft, setDraft] = useState("");

  const addDraft = () => {
    const clean = draft.trim();
    if (!clean) return;
    if (!values.some((value) => value.toLowerCase() === clean.toLowerCase())) {
      onChange([...values, clean]);
    }
    setDraft("");
  };

  return (
    <div className="space-y-1">
      <label className="text-label-md font-bold text-on-surface-variant uppercase">{label}</label>
      <div className="rounded-xl border border-outline-variant/60 bg-surface-container-lowest p-2">
        <div className="flex flex-wrap gap-2">
          {values.map((value) => (
            <span key={value} className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-3 py-1 text-label-sm text-primary">
              {value}
              <button type="button" onClick={() => onChange(values.filter((item) => item !== value))} className="hover:opacity-70">
                <span className="material-symbols-outlined text-[14px]">close</span>
              </button>
            </span>
          ))}
          <input
            className="min-w-[160px] flex-1 bg-transparent px-2 py-1 text-body-sm outline-none placeholder:text-outline/40"
            value={draft}
            placeholder={placeholder}
            onChange={(event) => setDraft(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === "Enter") {
                event.preventDefault();
                addDraft();
              }
            }}
            onBlur={addDraft}
          />
        </div>
      </div>
    </div>
  );
}
