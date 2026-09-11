import { describe, it, expect } from "vitest";

describe("Approvals Permission Filtering and Role Fallback", () => {
  it("filters out schedule failure items so only valid content GUIDs are checked", () => {
    const rawItems = [
      { id: "11111111-1111-1111-1111-111111111111", approvalSource: "content", status: "Awaiting Approval" },
      { id: "schedule-22222222-2222-2222-2222-222222222222", approvalSource: "schedule", status: "Publish Failed" },
      { id: "33333333-3333-3333-3333-333333333333", approvalSource: "content", status: "Awaiting Approval" },
    ];

    const contentItemsForReview = rawItems.filter(
      (item) => item.approvalSource === "content" && !item.id.startsWith("schedule-")
    );

    expect(contentItemsForReview).toHaveLength(2);
    expect(contentItemsForReview.map((item) => item.id)).toEqual([
      "11111111-1111-1111-1111-111111111111",
      "33333333-3333-3333-3333-333333333333",
    ]);

    // Ensure none have the "schedule-" prefix which causes HTTP 400 on backend
    expect(contentItemsForReview.every((item) => !item.id.startsWith("schedule-"))).toBe(true);
  });

  it("grants review permission for Owner or Manager via fallback", () => {
    const contentItemsForReview = [
      { id: "11111111-1111-1111-1111-111111111111" },
    ];

    // Simulate batch permissions returning false (or empty due to network)
    const reviewAllowed = (_index: number) => false;

    const createIsReviewAllowed = (isOwnerOrManager: boolean) => (id: string) => {
      if (isOwnerOrManager) return true;
      const index = contentItemsForReview.findIndex((item) => item.id === id);
      if (index === -1) return false;
      return reviewAllowed(index);
    };

    const ownerReview = createIsReviewAllowed(true);
    const creatorReview = createIsReviewAllowed(false);

    expect(ownerReview("11111111-1111-1111-1111-111111111111")).toBe(true);
    expect(creatorReview("11111111-1111-1111-1111-111111111111")).toBe(false);
  });

  it("maps index correctly through filtered content items for delegated permissions", () => {
    const contentItemsForReview = [
      { id: "content-A" },
      { id: "content-B" },
    ];

    // content-A is allowed, content-B is not
    const decisions = [true, false];
    const reviewAllowed = (index: number) => decisions[index] === true;

    const isReviewAllowed = (id: string) => {
      const index = contentItemsForReview.findIndex((item) => item.id === id);
      if (index === -1) return false;
      return reviewAllowed(index);
    };

    expect(isReviewAllowed("content-A")).toBe(true);
    expect(isReviewAllowed("content-B")).toBe(false);
    expect(isReviewAllowed("schedule-xyz")).toBe(false);
  });
});
