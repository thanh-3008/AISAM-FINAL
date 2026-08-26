import { describe, expect, it } from "vitest";
import { withoutCalendarContentPrefill } from "@/lib/calendarUrl";

describe("withoutCalendarContentPrefill", () => {
  it("removes the content prefill so a completed schedule modal cannot reopen", () => {
    expect(withoutCalendarContentPrefill("/calendar", "contentId=content-1"))
      .toBe("/calendar");
  });

  it("preserves unrelated calendar query parameters", () => {
    expect(withoutCalendarContentPrefill("/calendar", "view=week&contentId=content-1&brand=nike"))
      .toBe("/calendar?view=week&brand=nike");
  });
});
