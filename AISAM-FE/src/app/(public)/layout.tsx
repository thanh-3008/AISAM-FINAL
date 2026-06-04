import { PublicOnlyRoute } from "@/lib/guards/auth-guard";

export default function PublicLayout({ children }: { children: React.ReactNode }) {
  return <PublicOnlyRoute>{children}</PublicOnlyRoute>;
}
