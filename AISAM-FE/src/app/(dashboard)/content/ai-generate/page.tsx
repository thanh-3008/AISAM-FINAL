"use client";

import { useState, useRef, useEffect } from "react";
import { useRouter } from "next/navigation";
import Header from "@/components/layout/Header";
import CreditUsageHistoryModal from "@/components/ui/CreditUsageHistoryModal";
import { createContent, updateContentDetails, generateAIDraft, chatWithAI, getConversationMessages, type CreateContentPayload } from "@/services/contentService";
import { useToast } from "@/contexts/ToastContext";
import { PLATFORM_CONFIG, getBrandColor, PlatformIcon } from "@/lib/contentConstants";
import { fetchBrands, fetchProducts } from "@/services/brandService";
import { useWorkspaces } from "@/hooks/useWorkspaces";
import { fetchCreditWallet } from "@/services/workspaceService";
import { useFeatureGate } from "@/hooks/useFeatureGate";

interface ChatMessage {
  id: string;
  role: "user" | "assistant";
  text: string;
  canApply?: boolean;
}

interface Variation {
  id: string;
  prompt: string;
  result: string;
}

const VARIATION_TEMPLATES: Record<string, string> = {
  longer: "Make this content longer and more detailed",
  formal: "Rewrite this in a formal, professional tone",
  casual: "Rewrite this in a casual, conversational tone",
  hashtags: "Add relevant hashtags to this content",
  bullet: "Convert this content to bullet points",
  emoji: "Add relevant emojis to make this content more engaging",
};

const PLATFORMS = [
  { value: "instagram-feed", label: PLATFORM_CONFIG.instagram.label, icon: PLATFORM_CONFIG.instagram.icon, charLimit: 2200 },
  { value: "facebook-post", label: PLATFORM_CONFIG.facebook.label, icon: PLATFORM_CONFIG.facebook.icon, charLimit: 63206 },
  { value: "tiktok-video", label: PLATFORM_CONFIG.tiktok.label, icon: PLATFORM_CONFIG.tiktok.icon, charLimit: 2200 },
];

const AUTO_HASHTAGS: Record<string, string[]> = {
  "Lumina Tech": ["SmartHome", "Lighting", "TechLife", "Innovation", "LED"],
  "Summit Outdoor": ["Adventure", "OutdoorLife", "Nature", "Explore", "GearUp"],
  "Heritage Motors": ["AutoTech", "Performance", "Engine", "Racing", "CustomBuild"],
  "GreenLeaf Organics": ["Organic", "HealthyLiving", "EcoFriendly", "GreenLife", "Wellness"],
  "Pulse Finance": ["FinTech", "InvestSmart", "Wealth", "FinanceTips", "MoneyMatters"],
};

