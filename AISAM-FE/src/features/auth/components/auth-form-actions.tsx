import Link from "next/link";
import { Button } from "@/components/ui/button";

export function AuthSubmitButton({
  pending,
  label
}: {
  pending: boolean;
  label: string;
}) {
  return (
    <Button className="w-full" type="submit" disabled={pending}>
      {pending ? "Processing..." : label}
    </Button>
  );
}

export function AuthFooterLink({
  copy,
  href,
  cta
}: {
  copy: string;
  href: string;
  cta: string;
}) {
  return (
    <p className="text-center text-sm text-muted-foreground">
      {copy}{" "}
      <Link className="font-medium text-primary" href={href}>
        {cta}
      </Link>
    </p>
  );
}
