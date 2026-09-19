"use client";

import { useEffect, useRef, useState } from "react";
import ImageLightboxModal from "./ImageLightboxModal";

interface MixedMediaGalleryProps {
  videoUrl?: string;
  images: string[];
  title?: string;
  onImagesChange?: (images: string[]) => void;
  media?: Array<{ url: string; mimeType: string }>;
  onRemove?: (index: number) => void;
  onReplace?: (index: number) => void;
}

export default function MixedMediaGallery({ videoUrl, images, title, onImagesChange, media, onRemove, onReplace }: MixedMediaGalleryProps) {
  const legacyItems = [
    ...(videoUrl ? [{ type: "video" as const, url: videoUrl }] : []),
    ...images.map(url => ({ type: "image" as const, url })),
  ];
  const items = media?.map(item => ({ type: item.mimeType.startsWith("video/") ? "video" as const : "image" as const, url: item.url })) ?? legacyItems;
  const [activeIndex, setActiveIndex] = useState(0);
  const [lightboxMediaIndex, setLightboxMediaIndex] = useState<number | null>(null);
  const previousItemCount = useRef(items.length);

  useEffect(() => {
    if (items.length > previousItemCount.current && previousItemCount.current > 0) {
      const firstNewImage = items.findIndex((item, index) => index >= previousItemCount.current && item.type === "image");
      setActiveIndex(firstNewImage >= 0 ? firstNewImage : items.length - 1);
    } else if (activeIndex >= items.length) {
      setActiveIndex(Math.max(0, items.length - 1));
    }
    previousItemCount.current = items.length;
  }, [activeIndex, items.length]);

  if (items.length === 0) return null;
  const active = items[activeIndex] ?? items[0];
  const select = (index: number) => setActiveIndex((index + items.length) % items.length);
  const removeAt = (index: number) => {
    if (onRemove) return onRemove(index);
    const imageIndex = index - (videoUrl ? 1 : 0);
    if (items[index]?.type === "image") onImagesChange?.(images.filter((_, current) => current !== imageIndex));
  };
  const lightboxImages = items.filter(item => item.type === "image").map(item => item.url);
  const lightboxImageIndex = lightboxMediaIndex === null
    ? 0
    : items.slice(0, lightboxMediaIndex + 1).filter(item => item.type === "image").length - 1;

  return <div className="w-full space-y-3">
    <div className="group relative flex aspect-video items-center justify-center overflow-hidden rounded-xl bg-black">
      {active.type === "video"
        ? <video key={active.url} src={active.url} controls className="h-full w-full object-contain" />
        : <img src={active.url} alt={title || `Image ${activeIndex}`} className="h-full w-full object-contain" />}

      <>
        <button type="button" aria-label="Previous media" disabled={items.length < 2} onClick={() => select(activeIndex - 1)} className="absolute left-3 top-1/2 flex h-10 w-10 -translate-y-1/2 items-center justify-center rounded-full bg-black/55 text-white shadow-md transition-colors hover:bg-black/80 disabled:cursor-default disabled:opacity-35">
          <span className="material-symbols-outlined">chevron_left</span>
        </button>
        <button type="button" aria-label="Next media" disabled={items.length < 2} onClick={() => select(activeIndex + 1)} className="absolute right-3 top-1/2 flex h-10 w-10 -translate-y-1/2 items-center justify-center rounded-full bg-black/55 text-white shadow-md transition-colors hover:bg-black/80 disabled:cursor-default disabled:opacity-35">
          <span className="material-symbols-outlined">chevron_right</span>
        </button>
        <span className="absolute right-3 top-3 rounded-lg bg-black/60 px-2.5 py-1 text-label-xs font-semibold text-white">{activeIndex + 1} / {items.length}</span>
      </>
    </div>

    <div className="flex gap-2 overflow-x-auto pb-1">
      {items.map((item, index) => <div key={`${item.type}-${item.url}-${index}`} className={`group relative h-24 w-24 shrink-0 overflow-hidden rounded-lg border-2 ${index === activeIndex ? "border-primary" : "border-outline-variant/20"}`}>
        <button type="button" aria-label={`View media ${index + 1}`} onClick={() => select(index)} className="h-full w-full">
          {item.type === "video"
            ? <><video src={item.url} muted preload="metadata" className="h-full w-full object-cover" /><span className="material-symbols-outlined absolute inset-0 flex items-center justify-center bg-black/20 text-white">play_arrow</span></>
            : <img src={item.url} alt="" className="h-full w-full object-cover" />}
        </button>
        {item.type === "image" && <div className="absolute bottom-1 right-1 flex max-w-[calc(100%-0.5rem)] gap-1 opacity-100 transition-opacity sm:opacity-0 sm:group-hover:opacity-100 sm:group-focus-within:opacity-100">
          <button type="button" aria-label={`Enlarge image ${index + 1}`} title="Enlarge image" onClick={() => setLightboxMediaIndex(index)} className={`material-symbols-outlined h-6 w-6 items-center justify-center rounded-md bg-black/70 text-[15px] text-white hover:bg-black/90 ${index === activeIndex ? "flex" : "hidden sm:flex"}`}>fullscreen</button>
          {onReplace && <button type="button" aria-label={`Replace image ${index + 1}`} title="Replace image" onClick={() => onReplace(index)} className={`material-symbols-outlined h-6 w-6 items-center justify-center rounded-md bg-black/70 text-[15px] text-white hover:bg-black/90 ${index === activeIndex ? "flex" : "hidden sm:flex"}`}>refresh</button>}
          {(onRemove || onImagesChange) && <button type="button" aria-label={`Remove image ${index + 1}`} title="Remove image" onClick={() => removeAt(index)} className="material-symbols-outlined flex h-6 w-6 items-center justify-center rounded-md bg-black/65 text-[15px] text-white hover:bg-red-600">close</button>}
        </div>}
      </div>)}
    </div>

    {lightboxMediaIndex !== null && <ImageLightboxModal
      images={lightboxImages}
      initialIndex={Math.max(0, lightboxImageIndex)}
      isOpen
      onClose={() => setLightboxMediaIndex(null)}
      title={title || "Uploaded images"}
    />}
  </div>;
}
