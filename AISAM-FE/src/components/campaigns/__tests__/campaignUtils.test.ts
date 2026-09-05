import { describe, expect, it } from "vitest";
import { formatNumber, formatCurrency, getBudgetProgress, getCtr, getCpa } from "../campaignUtils";
import { type Campaign } from "@/services/campaignService";

describe("campaignUtils", () => {
  it("does not render redacted analytics as zero or NaN", () => {
    const campaign = { canViewAnalytics: false } as Campaign;
    expect(formatNumber(undefined)).toBe("—");
    expect(formatCurrency(null)).toBe("—");
    expect(getCtr(campaign)).toBe("—");
    expect(getCpa(campaign)).toBe("—");
  });
  it("formatNumber formats millions and thousands correctly", () => {
    expect(formatNumber(1500000)).toBe("1.5M");
    expect(formatNumber(1500)).toBe("1.5K");
    expect(formatNumber(999)).toBe("999");
  });

  it("formatCurrency formats VND and USD correctly", () => {
    const vnd = formatCurrency(1000000);
    expect(vnd.replace(/\s|\u00A0/g, "")).toBe("1.000.000đ");

    const usd = formatCurrency(1500, "USD");
    expect(usd.replace(/\s|\u00A0/g, "")).toBe("$1.500");
  });

  it("getBudgetProgress calculates correct percentage", () => {
    const campaign = { budget: 1000, spend: 250 } as Campaign;
    expect(getBudgetProgress(campaign)).toBe(25);
  });

  it("getBudgetProgress handles 0 budget", () => {
    const campaign = { budget: 0, spend: 250 } as Campaign;
    expect(getBudgetProgress(campaign)).toBe(0);
  });

  it("getBudgetProgress caps at 100%", () => {
    const campaign = { budget: 1000, spend: 1500 } as Campaign;
    expect(getBudgetProgress(campaign)).toBe(100);
  });

  it("getCtr calculates correct CTR percentage", () => {
    const campaign = { impressions: 1000, clicks: 50 } as Campaign;
    expect(getCtr(campaign)).toBe("5.00%");
  });

  it("getCtr returns 0% when impressions are 0", () => {
    const campaign = { impressions: 0, clicks: 0 } as Campaign;
    expect(getCtr(campaign)).toBe("0%");
  });

  it("getCpa calculates correctly", () => {
    const campaign = { spend: 500000, conversions: 5, adAccountCurrency: "VND" } as Campaign;
    const cpa = getCpa(campaign);
    expect(cpa.replace(/\s|\u00A0/g, "")).toBe("100.000đ");
  });

  it("getCpa returns — when conversions are 0", () => {
    const campaign = { spend: 500000, conversions: 0 } as Campaign;
    expect(getCpa(campaign)).toBe("—");
  });
});
