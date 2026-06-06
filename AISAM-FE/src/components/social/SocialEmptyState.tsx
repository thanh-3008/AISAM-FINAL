"use client";

interface SocialEmptyStateProps {
  hasFilters: boolean;
  onConnect: () => void;
}

export default function SocialEmptyState({ hasFilters, onConnect }: SocialEmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-center gap-6 animate-fade-up">
      <div className="relative">
        <div className="w-32 h-32 rounded-full bg-gradient-to-br from-primary/10 to-secondary/10 flex items-center justify-center">
          <div className="w-24 h-24 rounded-full bg-gradient-to-br from-primary/20 to-secondary/20 flex items-center justify-center animate-float">
            <span className="material-symbols-outlined text-primary text-5xl">public</span>
          </div>
        </div>
        <div className="absolute top-2 right-2 w-8 h-8 rounded-full bg-blue-500/20 flex items-center justify-center animate-float" style={{ animationDelay: "0.2s" }}>
          <span className="material-symbols-outlined text-blue-500 text-sm">facebook</span>
        </div>
        <div className="absolute bottom-4 left-0 w-8 h-8 rounded-full bg-pink-500/20 flex items-center justify-center animate-float" style={{ animationDelay: "0.4s" }}>
          <span className="material-symbols-outlined text-pink-500 text-sm">instagram</span>
        </div>
        <div className="absolute bottom-0 right-4 w-8 h-8 rounded-full bg-gray-500/20 flex items-center justify-center animate-float" style={{ animationDelay: "0.6s" }}>
          <span className="material-symbols-outlined text-gray-700 text-sm">tiktok</span>
        </div>
      </div>

      <div className="max-w-md space-y-2">
        <h2 className="text-headline-sm text-on-surface font-bold">
          {hasFilters ? "No matching accounts" : "No social accounts yet"}
        </h2>
        <p className="text-body-sm text-on-surface-variant">
          {hasFilters
            ? "Try adjusting your filters or search criteria to find what you're looking for."
            : "Connect your social media accounts to start publishing content across multiple platforms from one place."}
        </p>
      </div>

      {!hasFilters && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 max-w-lg mt-4">
          <div className="flex flex-col items-center gap-2 p-4 bg-surface-container-low rounded-xl">
            <span className="material-symbols-outlined text-primary text-2xl">link</span>
            <span className="text-[11px] text-on-surface font-medium text-center">Link multiple pages & profiles</span>
          </div>
          <div className="flex flex-col items-center gap-2 p-4 bg-surface-container-low rounded-xl">
            <span className="material-symbols-outlined text-primary text-2xl">schedule</span>
            <span className="text-[11px] text-on-surface font-medium text-center">Schedule posts in advance</span>
          </div>
          <div className="flex flex-col items-center gap-2 p-4 bg-surface-container-low rounded-xl">
            <span className="material-symbols-outlined text-primary text-2xl">analytics</span>
            <span className="text-[11px] text-on-surface font-medium text-center">Track performance metrics</span>
          </div>
        </div>
      )}

      {!hasFilters && (
        <button
          onClick={onConnect}
          className="mt-4 inline-flex items-center gap-2 px-6 py-3 bg-primary text-on-primary rounded-xl font-semibold text-label-md hover:shadow-lg hover:shadow-primary/25 active:scale-[0.97] transition-all"
        >
          <span className="material-symbols-outlined text-[18px]">add_link</span>
          Connect Your First Account
        </button>
      )}
    </div>
  );
}
