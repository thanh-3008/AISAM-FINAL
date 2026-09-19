"use client";

import React, { useState, useEffect, useCallback } from "react";

interface ImageLightboxModalProps {
  images: string[];
  initialIndex?: number;
  isOpen: boolean;
  onClose: () => void;
  title?: string;
}

export default function ImageLightboxModal({
  images,
  initialIndex = 0,
  isOpen,
  onClose,
  title,
}: ImageLightboxModalProps) {
  const [currentIndex, setCurrentIndex] = useState(initialIndex);
  const [zoomLevel, setZoomLevel] = useState(1);
  const [rotation, setRotation] = useState(0);

  // Sync initialIndex when modal opens
  useEffect(() => {
    if (isOpen) {
      setCurrentIndex(Math.max(0, Math.min(initialIndex, images.length - 1)));
      setZoomLevel(1);
      setRotation(0);
    }
  }, [isOpen, initialIndex, images.length]);

  const handleNext = useCallback(() => {
    if (images.length <= 1) return;
    setCurrentIndex((prev) => (prev + 1) % images.length);
    setZoomLevel(1);
    setRotation(0);
  }, [images.length]);

  const handlePrev = useCallback(() => {
    if (images.length <= 1) return;
    setCurrentIndex((prev) => (prev - 1 + images.length) % images.length);
    setZoomLevel(1);
    setRotation(0);
  }, [images.length]);

  // Keyboard navigation
  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose();
      } else if (e.key === "ArrowRight") {
        handleNext();
      } else if (e.key === "ArrowLeft") {
        handlePrev();
      } else if (e.key === "+" || e.key === "=") {
        setZoomLevel((z) => Math.min(3, z + 0.25));
      } else if (e.key === "-") {
        setZoomLevel((z) => Math.max(1, z - 0.25));
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, onClose, handleNext, handlePrev]);

  if (!isOpen || images.length === 0) return null;

  const currentImage = images[currentIndex] || "";

  return (
    <div
      className="fixed inset-0 z-[100] flex flex-col items-center justify-between bg-black/92 backdrop-blur-md animate-in fade-in duration-200 select-none"
      onClick={onClose}
    >
      {/* Top Bar */}
      <div
        className="w-full flex items-center justify-between px-6 py-4 bg-gradient-to-b from-black/80 to-transparent z-10"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center gap-3 text-white">
          <span className="material-symbols-outlined text-[24px] text-primary">photo_library</span>
          <div className="min-w-0">
            <h3 className="text-body-sm font-semibold truncate max-w-md">
              {title || "View image"}
            </h3>
            <p className="text-label-xs text-white/60">
              Image {currentIndex + 1} / {images.length}
            </p>
          </div>
        </div>

        {/* Action Controls */}
        <div className="flex items-center gap-2">
          {/* Zoom controls */}
          <div className="flex items-center bg-white/10 rounded-xl p-1 border border-white/10">
            <button
              onClick={() => setZoomLevel((z) => Math.max(1, z - 0.25))}
              disabled={zoomLevel <= 1}
              className="p-1.5 hover:bg-white/10 rounded-lg text-white/80 hover:text-white disabled:opacity-30 transition-all"
              title="Zoom out (-)"
            >
              <span className="material-symbols-outlined text-[18px]">zoom_out</span>
            </button>
            <span className="px-2 text-label-xs text-white/90 font-mono min-w-[40px] text-center">
              {Math.round(zoomLevel * 100)}%
            </span>
            <button
              onClick={() => setZoomLevel((z) => Math.min(3, z + 0.25))}
              disabled={zoomLevel >= 3}
              className="p-1.5 hover:bg-white/10 rounded-lg text-white/80 hover:text-white disabled:opacity-30 transition-all"
              title="Zoom in (+)"
            >
              <span className="material-symbols-outlined text-[18px]">zoom_in</span>
            </button>
            {zoomLevel > 1 && (
              <button
                onClick={() => setZoomLevel(1)}
                className="p-1.5 hover:bg-white/10 rounded-lg text-white/80 hover:text-white transition-all ml-1 text-label-2xs font-semibold"
                title="Reset to 100%"
              >
                1:1
              </button>
            )}
          </div>

          {/* Rotate */}
          <button
            onClick={() => setRotation((r) => (r + 90) % 360)}
            className="p-2 hover:bg-white/10 rounded-xl text-white/80 hover:text-white transition-all border border-white/10"
            title="Rotate image"
          >
            <span className="material-symbols-outlined text-[20px]">rotate_right</span>
          </button>

          {/* Open in new tab / download */}
          <a
            href={currentImage}
            target="_blank"
            rel="noopener noreferrer"
            className="p-2 hover:bg-white/10 rounded-xl text-white/80 hover:text-white transition-all border border-white/10"
            title="Open original in new tab"
            onClick={(e) => e.stopPropagation()}
          >
            <span className="material-symbols-outlined text-[20px]">open_in_new</span>
          </a>

          {/* Close button */}
          <button
            onClick={onClose}
            className="p-2 hover:bg-red-500/30 rounded-xl text-white/80 hover:text-red-300 transition-all border border-white/10"
            title="Close (Esc)"
          >
            <span className="material-symbols-outlined text-[20px]">close</span>
          </button>
        </div>
      </div>

      {/* Main Image Area */}
      <div
        className="flex-1 w-full flex items-center justify-center relative px-12 py-2 overflow-hidden"
        onClick={onClose}
      >
        {/* Prev button */}
        {images.length > 1 && (
          <button
            onClick={(e) => {
              e.stopPropagation();
              handlePrev();
            }}
            className="absolute left-4 top-1/2 -translate-y-1/2 w-12 h-12 rounded-full bg-black/60 hover:bg-black/80 text-white flex items-center justify-center border border-white/20 transition-all z-20 hover:scale-110 active:scale-95"
            title="Previous image (Left arrow)"
          >
            <span className="material-symbols-outlined text-[26px]">chevron_left</span>
          </button>
        )}

        {/* Current Image */}
        <div
          className="max-w-[90vw] max-h-[75vh] flex items-center justify-center transition-transform duration-200"
          style={{
            transform: `scale(${zoomLevel}) rotate(${rotation}deg)`,
          }}
          onClick={(e) => e.stopPropagation()}
        >
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img
            src={currentImage}
            alt={title || `Image ${currentIndex + 1}`}
            className="max-w-full max-h-[75vh] object-contain rounded-xl shadow-2xl transition-all cursor-zoom-in"
            onClick={() => setZoomLevel((z) => (z === 1 ? 2 : 1))}
            draggable={false}
          />
        </div>

        {/* Next button */}
        {images.length > 1 && (
          <button
            onClick={(e) => {
              e.stopPropagation();
              handleNext();
            }}
            className="absolute right-4 top-1/2 -translate-y-1/2 w-12 h-12 rounded-full bg-black/60 hover:bg-black/80 text-white flex items-center justify-center border border-white/20 transition-all z-20 hover:scale-110 active:scale-95"
            title="Next image (Right arrow)"
          >
            <span className="material-symbols-outlined text-[26px]">chevron_right</span>
          </button>
        )}
      </div>

      {/* Filmstrip Thumbnails Bar at Bottom */}
      {images.length > 1 && (
        <div
          className="w-full flex items-center justify-center gap-2.5 px-6 py-4 bg-gradient-to-t from-black/80 via-black/50 to-transparent z-10 overflow-x-auto"
          onClick={(e) => e.stopPropagation()}
        >
          {images.map((url, i) => (
            <button
              key={`${url}-${i}`}
              onClick={() => {
                setCurrentIndex(i);
                setZoomLevel(1);
                setRotation(0);
              }}
              className={`relative rounded-lg overflow-hidden shrink-0 w-16 h-16 transition-all border-2 ${
                i === currentIndex
                  ? "border-primary scale-110 ring-2 ring-primary/40 shadow-lg"
                  : "border-white/20 opacity-60 hover:opacity-100 hover:scale-105"
              }`}
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={url}
                alt={`Thumbnail ${i + 1}`}
                className="w-full h-full object-cover"
              />
              <span className="absolute bottom-0.5 right-0.5 px-1 rounded bg-black/70 text-white text-[9px] font-bold">
                {i + 1}
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
