"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { Button } from "@/components/ui/button";
import { ProfileCard } from "@/features/profile/components/profile-card";
import { useDeleteProfile, useProfiles, useRestoreProfile } from "@/features/profile/hooks/use-profiles";
import { useProfileStore } from "@/stores/profile-store";

export default function ProfilesPage() {
  const router = useRouter();
  const query = useProfiles();
  const activeProfile = useProfileStore((state) => state.activeProfile);
  const setActiveProfile = useProfileStore((state) => state.setActiveProfile);
  const deleteProfile = useDeleteProfile();
  const restoreProfile = useRestoreProfile();

  return (
    <section className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-semibold">Profiles</h2>
          <p className="text-sm text-muted-foreground">Select the active business context for later modules.</p>
        </div>
        <Button asChild>
          <Link href="/profiles/new">Create profile</Link>
        </Button>
      </div>
      {query.error ? (
        <AlertPanel title="Could not load profiles" description={(query.error as Error).message} tone="error" />
      ) : null}
      {query.isLoading ? <div className="rounded-2xl bg-card p-6 shadow-panel">Loading profiles...</div> : null}
      {query.data?.length === 0 ? (
        <AlertPanel title="No profiles yet" description="Create your first business profile to continue." />
      ) : null}
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {query.data?.map((profile) => (
          <ProfileCard
            key={profile.id}
            profile={profile}
            active={activeProfile?.id === profile.id}
            onSelect={() => {
              setActiveProfile(profile);
              router.push("/dashboard");
            }}
            onEdit={() => router.push(`/profiles/${profile.id}/edit`)}
            onDelete={() => deleteProfile.mutate(profile.id)}
            onRestore={() => restoreProfile.mutate(profile.id)}
          />
        ))}
      </div>
    </section>
  );
}
