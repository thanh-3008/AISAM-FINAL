"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getToken, isAdmin } from "@/lib/auth";

export default function AdminGuard({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [verified, setVerified] = useState(false);

  useEffect(() => {
    const token = getToken();
    if (!token) {
      router.replace("/login");
      return;
    }

    if (!isAdmin()) {
      router.replace("/dashboard");
      return;
    }

    setVerified(true);
  }, [router]);

  if (!verified) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-[#faf8ff]">
        <div className="flex flex-col items-center gap-4">
          <div className="w-8 h-8 border-2 border-[#004ccd] border-t-transparent rounded-full animate-spin" />
          <p className="text-sm text-[#424656]">Verifying access...</p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
