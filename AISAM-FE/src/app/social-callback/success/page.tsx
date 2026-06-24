"use client";

import Link from "next/link";
import { Suspense, useMemo } from "react";
import { useSearchParams } from "next/navigation";

type ResultStatus = "success" | "failure" | "partial";

function ResultCard() {
  const searchParams = useSearchParams();
  const status = (searchParams.get("status") || "success") as ResultStatus;
  const accountName = searchParams.get("account") || "Facebook Account";
  const handle = searchParams.get("handle") || "@facebook";
  const details = searchParams.get("details") || "Your social media account has been authenticated and is ready for use in AISAM.";

  const content = useMemo(() => {
    if (status === "failure") {
      return {
        icon: "error",
        iconWrap: "bg-error-container",
        iconColor: "text-error",
        title: "Connection Failed",
        description: "We couldn't connect your account due to missing permissions.",
        primaryLabel: "Retry Connection",
        primaryHref: "/social",
        secondaryLabel: "Contact Support",
        secondaryHref: "/social",
      };
    }

    if (status === "partial") {
      return {
        icon: "warning",
        iconWrap: "bg-warning-amber/10",
        iconColor: "text-warning-amber",
        title: "Account Linked",
        description: "Authentication succeeded, but AISAM could not find any pages associated with this profile.",
        primaryLabel: "Return to Settings",
        primaryHref: "/social",
        secondaryLabel: "",
        secondaryHref: "",
      };
    }

    return {
      icon: "check_circle",
      iconWrap: "bg-success-green/10",
      iconColor: "text-success-green",
      title: "Account Linked Successfully",
      description: details,
      primaryLabel: "Continue to Workspace",
      primaryHref: "/social",
      secondaryLabel: "",
      secondaryHref: "",
    };
  }, [details, status]);

  return (
    <main className="relative min-h-screen bg-background flex flex-col items-center justify-center p-4 overflow-hidden">
      <div className="absolute inset-0 z-0 overflow-hidden pointer-events-none">
        <div className="absolute top-[-10%] left-[-10%] w-[50%] h-[50%] rounded-full bg-primary-fixed/30 blur-[100px]" />
        <div className="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] rounded-full bg-secondary-fixed/30 blur-[100px]" />
      </div>

      <div className="relative z-10 w-full max-w-md rounded-xl p-stack-lg mb-8 bg-white/90 backdrop-blur-xl border border-outline-variant/40 shadow-[0_8px_32px_rgba(0,0,0,0.04)]">
        <div className="flex flex-col items-center text-center space-y-stack-md">
          <div className={`w-16 h-16 rounded-full ${content.iconWrap} flex items-center justify-center`}>
            <span className={`material-symbols-outlined ${content.iconColor} text-3xl`} style={{ fontVariationSettings: "'FILL' 1" }}>
              {content.icon}
            </span>
          </div>

          <div className="space-y-stack-sm">
            <h1 className="text-headline-md text-on-surface">{content.title}</h1>
            <p className="text-body-md text-on-surface-variant">{content.description}</p>
          </div>

          {status === "success" && (
            <div className="w-full bg-surface-container-lowest border border-outline-variant rounded-lg p-stack-md flex items-center gap-stack-md">
              <div className="w-12 h-12 rounded-full bg-surface-variant flex items-center justify-center overflow-hidden text-primary">
                <span className="material-symbols-outlined">share</span>
              </div>
              <div className="text-left flex-1 min-w-0">
                <p className="text-label-md text-on-surface truncate">{accountName}</p>
                <p className="text-body-sm text-on-surface-variant truncate">{handle}</p>
              </div>
              <span className="material-symbols-outlined text-primary">link</span>
            </div>
          )}

          {status === "failure" && (
            <div className="w-full bg-error-container/30 border border-error/20 rounded-lg p-stack-md text-left">
              <p className="text-label-sm text-error uppercase mb-1">Error Details</p>
              <p className="text-body-sm text-on-surface-variant">{details}</p>
            </div>
          )}

          {status === "partial" && (
            <div className="w-full bg-surface-container-lowest border border-outline-variant rounded-lg p-stack-md text-left">
              <h2 className="text-label-md text-on-surface mb-2">Next Steps</h2>
              <ul className="text-body-sm text-on-surface-variant space-y-2 list-disc pl-4">
                <li>Ensure you are an admin of the page you are trying to connect.</li>
                <li>Check if the page is published and publicly visible.</li>
                <li>Try re-authenticating with a different Facebook profile.</li>
              </ul>
            </div>
          )}

          <div className="w-full flex flex-col gap-stack-sm">
            <Link
              href={content.primaryHref}
              className="w-full bg-primary hover:bg-on-primary-fixed-variant text-on-primary text-label-md py-3 px-6 rounded-lg transition-colors flex items-center justify-center gap-2"
            >
              {status === "failure" && <span className="material-symbols-outlined text-[18px]">refresh</span>}
              {content.primaryLabel}
              {status === "success" && <span className="material-symbols-outlined text-[18px]">arrow_forward</span>}
            </Link>
            {content.secondaryLabel && (
              <Link
                href={content.secondaryHref}
                className="w-full bg-transparent border border-outline hover:bg-surface-container-low text-on-surface text-label-md py-3 px-6 rounded-lg transition-colors flex items-center justify-center gap-2"
              >
                {content.secondaryLabel}
              </Link>
            )}
          </div>
        </div>
      </div>
    </main>
  );
}

export default function SocialConnectionSuccessPage() {
  return (
    <Suspense fallback={null}>
      <ResultCard />
    </Suspense>
  );
}
