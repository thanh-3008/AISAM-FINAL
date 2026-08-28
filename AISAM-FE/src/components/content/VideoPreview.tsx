"use client";

import { useEffect, useState } from "react";

interface VideoPreviewProps {
  src?: string | null;
  poster?: string | null;
  className?: string;
  videoClassName?: string;
  controls?: boolean;
  autoPlay?: boolean;
  muted?: boolean;
  loop?: boolean;
  emptyLabel?: string;
}

export default function VideoPreview({
  src,
  poster,
  className = "aspect-video",
  videoClassName = "object-contain",
  controls = true,
  autoPlay = false,
  muted = false,
  loop = false,
  emptyLabel = "Video preview is unavailable.",
}: VideoPreviewProps) {
  const [loading, setLoading] = useState(Boolean(src));
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    setLoading(Boolean(src));
    setFailed(false);
  }, [src]);

  return (
    <div className={`relative overflow-hidden bg-black ${className}`}>
      {src && !failed ? (
        <video
          key={src}
          src={src}
          poster={poster || undefined}
          controls={controls}
          autoPlay={autoPlay}
          muted={muted}
          loop={loop}
          playsInline
          preload="metadata"
          aria-label="Selected video preview"
          className={`w-full h-full bg-black ${videoClassName}`}
          onLoadedMetadata={() => setLoading(false)}
          onCanPlay={() => setLoading(false)}
          onError={() => {
            setLoading(false);
            setFailed(true);
          }}
          onClick={(event) => event.stopPropagation()}
        />
      ) : (
        <div className="absolute inset-0 flex flex-col items-center justify-center gap-2 px-4 text-center text-white/70">
          <span className="material-symbols-outlined text-4xl">videocam_off</span>
          <span className="text-label-xs">{failed ? "This video cannot be previewed. Check the file format and try again." : emptyLabel}</span>
        </div>
      )}

      {loading && src && !failed && (
        <div className="pointer-events-none absolute inset-0 flex items-center justify-center bg-black/35 text-white">
          <span className="w-7 h-7 rounded-full border-2 border-white/30 border-t-white animate-spin" aria-label="Loading video preview" />
        </div>
      )}
    </div>
  );
}
