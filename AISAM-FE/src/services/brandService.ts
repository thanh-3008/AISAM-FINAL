import { apiClient } from "@/lib/apiClient";
import type { PagedResult } from "@/lib/apiTypes";

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
  const res: GenericResponse<PagedResult<BrandApiItem>> = await apiClient("/brands");
  brandList = res.data?.items ?? res.data?.data ?? [];
  return brandList;
}

export async function fetchProducts(brandId?: string): Promise<{ id: string; name: string; brandId: string }[]> {
  const query = brandId ? `?brandId=${brandId}` : "";
  const res: GenericResponse<PagedResult<ProductApiItem>> = await apiClient(`/products${query}`);
  productList = res.data?.items ?? res.data?.data ?? [];
  return productList;
}

export function getCachedBrands() {
  return brandList;
}

export function getCachedProducts() {
  return productList;
}
