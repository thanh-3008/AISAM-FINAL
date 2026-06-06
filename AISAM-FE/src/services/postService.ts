import { apiClient } from "@/lib/apiClient";

export type PostStatus = "Published" | "Scheduled" | "Failed" | "Draft";
export type PostPlatform = "facebook" | "instagram" | "tiktok" | "linkedin" | "youtube";
export type PostType = "IMAGE" | "TEXT" | "VIDEO" | "CAROUSEL" | "STORY";

export interface PostItem {
  id: string;
  contentId: string;
  integrationId: string;
  externalPostId: string | null;
  publishedAt: string;
  status: PostStatus;
  contentTitle: string | null;
  brandName: string | null;
  platform?: PostPlatform;
  type?: PostType;
  caption?: string;
  likes?: number;
  comments?: number;
  shares?: number;
  thumbnail?: string;
  errorMessage?: string;
  createdAt: string;
  updatedAt: string;
}

interface GenericResponse<T> {
  success: boolean;
  message?: string | null;
  statusCode?: number;
  data?: T;
  error?: { errorCode?: string; errorMessage?: string };
  timestamp?: string;
}

interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

function d(month: number, day: number, hour = 10, min = 0) {
  const date = new Date(2026, month - 1, day, hour, min);
  const isoDate = date.toISOString();
  return isoDate;
}

