export type ContentType = "IMAGE" | "TEXT" | "VIDEO";
export type ContentStatus = "Published" | "Scheduled" | "Draft" | "Awaiting Approval";

export interface ContentItem {
  id: string;
  title: string;
  brandName: string;
  productName: string;
  type: ContentType;
  status: ContentStatus;
  thumbnail: string;
  createdAt: string;
  platforms: string[];
  tags?: string[];
  hashtags?: string[];
}

export interface ContentDetail extends ContentItem {
  updatedAt: string;
  textContent?: string;
  description?: string;
  imageUrl?: string;
  videoUrl?: string;
  duration?: string;
  dimensions?: string;
  fileSize?: string;
  scheduledAt?: string;
  caption?: string;
  ctaLink?: string;
  internalNotes?: string;
}

export const ALL_TAGS = ["Product Launch", "Tutorial", "Seasonal", "Brand Story", "Behind the Scenes", "Testimonial", "Promotion", "Educational"];

const DEFAULT_TAGS: Record<string, string[]> = {
  c1: ["Product Launch", "Promotion"],
  c2: ["Tutorial", "Educational"],
  c3: ["Product Launch", "Seasonal"],
  c4: ["Brand Story", "Behind the Scenes"],
  c5: ["Testimonial"],
  c6: ["Seasonal", "Promotion"],
  c7: ["Brand Story", "Behind the Scenes"],
  c8: ["Educational"],
  c9: ["Brand Story"],
  c10: ["Promotion", "Seasonal"],
  c11: ["Product Launch", "Educational"],
  c12: ["Educational"],
  c13: ["Educational"],
  c14: ["Brand Story"],
};

export const MOCK_CONTENT: ContentItem[] = [
  { id: "c1", title: "Smart Bulb Product Showcase", brandName: "Lumina Tech", productName: "Smart Bulb", type: "VIDEO", status: "Published", thumbnail: "", createdAt: "2025-05-28T10:00:00Z", platforms: ["facebook", "instagram", "tiktok"], tags: DEFAULT_TAGS.c1 },
  { id: "c2", title: "LED Strip Installation Guide", brandName: "Lumina Tech", productName: "LED Strip", type: "TEXT", status: "Published", thumbnail: "", createdAt: "2025-05-25T14:30:00Z", platforms: ["facebook", "instagram"], tags: DEFAULT_TAGS.c2 },
  { id: "c3", title: "Midnight Blue Desk Lamp Ad", brandName: "Lumina Tech", productName: "Desk Lamp", type: "IMAGE", status: "Scheduled", thumbnail: "", createdAt: "2025-06-02T09:00:00Z", platforms: ["instagram", "facebook"], tags: DEFAULT_TAGS.c3 },
  { id: "c4", title: "Summit Tent - Built for Adventure", brandName: "Summit Outdoor", productName: "Tent", type: "VIDEO", status: "Published", thumbnail: "", createdAt: "2025-05-20T08:00:00Z", platforms: ["tiktok", "instagram"], tags: DEFAULT_TAGS.c4 },
  { id: "c5", title: "TrailBlazer Backpack Review", brandName: "Summit Outdoor", productName: "Backpack", type: "TEXT", status: "Awaiting Approval", thumbnail: "", createdAt: "2025-06-01T16:00:00Z", platforms: ["instagram"], tags: DEFAULT_TAGS.c5 },
  { id: "c6", title: "Winter Jacket Campaign", brandName: "Summit Outdoor", productName: "Jacket", type: "IMAGE", status: "Draft", thumbnail: "", createdAt: "2025-05-30T11:00:00Z", platforms: ["facebook", "instagram"], tags: DEFAULT_TAGS.c6 },
  { id: "c7", title: "Heritage V8 Engine Rebuild", brandName: "Heritage Motors", productName: "Engine Kit", type: "VIDEO", status: "Published", thumbnail: "", createdAt: "2025-04-15T13:00:00Z", platforms: ["tiktok", "facebook"], tags: DEFAULT_TAGS.c7 },
  { id: "c8", title: "All-Terrain Tire Review", brandName: "Heritage Motors", productName: "Tire Set", type: "IMAGE", status: "Scheduled", thumbnail: "", createdAt: "2025-06-03T07:00:00Z", platforms: ["instagram", "facebook"], tags: DEFAULT_TAGS.c8 },
  { id: "c9", title: "Organic Tea - From Farm to Cup", brandName: "GreenLeaf Organics", productName: "Organic Tea", type: "TEXT", status: "Published", thumbnail: "", createdAt: "2025-05-22T10:30:00Z", platforms: ["facebook", "instagram", "tiktok"], tags: DEFAULT_TAGS.c9 },
  { id: "c10", title: "Daily Wellness Pack Ad", brandName: "GreenLeaf Organics", productName: "Vitamin Pack", type: "IMAGE", status: "Scheduled", thumbnail: "", createdAt: "2025-06-04T12:00:00Z", platforms: ["instagram"], tags: DEFAULT_TAGS.c10 },
  { id: "c11", title: "Budget App Feature Overview", brandName: "Pulse Finance", productName: "Budget App", type: "VIDEO", status: "Published", thumbnail: "", createdAt: "2025-05-18T15:00:00Z", platforms: ["tiktok", "facebook", "instagram"], tags: DEFAULT_TAGS.c11 },
  { id: "c12", title: "Portfolio Tracker Infographic", brandName: "Pulse Finance", productName: "Portfolio Tracker", type: "IMAGE", status: "Awaiting Approval", thumbnail: "", createdAt: "2025-06-05T09:30:00Z", platforms: ["facebook", "instagram"], tags: DEFAULT_TAGS.c12 },
  { id: "c13", title: "Smart Home Energy Savings", brandName: "Lumina Tech", productName: "Smart Bulb", type: "TEXT", status: "Draft", thumbnail: "", createdAt: "2025-06-06T08:00:00Z", platforms: ["instagram"], tags: DEFAULT_TAGS.c13 },
  { id: "c14", title: "Heritage Motors Brand Film", brandName: "Heritage Motors", productName: "Engine Kit", type: "VIDEO", status: "Scheduled", thumbnail: "", createdAt: "2025-06-07T10:00:00Z", platforms: ["facebook", "instagram"], tags: DEFAULT_TAGS.c14 },
];

