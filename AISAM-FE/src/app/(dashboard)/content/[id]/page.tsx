"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { type ContentDetail, type ContentType, type ContentStatus } from "@/lib/mockContent";
import { PLATFORM_CONFIG, ALL_PLATFORMS, STATUS_OPTIONS, getTypeStyle, getTypeIcon, PlatformIcon } from "@/lib/contentConstants";
import { fetchContentById, updateContent, deleteContent, CONTENTTYPE_TO_ADTYPE } from "@/services/contentService";

export default function ContentDetailPage() {
  const params = useParams();
  const router = useRouter();
  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [showDelete, setShowDelete] = useState(false);
  const [showPlatformPicker, setShowPlatformPicker] = useState(false);
  const [visible, setVisible] = useState(false);

  const [item, setItem] = useState<ContentDetail | null>(null);
  const [loading, setLoading] = useState(true);

  const [form, setForm] = useState<{
    title: string; status: ContentStatus; description: string; platforms: string[];
    caption: string; ctaLink: string; scheduledAt: string; internalNotes: string; hashtags: string[];
  }>({ title: "", status: "Draft", description: "", platforms: [], caption: "", ctaLink: "", scheduledAt: "", internalNotes: "", hashtags: [] });

  const notFound = !item && !loading;

  useEffect(() => { const t = setTimeout(() => setVisible(true), 80); return () => clearTimeout(t); }, []);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      const result = await fetchContentById(params.id as string);
      setItem(result);
      setLoading(false);
    };
    load();
  }, [params.id]);

  useEffect(() => {
    if (item) setForm({ title: item.title, status: item.status, description: item.description || "", platforms: [...item.platforms], caption: item.caption || "", ctaLink: item.ctaLink || "", scheduledAt: item.scheduledAt || "", internalNotes: item.internalNotes || "", hashtags: item.hashtags || [] });
  }, [item?.id]);

  const handleSave = async () => {
    setSaving(true);
    await updateContent(params.id as string, {
      title: form.title,
      adType: item ? CONTENTTYPE_TO_ADTYPE[item.type] : undefined,
      textContent: form.caption,
    });
    if (item) {
      item.title = form.title;
      item.status = form.status;
      item.description = form.description || undefined;
      item.platforms = form.platforms;
      item.caption = form.caption || undefined;
      item.ctaLink = form.ctaLink || undefined;
      item.scheduledAt = form.scheduledAt || undefined;
      item.internalNotes = form.internalNotes || undefined;
      item.hashtags = form.hashtags;
    }
    setSaving(false);
    setEditing(false);
  };

  const handleDelete = async () => {
    await deleteContent(params.id as string);
    router.push("/content");
  };

  if (loading) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Content Library", href: "/content" }, { label: "Loading..." }]} />
        <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto flex items-center justify-center">
          <div className="flex flex-col items-center gap-3">
            <div className="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
            <p className="text-body-sm text-on-surface-variant">Loading content...</p>
          </div>
        </main>
      </>
    );
  }

  if (notFound) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Content Library", href: "/content" }, { label: "Not Found" }]} />
        <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto">
          <div className="flex flex-col items-center justify-center py-32 text-center gap-6">
            <div className="w-20 h-20 rounded-3xl bg-surface-container-high flex items-center justify-center">
              <span className="material-symbols-outlined text-outline/40 text-4xl">block</span>
            </div>
            <h2 className="text-headline-md text-on-surface font-bold">Content not found</h2>
            <p className="text-body-md text-on-surface-variant max-w-sm">The content you&apos;re looking for doesn&apos;t exist or has been deleted.</p>
            <Link href="/content" className="px-5 py-2.5 bg-primary text-on-primary rounded-xl font-semibold text-label-sm hover:shadow-lg active:scale-[0.97] transition-all">
              Back to Content Library
            </Link>
          </div>
        </main>
      </>
    );
  }

  if (!item) return null;

  const typeGradient = getTypeStyle(item.type);
  const typeIcon = getTypeIcon(item.type);

  return (
    <>
      <style>{`
        @keyframes fade-up { from { opacity: 0; transform: translateY(16px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes float { 0%,100% { transform: translateY(0px); } 50% { transform: translateY(-6px); } }
        .animate-fade-up { animation: fade-up 0.5s ease-out forwards; opacity: 0; }
        .animate-float { animation: float 4s ease-in-out infinite; }
      `}</style>

      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Content Library", href: "/content" }, { label: item.title }]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto space-y-6">

        {/* Back + Actions */}
        <div className={`flex items-center justify-between ${visible ? "animate-fade-up" : ""}`}>
          <button onClick={() => router.push("/content")}
            className="inline-flex items-center gap-1.5 px-3 py-2 rounded-xl border border-outline-variant/20 text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-all active:scale-[0.97] text-label-sm">
            <span className="material-symbols-outlined text-[16px]">arrow_back</span>
            Back
          </button>
          <div className="flex items-center gap-2">
            {!editing ? (
              <>
                <button onClick={() => setEditing(true)}
                  className="px-4 py-2 rounded-xl border border-outline-variant/20 text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-all active:scale-[0.97] text-label-sm font-semibold flex items-center gap-1.5">
                  <span className="material-symbols-outlined text-[16px]">edit</span>
                  Edit
                </button>
                <button onClick={() => setShowDelete(true)}
                  className="px-4 py-2 rounded-xl border border-danger-red/20 text-danger-red hover:bg-danger-red/5 transition-all active:scale-[0.97] text-label-sm font-semibold flex items-center gap-1.5">
                  <span className="material-symbols-outlined text-[16px]">delete</span>
                  Delete
                </button>
              </>
            ) : (
              <>
                <button onClick={() => { setEditing(false); if (item) setForm({ title: item.title, status: item.status, description: item.description || "", platforms: [...item.platforms], caption: item.caption || "", ctaLink: item.ctaLink || "", scheduledAt: item.scheduledAt || "", internalNotes: item.internalNotes || "", hashtags: item.hashtags || [] }); }}
                  className="px-4 py-2 rounded-xl border border-outline-variant/20 text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97] text-label-sm font-semibold">
                  Cancel
                </button>
                <button onClick={handleSave} disabled={saving}
                  className="px-4 py-2 rounded-xl bg-primary text-on-primary text-label-sm font-semibold hover:shadow-lg active:scale-[0.97] transition-all flex items-center gap-1.5 disabled:opacity-60">
                  {saving ? (
                    <>Saving...</>
                  ) : (
                    <><span className="material-symbols-outlined text-[16px]">check</span> Save</>
                  )}
                </button>
              </>
            )}
          </div>
        </div>

        {/* Main Content */}
        <div className="flex flex-col xl:flex-row gap-gutter">
          {/* Preview Area */}
          <div className="flex-1 min-w-0 space-y-gutter">
            {/* Content Preview */}
            <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.08s" }}>
              <div className="p-6">
                <div className="flex items-center gap-2 mb-4">
                  <span className="text-label-sm text-outline font-semibold uppercase tracking-wider">Preview</span>
                  <span className="px-2 py-0.5 rounded-md bg-gradient-to-br text-white text-label-xs font-semibold flex items-center gap-1" style={{ background: `linear-gradient(135deg, var(--color-${item.type === "IMAGE" ? "blue" : item.type === "TEXT" ? "purple" : "rose"}-500), var(--color-${item.type === "IMAGE" ? "blue" : item.type === "TEXT" ? "purple" : "rose"}-400))` }}>
                    <span className="material-symbols-outlined text-label-xs">{typeIcon}</span>
                    {item.type}
                  </span>
                </div>

                {item.type === "IMAGE" && (
                  <div className="w-full max-w-2xl mx-auto aspect-video bg-gradient-to-br from-surface-container to-surface-container-high rounded-xl flex items-center justify-center">
                    {item.imageUrl ? (
                      <img src={item.imageUrl} alt={item.title} className="w-full h-full object-contain rounded-xl" />
                    ) : (
                      <div className="text-center">
                        <div className={`w-24 h-24 mx-auto rounded-2xl bg-gradient-to-br ${typeGradient} flex items-center justify-center text-white shadow-lg mb-3`}>
                          <span className="material-symbols-outlined text-4xl">{typeIcon}</span>
                        </div>
                        <p className="text-body-sm text-outline">No image uploaded</p>
                      </div>
                    )}
                  </div>
                )}

                {item.type === "TEXT" && (
                  <div className="w-full max-w-2xl mx-auto">
                    <div className="bg-surface-container rounded-xl p-6 min-h-[200px]">
                      <p className="text-body-md text-on-surface leading-relaxed whitespace-pre-line">
                        {item.textContent || "No content yet."}
                      </p>
                    </div>
                    <div className="flex items-center gap-4 mt-3 text-label-xs text-outline">
                      <span>{(item.textContent || "").length} characters</span>
                      {item.textContent && <span>{item.textContent.split(/\s+/).length} words</span>}
                    </div>
                  </div>
                )}

                {item.type === "VIDEO" && (
                  <div className="w-full max-w-2xl mx-auto">
                    <div className="aspect-video bg-gradient-to-br from-surface-container to-surface-container-high rounded-xl flex items-center justify-center relative overflow-hidden">
                      <div className={`w-24 h-24 rounded-full bg-gradient-to-br ${typeGradient} flex items-center justify-center text-white shadow-lg cursor-pointer hover:scale-110 transition-transform`}>
                        <span className="material-symbols-outlined text-4xl">play_arrow</span>
                      </div>
                      {item.duration && (
                        <span className="absolute bottom-3 right-3 px-2 py-1 bg-black/50 text-white text-label-xs rounded-md font-semibold">
                          {item.duration}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-4 mt-3 text-label-xs text-outline">
                      {item.duration && <span>{item.duration}</span>}
                      {item.fileSize && <span>{item.fileSize}</span>}
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Description */}
            <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.16s" }}>
              <div className="p-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-label-md text-on-surface font-semibold">Description</h3>
                </div>
                {editing ? (
                  <textarea value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-xl p-4 text-body-sm text-on-surface placeholder:text-outline/30 focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all min-h-[100px] resize-y"
                    placeholder="Add a description..." />
                ) : (
                  <p className="text-body-sm text-on-surface-variant leading-relaxed">{item.description || "No description provided."}</p>
                )}
              </div>
            </div>
          </div>

          {/* Sidebar Metadata */}
          <div className="w-full xl:w-80 shrink-0 space-y-gutter">
            <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.12s" }}>
              <div className="p-5 space-y-5">
                {/* Title */}
                <div>
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Title</p>
                  {editing ? (
                    <input value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))}
                      className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-3 py-2 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all" />
                  ) : (
                    <p className="text-body-sm text-on-surface font-medium">{item.title}</p>
                  )}
                </div>

                {/* Brand / Product */}
                <div>
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Brand</p>
                  <p className="text-body-sm text-on-surface">{item.brandName}</p>
                </div>
                <div>
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Product</p>
                  <p className="text-body-sm text-on-surface">{item.productName}</p>
                </div>

                {/* Status */}
                <div>
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Status</p>
                  {editing ? (
                    <select value={form.status} onChange={(e) => setForm((p) => ({ ...p, status: e.target.value as ContentStatus }))}
                      className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-3 py-2 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all">
                      {STATUS_OPTIONS.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
                    </select>
                  ) : (
                    <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-semibold ${
                      item.status === "Published" ? "bg-emerald-50 text-emerald-600" :
                      item.status === "Scheduled" ? "bg-blue-50 text-blue-600" :
                      item.status === "Awaiting Approval" ? "bg-amber-50 text-amber-600" :
                      "bg-surface-container-high text-on-surface-variant"
                    }`}>
                      <span className={`w-1.5 h-1.5 rounded-full ${item.status === "Published" ? "bg-emerald-500 animate-pulse" : item.status === "Scheduled" ? "bg-blue-500" : item.status === "Awaiting Approval" ? "bg-amber-500" : "bg-outline"}`} />
                      {item.status}
                    </span>
                  )}
                </div>

                {/* Created / Updated */}
                <div>
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Created</p>
                  <p className="text-body-sm text-on-surface">{new Date(item.createdAt).toLocaleDateString("en-US", { year: "numeric", month: "long", day: "numeric" })}</p>
                </div>
                <div>
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Last Updated</p>
                  <p className="text-body-sm text-on-surface">{new Date(item.updatedAt).toLocaleDateString("en-US", { year: "numeric", month: "long", day: "numeric" })}</p>
                </div>

                {/* Platforms */}
                <div className="relative">
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Platforms</p>
                  {editing ? (
                    <>
                      <button onClick={() => setShowPlatformPicker(!showPlatformPicker)}
                        className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-3 py-2 text-body-sm text-left text-on-surface hover:border-primary/40 transition-all flex items-center justify-between">
                        <span>{form.platforms.length} selected</span>
                        <span className="material-symbols-outlined text-[14px] text-outline">expand_more</span>
                      </button>
                      {showPlatformPicker && (
                        <>
                          <div className="fixed inset-0 z-10" onClick={() => setShowPlatformPicker(false)} />
                          <div className="absolute left-0 right-0 top-full mt-1 bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-xl z-20 p-2 space-y-0.5 dropdown-enter">
                            {ALL_PLATFORMS.map((p) => (
                              <label key={p} className="flex items-center gap-2.5 px-3 py-2 rounded-lg hover:bg-surface-container cursor-pointer transition-colors">
                                <input type="checkbox" checked={form.platforms.includes(p)} onChange={() => {
                                  setForm((prev) => ({
                                    ...prev,
                                    platforms: prev.platforms.includes(p)
                                      ? prev.platforms.filter((x) => x !== p)
                                      : [...prev.platforms, p],
                                  }));
                                }} className="w-4 h-4 rounded border-outline-variant text-primary focus:ring-primary/30" />
                                <span className="text-label-sm text-on-surface capitalize">{p}</span>
                              </label>
                            ))}
                          </div>
                        </>
                      )}
                    </>
                  ) : (
                    <div className="flex items-center gap-1.5 flex-wrap">
                      {item.platforms.length === 0 ? (
                        <span className="text-body-sm text-outline/50">None</span>
                      ) : item.platforms.map((p) => (
                        <div key={p} className="px-2 py-0.5 rounded-lg bg-surface-container text-label-xs font-semibold flex items-center gap-1" style={{ backgroundColor: (PLATFORM_CONFIG[p]?.color || "#666") + "20", color: PLATFORM_CONFIG[p]?.color || "#666" }}>
                          <PlatformIcon platform={PLATFORM_CONFIG[p]?.icon || "default"} />
                          {p}
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                {/* Hashtags */}
                {item.hashtags && item.hashtags.length > 0 && (
                  <div>
                    <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Hashtags</p>
                    <div className="flex items-center gap-1.5 flex-wrap">
                      {item.hashtags.map((h) => (
                        <span key={h} className="text-[11px] text-primary font-medium">#{h}</span>
                      ))}
                    </div>
                  </div>
                )}

                {/* Caption */}
                {item.caption && (
                  <div>
                    <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Caption</p>
                    <p className="text-body-sm text-on-surface leading-relaxed whitespace-pre-line">{item.caption}</p>
                  </div>
                )}

                {/* CTA Link */}
                {item.ctaLink && (
                  <div>
                    <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">CTA Link</p>
                    <p className="text-body-sm text-primary break-all">{item.ctaLink}</p>
                  </div>
                )}

                {/* Scheduled Date */}
                {item.scheduledAt && (
                  <div>
                    <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Scheduled For</p>
                    <p className="text-body-sm text-on-surface">{new Date(item.scheduledAt).toLocaleString()}</p>
                  </div>
                )}

                {/* Internal Notes */}
                {item.internalNotes && (
                  <div>
                    <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Internal Notes</p>
                    <p className="text-body-sm text-on-surface-variant bg-amber-50 rounded-lg px-3 py-2 leading-relaxed">{item.internalNotes}</p>
                  </div>
                )}

                {/* Type-specific metadata */}
                {item.type === "IMAGE" && item.dimensions && (
                  <div>
                    <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Dimensions</p>
                    <p className="text-body-sm text-on-surface">{item.dimensions}</p>
                  </div>
                )}
                {item.type === "VIDEO" && (
                  <>
                    {item.duration && (
                      <div>
                        <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Duration</p>
                        <p className="text-body-sm text-on-surface">{item.duration}</p>
                      </div>
                    )}
                  </>
                )}
                {item.fileSize && (
                  <div>
                    <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">File Size</p>
                    <p className="text-body-sm text-on-surface">{item.fileSize}</p>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </main>

      {/* Delete Confirmation */}
      {showDelete && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150">
          <div className="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg p-6 w-full max-w-sm mx-4 animate-in fade-in zoom-in-95 duration-200">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-xl bg-danger-red/10 flex items-center justify-center">
                <span className="material-symbols-outlined text-danger-red text-[22px]">delete</span>
              </div>
              <div>
                <h3 className="text-headline-sm text-on-surface font-semibold">Delete Content</h3>
                <p className="text-body-sm text-on-surface-variant">This action cannot be undone</p>
              </div>
            </div>
            <p className="text-body-sm text-on-surface-variant mb-6">
              Are you sure you want to delete <span className="font-semibold text-on-surface">{item.title}</span>? This content will be permanently removed.
            </p>
            <div className="flex justify-end gap-3">
              <button onClick={() => setShowDelete(false)} className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">Cancel</button>
              <button onClick={handleDelete} className="px-5 py-2 rounded-xl bg-danger-red text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2">Delete</button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
