"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { Button } from "@/components/ui/button";
import { useProfileDetail } from "@/features/profile/hooks/use-profiles";
import { profileStatusLabel, profileTypeLabel } from "@/features/profile/utils/profile-enums";

export default function ProfileDetailPage() {
  const params = useParams<{ id: string }>();
  const query = useProfileDetail(params.id);

  if (query.isLoading) {
    return <div className="rounded-2xl bg-card p-6 shadow-panel">Loading profile...</div>;
  }

  if (query.error) {
    return <AlertPanel title="Could not load profile" description={(query.error as Error).message} tone="error" />;
  }

  const profile = query.data;
  if (!profile) {
    return <AlertPanel title="No profile data" description="The backend returned no profile payload." tone="error" />;
  }

  return (
    <section className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-semibold">{profile.name}</h2>
          <p className="text-sm text-muted-foreground">
            {profileTypeLabel(profile.profileType)} · {profileStatusLabel(profile.status)}
          </p>
        </div>
        <Button asChild variant="outline">
          <Link href={`/profiles/${profile.id}/edit`}>Edit profile</Link>
        </Button>
      </div>
      <div className="grid gap-4 md:grid-cols-2">
        <div className="rounded-2xl bg-card p-5 shadow-panel">
          <p className="text-sm text-muted-foreground">Company</p>
          <p className="mt-2">{profile.companyName ?? "Not set"}</p>
        </div>
        <div className="rounded-2xl bg-card p-5 shadow-panel">
          <p className="text-sm text-muted-foreground">Avatar URL</p>
          <p className="mt-2 break-all">{profile.avatarUrl ?? "Not set"}</p>
        </div>
        <div className="rounded-2xl bg-card p-5 shadow-panel md:col-span-2">
          <p className="text-sm text-muted-foreground">Bio</p>
          <p className="mt-2">{profile.bio ?? "No bio yet"}</p>
        </div>
      </div>
    </section>
  );
}
