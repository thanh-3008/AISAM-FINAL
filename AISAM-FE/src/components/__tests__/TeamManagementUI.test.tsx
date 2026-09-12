import React from "react";
import { afterEach, expect, it, vi } from "vitest";
import { cleanup, render, screen } from "@testing-library/react";
import TeamPage from "@/app/(dashboard)/team/page";

vi.mock("@/services/teamService", () => ({
  fetchMembers: vi.fn(async () => ({
    data: [
      {
        id: "m1",
        name: "Alice Smith",
        email: "alice@example.com",
        role: "Manager",
        status: "Active",
        createdAt: "2026-01-01T00:00:00Z",
        lastActive: "2026-01-02T00:00:00Z",
        creditUsed: 10,
        creditLimit: 100,
        quotaMode: "MonthlyAssigned",
      },
    ],
  })),
  fetchTeams: vi.fn(async () => ({
    data: [
      {
        id: "t1",
        name: "Marketing Team",
        description: "Handles social media",
        status: "Active",
        memberCount: 1,
        brandCount: 2,
        createdAt: "2026-01-01T00:00:00Z",
      },
    ],
  })),
  inviteMember: vi.fn(),
  updateMemberRole: vi.fn(),
  transferWorkspaceOwnership: vi.fn(),
  removeMember: vi.fn(),
  updateMemberQuota: vi.fn(),
}));

vi.mock("@/hooks/useWorkspaces", () => ({
  useWorkspaces: () => ({
    activeWorkspace: {
      id: "ws1",
      name: "Acme Workspace",
      isOwner: true,
      memberRole: "Manager",
      workspaceType: 1,
      memberLimit: 10,
    },
    refetch: vi.fn(),
  }),
  getWorkspaceTypeLabel: () => "Business",
  invalidateWorkspaceCache: vi.fn(),
}));

vi.mock("@/hooks/useFeatureGate", () => ({
  useFeatureGate: () => ({
    canAccess: () => true,
  }),
}));

vi.mock("@/components/layout/Header", () => ({
  default: ({ breadcrumbs }: { breadcrumbs: Array<{ label: string; href?: string }> }) => (
    <div data-testid="header">{breadcrumbs.map((b) => b.label).join(" / ")}</div>
  ),
}));

vi.mock("next/link", () => ({
  default: ({ href, children, ...props }: { href: string; children: React.ReactNode }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
}));

afterEach(cleanup);

it("renders the Brand permission informational banner and Member Performance action", async () => {
  render(<TeamPage />);

  // 1. Verify Brand permission banner
  expect(await screen.findByText("Access is managed by Brand.")).toBeTruthy();
  const brandLink = screen.getByRole("link", { name: /Select a Brand/i });
  expect(brandLink.getAttribute("href")).toBe("/brands");

  // 2. Verify Member Performance secondary action
  const performanceLink = screen.getByRole("link", { name: /Member Performance/i });
  expect(performanceLink).toBeTruthy();
  expect(performanceLink.getAttribute("href")).toBe("/team/performance");

  // 3. Verify primary action Create Team
  expect(screen.getByRole("button", { name: /Create Team/i })).toBeTruthy();

  // 4. Verify Tab Switcher
  expect(screen.getByRole("button", { name: /Teams \(1\)/i })).toBeTruthy();
  expect(screen.getByRole("button", { name: /Members \(1\)/i })).toBeTruthy();

  // 5. Verify no Vietnamese text remains
  expect(screen.queryByText(/Vai trò workspace không tự cấp quyền/i)).toBeNull();
  expect(screen.queryByText(/Hiệu suất thành viên/i)).toBeNull();
});
