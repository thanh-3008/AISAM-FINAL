"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import { usePublishPermission } from "@/hooks/usePublishPermission";
import { useResourcePermissions } from "@/hooks/useResourcePermissions";
import { Kind, Permission } from "@/services/permissionService";
import { useRbac } from "@/contexts/RbacContext";
import { apiClient } from "@/lib/apiClient";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import Header from "@/components/layout/Header";
import PostNowModal from "@/components/content/PostNowModal";
import type { ContentDetail, ContentType, ContentStatus } from "@/services/contentService";
import { PLATFORM_CONFIG, ALL_PLATFORMS, STATUS_OPTIONS, getTypeStyle, getTypeIcon, PlatformIcon } from "@/lib/contentConstants";
import { fetchContentById, updateContent, updateContentWithResult, deleteContent, CONTENTTYPE_TO_ADTYPE, fetchContentGenerations, submitForApproval, AiGenerationResponse, parseMultipleImageUrls } from "@/services/contentService";
import { useFeatureGate } from "@/hooks/useFeatureGate";
import RichTextPreview from "@/components/content/RichTextPreview";
import RichTextEditor from "@/components/content/RichTextEditor";
import MediaComposer from "@/components/content/MediaComposer";

interface FormState {
  title: string;
  status: ContentStatus;
  description: string;
  platforms: string[];
  caption: string;
  richTextJson?: string | null;
  ctaLink: string;
  scheduledAt: string;
  internalNotes: string;
  hashtags: string[];
  rejectionReason?: string;
  imageUrls: string[];
}

