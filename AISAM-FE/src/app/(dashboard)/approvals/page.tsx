"use client";

import { useMemo, useState } from "react";
import Header from "@/components/layout/Header";

type ApprovalStatus = "Pending" | "Approved" | "Rejected" | "Revision Requested";
type Urgency = "Urgent" | "Medium" | "Low";

type ApprovalItem = {
  id: string;
  title: string;
  type: string;
  brand: string;
  campaign: string;
  requester: string;
  requesterInitials: string;
  requesterTone: string;
  urgency: Urgency;
  status: ApprovalStatus;
  locked?: boolean;
  previewTone: string;
  headline: string;
  primaryText: string;
};

const approvals: ApprovalItem[] = [
  {
    id: "cloud-x-performance-hero",
    title: "Cloud-X Performance Hero",
    type: "Image + Copy Ad",
    brand: "Lumina Tech",
    campaign: "Q4 Tech Refresh Campaign",
    requester: "AISAM Studio",
    requesterInitials: "AI",
    requesterTone: "bg-primary-container text-on-primary",
    urgency: "Urgent",
    status: "Pending",
    previewTone: "from-blue-500 to-cyan-300",
    headline: "Scale Faster with Cloud-X Intelligence",
    primaryText: "Unlock the next era of infrastructure with AISAM-optimized targeting. Efficiency redefined for modern dev teams.",
  },
  {
    id: "winter-elegance-promo",
    title: "Winter Elegance Promo",
    type: "Instagram Reel Content",
    brand: "Velvet & Oak",
    campaign: "Seasonal Trends 2024",
    requester: "Sarah Adams",
    requesterInitials: "SA",
    requesterTone: "bg-secondary-fixed text-secondary",
    urgency: "Medium",
    status: "Pending",
    locked: true,
    previewTone: "from-violet-500 to-rose-300",
    headline: "Winter Elegance Starts Here",
    primaryText: "A quiet seasonal reel concept focused on texture, craft, and premium styling for the winter collection.",
  },
  {
    id: "summit-retargeting-carousel",
    title: "Summit Retargeting Carousel",
    type: "Carousel Ad",
    brand: "Summit Outdoor",
    campaign: "Trail Ready Launch",
    requester: "Marcus Lee",
    requesterInitials: "ML",
    requesterTone: "bg-tertiary-fixed text-tertiary",
    urgency: "Low",
    status: "Approved",
    previewTone: "from-emerald-500 to-lime-300",
    headline: "Built for the Long Route",
    primaryText: "Retarget high-intent shoppers with rugged product proof, material details, and trail-tested positioning.",
  },
  {
    id: "pulse-finance-static",
    title: "Pulse Finance Static",
    type: "LinkedIn Sponsored Post",
    brand: "Pulse Finance",
    campaign: "Portfolio Intelligence",
    requester: "Nina Park",
    requesterInitials: "NP",
    requesterTone: "bg-surface-container-highest text-on-surface",
    urgency: "Medium",
    status: "Rejected",
    previewTone: "from-slate-600 to-sky-300",
    headline: "Sharper Allocation Decisions",
    primaryText: "A financial services static asset that needs compliance review before returning to production.",
  },
];

const tabs: { label: string; value: "All" | ApprovalStatus; count: number }[] = [
  { label: "All", value: "All", count: 102 },
  { label: "Pending", value: "Pending", count: 12 },
  { label: "Approved", value: "Approved", count: 84 },
  { label: "Rejected", value: "Rejected", count: 6 },
  { label: "Revision Requested", value: "Revision Requested", count: 0 },
];

const urgencyStyles = {
  Urgent: "bg-error-container text-on-error-container",
  Medium: "bg-surface-container-high text-on-surface-variant",
  Low: "bg-primary-fixed text-primary",
};

