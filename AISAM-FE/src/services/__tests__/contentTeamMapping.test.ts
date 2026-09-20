import { describe, expect, it } from "vitest";
import { apiItemToContentDetail, apiItemToContentItem, type ContentApiItem } from "../contentService";

const content: ContentApiItem = {
  id: "content-1",
  profileId: "profile-1",
  brandId: "brand-1",
  brandName: "Shared Brand",
  teamId: "team-1",
  teamName: "Growth Team",
  productId: null,
  productName: null,
  adType: 0,
  title: "Campaign draft",
  textContent: "Draft",
  imageUrl: null,
  videoUrl: null,
  styleDescription: null,
  contextDescription: null,
  representativeCharacter: null,
  status: 0,
  rejectionReason: "Update the CTA",
  rejectedByUserId: "reviewer-1",
  rejectedByName: "Nguyen Reviewer",
  rejectedByRole: "Quản lý Team · Growth Team",
  rejectedAt: "2026-09-20T01:00:00Z",
  isAiGenerated: false,
  tags: null,
  createdAt: "2026-09-20T00:00:00Z",
  updatedAt: "2026-09-20T00:00:00Z",
};

describe("content metadata mapping", () => {
  it("keeps team and rejection identity in list and detail models", () => {
    const expected = {
      teamId: "team-1",
      teamName: "Growth Team",
      rejectionReason: "Update the CTA",
      rejectedByUserId: "reviewer-1",
      rejectedByName: "Nguyen Reviewer",
      rejectedByRole: "Quản lý Team · Growth Team",
      rejectedAt: "2026-09-20T01:00:00Z",
    };
    expect(apiItemToContentItem(content)).toMatchObject(expected);
    expect(apiItemToContentDetail(content)).toMatchObject(expected);
  });
});
