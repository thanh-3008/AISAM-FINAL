import { apiClient, apiFetch } from "@/lib/apiClient";
import { normalizeListResponse, type GenericResponse, type PagedResult } from "@/lib/apiResponse";

export interface ProductApiItem {
  id: string;
  brandId: string;
  name: string;
  description?: string | null;
  price?: number | null;
  images?: string[] | null;
  createdAt?: string;
  updatedAt?: string;
}

export interface ProductPayload {
  brandId: string;
  name: string;
  description?: string | null;
  price?: number | null;
  images?: string[] | null;
}

function productPayloadToFormData(data: Partial<ProductPayload>): FormData {
  const formData = new FormData();
  if (data.brandId !== undefined) formData.append("BrandId", data.brandId);
  if (data.name !== undefined) formData.append("Name", data.name);
  if (data.description !== undefined && data.description !== null) formData.append("Description", data.description);
  if (data.price !== undefined && data.price !== null) formData.append("Price", String(data.price));
  return formData;
}

export async function fetchProducts(brandId?: string): Promise<ProductApiItem[]> {
  try {
    const query = brandId ? `?brandId=${brandId}&pageSize=100` : "?pageSize=100";
    const res: GenericResponse<PagedResult<ProductApiItem> | ProductApiItem[]> = await apiClient(`/products${query}`);
    return normalizeListResponse(res);
  } catch {
    return [];
  }
}

export async function getProductById(id: string): Promise<ProductApiItem | null> {
  try {
    const res: GenericResponse<ProductApiItem> = await apiClient(`/products/${id}`);
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function createProduct(data: ProductPayload | FormData): Promise<ProductApiItem | null> {
  try {
    const isFormData = typeof FormData !== "undefined" && data instanceof FormData;
    const res: GenericResponse<ProductApiItem> = isFormData
      ? await apiFetch("/products", { method: "POST", body: data })
      : await apiFetch("/products", { method: "POST", body: productPayloadToFormData(data as ProductPayload) });
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function updateProduct(id: string, data: Partial<ProductPayload> | FormData): Promise<ProductApiItem | null> {
  try {
    const isFormData = typeof FormData !== "undefined" && data instanceof FormData;
    const res: GenericResponse<ProductApiItem> = isFormData
      ? await apiFetch(`/products/${id}`, { method: "PUT", body: data })
      : await apiFetch(`/products/${id}`, { method: "PUT", body: productPayloadToFormData(data as Partial<ProductPayload>) });
    return res?.data ?? null;
  } catch {
    return null;
  }
}

export async function deleteProduct(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<unknown> = await apiClient(`/products/${id}`, { method: "DELETE" });
    return res?.success === true;
  } catch {
    return false;
  }
}

export async function restoreProduct(id: string): Promise<boolean> {
  try {
    const res: GenericResponse<boolean> = await apiClient(`/products/${id}/restore`, { method: "POST" });
    return res?.success === true;
  } catch {
    return false;
  }
}
