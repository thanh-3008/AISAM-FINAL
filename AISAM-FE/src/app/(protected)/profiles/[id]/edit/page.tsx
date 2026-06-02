"use client";

import { useParams, useRouter } from "next/navigation";
import { useEffect } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { AlertPanel } from "@/components/feedback/alert-panel";
import { FormField } from "@/components/shared/form-field";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useProfileDetail, useUpdateProfile } from "@/features/profile/hooks/use-profiles";
import { profileSchema } from "@/features/profile/schemas/profile-schemas";
import { profileTypeLabel } from "@/features/profile/utils/profile-enums";

export default function EditProfilePage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const detail = useProfileDetail(params.id);
  const mutation = useUpdateProfile(params.id);
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

  useEffect(() => {
    if (!detail.data) {
      return;
    }
    form.reset({
      name: detail.data.name,
      profileType: profileTypeLabel(detail.data.profileType) as "Free" | "Basic" | "Pro",
      companyName: detail.data.companyName ?? "",
      bio: detail.data.bio ?? "",
      avatarUrl: detail.data.avatarUrl ?? ""
    });
  }, [detail.data, form]);

  return (
    <section className="space-y-6">
      <div>
        <h2 className="text-2xl font-semibold">Edit profile</h2>
        <p className="text-sm text-muted-foreground">Update the current business context without avatar file upload.</p>
      </div>
      {detail.error ? (
        <AlertPanel title="Could not load profile" description={(detail.error as Error).message} tone="error" />
      ) : null}
      <form
        className="grid gap-5 rounded-2xl bg-card p-6 shadow-panel"
        onSubmit={form.handleSubmit(async (values) => {
          await mutation.mutateAsync(values);
          router.push(`/profiles/${params.id}`);
        })}
      >
        {mutation.error ? (
          <AlertPanel title="Could not update profile" description={(mutation.error as Error).message} tone="error" />
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
        <Button type="submit" disabled={mutation.isPending || detail.isLoading}>
          {mutation.isPending ? "Saving..." : "Save changes"}
        </Button>
      </form>
    </section>
  );
}
