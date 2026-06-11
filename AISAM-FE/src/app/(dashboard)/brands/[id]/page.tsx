"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { apiClient, apiFetch } from "@/lib/apiClient";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import ProductModal, { type Product } from "@/components/brands/ProductModal";

interface Brand {
  id: string;
  userId: string;
  name: string;
  description: string | null;
  logoUrl: string | null;
  slogan: string | null;
  usp: string | null;
  targetAudience: string | null;
  profileId: string | null;
  createdAt: string;
  updatedAt: string;
  productsCount: number;
  contentsCount: number;
}

interface Campaign {
  id: string;
  brandId: string;
  name: string;
  platform: string;
  platformColor: string;
  platformBg: string;
  status: string;
  budget: string;
  spent: string;
  createdAt: string;
}

const MOCK_BRANDS: Brand[] = [
  { id: "mock-1", userId: "", name: "Lumina Tech", description: "Next-gen lighting solutions for smart homes and offices.", logoUrl: "", slogan: "Innovate Your Light", usp: "Smart lighting that adapts to your lifestyle", targetAudience: "Tech-savvy homeowners", profileId: null, productsCount: 3, contentsCount: 2, createdAt: "2025-01-15T00:00:00Z", updatedAt: "2025-06-04T00:00:00Z" },
  { id: "mock-2", userId: "", name: "Summit Outdoor", description: "Premium outdoor gear for adventure enthusiasts.", logoUrl: "", slogan: "Conquer Every Peak", usp: null, targetAudience: null, profileId: null, productsCount: 2, contentsCount: 0, createdAt: "2025-03-20T00:00:00Z", updatedAt: "2025-05-28T00:00:00Z" },
  { id: "mock-3", userId: "", name: "Heritage Motors", description: "Luxury automotive restoration and customization.", logoUrl: "", slogan: "Timeless Craftsmanship", usp: null, targetAudience: null, profileId: null, productsCount: 3, contentsCount: 2, createdAt: "2024-11-01T00:00:00Z", updatedAt: "2025-04-10T00:00:00Z" },
  { id: "mock-4", userId: "", name: "GreenLeaf Organics", description: "Organic farm-to-table produce and sustainable goods.", logoUrl: "", slogan: null, usp: null, targetAudience: null, profileId: null, productsCount: 3, contentsCount: 1, createdAt: "2025-02-10T00:00:00Z", updatedAt: "2025-06-01T00:00:00Z" },
  { id: "mock-5", userId: "", name: "Pulse Finance", description: "Real-time financial analytics and portfolio management.", logoUrl: "", slogan: null, usp: null, targetAudience: null, profileId: null, productsCount: 2, contentsCount: 3, createdAt: "2025-04-05T00:00:00Z", updatedAt: "2025-05-30T00:00:00Z" },
  { id: "mock-6", userId: "", name: "Apex Fitness", description: "AI-powered fitness tracking and workout planning.", logoUrl: "", slogan: null, usp: null, targetAudience: null, profileId: null, productsCount: 1, contentsCount: 0, createdAt: "2025-05-01T00:00:00Z", updatedAt: "2025-05-25T00:00:00Z" },
];

const MOCK_PRODUCTS: Record<string, Product[]> = {
  "mock-1": [
    { id: "p-1", brandId: "mock-1", name: "Smart LED Bulb Pro", description: "WiFi-enabled RGB smart bulb with voice control support and energy monitoring.", price: 49.99, stock: 142, createdAt: "2025-02-01T00:00:00Z" },
    { id: "p-2", brandId: "mock-1", name: "Home Hub Controller", description: "Central smart home hub with Matter protocol support for seamless integration.", price: 129.99, stock: 87, createdAt: "2025-03-15T00:00:00Z" },
    { id: "p-3", brandId: "mock-1", name: "Motion Sensor Switch", description: "Wireless motion-activated light switch with ambient light sensing.", price: 24.99, stock: 0, createdAt: "2025-05-01T00:00:00Z" },
  ],
  "mock-2": [
    { id: "p-4", brandId: "mock-2", name: "TrailBlazer Backpack 45L", description: "Lightweight waterproof hiking backpack with ergonomic support system.", price: 89.99, stock: 0, createdAt: "2025-04-10T00:00:00Z" },
    { id: "p-5", brandId: "mock-2", name: "Summit Tent 4P", description: "Four-season expedition tent for 4 people with quick-setup frame.", price: 349.99, stock: 23, createdAt: "2025-04-20T00:00:00Z" },
  ],
  "mock-3": [
    { id: "p-6", brandId: "mock-3", name: "Classic Coupe Restoration Kit", description: "Complete restoration package for 1960s coupes with authentic parts.", price: 2499.99, stock: 12, createdAt: "2024-12-01T00:00:00Z" },
    { id: "p-7", brandId: "mock-3", name: "Vintage Interior Set - Leather", description: "Hand-stitched premium leather interior replacement set.", price: 899.99, stock: 8, createdAt: "2025-01-15T00:00:00Z" },
    { id: "p-8", brandId: "mock-3", name: "Chrome Trim Package", description: "Show-quality chrome trim for classic and vintage models.", price: 449.99, stock: 0, createdAt: "2025-02-20T00:00:00Z" },
  ],
  "mock-4": [
    { id: "p-9", brandId: "mock-4", name: "Organic Produce Box - Weekly", description: "Farm-fresh organic vegetables and fruits delivered weekly.", price: 39.99, stock: 320, createdAt: "2025-02-15T00:00:00Z" },
    { id: "p-10", brandId: "mock-4", name: "Compostable Cutlery Set", description: "50-piece bamboo cutlery set, biodegradable and reusable.", price: 12.99, stock: 550, createdAt: "2025-03-01T00:00:00Z" },
    { id: "p-11", brandId: "mock-4", name: "Reusable Produce Bags - 5pk", description: "Organic cotton mesh produce bags with tare weight printed.", price: 9.99, stock: 0, createdAt: "2025-04-10T00:00:00Z" },
  ],
  "mock-5": [
    { id: "p-12", brandId: "mock-5", name: "Portfolio Tracker - Monthly", description: "Real-time portfolio performance tracking with AI insights.", price: 19.99, stock: 999, createdAt: "2025-04-10T00:00:00Z" },
    { id: "p-13", brandId: "mock-5", name: "Market Analytics Dashboard", description: "Advanced market analytics with AI-driven predictions and trends.", price: 49.99, stock: 999, createdAt: "2025-04-15T00:00:00Z" },
  ],
  "mock-6": [
    { id: "p-14", brandId: "mock-6", name: "Apex Workout Planner", description: "AI-generated personalized workout plans based on your fitness level.", price: 14.99, stock: 0, createdAt: "2025-05-10T00:00:00Z" },
  ],
};

