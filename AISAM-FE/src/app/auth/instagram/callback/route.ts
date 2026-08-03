import { type NextRequest, NextResponse } from "next/server";

function isLocalRequest(request: NextRequest): boolean {
  const host = request.nextUrl.hostname;
  return host === "localhost" || host === "127.0.0.1";
}

export function GET(request: NextRequest) {
  const configuredCompleteUrl = process.env.INSTAGRAM_COMPLETE_URL?.trim();
  const localCompleteUrl = isLocalRequest(request)
    ? process.env.INSTAGRAM_LOCAL_COMPLETE_URL?.trim()
    : "";
  const target = new URL(
    configuredCompleteUrl ||
      localCompleteUrl ||
      `${request.nextUrl.origin}/auth/instagram/complete`,
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
