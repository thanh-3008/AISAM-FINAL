import Link from "next/link";

export default function NotFound() {
  return (
    <div className="min-h-screen bg-background flex items-center justify-center px-6">
      <div className="text-center max-w-md">
        <div className="w-20 h-20 mx-auto mb-6 bg-surface-container rounded-full flex items-center justify-center">
          <span className="material-symbols-outlined text-outline text-[48px]" style={{ fontVariationSettings: "'FILL' 1" }}>search_off</span>
        </div>
        <h1 className="text-display-lg text-on-surface font-bold mb-2">404</h1>
        <p className="text-headline-sm text-on-surface-variant mb-2">Page not found</p>
        <p className="text-body-md text-on-surface-variant mb-8">The page you&apos;re looking for doesn&apos;t exist or has been moved.</p>
        <Link
          href="/"
          className="inline-flex items-center gap-2 px-6 py-3 bg-primary text-on-primary rounded-xl text-label-sm font-bold hover:scale-105 transition-all active:scale-95"
        >
          <span className="material-symbols-outlined text-[16px]">arrow_back</span>
          Back to Home
        </Link>
      </div>
    </div>
  );
}
