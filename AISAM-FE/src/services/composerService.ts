import { apiClient } from "@/lib/apiClient";
export interface MediaItem { assetId: string; sortOrder: number; isCover: boolean; url: string; mimeType: string; altText?: string; caption?: string; }
export interface MediaCollection { version: string; items: MediaItem[]; }
export interface Destination { id: string; name: string; platform: string; error: string | null; caption: string; capability: { maxItems: number; mixedMedia: boolean; multiVideo: boolean; }; }
export interface PublishPreview { version: string; approved: boolean; media: { url: string; mimeType: string; sortOrder: number; isCover: boolean }[]; destinations: Destination[]; }
export interface PublishOperation { id: string; integrationId: string; status: string; errorCode?: string; providerId?: string; attempts: number; }
export async function readMedia(id: string): Promise<MediaCollection> { return (await apiClient(`/content/${id}/media`)).data; }
export async function saveMedia(id: string, collection: MediaCollection): Promise<MediaCollection> {
  return (await apiClient(`/content/${id}/media`, { method: "PUT", data: { expectedVersion: collection.version,
    items: collection.items.map((m, i) => ({ assetId: m.assetId, sortOrder: i, isCover: m.isCover, altText: m.altText, caption: m.caption })) } })).data;
}
export async function uploadMediaItem(id: string, file: File): Promise<MediaItem> {
  const body = new FormData(); body.append("files", file);
  const response = await apiClient(`/content/${id}/media/upload`, { method: "POST", body });
  const result = response.data?.[0];
  if (!result?.assetId || result.error) throw new Error(result?.error ?? "Upload failed");
  return { assetId: result.assetId, url: result.url, mimeType: file.type, sortOrder: 0, isCover: false };
}
export async function importLegacyMedia(id: string, version: string): Promise<MediaCollection> {
  return (await apiClient(`/content/${id}/media/import-legacy`, { method: "POST", data: { expectedVersion: version } })).data;
}
export async function previewPublish(id: string): Promise<PublishPreview> { return (await apiClient(`/content/${id}/publish-preview`)).data; }
export async function startPublish(id: string, expectedVersion: string, integrationIds: string[], idempotencyKey: string): Promise<PublishOperation[]> {
  return (await apiClient(`/content/${id}/publish-operations`, { method: "POST", data: { expectedVersion, integrationIds, idempotencyKey } })).data.operations;
}
export async function readPublish(id: string, key: string): Promise<PublishOperation[]> {
  return (await apiClient(`/content/${id}/publish-operations?key=${encodeURIComponent(key)}`)).data;
}
