export type CampaignStatus = "ACTIVE" | "PAUSED" | "COMPLETED" | "DRAFT";
export type CampaignObjective = "AWARENESS" | "TRAFFIC" | "ENGAGEMENT" | "LEADS" | "SALES" | "APP_PROMOTION";

// MOCK SERVICE: No BE campaign API exists yet. All data is localStorage-only.
// This will be replaced when the campaign feature is implemented on the backend.
export interface AdSet {
  id: string;
  name: string;
  facebookAdSetId: string | null;
  dailyBudget: number | null;
  status: "ACTIVE" | "PAUSED";
  impressions: number;
  clicks: number;
  spend: number;
}

export interface Campaign {
  id: string;
  profileId: string;
  brandId: string;
  brandName: string;
  adAccountId: string;
  facebookCampaignId: string | null;
  name: string;
  objective: CampaignObjective;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
  status: CampaignStatus;
  createdAt: string;
  updatedAt: string;
  adSets: AdSet[];
  impressions: number;
  clicks: number;
  spend: number;
  conversions: number;
}

export interface CreateCampaignData {
  name: string;
  brandId: string;
  brandName: string;
  objective: CampaignObjective;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
}

const STORAGE_KEY = "aisam_campaigns_v1";

const INITIAL_MOCK_CAMPAIGNS: Campaign[] = [
  {
    id: "c1",
    profileId: "mock-profile",
    brandId: "b1",
    brandName: "Lumina Tech",
    adAccountId: "act_123456",
    facebookCampaignId: "23845678901234567",
    name: "Summer Sale 2024 - Smart Home",
    objective: "SALES",
    budget: 5000,
    startDate: new Date(Date.now() - 30 * 86400000).toISOString(),
    endDate: new Date(Date.now() + 30 * 86400000).toISOString(),
    status: "ACTIVE",
    createdAt: new Date(Date.now() - 35 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 2 * 86400000).toISOString(),
    adSets: [
      { id: "as1", name: "US - 25-44 - Tech Enthusiasts", facebookAdSetId: "23845678901234568", dailyBudget: 50, status: "ACTIVE", impressions: 45000, clicks: 1200, spend: 450 },
      { id: "as2", name: "UK - 18-34 - Smart Home", facebookAdSetId: "23845678901234569", dailyBudget: 30, status: "ACTIVE", impressions: 28000, clicks: 850, spend: 280 },
    ],
    impressions: 73000,
    clicks: 2050,
    spend: 730,
    conversions: 89,
  },
  {
    id: "c2",
    profileId: "mock-profile",
    brandId: "b2",
    brandName: "Summit Outdoor",
    adAccountId: "act_789012",
    facebookCampaignId: "23845678901234570",
    name: "Adventure Gear Brand Awareness",
    objective: "AWARENESS",
    budget: 3000,
    startDate: new Date(Date.now() - 15 * 86400000).toISOString(),
    endDate: new Date(Date.now() + 45 * 86400000).toISOString(),
    status: "ACTIVE",
    createdAt: new Date(Date.now() - 20 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 1 * 86400000).toISOString(),
    adSets: [
      { id: "as3", name: "Outdoor Enthusiasts - 25-54", facebookAdSetId: "23845678901234571", dailyBudget: 40, status: "ACTIVE", impressions: 120000, clicks: 3500, spend: 320 },
    ],
    impressions: 120000,
    clicks: 3500,
    spend: 320,
    conversions: 0,
  },
  {
    id: "c3",
    profileId: "mock-profile",
    brandId: "b3",
    brandName: "Heritage Motors",
    adAccountId: "act_345678",
    facebookCampaignId: null,
    name: "Classic Car Restoration - Lead Gen",
    objective: "LEADS",
    budget: 2000,
    startDate: new Date(Date.now() - 60 * 86400000).toISOString(),
    endDate: new Date(Date.now() - 10 * 86400000).toISOString(),
    status: "COMPLETED",
    createdAt: new Date(Date.now() - 65 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 10 * 86400000).toISOString(),
    adSets: [
      { id: "as4", name: "Car Enthusiasts - 35-64", facebookAdSetId: "23845678901234572", dailyBudget: 35, status: "PAUSED", impressions: 85000, clicks: 2100, spend: 1800 },
    ],
    impressions: 85000,
    clicks: 2100,
    spend: 1800,
    conversions: 45,
  },
  {
    id: "c4",
    profileId: "mock-profile",
    brandId: "b4",
    brandName: "GreenLeaf Organics",
    adAccountId: "act_901234",
    facebookCampaignId: "23845678901234573",
    name: "Organic Tea Collection - Traffic",
    objective: "TRAFFIC",
    budget: 1500,
    startDate: new Date(Date.now() - 5 * 86400000).toISOString(),
    endDate: null,
    status: "PAUSED",
    createdAt: new Date(Date.now() - 7 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 3 * 86400000).toISOString(),
    adSets: [
      { id: "as5", name: "Health Conscious - 25-45", facebookAdSetId: "23845678901234574", dailyBudget: 25, status: "PAUSED", impressions: 15000, clicks: 450, spend: 120 },
    ],
    impressions: 15000,
    clicks: 450,
    spend: 120,
    conversions: 12,
  },
  {
    id: "c5",
    profileId: "mock-profile",
    brandId: "b5",
    brandName: "Pulse Finance",
    adAccountId: "act_567890",
    facebookCampaignId: null,
    name: "Budget App Launch - Q4 2024",
    objective: "APP_PROMOTION",
    budget: 8000,
    startDate: null,
    endDate: null,
    status: "DRAFT",
    createdAt: new Date(Date.now() - 2 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 1 * 86400000).toISOString(),
    adSets: [],
    impressions: 0,
    clicks: 0,
    spend: 0,
    conversions: 0,
  },
  {
    id: "c6",
    profileId: "mock-profile",
    brandId: "b1",
    brandName: "Lumina Tech",
    adAccountId: "act_123456",
    facebookCampaignId: "23845678901234575",
    name: "LED Strip Holiday Promo",
    objective: "ENGAGEMENT",
    budget: 2500,
    startDate: new Date(Date.now() - 45 * 86400000).toISOString(),
    endDate: new Date(Date.now() - 15 * 86400000).toISOString(),
    status: "COMPLETED",
    createdAt: new Date(Date.now() - 50 * 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 15 * 86400000).toISOString(),
    adSets: [
      { id: "as6", name: "Home Decor - 18-45", facebookAdSetId: "23845678901234576", dailyBudget: 30, status: "PAUSED", impressions: 95000, clicks: 4200, spend: 2200 },
    ],
    impressions: 95000,
    clicks: 4200,
    spend: 2200,
    conversions: 156,
  },
];

