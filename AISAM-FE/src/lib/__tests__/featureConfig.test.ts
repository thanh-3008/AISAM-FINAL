import { describe, expect, it } from "vitest";
import { PERMISSION_MATRIX, WorkspaceRole } from "@/lib/featureConfig";

describe("workspace role permissions", () => {
  it("keeps content creation but removes review, publish, and schedule management from content creators", () => {
    expect(PERMISSION_MATRIX.createDraft).toContain(WorkspaceRole.ContentCreator);
    expect(PERMISSION_MATRIX.reviewContent).not.toContain(WorkspaceRole.ContentCreator);
    expect(PERMISSION_MATRIX.publishPost).not.toContain(WorkspaceRole.ContentCreator);
    expect(PERMISSION_MATRIX.manageSchedules).not.toContain(WorkspaceRole.ContentCreator);
  });

  it.each([WorkspaceRole.Owner, WorkspaceRole.Manager])("allows %s to review, publish, and manage schedules", (role) => {
    expect(PERMISSION_MATRIX.reviewContent).toContain(role);
    expect(PERMISSION_MATRIX.publishPost).toContain(role);
    expect(PERMISSION_MATRIX.manageSchedules).toContain(role);
  });
});
