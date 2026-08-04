import { type NextRequest, NextResponse } from "next/server";

function isLocalRequest(request: NextRequest): boolean {
  const host = request.nextUrl.hostname;
  return host === "localhost" || host === "127.0.0.1";
}

function isLocalUrl(value?: string): boolean {
  if (!value) return false;

  try {
    const hostname = new URL(value).hostname;
    return hostname === "localhost" || hostname === "127.0.0.1";
  } catch {
    return false;
  }
}

function getSafeUrl(request: NextRequest, ...values: Array<string | undefined>): string {
  const localRequest = isLocalRequest(request);

  for (const value of values) {
    const trimmed = value?.trim();
    if (!trimmed) continue;
    if (!localRequest && isLocalUrl(trimmed)) continue;
    return trimmed;
  }

  return `${request.nextUrl.origin}/auth/instagram/callback`;
}

export function GET(request: NextRequest) {
  const target = new URL(
    getSafeUrl(
      request,
      process.env.INSTAGRAM_CALLBACK_URL,
      process.env.NEXT_PUBLIC_INSTAGRAM_CALLBACK_URL,
      isLocalRequest(request) ? process.env.INSTAGRAM_LOCAL_CALLBACK_URL : undefined,
      isLocalRequest(request) ? process.env.NEXT_PUBLIC_INSTAGRAM_LOCAL_CALLBACK_URL : undefined,
    ),
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
