"use client";

/**
 * MultiVideoUpload.tsx
 * Upload, preview, delete, reorder multiple videos per post.
 * - Drag & drop reorder via HTML5 DnD
 * - Playable preview player
 * - Counter: 0/5 → 5/5
 * - Primary video = first in array
 * - Validates format (MP4, WebM, MOV) and size (max 50MB)
 * - Platform capability notice
 */

import { useRef, useState, useCallback } from "react";
import { uploadContentMedia } from "@/services/contentService";
import { validateMediaFile, MAX_MEDIA_FILE_SIZE_MB } from "@/lib/mediaUpload";

export const MAX_VIDEOS_PER_POST = 5;

interface MultiVideoUploadProps {
  videos: string[];
  onChange: (urls: string[]) => void;
  /** Optional: override upload function (for testing) */
  onUpload?: (file: File) => Promise<string>;
  className?: string;
  selectedPlatforms?: string[];
}

export default function MultiVideoUpload({
  videos,
  onChange,
  onUpload,
  className = "",
  selectedPlatforms = [],
}: MultiVideoUploadProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [uploading, setUploading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);
  const dragSrcIndex = useRef<number | null>(null);
  const [activePreviewUrl, setActivePreviewUrl] = useState<string | null>(null);

  const atMax = videos.length >= MAX_VIDEOS_PER_POST;

  // ---------------------------------------------------------------------------
  // Upload
  // ---------------------------------------------------------------------------

  const handleFileChange = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = Array.from(e.target.files ?? []);
      e.target.value = "";
      if (files.length === 0) return;

      const remaining = MAX_VIDEOS_PER_POST - videos.length;
      const toUpload = files.slice(0, remaining);

      if (files.length > remaining) {
        setUploadError(`Tối đa ${MAX_VIDEOS_PER_POST} video cho mỗi bài viết.`);
      } else {
        setUploadError(null);
      }

      setUploading(true);
      const newUrls: string[] = [];
      for (const file of toUpload) {
        const validationError = validateMediaFile(file, "video");
        if (validationError) {
          setUploadError(validationError);
          continue;
        }
        try {
          const uploadFn = onUpload ?? ((f: File) => uploadContentMedia(f, "video"));
          const url = await uploadFn(file);
          newUrls.push(url);
        } catch (err: unknown) {
          const msg = err instanceof Error ? err.message : "Tải video lên thất bại";
          setUploadError(msg);
        }
      }
      setUploading(false);

      if (newUrls.length > 0) {
        onChange([...videos, ...newUrls]);
      }
    },
    [videos, onChange, onUpload]
  );

  const handleDrop = useCallback(
    async (e: React.DragEvent) => {
      e.preventDefault();
      setDragOverIndex(null);
      if (atMax) return;

      const files = Array.from(e.dataTransfer.files).filter((f) =>
        f.type.startsWith("video/")
      );
      if (files.length === 0) return;

      const remaining = MAX_VIDEOS_PER_POST - videos.length;
      const toUpload = files.slice(0, remaining);

      setUploading(true);
      const newUrls: string[] = [];
      for (const file of toUpload) {
        const validationError = validateMediaFile(file, "video");
        if (validationError) {
          setUploadError(validationError);
          continue;
        }
        try {
          const uploadFn = onUpload ?? ((f: File) => uploadContentMedia(f, "video"));
          const url = await uploadFn(file);
          newUrls.push(url);
        } catch (err: unknown) {
          setUploadError(err instanceof Error ? err.message : "Tải video lên thất bại");
        }
      }
      setUploading(false);
      if (newUrls.length > 0) onChange([...videos, ...newUrls]);
    },
    [videos, onChange, onUpload, atMax]
  );

  // ---------------------------------------------------------------------------
  // Delete
  // ---------------------------------------------------------------------------

  const removeVideo = useCallback(
    (index: number) => {
      const updated = videos.filter((_, i) => i !== index);
      onChange(updated);
      setUploadError(null);
      if (activePreviewUrl && videos[index] === activePreviewUrl) {
        setActivePreviewUrl(null);
      }
    },
    [videos, onChange, activePreviewUrl, setActivePreviewUrl]
  );

  // ---------------------------------------------------------------------------
  // Reorder (drag & drop)
  // ---------------------------------------------------------------------------

  const handleDragStart = (index: number) => {
    dragSrcIndex.current = index;
  };

  const handleDragOver = (e: React.DragEvent, index: number) => {
    if (dragSrcIndex.current !== null) {
      e.preventDefault();
      setDragOverIndex(index);
    }
  };

  const handleDragEnd = () => {
    dragSrcIndex.current = null;
    setDragOverIndex(null);
  };

  const handleDropOnVideo = (e: React.DragEvent, targetIndex: number) => {
    e.preventDefault();
    const srcIndex = dragSrcIndex.current;
    dragSrcIndex.current = null;
    setDragOverIndex(null);

    if (srcIndex === null || srcIndex === targetIndex) return;

    const updated = [...videos];
    const [moved] = updated.splice(srcIndex, 1);
    updated.splice(targetIndex, 0, moved);
    onChange(updated);
  };

  const moveItem = (index: number, direction: "left" | "right") => {
    const targetIndex = direction === "left" ? index - 1 : index + 1;
    if (targetIndex < 0 || targetIndex >= videos.length) return;
    const updated = [...videos];
    const [moved] = updated.splice(index, 1);
    updated.splice(targetIndex, 0, moved);
    onChange(updated);
  };

  return (
    <div className={`space-y-3 ${className}`}>
      {/* Hidden file input */}
      <input
        ref={fileInputRef}
        type="file"
        accept="video/mp4,video/webm,video/quicktime"
        multiple
        className="hidden"
        onChange={handleFileChange}
      />

      {/* Header bar: Label + Counter */}
      <div className="flex items-center justify-between">
        <label className="text-xs font-semibold text-slate-700 dark:text-slate-300 uppercase tracking-wider">
          Video đính kèm
        </label>
        <div className="flex items-center gap-2">
          <span
            className={`text-xs font-medium px-2 py-0.5 rounded-full ${
              atMax
                ? "bg-amber-100 text-amber-800 dark:bg-amber-900/40 dark:text-amber-300"
                : "bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400"
            }`}
          >
            {videos.length}/{MAX_VIDEOS_PER_POST} video
          </span>
        </div>
      </div>

      {/* Platform capability notice when multiple videos exist */}
      {videos.length > 1 && (
        <div className="p-2.5 rounded-lg bg-blue-50/80 dark:bg-blue-950/40 border border-blue-200/80 dark:border-blue-900/60 text-xs text-blue-800 dark:text-blue-300 flex items-start gap-2">
          <svg className="w-4 h-4 shrink-0 mt-0.5 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div>
            <span className="font-semibold">Lưu ý khả năng phát hành:</span> TikTok, Facebook Page và Instagram Reels chỉ hỗ trợ 1 video cho mỗi bài đăng. Video đầu tiên sẽ được ưu tiên làm video chính.
          </div>
        </div>
      )}

      {/* Error display */}
      {uploadError && (
        <div className="p-2.5 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 dark:bg-rose-950/40 dark:border-rose-900/60 dark:text-rose-300 text-xs flex items-center justify-between">
          <span>{uploadError}</span>
          <button
            type="button"
            onClick={() => setUploadError(null)}
            className="text-rose-500 hover:text-rose-700 ml-2 font-bold"
          >
            ✕
          </button>
        </div>
      )}

      {/* Grid of video thumbnails */}
      {videos.length > 0 && (
        <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
          {videos.map((url, idx) => (
            <div
              key={`${url}-${idx}`}
              draggable
              onDragStart={() => handleDragStart(idx)}
              onDragOver={(e) => handleDragOver(e, idx)}
              onDragEnd={handleDragEnd}
              onDrop={(e) => handleDropOnVideo(e, idx)}
              className={`relative group rounded-xl overflow-hidden border-2 transition-all cursor-move aspect-video bg-black/90 flex items-center justify-center ${
                dragOverIndex === idx
                  ? "border-primary shadow-lg scale-105"
                  : idx === 0
                  ? "border-indigo-500 shadow-sm"
                  : "border-slate-200 dark:border-slate-700 hover:border-slate-400"
              }`}
            >
              {/* Video element as preview */}
              <video
                src={url}
                preload="metadata"
                className="w-full h-full object-cover pointer-events-none"
              />

              {/* Cover badge */}
              {idx === 0 && (
                <div className="absolute top-2 left-2 z-10 px-2 py-0.5 rounded bg-indigo-600 text-white text-[10px] font-bold tracking-wide shadow-md">
                  VIDEO CHÍNH
                </div>
              )}

              {/* Index counter */}
              <div className="absolute bottom-2 left-2 z-10 px-1.5 py-0.5 rounded bg-black/60 text-white text-[10px] font-mono">
                #{idx + 1}
              </div>

              {/* Overlay controls */}
              <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex flex-col justify-between p-2">
                <div className="flex justify-end gap-1">
                  {/* Play preview modal */}
                  <button
                    type="button"
                    onClick={() => setActivePreviewUrl(url)}
                    className="p-1 rounded bg-white/20 hover:bg-white/40 text-white transition-colors"
                    title="Phát xem thử"
                  >
                    <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </button>

                  {/* Remove */}
                  <button
                    type="button"
                    onClick={() => removeVideo(idx)}
                    className="p-1 rounded bg-rose-600/80 hover:bg-rose-600 text-white transition-colors"
                    title="Xóa video"
                  >
                    <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>

                {/* Reorder arrows */}
                <div className="flex items-center justify-center gap-2">
                  <button
                    type="button"
                    disabled={idx === 0}
                    onClick={() => moveItem(idx, "left")}
                    className="px-2 py-1 rounded bg-white/20 hover:bg-white/40 text-white disabled:opacity-30 disabled:cursor-not-allowed text-xs transition-colors"
                    title="Di chuyển sang trái / lên trước"
                  >
                    ◀
                  </button>
                  <button
                    type="button"
                    disabled={idx === videos.length - 1}
                    onClick={() => moveItem(idx, "right")}
                    className="px-2 py-1 rounded bg-white/20 hover:bg-white/40 text-white disabled:opacity-30 disabled:cursor-not-allowed text-xs transition-colors"
                    title="Di chuyển sang phải / ra sau"
                  >
                    ▶
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Upload Dropzone / Button */}
      {!atMax && (
        <div
          onDragOver={(e) => { e.preventDefault(); }}
          onDrop={handleDrop}
          onClick={() => fileInputRef.current?.click()}
          className="border-2 border-dashed border-slate-300 dark:border-slate-700 hover:border-indigo-500 dark:hover:border-indigo-400 rounded-xl p-6 text-center cursor-pointer transition-colors bg-slate-50/50 dark:bg-slate-900/30 hover:bg-indigo-50/30 dark:hover:bg-indigo-950/20"
        >
          {uploading ? (
            <div className="flex flex-col items-center justify-center py-2 gap-2 text-indigo-600 dark:text-indigo-400">
              <svg className="animate-spin w-7 h-7" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              <span className="text-xs font-medium">Đang tải video lên...</span>
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center py-2 gap-2 text-slate-500 dark:text-slate-400">
              <div className="w-10 h-10 rounded-full bg-indigo-50 dark:bg-indigo-950/50 flex items-center justify-center text-indigo-600 dark:text-indigo-400">
                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
              </div>
              <div className="text-xs font-medium text-slate-700 dark:text-slate-300">
                Kéo thả video vào đây hoặc <span className="text-indigo-600 dark:text-indigo-400 underline">chọn tệp</span>
              </div>
              <p className="text-[11px] text-slate-400">
                Hỗ trợ MP4, WebM, MOV (tối đa {MAX_MEDIA_FILE_SIZE_MB}MB/video, tối đa {MAX_VIDEOS_PER_POST} video)
              </p>
            </div>
          )}
        </div>
      )}

      {/* Video preview modal */}
      {activePreviewUrl && (
        <div
          className="fixed inset-0 z-50 bg-black/80 flex items-center justify-center p-4"
          onClick={() => setActivePreviewUrl(null)}
        >
          <div
            className="relative max-w-3xl w-full bg-black rounded-2xl overflow-hidden shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between p-3 bg-slate-900 text-white">
              <span className="text-xs font-semibold">Xem trước video</span>
              <button
                type="button"
                onClick={() => setActivePreviewUrl(null)}
                className="p-1 rounded hover:bg-white/20 text-white font-bold"
              >
                ✕
              </button>
            </div>
            <video
              src={activePreviewUrl}
              controls
              autoPlay
              className="w-full max-h-[70vh] object-contain bg-black"
            />
          </div>
        </div>
      )}
    </div>
  );
}
