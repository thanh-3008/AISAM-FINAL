"use client";

interface TeamEmptyStateProps {
  hasFilters: boolean;
  onCreate?: () => void;
  onInvite: () => void;
  isOwner?: boolean;
}

export default function TeamEmptyState({ hasFilters, onCreate, onInvite, isOwner = false }: TeamEmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-center gap-6 animate-fade-up">
      <div className="relative">
        <div className="w-32 h-32 rounded-full bg-gradient-to-br from-primary/10 to-secondary/10 flex items-center justify-center">
          <div className="w-24 h-24 rounded-full bg-gradient-to-br from-primary/20 to-secondary/20 flex items-center justify-center animate-float">
            <span className="material-symbols-outlined text-primary text-5xl">group</span>
          </div>
        </div>
        <div className="absolute top-2 right-2 w-8 h-8 rounded-full bg-emerald-500/20 flex items-center justify-center animate-float" style={{ animationDelay: "0.2s" }}>
          <span className="material-symbols-outlined text-emerald-500 text-sm">person_add</span>
        </div>
        <div className="absolute bottom-4 left-0 w-8 h-8 rounded-full bg-violet-500/20 flex items-center justify-center animate-float" style={{ animationDelay: "0.4s" }}>
          <span className="material-symbols-outlined text-violet-500 text-sm">schema</span>
        </div>
        <div className="absolute bottom-0 right-4 w-8 h-8 rounded-full bg-blue-500/20 flex items-center justify-center animate-float" style={{ animationDelay: "0.6s" }}>
          <span className="material-symbols-outlined text-blue-500 text-sm">mail</span>
        </div>
      </div>

      <div className="max-w-md space-y-2">
        <h2 className="text-headline-sm text-on-surface font-bold">
          {hasFilters ? "No matching members" : "No members yet"}
        </h2>
        <p className="text-body-sm text-on-surface-variant">
          {hasFilters
            ? "Try adjusting your filters or search criteria to find what you're looking for."
            : "Invite members to start collaborating in this workspace."}
        </p>
      </div>

      {!hasFilters && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 max-w-lg mt-4">
          <div className="flex flex-col items-center gap-2 p-4 bg-surface-container-low rounded-xl">
            <span className="material-symbols-outlined text-primary text-2xl">person_add</span>
            <span className="text-label-sm text-on-surface font-medium text-center">Invite team members</span>
          </div>
          <div className="flex flex-col items-center gap-2 p-4 bg-surface-container-low rounded-xl">
            <span className="material-symbols-outlined text-primary text-2xl">admin_panel_settings</span>
            <span className="text-label-sm text-on-surface font-medium text-center">Assign roles &amp; permissions</span>
          </div>
          <div className="flex flex-col items-center gap-2 p-4 bg-surface-container-low rounded-xl">
            <span className="material-symbols-outlined text-primary text-2xl">work</span>
            <span className="text-label-sm text-on-surface font-medium text-center">Collaborate on brands</span>
          </div>
        </div>
      )}

      {!hasFilters && isOwner && (
        <div className="flex gap-3 mt-4">
          <button
            onClick={onInvite}
            className="inline-flex items-center gap-2 px-6 py-3 border border-outline-variant/20 text-outline rounded-xl font-semibold text-label-md hover:text-on-surface hover:bg-surface-container active:scale-[0.97] transition-all"
          >
            <span className="material-symbols-outlined text-[18px]">person_add</span>
            Invite Member
          </button>
          {onCreate && (
            <button
              onClick={onCreate}
              className="inline-flex items-center gap-2 px-6 py-3 bg-primary text-on-primary rounded-xl font-semibold text-label-md hover:shadow-lg hover:shadow-primary/25 active:scale-[0.97] transition-all"
            >
              <span className="material-symbols-outlined text-[18px]">group_add</span>
              Create Team
            </button>
          )}
        </div>
      )}
    </div>
  );
}