function loadCampaigns(): Campaign[] {
  if (typeof window === "undefined") return [...INITIAL_MOCK_CAMPAIGNS];
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as Campaign[];
      if (Array.isArray(parsed) && parsed.length > 0) return parsed;
    }
  } catch { /* fallback */ }
  const initial = [...INITIAL_MOCK_CAMPAIGNS];
  localStorage.setItem(STORAGE_KEY, JSON.stringify(initial));
  return initial;
}

function saveCampaigns(campaigns: Campaign[]): void {
  if (typeof window === "undefined") return;
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(campaigns));
  } catch { /* ignore */ }
}

const MOCK_CAMPAIGNS: Campaign[] = loadCampaigns();

export async function fetchCampaigns(): Promise<{ data: Campaign[]; total: number }> {
  return { data: [...MOCK_CAMPAIGNS], total: MOCK_CAMPAIGNS.length };
}

export async function createCampaign(data: CreateCampaignData): Promise<Campaign> {
  const campaign: Campaign = {
    id: `c_${Date.now()}`,
    profileId: "mock-profile",
    brandId: data.brandId,
    brandName: data.brandName,
    adAccountId: `act_${Date.now()}`,
    facebookCampaignId: null,
    name: data.name,
    objective: data.objective,
    budget: data.budget,
    startDate: data.startDate,
    endDate: data.endDate,
    status: "DRAFT",
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    adSets: [],
    impressions: 0,
    clicks: 0,
    spend: 0,
    conversions: 0,
  };
  MOCK_CAMPAIGNS.unshift(campaign);
  saveCampaigns(MOCK_CAMPAIGNS);
  return campaign;
}

export async function restartCampaign(id: string): Promise<Campaign | null> {
  const idx = MOCK_CAMPAIGNS.findIndex((c) => c.id === id);
  if (idx < 0) return null;
  MOCK_CAMPAIGNS[idx].status = "ACTIVE";
  if (!MOCK_CAMPAIGNS[idx].facebookCampaignId) {
    MOCK_CAMPAIGNS[idx].facebookCampaignId = `fb_${Date.now()}_${Math.random().toString(36).slice(2, 10)}`;
  }
  MOCK_CAMPAIGNS[idx].updatedAt = new Date().toISOString();
  saveCampaigns(MOCK_CAMPAIGNS);
  return MOCK_CAMPAIGNS[idx];
}

export async function applyCampaign(id: string): Promise<Campaign | null> {
  const idx = MOCK_CAMPAIGNS.findIndex((c) => c.id === id);
  if (idx < 0) return null;
  MOCK_CAMPAIGNS[idx].status = "ACTIVE";
  MOCK_CAMPAIGNS[idx].facebookCampaignId = `fb_${Date.now()}_${Math.random().toString(36).slice(2, 10)}`;
  MOCK_CAMPAIGNS[idx].updatedAt = new Date().toISOString();
  saveCampaigns(MOCK_CAMPAIGNS);
  return MOCK_CAMPAIGNS[idx];
}

export async function updateCampaignStatus(id: string, status: CampaignStatus): Promise<Campaign | null> {
  const idx = MOCK_CAMPAIGNS.findIndex((c) => c.id === id);
  if (idx < 0) return null;
  MOCK_CAMPAIGNS[idx].status = status;
  MOCK_CAMPAIGNS[idx].updatedAt = new Date().toISOString();
  saveCampaigns(MOCK_CAMPAIGNS);
  return MOCK_CAMPAIGNS[idx];
}

export async function updateCampaign(id: string, data: CreateCampaignData): Promise<Campaign | null> {
  const idx = MOCK_CAMPAIGNS.findIndex((c) => c.id === id);
  if (idx < 0) return null;
  MOCK_CAMPAIGNS[idx] = {
    ...MOCK_CAMPAIGNS[idx],
    name: data.name,
    brandId: data.brandId,
    brandName: data.brandName,
    objective: data.objective,
    budget: data.budget,
    startDate: data.startDate,
    endDate: data.endDate,
    updatedAt: new Date().toISOString(),
  };
  saveCampaigns(MOCK_CAMPAIGNS);
  return MOCK_CAMPAIGNS[idx];
}

export async function deleteCampaign(id: string): Promise<boolean> {
  const idx = MOCK_CAMPAIGNS.findIndex((c) => c.id === id);
  if (idx >= 0) {
    MOCK_CAMPAIGNS.splice(idx, 1);
    saveCampaigns(MOCK_CAMPAIGNS);
  }
  return idx >= 0;
}

export async function getCampaignById(id: string): Promise<Campaign | null> {
  return MOCK_CAMPAIGNS.find((c) => c.id === id) || null;
}
