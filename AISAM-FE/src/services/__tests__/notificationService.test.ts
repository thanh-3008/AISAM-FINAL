import { describe, expect, it } from "vitest";
import { getNotificationTargetPath, shouldShowFullNotificationMessage } from "@/services/notificationService";

describe("notification content feedback", () => {
  it("routes a content notification to the matching content detail", () => {
    expect(getNotificationTargetPath({ targetType: "content", targetId: "content/with space" }))
      .toBe("/content/content%2Fwith%20space");
  });

  it("keeps approval feedback messages fully expanded", () => {
    expect(shouldShowFullNotificationMessage({ type: "APPROVAL", targetType: "content" })).toBe(true);
    expect(shouldShowFullNotificationMessage({ type: "CONTENT_PUBLISHED", targetType: "content" })).toBe(false);
  });
});