import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const CLAIM_ROLE =
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role";

function getRoleFromRequest(request: NextRequest): string | null {
  const token = request.cookies.get("aisam_token")?.value;
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return (
      payload[CLAIM_ROLE] ||
      payload["role"] ||
      null
    );
  } catch {
    return null;
  }
}

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const role = getRoleFromRequest(request);

  if (pathname.startsWith("/admin") && role !== "Admin") {
    const url = request.nextUrl.clone();
    url.pathname = "/dashboard";
    return NextResponse.redirect(url);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*"],
};