const MOCK_CAMPAIGNS: Record<string, Campaign[]> = {
  "mock-1": [
    { id: "c-1", brandId: "mock-1", name: "Winter Smart Home Campaign", platform: "FACEBOOK", platformColor: "text-blue-600", platformBg: "bg-blue-50", status: "Active", budget: "$5,000", spent: "$3,240", createdAt: "2025-05-01T00:00:00Z" },
    { id: "c-2", brandId: "mock-1", name: "Energy Savings Promotion", platform: "INSTAGRAM", platformColor: "text-pink-600", platformBg: "bg-pink-50", status: "Active", budget: "$2,500", spent: "$1,100", createdAt: "2025-05-15T00:00:00Z" },
  ],
  "mock-3": [
    { id: "c-3", brandId: "mock-3", name: "Classic Car Show Event", platform: "INSTAGRAM", platformColor: "text-pink-600", platformBg: "bg-pink-50", status: "Active", budget: "$3,000", spent: "$2,800", createdAt: "2025-03-01T00:00:00Z" },
    { id: "c-4", brandId: "mock-3", name: "Restoration Workshop Series", platform: "FACEBOOK", platformColor: "text-blue-600", platformBg: "bg-blue-50", status: "Draft", budget: "$1,500", spent: "$0", createdAt: "2025-04-10T00:00:00Z" },
  ],
  "mock-4": [
    { id: "c-5", brandId: "mock-4", name: "Farm to Table Awareness", platform: "LINKEDIN", platformColor: "text-blue-700", platformBg: "bg-blue-50", status: "Active", budget: "$1,800", spent: "$720", createdAt: "2025-05-01T00:00:00Z" },
  ],
  "mock-5": [
    { id: "c-6", brandId: "mock-5", name: "Q2 Financial Webinar", platform: "LINKEDIN", platformColor: "text-blue-700", platformBg: "bg-blue-50", status: "Active", budget: "$8,000", spent: "$4,200", createdAt: "2025-04-20T00:00:00Z" },
    { id: "c-7", brandId: "mock-5", name: "Retirement Planning Guide", platform: "FACEBOOK", platformColor: "text-blue-600", platformBg: "bg-blue-50", status: "Active", budget: "$3,500", spent: "$1,850", createdAt: "2025-05-05T00:00:00Z" },
    { id: "c-8", brandId: "mock-5", name: "Investor Education Series", platform: "INSTAGRAM", platformColor: "text-pink-600", platformBg: "bg-pink-50", status: "Draft", budget: "$2,000", spent: "$0", createdAt: "2025-05-20T00:00:00Z" },
  ],
};

const BRAND_PLATFORMS: Record<string, { icon: string; label: string; color: string }[]> = {
  "mock-1": [
    { icon: "hub", label: "Meta Ads", color: "text-primary" },
    { icon: "ads_click", label: "Google Ads", color: "text-tertiary" },
    { icon: "music_note", label: "TikTok Ads", color: "text-on-surface" },
  ],
  "mock-2": [
    { icon: "hub", label: "Meta Ads", color: "text-primary" },
  ],
  "mock-3": [
    { icon: "hub", label: "Meta Ads", color: "text-primary" },
    { icon: "ads_click", label: "Google Ads", color: "text-tertiary" },
  ],
  "mock-4": [
    { icon: "hub", label: "Meta Ads", color: "text-primary" },
    { icon: "music_note", label: "TikTok Ads", color: "text-on-surface" },
  ],
  "mock-5": [
    { icon: "hub", label: "Meta Ads", color: "text-primary" },
    { icon: "ads_click", label: "Google Ads", color: "text-tertiary" },
    { icon: "music_note", label: "TikTok Ads", color: "text-on-surface" },
  ],
  "mock-6": [],
};

const BRAND_COLORS: Record<string, string> = {
  "mock-1": "from-blue-600 to-blue-400",
  "mock-2": "from-emerald-600 to-emerald-400",
  "mock-3": "from-violet-600 to-violet-400",
  "mock-4": "from-amber-600 to-amber-400",
  "mock-5": "from-rose-600 to-rose-400",
  "mock-6": "from-cyan-600 to-cyan-400",
};

