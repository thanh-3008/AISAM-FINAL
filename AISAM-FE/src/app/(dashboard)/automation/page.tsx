"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Header from "@/components/layout/Header";
import {
  confirmAutomationPlan,
  cancelAutomationPlan,
  approveAutomationPlan,
  fetchAutomationPlan,
  fetchAutomationPlans,
  importAutomationCsv,
  retryAutomationPlan,
  rejectAutomationItem,
  cloneAutomationPlan,
  fetchAutomationPerformance,
  importAutomationGoogleSheet,
  setAutomationAutoApprove,
  updateAutomationItem,
  fetchAutomationTargets,
  approveAutomationTargets,
  type AutomationPlan,
  type AutomationItem,
  type AutomationPerformance,
  type AutomationTarget,
} from "@/services/automationService";
import { fetchBrands, fetchProducts } from "@/services/brandService";

const statusStyle: Record<string, string> = {
  AwaitingConfirmation: "bg-amber-500/10 text-amber-700",
  Generating: "bg-blue-500/10 text-blue-700",
  AwaitingApproval: "bg-purple-500/10 text-purple-700",
  Completed: "bg-emerald-500/10 text-emerald-700",
  Failed: "bg-red-500/10 text-red-700",
  PartiallyFailed: "bg-orange-500/10 text-orange-700",
};

const timezoneOptions = [
  { value: "Asia/Ho_Chi_Minh", label: "Vietnam", offset: "UTC+7" },
  { value: "Asia/Singapore", label: "Singapore", offset: "UTC+8" },
  { value: "Asia/Tokyo", label: "Japan", offset: "UTC+9" },
  { value: "UTC", label: "UTC", offset: "UTC+0" },
];

function timezoneLabel(value: string) {
  if (value === "Asia/Bangkok" || value === "Asia/Ho_Chi_Minh") return "Vietnam (UTC+7)";
  const option = timezoneOptions.find((entry) => entry.value === value);
  return option ? `${option.label} (${option.offset})` : value;
}

function formatDate(value: string) {
  return new Date(value).toLocaleString("en-US", { dateStyle: "short", timeStyle: "short" });
}

