"use client";

import { useState, useEffect } from "react";
import { motion, AnimatePresence, useReducedMotion } from "motion/react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import Header from "@/components/layout/Header";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { PlatformIcon } from "@/lib/contentConstants";
import { resolveApiMediaUrl } from "@/lib/apiBaseUrl";
import { deleteBrand, getBrandById, updateBrand, type BrandPayload } from "@/services/brandService";
import { deleteProduct, fetchProducts } from "@/services/productService";
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
  workspaceId: string | null;
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
  "w-full rounded-xl border border-outline-variant/20 bg-surface-container-low px-4 py-2.5 text-body-sm text-on-surface placeholder:text-outline/40 focus:ring-2 focus:ring-primary/10 outline-none transition-all";

const labelClass = "text-label-2xs text-outline uppercase font-bold tracking-widest block";

const easeOut = [0.16, 1, 0.3, 1] as const;

export default function BrandDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const prefersReducedMotion = useReducedMotion();
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
  const [products, setProducts] = useState<Product[]>([]);
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [form, setForm] = useState({
    name: "",
    description: "",
    logoUrl: "",
    slogan: "",
    usp: "",
    targetAudience: "",
  });

  const fadeUp = prefersReducedMotion ? { initial: {}, animate: {} } : {
    initial: { opacity: 0, y: 16 },
    animate: { opacity: 1, y: 0 },
  };

  useEffect(() => {
    if (!id) { setLoading(false); return; }
    const load = async () => {
      await Promise.all([
        (async () => {
          try {
            const result = await getBrandById(id);
            if (result) {
              const b = result as Brand;
              setBrand(b);
              setForm({ name: b.name, description: b.description || "", logoUrl: b.logoUrl || "", slogan: b.slogan || "", usp: b.usp || "", targetAudience: b.targetAudience || "" });
              return;
            }
          } catch { /* ignore */ }
          setError("Brand not found");
        })(),
        (async () => {
          try {
            const result = await fetchProducts(id);
            setProducts(result as Product[]);
            return;
          } catch { /* ignore */ }
        })(),
      ]).finally(() => setLoading(false));
    };
    load();
  }, [id, activeWorkspace?.id]);

  const handleSave = async () => {
    if (!form.name.trim()) { setError("Brand name is required"); return; }
    setSaving(true);
    setError(null);

    try {
      const body: BrandPayload = { name: form.name.trim() };
      if (form.description.trim()) body.description = form.description.trim();
      if (form.logoUrl.trim()) body.logoUrl = form.logoUrl.trim();
      if (form.slogan.trim()) body.slogan = form.slogan.trim();
      if (form.usp.trim()) body.usp = form.usp.trim();
      if (form.targetAudience.trim()) body.targetAudience = form.targetAudience.trim();

      const result = await updateBrand(id, body);
      if (result) {
        setBrand(result as Brand);
      } else {
        setError("Failed to save brand");
        return;
      }
    } catch (e: any) {
      setError(e?.message || "Failed to save brand");
      return;
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setShowDeleteDialog(false);
    setError(null);
    try {
      const result = await deleteBrand(id);
      if (result) {
        router.push("/brands");
      } else {
        setError("Failed to delete brand");
      }
    } catch (e: any) {
      setError(e?.message || "Failed to delete brand");
    }
  };

  const handleAddProduct = (product: Product) => {
    setProducts((prev) => [product, ...prev]);
  };

  const handleEditProduct = (updated: Product) => {
    setProducts((prev) => prev.map((p) => (p.id === updated.id ? updated : p)));
  };

  const handleDeleteProduct = async () => {
    if (!deletingProduct) return;
    const target = deletingProduct;
    setDeletingProduct(null);
    try {
      const deleted = await deleteProduct(target.id);
      if (!deleted) {
        setError("Failed to delete product");
        return;
      }
      setProducts((prev) => prev.filter((p) => p.id !== target.id));
    } catch (err: any) {
      setError(err?.message || "Failed to delete product");
    }
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
            <p className="text-body-sm text-danger-red font-semibold">{error}</p>
            <Link href="/brands" className="inline-block px-5 py-2.5 bg-primary text-on-primary rounded-xl text-body-sm font-semibold hover:opacity-90 transition-all shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40">
              Back to Brands
            </Link>
          </div>
        </main>
      </>
    );
  }

  const safeBrand = brand!;
  const initials = getInitials(safeBrand.name);
  const platforms: { icon: string; label: string; color: string }[] = [];
  const gradient = "from-primary/80 to-primary/40";

  const filteredProducts = productSearch.trim()
    ? products.filter(p => p.name.toLowerCase().includes(productSearch.toLowerCase()))
    : products;

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Brands", href: "/brands" }, { label: safeBrand.name }]} />
      <main className="ml-0 p-8 h-[calc(100vh-64px)] overflow-y-auto bg-surface-gray space-y-8">

        {error && (
          <motion.div {...fadeUp} transition={{ duration: 0.4, ease: easeOut }}
            className="flex items-center gap-3 rounded-xl border border-danger-red/20 bg-error-container/50 px-5 py-4 text-body-sm text-on-error-container">
            <span className="material-symbols-outlined text-error text-[20px]">error</span>
            <span className="flex-1">{error}</span>
            <button onClick={() => setError(null)} className="text-on-error-container/50 hover:text-on-error-container focus-visible:outline-none">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </motion.div>
        )}

        {/* ─── Brand Header ─── */}
        <motion.section {...fadeUp} transition={{ duration: 0.6, ease: easeOut }}
          className="relative overflow-hidden rounded-2xl">
          <div className={`absolute inset-x-0 top-0 h-1 bg-gradient-to-r ${gradient}`} />
          <div className="bg-gradient-to-br from-surface-container to-surface-container-lowest p-8">
            <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6">
              <div className="flex items-start gap-6">
                <div className="w-24 h-24 rounded-2xl bg-surface-container flex items-center justify-center border border-outline-variant overflow-hidden p-2 shrink-0">
                  {resolveApiMediaUrl(safeBrand.logoUrl) ? (
                    <img src={resolveApiMediaUrl(safeBrand.logoUrl)} alt={safeBrand.name} className="w-full h-full object-contain" />
                  ) : (
                    <span className={`w-full h-full rounded-xl bg-gradient-to-br ${gradient} flex items-center justify-center`}>
                      <span className="text-headline-lg font-bold text-white">{initials}</span>
                    </span>
                  )}
                </div>
                <div>
                  <h2 className="text-headline-sm font-bold text-on-surface mb-3">{safeBrand.name}</h2>
                  {safeBrand.slogan && (
                    <p className="text-label-md text-primary italic mb-2">&ldquo;{safeBrand.slogan}&rdquo;</p>
                  )}
                  {platforms.length > 0 && (
                    <div className="flex -space-x-2 mb-3">
                      {platforms.map((p, i) => (
                        <div key={i} className="w-8 h-8 rounded-full bg-surface-container-highest border-2 border-surface-container-lowest flex items-center justify-center" title={p.label}>
                          {["facebook", "instagram", "tiktok"].includes(p.icon) ? (
                            <PlatformIcon platform={p.icon} className="w-[18px] h-[18px]" />
                          ) : (
                            <span className={`material-symbols-outlined text-[18px] ${p.color}`}>{p.icon}</span>
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                  <p className="text-on-surface-variant text-body-sm max-w-2xl">{safeBrand.description}</p>
                </div>
              </div>
              <div className="flex items-center gap-3 shrink-0">
                <button className="px-4 py-2 rounded-xl border border-outline-variant/20 text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30 flex items-center gap-2">
                  <span className="material-symbols-outlined text-[18px]">link</span>
                  Manage Connections
                </button>
                <button onClick={() => setActiveTab("settings")} className="px-4 py-2 rounded-xl bg-primary text-on-primary text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40 transition-all flex items-center gap-2">
                  <span className="material-symbols-outlined text-[18px]">edit</span>
                  Edit Brand
                </button>
                <button className="p-2 rounded-xl border border-outline-variant/20 text-outline hover:text-on-surface hover:bg-surface-container transition-all active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30">
                  <span className="material-symbols-outlined text-[20px]">more_vert</span>
                </button>
              </div>
            </div>
          </div>
        </motion.section>

        {/* ─── Analytics Overview ─── */}
        <motion.section {...fadeUp} transition={{ duration: 0.5, delay: 0.08, ease: easeOut }}
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-gutter">
          {[
            { icon: "inventory_2", iconBg: "bg-gradient-to-br from-primary/20 to-primary/5", iconColor: "text-primary", label: "Total Products", value: products.length },
            { icon: "auto_awesome_motion", iconBg: "bg-gradient-to-br from-secondary/20 to-secondary/5", iconColor: "text-secondary", label: "Generated Content", value: 0 },
            { icon: "trending_up", iconBg: "bg-gradient-to-br from-tertiary/20 to-tertiary/5", iconColor: "text-tertiary", label: "Total Reach", value: "--" },
            { icon: "favorite", iconBg: "bg-gradient-to-br from-surface/20 to-surface/5", iconColor: "text-on-surface", label: "Engagement Rate", value: "--" },
          ].map((s, i) => (
            <motion.div key={s.label}
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, delay: 0.12 + i * 0.06, ease: easeOut }}
              whileHover={{ y: -4, boxShadow: "0 16px 48px rgba(0,0,0,0.08)" }}
              className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-6 flex items-center gap-4 transition-all duration-300">
              <div className={`w-11 h-11 rounded-xl ${s.iconBg} flex items-center justify-center ${s.iconColor} shrink-0`}>
                <span className="material-symbols-outlined text-[22px]">{s.icon}</span>
              </div>
              <div>
                <p className="text-label-sm text-on-surface-variant font-medium">{s.label}</p>
                <h4 className="text-kpi-lg text-on-surface leading-tight">{s.value}</h4>
              </div>
            </motion.div>
          ))}
        </motion.section>

        {/* ─── Tabbed Content ─── */}
        <motion.section {...fadeUp} transition={{ duration: 0.5, delay: 0.16, ease: easeOut }}>
          <div className="border-b border-outline-variant/20">
            <div className="flex items-center gap-1">
              {tabs.map((tab) => (
                <button key={tab.key}
                  onClick={() => setActiveTab(tab.key as TabKey)}
                  className={`relative px-5 py-3 text-label-sm font-semibold transition-all rounded-t-xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30 focus-visible:ring-inset active:scale-[0.97] ${
                    activeTab === tab.key
                      ? "text-primary"
                      : "text-outline hover:text-on-surface hover:bg-surface-container/50"
                  }`}>
                  {tab.label}
                  {activeTab === tab.key && (
                    <motion.div
                      layoutId="tab-underline"
                      className="absolute bottom-0 left-0 right-0 h-0.5 bg-primary rounded-full"
                      transition={{ type: "spring", stiffness: 300, damping: 30 }}
                    />
                  )}
                </button>
              ))}
            </div>
          </div>

          <AnimatePresence mode="wait">
            <motion.div
              key={activeTab}
              initial={{ opacity: 0, y: 8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -8 }}
              transition={{ duration: 0.2, ease: easeOut }}
              className="mt-6 bg-surface-container-lowest rounded-xl border border-outline-variant/30 p-6">

            {/* ═══ PRODUCTS ═══ */}
            {activeTab === "products" && (
              filteredProducts.length > 0 ? (
                <div>
                  <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
                    <div className="flex items-center gap-3 w-full max-w-lg">
                      <div className="relative flex-1">
                        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-[20px]">search</span>
                        <input className="w-full bg-surface-container-low border border-outline-variant rounded-xl pl-10 pr-4 py-2 text-body-sm focus:border-primary/50 focus:ring-2 focus:ring-primary/10 outline-none transition-all"
                          placeholder="Filter products..." value={productSearch} onChange={e => setProductSearch(e.target.value)} />
                        </div>
                        <button className="p-2 border border-outline-variant rounded-xl hover:bg-surface-container transition-all active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30">
                          <span className="material-symbols-outlined">filter_list</span>
                        </button>
                    </div>
                    <button onClick={() => setShowAddModal(true)}
                      className="bg-primary text-on-primary px-5 py-2 rounded-xl text-label-md hover:opacity-90 active:scale-[0.97] transition-all flex items-center gap-2 shadow-md shrink-0 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40">
                      <span className="material-symbols-outlined text-[20px]">add</span>
                      Add New Product
                    </button>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                    {filteredProducts.map((product, i) => {
                      const inStock = (product.stock ?? 0) > 0;
                      const imageUrl = resolveApiMediaUrl(product.images?.[0]);
                      return (
                        <motion.div key={product.id}
                          initial={{ opacity: 0, y: 16 }}
                          whileInView={{ opacity: 1, y: 0 }}
                          viewport={{ once: true, amount: 0.3 }}
                          transition={{ duration: 0.5, delay: i * 0.06, ease: easeOut }}
                          whileHover={{ y: -3, boxShadow: "0 10px 25px -12px rgba(0,0,0,0.15)" }}
                          className="group border border-outline-variant/20 bg-surface-container-lowest rounded-2xl overflow-hidden hover:border-primary/40 hover:shadow-[0_16px_48px_rgba(0,0,0,0.08)] transition-all duration-300 flex flex-col">
                          <div className="aspect-video relative overflow-hidden bg-surface-container-low">
                            {imageUrl ? (
                              <img src={imageUrl} alt={product.name} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
                            ) : (
                              <div className={`w-full h-full bg-gradient-to-br ${gradient} opacity-20 group-hover:scale-105 transition-transform duration-500`} />
                            )}
                          </div>
                          <div className="p-4 flex flex-col flex-1">
                            <h5 className="text-[16px] font-bold text-on-surface mb-1">{product.name}</h5>
                            <p className="text-on-surface-variant text-body-sm mb-2 line-clamp-2 flex-1">{product.description}</p>
                            <p className="text-label-lg font-bold text-primary mb-3">${(product.price ?? 0).toFixed(2)}</p>
                            <div className="flex items-center justify-between mt-auto">
                              <div className="flex items-center gap-3">
                                <div className="flex items-center gap-1.5 text-on-surface-variant">
                                  <span className="material-symbols-outlined text-[16px]">auto_awesome</span>
                                  <span className="text-label-sm">0 Ads</span>
                                </div>
                                <div className={`flex items-center gap-1.5 text-label-sm ${inStock ? "text-success-green" : "text-danger-red"}`}>
                                  <span className="material-symbols-outlined text-[14px]">{inStock ? "inventory" : "inventory_2"}</span>
                                  <span className="text-label-sm">{inStock ? `${product.stock} in stock` : "Out of stock"}</span>
                                </div>
                              </div>
                              <div className="flex items-center gap-1">
                                <button onClick={() => setViewingProduct(product)} className="p-1.5 rounded-full hover:bg-surface-container transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30" title="View details">
                                  <span className="material-symbols-outlined text-[16px] text-outline/40 hover:text-primary">visibility</span>
                                </button>
                                <button onClick={() => setEditingProduct(product)} className="p-1.5 rounded-full hover:bg-surface-container transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30" title="Edit product">
                                  <span className="material-symbols-outlined text-[16px] text-outline/40 hover:text-primary">edit</span>
                                </button>
                                <button onClick={() => setDeletingProduct(product)} className="p-1.5 rounded-full hover:bg-error-container/10 transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-danger-red/30" title="Delete product">
                                  <span className="material-symbols-outlined text-[16px] text-outline/40 hover:text-danger-red">delete</span>
                                </button>
                              </div>
                            </div>
                          </div>
                        </motion.div>
                      );
                    })}
                  </div>
                </div>
              ) : (
                <motion.div {...fadeUp} transition={{ duration: 0.4, ease: easeOut }}
                  className="flex flex-col items-center justify-center py-20 text-center gap-4">
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
                      className="bg-primary text-on-primary px-5 py-2.5 rounded-xl text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-md flex items-center gap-2 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40">
                      <span className="material-symbols-outlined text-[18px]">add</span>
                      Add Product
                    </button>
                  )}
                </motion.div>
              )
            )}

            {/* ═══ CAMPAIGNS ═══ */}
            {activeTab === "campaigns" && (
              campaigns.length > 0 ? (
                <div className="overflow-x-auto">
                  <table className="w-full text-left min-w-[600px]">
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
                        <motion.tr key={camp.id}
                          initial={{ opacity: 0, y: 12 }}
                          whileInView={{ opacity: 1, y: 0 }}
                          viewport={{ once: true, amount: 0.3 }}
                          transition={{ duration: 0.5, delay: i * 0.08, ease: easeOut }}
                          className="group hover:bg-surface-container/40 transition-colors duration-150">
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
                              camp.status === "Active" ? "bg-success-green/10 text-success-green" : "bg-surface-container-high text-on-surface-variant"
                            }`}>
                              <span className={`w-1.5 h-1.5 rounded-full ${camp.status === "Active" ? "bg-success-green animate-pulse" : "bg-outline"}`} />
                              {camp.status}
                            </span>
                          </td>
                        </motion.tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <motion.div {...fadeUp} transition={{ duration: 0.4, ease: easeOut }}
                  className="flex flex-col items-center justify-center py-20 text-center gap-4">
                  <div className="w-16 h-16 rounded-2xl bg-surface-container-high flex items-center justify-center">
                    <span className="material-symbols-outlined text-outline/50 text-3xl">campaign</span>
                  </div>
                  <div>
                    <h3 className="text-headline-sm text-on-surface font-semibold">No campaigns yet</h3>
                    <p className="text-body-sm text-on-surface-variant mt-1 max-w-sm">Launch your first campaign to start tracking performance</p>
                  </div>
                </motion.div>
              )
            )}

            {/* ═══ SETTINGS ═══ */}
            {activeTab === "settings" && (
              <div className="space-y-6 max-w-2xl">
                <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 p-6 shadow-sm space-y-6">
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
                      className="px-5 py-2 rounded-xl border border-outline-variant/20 text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-outline">Reset</button>
                    <button onClick={handleSave} disabled={saving}
                      className="px-5 py-2 rounded-xl bg-primary text-on-primary text-label-sm font-bold hover:scale-105 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 flex items-center gap-2 shadow-lg shadow-primary/20 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40">
                      {saving ? (
                        <><svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" /></svg> Saving...</>
                      ) : "Save Changes"}
                    </button>
                  </div>
                </div>

                <div className="bg-surface-container-lowest rounded-2xl border border-danger-red/20 p-6 shadow-sm">
                  <div className="flex items-center gap-3 mb-4">
                    <div className="w-10 h-10 rounded-xl bg-danger-red/10 flex items-center justify-center">
                      <span className="material-symbols-outlined text-danger-red text-[20px]">warning</span>
                    </div>
                    <div>
                      <h3 className="text-headline-sm font-semibold text-on-surface">Danger Zone</h3>
                      <p className="text-body-sm text-on-surface-variant">Irreversible actions for this brand</p>
                    </div>
                  </div>
                  <button onClick={() => setShowDeleteDialog(true)}
                    className="inline-flex items-center gap-1.5 px-4 py-2 bg-danger-red/10 text-danger-red rounded-xl text-label-sm font-semibold hover:bg-danger-red/20 active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-danger-red/30 transition-all">
                    <span className="material-symbols-outlined text-[16px]">delete</span>
                    Delete Brand
                  </button>
                </div>
              </div>
            )}
            </motion.div>
          </AnimatePresence>
        </motion.section>
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
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.15 }}
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: 8 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: 8 }}
            transition={{ duration: 0.2, ease: easeOut }}
            className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-xl w-full max-w-lg mx-4 max-h-[85vh] flex flex-col">
            <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-outline-variant/20 shrink-0">
              <h3 className="text-headline-sm text-on-surface font-bold">{viewingProduct.name}</h3>
              <button onClick={() => setViewingProduct(null)} className="text-outline hover:text-primary transition-colors active:scale-[0.97] focus-visible:outline-none">
                <span className="material-symbols-outlined text-[20px]">close</span>
              </button>
            </div>
            <div className="p-6 space-y-5 overflow-y-auto">
              <div className="aspect-video rounded-xl bg-surface-container-low overflow-hidden">
                {resolveApiMediaUrl(viewingProduct.images?.[0]) ? (
                  <img src={resolveApiMediaUrl(viewingProduct.images?.[0])} alt={viewingProduct.name} className="w-full h-full object-cover" />
                ) : (
                  <div className={`w-full h-full bg-gradient-to-br ${gradient} opacity-20`} />
                )}
              </div>
              <p className="text-body-sm text-on-surface-variant leading-relaxed">{viewingProduct.description}</p>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <span className="text-label-sm font-bold text-on-surface-variant uppercase">Price</span>
                  <p className="text-body-sm font-semibold text-on-surface">${(viewingProduct.price ?? 0).toFixed(2)}</p>
                </div>
                <div className="space-y-1">
                  <span className="text-label-sm font-bold text-on-surface-variant uppercase">Stock</span>
                  <p className={`text-body-sm font-semibold flex items-center gap-1.5 ${(viewingProduct.stock ?? 0) > 0 ? "text-success-green" : "text-danger-red"}`}>
                    <span className="material-symbols-outlined text-[16px]">{((viewingProduct.stock ?? 0) > 0) ? "inventory" : "inventory_2"}</span>
                    {(viewingProduct.stock ?? 0) > 0 ? `${viewingProduct.stock} in stock` : "Out of stock"}
                  </p>
                </div>
                <div className="col-span-2 space-y-1">
                  <span className="text-label-sm font-bold text-on-surface-variant uppercase">Created</span>
                  <p className="text-body-sm text-on-surface">{new Date(viewingProduct.createdAt).toLocaleDateString("en-US", { year: "numeric", month: "long", day: "numeric" })}</p>
                </div>
                <div className="col-span-2 space-y-1">
                  <span className="text-label-sm font-bold text-on-surface-variant uppercase">Ads Generated</span>
                  <p className="text-body-sm font-semibold text-on-surface">0</p>
                </div>
              </div>
            </div>
            <div className="bg-surface-container-lowest px-6 py-4 flex items-center justify-end gap-3 rounded-b-2xl shrink-0 border-t border-outline-variant/20">
              <button onClick={() => { setViewingProduct(null); setEditingProduct(viewingProduct); }} className="px-6 py-2 text-label-md font-bold text-on-surface-variant hover:bg-surface-container transition-colors rounded-xl active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30 flex items-center gap-1.5">
                <span className="material-symbols-outlined text-[16px]">edit</span>
                Edit
              </button>
              <button onClick={() => setViewingProduct(null)} className="px-6 py-2 bg-primary text-on-primary text-label-md font-bold rounded-xl shadow-md hover:opacity-90 transition-all active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40">
                Close
              </button>
            </div>
            </motion.div>
          </motion.div>
        )}

      {deletingProduct && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.15 }}
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: 8 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: 8 }}
            transition={{ duration: 0.2, ease: easeOut }}
            className="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg p-6 w-full max-w-sm mx-4">
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
              <button onClick={() => setDeletingProduct(null)} className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-outline">Cancel</button>
              <button onClick={handleDeleteProduct} className="px-5 py-2 rounded-xl bg-danger-red text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-danger-red/50">Delete</button>
            </div>
            </motion.div>
          </motion.div>
        )}

      {showDeleteDialog && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.15 }}
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
          <motion.div
            initial={{ opacity: 0, scale: 0.95, y: 8 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: 8 }}
            transition={{ duration: 0.2, ease: easeOut }}
            className="bg-surface-container-lowest rounded-xl border border-outline-variant shadow-lg p-6 w-full max-w-sm mx-4">
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
              <button onClick={() => setShowDeleteDialog(false)} className="px-5 py-2 rounded-xl border border-outline-variant text-label-md text-on-surface-variant hover:bg-surface-container transition-all active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-outline">Cancel</button>
              <button onClick={handleDelete} className="px-5 py-2 rounded-xl bg-danger-red text-white text-label-md hover:opacity-90 active:scale-[0.97] transition-all shadow-sm flex items-center gap-2 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-danger-red/50">Delete</button>
            </div>
            </motion.div>
          </motion.div>
        )}
    </>
  );
}
