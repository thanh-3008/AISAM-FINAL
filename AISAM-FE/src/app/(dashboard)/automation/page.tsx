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
  { value: "Asia/Ho_Chi_Minh", label: "Việt Nam", offset: "UTC+7" },
  { value: "Asia/Singapore", label: "Singapore", offset: "UTC+8" },
  { value: "Asia/Tokyo", label: "Nhật Bản", offset: "UTC+9" },
  { value: "UTC", label: "UTC", offset: "UTC+0" },
];

function timezoneLabel(value: string) {
  if (value === "Asia/Bangkok" || value === "Asia/Ho_Chi_Minh") return "Việt Nam (UTC+7)";
  const option = timezoneOptions.find((entry) => entry.value === value);
  return option ? `${option.label} (${option.offset})` : value;
}

function formatDate(value: string) {
  return new Date(value).toLocaleString("vi-VN", { dateStyle: "short", timeStyle: "short" });
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
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể tải automation plans."); }
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
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể tải plan."); }
    finally { setWorking(false); }
  };

  const handleImport = async () => {
    if (!name.trim() || !file) { setMessage("Nhập tên kế hoạch và chọn file CSV."); return; }
    setWorking(true); setMessage("");
    try {
      const plan = await importAutomationCsv(name.trim(), timezone, file);
      setShowImport(false); setName(""); setFile(null); setSelected(plan);
      await load();
    } catch (error) { setMessage(error instanceof Error ? error.message : "Import thất bại."); }
    finally { setWorking(false); }
  };

  const handleConfirm = async () => {
    if (!selected) return;
    setWorking(true);
    try { const plan = await confirmAutomationPlan(selected.id); setSelected(plan); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Xác nhận thất bại."); }
    finally { setWorking(false); }
  };

  const handleRetry = async (itemId?: string) => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try { setSelected(await retryAutomationPlan(selected.id, itemId)); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể thử lại."); }
    finally { setWorking(false); }
  };

  const handleCancel = async () => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try { setSelected(await cancelAutomationPlan(selected.id)); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể hủy kế hoạch."); }
    finally { setWorking(false); }
  };

  const handleApprove = async (itemId?: string) => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try {
      if (itemId) {
        const targets = await fetchAutomationTargets(selected.id, itemId);
        if (targets.length === 0) throw new Error("Brand này chưa liên kết Page đang hoạt động cho nền tảng đã chọn.");
        if (targets.length > 1) {
          setTargetItemId(itemId); setAvailableTargets(targets); setSelectedTargetIds(targets.filter((target) => target.isScheduled).map((target) => target.integrationId));
          return;
        }
        setSelected(await approveAutomationTargets(selected.id, itemId, [targets[0].integrationId]));
      } else setSelected(await approveAutomationPlan(selected.id));
      await load();
    }
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể duyệt và lên lịch."); }
    finally { setWorking(false); }
  };

  const handleApproveTargets = async () => {
    if (!selected || !targetItemId || selectedTargetIds.length === 0) { setMessage("Hãy chọn ít nhất một Page để đăng."); return; }
    setWorking(true); setMessage("");
    try { setSelected(await approveAutomationTargets(selected.id, targetItemId, selectedTargetIds)); setTargetItemId(null); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể tạo lịch cho các Page đã chọn."); }
    finally { setWorking(false); }
  };

  const handleReject = async (itemId: string) => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try { setSelected(await rejectAutomationItem(selected.id, itemId, "Rejected from Automation Plan")); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể từ chối nội dung."); }
    finally { setWorking(false); }
  };

  const handleGoogleSheet = async () => {
    const url = window.prompt("Dán URL Google Sheet đã bật chia sẻ bằng liên kết:");
    if (!url) return;
    const planName = window.prompt("Tên kế hoạch:", "Google Sheets plan");
    if (!planName) return;
    setWorking(true); setMessage("");
    try { const plan = await importAutomationGoogleSheet(planName, timezone, url); setSelected(plan); setPerformance(null); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể import Google Sheet."); }
    finally { setWorking(false); }
  };

  const handleClone = async () => {
    if (!selected) return;
    const planName = window.prompt("Tên kế hoạch mới:", `${selected.name} - bản tiếp theo`);
    if (!planName) return;
    const shift = Number(window.prompt("Dịch lịch thêm bao nhiêu ngày?", "7"));
    if (!Number.isInteger(shift) || shift < 1) { setMessage("Số ngày phải là số nguyên lớn hơn 0."); return; }
    setWorking(true); setMessage("");
    try { const plan = await cloneAutomationPlan(selected.id, planName, shift); setSelected(plan); setPerformance(null); await load(); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể tạo kế hoạch từ template."); }
    finally { setWorking(false); }
  };

  const handleAutoApprove = async () => {
    if (!selected) return;
    setWorking(true); setMessage("");
    try { setSelected(await setAutomationAutoApprove(selected.id, !selected.autoApprove)); }
    catch (error) { setMessage(error instanceof Error ? error.message : "Không thể thay đổi auto-approve."); }
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
    if (!editForm.brandId || !editForm.topic.trim() || !editForm.scheduledAt) { setMessage("Brand, chủ đề, ngày và giờ là bắt buộc."); return; }
    setWorking(true); setMessage("");
    try {
      const plan = await updateAutomationItem(selected.id, editing.id, { ...editForm, productId: editForm.productId || undefined, scheduledAt: new Date(editForm.scheduledAt).toISOString() });
      setSelected(plan); setEditing(null); await load();
    } catch (error) { setMessage(error instanceof Error ? error.message : "Không thể sửa yêu cầu."); }
    finally { setWorking(false); }
  };

  const downloadTemplate = () => {
    const csv = [
      "Brand,Product,Topic,Objective,Platforms,ContentType,Tone,CTA,Notes,Date,Time",
      'Tên thương hiệu,Tên sản phẩm,"Giới thiệu sản phẩm mới",Awareness,"Facebook|Instagram",Image,Trẻ trung,"Xem ngay","Dùng màu thương hiệu",10/08/2026,09:00',
      'Tên thương hiệu,Tên sản phẩm,"Video hướng dẫn",Engagement,"Facebook|Instagram|TikTok",Video,Thân thiện,"Theo dõi ngay","Video dọc",12/08/2026,19:30',
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
            <h1 className="text-headline-lg text-on-surface font-bold">Content Automation Plans</h1>
            <p className="text-body-sm text-on-surface-variant mt-1">Import lịch trình, kiểm tra đầu vào và quản lý toàn bộ bài trong một nơi.</p>
          </div>
          <div className="md:ml-auto flex gap-2">
            <button onClick={downloadTemplate} className="px-4 py-2.5 rounded-xl border border-outline-variant/30 text-label-sm font-semibold text-on-surface hover:bg-surface-container">Tải CSV mẫu</button>
            <button onClick={handleGoogleSheet} disabled={working} className="px-4 py-2.5 rounded-xl border border-outline-variant/30 text-label-sm font-semibold text-on-surface hover:bg-surface-container disabled:opacity-50">Google Sheets</button>
            <button onClick={() => setShowImport(true)} className="px-5 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-bold shadow-lg shadow-primary/20 flex items-center gap-2">
              <span className="material-symbols-outlined text-[18px]">upload_file</span> Import kế hoạch
            </button>
          </div>
        </section>

        {message && <div className="px-4 py-3 rounded-xl bg-amber-500/10 text-amber-700 text-body-sm">{message}</div>}

        <section className="grid grid-cols-2 lg:grid-cols-4 gap-3">
          {[
            ["Tổng kế hoạch", summary.total, "view_timeline"], ["Chờ xác nhận", summary.awaiting, "pending_actions"],
            ["Đang xử lý", summary.running, "progress_activity"], ["Credit dự kiến", summary.credits, "toll"],
          ].map(([label, value, icon]) => (
            <div key={String(label)} className="p-4 rounded-2xl bg-surface-container-lowest border border-outline-variant/20">
              <span className="material-symbols-outlined text-primary">{icon}</span>
              <p className="text-headline-md font-bold text-on-surface mt-2">{value}</p><p className="text-label-sm text-outline">{label}</p>
            </div>
          ))}
        </section>

        <section className="rounded-2xl bg-surface-container-lowest border border-outline-variant/20 overflow-hidden">
          <div className="px-5 py-4 border-b border-outline-variant/15"><h2 className="text-headline-sm font-bold text-on-surface">Các lịch trình</h2></div>
          {loading ? <div className="p-10 text-center text-outline">Đang tải...</div> : plans.length === 0 ? (
            <div className="p-14 text-center"><span className="material-symbols-outlined text-5xl text-outline/40">auto_awesome_motion</span><p className="font-semibold mt-3">Chưa có automation plan</p><p className="text-body-sm text-outline">Tải CSV mẫu và import kế hoạch đầu tiên.</p></div>
          ) : <div className="divide-y divide-outline-variant/10">{plans.map((plan) => (
            <button key={plan.id} onClick={() => openPlan(plan.id)} className="w-full px-5 py-4 text-left hover:bg-surface-container-low transition flex items-center gap-4">
              <div className="w-11 h-11 rounded-xl bg-primary/10 text-primary flex items-center justify-center"><span className="material-symbols-outlined">calendar_view_week</span></div>
              <div className="min-w-0 flex-1"><p className="font-bold text-on-surface truncate">{plan.name}</p><p className="text-label-xs text-outline">{plan.totalItems} platform items · {formatDate(plan.createdAt)}</p></div>
              <span className={`hidden sm:inline px-2.5 py-1 rounded-full text-label-xs font-bold ${statusStyle[plan.status] || "bg-surface-container text-outline"}`}>{plan.status}</span>
              <div className="text-right"><p className="font-bold text-on-surface">{plan.validItems}/{plan.totalItems}</p><p className="text-label-xs text-outline">hợp lệ</p></div>
              <span className="material-symbols-outlined text-outline">chevron_right</span>
            </button>
          ))}</div>}
        </section>
      </main>

      {showImport && <div className="fixed inset-0 z-[80] flex items-center justify-center p-4"><div className="absolute inset-0 bg-black/50" onClick={() => setShowImport(false)} /><div className="relative w-full max-w-lg bg-surface-container-lowest rounded-2xl p-6 shadow-2xl space-y-4">
        <div><h2 className="text-headline-sm font-bold">Import Automation Plan</h2><p className="text-body-sm text-outline">CSV được validate trước, chưa gọi AI và chưa trừ credit.</p></div>
        <label className="block text-label-sm font-semibold">Tên kế hoạch<input value={name} onChange={(event) => setName(event.target.value)} className="mt-1.5 w-full px-4 py-3 rounded-xl bg-surface-container border border-outline-variant/20 outline-none" placeholder="Chiến dịch tháng 8" /></label>
        <fieldset><legend className="text-label-sm font-semibold mb-2">Múi giờ</legend><div className="grid grid-cols-2 gap-2">{timezoneOptions.map((option) => <label key={option.value} className={`p-3 rounded-xl border cursor-pointer flex items-center gap-2 ${timezone === option.value ? "border-primary bg-primary/10 text-primary" : "border-outline-variant/20 bg-surface-container"}`}><input type="radio" name="automation-timezone" value={option.value} checked={timezone === option.value} onChange={(event) => setTimezone(event.target.value)} className="accent-primary" /><span><span className="block text-label-sm font-bold">{option.label}</span><span className="block text-label-xs opacity-70">{option.offset}</span></span></label>)}</div></fieldset>
        <label className="block p-6 rounded-xl border-2 border-dashed border-outline-variant/30 text-center cursor-pointer hover:border-primary/50"><span className="material-symbols-outlined text-3xl text-primary">csv</span><p className="font-semibold">{file?.name || "Chọn file CSV"}</p><input type="file" accept=".csv,text/csv" className="hidden" onChange={(event) => setFile(event.target.files?.[0] ?? null)} /></label>
        <div className="flex gap-3"><button onClick={() => setShowImport(false)} className="flex-1 py-2.5 rounded-xl border border-outline-variant/30 font-semibold">Hủy</button><button onClick={handleImport} disabled={working || !file || !name.trim()} className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary font-bold disabled:opacity-50">{working ? "Đang import..." : "Import & Validate"}</button></div>
      </div></div>}

      {selected && <div className="fixed inset-0 z-[80] flex justify-end"><div className="absolute inset-0 bg-black/40" onClick={() => setSelected(null)} /><aside className="relative w-full max-w-3xl h-full bg-background shadow-2xl overflow-y-auto">
        <div className="sticky top-0 bg-background/95 backdrop-blur border-b border-outline-variant/20 p-5 flex items-center gap-3 z-10"><button onClick={() => setSelected(null)} className="p-2 rounded-lg hover:bg-surface-container"><span className="material-symbols-outlined">close</span></button><div className="flex-1"><h2 className="text-headline-sm font-bold">{selected.name}</h2><p className="text-label-xs text-outline">{selected.sourceFileName || "Manual plan"} · {timezoneLabel(selected.timezone)}</p></div>{selected.status === "AwaitingConfirmation" && <button onClick={handleConfirm} disabled={working || selected.validItems === 0} className="px-4 py-2.5 rounded-xl bg-primary text-on-primary font-bold disabled:opacity-50">Xác nhận kế hoạch</button>}{selected.status === "Generating" && <button onClick={handleCancel} disabled={working} className="px-4 py-2.5 rounded-xl border border-red-500/30 text-red-600 font-bold disabled:opacity-50">Hủy</button>}{selected.items.some((item) => item.status === "AwaitingApproval") && <button onClick={() => handleApprove()} disabled={working} className="px-4 py-2.5 rounded-xl bg-primary text-on-primary font-bold disabled:opacity-50">Duyệt & lên lịch</button>}{["Failed", "PartiallyFailed", "AwaitingApproval"].includes(selected.status) && selected.items.some((item) => item.status === "GenerationFailed" && item.validationErrors.length === 0) && <button onClick={() => handleRetry()} disabled={working} className="px-4 py-2.5 rounded-xl border border-primary/30 text-primary font-bold disabled:opacity-50">Thử lại lỗi</button>}</div>
        <div className="p-5 space-y-4"><div className="grid grid-cols-3 gap-3"><div className="p-3 rounded-xl bg-emerald-500/10"><p className="font-bold text-emerald-700">{selected.validItems}</p><p className="text-label-xs">Hợp lệ</p></div><div className="p-3 rounded-xl bg-red-500/10"><p className="font-bold text-red-700">{selected.failedItems}</p><p className="text-label-xs">Cần xử lý</p></div><div className="p-3 rounded-xl bg-primary/10"><p className="font-bold text-primary">{selected.estimatedCredits}</p><p className="text-label-xs">Credit dự kiến</p></div></div>
          <div className="p-4 rounded-xl bg-surface-container-lowest border border-outline-variant/20 flex flex-wrap items-center gap-3"><div className="flex-1 min-w-48"><p className="font-bold text-on-surface">Vận hành nâng cao</p><p className="text-label-xs text-outline">{selected.templateSourcePlanId ? "Được tạo từ template" : "Có thể dùng lại làm template"} · Auto-approve {selected.autoApprove ? "đang bật" : "đang tắt"}</p></div><button onClick={handleClone} disabled={working} className="px-3 py-2 rounded-lg border border-outline-variant/30 text-label-xs font-bold">Dùng làm template</button>{["AwaitingConfirmation", "Generating"].includes(selected.status) && <button onClick={handleAutoApprove} disabled={working} className={`px-3 py-2 rounded-lg text-label-xs font-bold ${selected.autoApprove ? "bg-amber-500/15 text-amber-700" : "bg-primary/10 text-primary"}`}>{selected.autoApprove ? "Tắt auto-approve" : "Bật auto-approve"}</button>}</div>
          {performance && <div className="grid grid-cols-2 md:grid-cols-4 gap-2">{[["Đã đăng", performance.publishedItems], ["Impressions", performance.impressions], ["Engagement", performance.engagement], ["CTR trung bình", `${(performance.averageCtr * 100).toFixed(2)}%`]].map(([label, value]) => <div key={String(label)} className="p-3 rounded-xl bg-surface-container"><p className="font-bold text-on-surface">{value}</p><p className="text-label-xs text-outline">{label}</p></div>)}</div>}
          {selected.items.map((item) => <article key={item.id} className="p-4 rounded-xl bg-surface-container-lowest border border-outline-variant/20"><div className="flex items-start gap-3"><span className="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center text-label-sm font-bold">{item.rowIndex}</span><div className="flex-1 min-w-0"><div className="flex flex-wrap gap-2 items-center"><h3 className="font-bold text-on-surface">{item.topic || "Untitled"}</h3><span className="px-2 py-0.5 rounded-full bg-surface-container text-label-xs uppercase font-bold">{item.platform}</span><span className="text-label-xs text-outline">{item.contentType}</span></div><p className="text-label-xs text-outline mt-1">{item.brandName || "Unknown brand"} · {formatDate(item.scheduledAt)} · {item.usedCredits}/{item.estimatedCredits} credits</p>{item.generatedImageUrl && <img src={item.generatedImageUrl} alt="Generated automation asset" className="mt-3 w-28 h-28 rounded-xl object-cover" />}{item.generatedVideoUrl && <video src={item.generatedVideoUrl} controls className="mt-3 w-full max-w-xs rounded-xl bg-black" />}{item.generatedText && <p className="mt-3 text-body-sm text-on-surface-variant whitespace-pre-line line-clamp-4">{item.generatedText}</p>}{item.videoProvider && <p className="mt-1 text-label-xs text-outline">Video provider: {item.videoProvider}</p>}{item.validationErrors.length > 0 && <ul className="mt-2 space-y-1">{item.validationErrors.map((error) => <li key={error} className="text-label-xs text-red-600 flex gap-1"><span>•</span>{error}</li>)}</ul>}{selected.status === "AwaitingConfirmation" && <button onClick={() => openItemEditor(item)} className="mt-3 px-3 py-1.5 rounded-lg border border-primary/30 text-primary text-label-xs font-bold flex items-center gap-1"><span className="material-symbols-outlined text-[16px]">edit</span>Sửa yêu cầu</button>}{item.lastError && <div className="mt-2 flex items-center gap-2"><p className="text-label-xs text-red-600 flex-1">{item.lastError}</p>{item.status === "GenerationFailed" && item.validationErrors.length === 0 && <button onClick={() => handleRetry(item.id)} disabled={working} className="text-label-xs font-bold text-primary">Thử lại</button>}</div>}{item.status === "NeedsAttention" && item.contentId && item.lastError?.startsWith("No active ") && <button onClick={() => handleApprove(item.id)} disabled={working} className="mt-3 px-3 py-1.5 rounded-lg bg-primary text-on-primary text-label-xs font-bold">Thử lên lịch lại</button>}{item.status === "AwaitingApproval" && <div className="mt-3 flex gap-2"><button onClick={() => handleApprove(item.id)} disabled={working} className="px-3 py-1.5 rounded-lg bg-primary text-on-primary text-label-xs font-bold">Duyệt & lên lịch</button><button onClick={() => handleReject(item.id)} disabled={working} className="px-3 py-1.5 rounded-lg border border-red-500/30 text-red-600 text-label-xs font-bold">Từ chối</button></div>}{item.contentCalendarId && <p className="mt-2 text-label-xs text-emerald-700">Đã tạo lịch đăng · {formatDate(item.scheduledAt)}</p>}</div><span className={`px-2 py-1 rounded-lg text-label-xs font-bold ${["Pending", "AwaitingApproval", "Scheduled"].includes(item.status) ? "bg-emerald-500/10 text-emerald-700" : item.status.startsWith("Generating") ? "bg-blue-500/10 text-blue-700" : "bg-red-500/10 text-red-700"}`}>{item.status}</span></div></article>)}
        </div>
      </aside></div>}

      {targetItemId && <div className="fixed inset-0 z-[100] flex items-center justify-center p-4"><div className="absolute inset-0 bg-black/60" onClick={() => setTargetItemId(null)} /><div className="relative w-full max-w-lg bg-surface-container-lowest rounded-2xl p-6 shadow-2xl space-y-4">
        <div><h2 className="text-headline-sm font-bold">Chọn Page muốn đăng</h2><p className="text-body-sm text-outline mt-1">Brand này có nhiều Page liên kết. Bạn có thể chọn một hoặc nhiều Page.</p></div>
        <div className="space-y-2">{availableTargets.map((target) => <label key={target.integrationId} className={`p-4 rounded-xl border flex items-center gap-3 cursor-pointer ${selectedTargetIds.includes(target.integrationId) ? "border-primary bg-primary/10" : "border-outline-variant/20 bg-surface-container"}`}><input type="checkbox" checked={selectedTargetIds.includes(target.integrationId)} onChange={(event) => setSelectedTargetIds((ids) => event.target.checked ? [...ids, target.integrationId] : ids.filter((id) => id !== target.integrationId))} className="w-5 h-5 accent-primary" /><span className="flex-1"><span className="block font-bold text-on-surface">{target.name}</span><span className="block text-label-xs text-outline">{target.externalId || target.integrationId}{target.isScheduled ? " · Đã có lịch" : ""}</span></span></label>)}</div>
        <div className="flex gap-3"><button onClick={() => setTargetItemId(null)} className="flex-1 py-2.5 rounded-xl border border-outline-variant/30 font-semibold">Hủy</button><button onClick={handleApproveTargets} disabled={working || selectedTargetIds.length === 0} className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary font-bold disabled:opacity-50">{working ? "Đang tạo lịch..." : `Đăng lên ${selectedTargetIds.length} Page`}</button></div>
      </div></div>}

      {editing && <div className="fixed inset-0 z-[100] flex items-center justify-center p-4"><div className="absolute inset-0 bg-black/60" onClick={() => setEditing(null)} /><div className="relative w-full max-w-2xl max-h-[90vh] overflow-y-auto bg-surface-container-lowest rounded-2xl p-6 shadow-2xl space-y-4">
        <div className="flex items-center gap-3"><div className="flex-1"><h2 className="text-headline-sm font-bold">Sửa yêu cầu dòng {editing.rowIndex}</h2><p className="text-body-sm text-outline">Lưu xong hệ thống sẽ kiểm tra lại ngay.</p></div><button onClick={() => setEditing(null)} className="p-2 rounded-lg hover:bg-surface-container"><span className="material-symbols-outlined">close</span></button></div>
        <div className="grid md:grid-cols-2 gap-4">
          <label className="text-label-sm font-semibold">Thương hiệu *<select value={editForm.brandId} onChange={(event) => handleEditBrand(event.target.value)} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20"><option value="">Chọn thương hiệu</option>{brands.map((brand) => <option key={brand.id} value={brand.id}>{brand.name}</option>)}</select></label>
          <label className="text-label-sm font-semibold">Sản phẩm<select value={editForm.productId} onChange={(event) => setEditForm((value) => ({ ...value, productId: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20"><option value="">Không chọn</option>{products.map((product) => <option key={product.id} value={product.id}>{product.name}</option>)}</select></label>
          <label className="md:col-span-2 text-label-sm font-semibold">Chủ đề *<input value={editForm.topic} onChange={(event) => setEditForm((value) => ({ ...value, topic: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label className="text-label-sm font-semibold">Nền tảng<select value={editForm.platform} onChange={(event) => setEditForm((value) => ({ ...value, platform: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20"><option value="facebook">Facebook</option><option value="instagram">Instagram</option><option value="tiktok">TikTok</option></select></label>
          <label className="text-label-sm font-semibold">Loại nội dung<select value={editForm.contentType} onChange={(event) => setEditForm((value) => ({ ...value, contentType: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20"><option value="Text">Text</option><option value="Image">Image</option><option value="Video">Video</option><option value="Auto">Auto</option></select></label>
          <label className="text-label-sm font-semibold">Ngày và giờ *<input type="datetime-local" value={editForm.scheduledAt} onChange={(event) => setEditForm((value) => ({ ...value, scheduledAt: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label className="text-label-sm font-semibold">Mục tiêu<input value={editForm.objective} onChange={(event) => setEditForm((value) => ({ ...value, objective: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label className="text-label-sm font-semibold">Tone<input value={editForm.tone} onChange={(event) => setEditForm((value) => ({ ...value, tone: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label className="text-label-sm font-semibold">CTA<input value={editForm.cta} onChange={(event) => setEditForm((value) => ({ ...value, cta: event.target.value }))} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
          <label className="md:col-span-2 text-label-sm font-semibold">Ghi chú<textarea value={editForm.notes} onChange={(event) => setEditForm((value) => ({ ...value, notes: event.target.value }))} rows={3} className="mt-1.5 w-full px-3 py-3 rounded-xl bg-surface-container border border-outline-variant/20" /></label>
        </div>
        {editForm.platform === "tiktok" && !["Video", "Auto"].includes(editForm.contentType) && <p className="text-label-sm text-amber-700 bg-amber-500/10 px-3 py-2 rounded-lg">TikTok yêu cầu loại Video hoặc Auto.</p>}
        <div className="flex gap-3"><button onClick={() => setEditing(null)} className="flex-1 py-2.5 rounded-xl border border-outline-variant/30 font-semibold">Hủy</button><button onClick={handleSaveItem} disabled={working} className="flex-1 py-2.5 rounded-xl bg-primary text-on-primary font-bold disabled:opacity-50">{working ? "Đang lưu..." : "Lưu & kiểm tra lại"}</button></div>
      </div></div>}
    </div>
  );
}
