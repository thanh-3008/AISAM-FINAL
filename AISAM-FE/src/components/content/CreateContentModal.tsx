"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { holidayService, HolidayEventDto } from "@/services/holidayService";
import { fetchBrands } from "@/services/brandService";
import { useWorkspaces } from "@/hooks/useWorkspaces";

interface CreateContentModalProps {
  onClose: () => void;
}

export function CreateContentModal({ onClose }: CreateContentModalProps) {
  const router = useRouter();
  const { activeWorkspace } = useWorkspaces();
  const [step, setStep] = useState<"choose_path" | "holiday">("choose_path");

  // Holiday state
  const [upcomingHolidays, setUpcomingHolidays] = useState<HolidayEventDto[]>([]);
  const [brands, setBrands] = useState<{ id: string; name: string }[]>([]);
  const [selectedBrandId, setSelectedBrandId] = useState<string>("");
  const [loadingData, setLoadingData] = useState(true);
  
  // Custom Event state
  const [customEventName, setCustomEventName] = useState("");
  const [selectedHolidayId, setSelectedHolidayId] = useState<string | null>(null);

  // Generation state
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (step === "holiday" && activeWorkspace) {
      loadHolidayData();
    }
  }, [step, activeWorkspace]);

  const loadHolidayData = async () => {
    try {
      setLoadingData(true);
      if (!activeWorkspace) return;
      const [holidays, fetchedBrands] = await Promise.all([
        holidayService.getUpcoming(activeWorkspace.id, 14),
        fetchBrands()
      ]);
      setUpcomingHolidays(holidays);
      setBrands(fetchedBrands);
      if (fetchedBrands && fetchedBrands.length > 0) {
        setSelectedBrandId(fetchedBrands[0].id);
      }
    } catch (err: any) {
      console.error("Failed to load holiday data:", err);
    } finally {
      setLoadingData(false);
    }
  };

  const handleGenerateHoliday = async (isVideo: boolean) => {
    if (!activeWorkspace || !selectedBrandId) {
      setError("Please select a brand.");
      return;
    }
    
    // Validate custom vs predefined
    if (!selectedHolidayId && !customEventName.trim()) {
      setError("Please select a holiday or enter a custom event.");
      return;
    }

    try {
      setGenerating(true);
      setError(null);
      let result;

      if (selectedHolidayId) {
        if (isVideo) {
          result = await holidayService.generateVideo(activeWorkspace.id, selectedHolidayId, { brandId: selectedBrandId });
        } else {
          result = await holidayService.suggestCaption(activeWorkspace.id, selectedHolidayId, { brandId: selectedBrandId });
        }
      } else {
        result = await holidayService.suggestCustomEvent(activeWorkspace.id, {
          brandId: selectedBrandId,
          eventName: customEventName,
          adType: isVideo ? 2 : 0 // 0 = TextOnly, 2 = VideoText
        });
      }

      if (result && result.id) {
        onClose();
        router.push(`/content/${result.id}`);
      } else {
        setError("Generation succeeded but no content ID was returned.");
      }
    } catch (err: any) {
      setError(err?.message || "Failed to generate content. Please try again.");
    } finally {
      setGenerating(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="bg-surface-container-lowest rounded-2xl border border-outline-variant shadow-lg p-6 w-full max-w-2xl mx-4 animate-in fade-in zoom-in-95 duration-200 max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between mb-6 border-b border-outline-variant/10 pb-4">
          <div>
            <h3 className="text-headline-sm text-on-surface font-semibold flex items-center gap-2">
              <span className="material-symbols-outlined text-primary text-[24px]">add_circle</span>
              {step === "choose_path" ? "Create New Content" : "Create from Holiday / Event"}
            </h3>
            <p className="text-body-sm text-on-surface-variant mt-1">
              {step === "choose_path" ? "How would you like to start?" : "AI will generate content based on the selected event."}
            </p>
          </div>
          <button onClick={onClose} className="w-8 h-8 flex items-center justify-center rounded-full hover:bg-surface-container transition-colors">
            <span className="material-symbols-outlined text-[20px] text-outline">close</span>
          </button>
        </div>

        {step === "choose_path" && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <button onClick={() => { onClose(); router.push("/content/create"); }}
              className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-outline-variant/20 bg-surface-container-lowest hover:border-primary/40 hover:bg-surface-container/50 transition-all text-center group">
              <div className="w-14 h-14 rounded-full bg-primary/10 flex items-center justify-center group-hover:scale-110 transition-transform">
                <span className="material-symbols-outlined text-primary text-[28px]">edit_document</span>
              </div>
              <div>
                <h4 className="text-label-lg font-semibold text-on-surface mb-1">Manual Creation</h4>
                <p className="text-body-sm text-outline">Write and format your content from scratch</p>
              </div>
            </button>

            <button onClick={() => { onClose(); router.push("/content/ai-generate"); }}
              className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-outline-variant/20 bg-surface-container-lowest hover:border-primary/40 hover:bg-surface-container/50 transition-all text-center group">
              <div className="w-14 h-14 rounded-full bg-emerald-500/10 flex items-center justify-center group-hover:scale-110 transition-transform">
                <span className="material-symbols-outlined text-emerald-500 text-[28px]">smart_toy</span>
              </div>
              <div>
                <h4 className="text-label-lg font-semibold text-on-surface mb-1">AI from Product</h4>
                <p className="text-body-sm text-outline">Generate marketing posts based on product details</p>
              </div>
            </button>

            <button onClick={() => setStep("holiday")}
              className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-outline-variant/20 bg-surface-container-lowest hover:border-primary/40 hover:bg-surface-container/50 transition-all text-center group">
              <div className="w-14 h-14 rounded-full bg-tertiary/10 flex items-center justify-center group-hover:scale-110 transition-transform relative overflow-hidden">
                 <div className="absolute inset-0 bg-gradient-to-tr from-tertiary/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
                <span className="material-symbols-outlined text-tertiary text-[28px]">celebration</span>
              </div>
              <div>
                <h4 className="text-label-lg font-semibold text-on-surface mb-1">Holiday & Event</h4>
                <p className="text-body-sm text-outline">Create content for upcoming holidays or custom events</p>
              </div>
            </button>
          </div>
        )}

        {step === "holiday" && (
          <div className="space-y-6">
            {loadingData ? (
              <div className="h-40 flex items-center justify-center bg-surface-container/30 rounded-xl">
                <span className="material-symbols-outlined text-outline/30 animate-spin text-[32px]">refresh</span>
              </div>
            ) : (
              <>
                <div className="space-y-4">
                  {/* Brand Selector */}
                  <div>
                    <label className="text-label-sm text-on-surface-variant font-semibold mb-1.5 block">Brand <span className="text-danger-red">*</span></label>
                    <select
                      value={selectedBrandId}
                      onChange={(e) => setSelectedBrandId(e.target.value)}
                      className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-4 py-2.5 text-body-sm text-on-surface focus:border-primary/40 outline-none"
                    >
                      {brands.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
                    </select>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* Predefined Holidays */}
                    <div className={`p-4 rounded-xl border-2 transition-all cursor-pointer ${selectedHolidayId !== null ? "border-primary bg-primary/5" : "border-outline-variant/20 hover:border-outline-variant/50"}`}
                         onClick={() => { setSelectedHolidayId(upcomingHolidays[0]?.id || ""); setCustomEventName(""); }}>
                      <div className="flex items-center gap-2 mb-3">
                        <span className={`material-symbols-outlined ${selectedHolidayId !== null ? "text-primary" : "text-outline"}`}>event</span>
                        <h4 className="text-label-md font-semibold text-on-surface">Upcoming Holiday</h4>
                      </div>
                      
                      {upcomingHolidays.length > 0 ? (
                        <select 
                          value={selectedHolidayId || ""}
                          onChange={(e) => { setSelectedHolidayId(e.target.value); setCustomEventName(""); }}
                          onClick={(e) => e.stopPropagation()}
                          className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-3 py-2 text-label-sm text-on-surface outline-none"
                        >
                          {upcomingHolidays.map(h => (
                            <option key={h.id} value={h.id}>{h.localName || h.name} ({new Date(h.exactDate).toLocaleDateString("vi-VN")})</option>
                          ))}
                        </select>
                      ) : (
                        <p className="text-label-sm text-outline italic">No upcoming holidays in the next 14 days.</p>
                      )}
                    </div>

                    {/* Custom Event */}
                    <div className={`p-4 rounded-xl border-2 transition-all cursor-pointer ${selectedHolidayId === null ? "border-tertiary bg-tertiary/5" : "border-outline-variant/20 hover:border-outline-variant/50"}`}
                         onClick={() => setSelectedHolidayId(null)}>
                      <div className="flex items-center gap-2 mb-3">
                        <span className={`material-symbols-outlined ${selectedHolidayId === null ? "text-tertiary" : "text-outline"}`}>celebration</span>
                        <h4 className="text-label-md font-semibold text-on-surface">Custom Event</h4>
                      </div>
                      
                      <input
                        type="text"
                        placeholder="e.g. Kỷ niệm 5 năm..."
                        value={customEventName}
                        onChange={(e) => { setCustomEventName(e.target.value); setSelectedHolidayId(null); }}
                        onClick={(e) => e.stopPropagation()}
                        className="w-full bg-surface-container border border-outline-variant/20 rounded-xl px-3 py-2 text-label-sm text-on-surface outline-none focus:border-tertiary/50"
                      />
                    </div>
                  </div>
                </div>

                {error && (
                  <div className="p-3 bg-red-50 border border-red-100 rounded-xl flex items-start gap-2">
                    <span className="material-symbols-outlined text-red-500 text-[18px]">error</span>
                    <span className="text-label-sm text-red-600">{error}</span>
                  </div>
                )}

                <div className="flex items-center justify-between pt-4 border-t border-outline-variant/10">
                  <button onClick={() => setStep("choose_path")} className="px-4 py-2 text-label-sm font-semibold text-on-surface-variant hover:bg-surface-container rounded-xl transition-colors">
                    Back
                  </button>
                  <div className="flex items-center gap-2">
                    <button 
                      onClick={() => handleGenerateHoliday(false)}
                      disabled={generating}
                      className="px-5 py-2.5 rounded-xl bg-primary text-on-primary text-label-sm font-semibold hover:shadow-lg active:scale-[0.97] transition-all flex items-center gap-2 disabled:opacity-50"
                    >
                      {generating ? <span className="material-symbols-outlined text-[16px] animate-spin">refresh</span> : <span className="material-symbols-outlined text-[16px]">edit_document</span>}
                      Generate Post
                    </button>
                    <button 
                      onClick={() => handleGenerateHoliday(true)}
                      disabled={generating}
                      className="px-5 py-2.5 rounded-xl bg-surface-container-high text-on-surface text-label-sm font-semibold hover:bg-outline-variant/30 active:scale-[0.97] transition-all flex items-center gap-2 disabled:opacity-50"
                    >
                      {generating ? <span className="material-symbols-outlined text-[16px] animate-spin">refresh</span> : <span className="material-symbols-outlined text-[16px]">movie</span>}
                      Generate Video
                    </button>
                  </div>
                </div>
              </>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
