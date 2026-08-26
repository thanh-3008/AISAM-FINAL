import { describe, expect, it } from "vitest";
import { getApprovalBrands, matchesApprovalBrand } from "@/lib/approvalBrands";

describe("approval brand filtering", () => {
  const items = [
    { brandId: "nike-id", brandName: "Nike" },
    { brandId: "adidas-id", brandName: "Adidas" },
    { brandId: "nike-id", brandName: "Nike" },
    { brandId: "", brandName: "" },
  ];

  it("builds unique, named brand options without an empty Brand entry", () => {
    expect(getApprovalBrands(items)).toEqual([
      { brandId: "adidas-id", brandName: "Adidas" },
      { brandId: "nike-id", brandName: "Nike" },
    ]);
  });

  it("filters approvals by stable brand id", () => {
    expect(items.filter((item) => matchesApprovalBrand(item, "nike-id"))).toHaveLength(2);
    expect(items.filter((item) => matchesApprovalBrand(item, ""))).toHaveLength(4);
  });
});