const MOCK_POSTS: PostItem[] = [
  // ── Published posts (14 items) ──
  { id: "p1", contentId: "c1", integrationId: "fb-1", externalPostId: "fb_post_1001", publishedAt: d(6, 1, 10, 30), status: "Published", contentTitle: "Smart Bulb Product Showcase", brandName: "Lumina Tech", platform: "facebook", type: "VIDEO", caption: "Exploring the future of generative AI in corporate workflows. #AI #Enterprise...", likes: 1240, comments: 84, shares: 150, createdAt: d(5, 25), updatedAt: d(6, 1, 10, 45) },
  { id: "p2", contentId: "c2", integrationId: "li-1", externalPostId: "li_post_2001", publishedAt: d(5, 30, 14, 0), status: "Published", contentTitle: "LED Strip Installation Guide", brandName: "Lumina Tech", platform: "linkedin", type: "TEXT", caption: "A step-by-step guide to installing LED strips in any space. #DIY #Lighting...", likes: 560, comments: 32, shares: 45, createdAt: d(5, 20), updatedAt: d(5, 30, 14, 30) },
  { id: "p3", contentId: "c4", integrationId: "ig-1", externalPostId: "ig_post_3001", publishedAt: d(5, 28, 9, 15), status: "Published", contentTitle: "Summit Tent - Built for Adventure", brandName: "Summit Outdoor", platform: "instagram", type: "VIDEO", caption: "Adventure awaits with the Summit Tent. Built for the toughest conditions. #Outdoor...", likes: 3200, comments: 215, shares: 320, createdAt: d(5, 22), updatedAt: d(5, 28, 9, 45) },
  { id: "p4", contentId: "c7", integrationId: "fb-1", externalPostId: "fb_post_1002", publishedAt: d(5, 25, 11, 0), status: "Published", contentTitle: "Heritage V8 Engine Rebuild", brandName: "Heritage Motors", platform: "facebook", type: "VIDEO", caption: "Watch the full rebuild of a Heritage V8 engine. Pure craftsmanship. #Engineering...", likes: 890, comments: 67, shares: 120, createdAt: d(5, 18), updatedAt: d(5, 25, 11, 30) },
  { id: "p5", contentId: "c9", integrationId: "ig-1", externalPostId: "ig_post_3002", publishedAt: d(6, 3, 8, 45), status: "Published", contentTitle: "Organic Tea - From Farm to Cup", brandName: "GreenLeaf Organics", platform: "instagram", type: "TEXT", caption: "From farm to cup - the journey of our organic tea. #Organic #TeaTime...", likes: 1450, comments: 98, shares: 180, createdAt: d(5, 28), updatedAt: d(6, 3, 9, 0) },
  { id: "p7", contentId: "c11", integrationId: "fb-1", externalPostId: "fb_post_1003", publishedAt: d(5, 20, 13, 30), status: "Published", contentTitle: "Budget App Feature Overview", brandName: "Pulse Finance", platform: "facebook", type: "VIDEO", caption: "Take control of your finances with our Budget App. #Finance #Budgeting...", likes: 2100, comments: 156, shares: 250, createdAt: d(5, 15), updatedAt: d(5, 20, 14, 0) },
  { id: "p8", contentId: "c12", integrationId: "ig-1", externalPostId: "ig_post_3003", publishedAt: d(6, 2, 10, 0), status: "Published", contentTitle: "Investment Portfolio Tracker Ad", brandName: "Pulse Finance", platform: "instagram", type: "IMAGE", caption: "Track your investments like a pro. #Investing #Portfolio...", likes: 780, comments: 45, shares: 90, createdAt: d(5, 28), updatedAt: d(6, 2, 10, 30) },
  { id: "p11", contentId: "c1", integrationId: "tt-1", externalPostId: "tt_post_001", publishedAt: d(5, 15, 16, 0), status: "Published", contentTitle: "Smart Bulb TikTok Teaser", brandName: "Lumina Tech", platform: "tiktok", type: "VIDEO", caption: "Watch the magic happen ✨ #SmartHome #TechTok", likes: 8500, comments: 432, shares: 1200, createdAt: d(5, 10), updatedAt: d(5, 15, 16, 45) },
  { id: "p12", contentId: "c4", integrationId: "fb-1", externalPostId: "fb_post_1004", publishedAt: d(5, 10, 9, 0), status: "Published", contentTitle: "Summit Tent - Winter Edition", brandName: "Summit Outdoor", platform: "facebook", type: "IMAGE", caption: "Brave the elements with the Summit Winter Tent. #Camping #Winter", likes: 2300, comments: 178, shares: 320, createdAt: d(5, 5), updatedAt: d(5, 10, 9, 30) },
  { id: "p13", contentId: "c7", integrationId: "ig-1", externalPostId: "ig_post_3004", publishedAt: d(6, 5, 14, 30), status: "Published", contentTitle: "Heritage Motors Behind the Scenes", brandName: "Heritage Motors", platform: "instagram", type: "IMAGE", caption: "A sneak peek at our latest V8 build. #CarRestoration #Engineering", likes: 670, comments: 41, shares: 85, createdAt: d(5, 30), updatedAt: d(6, 5, 15, 0) },
  { id: "p14", contentId: "c11", integrationId: "li-1", externalPostId: "li_post_2003", publishedAt: d(4, 28, 11, 0), status: "Published", contentTitle: "Pulse Finance Year in Review", brandName: "Pulse Finance", platform: "linkedin", type: "TEXT", caption: "Our 2025 year in review — growth, challenges, and what's next. #Finance #YearInReview", likes: 4300, comments: 289, shares: 520, createdAt: d(4, 20), updatedAt: d(4, 28, 11, 45) },
  { id: "p15", contentId: "c9", integrationId: "fb-1", externalPostId: "fb_post_1005", publishedAt: d(6, 6, 7, 0), status: "Published", contentTitle: "Organic Tea Summer Collection", brandName: "GreenLeaf Organics", platform: "facebook", type: "IMAGE", caption: "Introducing our Summer Tea Collection. Refreshing flavors for sunny days. #Summer #Tea", likes: 980, comments: 73, shares: 120, createdAt: d(5, 30), updatedAt: d(6, 6, 7, 30) },
  { id: "p16", contentId: "c2", integrationId: "ig-1", externalPostId: "ig_post_3005", publishedAt: d(4, 15, 15, 0), status: "Published", contentTitle: "LED Strip Color Palette Ideas", brandName: "Lumina Tech", platform: "instagram", type: "IMAGE", caption: "10 stunning color palettes for your LED strip setup. #HomeDecor #RGB", likes: 5100, comments: 324, shares: 650, createdAt: d(4, 8), updatedAt: d(4, 15, 15, 45) },

  // ── Scheduled posts (6 items) ──
  { id: "p9", contentId: "c13", integrationId: "li-1", externalPostId: "li_post_2002", publishedAt: d(6, 10, 9, 0), status: "Scheduled", contentTitle: "Morning Yoga Routine Tutorial", brandName: "Summit Outdoor", platform: "linkedin", type: "VIDEO", caption: "Start your day right with this morning yoga routine. #Yoga #Wellness...", createdAt: d(6, 1), updatedAt: d(6, 1) },
  { id: "p10", contentId: "c14", integrationId: "fb-1", externalPostId: null, publishedAt: d(6, 12, 15, 0), status: "Scheduled", contentTitle: "Organic Vitamin Pack Promotion", brandName: "GreenLeaf Organics", platform: "facebook", type: "IMAGE", caption: "Boost your immunity with our Vitamin Pack. #Health #Vitamins...", createdAt: d(6, 2), updatedAt: d(6, 2) },
  { id: "p17", contentId: "c3", integrationId: "ig-1", externalPostId: null, publishedAt: d(6, 15, 10, 0), status: "Scheduled", contentTitle: "Desk Lamp Night Mode Feature", brandName: "Lumina Tech", platform: "instagram", type: "IMAGE", caption: "New night mode feature coming soon. Stay tuned! #Design #Lighting", createdAt: d(6, 5), updatedAt: d(6, 5) },
  { id: "p18", contentId: "c6", integrationId: "fb-1", externalPostId: null, publishedAt: d(6, 18, 8, 30), status: "Scheduled", contentTitle: "Summit Jacket Spring Collection", brandName: "Summit Outdoor", platform: "facebook", type: "VIDEO", caption: "Lightweight. Durable. Ready for spring. #OutdoorGear #Spring", createdAt: d(6, 10), updatedAt: d(6, 10) },
  { id: "p19", contentId: "c8", integrationId: "li-1", externalPostId: null, publishedAt: d(6, 20, 11, 0), status: "Scheduled", contentTitle: "All-Terrain Tire Tech Deep Dive", brandName: "Heritage Motors", platform: "linkedin", type: "TEXT", caption: "The engineering behind our all-terrain tire technology. #Automotive #Tech", createdAt: d(6, 12), updatedAt: d(6, 12) },
  { id: "p20", contentId: "c5", integrationId: "tt-1", externalPostId: null, publishedAt: d(6, 22, 14, 0), status: "Scheduled", contentTitle: "TrailBlazer Backpack TikTok Unboxing", brandName: "Summit Outdoor", platform: "tiktok", type: "VIDEO", caption: "Unboxing the all-new TrailBlazer Backpack! #Hiking #GearReview", createdAt: d(6, 15), updatedAt: d(6, 15) },

  // ── Failed posts (5 items) ──
  { id: "p6", contentId: "c10", integrationId: "li-1", externalPostId: null, publishedAt: d(6, 4, 16, 20), status: "Failed", contentTitle: "Matcha Green Tea Powder Review", brandName: "GreenLeaf Organics", platform: "linkedin", type: "IMAGE", caption: "Our honest review of matcha green tea powder. #Health #Wellness...", errorMessage: "API Timeout", createdAt: d(5, 30), updatedAt: d(6, 4, 16, 20) },
  { id: "p21", contentId: "c1", integrationId: "ig-1", externalPostId: null, publishedAt: d(6, 2, 9, 0), status: "Failed", contentTitle: "Smart Bulb Instagram Carousel", brandName: "Lumina Tech", platform: "instagram", type: "CAROUSEL", caption: "Swipe to see our Smart Bulb in action! #SmartHome", errorMessage: "Rate limit exceeded", createdAt: d(5, 28), updatedAt: d(6, 2, 9, 0) },
  { id: "p22", contentId: "c12", integrationId: "tt-1", externalPostId: null, publishedAt: d(5, 28, 12, 0), status: "Failed", contentTitle: "Portfolio Tracker TikTok Ad", brandName: "Pulse Finance", platform: "tiktok", type: "VIDEO", caption: "Track your money like never before. #FinanceTok #Investing", errorMessage: "Authentication failed", createdAt: d(5, 22), updatedAt: d(5, 28, 12, 0) },
  { id: "p23", contentId: "c4", integrationId: "ig-1", externalPostId: null, publishedAt: d(5, 22, 15, 0), status: "Failed", contentTitle: "Summit Tent IG Story Series", brandName: "Summit Outdoor", platform: "instagram", type: "STORY", caption: "Behind the scenes of our Summit Tent photoshoot. #Adventure", errorMessage: "Media processing error", createdAt: d(5, 18), updatedAt: d(5, 22, 15, 0) },
  { id: "p24", contentId: "c7", integrationId: "li-1", externalPostId: null, publishedAt: d(5, 18, 10, 0), status: "Failed", contentTitle: "Heritage Motors LinkedIn Article", brandName: "Heritage Motors", platform: "linkedin", type: "TEXT", caption: "The future of electric heritage vehicles. #EV #Automotive", errorMessage: "Content policy violation", createdAt: d(5, 12), updatedAt: d(5, 18, 10, 0) },
];

