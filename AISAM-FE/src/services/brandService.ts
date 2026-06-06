import { apiClient } from "@/lib/apiClient";
import { BRANDS, PRODUCTS } from "@/lib/contentConstants";

interface GenericResponse<T> {
  success: boolean;
  data?: T;
}

interface BrandApiItem {
  id: string;
  name: string;
}

interface ProductApiItem {
  id: string;
  name: string;
  brandId: string;
}

let brandList: { id: string; name: string }[] = [];
let productList: { id: string; name: string; brandId: string }[] = [];

export async function fetchBrands(): Promise<{ id: string; name: string }[]> {
  try {
    const res: GenericResponse<BrandApiItem[]> = await apiClient("/brands");
    if (res?.success && res.data && res.data.length > 0) {
      brandList = res.data;
      return res.data;
    }
  } catch {
    // fallback
  }
  return BRANDS.map((name, i) => ({ id: `mock-brand-${i}`, name }));
}

export async function fetchProducts(brandId?: string): Promise<{ id: string; name: string; brandId: string }[]> {
  try {
    const query = brandId ? `?brandId=${brandId}` : "";
    const res: GenericResponse<ProductApiItem[]> = await apiClient(`/products${query}`);
    if (res?.success && res.data && res.data.length > 0) {
      productList = res.data;
      return res.data;
    }
  } catch {
    // fallback
  }
  return fallbackProducts(brandId);
}

function fallbackProducts(brandId?: string): { id: string; name: string; brandId: string }[] {
  const all: { id: string; name: string; brandId: string }[] = [];
  for (const [brand, prods] of Object.entries(PRODUCTS)) {
    for (const p of prods) {
      all.push({ id: `mock-prod-${all.length}`, name: p, brandId: `mock-brand-${all.length}` });
    }
  }
  return all;
}

export function getCachedBrands() {
  return brandList;
}

export function getCachedProducts() {
  return productList;
}