export const MOCK_DETAILS: Record<string, ContentDetail> = {
  c1: {
    id: "c1", title: "Smart Bulb Product Showcase", brandName: "Lumina Tech", productName: "Smart Bulb",
    type: "VIDEO", status: "Published", thumbnail: "", createdAt: "2025-05-28T10:00:00Z", updatedAt: "2025-06-01T14:00:00Z",
    platforms: ["facebook", "instagram", "tiktok"],
    description: "A dynamic video showcasing the features of the Lumina Smart Bulb with energy-saving statistics and smart home integration demos.",
    videoUrl: "", duration: "2:34", fileSize: "45 MB",
  },
  c2: {
    id: "c2", title: "LED Strip Installation Guide", brandName: "Lumina Tech", productName: "LED Strip",
    type: "TEXT", status: "Published", thumbnail: "", createdAt: "2025-05-25T14:30:00Z", updatedAt: "2025-05-28T09:00:00Z",
    platforms: ["facebook", "instagram"],
    textContent: "Installing your new LED Strip is easy! Follow these simple steps:\n\n1. Clean the surface where you'll mount the strip.\n2. Measure and cut the strip at the marked cutting points.\n3. Peel off the adhesive backing and press firmly.\n4. Connect the power adapter and test.\n\nPro tip: Use corner connectors for clean 90-degree turns around cabinets and shelves.",
    description: "Step-by-step installation guide for Lumina LED Strip Lights.",
  },
  c3: {
    id: "c3", title: "Midnight Blue Desk Lamp Ad", brandName: "Lumina Tech", productName: "Desk Lamp",
    type: "IMAGE", status: "Scheduled", thumbnail: "", createdAt: "2025-06-02T09:00:00Z", updatedAt: "2025-06-02T09:00:00Z",
    platforms: ["instagram", "facebook"],
    description: "Elegant product photography for the new Midnight Blue Desk Lamp collection.",
    imageUrl: "", dimensions: "1080 x 1080 px", fileSize: "2.4 MB",
  },
  c4: {
    id: "c4", title: "Summit Tent - Built for Adventure", brandName: "Summit Outdoor", productName: "Tent",
    type: "VIDEO", status: "Published", thumbnail: "", createdAt: "2025-05-20T08:00:00Z", updatedAt: "2025-05-22T10:00:00Z",
    platforms: ["tiktok", "instagram"],
    description: "An epic adventure film showcasing the Summit Tent in extreme weather conditions.",
    videoUrl: "", duration: "3:45", fileSize: "68 MB",
  },
  c5: {
    id: "c5", title: "TrailBlazer Backpack Review", brandName: "Summit Outdoor", productName: "Backpack",
    type: "TEXT", status: "Awaiting Approval", thumbnail: "", createdAt: "2025-06-01T16:00:00Z", updatedAt: "2025-06-02T09:00:00Z",
    platforms: ["instagram"],
    textContent: "The TrailBlazer Backpack is built for the modern adventurer. With 40L of capacity, waterproof zippers, and ergonomic padding, it's the perfect companion for any trail.\n\nKey Features:\n- Waterproof 600D polyester\n- Ergonomic shoulder straps\n- Multiple compartments\n- Hidden rain cover\n\nAvailable in Forest Green, Midnight Black, and Sunset Orange.",
    description: "A detailed review of the TrailBlazer Backpack for outdoor enthusiasts.",
  },
  c6: {
    id: "c6", title: "Winter Jacket Campaign", brandName: "Summit Outdoor", productName: "Jacket",
    type: "IMAGE", status: "Draft", thumbnail: "", createdAt: "2025-05-30T11:00:00Z", updatedAt: "2025-05-30T11:00:00Z",
    platforms: ["facebook", "instagram"],
    description: "Winter collection campaign visuals featuring the Summit Premium Down Jacket.",
    imageUrl: "", dimensions: "1200 x 1200 px", fileSize: "3.1 MB",
  },
  c7: {
    id: "c7", title: "Heritage V8 Engine Rebuild", brandName: "Heritage Motors", productName: "Engine Kit",
    type: "VIDEO", status: "Published", thumbnail: "", createdAt: "2025-04-15T13:00:00Z", updatedAt: "2025-04-20T11:00:00Z",
    platforms: ["tiktok", "facebook"],
    description: "Watch our master technicians rebuild a classic V8 engine from start to finish.",
    videoUrl: "", duration: "8:12", fileSize: "120 MB",
  },
  c8: {
    id: "c8", title: "All-Terrain Tire Review", brandName: "Heritage Motors", productName: "Tire Set",
    type: "IMAGE", status: "Scheduled", thumbnail: "", createdAt: "2025-06-03T07:00:00Z", updatedAt: "2025-06-03T07:00:00Z",
    platforms: ["instagram", "facebook"],
    description: "High-resolution product shot of the Heritage All-Terrain Tire Set on rocky terrain.",
    imageUrl: "", dimensions: "1920 x 1080 px", fileSize: "4.2 MB",
  },
  c9: {
    id: "c9", title: "Organic Tea - From Farm to Cup", brandName: "GreenLeaf Organics", productName: "Organic Tea",
    type: "TEXT", status: "Published", thumbnail: "", createdAt: "2025-05-22T10:30:00Z", updatedAt: "2025-05-24T08:00:00Z",
    platforms: ["facebook", "instagram", "tiktok"],
    textContent: "From the misty highlands of Darjeeling to your morning cup — every leaf of GreenLeaf Organic Tea is hand-picked and sustainably sourced.\n\nOur promise:\n- 100% certified organic\n- No artificial flavors\n- Compostable packaging\n- Fair trade certified\n\nTaste the difference nature makes.",
    description: "Story of how GreenLeaf Organic Tea is sourced from farm to your cup.",
  },
  c10: {
    id: "c10", title: "Daily Wellness Pack Ad", brandName: "GreenLeaf Organics", productName: "Vitamin Pack",
    type: "IMAGE", status: "Scheduled", thumbnail: "", createdAt: "2025-06-04T12:00:00Z", updatedAt: "2025-06-04T12:00:00Z",
    platforms: ["instagram"],
    description: "Clean and vibrant product photography for the Daily Wellness Vitamin Pack.",
    imageUrl: "", dimensions: "1080 x 1350 px", fileSize: "1.8 MB",
  },
  c11: {
    id: "c11", title: "Budget App Feature Overview", brandName: "Pulse Finance", productName: "Budget App",
    type: "VIDEO", status: "Published", thumbnail: "", createdAt: "2025-05-18T15:00:00Z", updatedAt: "2025-05-20T09:00:00Z",
    platforms: ["tiktok", "facebook", "instagram"],
    description: "A walkthrough of the Pulse Budget App's key features including AI-powered spending insights.",
    videoUrl: "", duration: "4:18", fileSize: "52 MB",
  },
  c12: {
    id: "c12", title: "Portfolio Tracker Infographic", brandName: "Pulse Finance", productName: "Portfolio Tracker",
    type: "IMAGE", status: "Awaiting Approval", thumbnail: "", createdAt: "2025-06-05T09:30:00Z", updatedAt: "2025-06-05T09:30:00Z",
    platforms: ["facebook", "instagram"],
    description: "An infographic showing how the Portfolio Tracker simplifies investment management.",
    imageUrl: "", dimensions: "1080 x 1920 px", fileSize: "2.7 MB",
  },
  c13: {
    id: "c13", title: "Smart Home Energy Savings", brandName: "Lumina Tech", productName: "Smart Bulb",
    type: "TEXT", status: "Draft", thumbnail: "", createdAt: "2025-06-06T08:00:00Z", updatedAt: "2025-06-06T08:00:00Z",
    platforms: ["instagram"],
    textContent: "Did you know switching to Lumina Smart Bulbs can reduce your energy bill by up to 60%?\n\nOur latest study shows:\n- Average savings: $142/year per household\n- Lifespan: 25,000 hours (10x traditional bulbs)\n- Compatible with all major smart home systems\n\nMake the switch today and start saving.",
    description: "An article about energy savings with Lumina Smart Bulbs.",
  },
  c14: {
    id: "c14", title: "Heritage Motors Brand Film", brandName: "Heritage Motors", productName: "Engine Kit",
    type: "VIDEO", status: "Scheduled", thumbnail: "", createdAt: "2025-06-07T10:00:00Z", updatedAt: "2025-06-07T10:00:00Z",
    platforms: ["facebook", "instagram"],
    description: "A cinematic brand film celebrating 50 years of Heritage Motors craftsmanship.",
    videoUrl: "", duration: "6:30", fileSize: "95 MB",
  },
};