function getMockPaged(params: PostFilters): PagedResult<PostItem> {
  const {
    page = 1,
    pageSize = 20,
    brandId,
    status,
    platform,
    type,
    search,
    dateFrom,
    dateTo,
    minLikes = 0,
    minComments = 0,
    minShares = 0,
  } = params;
  
  let filtered = [...MOCK_LIVE];
  
  // Apply filters
  if (brandId) filtered = filtered.filter((p) => p.brandName === brandId);
  if (status) filtered = filtered.filter((p) => p.status === status);
  if (platform) filtered = filtered.filter((p) => p.platform === platform);
  if (type) filtered = filtered.filter((p) => p.type === type);
  if (search) {
    const q = search.toLowerCase();
    filtered = filtered.filter((p) => 
      (p.contentTitle || "").toLowerCase().includes(q) || 
      (p.brandName || "").toLowerCase().includes(q) ||
      (p.caption || "").toLowerCase().includes(q)
    );
  }
  if (dateFrom) filtered = filtered.filter((p) => new Date(p.publishedAt) >= new Date(dateFrom));
  if (dateTo) filtered = filtered.filter((p) => new Date(p.publishedAt) <= new Date(dateTo + "T23:59:59"));
  if (minLikes > 0) filtered = filtered.filter((p) => (p.likes || 0) >= minLikes);
  if (minComments > 0) filtered = filtered.filter((p) => (p.comments || 0) >= minComments);
  if (minShares > 0) filtered = filtered.filter((p) => (p.shares || 0) >= minShares);
  
  const totalCount = filtered.length;
  const startIndex = (page - 1) * pageSize;
  const data = filtered.slice(startIndex, startIndex + pageSize);
  
  return {
    data,
    totalCount,
    page,
    pageSize,
    totalPages: Math.ceil(totalCount / pageSize),
    hasNextPage: page * pageSize < totalCount,
    hasPreviousPage: page > 1,
  };
}

