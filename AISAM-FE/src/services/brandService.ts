import { apiClient } from "@/lib/apiClient";
import { normalizeListResponse, type GenericResponse, type PagedResult } from "@/lib/apiResponse";

export interface BrandApiItem {
  id: string;
  name: string;
  description?: string | null;
  logoUrl?: string | null;
  slogan?: string | null;
  usp?: string | null;
  targetAudience?: string | null;
  workspaceId?: string | null;
  productsCount?: number;
  contentsCount?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface ProductApiItem {
  id: string;
  name: string;
  brandId: string;
  description?: string | null;
  price?: number | null;
  images?: string[] | null;
  createdAt?: string;
  updatedAt?: string;
}

export interface BrandPayload {
  name: string;
  description?: string | null;
  logoUrl?: string | null;
  slogan?: string | null;
  usp?: string | null;
  targetAudience?: string | null;
}

let brandList: { id: string; name: string }[] = [];
let productList: { id: string; name: string; brandId: string }[] = [];

export async function fetchBrands(): Promise<{ id: string; name: string }[]> {
  try {
    const res: GenericResponse<PagedResult<BrandApiItem> | BrandApiItem[]> = await apiClient("/brands?pageSize=100");
    const brands = normalizeListResponse(res);
    brandList = brands;
    return brands;
  } catch {
    // ignore
  }
  return [];
}

export async function getBrandById(id: string): Promise<BrandApiItem | null> {
  try {
    const res: GenericResponse<BrandApiItem> = await apiClient(`/brands/${id}`);
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function createBrand(data: BrandPayload): Promise<BrandApiItem | null> {
  try {
    const res: GenericResponse<BrandApiItem> = await apiClient("/brands", {
      method: "POST",
      data,
    });
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function updateBrand(id: string, data: Partial<BrandPayload>): Promise<BrandApiItem | null> {
  try {
    const res: GenericResponse<BrandApiItem> = await apiClient(`/brands/${id}`, {
      method: "PUT",
      data,
    });
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function deleteBrand(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<unknown> = await apiClient(`/brands/${id}`, { method: "DELETE" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function restoreBrand(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/brands/${id}/restore`, { method: "POST" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function fetchProducts(brandId?: string): Promise<ProductApiItem[]> {
  try {
    const query = brandId ? `?brandId=${brandId}&pageSize=100` : "?pageSize=100";
    const res: GenericResponse<PagedResult<ProductApiItem> | ProductApiItem[]> = await apiClient(`/products${query}`);
    const products = normalizeListResponse(res);
    productList = products;
    return products;
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
