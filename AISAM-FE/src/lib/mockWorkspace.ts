import type { WorkspaceData } from "@/hooks/useWorkspaces";

export interface CreditWallet {
  id: string;
  workspaceId: string;
  balance: number;
  maxBalance: number;
  subscriptionEndDate?: string;
  subscriptionStatus?: string;
}

export interface WorkspaceDashboard {
  creditsRemaining: number;
  postsRemaining: number;
  totalAiUsage: number;
  topMembers: { userId: string; name: string; usage: number }[];
}

export function getMockWorkspaces(userId: string): WorkspaceData[] {
  return [
    {
      id: `ws-personal-${userId}`,
      userId,
      name: "My Personal Workspace",
      workspaceType: 1,
      plan: "Personal Pro",
      status: 1,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      isOwner: true,
      memberRole: "Owner",
    },
  ];
}

export function getMockCreditWallet(): CreditWallet {
  const futureDate = new Date();
  futureDate.setDate(futureDate.getDate() + 45);
  return {
    id: "wallet-1",
    workspaceId: "ws-1",
    balance: 850,
    maxBalance: 15000,
    subscriptionEndDate: futureDate.toISOString(),
    subscriptionStatus: "Active",
  };
}

export function getMockWorkspaceDashboard(): WorkspaceDashboard {
  return {
    creditsRemaining: 850,
    postsRemaining: 876,
    totalAiUsage: 124,
    topMembers: [
      { userId: "user-1", name: "You", usage: 124 },
    ],
  };
}

export function getMockPostQuota(): { used: number; total: number } {
  return {
    used: 124,
    total: 1000,
  };
}