export interface PostFilters {
  page?: number;
  pageSize?: number;
  brandId?: string;
  status?: PostStatus;
  platform?: PostPlatform;
  type?: PostType;
  search?: string;
  dateFrom?: string;
  dateTo?: string;
  minLikes?: number;
  minComments?: number;
  minShares?: number;
}

export async function fetchPosts(params?: PostFilters): Promise<PagedResult<PostItem>> {
  try {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", String(params.page));
    if (params?.pageSize) query.set("pageSize", String(params.pageSize));
    if (params?.brandId) query.set("brandId", params.brandId);
    if (params?.status) query.set("status", params.status);
    if (params?.platform) query.set("platform", params.platform);
    if (params?.type) query.set("type", params.type);
    if (params?.search) query.set("search", params.search);
    if (params?.dateFrom) query.set("dateFrom", params.dateFrom);
    if (params?.dateTo) query.set("dateTo", params.dateTo);
    if (params?.minLikes) query.set("minLikes", String(params.minLikes));
    if (params?.minComments) query.set("minComments", String(params.minComments));
    if (params?.minShares) query.set("minShares", String(params.minShares));
    
    const res: GenericResponse<PagedResult<PostItem>> = await apiClient(`/posts?${query.toString()}`);
    if (res?.data?.data?.length) return res.data;
  } catch { /* fallback */ }
  return getMockPaged(params || {});
}

/* ── Mock CRUD ── */
let MOCK_LIVE = [...MOCK_POSTS];
let nextId = 25;

export async function createPost(data: {
  contentTitle: string;
  brandName: string;
  platform: PostPlatform;
  type: PostType;
  caption: string;
  publishedAt: string;
  status: PostStatus;
}): Promise<PostItem> {
  const id = `p${++nextId}`;
  const now = new Date().toISOString();
  const post: PostItem = {
    id,
    contentId: `c${nextId}`,
    integrationId: `${data.platform}-1`,
    externalPostId: null,
    publishedAt: data.publishedAt || now,
    status: data.status,
    contentTitle: data.contentTitle,
    brandName: data.brandName,
    platform: data.platform,
    type: data.type,
    caption: data.caption,
    likes: 0,
    comments: 0,
    shares: 0,
    createdAt: now,
    updatedAt: now,
  };
  MOCK_LIVE.unshift(post);
  return post;
}

export async function updatePost(id: string, updates: Partial<PostItem>): Promise<PostItem | null> {
  const idx = MOCK_LIVE.findIndex((p) => p.id === id);
  if (idx === -1) return null;
  MOCK_LIVE[idx] = { ...MOCK_LIVE[idx], ...updates };
  return MOCK_LIVE[idx];
}

export async function deletePost(id: string): Promise<boolean> {
  const len = MOCK_LIVE.length;
  MOCK_LIVE = MOCK_LIVE.filter((p) => p.id !== id);
  return MOCK_LIVE.length < len;
}

export async function retryPost(id: string): Promise<PostItem | null> {
  const idx = MOCK_LIVE.findIndex((p) => p.id === id);
  if (idx === -1) return null;
  MOCK_LIVE[idx] = { ...MOCK_LIVE[idx], status: "Published", errorMessage: undefined, externalPostId: `${MOCK_LIVE[idx].platform}_post_${Date.now()}` };
  return MOCK_LIVE[idx];
}