const PRODUCT_ADS: Record<string, number> = {
  "p-1": 42, "p-2": 28, "p-3": 0,
  "p-4": 0, "p-5": 0,
  "p-6": 18, "p-7": 12, "p-8": 5,
  "p-9": 36, "p-10": 21, "p-11": 0,
  "p-12": 115, "p-13": 89,
  "p-14": 0,
};

const tabs = [
  { key: "products", label: "Products" },
  { key: "campaigns", label: "Campaigns" },
  { key: "settings", label: "Settings" },
] as const;

type TabKey = (typeof tabs)[number]["key"];

function getInitials(name: string) {
  return name.split(" ").map((w) => w[0]).join("").toUpperCase().slice(0, 2) || "?";
}

const inputClass =
  "w-full rounded-xl border border-outline-variant bg-surface-container-low px-4 py-2 text-body-sm placeholder:text-outline/40 focus:ring-2 focus:ring-primary/20 outline-none transition-all";

const labelClass = "font-label-md text-label-md text-on-surface-variant";

export default function BrandDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const { activeWorkspace } = useWorkspaces();
  const [brand, setBrand] = useState<Brand | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabKey>("products");
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showAddModal, setShowAddModal] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [viewingProduct, setViewingProduct] = useState<Product | null>(null);
  const [deletingProduct, setDeletingProduct] = useState<Product | null>(null);
  const [productSearch, setProductSearch] = useState("");
  const [products, setProducts] = useState<Product[]>(() => MOCK_PRODUCTS[id] || []);
  const [campaigns, setCampaigns] = useState<Campaign[]>(() => MOCK_CAMPAIGNS[id] || []);
  const [visible, setVisible] = useState(false);

  const [form, setForm] = useState({
    name: "",
    description: "",
    logoUrl: "",
    slogan: "",
    usp: "",
    targetAudience: "",
  });

  useEffect(() => {
    const timer = setTimeout(() => setVisible(true), 80);
    return () => clearTimeout(timer);
  }, []);

  useEffect(() => {
    if (!id) { setLoading(false); return; }
    let usedMock = false;
    const load = async () => {
    const fetchBrand = async () => {
      try {
        const result = await apiFetch(`/brands/${id}`);
        if (result?.success && result.data) {
          const b = result.data as Brand;
          setBrand(b);
          setForm({ name: b.name, description: b.description || "", logoUrl: b.logoUrl || "", slogan: b.slogan || "", usp: b.usp || "", targetAudience: b.targetAudience || "" });
          return;
        }
      } catch {
        // fallback to mock
      }
      const mockBrand = MOCK_BRANDS.find((b) => b.id === id);
      if (mockBrand) {
        setBrand(mockBrand);
        setForm({ name: mockBrand.name, description: mockBrand.description || "", logoUrl: mockBrand.logoUrl || "", slogan: mockBrand.slogan || "", usp: mockBrand.usp || "", targetAudience: mockBrand.targetAudience || "" });
        usedMock = true;
      } else {
        setError("Brand not found");
      }
    };
    const fetchProducts = async () => {
      if (usedMock) {
        const mock = MOCK_PRODUCTS[id];
        if (mock) setProducts(mock);
        return;
      }
      try {
        const result = await apiFetch(`/products?brandId=${id}`);
        if (result?.success && Array.isArray(result.data?.data)) {
          setProducts(result.data.data as Product[]);
          return;
        }
      } catch {
        // fallback to mock
      }
      const mock = MOCK_PRODUCTS[id];
      if (mock) setProducts(mock);
    };
    const fetchCampaigns = async () => {
      if (usedMock) {
        const mock = MOCK_CAMPAIGNS[id];
        if (mock) setCampaigns(mock);
        return;
      }
      try {
        const result = await apiFetch(`/campaigns?brandId=${id}`);
        if (result?.success && Array.isArray(result.data)) {
          setCampaigns(result.data as Campaign[]);
          return;
        }
      } catch {
        // fallback to mock
      }
      const mock = MOCK_CAMPAIGNS[id];
      if (mock) setCampaigns(mock);
    };
    await fetchBrand();
    await Promise.all([fetchProducts(), fetchCampaigns()]).finally(() => setLoading(false));
    };
    load();
  }, [id]);

  const handleSave = async () => {
    if (!form.name.trim()) { setError("Brand name is required"); return; }
    setSaving(true);
    setError(null);
    const updatedBrand: Brand = {
      ...(brand as Brand),
      name: form.name.trim(),
      description: form.description.trim() || null,
      logoUrl: form.logoUrl.trim() || null,
      slogan: form.slogan.trim() || null,
      usp: form.usp.trim() || null,
      targetAudience: form.targetAudience.trim() || null,
    };
    try {
      const body: Record<string, string> = { name: form.name.trim() };
      if (form.description.trim()) body.description = form.description.trim();
      if (form.logoUrl.trim()) body.logoUrl = form.logoUrl.trim();
      if (form.slogan.trim()) body.slogan = form.slogan.trim();
      if (form.usp.trim()) body.usp = form.usp.trim();
      if (form.targetAudience.trim()) body.targetAudience = form.targetAudience.trim();

      const result = await apiClient(`/brands/${id}`, { method: "PUT", data: body });
      if (result?.success && result.data) {
        setBrand(result.data);
      } else {
        setBrand(updatedBrand);
      }
    } catch {
      setBrand(updatedBrand);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setShowDeleteDialog(false);
    try {
      await apiFetch(`/brands/${id}`, { method: "DELETE" });
    } catch {
      // mock fallback — navigate away regardless
    }
    router.push("/brands");
  };

  const handleAddProduct = (product: Product) => {
    setProducts((prev) => {
      const next = [product, ...prev];
      MOCK_PRODUCTS[id] = next;
      return next;
    });
  };

  const handleEditProduct = (updated: Product) => {
    setProducts((prev) => {
      const next = prev.map((p) => (p.id === updated.id ? updated : p));
      MOCK_PRODUCTS[id] = next;
      return next;
    });
  };

  const handleDeleteProduct = async () => {
    if (!deletingProduct) return;
    const target = deletingProduct;
    setDeletingProduct(null);
    try {
      await apiFetch(`/products/${target.id}`, { method: "DELETE" });
    } catch {
      // mock fallback — remove from local state
    }
    setProducts((prev) => {
      const next = prev.filter((p) => p.id !== target.id);
      MOCK_PRODUCTS[id] = next;
      return next;
    });
  };

  if (loading) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Brands", href: "/brands" }, { label: "..." }]} />
        <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto bg-surface-gray space-y-8">
          <div className="animate-pulse space-y-6">
            <div className="bg-surface-container-lowest rounded-xl border border-outline-variant p-8">
              <div className="flex gap-6">
                <div className="w-24 h-24 rounded-2xl bg-surface-container" />
                <div className="space-y-3 flex-1">
                  <div className="h-8 w-64 bg-surface-container rounded-lg" />
                  <div className="h-4 w-96 bg-surface-container rounded-lg" />
                  <div className="h-4 w-48 bg-surface-container rounded-lg" />
                </div>
              </div>
            </div>
            <div className="grid grid-cols-4 gap-gutter">
              {Array.from({ length: 4 }).map((_, i) => (
                <div key={i} className="h-24 bg-surface-container-lowest rounded-xl border border-outline-variant" />
              ))}
            </div>
          </div>
        </main>
      </>
    );
  }

  if (error && !brand) {
    return (
      <>
        <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Brands", href: "/brands" }, { label: "Error" }]} />
        <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto bg-surface-gray flex items-center justify-center">
          <div className="text-center space-y-4">
            <div className="w-14 h-14 mx-auto rounded-2xl bg-error-container/30 flex items-center justify-center">
              <span className="material-symbols-outlined text-danger-red text-3xl">error_outline</span>
            </div>
            <p className="text-body-md text-danger-red font-semibold">{error}</p>
            <Link href="/brands" className="inline-block px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:opacity-90 transition-all shadow-sm">
              Back to Brands
            </Link>
          </div>
        </main>
      </>
    );
  }

  const safeBrand = brand!;
  const initials = getInitials(safeBrand.name);
  const platforms = BRAND_PLATFORMS[id] || [];
  const gradient = BRAND_COLORS[id] || "from-primary to-primary/70";

  const filteredProducts = productSearch.trim()
    ? products.filter(p => p.name.toLowerCase().includes(productSearch.toLowerCase()))
    : products;

  return (
    <>
      <style>{`
        @keyframes fade-up { from { opacity: 0; transform: translateY(16px); } to { opacity: 1; transform: translateY(0); } }
        @keyframes slide-up-row { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }
        .animate-fade-up { animation: fade-up 0.5s ease-out forwards; opacity: 0; }
        .ai-glow { box-shadow: 0 0 15px rgba(15, 98, 254, 0.15); }
      `}</style>

      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Brands", href: "/brands" }, { label: safeBrand.name }]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto bg-surface-gray space-y-8">

        {error && (
          <div className="flex items-center gap-3 rounded-xl border border-danger-red/20 bg-error-container/50 px-5 py-4 text-body-sm text-on-error-container animate-fade-up">
            <span className="material-symbols-outlined text-error text-[20px]">error</span>
            <span className="flex-1">{error}</span>
            <button onClick={() => setError(null)} className="text-on-error-container/50 hover:text-on-error-container">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>
        )}

        {/* ─── Brand Header ─── */}
        <section className={`relative overflow-hidden rounded-2xl ${visible ? "animate-fade-up" : ""}`}
          style={{ animationDelay: "0s" }}>
          <div className={`absolute inset-x-0 top-0 h-1 bg-gradient-to-r ${gradient}`} />
          <div className="bg-gradient-to-br from-surface-container to-surface-container-lowest p-8">
            <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6">
              <div className="flex items-start gap-6">
                <div className="w-24 h-24 rounded-2xl bg-surface-container flex items-center justify-center border border-outline-variant overflow-hidden p-2 shrink-0">
                  {safeBrand.logoUrl ? (
                    <img src={safeBrand.logoUrl} alt={safeBrand.name} className="w-full h-full object-contain" />
                  ) : (
                    <span className={`w-full h-full rounded-xl bg-gradient-to-br ${gradient} flex items-center justify-center`}>
                      <span className="text-headline-lg font-bold text-white">{initials}</span>
                    </span>
                  )}
                </div>
                <div>
                  <h2 className="text-headline-lg text-on-surface mb-3">{safeBrand.name}</h2>
                  {safeBrand.slogan && (
                    <p className="text-label-md text-primary italic mb-2">&ldquo;{safeBrand.slogan}&rdquo;</p>
                  )}
                  {platforms.length > 0 && (
                    <div className="flex -space-x-2 mb-3">
                      {platforms.map((p, i) => (
                        <div key={i} className="w-8 h-8 rounded-full bg-surface-container-highest border-2 border-surface-container-lowest flex items-center justify-center" title={p.label}>
                          <span className={`material-symbols-outlined text-[18px] ${p.color}`}>{p.icon}</span>
                        </div>
                      ))}
                    </div>
                  )}
                  <p className="text-on-surface-variant text-body-sm max-w-2xl">{safeBrand.description}</p>
                </div>
              </div>
              <div className="flex items-center gap-3 shrink-0">
                <button className="px-4 py-2 rounded-xl border border-outline-variant text-label-md hover:bg-surface-container transition-all active:scale-[0.97] flex items-center gap-2">
                  <span className="material-symbols-outlined text-[20px]">link</span>
                  Manage Connections
                </button>
                <button onClick={() => setActiveTab("settings")} className="px-4 py-2 rounded-xl border border-outline-variant text-label-md hover:bg-surface-container transition-all active:scale-[0.97] flex items-center gap-2">
                  <span className="material-symbols-outlined text-[20px]">edit</span>
                  Edit Brand
                </button>
                <button className="p-2 rounded-xl border border-outline-variant hover:bg-surface-container transition-all active:scale-[0.97]">
                  <span className="material-symbols-outlined">more_vert</span>
                </button>
              </div>
            </div>
          </div>
        </section>

        {/* ─── Analytics Overview ─── */}
        <section className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-gutter ${visible ? "animate-fade-up" : ""}`}
          style={{ animationDelay: "0.08s" }}>
          {[
            { icon: "inventory_2", iconBg: "bg-primary-fixed", iconColor: "text-primary", label: "Total Products", value: products.length },
            { icon: "auto_awesome_motion", iconBg: "bg-secondary-fixed", iconColor: "text-secondary", label: "Generated Content", value: Object.values(PRODUCT_ADS).reduce((a, b) => a + b, 0) },
            { icon: "trending_up", iconBg: "bg-tertiary-fixed", iconColor: "text-tertiary", label: "Total Reach", value: `${(Math.floor(Math.random() * 500) + 300)}K` },
            { icon: "favorite", iconBg: "bg-surface-container-high", iconColor: "text-on-surface", label: "Engagement Rate", value: `${(Math.random() * 5 + 1.5).toFixed(1)}%` },
          ].map((s, i) => (
            <div key={s.label} className="bg-surface-container p-5 rounded-xl border border-outline-variant/30 flex items-center gap-4"
              style={{ animation: visible ? `slide-up-row 0.4s ease-out ${0.12 + i * 0.06}s forwards` : "none", opacity: 0 }}>
              <div className={`w-12 h-12 rounded-xl ${s.iconBg} flex items-center justify-center ${s.iconColor}`}>
                <span className="material-symbols-outlined">{s.icon}</span>
              </div>
              <div>
                <p className="text-on-surface-variant text-label-sm">{s.label}</p>
                <h4 className="text-headline-sm font-bold">{s.value}</h4>
              </div>
            </div>
          ))}
        </section>

        {/* ─── Tabbed Content ─── */}
        <section className={visible ? "animate-fade-up" : ""} style={{ animationDelay: "0.16s" }}>
          <div className="border-b border-outline-variant">
            <div className="flex items-center gap-1">
              {tabs.map((tab) => (
                <button key={tab.key}
                  onClick={() => setActiveTab(tab.key as TabKey)}
                  className={`px-5 py-3 text-label-md transition-all rounded-t-xl border-b-2 ${
                    activeTab === tab.key
                      ? "border-primary text-primary"
                      : "border-transparent text-on-surface-variant hover:text-on-surface"
                  }`}>
                  {tab.label}
                </button>
              ))}
            </div>
          </div>

          <div className="mt-6 bg-surface-container-lowest rounded-xl border border-outline-variant/30 p-6" key={activeTab}>

            {/* ═══ PRODUCTS ═══ */}
            {activeTab === "products" && (
              filteredProducts.length > 0 ? (
                <div>
                  <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
                    <div className="flex items-center gap-3 w-full max-w-lg">
                      <div className="relative flex-1">
                        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-[20px]">search</span>
                        <input className="w-full bg-surface-container-low border border-outline-variant rounded-xl pl-10 pr-4 py-2 text-body-sm focus:ring-2 focus:ring-primary/20 outline-none transition-all"
                          placeholder="Filter products..." value={productSearch} onChange={e => setProductSearch(e.target.value)} />
                        </div>
                        <button className="p-2 border border-outline-variant rounded-xl hover:bg-surface-container transition-all active:scale-[0.97]">
                          <span className="material-symbols-outlined">filter_list</span>
                        </button>
                    </div>
                    <button onClick={() => setShowAddModal(true)}
                      className="bg-primary text-on-primary px-5 py-2 rounded-xl text-label-md hover:opacity-90 active:scale-[0.97] transition-all flex items-center gap-2 shadow-md ai-glow shrink-0">
                      <span className="material-symbols-outlined text-[20px]">add</span>
                      Add New Product
                    </button>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                    {filteredProducts.map((product, i) => {
                      const adsCount = PRODUCT_ADS[product.id] || 0;
                      const inStock = (product.stock ?? 0) > 0;
                      return (
                        <div key={product.id} className="group border border-outline-variant rounded-xl overflow-hidden hover:border-primary/40 hover:shadow-md transition-all"
                          style={{ animation: visible ? `fade-up 0.5s ease-out ${0.2 + i * 0.06}s forwards` : "none", opacity: 0 }}>
                          <div className="aspect-video relative overflow-hidden bg-surface-container-low">
                            <div className={`w-full h-full bg-gradient-to-br ${gradient} opacity-20 group-hover:scale-105 transition-transform duration-500`} />
                          </div>
                          <div className="p-4">
                            <h5 className="text-[16px] font-bold text-on-surface mb-1">{product.name}</h5>
                            <p className="text-on-surface-variant text-body-sm mb-2 line-clamp-2">{product.description}</p>
                            <p className="text-label-lg font-bold text-primary mb-3">${product.price.toFixed(2)}</p>
                            <div className="flex items-center justify-between">
                              <div className="flex items-center gap-3">
                                <div className="flex items-center gap-1.5 text-on-surface-variant">
                                  <span className="material-symbols-outlined text-[16px]">auto_awesome</span>
                                  <span className="text-label-sm">{adsCount} Ads</span>
                                </div>
                                <div className={`flex items-center gap-1.5 text-label-sm ${inStock ? "text-success-green" : "text-danger-red"}`}>
                                  <span className="material-symbols-outlined text-[14px]">{inStock ? "inventory" : "inventory_2"}</span>
                                  <span className="text-label-sm">{inStock ? `${product.stock} in stock` : "Out of stock"}</span>
                                </div>
                              </div>
                              <div className="flex items-center gap-1">
                                <button onClick={() => setViewingProduct(product)} className="p-1.5 rounded-full hover:bg-surface-container transition-all" title="View details">
                                  <span className="material-symbols-outlined text-[16px] text-outline/40 hover:text-primary">visibility</span>
                                </button>
                                <button onClick={() => setEditingProduct(product)} className="p-1.5 rounded-full hover:bg-surface-container transition-all" title="Edit product">
                                  <span className="material-symbols-outlined text-[16px] text-outline/40 hover:text-primary">edit</span>
                                </button>
                                <button onClick={() => setDeletingProduct(product)} className="p-1.5 rounded-full hover:bg-error-container/10 transition-all" title="Delete product">
                                  <span className="material-symbols-outlined text-[16px] text-outline/40 hover:text-danger-red">delete</span>
                                </button>
                              </div>
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center py-20 text-center gap-4">
                  <div className="w-16 h-16 rounded-2xl bg-surface-container-high flex items-center justify-center">
                    <span className="material-symbols-outlined text-outline/50 text-3xl">inventory</span>
                  </div>
                  <div>
                    <h3 className="text-headline-sm text-on-surface font-semibold">{productSearch ? "No matching products" : "No products yet"}</h3>
                    <p className="text-body-sm text-on-surface-variant mt-1 max-w-sm">
                      {productSearch ? "Try a different search term" : "Products associated with this brand will appear here"}
                    </p>
                  </div>
                  {!productSearch && (
                    <button onClick={() => setShowAddModal(true)}
                      className="bg-primary text-on-primary px-5 py-2.5 rounded-xl text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-md ai-glow flex items-center gap-2">
                      <span className="material-symbols-outlined text-[18px]">add</span>
                      Add Product
                    </button>
                  )}
                </div>
              )
            )}

            {/* ═══ CAMPAIGNS ═══ */}
            {activeTab === "campaigns" && (
              campaigns.length > 0 ? (
                <div className="overflow-x-auto">
                  <table className="w-full text-left">
                    <thead>
                      <tr className="text-label-sm text-outline border-b border-outline-variant">
                        <th className="px-5 py-3.5 font-semibold">Campaign</th>
                        <th className="px-5 py-3.5 font-semibold">Platform</th>
                        <th className="px-5 py-3.5 font-semibold">Budget</th>
                        <th className="px-5 py-3.5 font-semibold">Spent</th>
                        <th className="px-5 py-3.5 font-semibold">Status</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-outline-variant">
                      {campaigns.map((camp, i) => (
                        <tr key={camp.id} className="group hover:bg-surface-container/40 transition-colors duration-150"
                          style={{ animation: visible ? `slide-up-row 0.4s ease-out ${0.15 + i * 0.08}s forwards` : "none", opacity: 0 }}>
                          <td className="px-5 py-4">
                            <div className="flex items-center gap-3">
                              <div className="w-8 h-8 rounded-lg bg-surface-container-high flex items-center justify-center group-hover:scale-110 group-hover:bg-primary/10 transition-all duration-300">
                                <span className="material-symbols-outlined text-outline group-hover:text-primary text-[16px] transition-colors">campaign</span>
                              </div>
                              <span className="text-body-sm font-medium text-on-surface group-hover:text-primary transition-colors">{camp.name}</span>
                            </div>
                          </td>
                          <td className="px-5 py-4">
                            <span className={`px-2.5 py-1 ${camp.platformBg} ${camp.platformColor} rounded-lg text-label-xs font-bold tracking-wide inline-block`}>{camp.platform}</span>
                          </td>
                          <td className="px-5 py-4 text-body-sm text-on-surface font-medium">{camp.budget}</td>
                          <td className="px-5 py-4">
                            <span className="text-body-sm text-on-surface font-medium">{camp.spent}</span>
                            {parseFloat(camp.spent.replace(/[^0-9.]/g, "")) > 0 && (
                              <span className="text-label-sm text-outline ml-2">({Math.round(parseFloat(camp.spent.replace(/[^0-9.]/g, "")) / parseFloat(camp.budget.replace(/[^0-9.]/g, "")) * 100)}%)</span>
                            )}
                          </td>
                          <td className="px-5 py-4">
                            <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-sm font-semibold ${
                              camp.status === "Active" ? "bg-emerald-50 text-emerald-600" : "bg-surface-container-high text-on-surface-variant"
                            }`}>
                              <span className={`w-1.5 h-1.5 rounded-full ${camp.status === "Active" ? "bg-emerald-500 animate-pulse" : "bg-outline"}`} />
                              {camp.status}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center py-20 text-center gap-4">
                  <div className="w-16 h-16 rounded-2xl bg-surface-container-high flex items-center justify-center">
                    <span className="material-symbols-outlined text-outline/50 text-3xl">campaign</span>
                  </div>
                  <div>
                    <h3 className="text-headline-sm text-on-surface font-semibold">No campaigns yet</h3>
                    <p className="text-body-sm text-on-surface-variant mt-1 max-w-sm">Launch your first campaign to start tracking performance</p>
                  </div>
                </div>
              )
            )}

            {/* ═══ SETTINGS ═══ */}
            {activeTab === "settings" && (
              <div className="space-y-6 max-w-2xl">
                <div className="bg-surface-container-low rounded-xl border border-outline-variant p-6 space-y-6">
                  <div>
                    <h3 className="text-headline-sm font-semibold text-on-surface">Edit Brand</h3>
                    <p className="text-body-sm text-on-surface-variant mt-1">Update your brand&apos;s information below</p>
                  </div>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="space-y-1">
                      <label className={labelClass}>Brand Name <span className="text-danger-red">*</span></label>
                      <input className={inputClass} value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} />
                    </div>
                    <div className="space-y-1">
                      <label className={labelClass}>Slogan</label>
                      <input className={inputClass} placeholder="e.g. Innovate Your Light" value={form.slogan} onChange={e => setForm(f => ({ ...f, slogan: e.target.value }))} />
                    </div>
                    <div className="space-y-1">
                      <label className={labelClass}>Unique Selling Proposition</label>
                      <input className={inputClass} placeholder="e.g. Smart lighting that adapts" value={form.usp} onChange={e => setForm(f => ({ ...f, usp: e.target.value }))} />
                    </div>
                    <div className="space-y-1">
                      <label className={labelClass}>Target Audience</label>
                      <input className={inputClass} placeholder="e.g. Tech-savvy homeowners" value={form.targetAudience} onChange={e => setForm(f => ({ ...f, targetAudience: e.target.value }))} />
                    </div>
                  </div>
                  <div className="space-y-1">
                    <label className={labelClass}>Description</label>
                    <textarea className={`${inputClass} resize-none min-h-[80px]`} rows={3} value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} />
                  </div>
                  <div className="space-y-1">
                    <label className={labelClass}>Logo URL</label>
                    <input className={inputClass} placeholder="https://example.com/logo.png" type="url" value={form.logoUrl} onChange={e => setForm(f => ({ ...f, logoUrl: e.target.value }))} />
                  </div>
                  <div className="flex justify-end gap-3 pt-2">
                    <button onClick={() => { if (brand) { setForm({ name: brand.name, description: brand.description || "", logoUrl: brand.logoUrl || "", slogan: brand.slogan || "", usp: brand.usp || "", targetAudience: brand.targetAudience || "" }); } setError(null); }}
                      className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">Reset</button>
                    <button onClick={handleSave} disabled={saving}
                      className="px-5 py-2 rounded-xl bg-primary text-on-primary text-label-md hover:opacity-90 active:scale-[0.97] transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 shadow-md">
                      {saving ? (
                        <><svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" /></svg> Saving...</>
                      ) : "Save Changes"}
                    </button>
                  </div>
                </div>

                <div className="bg-surface-container-low rounded-xl border border-danger-red/30 p-6">
                  <div className="flex items-center gap-3 mb-4">
                    <div className="w-9 h-9 rounded-xl bg-danger-red/10 flex items-center justify-center">
                      <span className="material-symbols-outlined text-danger-red text-[18px]">warning</span>
                    </div>
                    <div>
                      <h3 className="text-headline-sm font-semibold text-on-surface">Danger Zone</h3>
                      <p className="text-body-sm text-on-surface-variant">Irreversible actions for this brand</p>
                    </div>
                  </div>
                  <button onClick={() => setShowDeleteDialog(true)}
                    className="inline-flex items-center gap-1.5 px-4 py-2 border border-danger-red/30 text-danger-red rounded-xl text-body-sm font-medium hover:bg-danger-red/5 hover:border-danger-red/50 active:scale-[0.97] transition-colors">
                    <span className="material-symbols-outlined text-[16px]">delete</span>
                    Delete Brand
                  </button>
                </div>
              </div>
            )}

          </div>
        </section>
      </main>

      <ProductModal
        key={`add-${showAddModal}`}
        open={showAddModal}
        mode="add"
        onClose={() => setShowAddModal(false)}
        onSuccess={handleAddProduct}
        brandId={id}
      />

      {editingProduct && (
        <ProductModal
          open={!!editingProduct}
          mode="edit"
          onClose={() => setEditingProduct(null)}
          onSuccess={handleEditProduct}
          brandId={id}
          product={editingProduct}
        />
      )}

      {viewingProduct && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150">
          <div className="bg-surface rounded-2xl border border-outline-variant/20 shadow-xl w-full max-w-lg mx-4 animate-in fade-in zoom-in-95 duration-200 max-h-[85vh] flex flex-col">
            <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-outline-variant/20 shrink-0">
              <h3 className="text-headline-sm text-on-surface font-bold">{viewingProduct.name}</h3>
              <button onClick={() => setViewingProduct(null)} className="text-outline hover:text-primary transition-colors active:scale-[0.97]">
                <span className="material-symbols-outlined text-[20px]">close</span>
              </button>
            </div>
            <div className="p-6 space-y-5 overflow-y-auto">
              <div className="aspect-video rounded-xl bg-surface-container-low overflow-hidden">
                <div className={`w-full h-full bg-gradient-to-br ${gradient} opacity-20`} />
              </div>
              <p className="text-body-md text-on-surface-variant leading-relaxed">{viewingProduct.description}</p>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <span className="text-label-sm font-bold text-on-surface-variant uppercase">Price</span>
                  <p className="text-body-md font-semibold text-on-surface">${viewingProduct.price.toFixed(2)}</p>
                </div>
                <div className="space-y-1">
                  <span className="text-label-sm font-bold text-on-surface-variant uppercase">Stock</span>
                  <p className={`text-body-md font-semibold flex items-center gap-1.5 ${(viewingProduct.stock ?? 0) > 0 ? "text-success-green" : "text-danger-red"}`}>
                    <span className="material-symbols-outlined text-[16px]">{((viewingProduct.stock ?? 0) > 0) ? "inventory" : "inventory_2"}</span>
                    {(viewingProduct.stock ?? 0) > 0 ? `${viewingProduct.stock} in stock` : "Out of stock"}
                  </p>
                </div>
                <div className="col-span-2 space-y-1">
                  <span className="text-label-sm font-bold text-on-surface-variant uppercase">Created</span>
                  <p className="text-body-md text-on-surface">{new Date(viewingProduct.createdAt).toLocaleDateString("en-US", { year: "numeric", month: "long", day: "numeric" })}</p>
                </div>
                <div className="col-span-2 space-y-1">
                  <span className="text-label-sm font-bold text-on-surface-variant uppercase">Ads Generated</span>
                  <p className="text-body-md font-semibold text-on-surface">{PRODUCT_ADS[viewingProduct.id] || 0}</p>
                </div>
              </div>
            </div>
            <div className="bg-surface-container-low px-6 py-4 flex items-center justify-end gap-3 rounded-b-2xl shrink-0">
              <button onClick={() => { setViewingProduct(null); setEditingProduct(viewingProduct); }} className="px-6 py-2 text-label-md font-bold text-on-surface-variant hover:bg-surface-container transition-colors rounded-xl active:scale-[0.97] flex items-center gap-1.5">
                <span className="material-symbols-outlined text-[16px]">edit</span>
                Edit
              </button>
              <button onClick={() => setViewingProduct(null)} className="px-6 py-2 bg-primary text-on-primary text-label-md font-bold rounded-xl shadow-md hover:opacity-90 transition-all active:scale-[0.97]">
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {deletingProduct && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150">
          <div className="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg p-6 w-full max-w-sm mx-4 animate-in fade-in zoom-in-95 duration-200">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-xl bg-danger-red/10 flex items-center justify-center">
                <span className="material-symbols-outlined text-danger-red text-[22px]">delete</span>
              </div>
              <div>
                <h3 className="text-headline-sm text-on-surface font-semibold">Delete Product</h3>
                <p className="text-body-sm text-on-surface-variant">This action cannot be undone</p>
              </div>
            </div>
            <p className="text-body-sm text-on-surface-variant mb-6">
              Are you sure you want to delete <span className="font-semibold text-on-surface">{deletingProduct.name}</span>?
            </p>
            <div className="flex justify-end gap-3">
              <button onClick={() => setDeletingProduct(null)} className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">Cancel</button>
              <button onClick={handleDeleteProduct} className="px-5 py-2 rounded-xl bg-danger-red text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2">Delete</button>
            </div>
          </div>
        </div>
      )}

      {showDeleteDialog && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150">
          <div className="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg p-6 w-full max-w-sm mx-4 animate-in fade-in zoom-in-95 duration-200">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-xl bg-danger-red/10 flex items-center justify-center">
                <span className="material-symbols-outlined text-danger-red text-[22px]">delete</span>
              </div>
              <div>
                <h3 className="text-headline-sm text-on-surface font-semibold">Delete Brand</h3>
                <p className="text-body-sm text-on-surface-variant">This action cannot be undone</p>
              </div>
            </div>
            <p className="text-body-sm text-on-surface-variant mb-6">
              Are you sure you want to delete <span className="font-semibold text-on-surface">{safeBrand.name}</span>? All associated products and campaigns will be permanently removed.
            </p>
            <div className="flex justify-end gap-3">
              <button onClick={() => setShowDeleteDialog(false)} className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97]">Cancel</button>
              <button onClick={handleDelete} className="px-5 py-2 rounded-xl bg-danger-red text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2">Delete</button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
