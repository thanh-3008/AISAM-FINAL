import { afterEach, expect, it, vi } from "vitest";
import { combinePublishingActivity, getDateRange } from "../analyticsService";

afterEach(() => vi.useRealTimers());

it("includes exactly the selected number of calendar days", () => {
  vi.useFakeTimers();
  vi.setSystemTime(new Date(2026, 8, 20, 12));

  for (const [range, days] of [["7d", 7], ["30d", 30], ["90d", 90]] as const) {
    const { from, to } = getDateRange(range);
    const start = new Date(from);
    const end = new Date(to);
    expect(start.getHours()).toBe(0);
    expect(end.getHours()).toBe(23);
    expect(Math.round((end.getTime() - start.getTime() + 1) / 86400000)).toBe(days);
  }
});

it("uses the dates selected for a custom range", () => {
  const { from, to } = getDateRange("custom", "2026-09-01", "2026-09-03");
  expect(new Date(from).getDate()).toBe(1);
  expect(new Date(to).getDate()).toBe(3);
  expect(() => getDateRange("custom", "2026-09-03", "2026-09-01")).toThrow();
});

it("counts immediate and scheduled published posts once per destination", () => {
  const activity = combinePublishingActivity(
    [
      { date: "2026-09-19", publishedPosts: 2 },
      { date: "2026-09-20", publishedPosts: 1 },
    ],
    [{ date: "2026-09-19", completed: 1, failed: 1, pending: 1, retryAttempts: 2, successRate: 50 }]
  );

  expect(activity[0]).toMatchObject({ published: 2, scheduledCompleted: 1, failed: 1, pending: 1, successRate: 50 });
  expect(activity[1]).toMatchObject({ published: 1, scheduledCompleted: 0, failed: 0 });
  expect(activity.reduce((sum, point) => sum + point.published, 0)).toBe(3);
});
