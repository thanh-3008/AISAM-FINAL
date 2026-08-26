export interface ApprovalBrandItem {
  brandId: string;
  brandName: string;
}

export function getApprovalBrands(items: ApprovalBrandItem[]): ApprovalBrandItem[] {
  const brands = new Map<string, string>();
  for (const item of items) {
    const brandId = item.brandId.trim();
    const brandName = item.brandName.trim();
    if (brandId && brandName && !brands.has(brandId)) {
      brands.set(brandId, brandName);
    }
  }

  return [...brands.entries()]
    .map(([brandId, brandName]) => ({ brandId, brandName }))
    .sort((a, b) => a.brandName.localeCompare(b.brandName));
}

export function matchesApprovalBrand(item: ApprovalBrandItem, brandId: string): boolean {
  return !brandId || item.brandId === brandId;
}
