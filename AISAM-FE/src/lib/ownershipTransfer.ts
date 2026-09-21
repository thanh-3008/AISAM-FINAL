export interface OwnershipTransferMember {
  workspaceRole?: string | null;
}

export function isOwnershipTransferCandidate(member: OwnershipTransferMember): boolean {
  return member.workspaceRole === "WorkspaceManager";
}
