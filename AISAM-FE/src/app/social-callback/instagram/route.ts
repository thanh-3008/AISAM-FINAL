import { type NextRequest, NextResponse } from "next/server";

function isLocalRequest(request: NextRequest): boolean {
  const host = request.nextUrl.hostname;
  return host === "localhost" || host === "127.0.0.1";
}

export function GET(request: NextRequest) {
  const configuredCallbackUrl = process.env.INSTAGRAM_CALLBACK_URL?.trim();
  const localCallbackUrl = isLocalRequest(request)
    ? process.env.INSTAGRAM_LOCAL_CALLBACK_URL?.trim()
    : "";

  const target = new URL(
    configuredCallbackUrl ||
      localCallbackUrl ||
      `${request.nextUrl.origin}/auth/instagram/callback`,
  );
  target.search = request.nextUrl.search;

  return NextResponse.redirect(target, {
    status: 302,
    headers: {
      "Cache-Control": "no-store",
      "Referrer-Policy": "no-referrer",
    },
  });
}
