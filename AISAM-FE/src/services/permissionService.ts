import { apiClient } from "@/lib/apiClient";

export const Kind = { Workspace: 0, Brand: 1, Content: 2, Channel: 3, Post: 4 } as const;
export const Permission = { BrandView: 0, BrandManage: 1, ContentView: 2, ContentCreate: 3, ContentEdit: 4, ContentDelete: 5, ContentViewAllCreators: 6, ApprovalReview: 7, PostPublish: 8, PostView: 9, SocialView: 10, SocialManage: 11 } as const;
export interface PermissionCheck { kind: number; resourceId: string; permission: number; channelId?: string }
export async function checkPermissions(items: PermissionCheck[]): Promise<boolean[]> {
  const decisions: boolean[] = [];
  for (let offset = 0; offset < items.length; offset += 100) {
    const batch = items.slice(offset, offset + 100);
    const response = await apiClient("/permissions/check", { method: "POST", data: batch });
    if (!Array.isArray(response?.data) || response.data.length !== batch.length) throw new Error("Không tải được quyền truy cập.");
    decisions.push(...response.data.map((value: unknown) => value === true));
  }
  return decisions;
}

export interface AssignmentSnapshot {
  revision: string;
  teams: { id: string; teamId: string; isActive: boolean }[];
  channels: { teamBrandId: string; integrationId: string; canView: boolean; canPublish: boolean; canManage: boolean }[];
}
export async function readAssignments(brandId: string): Promise<AssignmentSnapshot> {
  return (await apiClient(`/brands/${brandId}/access`)).data;
}
export async function changeAssignment(brandId: string, teamId: string, revision: string, active: boolean,
  channel?: { id: string; canView: boolean; canPublish: boolean; canManage: boolean }): Promise<AssignmentSnapshot> {
  const path = `/brands/${brandId}/${channel ? `channels/${channel.id}/` : ""}teams/${teamId}`;
  const response = await apiClient(path, active
    ? { method: "PUT", data: { expectedRevision: revision, ...channel } }
    : { method: "DELETE", headers: { "If-Match": revision } });
  window.dispatchEvent(new Event("aisam-permissions-changed"));
  return response.data;
}