export default function ContentDetailPage() {
  const params = useParams();
  const router = useRouter();
  const searchParams = useSearchParams();
  const isEditParam = searchParams.get("edit") === "true";
  const featureGate = useFeatureGate();
  const rbac = useRbac();

  const allowed = useResourcePermissions([Permission.ContentEdit, Permission.ContentDelete, Permission.ApprovalWithdraw].map(permission => ({ kind: Kind.Content, resourceId: String(params.id), permission })));
  const [mediaDirty, setMediaDirty] = useState(false);
  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [showDelete, setShowDelete] = useState(false);
  const [showPlatformPicker, setShowPlatformPicker] = useState(false);
  const [showPostNow, setShowPostNow] = useState(false);
  const [visible, setVisible] = useState(false);
  const [toast, setToast] = useState<{ message: string; type: "success" | "error" } | null>(null);

  const [item, setItem] = useState<ContentDetail | null>(null);
  const canPublish = usePublishPermission(String(params.id), item?.brandId);
  const canManageSchedules = rbac ? canPublish : featureGate.can("manageSchedules");
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [generations, setGenerations] = useState<AiGenerationResponse[]>([]);

  const [form, setForm] = useState<FormState>({ title: "", status: "Draft", description: "", platforms: [], caption: "", ctaLink: "", scheduledAt: "", internalNotes: "", hashtags: [], imageUrls: [] });

  const notFound = !item && !loading;

  const formRef = useRef(form);
  const itemRef = useRef(item);

  const handleMediaSaved = useCallback(async () => {
    const refreshed = await fetchContentById(String(params.id));
    if (!refreshed) {
      setItem(p => p ? { ...p, status: "Draft" } : p);
      return;
    }
    const refreshedImages = refreshed.imageUrls?.length
      ? refreshed.imageUrls
      : parseMultipleImageUrls(refreshed.imageUrl);
    setItem(refreshed as any);
    setForm(previous => {
      const next = { ...previous, status: "Draft" as ContentStatus, imageUrls: refreshedImages };
      formRef.current = next;
      return next;
    });
  }, [params.id]);

  const updateForm = useCallback((partial: Partial<FormState>) => {
    formRef.current = { ...formRef.current, ...partial };
    setForm((prev) => ({ ...prev, ...partial }));
  }, []);

  const handleMediaCollectionChange = useCallback((media: { items: Array<{ url: string; mimeType: string }> }) => {
    updateForm({ imageUrls: media.items.filter(entry => entry.mimeType.startsWith("image/")).map(entry => entry.url) });
  }, [updateForm]);

  useEffect(() => {
    formRef.current = form;
    itemRef.current = item;
  }, [form, item]);

  useEffect(() => { const t = setTimeout(() => setVisible(true), 80); return () => clearTimeout(t); }, []);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      const result = await fetchContentById(params.id as string);
      setItem(result as any);
      const gens = await fetchContentGenerations(params.id as string);
      setGenerations(gens);
      setLoading(false);
    };
    load();
  }, [params.id]);

  useEffect(() => {
    const hasProcessing = generations.some((g) => g.status === 0 || g.status === 1);
    if (!hasProcessing) return;

    const poll = async () => {
      const gens = await fetchContentGenerations(params.id as string);
      if (gens.length > 0) setGenerations(gens);
    };

    const interval = setInterval(poll, 10000);
    return () => clearInterval(interval);
  }, [generations, params.id]);

  const autoEditTriggeredRef = useRef(false);

  useEffect(() => {
    autoEditTriggeredRef.current = false;
  }, [item?.id]);

  useEffect(() => {
    if (item) {
      const initialImages = (item.imageUrls && item.imageUrls.length > 0)
        ? item.imageUrls
        : parseMultipleImageUrls(item.imageUrl);

      const initialForm: FormState = {
        title: item.title,
        status: item.status,
        description: item.description || "",
        platforms: [...item.platforms],
        caption: item.caption || item.textContent || "",
        richTextJson: item.richTextJson,
        ctaLink: item.ctaLink || "",
        scheduledAt: item.scheduledAt || "",
        internalNotes: item.internalNotes || "",
        hashtags: item.hashtags || [],
        rejectionReason: item.rejectionReason || "",
        imageUrls: initialImages,
      };
      formRef.current = initialForm;
      setForm(initialForm);
    }
  }, [item?.id]);

  useEffect(() => {
    if (!isEditParam || autoEditTriggeredRef.current) return;
    if (!item || !allowed.isReady) return;

    autoEditTriggeredRef.current = true;
    if (allowed(0) && item.status !== "Approved") {
      setEditing(true);
    } else if (item.status === "Approved") {
      setToast({
        message: "Nội dung đã duyệt. Vui lòng bấm 'Thu hồi duyệt để sửa' nếu muốn thay đổi.",
        type: "error",
      });
    } else if (item.status === "Awaiting Approval") {
      setToast({
        message: "Nội dung đang chờ duyệt, không thể chỉnh sửa.",
        type: "error",
      });
    } else if (item.status === "Published" || item.status === "Scheduled") {
      setToast({
        message: "Nội dung đã lên lịch hoặc xuất bản, không thể chỉnh sửa trực tiếp.",
        type: "error",
      });
    } else {
      setToast({
        message: "Bạn không có quyền chỉnh sửa nội dung này.",
        type: "error",
      });
    }
  }, [isEditParam, item?.id, item?.status, allowed.isReady, allowed]);

  const handleWithdrawAndEdit = async () => {
    if (!item) return;
    setSaving(true);
    try {
      await apiClient(`/content/${item.id}/withdraw`, { method: "POST" });
      const refreshed = await fetchContentById(item.id);
      if (refreshed) {
        setItem(refreshed);
        const refreshedImages = (refreshed.imageUrls && refreshed.imageUrls.length > 0)
          ? refreshed.imageUrls
          : parseMultipleImageUrls(refreshed.imageUrl);
        const refreshedForm: FormState = {
          title: refreshed.title,
          status: refreshed.status,
          description: refreshed.description || "",
          platforms: [...refreshed.platforms],
          caption: refreshed.caption || refreshed.textContent || "",
          richTextJson: refreshed.richTextJson,
          ctaLink: refreshed.ctaLink || "",
          scheduledAt: refreshed.scheduledAt || "",
          internalNotes: refreshed.internalNotes || "",
          hashtags: refreshed.hashtags || [],
          rejectionReason: refreshed.rejectionReason || "",
          imageUrls: refreshedImages,
        };
        formRef.current = refreshedForm;
        setForm(refreshedForm);
      }
      window.dispatchEvent(new Event("aisam-permissions-changed"));
      setEditing(true);
      setToast({ message: "Đã thu hồi phê duyệt. Bạn có thể chỉnh sửa bài viết ngay bây giờ.", type: "success" });
    } catch (error) {
      setToast({ type: "error", message: error instanceof Error ? error.message : "Không thu hồi được duyệt." });
    } finally {
      setSaving(false);
    }
  };

  const handleSave = async () => {
    if (!allowed(0)) {
      setToast({ message: "Bạn không có quyền chỉnh sửa bài viết này hoặc cần thu hồi duyệt trước.", type: "error" });
      return;
    }
    if (typeof window !== "undefined") {
      window.dispatchEvent(new CustomEvent("aisam-flush-editor"));
    }
    const currentForm = formRef.current;
    setSaving(true);
    const result = await updateContentWithResult(params.id as string, {
      title: currentForm.title,
      adType: item ? CONTENTTYPE_TO_ADTYPE[item.type] : undefined,
      textContent: currentForm.caption,
      richTextJson: currentForm.richTextJson,
      richTextVersion: currentForm.richTextJson ? 1 : null,
      contextDescription: currentForm.description,
      imageUrls: item?.type === "IMAGE" || item?.type === "VIDEO" ? currentForm.imageUrls : undefined,
      imageUrl: (item?.type === "IMAGE" || item?.type === "VIDEO") && currentForm.imageUrls.length > 0 ? currentForm.imageUrls[0] : undefined,
    });
    if (result.success) {
      setEditing(false);
      setToast({ message: "Lưu thay đổi bài viết thành công!", type: "success" });
      const refreshed = await fetchContentById(params.id as string);
      if (refreshed) {
        setItem(refreshed);
      }
    } else {
      setToast({ message: result.error || "Lưu bài viết thất bại. Vui lòng kiểm tra lại.", type: "error" });
    }
    setSaving(false);
  };

  const handleDelete = async () => {
    if (!allowed(1)) return;
    const ok = await deleteContent(params.id as string);
    if (ok) router.push("/content");
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    const result = await submitForApproval(params.id as string);
    setIsSubmitting(false);
    if (result.success) {
      setToast({ message: "Content submitted for approval successfully.", type: "success" });
      if (item) {
        const updatedItem = { ...item, status: "Awaiting Approval" as ContentStatus };
        setItem(updatedItem);
        // Synchronize form status to prevent autosave cleanup from overwriting server status
        setForm(prev => ({ ...prev, status: "Awaiting Approval" }));
      }
    } else {
      setToast({ message: result.error || "Failed to submit content.", type: "error" });
    }
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

        {form.status === "Rejected" && form.rejectionReason && (
          <div className={`bg-danger-red/10 border border-danger-red/20 rounded-xl p-4 flex gap-3 text-danger-red ${visible ? "animate-fade-up" : ""}`}>
            <span className="material-symbols-outlined text-[20px] shrink-0 mt-0.5">error</span>
            <div>
              <h4 className="text-label-sm font-bold mb-1">Content needs revision</h4>
              <p className="text-body-sm leading-relaxed whitespace-pre-wrap">{form.rejectionReason}</p>
            </div>
          </div>
        )}

        {/* Notice banner for Approved status */}
        {item.status === "Approved" && !editing && (
          <div className={`bg-amber-50/90 border border-amber-200/90 rounded-2xl p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3 text-amber-900 ${visible ? "animate-fade-up" : ""}`}>
            <div className="flex items-center gap-3">
              <span className="material-symbols-outlined text-amber-600 text-[24px] shrink-0">verified</span>
              <div>
                <h4 className="text-label-sm font-bold text-amber-950">Bài viết đã được phê duyệt (Approved)</h4>
                <p className="text-body-sm text-amber-900/80">Quyền chỉnh sửa được tạm khóa theo quy trình xét duyệt. Bạn có thể thu hồi duyệt để sửa lại nội dung hoặc hình ảnh.</p>
              </div>
            </div>
            <button
              onClick={handleWithdrawAndEdit}
              disabled={saving}
              className="px-4 py-2 rounded-xl bg-amber-600 text-white text-label-sm font-semibold hover:bg-amber-700 transition-all shrink-0 active:scale-[0.97] flex items-center justify-center gap-1.5 shadow-sm"
            >
              <span className="material-symbols-outlined text-[16px]">undo</span>
              Thu hồi duyệt &amp; Sửa bài
            </button>
          </div>
        )}

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
                {item.status === "Approved" && (
                  <button
                    disabled={saving}
                    onClick={handleWithdrawAndEdit}
                    className="px-4 py-2 rounded-xl bg-amber-500/15 text-amber-800 border border-amber-300 hover:bg-amber-500/25 transition-all active:scale-[0.97] text-label-sm font-semibold flex items-center gap-1.5"
                    title="Thu hồi duyệt để mở khóa quyền chỉnh sửa bài viết"
                  >
                    <span className="material-symbols-outlined text-[16px]">edit_note</span>
                    Thu hồi duyệt để sửa
                  </button>
                )}
                {item.status === "Approved" && (canPublish || canManageSchedules) && (
                  <>
                    {canPublish && <button disabled={mediaDirty} onClick={() => setShowPostNow(true)}
                      className="px-4 py-2 rounded-xl bg-primary text-on-primary text-label-sm font-semibold hover:shadow-lg active:scale-[0.97] transition-all flex items-center gap-1.5">
                      <span className="material-symbols-outlined text-[16px]">send</span>
                      Post Now
                    </button>}
                    {canManageSchedules && <button disabled={mediaDirty} onClick={() => router.push(`/calendar?contentId=${item.id}`)}
                      className="px-4 py-2 rounded-xl border border-outline-variant/20 text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-all active:scale-[0.97] text-label-sm font-semibold flex items-center gap-1.5">
                      <span className="material-symbols-outlined text-[16px]">calendar_month</span>
                      Schedule
                    </button>}
                  </>
                )}
                {(item.status === "Draft" || item.status === "Rejected") && (
                  <button onClick={handleSubmit} disabled={mediaDirty || isSubmitting || !allowed(0)}
                    className="px-4 py-2 rounded-xl bg-amber-500 text-white text-label-sm font-semibold hover:bg-amber-600 transition-all active:scale-[0.97] disabled:opacity-60 flex items-center gap-1.5">
                    {isSubmitting ? (
                      <span className="w-3.5 h-3.5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    ) : (
                      <span className="material-symbols-outlined text-[16px]">send</span>
                    )}
                    Submit for Approval
                  </button>
                )}
                {item.status !== "Approved" && (
                  <button
                    disabled={!allowed(0)}
                    onClick={() => setEditing(true)}
                    title={!allowed(0) ? (!allowed.isReady ? "Đang kiểm tra quyền..." : (item.status === "Awaiting Approval" ? "Nội dung đang chờ duyệt, không thể chỉnh sửa." : (item.status === "Published" || item.status === "Scheduled" ? "Nội dung đã lên lịch hoặc xuất bản, không thể chỉnh sửa trực tiếp." : "Bạn không có quyền chỉnh sửa bài viết này."))) : undefined}
                    className="px-4 py-2 rounded-xl border border-outline-variant/20 text-on-surface-variant hover:bg-surface-container hover:text-on-surface transition-all active:scale-[0.97] text-label-sm font-semibold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-transparent"
                  >
                    <span className="material-symbols-outlined text-[16px]">edit</span>
                    Edit
                  </button>
                )}
                <button
                  disabled={!allowed(1)}
                  onClick={() => setShowDelete(true)}
                  title={!allowed(1) ? (!allowed.isReady ? "Đang kiểm tra quyền..." : (item.status === "Published" || item.status === "Scheduled" ? "Không thể xóa nội dung đã lên lịch hoặc xuất bản." : "Bạn không có quyền xóa bài viết này.")) : undefined}
                  className="px-4 py-2 rounded-xl border border-danger-red/20 text-danger-red hover:bg-danger-red/5 transition-all active:scale-[0.97] text-label-sm font-semibold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-transparent"
                >
                  <span className="material-symbols-outlined text-[16px]">delete</span>
                  Delete
                </button>
              </>
            ) : (
              <>
                <button onClick={() => {
                  setEditing(false);
                  if (item) {
                    const origImages = (item.imageUrls && item.imageUrls.length > 0)
                      ? item.imageUrls
                      : parseMultipleImageUrls(item.imageUrl);
                    setForm({
                      title: item.title,
                      status: item.status,
                      description: item.description || "",
                      platforms: [...item.platforms],
                      caption: item.caption || item.textContent || "",
                      richTextJson: item.richTextJson,
                      ctaLink: item.ctaLink || "",
                      scheduledAt: item.scheduledAt || "",
                      internalNotes: item.internalNotes || "",
                      hashtags: item.hashtags || [],
                      rejectionReason: item.rejectionReason || "",
                      imageUrls: origImages,
                    });
                  }
                }}
                  className="px-4 py-2 rounded-xl border border-outline-variant/20 text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97] text-label-sm font-semibold">
                  Cancel
                </button>
                <button onClick={handleSave} disabled={saving || !allowed(0)}
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
          <div className={`flex-1 min-w-0 ${item.type === "VIDEO" ? 'grid grid-cols-1 lg:grid-cols-12 gap-gutter' : 'space-y-gutter'}`}>
            {/* Description */}
            <div className={`lg:col-span-4 bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.08s" }}>
              <div className="p-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-label-md text-on-surface font-semibold">Description</h3>
                </div>
                {editing ? (
                  <textarea value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-xl p-4 text-body-sm text-on-surface placeholder:text-outline/30 focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all min-h-25 resize-y"
                    placeholder="Add a description..." />
                ) : (
                  <p className="text-body-sm text-on-surface-variant leading-relaxed">{item.description || "No description provided."}</p>
                )}
              </div>
            </div>

            {/* Content Preview */}
            <div className={`lg:col-span-8 bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.16s" }}>
              <div className="p-6">
                <div className="flex items-center gap-2 mb-4">
                  <span className="text-label-sm text-outline font-semibold uppercase tracking-wider">Preview</span>
                  <span className="px-2 py-0.5 rounded-md bg-linear-to-br text-white text-label-xs font-semibold flex items-center gap-1" style={{ background: `linear-gradient(135deg, var(--color-${item.type === "IMAGE" ? "blue" : item.type === "TEXT" ? "purple" : "rose"}-500), var(--color-${item.type === "IMAGE" ? "blue" : item.type === "TEXT" ? "purple" : "rose"}-400))` }}>
                    <span className="material-symbols-outlined text-label-xs">{typeIcon}</span>
                    {item.type}
                  </span>
                </div>

                {(item.type === "IMAGE" || item.type === "VIDEO") && <MediaComposer
                  compact
                  autoSave
                  contentId={String(params.id)}
                  canEdit={editing && allowed(0)}
                  fallbackVideoUrl={item.videoUrl}
                  fallbackImages={form.imageUrls}
                  onCollectionChange={handleMediaCollectionChange}
                  onDirtyChange={setMediaDirty}
                  onSaved={handleMediaSaved}
                />}

                {item.type === "TEXT" && <MediaComposer contentId={String(params.id)} canEdit={allowed(0)} onDirtyChange={setMediaDirty} onSaved={handleMediaSaved} />}
                {item.type === "TEXT" && (
                  <div className="w-full max-w-2xl mx-auto">
                    <div className="bg-surface-container rounded-xl p-6 min-h-50">
                      {editing ? (
                        <RichTextEditor
                          value={form.caption}
                          richTextJson={form.richTextJson}
                          onChange={(caption, richTextJson) => updateForm({ caption, richTextJson })}
                          placeholder="Write your content..."
                          minHeight={200}
                        />
                      ) : (
                        <RichTextPreview
                          content={item.textContent || ""} richTextJson={item.richTextJson}
                          className="text-body-md"
                        />
                      )}
                    </div>
                    <div className="flex items-center gap-4 mt-3 text-label-xs text-outline">
                      <span>{(item.textContent || "").length} characters</span>
                      {item.textContent && <span>{item.textContent.split(/\s+/).length} words</span>}
                    </div>
                  </div>
                )}

                {item.type === "VIDEO" && (
                  <div className="flex items-center gap-4 mt-3 text-label-xs text-outline">
                    {item.duration && <span>{item.duration}</span>}
                    {item.fileSize && <span>{item.fileSize}</span>}
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Sidebar Metadata */}
          <div className="w-full xl:w-80 shrink-0 space-y-gutter">
            {/* AI Generation Status */}
            {generations.length > 0 && (
              <div className={`bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm overflow-hidden ${visible ? "animate-fade-up" : ""}`} style={{ animationDelay: "0.10s" }}>
                <div className="p-5 space-y-3">
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider">AI Generation Status</p>
                  {generations.map((gen, idx) => (
                    <div key={gen.id || idx} className="p-3 rounded-lg border border-outline-variant/20 bg-surface-container-high/30">
                      <div className="flex items-center justify-between mb-1.5">
                        <span className="text-label-sm font-semibold text-on-surface">Job #{idx + 1}</span>
                        <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                          gen.status === 2 ? "bg-emerald-100 text-emerald-700" :
                          gen.status === 3 ? "bg-red-100 text-red-700" :
                          "bg-blue-100 text-blue-700 animate-pulse"
                        }`}>
                          {gen.status === 0 ? "Pending" : gen.status === 1 ? "Processing" : gen.status === 2 ? "Completed" : "Failed"}
                        </span>
                      </div>
                      <p className="text-[11px] text-outline">
                        Started: {new Date(gen.createdAt).toLocaleString()}
                      </p>
                      {gen.errorMessage && (
                        <p className="mt-2 text-[11px] text-red-600 font-medium p-1.5 bg-red-50 rounded">
                          {gen.errorMessage}
                        </p>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}

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

                <div>
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Created by</p>
                  <p className="text-body-sm text-on-surface flex items-center gap-1.5">
                    <span className="material-symbols-outlined text-[17px] text-outline">person</span>
                    {item.creatorName}
                  </p>
                </div>

                {/* Status */}
                <div>
                  <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Status</p>
                  <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-semibold ${
                    item.status === "Published" ? "bg-emerald-50 text-emerald-600" :
                    item.status === "Scheduled" ? "bg-blue-50 text-blue-600" :
                    item.status === "Awaiting Approval" ? "bg-amber-50 text-amber-600" :
                    "bg-surface-container-high text-on-surface-variant"
                  }`}>
                    <span className={`w-1.5 h-1.5 rounded-full ${item.status === "Published" ? "bg-emerald-500 animate-pulse" : item.status === "Scheduled" ? "bg-blue-500" : item.status === "Awaiting Approval" ? "bg-amber-500" : "bg-outline"}`} />
                    {item.status}
                  </span>
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

                {/* Caption - only shown for non-TEXT content (TEXT content is edited in the main body above) */}
                {item.type !== "TEXT" && (item.caption || item.textContent || editing) && (
                  <div>
                    <p className="text-label-xs text-outline font-semibold uppercase tracking-wider mb-1.5">Caption</p>
                    {editing ? (
                      <RichTextEditor
                        value={form.caption}
                        richTextJson={form.richTextJson}
                        onChange={(caption, richTextJson) => updateForm({ caption, richTextJson })}
                        placeholder="Write a caption..."
                      />
                    ) : (
                      <RichTextPreview
                        content={item.caption || item.textContent || ""}
                        richTextJson={item.richTextJson}
                        className="text-body-sm text-on-surface leading-relaxed"
                      />
                    )}
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

      {/* Post Now Modal */}
      {showPostNow && item && canPublish && (
        <PostNowModal
          contentId={item.id}
          brandId={item.brandId}
          onClose={() => setShowPostNow(false)}
          onSuccess={() => {
            setShowPostNow(false);
            setToast({ message: "Post published successfully!", type: "success" });
            fetchContentById(params.id as string).then((result) => setItem(result as any));
          }}
        />
      )}

      {/* Toast */}
      {toast && (
        <div className="fixed bottom-8 left-1/2 -translate-x-1/2 z-100 flex items-center gap-3 bg-inverse-surface text-inverse-on-surface px-5 py-3 rounded-xl shadow-2xl animate-in fade-in slide-in-from-bottom-2 duration-300">
          <span className={`material-symbols-outlined text-[18px] ${toast.type === "success" ? "text-emerald-400" : "text-danger-red"}`}>
            {toast.type === "success" ? "check_circle" : "error"}
          </span>
          <p className="text-label-sm font-semibold">{toast.message}</p>
          <button onClick={() => setToast(null)} className="p-1 hover:bg-white/10 rounded-full transition-all">
            <span className="material-symbols-outlined text-[14px]">close</span>
          </button>
        </div>
      )}
    </>
  );
}
