import { ProtectedShell } from "@/components/layout/protected-shell";
import { ProtectedRoute } from "@/lib/guards/auth-guard";

export default function ProtectedLayout({ children }: { children: React.ReactNode }) {
  return (
    <ProtectedRoute>
      <ProtectedShell>{children}</ProtectedShell>
    </ProtectedRoute>
  );
}
