"use client";
import React, { useEffect, useState } from "react";
import { previewPublish, startPublish, readPublish, type PublishPreview, type PublishOperation } from "@/services/composerService";
import { getStoredActiveWorkspace } from "@/stores/workspace-store";
import { getUserIdFromToken } from "@/lib/auth";
import { PlatformIcon } from "@/lib/contentConstants";

interface Props {
  contentId: string;
  brandId?: string;
  onClose: () => void;
  onSuccess: () => void;
}

interface Attempt {
  key: string;
  version: string;
  ids: string[];
  previous?: PublishOperation[];
}

export default function PostNowModal({ contentId, onClose, onSuccess }: Props) {
  const [preview, setPreview] = useState<PublishPreview | null>(null);
  const [selected, setSelected] = useState<string[]>([]);
  const [attempt, setAttempt] = useState<Attempt | null>(null);
  const [results, setResults] = useState<PublishOperation[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [activeTabId, setActiveTabId] = useState<string | null>(null);

  const scope = `${getUserIdFromToken()}:${getStoredActiveWorkspace()?.id}:${contentId}`;
  const storageKey = `aisam-publish:${scope}`;

  useEffect(() => {
    let active = true;
    setPreview(null);
    setAttempt(null);
    setResults([]);
    setSelected([]);
    setError("");
    previewPublish(contentId)
      .then(p => {
        if (active) setPreview(p);
      })
      .catch(e => {
        if (active) setError(e.message);
      });

    try {
      const previous = JSON.parse(localStorage.getItem(storageKey) ?? "null");
      if (previous) {
        setAttempt(previous);
        setSelected(previous.ids);
        setResults(previous.previous ?? []);
      }
    } catch {
      /* invalid draft */
    }

    return () => {
      active = false;
    };
  }, [contentId, storageKey]);

  useEffect(() => {
    if (!attempt) return;
    let active = true;
    let polls = 0;
    const timer = setInterval(async () => {
      if (++polls > 20) {
        clearInterval(timer);
        return;
      }
      try {
        const rows = await readPublish(contentId, attempt.key);
        if (active && rows.length) {
          setResults([...(attempt.previous ?? []), ...rows]);
          if (rows.every(r => ["Published", "Failed", "NeedsAttention", "Cancelled"].includes(r.status))) {
            clearInterval(timer);
          }
        }
      } catch {
        /* Keep the durable request and let the user check again. */
      }
    }, 3000);

    return () => {
      active = false;
      clearInterval(timer);
    };
  }, [attempt, contentId]);

  async function check() {
    if (!attempt) return;
    setBusy(true);
    setError("");
    try {
      const rows = await readPublish(contentId, attempt.key);
      setResults([...(attempt.previous ?? []), ...rows]);
      if (!rows.length) {
        setError("Chưa có kết quả được xác nhận. Không tạo lượt đăng mới; thử kiểm tra lại.");
      }
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBusy(false);
    }
  }

  async function publish() {
    if (!preview || !preview.approved || attempt || selected.length === 0 || selected.some(id => !preview.destinations.some(d => d.id === id && !d.error))) return;
    const request = {
      key: crypto.randomUUID(),
      version: preview.version,
      ids: [...selected],
      previous: results.filter(r => r.status === "Published")
    };
    // Save before the network call so timeout/reload cannot accidentally send a fresh request.
    try {
      localStorage.setItem(storageKey, JSON.stringify(request));
    } catch {
      setError("Không thể lưu mã lần đăng trên trình duyệt. Hãy bật local storage rồi thử lại.");
      return;
    }
    setAttempt(request);
    setBusy(true);
    setError("");
    try {
      setResults([...request.previous, ...await startPublish(contentId, request.version, request.ids, request.key)]);
    } catch (e) {
      setError(`${(e as Error).message}. Hãy kiểm tra kết quả; không đăng lại tự động.`);
    } finally {
      setBusy(false);
    }
  }

  const blocked = selected.some(id => !preview?.destinations.some(d => d.id === id && !d.error));
  const retryable = results.filter(r => r.status === "Failed" || (r.status === "NeedsAttention" && /^(MEDIA_|SOCIAL_|ACCESS_|RESOURCE_)/.test(r.errorCode ?? "")));
  const canPrepareRetry = !!attempt && attempt.ids.every(id => results.some(r => r.integrationId === id)) &&
    results.every(r => r.status === "Published" || retryable.includes(r)) && retryable.length > 0;

  const validDestinations = preview?.destinations.filter(d => !d.error && !results.some(r => r.integrationId === d.id && r.status === "Published")) ?? [];
  const canSelectAll = !attempt && !busy && validDestinations.length > 1;

  const activeDest = preview?.destinations.find(d => d.id === activeTabId)
    ?? preview?.destinations.find(d => selected.includes(d.id))
    ?? preview?.destinations[0];

  return (
    <div
      className="fixed inset-0 z-50 bg-black/60 backdrop-blur-xs flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      aria-label="Publish content"
    >
      <div className="bg-surface-container-lowest rounded-2xl border border-surface-container shadow-2xl max-w-2xl w-full max-h-[90vh] flex flex-col overflow-hidden">
        {/* Tier 1: Fixed Header */}
        <div className="shrink-0 px-6 py-4 border-b border-surface-container flex items-center justify-between bg-surface-container-lowest">
          <div className="flex items-center gap-2.5">
            <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary">
              <span className="material-symbols-outlined text-lg" aria-hidden="true">send</span>
            </div>
            <div>
              <h3 className="font-bold text-lg text-on-surface">Post Now</h3>
              <p className="text-xs text-on-surface-variant">Chọn kênh phân phối và kiểm tra nội dung trước khi xuất bản</p>
            </div>
          </div>
          <button
            onClick={onClose}
            disabled={busy}
            aria-label="Close"
            className="p-1.5 rounded-lg hover:bg-surface-container-high text-on-surface-variant hover:text-on-surface transition-colors disabled:opacity-50"
          >
            <span className="material-symbols-outlined text-xl" aria-hidden="true">close</span>
          </button>
        </div>

        {/* Tier 2: Scrollable Body */}
        <div className="flex-1 overflow-y-auto p-6 space-y-5">
          {!preview && !error && (
            <div className="flex items-center gap-2 text-sm text-on-surface-variant p-3 bg-surface-container rounded-xl">
              <span className="material-symbols-outlined animate-spin text-base" aria-hidden="true">progress_activity</span>
              <p>Đang kiểm tra nội dung và quyền kênh…</p>
            </div>
          )}

          {preview && !preview.approved && (
            <div className="p-3.5 bg-amber-50 border border-amber-200 text-amber-800 rounded-xl text-sm flex items-start gap-2.5" role="alert">
              <span className="material-symbols-outlined text-amber-600 text-base mt-0.5 shrink-0" aria-hidden="true">warning</span>
              <p>Nội dung cần được duyệt trước khi đăng. Hãy lưu media và gửi duyệt lại.</p>
            </div>
          )}

          {error && (
            <div className="p-3.5 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-sm flex items-start gap-2.5" role="alert">
              <span className="material-symbols-outlined text-rose-600 text-base mt-0.5 shrink-0" aria-hidden="true">error</span>
              <p>{error}</p>
            </div>
          )}

          {/* Channel Selector Section */}
          {preview && (
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <h4 className="text-xs font-bold uppercase tracking-wider text-on-surface-variant">Kênh xuất bản</h4>
                  <span className="text-xs bg-surface-container px-2 py-0.5 rounded-full font-medium text-on-surface-variant">
                    {selected.length}/{preview.destinations.length} đã chọn
                  </span>
                </div>
                {canSelectAll && (
                  <button
                    type="button"
                    onClick={() => {
                      const allValidIds = validDestinations.map(d => d.id);
                      const allSelected = allValidIds.every(id => selected.includes(id));
                      if (allSelected) {
                        setSelected(old => old.filter(id => !allValidIds.includes(id)));
                      } else {
                        setSelected(old => Array.from(new Set([...old, ...allValidIds])));
                      }
                    }}
                    className="text-xs font-semibold text-primary hover:underline"
                  >
                    {validDestinations.every(d => selected.includes(d.id)) ? "Bỏ chọn tất cả" : "Chọn tất cả kênh hợp lệ"}
                  </button>
                )}
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
                {preview.destinations.map(d => {
                  const isSelected = selected.includes(d.id);
                  const isPublished = results.some(r => r.integrationId === d.id && r.status === "Published");
                  const isDisabled = busy || !!attempt || isPublished || (!!d.error && !isSelected);

                  return (
                    <div
                      key={d.id}
                      className={`border rounded-xl p-3 transition-all ${
                        isSelected
                          ? "border-primary/50 bg-primary/5 shadow-2xs"
                          : "border-surface-container hover:border-surface-container-high bg-surface-container-lowest"
                      } ${isDisabled && !isSelected ? "opacity-60 bg-surface-container/30" : ""}`}
                    >
                      <label className="flex items-start gap-2.5 cursor-pointer select-none">
                        <input
                          type="checkbox"
                          checked={isSelected}
                          disabled={isDisabled}
                          onChange={() => setSelected(old => old.includes(d.id) ? old.filter(id => id !== d.id) : [...old, d.id])}
                          className="mt-0.5 rounded border-surface-container-high text-primary focus:ring-primary h-4 w-4"
                        />
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-1.5 flex-wrap">
                            <PlatformIcon platform={d.platform} className="w-3.5 h-3.5 shrink-0" />
                            <span className="font-semibold text-sm text-on-surface truncate">{d.name}</span>
                            <span className="text-xs text-on-surface-variant font-medium capitalize">— {d.platform}</span>
                            {isPublished && (
                              <span className="inline-flex items-center gap-0.5 text-[11px] font-semibold text-emerald-700 bg-emerald-100/70 px-1.5 py-0.2 rounded-full">
                                <span className="material-symbols-outlined text-xs" aria-hidden="true">check</span> Đã đăng
                              </span>
                            )}
                          </div>

                          {d.error && (
                            <div className="mt-1 text-xs text-rose-600 space-y-0.5">
                              <p role="status">Không tương thích hoặc thiếu quyền: {d.error}. Bạn có thể bỏ chọn kênh này.</p>
                              {d.error === "SOCIAL_REAUTH_REQUIRED" && (
                                <a href="/social" className="inline-flex items-center gap-1 underline font-medium hover:text-rose-700">
                                  <span className="material-symbols-outlined text-xs" aria-hidden="true">open_in_new</span> Kết nối lại tài khoản
                                </a>
                              )}
                            </div>
                          )}
                        </div>
                      </label>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* Platform Preview Section */}
          {preview && preview.destinations.length > 0 && activeDest && (
            <div className="border border-surface-container rounded-2xl overflow-hidden bg-surface-container-lowest">
              <div className="border-b border-surface-container bg-surface-container-low/60 px-3 py-2 flex items-center justify-between gap-2 overflow-x-auto">
                <span className="text-xs font-bold uppercase tracking-wider text-on-surface-variant shrink-0">Xem trước kênh:</span>
                <div className="flex items-center gap-1.5 shrink-0">
                  {preview.destinations.map(d => {
                    const isActive = activeDest.id === d.id;
                    const isChecked = selected.includes(d.id);
                    return (
                      <button
                        key={d.id}
                        type="button"
                        onClick={() => setActiveTabId(d.id)}
                        className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-all ${
                          isActive
                            ? "bg-primary text-white shadow-xs font-semibold"
                            : "text-on-surface-variant hover:text-on-surface hover:bg-surface-container"
                        }`}
                      >
                        <PlatformIcon platform={d.platform} className="w-3.5 h-3.5" />
                        <span>{d.name}</span>
                        {isChecked && (
                          <span className={`w-1.5 h-1.5 rounded-full ${isActive ? "bg-white" : "bg-primary"}`} />
                        )}
                      </button>
                    );
                  })}
                </div>
              </div>

              <div className="p-4 space-y-3">
                <div className="flex items-center justify-between text-xs text-on-surface-variant border-b border-surface-container pb-2">
                  <div className="flex items-center gap-1.5">
                    <PlatformIcon platform={activeDest.platform} className="w-3.5 h-3.5" />
                    <span className="font-semibold text-on-surface">{activeDest.name}</span>
                  </div>
                  <span>
                    {Array.from(activeDest.caption).length} ký tự · tối đa {activeDest.capability.maxItems} media theo adapter
                  </span>
                </div>

                <p className="text-sm text-on-surface whitespace-pre-wrap leading-relaxed max-h-36 overflow-y-auto">
                  {activeDest.caption}
                </p>

                {preview.media && preview.media.length > 0 && (
                  <div className="pt-2 border-t border-surface-container">
                    <div className="text-xs font-medium text-on-surface-variant mb-2">
                      Media đính kèm ({preview.media.length}):
                    </div>
                    <div className="flex gap-2 overflow-x-auto pb-1">
                      {preview.media.map((m, i) => m.mimeType.startsWith("video/")
                        ? <video key={i} src={m.url} controls preload="metadata" className="w-28 h-20 object-cover rounded-lg border border-surface-container shrink-0" />
                        : <img key={i} src={m.url} alt={`Media ${i + 1}`} className="w-28 h-20 object-cover rounded-lg border border-surface-container shrink-0" />)}
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Results Section */}
          {results.length > 0 && (
            <div className="space-y-2">
              <h4 className="text-xs font-bold uppercase tracking-wider text-on-surface-variant">Kết quả xử lý</h4>
              <ul className="space-y-2">
                {results.map(result => {
                  const dest = preview?.destinations.find(d => d.id === result.integrationId);
                  const isSuccess = result.status === "Published";
                  const isFailed = result.status === "Failed";
                  return (
                    <li
                      key={result.id}
                      className={`border p-3 rounded-xl flex items-center justify-between text-sm ${
                        isSuccess
                          ? "border-emerald-200 bg-emerald-50/60 text-emerald-900"
                          : isFailed
                          ? "border-rose-200 bg-rose-50/60 text-rose-800"
                          : "border-surface-container bg-surface-container-low"
                      }`}
                    >
                      <div className="flex items-center gap-2 min-w-0">
                        {dest && <PlatformIcon platform={dest.platform} className="w-4 h-4 shrink-0" />}
                        <span className="font-semibold">{dest?.name ?? result.integrationId}:</span>
                        <strong>{result.status}</strong>
                        {result.errorCode && <span> — {result.errorCode}</span>}
                        {result.providerId && <span className="text-xs opacity-75"> — ID: {result.providerId}</span>}
                      </div>
                      {result.errorCode === "SOCIAL_REAUTH_REQUIRED" && (
                        <a href="/social" className="text-xs font-semibold underline hover:opacity-80 ml-2 shrink-0">
                          Kết nối lại
                        </a>
                      )}
                    </li>
                  );
                })}
              </ul>
            </div>
          )}

          {attempt && (
            <p className="text-xs text-on-surface-variant bg-surface-container/60 p-3 rounded-xl border border-surface-container">
              Kết quả từng kênh được lưu riêng. Nếu timeout hoặc NeedsAttention, cần đối soát trước khi tạo yêu cầu mới.
            </p>
          )}
        </div>

        {/* Tier 3: Fixed Footer */}
        <div className="shrink-0 px-6 py-4 border-t border-surface-container bg-surface-container-lowest flex items-center justify-between gap-3">
          <button
            type="button"
            disabled={busy}
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-on-surface-variant hover:text-on-surface rounded-xl hover:bg-surface-container transition-colors disabled:opacity-50"
          >
            Đóng
          </button>

          <div className="flex items-center gap-2">
            {!attempt ? (
              <button
                type="button"
                disabled={busy || !preview?.approved || !selected.length || blocked}
                onClick={publish}
                className="px-5 py-2 text-sm font-semibold bg-primary text-white rounded-xl hover:bg-primary-hover shadow-xs transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Đăng lên {selected.length} kênh
              </button>
            ) : (
              <button
                type="button"
                disabled={busy}
                onClick={check}
                className="px-5 py-2 text-sm font-semibold bg-primary text-white rounded-xl hover:bg-primary-hover shadow-xs transition-all disabled:opacity-50"
              >
                Kiểm tra kết quả
              </button>
            )}

            {attempt && attempt.ids.every(id => results.some(r => r.integrationId === id && r.status === "Published")) && results.every(r => r.status === "Published") && (
              <button
                type="button"
                onClick={onSuccess}
                className="px-5 py-2 text-sm font-semibold bg-emerald-600 text-white rounded-xl hover:bg-emerald-700 shadow-xs transition-all"
              >
                Hoàn tất
              </button>
            )}

            {canPrepareRetry && (
              <button
                type="button"
                disabled={busy}
                onClick={async () => {
                  setBusy(true);
                  try {
                    setPreview(await previewPublish(contentId));
                    setSelected(retryable.map(r => r.integrationId));
                    setAttempt(null);
                  } catch (e) {
                    setError((e as Error).message);
                  } finally {
                    setBusy(false);
                  }
                }}
                className="px-4 py-2 text-sm font-semibold bg-amber-600 text-white rounded-xl hover:bg-amber-700 shadow-xs transition-all disabled:opacity-50"
              >
                Chuẩn bị lượt mới cho kênh lỗi đã xác nhận
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
