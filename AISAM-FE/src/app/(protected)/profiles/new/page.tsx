"use client";

import { useRouter } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { FormField } from "@/components/shared/form-field";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useCreateProfile } from "@/features/profile/hooks/use-profiles";
import { profileSchema } from "@/features/profile/schemas/profile-schemas";

export default function NewProfilePage() {
  const router = useRouter();
  const mutation = useCreateProfile();
  const form = useForm<z.infer<typeof profileSchema>>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      name: "",
      profileType: "Free",
      companyName: "",
      bio: "",
      avatarUrl: ""
    }
  });

  return (
    <section className="space-y-6">
      <div>
        <h2 className="text-2xl font-semibold">Create profile</h2>
        <p className="text-sm text-muted-foreground">This form submits as multipart/form-data but uses AvatarUrl only.</p>
      </div>
      <form
        className="grid gap-5 rounded-2xl bg-card p-6 shadow-panel"
        onSubmit={form.handleSubmit(async (values) => {
          const profile = await mutation.mutateAsync(values);
          router.push(`/profiles/${profile.id}`);
        })}
      >
        {mutation.error ? (
          <AlertPanel title="Could not create profile" description={(mutation.error as Error).message} tone="error" />
        ) : null}
        <FormField id="name" label="Profile name" error={form.formState.errors.name?.message}>
          <Input id="name" {...form.register("name")} />
        </FormField>
        <FormField id="profileType" label="Profile type" error={form.formState.errors.profileType?.message}>
          <select id="profileType" className="h-11 rounded-xl border bg-card px-3" {...form.register("profileType")}>
            <option value="Free">Free</option>
            <option value="Basic">Basic</option>
            <option value="Pro">Pro</option>
          </select>
        </FormField>
        <FormField id="companyName" label="Company name" error={form.formState.errors.companyName?.message}>
          <Input id="companyName" {...form.register("companyName")} />
        </FormField>
        <FormField id="bio" label="Bio" error={form.formState.errors.bio?.message}>
          <Textarea id="bio" {...form.register("bio")} />
        </FormField>
        <FormField id="avatarUrl" label="Avatar URL" error={form.formState.errors.avatarUrl?.message}>
          <Input id="avatarUrl" {...form.register("avatarUrl")} />
        </FormField>
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Creating..." : "Create profile"}
        </Button>
      </form>
    </section>
  );
}
