"use client";

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="min-h-screen bg-background flex items-center justify-center px-6">
      <div className="text-center max-w-md">
        <div className="w-16 h-16 mx-auto mb-6 bg-error/10 rounded-2xl flex items-center justify-center">
          <span className="material-symbols-outlined text-error text-[32px]" style={{ fontVariationSettings: "'FILL' 1" }}>error_outline</span>
        </div>
        <h1 className="text-headline-lg text-on-surface font-bold mb-3">Something went wrong</h1>
        <p className="text-body-md text-on-surface-variant mb-8">An unexpected error occurred. Please try again.</p>
        <button
          onClick={reset}
          className="px-6 py-3 bg-primary text-on-primary rounded-xl text-label-sm font-bold hover:scale-105 transition-all active:scale-95"
        >
          Try Again
        </button>
      </div>
    </div>
  );
}
