import { type NextRequest, NextResponse } from "next/server";
import { resolveServerOrigin } from "@/lib/originResolver";

function isLocalRequest(request: NextRequest): boolean {
  const host = request.nextUrl.hostname.toLowerCase();
  return host === "localhost" || host === "127.0.0.1";
}

export function GET(request: NextRequest) {
  if (isLocalRequest(request)) {
    const localOverride = process.env.INSTAGRAM_LOCAL_COMPLETE_URL?.trim();
    if (localOverride) {
      const target = new URL(localOverride);
      target.search = request.nextUrl.search;
      return NextResponse.redirect(target, {
        status: 302,
        headers: {
          "Cache-Control": "no-store",
          "Referrer-Policy": "no-referrer",
        },
      });
    }
  }

  const origin = resolveServerOrigin(request);
  const target = new URL(`${origin}/auth/instagram/complete`);
  target.search = request.nextUrl.search;

  return NextResponse.redirect(target, {
    status: 302,
    headers: {
      "Cache-Control": "no-store",
      "Referrer-Policy": "no-referrer",
    },
  });
}
