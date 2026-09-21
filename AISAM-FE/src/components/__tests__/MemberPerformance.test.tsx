import React from "react";
import { afterEach, expect, it, vi } from "vitest";
import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import Page from "@/app/(dashboard)/team/performance/page";
import { inclusiveToExclusiveUtc } from "@/lib/dateRanges";
import { apiClient } from "@/lib/apiClient";

vi.mock("@/lib/apiClient", () => ({ apiClient: vi.fn() }));
vi.mock("next/link", () => ({ default: ({ children }: { children: React.ReactNode }) => <span>{children}</span> }));
afterEach(() => { cleanup(); vi.clearAllMocks(); });

const ownerReport = {
  success: true,
  data: {
    items: [{ memberId: "c", name: "Creator", teamRoles: ["ContentCreator"], contentsCreated: 2,
      creatorPublishedPosts: 1, publisherPublishedPosts: 0, reviewedSubmissions: 0,
      approvalRate: null, turnaroundHours: null, onTimeRate: null, failedPublishRate: null,
      pendingSchedules: 0, completedSchedules: 0, failedSchedules: 0, postsWithInsights: 0,
      engagement: null, impressions: null, reach: null, engagementRate: null, insightsUpdatedAt: null }],
    total: 1, updatedAt: "2026-09-08T00:00:00Z", unattributedContents: null,
    brands: [{ id: "alpha", name: "Alpha" }], teams: [{ id: "team-a", name: "Team A" }],
    members: [{ id: "c", name: "Creator" }], metricDefinitions: { period: "UTC" },
    canViewAllTeams: true, teamSelectionRequired: false,
  },
};

it("includes the whole selected end date in the API range", () => {
  expect(inclusiveToExclusiveUtc("2026-09-21")).toBe("2026-09-22T00:00:00.000Z");
});

it("separates role-aware areas and sends scoped filters", async () => {
  vi.mocked(apiClient).mockResolvedValue(ownerReport);
  render(<Page />);
  await screen.findAllByText("Creator");
  expect(screen.getByRole("button", { name: /Content creation/i }).getAttribute("aria-pressed")).toBe("true");
  fireEvent.click(screen.getByRole("button", { name: /Content outcomes/i }));
  expect(await screen.findByText("Not synced")).toBeTruthy();
  expect(screen.getAllByText("Insufficient data").length).toBeGreaterThan(1);
  fireEvent.change(screen.getByLabelText("Brand"), { target: { value: "alpha" } });
  await waitFor(() => expect(apiClient).toHaveBeenLastCalledWith(expect.stringContaining("brandId=alpha")));
});

it("automatically scopes a Team Manager to a managed Team", async () => {
  vi.mocked(apiClient)
    .mockResolvedValueOnce({ success: true, data: { items: [], total: 0, updatedAt: "2026-09-08T00:00:00Z",
      brands: [], teams: [{ id: "team-a", name: "Team A" }], members: [], metricDefinitions: {},
      canViewAllTeams: false, teamSelectionRequired: true } })
    .mockResolvedValue(ownerReport);
  render(<Page />);
  await waitFor(() => expect(apiClient).toHaveBeenCalledTimes(2));
  expect(vi.mocked(apiClient).mock.calls[1][0]).toContain("teamId=team-a");
  expect((screen.getByLabelText("Team") as HTMLSelectElement).value).toBe("team-a");
});

it("settles loading on an API error", async () => {
  vi.mocked(apiClient).mockRejectedValue(new Error("Không có quyền")); render(<Page />);
  expect((await screen.findByRole("alert")).textContent).toContain("Không có quyền");
  expect(screen.queryByRole("status")).toBeNull();
});

it("shows Not applicable for a Viewer instead of treating missing responsibility as poor performance", async () => {
  vi.mocked(apiClient).mockResolvedValue({ ...ownerReport, data: { ...ownerReport.data,
    items: [{ ...ownerReport.data.items[0], memberId: "v", name: "Viewer", teamRoles: ["Viewer"],
      contentsCreated: 0, creatorPublishedPosts: 0 }], members: [{ id: "v", name: "Viewer" }] } });
  render(<Page />);
  await screen.findAllByText("Viewer");
  expect(screen.getAllByText("Not applicable").length).toBeGreaterThan(1);
  expect(screen.queryByRole("alert")).toBeNull();
});
