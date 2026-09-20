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
  isAiGenerated: false,
  tags: null,
  createdAt: "2026-09-20T00:00:00Z",
  updatedAt: "2026-09-20T00:00:00Z",
};

describe("content team mapping", () => {
  it("keeps team identity in list and detail models", () => {
    expect(apiItemToContentItem(content)).toMatchObject({ teamId: "team-1", teamName: "Growth Team" });
    expect(apiItemToContentDetail(content)).toMatchObject({ teamId: "team-1", teamName: "Growth Team" });
  });
});