const statusStyles = {
  Pending: "bg-warning-amber/20 text-on-surface",
  Approved: "bg-success-green/10 text-success-green",
  Rejected: "bg-error-container text-on-error-container",
  "Revision Requested": "bg-secondary-fixed text-secondary",
};

function ApprovalPreview({ item, large = false }: { item: ApprovalItem; large?: boolean }) {
  return (
    <div className={`relative overflow-hidden rounded-xl bg-gradient-to-br ${item.previewTone} ${large ? "aspect-[16/10]" : "h-12 w-16"} shadow-sm`}>
      <div className="absolute inset-0 bg-[linear-gradient(135deg,rgba(255,255,255,.35),transparent_45%,rgba(0,0,0,.16))]" />
      <div className="absolute bottom-1.5 left-1.5 right-1.5 h-1 rounded-full bg-white/50" />
      <div className={`${large ? "h-16 w-16" : "h-5 w-5"} absolute right-2 top-2 rounded-full bg-white/25`} />
    </div>
  );
}

function Toast({ message, type }: { message: string; type: "success" | "error" }) {
  if (!message) return null;

  return (
    <div className="fixed bottom-8 left-1/2 z-[80] flex -translate-x-1/2 items-center gap-4 rounded-2xl bg-inverse-surface px-5 py-4 text-inverse-on-surface shadow-2xl">
      <div className={`flex h-8 w-8 items-center justify-center rounded-full ${type === "error" ? "bg-error" : "bg-success-green"}`}>
        <span className="material-symbols-outlined text-[18px] text-white">{type === "error" ? "close" : "check"}</span>
      </div>
      <div>
        <p className="text-body-sm font-bold">{message}</p>
        <p className="text-label-sm opacity-70">The queue has been updated.</p>
      </div>
    </div>
  );
}

