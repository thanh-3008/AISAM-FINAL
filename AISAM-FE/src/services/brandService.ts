import { apiClient } from "@/lib/apiClient";
import { protectedCacheRevision, registerProtectedCache } from "@/lib/accessEvents";

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
registerProtectedCache(() => { brandList = []; productList = []; });

export async function fetchBrands(): Promise<{ id: string; name: string }[]> {
  const revision = protectedCacheRevision();
  try {
    const res: GenericResponse<{ data: BrandApiItem[] }> = await apiClient("/brands?pageSize=100");
    if (revision === protectedCacheRevision() && res?.success && res.data?.data) {
      brandList = res.data.data;
      return res.data.data;
    }
  } catch {
    // ignore
  }
  return [];
}

export async function fetchProducts(brandId?: string): Promise<{ id: string; name: string; brandId: string }[]> {
  const revision = protectedCacheRevision();
  try {
    const query = brandId ? `?brandId=${brandId}&pageSize=100` : "?pageSize=100";
    const res: GenericResponse<{ data: ProductApiItem[] }> = await apiClient(`/products${query}`);
    if (revision === protectedCacheRevision() && res?.success && res.data?.data) {
      productList = res.data.data;
      return res.data.data;
    }
  } catch {
    // ignore
  }
  return [];
}

export function getCachedBrands() {
  return brandList;
}

export function getCachedProducts() {
  return productList;
}
