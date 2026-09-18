"use client";

import { useEffect, useState } from "react";
import { readMedia, type MediaItem } from "@/services/composerService";
import MixedMediaGallery from "./MixedMediaGallery";

interface ContentMediaGalleryProps {
  contentId?: string;
  videoUrl?: string;
  images?: string[];
  title?: string;
}

export default function ContentMediaGallery({ contentId, videoUrl, images = [], title }: ContentMediaGalleryProps) {
  const [media, setMedia] = useState<MediaItem[] | null>(null);

  useEffect(() => {
    let active = true;
    setMedia(null);
    if (!contentId) return () => { active = false; };
    readMedia(contentId)
      .then(collection => { if (active) setMedia(collection.items); })
      .catch(() => { if (active) setMedia([]); });
    return () => { active = false; };
  }, [contentId]);

  return <MixedMediaGallery
    videoUrl={media?.length ? undefined : videoUrl}
    images={media?.length ? [] : images}
    media={media?.length ? media : undefined}
    title={title}
  />;
}
