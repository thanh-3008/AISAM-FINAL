import { type NextRequest, NextResponse } from "next/server";

export function GET(request: NextRequest) {
  const localCallbackUrl =
    process.env.INSTAGRAM_LOCAL_CALLBACK_URL?.trim() ||
    "http://localhost:3000/auth/instagram/callback";

  const target = new URL(localCallbackUrl);
  target.search = request.nextUrl.search;

  return NextResponse.redirect(target, {
    status: 302,
    headers: {
      "Cache-Control": "no-store",
      "Referrer-Policy": "no-referrer",
    },
  });
}
