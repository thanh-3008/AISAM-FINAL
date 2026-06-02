"use client";

import Link from "next/link";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { Button } from "@/components/ui/button";
import { profileTypeLabel } from "@/features/profile/utils/profile-enums";
import { useProfileStore } from "@/stores/profile-store";

export default function DashboardPage() {
  const activeProfile = useProfileStore((state) => state.activeProfile);

  return (
    <section className="space-y-6">
      <div>
        <h2 className="text-3xl font-semibold">Dashboard foundation</h2>
        <p className="mt-2 text-sm text-muted-foreground">
          This route is intentionally light. Later modules will attach `X-Profile-Id` using the selected profile.
        </p>
      </div>
      {!activeProfile ? (
        <AlertPanel
          title="No active profile selected"
          description="Choose a profile before working with later features such as brand, content, social, or payment."
          tone="error"
        />
      ) : (
        <AlertPanel
          title={`Active profile: ${activeProfile.name}`}
          description={`Profile type: ${profileTypeLabel(activeProfile.profileType)}. Future modules can now consume this state.`}
          tone="success"
        />
      )}
      <Button asChild>
        <Link href="/profiles">Go to profiles</Link>
      </Button>
    </section>
  );
}
