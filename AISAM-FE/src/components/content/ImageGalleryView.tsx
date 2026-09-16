"use client";

import React, { useState } from "react";
import ImageLightboxModal from "./ImageLightboxModal";

interface ImageGalleryViewProps {
  images: string[];
  title?: string;
  className?: string;
  showCoverBadge?: boolean;
  aspectRatio?: "video" | "square" | "auto";
}

export default function ImageGalleryView({
  images,
  title,
  className = "",
  showCoverBadge = true,
  aspectRatio = "video",
}: ImageGalleryViewProps) {
  const [activeIndex, setActiveIndex] = useState(0);
  const [isLightboxOpen, setIsLightboxOpen] = useState(false);

  if (!images || images.length === 0) {
    return (
      <div className={`w-full aspect-video bg-gradient-to-br from-surface-container to-surface-container-high rounded-2xl flex items-center justify-center border border-outline-variant/20 ${className}`}>
        <div className="text-center p-6">
          <div className="w-14 h-14 mx-auto rounded-2xl bg-primary/10 flex items-center justify-center text-primary mb-3">
            <span className="material-symbols-outlined text-[28px]">image</span>
          </div>
          <p className="text-body-sm text-outline font-medium">Chưa có ảnh nào được tải lên</p>
        </div>
      </div>
    );
  }

  const safeIndex = Math.min(activeIndex, images.length - 1);
  const currentUrl = images[safeIndex] || images[0];

  const aspectClass =
    aspectRatio === "square"
      ? "aspect-square"
      : aspectRatio === "video"
      ? "aspect-video"
      : "max-h-[500px]";

  return (
    <div className={`space-y-3 w-full max-w-3xl mx-auto ${className}`}>
      {/* Featured / Active Image Card */}
      <div
        className={`relative group rounded-2xl overflow-hidden bg-gradient-to-br from-surface-container to-surface-container-high border border-outline-variant/20 shadow-sm cursor-pointer ${aspectClass} flex items-center justify-center`}
        onClick={() => setIsLightboxOpen(true)}
      >
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={currentUrl}
          alt={title || `Ảnh ${safeIndex + 1}`}
          className="w-full h-full object-contain transition-transform duration-300 group-hover:scale-[1.01]"
        />

        {/* Cover badge on first image */}
        {showCoverBadge && safeIndex === 0 && (
          <div className="absolute top-3 left-3 px-2.5 py-1 rounded-lg bg-primary text-on-primary text-label-xs font-bold shadow-md flex items-center gap-1 backdrop-blur-xs">
            <span className="material-symbols-outlined text-[14px]">star</span>
            Ảnh bìa
          </div>
        )}

        {/* Counter badge */}
        {images.length > 1 && (
          <div className="absolute top-3 right-3 px-2.5 py-1 rounded-lg bg-black/60 backdrop-blur-md text-white text-label-xs font-semibold shadow-md flex items-center gap-1.5">
            <span className="material-symbols-outlined text-[14px]">photo_library</span>
            <span>{safeIndex + 1} / {images.length}</span>
          </div>
        )}

        {/* Expand / Lightbox hint button */}
        <div className="absolute bottom-3 right-3 opacity-0 group-hover:opacity-100 transition-opacity flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-black/70 backdrop-blur-md text-white text-label-xs font-medium shadow-lg hover:bg-black/85">
          <span className="material-symbols-outlined text-[16px]">fullscreen</span>
          <span>Phóng to</span>
        </div>

        {/* Previous button overlay */}
        {images.length > 1 && (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              setActiveIndex((prev) => (prev - 1 + images.length) % images.length);
            }}
            className="absolute left-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-black/50 hover:bg-black/80 text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all active:scale-95 shadow-md"
            title="Ảnh trước"
          >
            <span className="material-symbols-outlined text-[20px]">chevron_left</span>
          </button>
        )}

        {/* Next button overlay */}
        {images.length > 1 && (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              setActiveIndex((prev) => (prev + 1) % images.length);
            }}
            className="absolute right-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-black/50 hover:bg-black/80 text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all active:scale-95 shadow-md"
            title="Ảnh tiếp theo"
          >
            <span className="material-symbols-outlined text-[20px]">chevron_right</span>
          </button>
        )}
      </div>

      {/* Thumbnails Filmstrip */}
      {images.length > 1 && (
        <div className="space-y-1.5">
          <div className="flex items-center justify-between px-1 text-label-xs text-outline">
            <span className="flex items-center gap-1 font-medium">
              <span className="material-symbols-outlined text-[14px]">collections</span>
              Tất cả ảnh đã upload ({images.length})
            </span>
            <button
              type="button"
              onClick={() => setIsLightboxOpen(true)}
              className="text-primary hover:underline flex items-center gap-0.5 font-semibold"
            >
              Xem toàn màn hình
              <span className="material-symbols-outlined text-[14px]">open_in_full</span>
            </button>
          </div>

          <div className="grid grid-cols-5 gap-2">
            {images.map((url, idx) => (
              <button
                key={`${url}-${idx}`}
                type="button"
                onClick={() => setActiveIndex(idx)}
                className={`relative rounded-xl overflow-hidden aspect-square border-2 transition-all group ${
                  idx === safeIndex
                    ? "border-primary ring-2 ring-primary/20 shadow-md scale-[1.02]"
                    : "border-outline-variant/20 hover:border-primary/40 opacity-80 hover:opacity-100"
                }`}
              >
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={url}
                  alt={`Thumbnail ${idx + 1}`}
                  className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                />

                {/* Index badge */}
                <span className="absolute top-1 right-1 w-4 h-4 rounded-full bg-black/60 text-white text-[9px] font-bold flex items-center justify-center">
                  {idx + 1}
                </span>

                {/* Cover label on first thumbnail */}
                {showCoverBadge && idx === 0 && (
                  <span className="absolute bottom-1 left-1 px-1 py-0.2 rounded bg-primary text-on-primary text-[8px] font-bold leading-tight">
                    Bìa
                  </span>
                )}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Fullscreen Lightbox Modal */}
      <ImageLightboxModal
        images={images}
        initialIndex={safeIndex}
        isOpen={isLightboxOpen}
        onClose={() => setIsLightboxOpen(false)}
        title={title}
      />
    </div>
  );
}