export default function AutomationPage() {
  const [plans, setPlans] = useState<AutomationPlan[]>([]);
  const [selected, setSelected] = useState<AutomationPlan | null>(null);
  const [loading, setLoading] = useState(true);
  const [showImport, setShowImport] = useState(false);
  const [name, setName] = useState("");
  const [timezone, setTimezone] = useState("Asia/Ho_Chi_Minh");
  const [file, setFile] = useState<File | null>(null);
  const [working, setWorking] = useState(false);
  const [message, setMessage] = useState("");
  const [performance, setPerformance] = useState<AutomationPerformance | null>(null);
  const [editing, setEditing] = useState<AutomationItem | null>(null);
  const [editForm, setEditForm] = useState({ brandId: "", productId: "", topic: "", platform: "facebook", contentType: "Text", objective: "", tone: "", cta: "", notes: "", scheduledAt: "" });
  const [brands, setBrands] = useState<{ id: string; name: string }[]>([]);
  const [products, setProducts] = useState<{ id: string; name: string; brandId: string }[]>([]);
  const [targetItemId, setTargetItemId] = useState<string | null>(null);
  const [availableTargets, setAvailableTargets] = useState<AutomationTarget[]>([]);
  const [selectedTargetIds, setSelectedTargetIds] = useState<string[]>([]);

  const load = useCallback(async () => {
    setLoading(true);
    try { setPlans(await fetchAutomationPlans()); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to load automation plans."); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  useEffect(() => {
    if (!selected || selected.status !== "Generating") return;
    const timer = window.setInterval(async () => {
      try {
        const refreshed = await fetchAutomationPlan(selected.id);
        setSelected(refreshed);
        if (refreshed.status !== "Generating") await load();
      } catch { /* retry on the next polling tick */ }
    }, 3000);
    return () => window.clearInterval(timer);
  }, [selected?.id, selected?.status, load]);

  const summary = useMemo(() => ({
    total: plans.length,
    awaiting: plans.filter((plan) => plan.status === "AwaitingConfirmation").length,
    running: plans.filter((plan) => ["Generating", "Scheduling"].includes(plan.status)).length,
    credits: plans.reduce((sum, plan) => sum + plan.estimatedCredits, 0),
  }), [plans]);

  const openPlan = async (id: string) => {
    setWorking(true);
    try {
      const [plan, report] = await Promise.all([fetchAutomationPlan(id), fetchAutomationPerformance(id)]);
      setSelected(plan); setPerformance(report);
    }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to load plan."); }
    finally { setWorking(false); }
  };

  const handleImport = async () => {
    if (!name.trim() || !file) { setMessage("Enter a plan name and select a CSV file."); return; }
    setWorking(true); setMessage("");
    try {
      const plan = await importAutomationCsv(name.trim(), timezone, file);
      setShowImport(false); setName(""); setFile(null); setSelected(plan);
      await load();
    } catch (error) { setMessage(error instanceof Error ? error.message : "Import failed."); }
    finally { setWorking(false); }
  };

  const handleConfirm = async () => {
    if (!selected) return;
    setWorking(true);
    try { const plan = await confirmAutomationPlan(selected.id); setSelected(plan); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Confirmation failed."); }
    finally { setWorking(false); }
  };

  const handleRetry = async (itemId?: string) => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try { setSelected(await retryAutomationPlan(selected.id, itemId)); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to retry."); }
    finally { setWorking(false); }
  };

  const handleCancel = async () => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try { setSelected(await cancelAutomationPlan(selected.id)); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to cancel plan."); }
    finally { setWorking(false); }
  };

  const handleApprove = async (itemId?: string) => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try {
      if (itemId) {
        const targets = await fetchAutomationTargets(selected.id, itemId);
        if (targets.length === 0) throw new Error("This brand has no active page linked for the selected platform.");
        if (targets.length > 1) {
          setTargetItemId(itemId); setAvailableTargets(targets); setSelectedTargetIds(targets.filter((target) => target.isScheduled).map((target) => target.integrationId));
          return;
        }
        setSelected(await approveAutomationTargets(selected.id, itemId, [targets[0].integrationId]));
      } else setSelected(await approveAutomationPlan(selected.id));
      await load();
    }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to approve and schedule."); }
    finally { setWorking(false); }
  };

  const handleApproveTargets = async () => {
    if (!selected || !targetItemId || selectedTargetIds.length === 0) { setMessage("Select at least one page to publish to."); return; }
    setWorking(true); setMessage("");
    try { setSelected(await approveAutomationTargets(selected.id, targetItemId, selectedTargetIds)); setTargetItemId(null); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to schedule posts for the selected pages."); }
    finally { setWorking(false); }
  };

  const handleReject = async (itemId: string) => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try { setSelected(await rejectAutomationItem(selected.id, itemId, "Rejected from Automation Plan")); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to reject content."); }
    finally { setWorking(false); }
  };

  const handleGoogleSheet = async () => {
    const url = window.prompt("Paste the Google Sheet URL with link sharing enabled:");
    if (!url) return;
    const planName = window.prompt("Plan name:", "Google Sheets plan");
    if (!planName) return;
    setWorking(true); setMessage("");
    try { const plan = await importAutomationGoogleSheet(planName, timezone, url); setSelected(plan); setPerformance(null); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to import Google Sheet."); }
    finally { setWorking(false); }
  };

  const handleClone = async () => {
    if (!selected) return;
    const planName = window.prompt("New plan name:", `${selected.name} - next version`);
    if (!planName) return;
    const shift = Number(window.prompt("Shift schedule by how many days?", "7"));
    if (!Number.isInteger(shift) || shift < 1) { setMessage("Days must be an integer greater than 0."); return; }
    setWorking(true); setMessage("");
    try { const plan = await cloneAutomationPlan(selected.id, planName, shift); setSelected(plan); setPerformance(null); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to create plan from template."); }
    finally { setWorking(false); }
  };

  const handleAutoApprove = async () => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try { setSelected(await setAutomationAutoApprove(selected.id, !selected.autoApprove)); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Unable to change auto-approve setting."); }
    finally { setWorking(false); }
  };

  const openItemEditor = async (item: AutomationItem) => {
    const localDate = new Date(item.scheduledAt);
    const localValue = new Date(localDate.getTime() - localDate.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
    setEditing(item);
    setEditForm({ brandId: item.brandId, productId: item.productId || "", topic: item.topic, platform: item.platform, contentType: item.contentType, objective: item.objective || "", tone: item.tone || "", cta: item.cta || "", notes: item.notes || "", scheduledAt: localValue });
    const [brandData, productData] = await Promise.all([fetchBrands(), fetchProducts(item.brandId)]);
    setBrands(brandData); setProducts(productData);
  };

  const handleEditBrand = async (brandId: string) => {
    setEditForm((value) => ({ ...value, brandId, productId: "" }));
    setProducts(await fetchProducts(brandId));
  };

  const handleSaveItem = async () => {
    if (!selected || !editing) return;
    if (!editForm.brandId || !editForm.topic.trim() || !editForm.scheduledAt) { setMessage("Brand, topic, date and time are required."); return; }
    setWorking(true); setMessage("");
    try {
      const plan = await updateAutomationItem(selected.id, editing.id, { ...editForm, productId: editForm.productId || undefined, scheduledAt: new Date(editForm.scheduledAt).toISOString() });
      setSelected(plan); setEditing(null); await load();
    } catch (error) { setMessage(error instanceof Error ? error.message : "Unable to edit request."); }
    finally { setWorking(false); }
  };

  const downloadTemplate = () => {
    const csv = [
      "Brand,Product,Topic,Objective,Platforms,ContentType,Tone,CTA,Notes,Date,Time",
      'Brand Name,Product Name,"New product introduction",Awareness,"Facebook|Instagram",Image,Youthful,"View now","Use brand colors",10/08/2026,09:00',
      'Brand Name,Product Name,"Tutorial video",Engagement,"Facebook|Instagram|TikTok",Video,Friendly,"Follow now","Vertical video",12/08/2026,19:30',
    ].join("\n");
    const url = URL.createObjectURL(new Blob(["\uFEFF", csv], { type: "text/csv;charset=utf-8" }));
    const anchor = document.createElement("a"); anchor.href = url; anchor.download = "aisam-automation-template.csv"; anchor.click(); URL.revokeObjectURL(url);
  };

  return (
    <div className="min-h-screen bg-background">
      <Header breadcrumbs={[{ label: "Dashboard", href: "/dashboard" }, { label: "AI Automation" }]} />
      <main className="p-6 md:p-8 max-w-[1500px] mx-auto space-y-6">
        <section className="flex flex-col md:flex-row md:items-center gap-4">
          <div>
            <p className="text-label-sm text-primary font-bold tracking-widest uppercase">AI Campaign Autopilot</p>
            <h1 className="text-headline-sm text-on-surface font-bold">Content Automation Plans</h1>
            <p className="text-[11px] text-outline mt-1">Import schedules, validate inputs, and manage all posts in one place.</p>
          </div>
          <div className="md:ml-auto flex gap-2">
            <button onClick={downloadTemplate} className="px-4 py-2.5 rounded-xl border border-outline-variant/30 text-label-sm font-semibold text-on-surface hover:bg-surface-container">Download CSV Template</button>
            <button onClick={handleGoogleSheet} disabled={working} className="px-4 py-2.5 rounded-xl border border-outline-variant/30 text-label-sm font-semibold text-on-surface hover:bg-surface-container disabled:opacity-50">Google Sheets</button>
            <button onClick={() => setShowImport(true)} className="px-5 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold shadow-lg shadow-primary/20 flex items-center gap-2">
              <span className="material-symbols-outlined text-[18px]">upload_file</span> Import Plan
            </button>
          </div>
        </section>

        {message && <div className="px-4 py-3 rounded-xl bg-amber-500/10 text-amber-700 text-body-sm">{message}</div>}

        <section className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          {[
            ["Total Plans", summary.total, "view_timeline"], ["Awaiting Confirmation", summary.awaiting, "pending_actions"],
            ["Processing", summary.running, "progress_activity"], ["Est. Credits", summary.credits, "toll"],
          ].map(([label, value, icon]) => (
            <div key={String(label)} className="p-4 rounded-2xl bg-surface-container-lowest border border-outline-variant/20">
              <span className="material-symbols-outlined text-primary">{icon}</span>
              <p className="text-lg font-bold text-on-surface mt-2">{value}</p><p className="text-label-xs text-outline">{label}</p>
            </div>
          ))}
        </section>

        <section className="rounded-2xl bg-surface-container-lowest border border-outline-variant/20 overflow-hidden">
          <div className="px-5 py-4 border-b border-outline-variant/15"><h2 className="text-headline-sm font-bold text-on-surface">Plans</h2></div>
          {loading ? <div className="p-10 text-center text-outline">Loading...</div> : plans.length === 0 ? (
            <div className="p-14 text-center"><span className="material-symbols-outlined text-5xl text-outline/40">auto_awesome_motion</span><h2 className="text-headline-sm font-bold mt-3">No automation plans yet</h2><p className="text-body-sm text-outline">Download the CSV template and import your first plan.</p></div>
          ) : <div className="divide-y divide-outline-variant/10">{plans.map((plan) => (
            <button key={plan.id} onClick={() => openPlan(plan.id)} className="w-full px-5 py-4 text-left hover:bg-surface-container-low transition flex items-center gap-4">
              <div className="w-11 h-11 rounded-xl bg-primary/10 text-primary flex items-center justify-center"><span className="material-symbols-outlined">calendar_view_week</span></div>
              <div className="min-w-0 flex-1"><p className="text-body-sm font-bold text-on-surface truncate">{plan.name}</p><p className="text-[11px] text-outline">{plan.totalItems} platform items · {formatDate(plan.createdAt)}</p></div>
              <span className={`hidden sm:inline px-2.5 py-1 rounded-full text-label-2xs font-bold ${statusStyle[plan.status] || "bg-surface-container text-outline"}`}>{plan.status}</span>
              <div className="text-right"><p className="text-[11px] font-bold text-on-surface">{plan.validItems}/{plan.totalItems}</p><p className="text-[11px] text-outline">valid</p></div>
              <span className="material-symbols-outlined text-outline">chevron_right</span>
            </button>
          ))}</div>}
        </section>
      </main>

      {showImport && <div className="fixed inset-0 z-[80] flex items-center justify-center p-4"><div className="absolute inset-0 bg-black/50" onClick={() => setShowImport(false)} /><div className="relative w-full max-w-lg bg-surface-container-lowest rounded-2xl p-6 shadow-2xl space-y-4">
        <div><h2 className="text-headline-sm font-bold">Import Automation Plan</h2><p className="text-body-sm text-outline">CSV is validated first — no AI calls or credit deductions yet.</p></div>
        <label className="block"><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Plan Name</span><input value={name} onChange={(event) => setName(event.target.value)} className="mt-1.5 w-full px-4 py-3 rounded-xl bg-surface-container border border-outline-variant/20 outline-none" placeholder="August Campaign" /></label>
        <fieldset><legend className="text-label-2xs text-outline uppercase font-bold tracking-widest mb-2">Timezone</legend><div className="grid grid-cols-2 gap-2">{timezoneOptions.map((option) => <label key={option.value} className={`p-3 rounded-xl border cursor-pointer flex items-center gap-2 ${timezone === option.value ? "border-primary bg-primary/10 text-primary" : "border-outline-variant/20 bg-surface-container"}`}><input type="radio" name="automation-timezone" value={option.value} checked={timezone === option.value} onChange={(event) => setTimezone(event.target.value)} className="accent-primary" /><span><span className="block text-label-xs font-bold">{option.label}</span><span className="block text-[11px] opacity-70">{option.offset}</span></span></label>)}</div></fieldset>
        <label className="block p-6 rounded-xl border-2 border-dashed border-outline-variant/30 text-center cursor-pointer hover:border-primary/50"><span className="material-symbols-outlined text-3xl text-primary">csv</span><p className="font-semibold">{file?.name || "Select CSV file"}</p><input type="file" accept=".csv,text/csv" className="hidden" onChange={(event) => setFile(event.target.files?.[0] ?? null)} /></label>
        <div className="flex gap-3"><button onClick={() => setShowImport(false)} className="flex-1 py-2.5 rounded-xl border border-outline-variant/30 text-label-sm font-semibold">Cancel</button><button onClick={handleImport} disabled={working || !file || !name.trim()} className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold disabled:opacity-50">{working ? "Importing..." : "Import & Validate"}</button></div>
      </div></div>}

      {selected && <div className="fixed inset-0 z-[80] flex justify-end"><div className="absolute inset-0 bg-black/40" onClick={() => setSelected(null)} /><aside className="relative w-full max-w-3xl h-full bg-background shadow-2xl overflow-y-auto">
        <div className="sticky top-0 bg-background/95 backdrop-blur border-b border-outline-variant/20 p-5 flex items-center gap-3 z-10"><button onClick={() => setSelected(null)} className="p-2 rounded-lg hover:bg-surface-container"><span className="material-symbols-outlined">close</span></button><div className="flex-1"><h2 className="text-headline-sm font-bold">{selected.name}</h2><p className="text-[11px] text-outline">{selected.sourceFileName || "Manual plan"} · {timezoneLabel(selected.timezone)}</p></div>{selected.status === "AwaitingConfirmation" && <button onClick={handleConfirm} disabled={working || selected.validItems === 0} className="px-4 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold disabled:opacity-50">Confirm Plan</button>}{selected.status === "Generating" && <button onClick={handleCancel} disabled={working} className="px-4 py-2.5 rounded-xl border border-red-500/30 text-red-600 text-label-sm font-bold disabled:opacity-50">Cancel</button>}{selected.items.some((item) => item.status === "AwaitingApproval") && <button onClick={() => handleApprove()} disabled={working} className="px-4 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold disabled:opacity-50">Approve & Schedule</button>}{["Failed", "PartiallyFailed", "AwaitingApproval"].includes(selected.status) && selected.items.some((item) => item.status === "GenerationFailed" && item.validationErrors.length === 0) && <button onClick={() => handleRetry()} disabled={working} className="px-4 py-2.5 rounded-xl border border-primary/30 text-primary text-label-sm font-bold disabled:opacity-50">Retry Failed</button>}</div>
        <div className="p-5 space-y-4"><div className="grid grid-cols-3 gap-3"><div className="p-3 rounded-xl bg-emerald-500/10"><p className="text-lg font-bold text-emerald-700">{selected.validItems}</p><p className="text-label-xs">Valid</p></div><div className="p-3 rounded-xl bg-red-500/10"><p className="text-lg font-bold text-red-700">{selected.failedItems}</p><p className="text-label-xs">Needs Fix</p></div><div className="p-3 rounded-xl bg-primary/10"><p className="text-lg font-bold text-primary">{selected.estimatedCredits}</p><p className="text-label-xs">Est. Credits</p></div></div>
          <div className="p-4 rounded-xl bg-surface-container-lowest border border-outline-variant/20 flex flex-wrap items-center gap-3"><div className="flex-1 min-w-48"><p className="text-body-sm font-bold text-on-surface">Advanced Operations</p><p className="text-[11px] text-outline">{selected.templateSourcePlanId ? "Created from template" : "Can be reused as template"} · Auto-approve {selected.autoApprove ? "on" : "off"}</p></div><button onClick={handleClone} disabled={working} className="px-3 py-2 rounded-lg border border-outline-variant/30 text-[11px] font-semibold">Use as Template</button>{["AwaitingConfirmation", "Generating"].includes(selected.status) && <button onClick={handleAutoApprove} disabled={working} className={`px-3 py-2 rounded-lg text-[11px] font-semibold ${selected.autoApprove ? "bg-amber-500/15 text-amber-700" : "bg-primary/10 text-primary"}`}>{selected.autoApprove ? "Disable Auto-approve" : "Enable Auto-approve"}</button>}</div>
          {performance && <div className="grid grid-cols-2 md:grid-cols-4 gap-2">{[["Published", performance.publishedItems], ["Impressions", performance.impressions], ["Engagement", performance.engagement], ["Avg CTR", `${(performance.averageCtr * 100).toFixed(2)}%`]].map(([label, value]) => <div key={String(label)} className="p-3 rounded-xl bg-surface-container"><p className="text-lg font-bold text-on-surface">{value}</p><p className="text-label-xs text-outline">{label}</p></div>)}</div>}
          {selected.items.map((item) => <article key={item.id} className="p-4 rounded-xl bg-surface-container-lowest border border-outline-variant/20"><div className="flex items-start gap-3"><span className="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center text-label-xs font-bold">{item.rowIndex}</span><div className="flex-1 min-w-0"><div className="flex flex-wrap gap-2 items-center"><h3 className="text-body-sm font-bold text-on-surface">{item.topic || "Untitled"}</h3><span className="px-2 py-0.5 rounded-full bg-surface-container text-[9px] uppercase font-bold">{item.platform}</span><span className="text-[11px] text-outline">{item.contentType}</span></div><p className="text-[11px] text-outline mt-1">{item.brandName || "Unknown brand"} · {formatDate(item.scheduledAt)} · {item.usedCredits}/{item.estimatedCredits} credits</p>{item.generatedImageUrl && <img src={item.generatedImageUrl} alt="Generated automation asset" className="mt-3 w-28 h-28 rounded-xl object-cover" />}{item.generatedVideoUrl && <video src={item.generatedVideoUrl} controls className="mt-3 w-full max-w-xs rounded-xl bg-black" />}{item.generatedText && <p className="mt-3 text-body-sm text-on-surface-variant whitespace-pre-line line-clamp-4">{item.generatedText}</p>}{item.videoProvider && <p className="mt-1 text-[11px] text-outline">Video provider: {item.videoProvider}</p>}{item.validationErrors.length > 0 && <ul className="mt-2 space-y-1">{item.validationErrors.map((error) => <li key={error} className="text-[11px] text-red-600 flex gap-1"><span>•</span>{error}</li>)}</ul>}{selected.status === "AwaitingConfirmation" && <button onClick={() => openItemEditor(item)} className="mt-3 px-3 py-1.5 rounded-lg border border-primary/30 text-primary text-[11px] font-semibold flex items-center gap-1"><span className="material-symbols-outlined text-[16px]">edit</span>Edit Request</button>}{item.lastError && <div className="mt-2 flex items-center gap-2"><p className="text-[11px] text-red-600 flex-1">{item.lastError}</p>{item.status === "GenerationFailed" && item.validationErrors.length === 0 && <button onClick={() => handleRetry(item.id)} disabled={working} className="text-[11px] font-semibold text-primary">Retry</button>}</div>}{item.status === "NeedsAttention" && item.contentId && item.lastError?.startsWith("No active ") && <button onClick={() => handleApprove(item.id)} disabled={working} className="mt-3 px-3 py-1.5 rounded-lg bg-primary text-on-primary text-[11px] font-semibold">Reschedule</button>}{item.status === "AwaitingApproval" && <div className="mt-3 flex gap-2"><button onClick={() => handleApprove(item.id)} disabled={working} className="px-3 py-1.5 rounded-lg bg-primary text-on-primary text-[11px] font-semibold">Approve & Schedule</button><button onClick={() => handleReject(item.id)} disabled={working} className="px-3 py-1.5 rounded-lg border border-red-500/30 text-red-600 text-[11px] font-semibold">Reject</button></div>}{item.contentCalendarId && <p className="mt-2 text-[11px] text-emerald-700">Scheduled · {formatDate(item.scheduledAt)}</p>}</div><span className={`px-2 py-1 rounded-lg text-label-2xs font-bold ${["Pending", "AwaitingApproval", "Scheduled"].includes(item.status) ? "bg-emerald-500/10 text-emerald-700" : item.status.startsWith("Generating") ? "bg-blue-500/10 text-blue-700" : "bg-red-500/10 text-red-700"}`}>{item.status}</span></div></article>)}
        </div>
      </aside></div>}

      {targetItemId && <div className="fixed inset-0 z-[100] flex items-center justify-center p-4"><div className="absolute inset-0 bg-black/60" onClick={() => setTargetItemId(null)} /><div className="relative w-full max-w-lg bg-surface-container-lowest rounded-2xl p-6 shadow-2xl space-y-4">
        <div><h2 className="text-headline-sm font-bold">Select Pages to Publish</h2><p className="text-body-sm text-outline mt-1">This brand has multiple linked pages. You can select one or more pages.</p></div>
        <div className="space-y-2">{availableTargets.map((target) => <label key={target.integrationId} className={`p-4 rounded-xl border flex items-center gap-3 cursor-pointer ${selectedTargetIds.includes(target.integrationId) ? "border-primary bg-primary/10" : "border-outline-variant/20 bg-surface-container"}`}><input type="checkbox" checked={selectedTargetIds.includes(target.integrationId)} onChange={(event) => setSelectedTargetIds((ids) => event.target.checked ? [...ids, target.integrationId] : ids.filter((id) => id !== target.integrationId))} className="w-5 h-5 accent-primary" /><span className="flex-1"><span className="block font-bold text-on-surface">{target.name}</span><span className="block text-[11px] text-outline">{target.externalId || target.integrationId}{target.isScheduled ? " · Already scheduled" : ""}</span></span></label>)}</div>
        <div className="flex gap-3"><button onClick={() => setTargetItemId(null)} className="flex-1 py-2.5 rounded-xl border border-outline-variant/30 text-label-sm font-semibold">Cancel</button><button onClick={handleApproveTargets} disabled={working || selectedTargetIds.length === 0} className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold disabled:opacity-50">{working ? "Scheduling..." : `Publish to ${selectedTargetIds.length} page(s)`}</button></div>
      </div></div>}

      {editing && <div className="fixed inset-0 z-[100] flex items-center justify-center p-4"><div className="absolute inset-0 bg-black/60" onClick={() => setEditing(null)} /><div className="relative w-full max-w-2xl max-h-[90vh] overflow-y-auto bg-surface-container-lowest rounded-2xl p-6 shadow-2xl space-y-4">
        <div className="flex items-center gap-3"><div className="flex-1"><h2 className="text-headline-sm font-bold">Edit Request — Row {editing.rowIndex}</h2><p className="text-body-sm text-outline">After saving, the system will revalidate immediately.</p></div><button onClick={() => setEditing(null)} className="p-2 rounded-lg hover:bg-surface-container"><span className="material-symbols-outlined">close</span></button></div>
        <div className="grid md:grid-cols-2 gap-4">
          <label><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Brand *</span><select value={editForm.brandId} onChange={(event) => handleEditBrand(event.target.value)} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20"><option value="">Select brand</option>{brands.map((brand) => <option key={brand.id} value={brand.id}>{brand.name}</option>)}</select></label>
          <label><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Product</span><select value={editForm.productId} onChange={(event) => setEditForm((value) => ({ ...value, productId: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20"><option value="">None</option>{products.map((product) => <option key={product.id} value={product.id}>{product.name}</option>)}</select></label>
          <label className="md:col-span-2"><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Topic *</span><input value={editForm.topic} onChange={(event) => setEditForm((value) => ({ ...value, topic: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Platform</span><select value={editForm.platform} onChange={(event) => setEditForm((value) => ({ ...value, platform: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20"><option value="facebook">Facebook</option><option value="instagram">Instagram</option><option value="tiktok">TikTok</option></select></label>
          <label><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Content Type</span><select value={editForm.contentType} onChange={(event) => setEditForm((value) => ({ ...value, contentType: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20"><option value="Text">Text</option><option value="Image">Image</option><option value="Video">Video</option><option value="Auto">Auto</option></select></label>
          <label><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Date & Time *</span><input type="datetime-local" value={editForm.scheduledAt} onChange={(event) => setEditForm((value) => ({ ...value, scheduledAt: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Objective</span><input value={editForm.objective} onChange={(event) => setEditForm((value) => ({ ...value, objective: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Tone</span><input value={editForm.tone} onChange={(event) => setEditForm((value) => ({ ...value, tone: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">CTA</span><input value={editForm.cta} onChange={(event) => setEditForm((value) => ({ ...value, cta: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label className="md:col-span-2"><span className="text-label-2xs text-outline uppercase font-bold tracking-widest">Notes</span><textarea value={editForm.notes} onChange={(event) => setEditForm((value) => ({ ...value, notes: event.target.value }))} rows={3} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
        </div>
        {editForm.platform === "tiktok" && !["Video", "Auto"].includes(editForm.contentType) && <p className="text-[11px] text-amber-700 bg-amber-500/10 px-3 py-2 rounded-lg">TikTok requires Video or Auto content type.</p>}
        <div className="flex gap-3"><button onClick={() => setEditing(null)} className="flex-1 py-2.5 rounded-xl border border-outline-variant/30 text-label-sm font-semibold">Cancel</button><button onClick={handleSaveItem} disabled={working} className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold disabled:opacity-50">{working ? "Saving..." : "Save & Revalidate"}</button></div>
      </div></div>}
    </div>
  );
}
