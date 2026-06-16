"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function CreateProfileRedirect() {
  const router = useRouter();
  useEffect(() => { router.replace("/profiles"); }, [router]);
  return null;
}
