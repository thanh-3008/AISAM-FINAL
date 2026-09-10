"use client";
import React, { useEffect, useState } from "react";
import { previewPublish, startPublish, readPublish, type PublishPreview, type PublishOperation } from "@/services/composerService";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";
import { getUserIdFromToken } from "@/lib/auth";
interface Props { contentId: string; brandId?: string; onClose: () => void; onSuccess: () => void; }
interface Attempt { key: string; version: string; ids: string[]; previous?: PublishOperation[]; }
export default function PostNowModal({ contentId, onClose, onSuccess }: Props) {
  const [preview, setPreview] = useState<PublishPreview | null>(null);
  const [selected, setSelected] = useState<string[]>([]);
  const [attempt, setAttempt] = useState<Attempt | null>(null);
  const [results, setResults] = useState<PublishOperation[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const scope = `${getUserIdFromToken()}:${getStoredActiveWorkspace()?.id}:${contentId}`;
  const storageKey = `aisam-publish:${scope}`;
  useEffect(() => {
    let active = true;
    setPreview(null); setAttempt(null); setResults([]); setSelected([]); setError("");
    previewPublish(contentId).then(p => { if (active) setPreview(p); }).catch(e => { if (active) setError(e.message); });
    try { const previous = JSON.parse(localStorage.getItem(storageKey) ?? "null"); if (previous) { setAttempt(previous); setSelected(previous.ids); setResults(previous.previous ?? []); } } catch { /* invalid draft */ }
    return () => { active = false; };
  }, [contentId, storageKey]);
  useEffect(() => {
    if (!attempt) return;
    let active = true; let polls = 0;
    const timer = setInterval(async () => {
      if (++polls > 20) { clearInterval(timer); return; }
      try { const rows = await readPublish(contentId, attempt.key);
        if (active && rows.length) {
          setResults([...(attempt.previous ?? []), ...rows]);
          if (rows.every(r => ["Published", "Failed", "NeedsAttention", "Cancelled"].includes(r.status))) clearInterval(timer);
        }
      } catch { /* Keep the durable request and let the user check again. */ }
    }, 3000);
    return () => { active = false; clearInterval(timer); };
  }, [attempt, contentId]);
  async function check() {
    if (!attempt) return;
    setBusy(true); setError("");
    try { const rows = await readPublish(contentId, attempt.key); setResults([...(attempt.previous ?? []), ...rows]);
      if (!rows.length) setError("Chưa có kết quả được xác nhận. Không tạo lượt đăng mới; thử kiểm tra lại.");
    } catch (e) { setError((e as Error).message); } finally { setBusy(false); }
  }
  async function publish() {
    if (!preview || !preview.approved || attempt || selected.length === 0 || selected.some(id => !preview.destinations.some(d => d.id === id && !d.error))) return;
    const request = { key: crypto.randomUUID(), version: preview.version, ids: [...selected], previous: results.filter(r => r.status === "Published") };
    // Save before the network call so timeout/reload cannot accidentally send a fresh request.
    try { localStorage.setItem(storageKey, JSON.stringify(request)); } catch { setError("Không thể lưu mã lần đăng trên trình duyệt. Hãy bật local storage rồi thử lại."); return; }
    setAttempt(request); setBusy(true); setError("");
    try { setResults([...request.previous, ...await startPublish(contentId, request.version, request.ids, request.key)]); }
    catch (e) { setError(`${(e as Error).message}. Hãy kiểm tra kết quả; không đăng lại tự động.`); }
    finally { setBusy(false); }
  }
  const blocked = selected.some(id => !preview?.destinations.some(d => d.id === id && !d.error));
  const retryable = results.filter(r => r.status === "Failed" || (r.status === "NeedsAttention" && /^(MEDIA_|SOCIAL_|ACCESS_|RESOURCE_)/.test(r.errorCode ?? "")));
  const canPrepareRetry = !!attempt && attempt.ids.every(id => results.some(r => r.integrationId === id)) &&
    results.every(r => r.status === "Published" || retryable.includes(r)) && retryable.length > 0;
  return <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center p-4" role="dialog" aria-modal="true" aria-label="Publish content">
    <div className="bg-surface-container-lowest rounded-xl p-6 max-w-2xl w-full max-h-[90vh] overflow-y-auto space-y-4">
      <h3 className="font-bold text-xl">Post Now</h3>
      {!preview && !error && <p>Đang kiểm tra nội dung và quyền kênh…</p>}
      {preview && !preview.approved && <p role="alert">Nội dung cần được duyệt trước khi đăng. Hãy lưu media và gửi duyệt lại.</p>}
      {preview?.destinations.map(d => <section key={d.id} className="border rounded p-3 space-y-2">
        <label><input type="checkbox" checked={selected.includes(d.id)} disabled={busy || !!attempt || results.some(r => r.integrationId === d.id && r.status === "Published") || (!!d.error && !selected.includes(d.id))}
          onChange={() => setSelected(old => old.includes(d.id) ? old.filter(id => id !== d.id) : [...old, d.id])} /> {d.name} — {d.platform}</label>
        {d.error && <p role="status">Không tương thích hoặc thiếu quyền: {d.error}. Bạn có thể bỏ chọn kênh này.</p>}
        {d.error === "SOCIAL_REAUTH_REQUIRED" && <a href="/social">Kết nối lại tài khoản</a>}
        <p className="whitespace-pre-wrap">{d.caption}</p>
        <p>{Array.from(d.caption).length} ký tự · tối đa {d.capability.maxItems} media theo adapter</p>
        <div className="flex gap-2 overflow-auto">{preview.media.map((m, i) => m.mimeType.startsWith("video/")
          ? <video key={i} src={m.url} controls preload="metadata" className="w-32" />
          : <img key={i} src={m.url} alt={`Media ${i + 1}`} className="w-32 object-contain" />)}</div>
      </section>)}
      {error && <p role="alert">{error}</p>}
      <ul>{results.map(result => <li key={result.id} className="border p-2">
        {preview?.destinations.find(d => d.id === result.integrationId)?.name ?? result.integrationId}: <strong>{result.status}</strong>
        {result.errorCode && <span> — {result.errorCode}</span>}{result.providerId && <span> — ID: {result.providerId}</span>}
        {result.errorCode === "SOCIAL_REAUTH_REQUIRED" && <a href="/social"> Kết nối lại</a>}
      </li>)}</ul>
      {attempt && <p>Kết quả từng kênh được lưu riêng. Nếu timeout hoặc NeedsAttention, cần đối soát trước khi tạo yêu cầu mới.</p>}
      <div className="flex gap-4">
        <button disabled={busy} onClick={onClose}>Đóng</button>
        {!attempt ? <button disabled={busy || !preview?.approved || !selected.length || blocked} onClick={publish}>Đăng lên {selected.length} kênh</button>
          : <button disabled={busy} onClick={check}>Kiểm tra kết quả</button>}
        {attempt && attempt.ids.every(id => results.some(r => r.integrationId === id && r.status === "Published")) && results.every(r => r.status === "Published") && <button onClick={onSuccess}>Hoàn tất</button>}
        {canPrepareRetry && <button disabled={busy} onClick={async () => {
          setBusy(true);
          try { setPreview(await previewPublish(contentId)); setSelected(retryable.map(r => r.integrationId)); setAttempt(null); }
          catch (e) { setError((e as Error).message); } finally { setBusy(false); }
        }}>Chuẩn bị lượt mới cho kênh lỗi đã xác nhận</button>}
      </div>
    </div>
  </div>;
}