export default function ApprovalsPage() {
  const [activeTab, setActiveTab] = useState<"All" | ApprovalStatus>("All");
  const [brand, setBrand] = useState("All");
  const [priority, setPriority] = useState("All");
  const [layout, setLayout] = useState<"list" | "grid">("list");
  const [selected, setSelected] = useState<ApprovalItem | null>(null);
  const [rejectOpen, setRejectOpen] = useState(false);
  const [toast, setToast] = useState<{ message: string; type: "success" | "error" }>({ message: "", type: "success" });

  const showToast = (message: string, type: "success" | "error" = "success") => {
    setToast({ message, type });
    window.setTimeout(() => setToast({ message: "", type }), 2600);
  };

  const filtered = useMemo(() => {
    return approvals.filter((item) => {
      const tabMatch = activeTab === "All" || item.status === activeTab;
      const brandMatch = brand === "All" || item.brand === brand;
      const priorityMatch = priority === "All" || item.urgency === priority;
      return tabMatch && brandMatch && priorityMatch;
    });
  }, [activeTab, brand, priority]);

  const selectedItem = selected ?? approvals[0];

  return (
    <>
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "Approvals" }]} />
      <main className="h-[calc(100vh-64px)] overflow-y-auto p-4 sm:p-6 lg:p-8">
        <div className="mx-auto flex w-full max-w-7xl flex-col gap-6">
          <section className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <h1 className="text-headline-md font-bold text-on-surface sm:text-headline-lg">Content Approvals</h1>
              <p className="mt-1 text-body-md text-on-surface-variant">Review and manage AI-generated marketing assets across your portfolio.</p>
            </div>
            <div className="flex items-center gap-3">
              <div className="flex -space-x-2">
                {["JD", "AM", "+3"].map((initials, index) => (
                  <span key={initials} className={`flex h-8 w-8 items-center justify-center rounded-full border-2 border-white text-[10px] font-bold ${index === 0 ? "bg-blue-100 text-blue-700" : index === 1 ? "bg-purple-100 text-purple-700" : "bg-surface-container-high text-on-surface"}`}>
                    {initials}
                  </span>
                ))}
              </div>
              <button className="text-label-md font-bold text-primary transition-colors hover:text-primary-container">Team Settings</button>
            </div>
          </section>

          <section className="grid grid-cols-1 gap-4 md:grid-cols-4">
            {[
              { label: "Total Queue", value: "102", icon: "fact_check", color: "bg-primary-fixed text-primary" },
              { label: "Pending Review", value: "12", icon: "pending_actions", color: "bg-warning-amber/20 text-on-surface" },
              { label: "Approved", value: "84", icon: "verified", color: "bg-success-green/10 text-success-green" },
              { label: "Rejected", value: "6", icon: "block", color: "bg-error-container text-on-error-container" },
            ].map((stat) => (
              <div key={stat.label} className="rounded-2xl border border-outline-variant/25 bg-surface-container-lowest p-5 shadow-sm">
                <div className="flex items-center gap-4">
                  <div className={`flex h-11 w-11 items-center justify-center rounded-xl ${stat.color}`}>
                    <span className="material-symbols-outlined text-[22px]">{stat.icon}</span>
                  </div>
                  <div>
                    <p className="text-label-md text-on-surface-variant">{stat.label}</p>
                    <p className="text-headline-md font-bold text-on-surface">{stat.value}</p>
                  </div>
                </div>
              </div>
            ))}
          </section>

          <section className="overflow-x-auto border-b border-outline-variant/40">
            <div className="flex min-w-max gap-7">
              {tabs.map((tab) => {
                const active = activeTab === tab.value;
                return (
                  <button
                    key={tab.label}
                    className={`flex items-center gap-2 border-b-2 pb-4 text-body-sm font-bold transition-all ${active ? "border-primary text-primary" : "border-transparent text-outline hover:text-on-surface"}`}
                    onClick={() => setActiveTab(tab.value)}
                  >
                    {tab.label} ({tab.count})
                    {tab.value === "Pending" && <span className="h-2 w-2 rounded-full bg-warning-amber" />}
                  </button>
                );
              })}
            </div>
          </section>

          <section className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div className="flex flex-wrap items-center gap-3">
              <label className="relative">
                <select className="appearance-none rounded-xl border border-outline-variant/40 bg-surface-container-lowest py-2 pl-4 pr-10 text-body-sm outline-none focus:border-primary/60 focus:ring-2 focus:ring-primary/10" onChange={(event) => setBrand(event.target.value)} value={brand}>
                  <option value="All">Brand: All</option>
                  <option value="Lumina Tech">Brand: Lumina Tech</option>
                  <option value="Velvet & Oak">Brand: Velvet & Oak</option>
                  <option value="Summit Outdoor">Brand: Summit Outdoor</option>
                  <option value="Pulse Finance">Brand: Pulse Finance</option>
                </select>
                <span className="material-symbols-outlined pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-[18px] text-outline">expand_more</span>
              </label>
              <label className="relative">
                <select className="appearance-none rounded-xl border border-outline-variant/40 bg-surface-container-lowest py-2 pl-4 pr-10 text-body-sm outline-none focus:border-primary/60 focus:ring-2 focus:ring-primary/10" onChange={(event) => setPriority(event.target.value)} value={priority}>
                  <option value="All">Priority: All</option>
                  <option value="Urgent">Urgent</option>
                  <option value="Medium">Medium</option>
                  <option value="Low">Low</option>
                </select>
                <span className="material-symbols-outlined pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-[18px] text-outline">expand_more</span>
              </label>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-label-md text-outline">View:</span>
              <button className={`flex h-9 w-9 items-center justify-center rounded-lg transition-all ${layout === "grid" ? "bg-surface-container-high text-primary" : "text-outline hover:bg-surface-container"}`} onClick={() => setLayout("grid")} title="Grid view">
                <span className="material-symbols-outlined text-[18px]">grid_view</span>
              </button>
              <button className={`flex h-9 w-9 items-center justify-center rounded-lg transition-all ${layout === "list" ? "bg-surface-container-high text-primary" : "text-outline hover:bg-surface-container"}`} onClick={() => setLayout("list")} title="List view">
                <span className="material-symbols-outlined text-[18px]">view_list</span>
              </button>
            </div>
          </section>

          {layout === "list" ? (
            <section className="overflow-hidden rounded-2xl border border-outline-variant/25 bg-surface-container-lowest shadow-sm">
              <div className="overflow-x-auto">
                <table className="w-full min-w-[900px] text-left">
                  <thead className="bg-surface-container-low text-label-md uppercase tracking-wider text-outline">
                    <tr>
                      <th className="px-6 py-4 font-bold">Content Preview</th>
                      <th className="px-6 py-4 font-bold">Brand & Context</th>
                      <th className="px-6 py-4 font-bold">Requester</th>
                      <th className="px-6 py-4 font-bold">Urgency</th>
                      <th className="px-6 py-4 font-bold">Status</th>
                      <th className="px-6 py-4 text-right font-bold">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-outline-variant/30">
                    {filtered.map((item) => (
                      <tr key={item.id} className="group cursor-pointer transition-colors hover:bg-surface-container-low/50" onClick={() => { setRejectOpen(false); setSelected(item); }}>
                        <td className="px-6 py-4">
                          <div className="flex items-center gap-4">
                            <ApprovalPreview item={item} />
                            <div>
                              <p className="text-body-sm font-bold text-on-surface">{item.title}</p>
                              <p className="text-label-sm text-outline">{item.type}</p>
                            </div>
                          </div>
                        </td>
                        <td className="px-6 py-4">
                          <p className="text-body-sm font-bold text-primary">{item.brand}</p>
                          <p className="text-label-sm text-outline">{item.campaign}</p>
                        </td>
                        <td className="px-6 py-4">
                          <div className="flex items-center gap-2">
                            <span className={`flex h-7 w-7 items-center justify-center rounded-full text-[10px] font-bold ${item.requesterTone}`}>{item.requesterInitials}</span>
                            <span className="text-body-sm text-on-surface">{item.requester}</span>
                          </div>
                        </td>
                        <td className="px-6 py-4">
                          <span className={`inline-flex items-center gap-1 rounded-lg px-2 py-1 text-[10px] font-bold uppercase ${urgencyStyles[item.urgency]}`}>
                            {item.urgency === "Urgent" && <span className="material-symbols-outlined text-[13px]">priority_high</span>}
                            {item.urgency}
                          </span>
                        </td>
                        <td className="px-6 py-4">
                          <span className={`inline-flex rounded-lg px-2 py-1 text-[10px] font-bold uppercase ${statusStyles[item.status]}`}>{item.status}</span>
                        </td>
                        <td className="px-6 py-4 text-right">
                          {item.locked ? (
                            <span className="inline-flex cursor-not-allowed items-center justify-end gap-1 text-label-sm font-semibold text-outline opacity-60">
                              <span className="material-symbols-outlined text-[16px]">lock</span>
                              Leader Only
                            </span>
                          ) : (
                            <div className="flex items-center justify-end gap-1 opacity-100 transition-opacity sm:opacity-0 sm:group-hover:opacity-100">
                              <button className="flex h-9 w-9 items-center justify-center rounded-lg text-success-green transition-all hover:bg-success-green/10" onClick={(event) => { event.stopPropagation(); showToast("Asset Approved"); }} title="Quick approve">
                                <span className="material-symbols-outlined text-[20px]">check_circle</span>
                              </button>
                              <button className="flex h-9 w-9 items-center justify-center rounded-lg text-secondary transition-all hover:bg-secondary/10" onClick={(event) => { event.stopPropagation(); setSelected(item); }} title="Request changes">
                                <span className="material-symbols-outlined text-[20px]">edit_note</span>
                              </button>
                              <button className="flex h-9 w-9 items-center justify-center rounded-lg text-danger-red transition-all hover:bg-danger-red/10" onClick={(event) => { event.stopPropagation(); showToast("Asset Rejected", "error"); }} title="Reject">
                                <span className="material-symbols-outlined text-[20px]">cancel</span>
                              </button>
                            </div>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {filtered.length === 0 && (
                <div className="flex flex-col items-center justify-center px-6 py-20 text-center">
                  <div className="mb-4 flex h-20 w-20 items-center justify-center rounded-full bg-surface-container">
                    <span className="material-symbols-outlined text-[40px] text-outline">done_all</span>
                  </div>
                  <h2 className="text-headline-sm font-bold text-on-surface">Zero Pending Approvals</h2>
                  <p className="mt-2 text-body-sm text-outline">Your queue is empty.</p>
                </div>
              )}
            </section>
          ) : (
            <section className="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-3">
              {filtered.map((item) => (
                <button key={item.id} className="overflow-hidden rounded-2xl border border-outline-variant/25 bg-surface-container-lowest text-left shadow-sm transition-all hover:-translate-y-1 hover:shadow-lg" onClick={() => { setRejectOpen(false); setSelected(item); }}>
                  <ApprovalPreview item={item} large />
                  <div className="p-5">
                    <div className="mb-3 flex items-start justify-between gap-3">
                      <div>
                        <h2 className="text-headline-sm font-bold text-on-surface">{item.title}</h2>
                        <p className="text-body-sm text-on-surface-variant">{item.type}</p>
                      </div>
                      <span className={`shrink-0 rounded-lg px-2 py-1 text-[10px] font-bold uppercase ${urgencyStyles[item.urgency]}`}>{item.urgency}</span>
                    </div>
                    <div className="flex items-center justify-between border-t border-outline-variant/20 pt-4">
                      <div>
                        <p className="text-body-sm font-bold text-primary">{item.brand}</p>
                        <p className="text-label-sm text-outline">{item.requester}</p>
                      </div>
                      <span className={`rounded-lg px-2 py-1 text-[10px] font-bold uppercase ${statusStyles[item.status]}`}>{item.status}</span>
                    </div>
                  </div>
                </button>
              ))}
            </section>
          )}

          <div className="flex justify-center">
            <button className="rounded-full border border-outline-variant/40 px-6 py-2.5 text-label-md font-bold text-on-surface transition-all hover:bg-surface-container-low">
              Load More History
            </button>
          </div>
        </div>
      </main>

      {selected && (
        <>
          <button aria-label="Close approval drawer" className="fixed inset-0 z-[60] bg-on-background/30 backdrop-blur-sm" onClick={() => setSelected(null)} />
          <aside aria-labelledby="approval-drawer-title" aria-modal="true" className="fixed right-0 top-0 z-[70] flex h-screen w-full lg:w-[66.666vw] flex-col border-l border-outline-variant bg-surface-container-lowest shadow-2xl" role="dialog">
            <div className="sticky top-0 z-10 flex items-center justify-between border-b border-outline-variant bg-surface-container-lowest px-6 py-4">
              <div className="flex items-center gap-3">
                <span className="material-symbols-outlined text-[26px] text-primary">fact_check</span>
                <div>
                  <h2 className="text-headline-sm font-bold text-on-surface" id="approval-drawer-title">Asset Approval Required</h2>
                  <p className="text-label-sm uppercase tracking-wider text-on-surface-variant">Campaign: {selectedItem.campaign}</p>
                </div>
              </div>
              <button className="flex h-9 w-9 items-center justify-center rounded-full text-outline transition-all hover:bg-surface-container hover:text-on-surface" onClick={() => setSelected(null)} title="Close">
                <span className="material-symbols-outlined text-[20px]">close</span>
              </button>
            </div>

            <div className="flex flex-1 flex-col gap-6 overflow-y-auto bg-surface-bright p-6 lg:flex-row">
              <div className="flex w-full flex-col gap-6 lg:w-3/5">
                <section className="overflow-hidden rounded-2xl border border-outline-variant bg-surface-container-lowest shadow-sm">
                  <div className="relative">
                    <ApprovalPreview item={selectedItem} large />
                    <div className="absolute left-4 top-4 inline-flex items-center gap-1 rounded-full border border-outline-variant bg-surface-container-lowest/90 px-3 py-1.5 text-label-sm font-bold text-on-surface shadow-sm backdrop-blur-md">
                      <span className="material-symbols-outlined text-[16px] text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>auto_awesome</span>
                      AI Generated
                    </div>
                  </div>
                  <div className="border-t border-outline-variant bg-surface-container-lowest p-5">
                    <div className="mb-2 flex items-center justify-between">
                      <span className="text-label-md font-bold text-outline">Primary Text</span>
                      <button className="flex h-8 w-8 items-center justify-center rounded-lg text-outline transition-all hover:bg-surface-container hover:text-primary" title="Copy text">
                        <span className="material-symbols-outlined text-[18px]">content_copy</span>
                      </button>
                    </div>
                    <p className="text-body-md text-on-surface">{selectedItem.primaryText}</p>
                  </div>
                </section>

                <section className="overflow-hidden rounded-2xl border border-outline-variant bg-surface-container-lowest shadow-sm">
                  <div className="flex items-center gap-2 border-b border-outline-variant bg-surface-container-low p-4">
                    <span className="material-symbols-outlined text-[20px] text-secondary">psychology</span>
                    <h3 className="text-headline-sm text-[16px] font-bold text-on-surface">Generation Context</h3>
                  </div>
                  <div className="space-y-5 p-5">
                    <div>
                      <h4 className="mb-2 text-label-md font-bold text-on-surface-variant">Brand Context Applied</h4>
                      <div className="flex flex-wrap gap-2">
                        <span className="inline-flex items-center gap-1 rounded-lg bg-primary-fixed px-2.5 py-1 text-label-sm font-bold text-primary">
                          <span className="material-symbols-outlined text-[14px]">palette</span>
                          Tone: Professional
                        </span>
                        <span className="inline-flex items-center gap-1 rounded-lg bg-secondary-fixed px-2.5 py-1 text-label-sm font-bold text-secondary">
                          <span className="material-symbols-outlined text-[14px]">sell</span>
                          Audience: B2B Executive
                        </span>
                      </div>
                    </div>
                    <div>
                      <h4 className="mb-2 text-label-md font-bold text-on-surface-variant">Product Rules Enforced</h4>
                      <ul className="space-y-2">
                        {[
                          `Included exact brand name "${selectedItem.brand}"`,
                          "Maintained clean product-first visual composition",
                        ].map((rule) => (
                          <li key={rule} className="flex items-start gap-2">
                            <span className="material-symbols-outlined mt-0.5 text-[18px] text-success-green">check_circle</span>
                            <span className="text-body-sm text-on-surface">{rule}</span>
                          </li>
                        ))}
                      </ul>
                    </div>
                  </div>
                </section>
              </div>

              <div className="flex w-full flex-col gap-6 border-t border-outline-variant pt-6 lg:w-2/5 lg:border-l lg:border-t-0 lg:pl-6 lg:pt-0">
                <div className="flex gap-3 rounded-xl border border-primary-fixed bg-surface-container-low p-4">
                  <span className="material-symbols-outlined text-primary">info</span>
                  <p className="text-body-sm text-on-surface">Approval is required before this asset can be scheduled in a Campaign.</p>
                </div>

                <section className="rounded-2xl border border-outline-variant bg-surface-container-lowest p-5 shadow-sm">
                  <h3 className="mb-4 text-label-md font-bold uppercase tracking-wide text-on-surface-variant">Review Actions</h3>
                  <button className="inline-flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-4 py-3 text-label-md text-on-primary shadow-sm transition-all hover:bg-surface-tint" onClick={() => { showToast("Approved for Publishing"); setSelected(null); }}>
                    <span className="material-symbols-outlined text-[18px]">thumb_up</span>
                    Approve for Publishing
                  </button>
                  <div className="mt-3 grid grid-cols-2 gap-2">
                    <button className="inline-flex items-center justify-center gap-2 rounded-xl border border-outline px-3 py-2.5 text-label-md text-on-surface transition-all hover:bg-surface-container">
                      <span className="material-symbols-outlined text-[18px]">edit_note</span>
                      Request Changes
                    </button>
                    <button className="inline-flex items-center justify-center gap-2 rounded-xl border border-error/50 px-3 py-2.5 text-label-md text-error transition-all hover:bg-error-container" onClick={() => setRejectOpen(true)}>
                      <span className="material-symbols-outlined text-[18px]">thumb_down</span>
                      Reject
                    </button>
                  </div>

                  {rejectOpen && (
                    <div className="mt-4 flex flex-col gap-2 border-t border-outline-variant pt-4">
                      <label className="flex items-center gap-1 text-label-sm font-bold text-error" htmlFor="approval-feedback">
                        <span className="material-symbols-outlined text-[14px]">warning</span>
                        Required: Feedback for AI revision
                      </label>
                      <textarea
                        className="min-h-24 resize-none rounded-xl border border-outline-variant bg-surface px-3 py-2 text-body-sm text-on-surface outline-none placeholder:text-outline focus:border-error focus:ring-2 focus:ring-error/10"
                        id="approval-feedback"
                        placeholder="Explain why this asset was rejected..."
                      />
                      <div className="mt-1 flex justify-end gap-2">
                        <button className="rounded-lg px-3 py-1.5 text-label-sm text-on-surface-variant transition-all hover:bg-surface-container" onClick={() => setRejectOpen(false)}>
                          Cancel
                        </button>
                        <button className="rounded-lg bg-error px-3 py-1.5 text-label-sm text-on-error shadow-sm transition-all hover:bg-danger-red" onClick={() => { showToast("Rejection Submitted", "error"); setSelected(null); }}>
                          Submit Rejection
                        </button>
                      </div>
                    </div>
                  )}
                </section>

                <section className="flex-1">
                  <h3 className="mb-4 text-label-md font-bold uppercase tracking-wide text-on-surface-variant">Audit Trail</h3>
                  <div className="relative ml-2 space-y-6 border-l-2 border-outline-variant pl-4">
                    {[
                      { dot: "bg-primary", title: "Pending Review", meta: "Assigned to: Jane Doe (Manager)", time: "Just now", icon: "schedule" },
                      { dot: "bg-surface-container-highest", title: "Asset Generated", meta: "By: AISAM Studio Model v4.2", time: "Oct 24, 2023 - 10:15 AM", icon: "history" },
                      { dot: "bg-surface-container-highest", title: "Prompt Submitted", meta: `By: ${selectedItem.requester}`, time: "Oct 24, 2023 - 10:12 AM", icon: "history" },
                    ].map((entry) => (
                      <div key={entry.title} className="relative">
                        <span className={`absolute -left-[23px] top-1 h-3 w-3 rounded-full border-2 border-surface-container-lowest ${entry.dot}`} />
                        <p className="text-label-md font-bold text-on-surface">{entry.title}</p>
                        <p className="mt-0.5 text-body-sm text-outline">{entry.meta}</p>
                        <p className="mt-1 flex items-center gap-1 text-label-sm text-outline">
                          <span className="material-symbols-outlined text-[14px]">{entry.icon}</span>
                          {entry.time}
                        </p>
                      </div>
                    ))}
                  </div>
                </section>
              </div>
            </div>
          </aside>
        </>
      )}

      <Toast message={toast.message} type={toast.type} />
    </>
  );
}
