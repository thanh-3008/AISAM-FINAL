import type { ProfileResponseDto } from "@/features/profile/types/profile";
import { Button } from "@/components/ui/button";
import { profileStatusLabel, profileTypeLabel } from "@/features/profile/utils/profile-enums";

export function ProfileCard({
  profile,
  active,
  onSelect,
  onEdit,
  onDelete,
  onRestore
}: {
  profile: ProfileResponseDto;
  active: boolean;
  onSelect: () => void;
  onEdit: () => void;
  onDelete: () => void;
  onRestore: () => void;
}) {
  return (
    <div className="rounded-2xl border bg-card p-5 shadow-panel">
      <div className="flex items-start justify-between gap-3">
        <div>
          <h3 className="text-lg font-semibold">{profile.name}</h3>
          <p className="text-sm text-muted-foreground">
            {profileTypeLabel(profile.profileType)} · {profileStatusLabel(profile.status)}
          </p>
        </div>
        {active ? <span className="rounded-full bg-primary/10 px-3 py-1 text-xs text-primary">Active</span> : null}
      </div>
      {profile.companyName ? <p className="mt-4 text-sm">{profile.companyName}</p> : null}
      {profile.bio ? <p className="mt-2 text-sm text-muted-foreground">{profile.bio}</p> : null}
      <div className="mt-5 flex flex-wrap gap-2">
        <Button variant="primary" size="sm" onClick={onSelect}>
          {active ? "Selected" : "Select"}
        </Button>
        <Button variant="outline" size="sm" onClick={onEdit}>
          Edit
        </Button>
        {profileStatusLabel(profile.status) === "Cancelled" ? (
          <Button variant="secondary" size="sm" onClick={onRestore}>
            Restore
          </Button>
        ) : (
          <Button variant="ghost" size="sm" onClick={onDelete}>
            Delete
          </Button>
        )}
      </div>
    </div>
  );
}
