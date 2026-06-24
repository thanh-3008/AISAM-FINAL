"use client";

import { useState, useMemo, useRef, useCallback, useEffect } from "react";
import { useRouter } from "next/navigation";
import Header from "@/components/layout/Header";

import { PLATFORM_CONFIG, CONTENT_TYPES, CREATE_STATUS_OPTIONS, ALL_TAGS, getBrandColor, PlatformIcon, type ContentType, type ContentStatus } from "@/lib/contentConstants";
import { createContent, type CreateContentPayload } from "@/services/contentService";
import { fetchBrands, fetchProducts } from "@/services/brandService";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";

const SAMPLE_AVATARS = [
  "https://api.dicebear.com/7.x/notionists/svg?seed=1",
  "https://api.dicebear.com/7.x/notionists/svg?seed=2",
  "https://api.dicebear.com/7.x/notionists/svg?seed=3",
];

export default function CreateContentPage() {
  const router = useRouter();
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  const [brandList, setBrandList] = useState<{ id: string; name: string }[]>([]);
  const [productList, setProductList] = useState<{ id: string; name: string; brandId: string }[]>([]);

  const [form, setForm] = useState({
    title: "",
    brandId: "",
    productId: "",
    type: "TEXT" as ContentType,
    status: "Awaiting Approval" as ContentStatus,
    platforms: [] as string[],
    tags: [] as string[],
    hashtags: [] as string[],
    thumbnail: "",
    textContent: "",
    imageUrl: "",
    videoUrl: "",
    duration: "",
    description: "",
    caption: "",
    ctaLink: "",
    scheduledAt: "",
    internalNotes: "",
  });

  useEffect(() => {
    fetchBrands().then(setBrandList);
  }, []);

  useEffect(() => {
    if (form.brandId) {
      fetchProducts(form.brandId).then(setProductList);
    } else {
      setProductList([]);
    }
  }, [form.brandId]);

  useEffect(() => {
    if (brandList.length > 0 && !form.brandId) {
      update({ brandId: brandList[0].id });
    }
  }, [brandList]);

  const [hashtagInput, setHashtagInput] = useState("");

  const [showPlatformPicker, setShowPlatformPicker] = useState(false);
  const [showTagPicker, setShowTagPicker] = useState(false);
  const [dragOver, setDragOver] = useState<string | null>(null);
  const [previewPlatform, setPreviewPlatform] = useState("facebook");

  const imageInputRef = useRef<HTMLInputElement>(null);
  const videoInputRef = useRef<HTMLInputElement>(null);
  const thumbnailInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = useCallback((field: "imageUrl" | "videoUrl" | "thumbnail") => {
    const input = field === "imageUrl" ? imageInputRef : field === "videoUrl" ? videoInputRef : thumbnailInputRef;
    input.current?.click();
  }, []);

  const handleFileChange = useCallback((field: "imageUrl" | "videoUrl" | "thumbnail", e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const url = URL.createObjectURL(file);
      update({ [field]: url });
      if (field === "thumbnail") update({ thumbnail: url });
    }
  }, []);

  const handleDrop = useCallback((field: "imageUrl" | "videoUrl" | "thumbnail", e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(null);
    const file = e.dataTransfer.files?.[0];
    if (file) {
      const url = URL.createObjectURL(file);
      update({ [field]: url });
      if (field === "thumbnail") update({ thumbnail: url });
    }
  }, []);

  const clearFile = useCallback((field: "imageUrl" | "videoUrl" | "thumbnail") => {
    update({ [field]: "" });
    if (field === "thumbnail") update({ thumbnail: "" });
  }, []);

  const addHashtag = (raw: string) => {
    const tag = raw.trim().replace(/^#/, "");
    if (tag && !form.hashtags.includes(tag)) {
      update({ hashtags: [...form.hashtags, tag] });
    }
  };

  const removeHashtag = (tag: string) => {
    update({ hashtags: form.hashtags.filter((h) => h !== tag) });
  };

  const handleHashtagKey = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" || e.key === ",") {
      e.preventDefault();
      addHashtag(hashtagInput);
      setHashtagInput("");
    }
  };

  const update = (partial: Partial<typeof form>) => setForm((p) => ({ ...p, ...partial }));

  const availableProducts = brandList.length > 0 ? productList : [];
  const selectedBrand = brandList.find(b => b.id === form.brandId);
  const selectedProduct = productList.find(p => p.id === form.productId);
  const selectedBrandName = selectedBrand?.name || "";
  const selectedProductName = selectedProduct?.name || "";
  const isValid = form.title.trim().length > 0 && form.productId && form.brandId.length > 0;

  const handleSave = async () => {
    if (!isValid) return;
    setSaving(true);
    setSaveError(null);

    const storedWs = getStoredActiveWorkspace();
    if (!storedWs) {
      setSaving(false);
      setSaveError("Bạn cần chọn Workspace trước khi tạo nội dung.");
      return;
    }

    const payload: CreateContentPayload = {
      brandId: form.brandId,
      productId: form.productId || null,
      adType: form.type === "IMAGE" ? 1 : form.type === "VIDEO" ? 2 : 0,
      title: form.title,
      textContent: form.textContent || form.caption || form.description || "",
      imageUrl: form.imageUrl || undefined,
      videoUrl: form.videoUrl || undefined,
      styleDescription: form.description || undefined,
      contextDescription: form.caption || undefined,
      status: form.status === "Awaiting Approval" ? 1 : 0,
    };

    try {
      const result = await createContent(payload);
      if (result) {
        setSaved(true);
        setTimeout(() => router.push("/content"), 1000);
      } else {
        setSaveError("Không thể lưu nội dung. BE trả về lỗi, kiểm tra console (F12) để biết chi tiết.");
      }
    } catch (e: any) {
      const msg = e?.message || "";
      if (msg.includes("Profile not found")) {
        setSaveError("Workspace hiện tại không tồn tại. Đang chuyển hướng...");
        setTimeout(() => router.push("/overview"), 2000);
      } else {
        setSaveError(msg || "Lỗi không xác định khi lưu nội dung.");
      }
    } finally {
      setSaving(false);
    }
  };

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Content Library", href: "/content" },
        { label: "Create Content" },
      ]} />

      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
        <div className="max-w-5xl mx-auto space-y-6">
          {/* Header */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <button onClick={() => router.push("/content")}
                className="w-9 h-9 flex items-center justify-center rounded-xl border border-outline-variant/20 text-outline/50 hover:bg-surface-container hover:text-on-surface transition-all active:scale-[0.97]">
                <span className="material-symbols-outlined text-[18px]">arrow_back</span>
              </button>
              <div>
                <h1 className="text-headline-sm font-bold text-on-surface">Create New Content</h1>
                <p className="text-body-sm text-on-surface-variant">Write and format your content manually</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              {saved && (
                <span className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-emerald-50 text-emerald-600 text-label-sm font-semibold animate-in fade-in slide-in-from-right-2 duration-200">
                  <span className="material-symbols-outlined text-[14px]">check_circle</span>
                  Saved! Redirecting...
                </span>
              )}
              {saveError && (
                <span className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-danger-red/10 text-danger-red text-label-sm font-semibold animate-in fade-in slide-in-from-right-2 duration-200">
                  <span className="material-symbols-outlined text-[14px]">error</span>
                  {saveError}
                </span>
              )}
              <button onClick={() => router.push("/content")}
                className="px-4 py-2 rounded-xl border border-outline-variant/20 text-label-sm text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">
                Cancel
              </button>
              <button onClick={handleSave} disabled={!isValid || saving || saved}
                className="px-5 py-2 rounded-xl bg-primary text-on-primary text-label-sm font-semibold hover:shadow-lg active:scale-[0.97] transition-all flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed">
                {saving ? (
                  <span className="flex items-center gap-2">
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    Saving...
                  </span>
                ) : (
                  <><span className="material-symbols-outlined text-[16px]">check</span> Save Content</>
                )}
              </button>
            </div>
          </div>

          {/* Main content */}
          <div className="flex flex-col xl:flex-row gap-gutter">
            {/* Left: Form */}
            <div className="flex-1 min-w-0 space-y-5">
              <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 space-y-5">
                {/* Title */}
                <div>
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Title <span className="text-danger-red">*</span></label>
                  <input value={form.title} onChange={(e) => update({ title: e.target.value })}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all text-lg font-semibold placeholder:text-outline/30"
                    placeholder="Enter a compelling title for your content..." autoFocus />
                </div>

                {/* Brand & Product */}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Brand <span className="text-danger-red">*</span></label>
                    <select value={form.brandId} onChange={(e) => update({ brandId: e.target.value, productId: "" })}
                      className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all">
                      {brandList.map((b) => <option key={b.id} value={b.id}>{b.name}</option>)}
                    </select>
                  </div>
                  <div>
                    <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Product <span className="text-danger-red">*</span></label>
                    <select value={form.productId} onChange={(e) => update({ productId: e.target.value })}
                      className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all">
                      <option value="">Select product</option>
                      {availableProducts.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
                    </select>
                  </div>
                </div>

                {/* Content Type */}
                <div>
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Content Type</label>
                  <div className="grid grid-cols-3 gap-3">
                    {CONTENT_TYPES.map((t) => (
                      <button key={t.value} type="button" onClick={() => update({ type: t.value })}
                        className={`flex flex-col items-center gap-2 p-4 rounded-xl border-2 transition-all ${
                          form.type === t.value
                            ? "border-primary bg-primary/5 text-primary"
                            : "border-outline-variant/20 bg-surface-container text-on-surface-variant hover:border-primary/30"
                        }`}>
                        <span className="material-symbols-outlined text-[24px]">{t.icon}</span>
                        <span className="text-label-sm font-semibold">{t.label}</span>
                      </button>
                    ))}
                  </div>
                </div>

                {/* Type-specific fields */}
                {form.type === "TEXT" && (
                  <div>
                    <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Content Body</label>
                    <textarea value={form.textContent} onChange={(e) => update({ textContent: e.target.value })}
                      className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all min-h-[200px] resize-y leading-relaxed"
                      placeholder="Write your content here..." />
                    <div className="flex items-center justify-end gap-3 mt-1.5 text-label-xs text-outline">
                      <span>{form.textContent.length} characters</span>
                      <span>{form.textContent.split(/\s+/).filter(Boolean).length} words</span>
                    </div>
                  </div>
                )}

                {form.type === "IMAGE" && (
                  <div>
                    <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Upload Image</label>
                    <input ref={imageInputRef} type="file" accept="image/*" className="hidden" onChange={(e) => handleFileChange("imageUrl", e)} />
                    <div
                      onDragOver={(e) => { e.preventDefault(); setDragOver("image"); }}
                      onDragLeave={() => setDragOver(null)}
                      onDrop={(e) => handleDrop("imageUrl", e)}
                      onClick={() => handleFileSelect("imageUrl")}
                      className={`relative border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-all ${
                        dragOver === "image" ? "border-primary bg-primary/5" : form.imageUrl ? "border-transparent bg-surface-container" : "border-outline-variant/30 hover:border-primary/40 hover:bg-surface-container/50"
                      }`}>
                      {form.imageUrl ? (
                        <div className="relative">
                          <div className="max-h-[300px] overflow-hidden rounded-lg">
                            <img src={form.imageUrl} alt="Uploaded preview" className="w-full h-auto object-contain max-h-[280px]" />
                          </div>
                          <div className="absolute top-2 right-2 flex gap-1.5">
                            <button onClick={(e) => { e.stopPropagation(); handleFileSelect("imageUrl"); }}
                              className="w-8 h-8 rounded-lg bg-black/50 text-white flex items-center justify-center hover:bg-black/70 transition-all">
                              <span className="material-symbols-outlined text-[16px]">refresh</span>
                            </button>
                            <button onClick={(e) => { e.stopPropagation(); clearFile("imageUrl"); }}
                              className="w-8 h-8 rounded-lg bg-black/50 text-white flex items-center justify-center hover:bg-danger-red/80 transition-all">
                              <span className="material-symbols-outlined text-[16px]">close</span>
                            </button>
                          </div>
                          <p className="text-label-xs text-outline mt-2">Click to replace or drag a new image</p>
                        </div>
                      ) : (
                        <div className="flex flex-col items-center gap-2">
                          <div className="w-14 h-14 rounded-2xl bg-primary/10 flex items-center justify-center">
                            <span className="material-symbols-outlined text-primary text-3xl">add_photo_alternate</span>
                          </div>
                          <div>
                            <p className="text-body-sm text-on-surface font-medium">Click to upload</p>
                            <p className="text-label-sm text-outline/60 mt-0.5">or drag and drop your image here</p>
                          </div>
                          <p className="text-label-2xs text-outline/40">PNG, JPG, WebP up to 10MB</p>
                        </div>
                      )}
                    </div>
                  </div>
                )}

                {form.type === "VIDEO" && (
                  <div className="space-y-4">
                    <div>
                      <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Upload Video</label>
                      <input ref={videoInputRef} type="file" accept="video/*" className="hidden" onChange={(e) => handleFileChange("videoUrl", e)} />
                      <div
                        onDragOver={(e) => { e.preventDefault(); setDragOver("video"); }}
                        onDragLeave={() => setDragOver(null)}
                        onDrop={(e) => handleDrop("videoUrl", e)}
                        onClick={() => handleFileSelect("videoUrl")}
                        className={`relative border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-all ${
                          dragOver === "video" ? "border-primary bg-primary/5" : form.videoUrl ? "border-transparent bg-surface-container" : "border-outline-variant/30 hover:border-primary/40 hover:bg-surface-container/50"
                        }`}>
                        {form.videoUrl ? (
                          <div className="relative">
                            <video src={form.videoUrl} className="w-full max-h-[280px] rounded-lg" controls />
                            <div className="absolute top-2 right-2 flex gap-1.5">
                              <button onClick={(e) => { e.stopPropagation(); handleFileSelect("videoUrl"); }}
                                className="w-8 h-8 rounded-lg bg-black/50 text-white flex items-center justify-center hover:bg-black/70 transition-all">
                                <span className="material-symbols-outlined text-[16px]">refresh</span>
                              </button>
                              <button onClick={(e) => { e.stopPropagation(); clearFile("videoUrl"); }}
                                className="w-8 h-8 rounded-lg bg-black/50 text-white flex items-center justify-center hover:bg-danger-red/80 transition-all">
                                <span className="material-symbols-outlined text-[16px]">close</span>
                              </button>
                            </div>
                          </div>
                        ) : (
                          <div className="flex flex-col items-center gap-2">
                            <div className="w-14 h-14 rounded-2xl bg-rose-500/10 flex items-center justify-center">
                              <span className="material-symbols-outlined text-rose-500 text-3xl">videocam</span>
                            </div>
                            <div>
                              <p className="text-body-sm text-on-surface font-medium">Click to upload</p>
                              <p className="text-label-sm text-outline/60 mt-0.5">or drag and drop your video here</p>
                            </div>
                            <p className="text-label-2xs text-outline/40">MP4, WebM, MOV up to 100MB</p>
                          </div>
                        )}
                      </div>
                    </div>
                    <div>
                      <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Duration</label>
                      <input value={form.duration} onChange={(e) => update({ duration: e.target.value })}
                        className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all"
                        placeholder="e.g. 2:34" />
                    </div>
                  </div>
                )}

                {/* Description */}
                <div>
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Description</label>
                  <textarea value={form.description} onChange={(e) => update({ description: e.target.value })}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all min-h-[80px] resize-y"
                    placeholder="Add a brief description of this content..." />
                </div>

                {/* Caption */}
                <div>
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Social Media Caption</label>
                  <textarea value={form.caption} onChange={(e) => update({ caption: e.target.value })}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all min-h-[100px] resize-y leading-relaxed"
                    placeholder="Write the caption that will appear on social media posts..." />
                  <div className="flex items-center justify-end gap-3 mt-1.5 text-label-xs text-outline">
                    <span>{form.caption.length} characters</span>
                    <span>{form.caption.split(/\s+/).filter(Boolean).length} words</span>
                  </div>
                </div>
              </div>

              {/* Meta section */}
              <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 space-y-5">
                <h3 className="text-label-md text-on-surface font-semibold flex items-center gap-2">
                  <span className="material-symbols-outlined text-[16px] text-outline">tune</span>
                  Publishing Settings
                </h3>

                {/* Status */}
                <div>
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Status</label>
                  <div className="grid grid-cols-2 gap-2">
                    {CREATE_STATUS_OPTIONS.map((s) => (
                      <button key={s.value} type="button" onClick={() => update({ status: s.value })}
                        className={`px-3 py-2 rounded-xl text-label-sm font-semibold transition-all ${
                          form.status === s.value
                            ? "bg-primary text-on-primary shadow-sm"
                            : "bg-surface-container text-on-surface-variant hover:bg-surface-container-high"
                        }`}>
                        {s.label}
                      </button>
                    ))}
                  </div>
                </div>

                {/* Platforms */}
                <div className="relative">
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Platforms</label>
                  <button type="button" onClick={() => setShowPlatformPicker(!showPlatformPicker)}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-left text-on-surface hover:border-primary/40 transition-all flex items-center justify-between">
                    <span className={form.platforms.length === 0 ? "text-outline/40" : ""}>
                      {form.platforms.length === 0 ? "Select platforms to publish" : `${form.platforms.length} platform${form.platforms.length > 1 ? "s" : ""} selected`}
                    </span>
                    <span className={`material-symbols-outlined text-[14px] text-outline transition-transform ${showPlatformPicker ? "rotate-180" : ""}`}>expand_more</span>
                  </button>
                  {showPlatformPicker && (
                    <>
                      <div className="fixed inset-0 z-10" onClick={() => setShowPlatformPicker(false)} />
                      <div className="absolute left-0 right-0 top-full mt-1 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-xl z-20 p-2 space-y-0.5 dropdown-enter">
                        {Object.entries(PLATFORM_CONFIG).map(([key, cfg]) => (
                          <label key={key} className="flex items-center gap-2.5 px-3 py-2 rounded-lg hover:bg-surface-container cursor-pointer transition-colors">
                            <input type="checkbox" checked={form.platforms.includes(key)} onChange={() => {
                              update({
                                platforms: form.platforms.includes(key)
                                  ? form.platforms.filter((x) => x !== key)
                                  : [...form.platforms, key],
                              });
                            }} className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/30" />
                            <PlatformIcon platform={cfg.icon} />
                            <span className="text-label-sm text-on-surface">{cfg.label}</span>
                          </label>
                        ))}
                      </div>
                    </>
                  )}
                  {form.platforms.length > 0 && (
                    <div className="flex items-center gap-1.5 mt-2 flex-wrap">
                      {form.platforms.map((p) => {
                        const cfg = PLATFORM_CONFIG[p];
                        return (
                          <span key={p} className="px-2 py-0.5 rounded-lg flex items-center gap-1 text-label-xs font-semibold"
                            style={{ backgroundColor: (cfg?.color || "#666") + "20", color: cfg?.color || "#666" }}>
                            <PlatformIcon platform={cfg?.icon || "default"} />
                            {cfg?.label || p}
                            <button onClick={() => update({ platforms: form.platforms.filter((x) => x !== p) })} className="hover:opacity-60">
                              <span className="material-symbols-outlined text-label-xs">close</span>
                            </button>
                          </span>
                        );
                      })}
                    </div>
                  )}
                </div>

                {/* Tags */}
                <div className="relative">
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Tags</label>
                  <button type="button" onClick={() => setShowTagPicker(!showTagPicker)}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-left text-on-surface hover:border-primary/40 transition-all flex items-center justify-between">
                    <span className={form.tags.length === 0 ? "text-outline/40" : ""}>
                      {form.tags.length === 0 ? "Add tags to categorize" : `${form.tags.length} tag${form.tags.length > 1 ? "s" : ""} selected`}
                    </span>
                    <span className={`material-symbols-outlined text-[14px] text-outline transition-transform ${showTagPicker ? "rotate-180" : ""}`}>expand_more</span>
                  </button>
                  {showTagPicker && (
                    <>
                      <div className="fixed inset-0 z-10" onClick={() => setShowTagPicker(false)} />
                      <div className="absolute left-0 right-0 top-full mt-1 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-xl z-20 p-2 space-y-0.5 dropdown-enter">
                        {ALL_TAGS.map((t) => (
                          <label key={t} className="flex items-center gap-2.5 px-3 py-2 rounded-lg hover:bg-surface-container cursor-pointer transition-colors">
                            <input type="checkbox" checked={form.tags.includes(t)} onChange={() => {
                              update({
                                tags: form.tags.includes(t) ? form.tags.filter((x) => x !== t) : [...form.tags, t],
                              });
                            }} className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/30" />
                            <span className="text-label-sm text-on-surface">{t}</span>
                          </label>
                        ))}
                      </div>
                    </>
                  )}
                  {form.tags.length > 0 && (
                    <div className="flex items-center gap-1.5 mt-2 flex-wrap">
                      {form.tags.map((t) => (
                        <span key={t} className="px-2 py-0.5 rounded-md bg-surface-container text-label-xs font-semibold text-on-surface-variant flex items-center gap-1">
                          {t}
                          <button onClick={() => update({ tags: form.tags.filter((x) => x !== t) })} className="hover:opacity-60">
                            <span className="material-symbols-outlined text-label-xs">close</span>
                          </button>
                        </span>
                      ))}
                    </div>
                  )}
                </div>

                {/* Hashtags */}
                <div>
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Hashtags</label>
                  <div className="flex items-center flex-wrap gap-1.5 px-3 py-2 bg-surface-container border border-outline-variant/20 rounded-xl min-h-[44px] cursor-text transition-all focus-within:border-primary/40 focus-within:ring-2 focus-within:ring-primary/5"
                    onClick={() => document.getElementById("hashtag-input")?.focus()}>
                    {form.hashtags.map((tag) => (
                      <span key={tag} className="inline-flex items-center gap-0.5 px-1.5 py-0.5 rounded-md bg-primary/10 text-primary text-label-xs font-semibold">
                        #{tag}
                        <button onClick={() => removeHashtag(tag)} className="hover:opacity-60">
                          <span className="material-symbols-outlined text-label-xs">close</span>
                        </button>
                      </span>
                    ))}
                    <input id="hashtag-input" value={hashtagInput} onChange={(e) => setHashtagInput(e.target.value)}
                      onKeyDown={handleHashtagKey}
                      onBlur={() => { if (hashtagInput) { addHashtag(hashtagInput); setHashtagInput(""); } }}
                      className="flex-1 min-w-[80px] bg-transparent border-none outline-none text-body-sm text-on-surface placeholder:text-outline/30"
                      placeholder={form.hashtags.length === 0 ? "Type and press Enter to add hashtags" : "Add more..."} />
                  </div>
                </div>

                {/* CTA Link */}
                <div>
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">CTA Link</label>
                  <input value={form.ctaLink} onChange={(e) => update({ ctaLink: e.target.value })}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all"
                    placeholder="https://example.com/landing-page" />
                </div>

                {/* Internal Notes */}
                <div>
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Internal Notes</label>
                  <textarea value={form.internalNotes} onChange={(e) => update({ internalNotes: e.target.value })}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-3 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all min-h-[60px] resize-y"
                    placeholder="Notes for your team (not published)..." />
                </div>

                {/* Thumbnail */}
                <div>
                  <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Thumbnail</label>
                  <input ref={thumbnailInputRef} type="file" accept="image/*" className="hidden" onChange={(e) => handleFileChange("thumbnail", e)} />
                  <div
                    onDragOver={(e) => { e.preventDefault(); setDragOver("thumb"); }}
                    onDragLeave={() => setDragOver(null)}
                    onDrop={(e) => handleDrop("thumbnail", e)}
                    onClick={() => handleFileSelect("thumbnail")}
                    className={`border-2 border-dashed rounded-xl p-6 text-center cursor-pointer transition-all ${
                      dragOver === "thumb" ? "border-primary bg-primary/5" : form.thumbnail ? "border-transparent bg-surface-container" : "border-outline-variant/30 hover:border-primary/40 hover:bg-surface-container/50"
                    }`}>
                    {form.thumbnail ? (
                      <div className="relative inline-block">
                        <div className="w-40 h-24 rounded-lg overflow-hidden">
                          <img src={form.thumbnail} alt="Thumbnail" className="w-full h-full object-cover" />
                        </div>
                        <button onClick={(e) => { e.stopPropagation(); clearFile("thumbnail"); }}
                          className="absolute -top-2 -right-2 w-6 h-6 rounded-full bg-black/50 text-white flex items-center justify-center hover:bg-danger-red/80 transition-all">
                          <span className="material-symbols-outlined text-[12px]">close</span>
                        </button>
                      </div>
                    ) : (
                      <div className="flex items-center gap-3 justify-center">
                        <span className="material-symbols-outlined text-outline/40 text-2xl">image</span>
                        <span className="text-body-sm text-outline/60">Click to upload thumbnail image</span>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>

            {/* Right: Live Preview */}
            <div className="w-full xl:w-[420px] shrink-0">
              <div className="sticky top-0 space-y-3">
                {/* Platform Tabs */}
                <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden">
                  <div className="p-3 border-b border-outline-variant/10 flex items-center gap-1.5">
                    <span className="material-symbols-outlined text-[14px] text-outline">visibility</span>
                    <span className="text-label-sm text-on-surface font-semibold mr-auto">Post Preview</span>
                  </div>
                  <div className="flex gap-1 p-2">
                    {(["facebook", "instagram", "tiktok"] as const).map((p) => {
                      const cfg = PLATFORM_CONFIG[p];
                      const isSelected = form.platforms.includes(p) || form.platforms.length === 0;
                      if (!isSelected) return null;
                      return (
                        <button key={p} onClick={() => setPreviewPlatform(p)}
                          className={`flex-1 flex items-center justify-center gap-1.5 px-2 py-1.5 rounded-lg text-label-xs font-semibold transition-all ${
                            previewPlatform === p
                              ? "bg-surface-container text-on-surface shadow-sm"
                              : "text-outline/50 hover:bg-surface-container/50 hover:text-outline"
                          }`}>
                          <PlatformIcon platform={cfg.icon} />
                          <span className="hidden sm:inline">{cfg.label}</span>
                        </button>
                      );
                    })}
                  </div>
                </div>

                {/* Platform-specific Post Card */}
                <div className="bg-white rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden">
                  {/* Facebook Post */}
                  {previewPlatform === "facebook" && (
                    <div className="font-sans">
                      <div className="p-3.5 flex items-center gap-3">
                        <div className="w-9 h-9 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white text-[12px] font-bold shrink-0">
                          {selectedBrandName.charAt(0)}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-[13px] font-semibold text-[#1a1a1a] leading-tight">{selectedBrandName || "Brand Name"}</p>
                          <p className="text-[11px] text-[#65676b]">{selectedProductName ? `Promoting ${selectedProductName} · ` : ""}Just now · <span className="material-symbols-outlined text-label-xs align-middle">public</span></p>
                        </div>
                        <span className="material-symbols-outlined text-[18px] text-[#65676b]">more_horiz</span>
                      </div>
                      {form.title && (
                      <p className="px-3.5 text-[15px] font-semibold text-[#1a1a1a] mb-1">{form.title}</p>
                      )}
                      {form.caption && (
                        <p className="px-3.5 text-[15px] text-[#1a1a1a] leading-[1.35] whitespace-pre-line mb-1">
                          {form.caption}
                        </p>
                      )}
                      {!form.caption && form.description && (
                        <p className="px-3.5 text-[15px] text-[#1a1a1a] leading-[1.35] whitespace-pre-line mb-1">
                          {form.description}
                        </p>
                      )}
                      {form.textContent && form.textContent !== form.caption && (
                        <div className="px-3.5 mb-2.5">
                          <div className="p-2.5 bg-[#f0f2f5] rounded-lg border border-[#e4e6eb]">
                            <p className="text-[11px] text-[#65676b] font-semibold uppercase tracking-wide mb-1">Article</p>
                            <p className="text-[13px] text-[#1a1a1a] leading-[1.4] whitespace-pre-line line-clamp-4">
                              {form.textContent}
                            </p>
                          </div>
                        </div>
                      )}
                      {form.hashtags.length > 0 && (
                        <p className="px-3.5 text-[13px] text-[#216fdb] mb-2.5">
                          {form.hashtags.map((h) => `#${h}`).join(" ")}
                        </p>
                      )}
                      {(form.thumbnail || form.imageUrl) && (
                        <div className="border-t border-b border-[#e4e6eb]">
                          <img src={form.thumbnail || form.imageUrl} alt="" className="w-full max-h-[300px] object-contain bg-[#f0f2f5]" />
                        </div>
                      )}
                      {form.videoUrl && (
                        <div className="border-t border-b border-[#e4e6eb] bg-[#1a1a1a] aspect-video flex items-center justify-center">
                          <div className="w-14 h-14 rounded-full bg-white/20 flex items-center justify-center backdrop-blur cursor-pointer">
                            <span className="material-symbols-outlined text-white text-3xl">play_arrow</span>
                          </div>
                        </div>
                      )}
                      {form.ctaLink && (
                        <div className="mx-3.5 mt-2.5 p-2.5 border border-[#e4e6eb] rounded-lg">
                          <p className="text-[11px] text-[#65676b] uppercase font-semibold">Learn More</p>
                          <p className="text-[13px] text-[#1a1a1a] truncate">{form.ctaLink}</p>
                        </div>
                      )}
                      <div className="px-3.5 py-2 flex items-center gap-1 text-[13px] text-[#65676b] border-t border-[#e4e6eb] mt-2.5">
                        <svg viewBox="0 0 24 24" className="w-[18px] h-[18px]" fill="#65676b"><path d="M1 21h4V9H1v12zm22-11c0-1.1-.9-2-2-2h-6.31l.95-4.57.03-.32c0-.41-.17-.79-.44-1.06L14.17 1 7.59 7.59C7.22 7.95 7 8.45 7 9v10c0 1.1.9 2 2 2h9.33c.83 0 1.54-.5 1.84-1.22l3.02-7.05c.09-.23.14-.47.14-.73v-2z"/></svg>
                        <span className="font-semibold">Like</span>
                        <svg viewBox="0 0 24 24" className="w-[18px] h-[18px] ml-5" fill="#65676b"><path d="M12 2C6.48 2 2 6.48 2 12c0 5.52 4.48 10 10 10 5.52 0 10-4.48 10-10 0-5.52-4.48-10-10-10zm-2 15l-4 4V7c0-1.1.9-2 2-2h8c1.1 0 2 .9 2 2v8c0 1.1-.9 2-2 2h-6z"/></svg>
                        <span className="font-semibold">Comment</span>
                        <svg viewBox="0 0 24 24" className="w-[18px] h-[18px] ml-5" fill="#65676b"><path d="M21 12l-7-7v4c-7 1-10 5-11 11 2.5-3.5 6-5.5 11-5.5v4l7-7z"/></svg>
                        <span className="font-semibold">Share</span>
                      </div>
                    </div>
                  )}

                  {/* Instagram Post */}
                  {previewPlatform === "instagram" && (
                    <div className="font-sans bg-white">
                      <div className="p-3 flex items-center gap-2.5">
                        <div className="w-7 h-7 rounded-full bg-gradient-to-br from-purple-500 via-pink-500 to-orange-400 p-[2px]">
                          <div className="w-full h-full rounded-full bg-white flex items-center justify-center">
                            <span className="text-label-2xs font-bold" style={{ color: getBrandColor(selectedBrandName) || "#666" }}>{selectedBrandName.charAt(0)}</span>
                          </div>
                        </div>
                        <p className="text-[12px] font-semibold text-[#262626] flex-1">{selectedBrandName || "brand"}</p>
                        <span className="material-symbols-outlined text-[18px] text-[#262626]">more_horiz</span>
                      </div>
                      <div className="aspect-square bg-[#fafafa] flex items-center justify-center border-t border-b border-[#efefef]">
                        {(form.thumbnail || form.imageUrl) ? (
                          <img src={form.thumbnail || form.imageUrl} alt="" className="w-full h-full object-contain" />
                        ) : (
                          <div className="flex flex-col items-center gap-2 text-[#c7c7c7]">
                            <span className="material-symbols-outlined text-4xl">{form.type === "VIDEO" ? "play_circle" : "landscape"}</span>
                            <span className="text-[11px]">Instagram Post</span>
                          </div>
                        )}
                      </div>
                      <div className="p-3 space-y-1.5">
                        <div className="flex items-center gap-3">
                          <svg viewBox="0 0 24 24" className="w-[22px] h-[22px]" fill="#262626"><path d="M16.5 3C14.5 3 12.9 4.1 12 5.6 11.1 4.1 9.5 3 7.5 3 4.4 3 2 5.4 2 8.5c0 3.9 3.2 6.6 8.3 11.1l1.7 1.6 1.7-1.6C18.8 15.1 22 12.4 22 8.5 22 5.4 19.6 3 16.5 3zM12 18.1l-.9-.8C6.7 13 4 10.5 4 8.5 4 6.6 5.6 5 7.5 5c1.5 0 3 1 3.6 2.4h1.8C13.5 6 15 5 16.5 5 18.4 5 20 6.6 20 8.5c0 2-2.7 4.5-7.1 8.8l-.9.8z"/></svg>
                          <svg viewBox="0 0 24 24" className="w-[22px] h-[22px]" fill="#262626"><path d="M12 2C6.5 2 2 6.5 2 12c0 5.5 4.5 10 10 10s10-4.5 10-10c0-5.5-4.5-10-10-10zm5.5 12.5h-11v-1h11v1zm-2 3h-7v-1h7v1zm2-6h-11v-1h11v1z"/></svg>
                          <svg viewBox="0 0 24 24" className="w-[22px] h-[22px]" fill="#262626"><path d="M2 2v20l5-5h13V2H2zm18 13H6.5l-2.5 2.5V4h16v11z"/></svg>
                          <svg viewBox="0 0 24 24" className="w-[22px] h-[22px] ml-auto" fill="#262626"><path d="M17 3H7c-1.1 0-2 .9-2 2v14l5-3 5 3V5c0-1.1-.9-2-2-2z"/></svg>
                        </div>
                        <p className="text-[12px] font-semibold text-[#262626]">{selectedBrandName ? `${selectedBrandName.toLowerCase().replace(/\s+/g, "")} ` : ""}<span className="font-normal whitespace-pre-line">{form.caption || form.description || form.title || "Write a caption..."}</span></p>
                        {form.hashtags.length > 0 && (
                          <p className="text-[12px] text-[#00376b]">{form.hashtags.map((h) => `#${h}`).join(" ")}</p>
                        )}
                        <p className="text-label-xs text-[#8e8e8e] uppercase tracking-wide">View all comments</p>
                      </div>
                    </div>
                  )}

                  {/* TikTok Post */}
                  {previewPlatform === "tiktok" && (
                    <div className="font-sans bg-[#111111] text-white relative overflow-hidden">
                      <div className="aspect-[9/16] flex items-center justify-center relative">
                        {form.thumbnail ? (
                          <img src={form.thumbnail} alt="" className="absolute inset-0 w-full h-full object-cover" />
                        ) : form.videoUrl ? (
                          <div className="absolute inset-0 bg-gradient-to-br from-gray-800 to-gray-900 flex items-center justify-center">
                            <svg viewBox="0 0 24 24" className="w-[48px] h-[48px]" fill="rgba(255,255,255,0.3)"><path d="M10 16.5V8h7v2h-5v6.5a3.5 3.5 0 1 1-2-3.2z"/></svg>
                          </div>
                        ) : (
                          <div className="absolute inset-0 bg-gradient-to-br from-gray-800 to-gray-900 flex items-center justify-center">
                            <svg viewBox="0 0 24 24" className="w-[48px] h-[48px]" fill="rgba(255,255,255,0.3)"><path d="M10 16.5V8h7v2h-5v6.5a3.5 3.5 0 1 1-2-3.2z"/></svg>
                          </div>
                        )}
                        <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/70 to-transparent p-4 pt-12">
                          <div className="flex items-center gap-2 mb-2">
                            <div className="w-8 h-8 rounded-full bg-gradient-to-br flex items-center justify-center text-label-xs font-bold shrink-0 border border-white/30"
                              style={{ background: `linear-gradient(135deg, ${getBrandColor(selectedBrandName) || "#666"}, ${getBrandColor(selectedBrandName) || "#999"}` }}>
                          {selectedBrandName.charAt(0)}
                            </div>
                            <p className="text-[13px] font-semibold">@{selectedBrandName?.toLowerCase().replace(/\s+/g, "") || "brand"}</p>
                          </div>
                          <p className="text-[12px] leading-relaxed whitespace-pre-line">{form.caption || form.description || form.title || "Add a caption..."}</p>
                          {form.hashtags.length > 0 && (
                            <p className="text-[12px] text-[#00acee] mt-0.5">{form.hashtags.map((h) => `#${h}`).join(" ")}</p>
                          )}
                          <div className="flex items-center gap-1.5 mt-1.5 text-[11px] text-white/60">
                            <svg viewBox="0 0 24 24" className="w-[14px] h-[14px]" fill="rgba(255,255,255,0.6)"><path d="M9 3v10.5a4.5 4.5 0 1 0 2-3.8V7h7V3H9z"/></svg>
                            <span>original sound - {selectedBrandName || "Creator"}</span>
                          </div>
                        </div>
                        <div className="absolute bottom-4 right-3 flex flex-col items-center gap-3">
                          <div className="flex flex-col items-center gap-0.5">
                            <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center backdrop-blur">
                              <svg viewBox="0 0 24 24" className="w-[20px] h-[20px]" fill="white"><path d="M16.5 3C14.5 3 12.9 4.1 12 5.6 11.1 4.1 9.5 3 7.5 3 4.4 3 2 5.4 2 8.5c0 3.9 3.2 6.6 8.3 11.1l1.7 1.6 1.7-1.6C18.8 15.1 22 12.4 22 8.5 22 5.4 19.6 3 16.5 3z"/></svg>
                            </div>
                            <span className="text-label-xs">12.4K</span>
                          </div>
                          <div className="flex flex-col items-center gap-0.5">
                            <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center backdrop-blur">
                              <svg viewBox="0 0 24 24" className="w-[20px] h-[20px]" fill="white"><path d="M12 2C6.5 2 2 6.5 2 12c0 5.5 4.5 10 10 10s10-4.5 10-10c0-5.5-4.5-10-10-10zm5.5 12.5h-11v-1h11v1zm-2 3h-7v-1h7v1zm2-6h-11v-1h11v1z"/></svg>
                            </div>
                            <span className="text-label-xs">834</span>
                          </div>
                          <div className="flex flex-col items-center gap-0.5">
                            <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center backdrop-blur">
                              <svg viewBox="0 0 24 24" className="w-[20px] h-[20px]" fill="white"><path d="M17 3H7c-1.1 0-2 .9-2 2v14l5-3 5 3V5c0-1.1-.9-2-2-2z"/></svg>
                            </div>
                            <span className="text-label-xs">2.1K</span>
                          </div>
                          <div className="flex flex-col items-center gap-0.5">
                            <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center backdrop-blur">
                              <svg viewBox="0 0 24 24" className="w-[20px] h-[20px]" fill="white"><path d="M18 16.08c-.76 0-1.44.3-1.96.77L8.91 12.7c.05-.23.09-.46.09-.7s-.04-.47-.09-.7l7.05-4.11c.54.5 1.25.81 2.04.81 1.66 0 3-1.34 3-3s-1.34-3-3-3-3 1.34-3 3c0 .24.04.47.09.7L8.04 9.81C7.5 9.31 6.79 9 6 9c-1.66 0-3 1.34-3 3s1.34 3 3 3c.79 0 1.5-.31 2.04-.81l7.12 4.16c-.05.21-.08.43-.08.65 0 1.61 1.31 2.92 2.92 2.92 1.61 0 2.92-1.31 2.92-2.92s-1.31-2.92-2.92-2.92z"/></svg>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </>
  );
}
