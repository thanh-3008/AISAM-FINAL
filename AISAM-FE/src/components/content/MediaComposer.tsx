"use client";
import React, { useEffect, useState, useRef } from "react";
import { readMedia, saveMedia, uploadMediaItem, importLegacyMedia, type MediaCollection } from "@/services/composerService";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";
import { getUserIdFromToken } from "@/lib/auth";
import { loadComposerFiles, storeComposerFiles, type QueuedFile } from "@/lib/composerFiles";

export default function MediaComposer({ contentId, canEdit, onSaved, onDirtyChange }: { contentId: string; canEdit: boolean; onSaved?: () => void; onDirtyChange?: (dirty: boolean) => void }) {
  const [collection, setCollection] = useState<MediaCollection | null>(null);
  const [busy, setBusy] = useState(false);
  const [dirty, setDirty] = useState(false);
  const [error, setError] = useState("");
  const [files, setFiles] = useState<QueuedFile[]>([]);
  const [filesReady, setFilesReady] = useState(false);
  const [loadedKey, setLoadedKey] = useState("");
  const [recovery, setRecovery] = useState<MediaCollection | null>(null);
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
    readMedia(contentId).then(value => { if (!active) return; setCollection(value);
      try { const draft = JSON.parse(sessionStorage.getItem(key) ?? "null"); if (draft) setRecovery(draft); } catch { /* damaged local draft */ }
    }).catch(e => { if (active) setError(e.message); });
    return () => { active = false; generation.current++; };
  }, [contentId, key]);
  useEffect(() => {
    if (filesReady && loadedKey === key) void storeComposerFiles(key, files).catch(() => setError("Không thể lưu file nháp trên trình duyệt; giữ trang mở để tránh mất file chưa upload."));
  }, [files, filesReady, loadedKey, key]);
  function change(value: MediaCollection) {
    setDirty(true);
    setCollection(value);
    try { sessionStorage.setItem(key, JSON.stringify(value)); } catch { setError("Không thể lưu bản nháp trên trình duyệt. Hãy lưu media trước khi đóng trang."); }
  }
  function add(incoming: FileList | File[]) {
    if (!canEdit || busy || !filesReady || !collection) return;
    const list = Array.from(incoming);
    if (list.some(f => f.size > 50 * 1024 * 1024 || f.size === 0) || list.reduce((sum,f)=>sum+f.size,0) > 200*1024*1024) { setError("Mỗi file từ 1 byte đến 50MB; mỗi lần chọn tối đa 200MB."); return; }
    if ((collection?.items.length ?? 0) + files.filter(f => f.status !== "Done").length + list.length > 10) { setError("Tối đa 10 media. Hãy bỏ bớt file trước khi thêm."); return; }
    setFiles(old => [...old, ...list.map(file => ({ id: crypto.randomUUID(), file, status: "Pending" }))]);
  }
  async function upload() {
    if (!collection || !canEdit || busy) return;
    const activeGeneration = generation.current;
    setBusy(true); setError("");
    let current = collection;
    for (const entry of files.filter(f => f.status !== "Done")) {
      if (scope !== `${getUserIdFromToken()}:${getStoredActiveWorkspace()?.id}:${contentId}`) break;
      setFiles(old => old.map(f => f.id === entry.id ? { ...f, status: "Uploading", error: undefined } : f));
      try {
        const item = await uploadMediaItem(contentId, entry.file);
        if (generation.current !== activeGeneration || scope !== `${getUserIdFromToken()}:${getStoredActiveWorkspace()?.id}:${contentId}`) break;
        current = { ...current, items: [...current.items, { ...item, isCover: current.items.length === 0 }] };
        change(current);
        setFiles(old => old.map(f => f.id === entry.id ? { ...f, status: "Done" } : f));
      } catch (e) { if (generation.current === activeGeneration) setFiles(old => old.map(f => f.id === entry.id ? { ...f, status: "Failed", error: (e as Error).message } : f)); }
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
  return <section className="rounded-xl border p-4 space-y-3" aria-label="Media collection">
    <h3 className="font-bold">Media collection</h3>
    <p>Tối đa 10 ảnh/video. Lưu media sẽ yêu cầu duyệt lại nội dung; lịch đã tạo giữ snapshot cũ.</p>
    {error && <p role="alert">{error}</p>}
    {recovery && <div>Bản nháp media chưa lưu được tìm thấy.
      <button disabled={!canEdit || busy} onClick={() => { change({ ...recovery, version: collection!.version }); setRecovery(null); }}>Khôi phục để đối chiếu</button>
      <button onClick={() => { sessionStorage.removeItem(key); setRecovery(null); }}>Bỏ bản nháp</button>
      <p>Đối chiếu {recovery.items.length} media trong bản nháp với {collection?.items.length ?? 0} media trên server trước khi lưu. File chưa upload được khôi phục khi trình duyệt cho phép.</p>
    </div>}
    {canEdit && <div onDragOver={e => e.preventDefault()} onDrop={e => { e.preventDefault(); if (!busy) add(e.dataTransfer.files); }} className="border border-dashed p-4">
      <label>Chọn hoặc thả ảnh/video <input aria-label="Media files" type="file" multiple accept="image/jpeg,image/png,image/webp,image/gif,video/mp4,video/webm,video/quicktime" disabled={busy || !collection || !filesReady} onChange={e => { if (e.target.files) add(e.target.files); e.target.value = ""; }} /></label>
    </div>}
    <ol>{collection?.items.map((item, index) => <li key={item.assetId} draggable={canEdit && !busy} onDragStart={() => { drag.current = index; }} onDragOver={e => e.preventDefault()} onDrop={e => { e.preventDefault(); e.stopPropagation(); if (drag.current !== null && canEdit && !busy) move(drag.current, index); drag.current = null; }} className="border rounded p-2 my-2">
      {item.mimeType.startsWith("video/") ? <video src={item.url} controls preload="metadata" className="max-h-40" /> : <img src={item.url} alt={item.altText ?? `Media ${index + 1}`} className="max-h-40" />}
      <span>{index + 1}. {item.mimeType} {item.isCover ? "— Cover" : ""}</span>
      {canEdit && <div>
        <button disabled={busy || index === 0} onClick={() => move(index, index - 1)}>Lên</button>{" "}
        <button disabled={busy || index === collection.items.length - 1} onClick={() => move(index, index + 1)}>Xuống</button>{" "}
        <button disabled={busy} onClick={() => change({ ...collection, items: collection.items.map(m => ({ ...m, isCover: m.assetId === item.assetId })) })}>Đặt cover</button>{" "}
        <button disabled={busy} onClick={() => change({ ...collection, items: collection.items.filter(m => m.assetId !== item.assetId) })}>Bỏ media</button>
      </div>}
    </li>)}</ol>
    <ul>{files.map(f => <li key={f.id}>{f.file.name}: {f.status} {f.error}
      {f.status !== "Done" && !busy && <button onClick={() => setFiles(old => old.filter(v => v.id !== f.id))}>Bỏ file</button>}
    </li>)}</ul>
    {busy && <progress aria-label="Processing media" />}
    {canEdit && <div className="flex gap-3">
      <button disabled={busy || !collection || !files.some(f => f.status !== "Done")} onClick={upload}>Upload / thử lại file lỗi</button>
      <button disabled={busy || !collection || files.some(f => f.status !== "Done")} onClick={save}>Lưu media</button>
      {collection?.items.length === 0 && <button disabled={busy} onClick={async () => { setBusy(true); try { setCollection(await importLegacyMedia(contentId, collection.version)); onSaved?.(); } catch (e) { setError((e as Error).message); } finally { setBusy(false); } }}>Nhập media cũ của nội dung</button>}
    </div>}
    <p>Cover là lựa chọn trong draft; khả năng đăng ảnh/video và thứ tự thực tế được kiểm tra theo từng kênh trước khi đăng.</p>
  </section>;
}
