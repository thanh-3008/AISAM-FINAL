import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const CLAIM_ROLE =
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role";

function getRoleFromRequest(request: NextRequest): string | null {
  const cookieRole = request.cookies.get("aisam_role")?.value;
  if (cookieRole) return cookieRole;

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

const userRoutes = [
  "/dashboard",
  "/brands",
  "/content",
  "/approvals",
  "/posts",
  "/calendar",
  "/social",
  "/campaigns",
  "/analytics",
  "/team",
  "/notifications",
  "/overview",
  "/pricing",
  "/credit-pack",
  "/credit-history",
  "/workspace-dashboard",
  "/workspace-members",
  "/profiles",
];

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const role = getRoleFromRequest(request);

  if (pathname.startsWith("/admin") && role !== "Admin") {
    const url = request.nextUrl.clone();
    url.pathname = "/dashboard";
    return NextResponse.redirect(url);
  }

  // Redirect admin users away from user routes
  if (role === "Admin" && userRoutes.some((route) => pathname === route || pathname.startsWith(route + "/"))) {
    const url = request.nextUrl.clone();
    url.pathname = "/admin/dashboard";
    return NextResponse.redirect(url);
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    "/admin/:path*",
    "/dashboard/:path*",
    "/brands/:path*",
    "/content/:path*",
    "/approvals/:path*",
    "/posts/:path*",
    "/calendar/:path*",
    "/social/:path*",
    "/campaigns/:path*",
    "/analytics/:path*",
    "/team/:path*",
    "/notifications/:path*",
    "/overview/:path*",
    "/pricing/:path*",
    "/credit-pack/:path*",
    "/credit-history/:path*",
    "/workspace-dashboard/:path*",
    "/workspace-members/:path*",
    "/profiles/:path*",
  ],
};
