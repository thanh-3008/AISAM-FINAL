"use client";
import React, { useEffect, useState, useRef } from "react";
import { readMedia, saveMedia, uploadMediaItem, importLegacyMedia, type MediaCollection } from "@/services/composerService";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";
import { getUserIdFromToken } from "@/lib/auth";
import { loadComposerFiles, storeComposerFiles, type QueuedFile } from "@/lib/composerFiles";
import MixedMediaGallery from "./MixedMediaGallery";

function createQueuedFileId() {
  return globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(36).slice(2)}`;
}

function createPreviewUrl(file: File) {
  return typeof URL.createObjectURL === "function"
    ? URL.createObjectURL(file)
    : undefined;
}

function normalizeMediaFile(file: File) {
  if (file.type) return file;
  const extension = file.name.split(".").pop()?.toLowerCase();
  const mimeTypes: Record<string, string> = {
    jpg: "image/jpeg", jpeg: "image/jpeg", png: "image/png", webp: "image/webp", gif: "image/gif",
    mp4: "video/mp4", mov: "video/quicktime", webm: "video/webm",
  };
  const type = extension ? mimeTypes[extension] : undefined;
  return type ? new File([file], file.name, { type, lastModified: file.lastModified }) : file;
}

interface MediaComposerProps {
  contentId: string;
  canEdit: boolean;
  onSaved?: () => void;
  onDirtyChange?: (dirty: boolean) => void;
  onCollectionChange?: (collection: MediaCollection) => void;
  compact?: boolean;
  autoSave?: boolean;
  fallbackVideoUrl?: string;
  fallbackImages?: string[];
}

export default function MediaComposer({ contentId, canEdit, onSaved, onDirtyChange, onCollectionChange, compact = false, autoSave = false, fallbackVideoUrl, fallbackImages = [] }: MediaComposerProps) {
  const [collection, setCollection] = useState<MediaCollection | null>(null);
  const [busy, setBusy] = useState(false);
  const [dirty, setDirty] = useState(false);
  const [error, setError] = useState("");
  const [files, setFiles] = useState<QueuedFile[]>([]);
  const [filesReady, setFilesReady] = useState(false);
  const [loadedKey, setLoadedKey] = useState("");
  const [recovery, setRecovery] = useState<MediaCollection | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const replaceIndexRef = useRef<number | null>(null);
  const drag = useRef<number | null>(null);
  const generation = useRef(0);
  const scope = `${getUserIdFromToken()}:${getStoredActiveWorkspace()?.id}:${contentId}`;
  const key = `aisam-media-draft:${scope}`;
  useEffect(() => { onDirtyChange?.(dirty || busy || files.some(f => f.status !== "Done")); }, [dirty, busy, files, onDirtyChange]);
  useEffect(() => {
    let active = true;
    generation.current++; setDirty(false); setError(""); setBusy(false);
    setCollection(null); setRecovery(null); setFiles([]); setFilesReady(false);
    loadComposerFiles(key).then(value => { if (active) setFiles(value); })
      .catch(() => { if (active) setError("Trình duyệt không hỗ trợ khôi phục file; giữ trang mở đến khi upload xong."); })
      .finally(() => { if (active) { setFilesReady(true); setLoadedKey(key); } });
    readMedia(contentId).then(value => { if (!active) return; setCollection(value); if (value.items.length > 0) onCollectionChange?.(value);
      try { const draft = JSON.parse(sessionStorage.getItem(key) ?? "null"); if (draft) setRecovery(draft); } catch { /* damaged local draft */ }
    }).catch(e => { if (active) setError(e.message); });
    return () => { active = false; generation.current++; };
  }, [contentId, key, onCollectionChange]);
  useEffect(() => {
    if (filesReady && loadedKey === key) void storeComposerFiles(key, files).catch(() => setError("Không thể lưu file nháp trên trình duyệt; giữ trang mở để tránh mất file chưa upload."));
  }, [files, filesReady, loadedKey, key]);
  function change(value: MediaCollection) {
    setDirty(true);
    setCollection(value);
    onCollectionChange?.(value);
    try { sessionStorage.setItem(key, JSON.stringify(value)); } catch { setError("Không thể lưu bản nháp trên trình duyệt. Hãy lưu media trước khi đóng trang."); }
  }
  async function add(incoming: FileList | File[]) {
    if (!canEdit || busy || !filesReady || !collection) return;
    const list = Array.from(incoming, normalizeMediaFile);
    if (list.some(f => f.size > 50 * 1024 * 1024 || f.size === 0) || list.reduce((sum,f)=>sum+f.size,0) > 200*1024*1024) { setError("Each file must be between 1 byte and 50MB; each selection is limited to 200MB."); return; }
    const existingCount = collection.items.length || (fallbackVideoUrl ? 1 : 0) + fallbackImages.length;
    if (existingCount + files.filter(f => f.status !== "Done").length + list.length > 10) { setError("A maximum of 10 media files is allowed. Remove a file before adding another."); return; }
    const queued = list.map(file => ({ id: createQueuedFileId(), file, status: "Pending", previewUrl: createPreviewUrl(file) } satisfies QueuedFile));
    setFiles(old => [...old, ...queued]);
    let targetCollection = collection;
    if (collection.items.length === 0) {
      setBusy(true);
      try {
        targetCollection = await importLegacyMedia(contentId, collection.version);
        setCollection(targetCollection);
      } catch (e) {
        setError((e as Error).message);
        setFiles(old => old.map(f => queued.some(q => q.id === f.id)
          ? { ...f, status: "Failed", error: "Could not prepare the current media. Please try again." }
          : f));
        setBusy(false);
        return;
      }
      setBusy(false);
    }
    await upload(queued, targetCollection);
  }
  async function retryFailedFiles() {
    const entries = files.filter(file => file.status === "Failed");
    if (!collection || entries.length === 0 || busy) return;
    let targetCollection = collection;
    if (collection.items.length === 0) {
      setBusy(true); setError("");
      try {
        targetCollection = await importLegacyMedia(contentId, collection.version);
        setCollection(targetCollection);
      } catch (e) {
        setError((e as Error).message); setBusy(false); return;
      }
      setBusy(false);
    }
    await upload(entries, targetCollection);
  }
  function handleFileInput(input: HTMLInputElement) {
    const selected = Array.from(input.files ?? []);
    input.value = "";
    const replaceIndex = replaceIndexRef.current;
    replaceIndexRef.current = null;
    if (selected.length === 0) return;
    if (replaceIndex !== null) void replaceDisplayed(replaceIndex, selected[0]);
    else void add(selected);
  }
  async function upload(entries: QueuedFile[] = files.filter(f => f.status !== "Done"), baseCollection: MediaCollection | null = collection) {
    if (!baseCollection || !canEdit || busy) return;
    const activeGeneration = generation.current;
    setBusy(true); setError("");
    let current = baseCollection;
    let uploadedAny = false;
    for (const entry of entries) {
      if (scope !== `${getUserIdFromToken()}:${getStoredActiveWorkspace()?.id}:${contentId}`) break;
      setFiles(old => old.map(f => f.id === entry.id ? { ...f, status: "Uploading", error: undefined } : f));
      try {
        const item = await uploadMediaItem(contentId, entry.file);
        if (generation.current !== activeGeneration || scope !== `${getUserIdFromToken()}:${getStoredActiveWorkspace()?.id}:${contentId}`) break;
        current = { ...current, items: [...current.items, { ...item, isCover: current.items.length === 0 }] };
        uploadedAny = true;
        change(current);
        setFiles(old => old.map(f => {
          if (f.id !== entry.id) return f;
          if (f.previewUrl) URL.revokeObjectURL(f.previewUrl);
          return { ...f, status: "Done", previewUrl: undefined };
        }));
      } catch (e) { if (generation.current === activeGeneration) setFiles(old => old.map(f => f.id === entry.id ? { ...f, status: "Failed", error: (e as Error).message } : f)); }
    }
    if (generation.current === activeGeneration && autoSave && uploadedAny) {
      try {
        const saved = await saveMedia(contentId, current);
        setCollection(saved); setDirty(false); sessionStorage.removeItem(key); setRecovery(null); onCollectionChange?.(saved); onSaved?.();
      } catch (e) { setError((e as Error).message); }
    }
    if (generation.current === activeGeneration) setBusy(false);
  }
  async function save() {
    if (!collection) return;
    setBusy(true); setError("");
    try { const saved = await saveMedia(contentId, collection); setCollection(saved); setDirty(false); sessionStorage.removeItem(key); setRecovery(null); onSaved?.(); }
    catch (e) { setError(`${(e as Error).message}. Bản nháp vẫn được giữ; nếu nội dung đã thay đổi, tải lại trang để đối chiếu.`); }
    finally { setBusy(false); }
  }
  function move(from: number, to: number) {
    if (!collection || to < 0 || to >= collection.items.length) return;
    const items = [...collection.items]; const [item] = items.splice(from, 1); items.splice(to, 0, item); change({ ...collection, items });
  }
  async function remove(index: number) {
    if (!collection || busy) return;
    const removed = collection.items[index];
    const items = collection.items.filter((_, itemIndex) => itemIndex !== index);
    if (removed.isCover && items.length > 0) items[0] = { ...items[0], isCover: true };
    const next = { ...collection, items };
    change(next);
    if (!autoSave) return;
    setBusy(true); setError("");
    try {
      const saved = await saveMedia(contentId, next);
      setCollection(saved); setDirty(false); sessionStorage.removeItem(key); onCollectionChange?.(saved); onSaved?.();
    } catch (e) { setError((e as Error).message); }
    finally { setBusy(false); }
  }
  async function removeDisplayed(index: number) {
    if (!collection || busy) return;
    if (collection.items.length > 0) {
      await remove(index);
      return;
    }

    const displayed = [
      ...(fallbackVideoUrl ? [{ url: fallbackVideoUrl }] : []),
      ...fallbackImages.map(url => ({ url })),
    ];
    const targetUrl = displayed[index]?.url;
    if (!targetUrl) return;

    setBusy(true); setError("");
    try {
      const imported = await importLegacyMedia(contentId, collection.version);
      const importedIndex = imported.items.findIndex(item => item.url === targetUrl);
      if (importedIndex < 0) throw new Error("The selected media could not be found.");
      const removed = imported.items[importedIndex];
      const items = imported.items.filter((_, itemIndex) => itemIndex !== importedIndex);
      if (removed.isCover && items.length > 0) items[0] = { ...items[0], isCover: true };
      const next = { ...imported, items };
      if (!autoSave) {
        change(next);
        return;
      }
      const saved = await saveMedia(contentId, next);
      setCollection(saved); setDirty(false); sessionStorage.removeItem(key); onCollectionChange?.(saved); onSaved?.();
    } catch (e) { setError((e as Error).message); }
    finally { setBusy(false); }
  }
  async function replaceDisplayed(index: number, incoming: File) {
    if (!collection || busy) return;
    const file = normalizeMediaFile(incoming);
    if (!file.type.startsWith("image/") || file.size <= 0 || file.size > 50 * 1024 * 1024) {
      setError("Choose a valid image up to 50MB.");
      return;
    }

    setBusy(true); setError("");
    try {
      let target = collection;
      let targetIndex = index;
      if (target.items.length === 0) {
        const displayed = [
          ...(fallbackVideoUrl ? [{ url: fallbackVideoUrl }] : []),
          ...fallbackImages.map(url => ({ url })),
        ];
        const targetUrl = displayed[index]?.url;
        if (!targetUrl) throw new Error("The selected image could not be found.");
        target = await importLegacyMedia(contentId, target.version);
        targetIndex = target.items.findIndex(item => item.url === targetUrl);
      }
      if (targetIndex < 0 || !target.items[targetIndex]?.mimeType.startsWith("image/")) {
        throw new Error("The selected image could not be found.");
      }
      const uploaded = await uploadMediaItem(contentId, file);
      const previous = target.items[targetIndex];
      const items = target.items.map((item, itemIndex) => itemIndex === targetIndex
        ? { ...uploaded, sortOrder: previous.sortOrder, isCover: previous.isCover, altText: previous.altText, caption: previous.caption }
        : item);
      const next = { ...target, items };
      if (!autoSave) {
        change(next);
        return;
      }
      const saved = await saveMedia(contentId, next);
      setCollection(saved); setDirty(false); sessionStorage.removeItem(key); onCollectionChange?.(saved); onSaved?.();
    } catch (e) { setError((e as Error).message); }
    finally { setBusy(false); }
  }
  const pendingFiles = files.filter(f => f.status !== "Done");
  return <section className="space-y-3" aria-label="Media collection">
    {!compact && <div className="flex items-center justify-between gap-3">
      <div>
        <h3 className="text-label-sm font-semibold text-on-surface-variant">Media</h3>
      </div>
      <span className="rounded-full bg-surface-container px-2.5 py-1 text-label-xs font-semibold text-outline">
        {collection?.items.length ?? 0}/10
      </span>
    </div>}

    {error && <p role="alert" className="rounded-lg bg-red-50 px-3 py-2 text-label-xs text-red-600">{error}</p>}
    {recovery && <div className="flex flex-wrap items-center gap-2 rounded-lg bg-amber-50 px-3 py-2 text-label-xs text-amber-800">
      <span>Tìm thấy bản nháp có {recovery.items.length} file.</span>
      <button type="button" className="font-semibold underline" disabled={!canEdit || busy} onClick={() => { change({ ...recovery, version: collection!.version }); setRecovery(null); }}>Khôi phục</button>
      <button type="button" className="font-semibold underline" onClick={() => { sessionStorage.removeItem(key); setRecovery(null); }}>Bỏ qua</button>
    </div>}

    {!compact && collection && collection.items.length > 0 && <ol className="grid grid-cols-2 gap-3 sm:grid-cols-3">
      {collection.items.map((item, index) => <li key={item.assetId} draggable={canEdit && !busy} onDragStart={() => { drag.current = index; }} onDragOver={e => e.preventDefault()} onDrop={e => { e.preventDefault(); e.stopPropagation(); if (drag.current !== null && canEdit && !busy) move(drag.current, index); drag.current = null; }} className="group relative overflow-hidden rounded-lg border border-outline-variant/20 bg-surface-container">
        <div className="aspect-square">
          {item.mimeType.startsWith("video/") ? <video src={item.url} controls preload="metadata" className="h-full w-full object-cover" /> : <img src={item.url} alt={item.altText ?? `Media ${index + 1}`} className="h-full w-full object-cover" />}
        </div>
        <span className="absolute left-2 top-2 rounded bg-black/60 px-1.5 py-0.5 text-[11px] font-semibold text-white">{item.isCover ? "Ảnh bìa" : index + 1}</span>
        {canEdit && <div className="flex items-center justify-center gap-1 border-t border-outline-variant/20 bg-surface-container-lowest p-1.5">
          <button type="button" title="Lên" aria-label="Lên" className="material-symbols-outlined rounded p-1 text-[18px] disabled:opacity-30" disabled={busy || index === 0} onClick={() => move(index, index - 1)}>arrow_back</button>
          <button type="button" title="Xuống" aria-label="Xuống" className="material-symbols-outlined rounded p-1 text-[18px] disabled:opacity-30" disabled={busy || index === collection.items.length - 1} onClick={() => move(index, index + 1)}>arrow_forward</button>
          <button type="button" title="Đặt làm ảnh bìa" aria-label="Đặt cover" className="material-symbols-outlined rounded p-1 text-[18px] disabled:opacity-30" disabled={busy || item.isCover} onClick={() => change({ ...collection, items: collection.items.map(m => ({ ...m, isCover: m.assetId === item.assetId })) })}>star</button>
          <button type="button" title="Xóa" aria-label="Bỏ media" className="material-symbols-outlined rounded p-1 text-[18px] text-red-500 disabled:opacity-30" disabled={busy} onClick={() => change({ ...collection, items: collection.items.filter(m => m.assetId !== item.assetId) })}>delete</button>
        </div>}
      </li>)}
    </ol>}

    {canEdit && <>
      <input
        ref={node => {
          fileInputRef.current = node;
          if (node) node.onchange = () => handleFileInput(node);
        }}
        className="hidden"
        aria-label="Media files"
        type="file"
        multiple
        accept="image/jpeg,image/png,image/webp,image/gif,video/mp4,video/webm,video/quicktime"
        disabled={busy || !collection || !filesReady}
      />
      <button
        type="button"
        disabled={busy || !collection || !filesReady}
        onClick={() => {
          if (!fileInputRef.current) return;
          fileInputRef.current.value = "";
          fileInputRef.current.click();
        }}
        onDragOver={e => e.preventDefault()}
        onDrop={e => { e.preventDefault(); if (!busy) void add(Array.from(e.dataTransfer.files)); }}
        className="flex w-full items-center gap-3 rounded-xl border-2 border-dashed border-outline-variant/30 px-4 py-3 text-left transition-colors hover:border-primary/40 hover:bg-surface-container/50 disabled:cursor-not-allowed disabled:opacity-60"
      >
      <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10">
        {busy ? <span className="h-4 w-4 animate-spin rounded-full border-2 border-primary/30 border-t-primary" /> : <span className="material-symbols-outlined text-[18px] text-primary">add_photo_alternate</span>}
      </span>
      <span className="min-w-0 text-left">
        <span className="block text-label-sm font-semibold text-on-surface">{busy ? "Uploading..." : "Add Image or Video"}</span>
        <span className="block text-label-xs text-outline">PNG, JPG, WebP, GIF, MP4 · up to 50MB</span>
      </span>
      {compact && <span className="ml-auto shrink-0 rounded-full bg-surface-container px-2.5 py-1 text-label-xs font-semibold text-outline">
        {(collection?.items.length || (fallbackVideoUrl ? 1 : 0) + fallbackImages.length) + pendingFiles.length}/10
      </span>}
      </button>
    </>}

    {compact && <MixedMediaGallery
      videoUrl={collection?.items.length ? undefined : fallbackVideoUrl}
      images={collection?.items.length ? [] : fallbackImages}
      media={[
        ...(collection?.items.length ? collection.items : [
          ...(fallbackVideoUrl ? [{ url: fallbackVideoUrl, mimeType: "video/legacy" }] : []),
          ...fallbackImages.map(url => ({ url, mimeType: "image/legacy" })),
        ]),
        ...pendingFiles.filter(file => file.previewUrl).map(file => ({ url: file.previewUrl!, mimeType: file.file.type })),
      ]}
      onReplace={canEdit && !busy && pendingFiles.length === 0 ? index => {
        replaceIndexRef.current = index;
        if (!fileInputRef.current) return;
        fileInputRef.current.value = "";
        fileInputRef.current.click();
      } : undefined}
      onRemove={canEdit && !busy && pendingFiles.length === 0 ? index => void removeDisplayed(index) : undefined}
    />}

    {pendingFiles.length > 0 && <ul className="space-y-2 text-label-xs text-on-surface-variant">{pendingFiles.map(f => <li className="flex items-center gap-3 rounded-lg border border-outline-variant/20 p-2" key={f.id}>
      {f.previewUrl ? <img src={f.previewUrl} alt={f.file.name} className="h-14 w-14 shrink-0 rounded-md object-cover" /> : <span className="material-symbols-outlined flex h-14 w-14 shrink-0 items-center justify-center rounded-md bg-surface-container text-outline">movie</span>}
      <span className="min-w-0 flex-1 truncate">{f.file.name}<span className={`block ${f.status === "Failed" ? "text-red-600" : "text-outline"}`}>{f.status === "Failed" ? f.error : "Uploading..."}</span></span>
      {!busy && <button type="button" className="shrink-0 text-red-500" onClick={() => setFiles(old => old.filter(v => { if (v.id === f.id && v.previewUrl) URL.revokeObjectURL(v.previewUrl); return v.id !== f.id; }))}>Remove</button>}
    </li>)}</ul>}
    {busy && <progress aria-label="Processing media" className="h-1 w-full" />}

    {canEdit && <div className="flex flex-wrap justify-end gap-2">
      {files.some(f => f.status === "Failed") && <button type="button" className="rounded-lg border border-outline-variant/30 px-3 py-2 text-label-sm font-semibold" disabled={busy || !collection} onClick={() => void retryFailedFiles()}>Retry failed files</button>}
      {!compact && collection?.items.length === 0 && <button type="button" className="rounded-lg border border-outline-variant/30 px-3 py-2 text-label-sm font-semibold" disabled={busy} onClick={async () => { setBusy(true); try { setCollection(await importLegacyMedia(contentId, collection.version)); onSaved?.(); } catch (e) { setError((e as Error).message); } finally { setBusy(false); } }}>Dùng media cũ</button>}
      {!autoSave && dirty && <button type="button" className="rounded-lg bg-primary px-4 py-2 text-label-sm font-semibold text-on-primary disabled:opacity-60" disabled={busy || !collection || pendingFiles.length > 0} onClick={save}>Lưu thay đổi</button>}
    </div>}
  </section>;
}
