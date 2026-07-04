import { type NextRequest, NextResponse } from "next/server";

export function GET(request: NextRequest) {
  const localCompleteUrl =
    process.env.INSTAGRAM_LOCAL_COMPLETE_URL?.trim() ||
    "http://localhost:3000/auth/instagram/complete";
  const target = new URL(localCompleteUrl);
  target.search = request.nextUrl.search;

  return NextResponse.redirect(target, {
    status: 302,
    headers: {
      "Cache-Control": "no-store",
      "Referrer-Policy": "no-referrer",
    },
  });
}