export default function AIGeneratePage() {
  const router = useRouter();
  const { addToast } = useToast();
  const { activeWorkspace } = useWorkspaces();
  const featureGate = useFeatureGate();
  const [creditBalance, setCreditBalance] = useState<number | null>(null);
  const [insufficientCredits, setInsufficientCredits] = useState(false);
  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);

  const [brandList, setBrandList] = useState<{ id: string; name: string }[]>([]);
  const [productList, setProductList] = useState<{ id: string; name: string; brandId: string }[]>([]);
  const [brandId, setBrandId] = useState("");
  const [productId, setProductId] = useState("");
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [hashtags, setHashtags] = useState<string[]>([]);
  const [platform, setPlatform] = useState(PLATFORMS[0].value);
  const [imageUrl, setImageUrl] = useState<string | null>(null);

  const [chatInput, setChatInput] = useState("");
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [conversationId, setConversationId] = useState<string | null>(null);
  const [isGenerating, setIsGenerating] = useState(false);

  const [variations, setVariations] = useState<Variation[]>([]);
  const [selectedVariation, setSelectedVariation] = useState<string | null>(null);
  const [generatedId, setGeneratedId] = useState<string | null>(null);
  const [justGenerated, setJustGenerated] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isVideo, setIsVideo] = useState(false);

  const chatEndRef = useRef<HTMLDivElement>(null);

  const availableProducts = brandList.length > 0 ? productList : [];

  useEffect(() => {
    fetchBrands().then(list => {
      setBrandList(list);
      if (list.length > 0) setBrandId(list[0].id);
    });
  }, []);

  useEffect(() => {
    if (brandId) {
      fetchProducts(brandId).then(setProductList);
    } else {
      setProductList([]);
    }
  }, [brandId]);

  const selectedBrand = brandList.find(b => b.id === brandId);
  const brandName = selectedBrand?.name || "";
  const selectedProduct = productList.find(p => p.id === productId);
  const productName = selectedProduct?.name || "";
  const conversationStorageKey = activeWorkspace?.id && brandId
    ? `ai-conversation-${activeWorkspace.id}-${brandId}-${productId || "no-product"}`
    : null;

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  useEffect(() => {
    fetchCreditWallet().then(w => { if (w) setCreditBalance(w.balance); });
  }, [activeWorkspace?.id]);

  useEffect(() => {
    setConversationId(null);
    setVariations([]);
    setMessages([]);
    setSelectedVariation(null);
    setGeneratedId(null);
    setJustGenerated(false);
    setImageUrl(null);
    setIsVideo(false);
    if (!conversationStorageKey) return;

    let cancelled = false;
    const savedId = sessionStorage.getItem(conversationStorageKey);
    if (!savedId) return;
    setConversationId(savedId);
    getConversationMessages(savedId).then(msgs => {
      if (!cancelled && msgs) {
        setMessages(msgs.map((message, index) => ({
          id: `history-${index}`,
          role: message.senderType === 0 ? "user" : "assistant",
          text: message.message,
          canApply: false,
        })));
      }
    });
    return () => { cancelled = true; };
  }, [conversationStorageKey]);

  const simulateAIResponse = async (userPrompt: string) => {
    if (creditBalance !== null && creditBalance <= 0) {
      setInsufficientCredits(true);
      addToast("Insufficient AI Credits. Please purchase more credits.");
      return;
    }
    setInsufficientCredits(false);
    setIsGenerating(true);

    const aiReply = await chatWithAI(userPrompt, 0, brandId || undefined, productId || undefined, conversationId || undefined, messages.map(m => ({ role: m.role, text: m.text })));
    if (aiReply?.errorMessage) {
      setIsGenerating(false);
      addToast(aiReply.errorMessage);
      return;
    }
    if (aiReply) {
      setConversationId(aiReply.conversationId ?? null);
      if (conversationStorageKey && aiReply.conversationId) sessionStorage.setItem(conversationStorageKey, aiReply.conversationId);

      const generatedHashtags = AUTO_HASHTAGS[brandName] || [];
      const canCreate = aiReply.shouldCreateContent || Boolean(aiReply.createdContentId);
      const aiMsg: ChatMessage = {
        id: `ai-${Date.now()}`,
        role: "assistant",
        text: aiReply.text || "",
        canApply: canCreate,
      };
      setMessages((prev) => [...prev, aiMsg]);

      const variation: Variation = {
        id: `v-${Date.now()}`,
        prompt: userPrompt,
        result: aiReply.text || "",
      };
      if (aiReply.shouldCreateContent || aiReply.createdContentId) {
        setVariations((prev) => [variation, ...prev]);
      }

      fetchCreditWallet().then(w => { if (w) setCreditBalance(w.balance); });

      setIsGenerating(false);

      if (aiReply.createdContentId) {
        setGeneratedId(aiReply.createdContentId ?? null);
        setJustGenerated(true); // Automatically show "View Post" so the user can navigate to see the processing status
      }
      if (aiReply.shouldCreateContent || aiReply.createdContentId) {
        handleApplyVariation(variation);
      }
      return;
    }

    setIsGenerating(false);
    addToast("AI service request failed. Your credit balance was not the cause; restart/check the backend and Gemini connection, then try again.");
  };

  const handleManualSave = async () => {
    if (!title && !content && !imageUrl) {
      addToast("Nothing to save.");
      return;
    }
    
    setIsSaving(true);
    const payload: Partial<CreateContentPayload> = {
      brandId,
      productId: productId || null,
      adType: 0,
      title: title || `AI Generated — ${brandName}`,
      textContent: content || "",
      status: 1, // Awaiting Approval
      isAiGenerated: true,
    };

    if (imageUrl) {
      payload.imageUrl = imageUrl;
      payload.adType = 1;
    } else if (isVideo) {
      payload.adType = 2;
    } else {
      payload.adType = 0;
    }

    try {
      let result;
      if (generatedId) {
        result = await updateContentDetails(generatedId, payload);
      } else {
        result = await createContent(payload as CreateContentPayload);
      }
      
      if (result) {
        setGeneratedId(result.id);
        setJustGenerated(true);
        addToast("Post saved successfully!");
      }
    } catch (e: any) {
      addToast(e?.message || "Failed to save post");
    } finally {
      setIsSaving(false);
    }
  };

  const handleSendChat = () => {
    const text = chatInput.trim();
    if (!text || isGenerating) return;

    const userMsg: ChatMessage = { id: `u-${Date.now()}`, role: "user", text };
    setMessages((prev) => [...prev, userMsg]);
    setChatInput("");
    setSelectedVariation(null);

    simulateAIResponse(text);
  };

  const handleQuickTemplate = (key: string) => {
    const prompt = VARIATION_TEMPLATES[key];
    const userMsg: ChatMessage = { id: `u-${Date.now()}`, role: "user", text: prompt };
    setMessages((prev) => [...prev, userMsg]);
    setSelectedVariation(null);
    simulateAIResponse(prompt);
  };

  const handleApplyVariation = (variation: Variation) => {
    const h = AUTO_HASHTAGS[brandName] || [];
    let cleanContent = variation.result;
    const imageMatch = cleanContent.match(/\[IMAGE:\s*(.+?)\]/);
    if (imageMatch) {
      setImageUrl(imageMatch[1]);
      cleanContent = cleanContent.replace(/\[IMAGE:\s*(.+?)\]/g, '').trim();
    }
    const videoMatch = cleanContent.match(/\[VIDEO_JOB:\s*(.+?)\]/);
    if (videoMatch) {
      setIsVideo(true);
      cleanContent = cleanContent.replace(/\[VIDEO_JOB:\s*(.+?)\]/g, '').trim();
    } else {
      setIsVideo(false);
    }
    setTitle(`AI Generated — ${brandName}`);
    setContent(cleanContent);
    setHashtags(h);
    setSelectedVariation(variation.id);
  };

  return (
    <>
      <Header breadcrumbs={[
        { label: "Dashboard", href: "/dashboard" },
        { label: "Content Library", href: "/content" },
        { label: "AI Generate" },
      ]} />

      <main className="ml-0 p-6 h-[calc(100vh-64px)] overflow-hidden flex flex-col">
        <div className="flex items-center justify-between mb-5 shrink-0">
          <div className="flex items-center gap-4">
            <button onClick={() => router.push("/content")}
              className="w-9 h-9 flex items-center justify-center rounded-xl border border-outline-variant/20 text-outline/50 hover:bg-surface-container hover:text-on-surface transition-all active:scale-[0.97]">
              <span className="material-symbols-outlined text-[18px]">arrow_back</span>
            </button>
            <div>
              <h1 className="text-headline-sm font-bold text-on-surface">AI Generate</h1>
              <p className="text-body-sm text-on-surface-variant">Create content with AI assistance</p>
            </div>
          </div>
        </div>

        <div className="flex gap-gutter flex-1 min-h-0">
          {/* Left Column: Chat History */}
          <div className="w-[260px] shrink-0 bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm flex flex-col overflow-hidden">
            <div className="p-4 border-b border-outline-variant/10">
              <h2 className="text-label-md font-semibold text-on-surface flex items-center gap-2">
                <span className="material-symbols-outlined text-[16px] text-outline">history</span>
                Chat History
              </h2>
            </div>
            <div className="flex-1 overflow-y-auto p-3 space-y-2">
              {variations.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full text-center px-4">
                  <div className="w-12 h-12 rounded-2xl bg-primary/10 flex items-center justify-center mb-3">
                    <span className="material-symbols-outlined text-primary text-2xl">chat</span>
                  </div>
                  <p className="text-body-sm text-on-surface font-medium">No generations yet</p>
                  <p className="text-label-sm text-outline/50 mt-1">Ask the AI assistant to generate content</p>
                </div>
              ) : (
                variations.map((v) => (
                  <div key={v.id} className={`p-3 rounded-xl border transition-all ${
                    selectedVariation === v.id
                      ? "border-primary bg-primary/5"
                      : "border-outline-variant/20 bg-surface-container hover:border-primary/30"
                  }`}>
                    <p className="text-[11px] text-outline/60 font-medium mb-1 line-clamp-1">Prompt: {v.prompt}</p>
                    <p className="text-[11px] text-on-surface-variant line-clamp-3 mb-2 leading-relaxed">{v.result}</p>
                    <button onClick={() => handleApplyVariation(v)}
                      className="text-label-xs font-semibold text-primary hover:text-primary/80 transition-colors flex items-center gap-1">
                      <span className="material-symbols-outlined text-[12px]">check</span>
                      Apply
                    </button>
                  </div>
                ))
              )}
            </div>

            {/* Quick Templates */}
            <div className="border-t border-outline-variant/10 p-3">
              <p className="text-label-xs font-semibold text-outline/50 uppercase tracking-wider mb-2">Quick Templates</p>
              <div className="grid grid-cols-3 gap-1.5">
                {Object.keys(VARIATION_TEMPLATES).map((key) => (
                  <button key={key} onClick={() => handleQuickTemplate(key)}
                    className="px-2 py-1.5 rounded-lg bg-surface-container text-label-xs text-on-surface-variant font-medium hover:bg-surface-container-high hover:text-on-surface transition-all active:scale-[0.97] truncate">
                    {key.charAt(0).toUpperCase() + key.slice(1)}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Center Column: Brand/Product Toolbar + Preview */}
          <div className="flex-1 min-w-0 flex flex-col gap-gutter overflow-y-auto pr-1 pb-4">
            {/* Brand & Product Toolbar */}
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-3 flex items-center gap-4 shrink-0">
              <div className="flex items-center gap-2">
                <span className="material-symbols-outlined text-[14px] text-outline">business</span>
                <select value={brandId} onChange={(e) => { setBrandId(e.target.value); setProductId(""); }}
                  className="bg-surface-container border border-outline-variant/20 rounded-lg px-2.5 py-1.5 text-[11px] text-on-surface font-medium focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all">
                  {brandList.map((b) => <option key={b.id} value={b.id}>{b.name}</option>)}
                </select>
              </div>
              <div className="flex items-center gap-2">
                <span className="material-symbols-outlined text-[14px] text-outline">inventory_2</span>
                <select value={productId} onChange={(e) => setProductId(e.target.value)}
                  className="bg-surface-container border border-outline-variant/20 rounded-lg px-2.5 py-1.5 text-[11px] text-on-surface font-medium focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all">
                  <option value="">Select product</option>
                  {availableProducts.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
                </select>
              </div>
              <div className="flex items-center gap-2">
                {creditBalance !== null && (
                  <div className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-label-xs font-semibold ${
                    creditBalance <= 0 ? "bg-danger-red/10 text-danger-red" : "bg-surface-container text-on-surface-variant"
                  }`}>
                    <span className="material-symbols-outlined text-[14px]">token</span>
                    {creditBalance} Credits
                  </div>
                )}
                <button
                  onClick={() => setIsHistoryModalOpen(true)}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-primary/10 text-primary text-label-xs font-semibold hover:bg-primary/20 transition-colors"
                >
                  <span className="material-symbols-outlined text-[14px]">history</span>
                  History
                </button>
              </div>
              {insufficientCredits && (
                <span className="flex items-center gap-1 px-3 py-1.5 rounded-lg bg-danger-red/10 text-danger-red text-label-xs font-semibold">
                  <span className="material-symbols-outlined text-[12px]">error</span>
                  Insufficient Credits
                </span>
              )}
              {justGenerated && generatedId && (
                <button onClick={() => router.push(`/content/${generatedId}`)}
                  className="ml-auto px-3 py-1.5 rounded-lg bg-success-green text-white text-label-xs font-semibold hover:bg-success-green/90 transition-all active:scale-[0.97] flex items-center gap-1">
                  <span className="material-symbols-outlined text-[12px]">open_in_new</span>
                  View Post
                </button>
              )}
            </div>

            {/* Editable Form for Title & Caption */}
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm p-4 shrink-0 flex flex-col gap-3">
              <div className="flex items-center justify-between mb-1">
                <span className="text-label-sm font-semibold text-on-surface">Edit Generated Content</span>
                <button onClick={handleManualSave} disabled={isSaving || (!title && !content && !imageUrl && !isVideo)}
                  className="px-4 py-2 rounded-lg bg-primary text-on-primary text-label-xs font-semibold hover:bg-primary/90 transition-all active:scale-[0.97] disabled:opacity-50 flex items-center gap-1.5">
                  {isSaving ? (
                    <span className="w-3.5 h-3.5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  ) : (
                    <span className="material-symbols-outlined text-[14px]">save</span>
                  )}
                  Save Post
                </button>
              </div>
              <div className="grid grid-cols-[1fr,2fr] gap-3">
                <div>
                  <label className="text-label-xs text-on-surface-variant font-semibold mb-1 block">Title</label>
                  <input value={title} onChange={(e) => setTitle(e.target.value)}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-lg px-3 py-2 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all"
                    placeholder="Enter title..." />
                </div>
                <div>
                  <label className="text-label-xs text-on-surface-variant font-semibold mb-1 block">Caption</label>
                  <textarea value={content} onChange={(e) => setContent(e.target.value)}
                    className="w-full bg-surface-container border border-outline-variant/20 rounded-lg px-3 py-2 text-body-sm text-on-surface focus:border-primary/40 focus:ring-2 focus:ring-primary/5 outline-none transition-all resize-y min-h-[40px] max-h-[120px]"
                    placeholder="Generated caption will appear here..." />
                </div>
              </div>
            </div>

            {/* Post Preview */}
            <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm flex flex-col shrink-0 min-h-[600px]">
              <div className="p-3 border-b border-outline-variant/10 flex items-center gap-2 shrink-0">
                <span className="material-symbols-outlined text-[14px] text-outline">visibility</span>
                <span className="text-label-sm text-on-surface font-semibold mr-auto">Post Preview</span>
                <div className="flex gap-1">
                  {PLATFORMS.map((p) => (
                    <button key={p.value} onClick={() => setPlatform(p.value)}
                      className={`flex items-center gap-1.5 px-2 py-1.5 rounded-lg text-label-xs font-semibold transition-all ${
                        platform === p.value
                          ? "bg-surface-container text-on-surface shadow-sm"
                          : "text-outline/50 hover:bg-surface-container/50 hover:text-outline"
                      }`}>
                      <PlatformIcon platform={p.icon} />
                      <span className="hidden sm:inline">{p.label}</span>
                    </button>
                  ))}
                </div>
              </div>

              <div className="flex-1 overflow-y-auto p-4 bg-[#f0f2f5] flex items-start justify-center">
                <div className="w-full max-w-[500px] bg-white rounded-2xl border border-[#e4e6eb] shadow-sm overflow-hidden">
                  {/* Facebook Post */}
                  {platform.startsWith("facebook") && (
                    <div className="font-sans">
                      <div className="p-3.5 flex items-center gap-3">
                        <div className="w-9 h-9 rounded-full flex items-center justify-center text-white text-[12px] font-bold shrink-0"
                          style={{ background: getBrandColor(brandName) || "#1877F2" }}>
                          {brandName.charAt(0)}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-[13px] font-semibold text-[#1a1a1a] leading-tight">{brandName}</p>
                          <p className="text-[11px] text-[#65676b]">
                            {productName ? `Promoting ${productName} · ` : ""}Just now · <span className="material-symbols-outlined text-label-xs align-middle">public</span>
                          </p>
                        </div>
                        <span className="material-symbols-outlined text-[18px] text-[#65676b]">more_horiz</span>
                      </div>
                      {title && (
                        <p className="px-3.5 text-[15px] font-semibold text-[#1a1a1a] mb-1">{title}</p>
                      )}
                      <p className="px-3.5 text-[15px] text-[#1a1a1a] leading-[1.35] whitespace-pre-line mb-2.5">
                        {content || "Your AI-generated content will appear here..."}
                      </p>
                      {hashtags.length > 0 && (
                        <p className="px-3.5 text-[13px] text-[#216fdb] mb-2.5">
                          {hashtags.map((h) => `#${h}`).join(" ")}
                        </p>
                      )}
                      <div className="border-t border-b border-[#e4e6eb] bg-[#f0f2f5] aspect-video flex items-center justify-center overflow-hidden">
                        {imageUrl ? (
                          <img src={imageUrl} alt="Preview" className="w-full h-full object-cover" />
                        ) : (
                          <div className="flex flex-col items-center gap-2 text-[#c7c7c7]">
                            <span className="material-symbols-outlined text-4xl">landscape</span>
                            <span className="text-[11px]">Image placeholder</span>
                          </div>
                        )}
                      </div>
                      <div className="px-3.5 py-2 flex items-center gap-1 text-[13px] text-[#65676b]">
                        <svg viewBox="0 0 24 24" className="w-[18px] h-[18px]" fill="#65676b"><path d="M1 21h4V9H1v12zm22-11c0-1.1-.9-2-2-2h-6.31l.95-4.57.03-.32c0-.41-.17-.79-.44-1.06L14.17 1 7.59 7.59C7.22 7.95 7 8.45 7 9v10c0 1.1.9 2 2 2h9.33c.83 0 1.54-.5 1.84-1.22l3.02-7.05c.09-.23.14-.47.14-.73v-2z"/></svg>
                        <span className="font-semibold">Like</span>
                        <svg viewBox="0 0 24 24" className="w-[18px] h-[18px] ml-5" fill="#65676b"><path d="M12 2C6.48 2 2 6.48 2 12c0 5.52 4.48 10 10 10 5.52 0 10-4.48 10-10 0-5.52-4.48-10-10-10zm-2 15l-4 4V7c0-1.1.9-2 2-2h8c1.1 0 2 .9 2 2v8c0 1.1-.9 2-2 2h-6z"/></svg>
                        <span className="font-semibold">Comment</span>
                        <svg viewBox="0 0 24 24" className="w-[18px] h-[18px] ml-5" fill="#65676b"><path d="M21 12l-7-7v4c-7 1-10 5-11 11 2.5-3.5 6-5.5 11-5.5v4l7-7z"/></svg>
                        <span className="font-semibold">Share</span>
                      </div>
                    </div>
                  )}

                  {/* Instagram Post */}
                  {platform.startsWith("instagram") && (
                    <div className="font-sans bg-white">
                      <div className="p-3 flex items-center gap-2.5">
                        <div className="w-7 h-7 rounded-full bg-gradient-to-br from-purple-500 via-pink-500 to-orange-400 p-[2px]">
                          <div className="w-full h-full rounded-full bg-white flex items-center justify-center">
                            <span className="text-label-2xs font-bold" style={{ color: getBrandColor(brandName) || "#666" }}>{brandName.charAt(0)}</span>
                          </div>
                        </div>
                        <p className="text-[12px] font-semibold text-[#262626] flex-1">{brandName || "brand"}</p>
                        <span className="material-symbols-outlined text-[18px] text-[#262626]">more_horiz</span>
                      </div>
                      <div className="aspect-square bg-[#fafafa] flex items-center justify-center border-t border-b border-[#efefef] overflow-hidden">
                        {imageUrl ? (
                          <img src={imageUrl} alt="Preview" className="w-full h-full object-cover" />
                        ) : (
                          <div className="flex flex-col items-center gap-2 text-[#c7c7c7]">
                            <span className="material-symbols-outlined text-4xl">landscape</span>
                            <span className="text-[11px]">Instagram Post</span>
                          </div>
                        )}
                      </div>
                      <div className="p-3 space-y-1.5">
                        <div className="flex items-center gap-3">
                          <svg viewBox="0 0 24 24" className="w-[22px] h-[22px]" fill="#262626"><path d="M16.5 3C14.5 3 12.9 4.1 12 5.6 11.1 4.1 9.5 3 7.5 3 4.4 3 2 5.4 2 8.5c0 3.9 3.2 6.6 8.3 11.1l1.7 1.6 1.7-1.6C18.8 15.1 22 12.4 22 8.5 22 5.4 19.6 3 16.5 3z"/></svg>
                          <svg viewBox="0 0 24 24" className="w-[22px] h-[22px]" fill="#262626"><path d="M12 2C6.5 2 2 6.5 2 12c0 5.5 4.5 10 10 10s10-4.5 10-10c0-5.5-4.5-10-10-10zm5.5 12.5h-11v-1h11v1zm-2 3h-7v-1h7v1zm2-6h-11v-1h11v1z"/></svg>
                          <svg viewBox="0 0 24 24" className="w-[22px] h-[22px]" fill="#262626"><path d="M2 2v20l5-5h13V2H2zm18 13H6.5l-2.5 2.5V4h16v11z"/></svg>
                          <svg viewBox="0 0 24 24" className="w-[22px] h-[22px] ml-auto" fill="#262626"><path d="M17 3H7c-1.1 0-2 .9-2 2v14l5-3 5 3V5c0-1.1-.9-2-2-2z"/></svg>
                        </div>
                        <p className="text-[12px] font-semibold text-[#262626]">{brandName ? `${brandName.toLowerCase().replace(/\s+/g, "")} ` : ""}<span className="font-normal whitespace-pre-line">{content || "Write a caption..."}</span></p>
                        {hashtags.length > 0 && (
                          <p className="text-[12px] text-[#00376b]">{hashtags.map((h) => `#${h}`).join(" ")}</p>
                        )}
                        <p className="text-label-xs text-[#8e8e8e] uppercase tracking-wide">View all comments</p>
                      </div>
                    </div>
                  )}

                  {/* TikTok / dark-themed post */}
                  {platform.startsWith("tiktok") && (
                    <div className="font-sans bg-[#111111] text-white relative overflow-hidden">
                      <div className="aspect-[9/16] flex items-center justify-center relative overflow-hidden">
                        {imageUrl ? (
                          <img src={imageUrl} alt="Preview" className="absolute inset-0 w-full h-full object-cover opacity-80" />
                        ) : (
                          <div className="absolute inset-0 bg-gradient-to-br from-gray-800 to-gray-900 flex items-center justify-center">
                            <svg viewBox="0 0 24 24" className="w-[48px] h-[48px]" fill="rgba(255,255,255,0.3)"><path d="M10 16.5V8h7v2h-5v6.5a3.5 3.5 0 1 1-2-3.2z"/></svg>
                          </div>
                        )}
                        <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/70 to-transparent p-4 pt-12">
                          <div className="flex items-center gap-2 mb-2">
                            <div className="w-8 h-8 rounded-full bg-gradient-to-br flex items-center justify-center text-label-xs font-bold shrink-0 border border-white/30"
                              style={{ background: `linear-gradient(135deg, ${getBrandColor(brandName) || "#666"}, ${getBrandColor(brandName) || "#999"}` }}>
                              {brandName.charAt(0)}
                            </div>
                            <p className="text-[13px] font-semibold">@{brandName?.toLowerCase().replace(/\s+/g, "") || "brand"}</p>
                          </div>
                          <p className="text-[12px] leading-relaxed whitespace-pre-line">{content || "Add a caption..."}</p>
                          {hashtags.length > 0 && (
                            <p className="text-[12px] text-[#00acee] mt-0.5">{hashtags.map((h) => `#${h}`).join(" ")}</p>
                          )}
                          <div className="flex items-center gap-1.5 mt-1.5 text-[11px] text-white/60">
                            <svg viewBox="0 0 24 24" className="w-[14px] h-[14px]" fill="rgba(255,255,255,0.6)"><path d="M9 3v10.5a4.5 4.5 0 1 0 2-3.8V7h7V3H9z"/></svg>
                            <span>original sound - {brandName || "Creator"}</span>
                          </div>
                        </div>
                        <div className="absolute bottom-4 right-3 flex flex-col items-center gap-3">
                          <div className="flex flex-col items-center gap-0.5">
                            <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center backdrop-blur">
                              <svg viewBox="0 0 24 24" className="w-[20px] h-[20px]" fill="white"><path d="M16.5 3C14.5 3 12.9 4.1 12 5.6 11.1 4.1 9.5 3 7.5 3 4.4 3 2 5.4 2 8.5c0 3.9 3.2 6.6 8.3 11.1l1.7 1.6 1.7-1.6C18.8 15.1 22 12.4 22 8.5 22 5.4 19.6 3 16.5 3z"/></svg>
                            </div>
                            <span className="text-label-xs">12.4K</span>
                          </div>
                          <div className="flex flex-col items-center gap-0.5">
                            <div className="w-10 h-10 rounded-full bg-white/10 flex items-center justify-center backdrop-blur">
                              <svg viewBox="0 0 24 24" className="w-[20px] h-[20px]" fill="white"><path d="M12 2C6.5 2 2 6.5 2 12c0 5.5 4.5 10 10 10s10-4.5 10-10c0-5.5-4.5-10-10-10zm5.5 12.5h-11v-1h11v1zm-2 3h-7v-1h7v1zm2-6h-11v-1h11v1z"/></svg>
                            </div>
                            <span className="text-label-xs">834</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Right Column: AI Assistant Chat */}
          <div className="w-[320px] shrink-0 bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-sm flex flex-col overflow-hidden">
            <div className="p-4 border-b border-outline-variant/10 space-y-1.5 shrink-0">
              <div className="flex items-center gap-2">
                <span className="material-symbols-outlined text-[16px] text-primary">psychology</span>
                <h2 className="text-label-md font-semibold text-on-surface">AI Assistant</h2>
              </div>
              <div className="flex items-center gap-1.5 text-label-xs text-outline/60">
                <span className="px-1.5 py-0.5 rounded bg-surface-container text-on-surface-variant font-medium">{brandName}</span>
                {productName && (
                  <span className="px-1.5 py-0.5 rounded bg-surface-container text-on-surface-variant font-medium">{productName}</span>
                )}
              </div>
            </div>

            <div className="flex-1 overflow-y-auto p-3 space-y-3">
              {messages.length === 0 && !isGenerating ? (
                <div className="flex flex-col items-center justify-center h-full text-center px-4">
                  <div className="w-12 h-12 rounded-2xl bg-primary/10 flex items-center justify-center mb-3">
                    <span className="material-symbols-outlined text-primary text-2xl">auto_awesome</span>
                  </div>
                  <p className="text-body-sm text-on-surface font-medium">Ask the AI assistant</p>
                  <p className="text-label-sm text-outline/50 mt-1">Describe what content you need and let AI generate it</p>
                </div>
              ) : (
                messages.map((msg) => (
                  <div key={msg.id} className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}>
                    <div className={`max-w-[85%] rounded-xl px-3 py-2 ${
                      msg.role === "user"
                        ? "bg-primary/10 text-on-surface"
                        : "bg-surface-container text-on-surface"
                    }`}>
                      {msg.role === "assistant" && msg.canApply && (
                        <div className="flex items-center gap-1.5 mb-1">
                          <span className="material-symbols-outlined text-[12px] text-primary">auto_awesome</span>
                          <span className="text-label-2xs font-semibold text-primary uppercase tracking-wider">AI</span>
                        </div>
                      )}
                      {msg.text.includes("[IMAGE:") || msg.text.includes("[VIDEO_JOB:") ? (
                        <>
                          <p className="text-[12px] leading-relaxed whitespace-pre-line">
                            {msg.text.replace(/\[IMAGE:\s*.+?\]/g, '').replace(/\[VIDEO_JOB:\s*.+?\]/g, '').trim()}
                          </p>
                          {(() => {
                            const imgMatch = msg.text.match(/\[IMAGE:\s*(.+?)\]/);
                            if (imgMatch) return <img src={imgMatch[1]} alt="Generated" className="mt-2 rounded-lg max-w-[200px] border border-outline-variant/20" />;
                            const vidMatch = msg.text.match(/\[VIDEO_JOB:\s*(.+?)\]/);
                            if (vidMatch) return <div className="mt-2 p-2 rounded-lg bg-primary/10 border border-primary/20 text-[11px] text-primary flex items-center gap-1.5"><span className="material-symbols-outlined text-[14px]">movie</span><span>Đang tạo video (Job: {vidMatch[1]}). Vui lòng kiểm tra lại sau tại danh sách Content.</span></div>;
                            return null;
                          })()}
                        </>
                      ) : (
                        <p className="text-[12px] leading-relaxed whitespace-pre-line">{msg.text}</p>
                      )}
                      {msg.role === "assistant" && (
                        <button onClick={() => handleApplyVariation({ id: msg.id, prompt: "", result: msg.text })}
                          className="mt-1.5 text-label-xs font-semibold text-primary hover:text-primary/80 transition-colors flex items-center gap-0.5">
                          <span className="material-symbols-outlined text-label-xs">check</span>
                          Apply to editor
                        </button>
                      )}
                    </div>
                  </div>
                ))
              )}
              {isGenerating && (
                <div className="flex justify-start">
                  <div className="bg-surface-container rounded-xl px-4 py-3">
                    <div className="flex items-center gap-1.5">
                      <span className="w-1.5 h-1.5 rounded-full bg-primary/40 animate-bounce" style={{ animationDelay: "0ms" }} />
                      <span className="w-1.5 h-1.5 rounded-full bg-primary/40 animate-bounce" style={{ animationDelay: "150ms" }} />
                      <span className="w-1.5 h-1.5 rounded-full bg-primary/40 animate-bounce" style={{ animationDelay: "300ms" }} />
                    </div>
                  </div>
                </div>
              )}
              <div ref={chatEndRef} />
            </div>

            <div className="border-t border-outline-variant/10 p-3 shrink-0">
              <div className="flex items-center gap-2 bg-surface-container border border-outline-variant/20 rounded-xl px-3 py-2 focus-within:border-primary/40 focus-within:ring-2 focus-within:ring-primary/5 transition-all">
                <input value={chatInput} onChange={(e) => setChatInput(e.target.value)}
                  onKeyDown={(e) => { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); handleSendChat(); } }}
                  className="flex-1 bg-transparent border-none outline-none text-body-sm text-on-surface placeholder:text-outline/30"
                  placeholder="Ask AI to generate content..." disabled={isGenerating} />
                <button onClick={handleSendChat} disabled={!chatInput.trim() || isGenerating}
                  className="w-7 h-7 rounded-lg bg-primary text-on-primary flex items-center justify-center disabled:opacity-40 disabled:cursor-not-allowed hover:bg-primary/90 transition-all active:scale-[0.95]">
                  <span className="material-symbols-outlined text-[14px]">send</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </main>

      <CreditUsageHistoryModal
        isOpen={isHistoryModalOpen}
        onClose={() => setIsHistoryModalOpen(false)}
      />
    </>
  );
}
