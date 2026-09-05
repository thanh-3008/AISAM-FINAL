"use client";

/**
 * MultiImageUpload.tsx
 * Upload, preview, delete, reorder up to 5 images per post.
 * - Drag & drop reorder via HTML5 DnD
 * - Counter: 0/5 → 5/5
 * - Primary image = first in array
 * - Blocks upload when at max
 */

import { useRef, useState, useCallback } from "react";
import { MAX_IMAGES_PER_POST } from "@/lib/richTextUtils";
import { uploadContentMedia } from "@/services/contentService";
import { validateMediaFile } from "@/lib/mediaUpload";

interface MultiImageUploadProps {
  images: string[];
  onChange: (urls: string[]) => void;
  /** Optional: override upload function (for testing) */
  onUpload?: (file: File) => Promise<string>;
  className?: string;
}

export default function MultiImageUpload({
  images,
  onChange,
  onUpload,
  className = "",
}: MultiImageUploadProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [uploading, setUploading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);
  const dragSrcIndex = useRef<number | null>(null);

  const atMax = images.length >= MAX_IMAGES_PER_POST;

  // ---------------------------------------------------------------------------
  // Upload
  // ---------------------------------------------------------------------------

  const handleFileChange = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = Array.from(e.target.files ?? []);
      e.target.value = "";
      if (files.length === 0) return;

      const remaining = MAX_IMAGES_PER_POST - images.length;
      const toUpload = files.slice(0, remaining);

      if (files.length > remaining) {
        setUploadError(`Maximum ${MAX_IMAGES_PER_POST} images allowed per post.`);
      } else {
        setUploadError(null);
      }

      setUploading(true);
      const newUrls: string[] = [];
      for (const file of toUpload) {
        const validationError = validateMediaFile(file, "image");
        if (validationError) {
          setUploadError(validationError);
          continue;
        }
        try {
          const uploadFn = onUpload ?? ((f: File) => uploadContentMedia(f, "image"));
          const url = await uploadFn(file);
          newUrls.push(url);
        } catch (err: unknown) {
          const msg = err instanceof Error ? err.message : "Upload failed";
          setUploadError(msg);
        }
      }
      setUploading(false);

      if (newUrls.length > 0) {
        onChange([...images, ...newUrls]);
      }
    },
    [images, onChange, onUpload]
  );

  const handleDrop = useCallback(
    async (e: React.DragEvent) => {
      e.preventDefault();
      setDragOverIndex(null);
      if (atMax) return;

      const files = Array.from(e.dataTransfer.files).filter((f) =>
        f.type.startsWith("image/")
      );
      if (files.length === 0) return;

      const remaining = MAX_IMAGES_PER_POST - images.length;
      const toUpload = files.slice(0, remaining);

      setUploading(true);
      const newUrls: string[] = [];
      for (const file of toUpload) {
        try {
          const uploadFn = onUpload ?? ((f: File) => uploadContentMedia(f, "image"));
          const url = await uploadFn(file);
          newUrls.push(url);
        } catch (err: unknown) {
          setUploadError(err instanceof Error ? err.message : "Upload failed");
        }
      }
      setUploading(false);
      if (newUrls.length > 0) onChange([...images, ...newUrls]);
    },
    [images, onChange, onUpload, atMax]
  );

  // ---------------------------------------------------------------------------
  // Delete
  // ---------------------------------------------------------------------------

  const removeImage = useCallback(
    (index: number) => {
      const updated = images.filter((_, i) => i !== index);
      onChange(updated);
      setUploadError(null);
    },
    [images, onChange]
  );

  // ---------------------------------------------------------------------------
  // Reorder (drag & drop)
  // ---------------------------------------------------------------------------

  const handleDragStart = (index: number) => {
    dragSrcIndex.current = index;
  };

  const handleDragOver = (e: React.DragEvent, index: number) => {
    // Only handle drag-over for reordering (not file drop) when dragging an existing image
    if (dragSrcIndex.current !== null) {
      e.preventDefault();
      setDragOverIndex(index);
    }
  };

  const handleDragEnd = () => {
    dragSrcIndex.current = null;
    setDragOverIndex(null);
  };

  const handleDropOnImage = (e: React.DragEvent, targetIndex: number) => {
    e.preventDefault();
    const srcIndex = dragSrcIndex.current;
    dragSrcIndex.current = null;
    setDragOverIndex(null);

    if (srcIndex === null || srcIndex === targetIndex) return;

    const updated = [...images];
    const [moved] = updated.splice(srcIndex, 1);
    updated.splice(targetIndex, 0, moved);
    onChange(updated);
  };

  // ---------------------------------------------------------------------------
  // Replace single image
  // ---------------------------------------------------------------------------

  const replaceImageInputRef = useRef<HTMLInputElement>(null);
  const replaceIndexRef = useRef<number | null>(null);

  const handleReplace = (index: number) => {
    replaceIndexRef.current = index;
    replaceImageInputRef.current?.click();
  };

  const handleReplaceFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file || replaceIndexRef.current === null) return;

    const validationError = validateMediaFile(file, "image");
    if (validationError) { setUploadError(validationError); return; }

    setUploading(true);
    try {
      const uploadFn = onUpload ?? ((f: File) => uploadContentMedia(f, "image"));
      const url = await uploadFn(file);
      const updated = [...images];
      updated[replaceIndexRef.current] = url;
      onChange(updated);
      setUploadError(null);
    } catch (err: unknown) {
      setUploadError(err instanceof Error ? err.message : "Upload failed");
    } finally {
      setUploading(false);
      replaceIndexRef.current = null;
    }
  };

  // ---------------------------------------------------------------------------
  // Render
  // ---------------------------------------------------------------------------

  return (
    <div className={`space-y-3 ${className}`}>
      {/* Hidden file inputs */}
      <input
        ref={fileInputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp,image/gif"
        multiple
        className="hidden"
        onChange={handleFileChange}
      />
      <input
        ref={replaceImageInputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp,image/gif"
        className="hidden"
        onChange={handleReplaceFileChange}
      />

      {/* Label + counter */}
      <div className="flex items-center justify-between">
        <label className="text-label-sm text-on-surface-variant font-semibold">
          Images
        </label>
        <span
          className={`text-label-xs font-semibold px-2 py-0.5 rounded-full ${
            atMax
              ? "bg-orange-100 text-orange-600"
              : images.length > 0
              ? "bg-primary/10 text-primary"
              : "bg-surface-container text-outline"
          }`}
        >
          {images.length} / {MAX_IMAGES_PER_POST}
        </span>
      </div>

      {/* Image Grid */}
      {images.length > 0 && (
        <div
          className="grid grid-cols-3 gap-2"
          onDragOver={(e) => {
            // Allow file drops onto the grid
            if (dragSrcIndex.current === null) e.preventDefault();
          }}
          onDrop={(e) => {
            if (dragSrcIndex.current === null) handleDrop(e);
          }}
        >
          {images.map((url, index) => (
            <div
              key={`${url}-${index}`}
              draggable
              onDragStart={() => handleDragStart(index)}
              onDragOver={(e) => handleDragOver(e, index)}
              onDragEnd={handleDragEnd}
              onDrop={(e) => handleDropOnImage(e, index)}
              className={`relative rounded-xl overflow-hidden border-2 transition-all cursor-grab active:cursor-grabbing ${
                dragOverIndex === index
                  ? "border-primary scale-105 shadow-lg"
                  : index === 0
                  ? "border-primary/40"
                  : "border-outline-variant/20"
              }`}
            >
              <div className="aspect-square bg-surface-container">
                <img
                  src={url}
                  alt={`Image ${index + 1}`}
                  className="w-full h-full object-cover"
                  draggable={false}
                />
              </div>

              {/* Primary badge */}
              {index === 0 && (
                <div className="absolute top-1 left-1 px-1.5 py-0.5 rounded-md bg-primary text-on-primary text-label-2xs font-bold">
                  Cover
                </div>
              )}

              {/* Order badge */}
              <div className="absolute top-1 right-1 w-5 h-5 rounded-full bg-black/50 text-white text-[10px] font-bold flex items-center justify-center">
                {index + 1}
              </div>

              {/* Action buttons */}
              <div className="absolute bottom-1 right-1 flex gap-1">
                <button
                  type="button"
                  onClick={(e) => { e.stopPropagation(); handleReplace(index); }}
                  className="w-6 h-6 rounded-md bg-black/60 text-white flex items-center justify-center hover:bg-black/80 transition-all"
                  title="Replace image"
                >
                  <span className="material-symbols-outlined text-[12px]">refresh</span>
                </button>
                <button
                  type="button"
                  onClick={(e) => { e.stopPropagation(); removeImage(index); }}
                  className="w-6 h-6 rounded-md bg-black/60 text-white flex items-center justify-center hover:bg-red-500/80 transition-all"
                  title="Remove image"
                >
                  <span className="material-symbols-outlined text-[12px]">close</span>
                </button>
              </div>

              {/* Drag handle hint */}
              <div className="absolute bottom-1 left-1 w-6 h-6 rounded-md bg-black/40 text-white/60 flex items-center justify-center">
                <span className="material-symbols-outlined text-[12px]">drag_indicator</span>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add Image Button / Drop Zone */}
      {!atMax && (
        <div
          onDragOver={(e) => { e.preventDefault(); }}
          onDrop={handleDrop}
          onClick={() => !uploading && fileInputRef.current?.click()}
          className={`flex items-center gap-3 px-4 py-3 rounded-xl border-2 border-dashed cursor-pointer transition-all
            ${uploading
              ? "border-primary/30 bg-primary/5 cursor-wait"
              : "border-outline-variant/30 hover:border-primary/40 hover:bg-surface-container/50"
            }`}
        >
          <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
            {uploading ? (
              <span className="w-4 h-4 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
            ) : (
              <span className="material-symbols-outlined text-primary text-[18px]">add_photo_alternate</span>
            )}
          </div>
          <div>
            <p className="text-label-sm font-semibold text-on-surface">
              {uploading ? "Uploading..." : "Add Image"}
            </p>
            <p className="text-label-xs text-outline">
              {atMax
                ? `Maximum ${MAX_IMAGES_PER_POST} images reached`
                : `PNG, JPG, WebP, GIF · up to 50MB · ${images.length}/${MAX_IMAGES_PER_POST}`}
            </p>
          </div>
        </div>
      )}

      {/* At max message */}
      {atMax && (
        <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-orange-50 border border-orange-100">
          <span className="material-symbols-outlined text-orange-500 text-[16px]">warning</span>
          <p className="text-label-xs text-orange-600 font-medium">
            Maximum {MAX_IMAGES_PER_POST} images allowed per post. Remove an image to add another.
          </p>
        </div>
      )}

      {/* Upload error */}
      {uploadError && (
        <div className="flex items-center gap-2 px-3 py-2 rounded-xl bg-red-50 border border-red-100">
          <span className="material-symbols-outlined text-red-500 text-[16px]">error</span>
          <p className="text-label-xs text-red-600">{uploadError}</p>
        </div>
      )}

      {images.length > 1 && (
        <p className="text-label-xs text-outline">
          <span className="material-symbols-outlined text-[12px] align-middle mr-0.5">drag_indicator</span>
          Drag images to reorder. First image is used as cover.
        </p>
      )}
    </div>
  );
}
