import React, { useState, useEffect } from 'react';
import { holidayService, HolidayEventDto } from '@/services/holidayService';
import { fetchBrands } from '@/services/brandService';
import { useWorkspaces } from '@/hooks/useWorkspaces';

interface HolidayBannerProps {
  onSuccess?: () => void;
}

export const HolidayBanner: React.FC<HolidayBannerProps> = ({ onSuccess }) => {
  const { activeWorkspace } = useWorkspaces();
  const [upcomingHolidays, setUpcomingHolidays] = useState<HolidayEventDto[]>([]);
  const [brands, setBrands] = useState<{ id: string; name: string }[]>([]);
  const [selectedBrandId, setSelectedBrandId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [generatingFor, setGeneratingFor] = useState<string | null>(null);
  const [generatingCustom, setGeneratingCustom] = useState(false);
  const [customEventName, setCustomEventName] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (activeWorkspace) {
      loadData();
    }
  }, [activeWorkspace]);

  const loadData = async () => {
    try {
      setLoading(true);
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
      console.error('Failed to load holiday data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSuggestCaption = async (holidayId: string, isVideo: boolean = false) => {
    if (!activeWorkspace || !selectedBrandId) {
      setError('Please select a brand first to suggest captions.');
      return;
    }
    try {
      setGeneratingFor(holidayId);
      setError(null);
      if (isVideo) {
        await holidayService.generateVideo(activeWorkspace.id, holidayId, { brandId: selectedBrandId });
      } else {
        await holidayService.suggestCaption(activeWorkspace.id, holidayId, { brandId: selectedBrandId });
      }
      if (onSuccess) onSuccess();
    } catch (err: any) {
      setError(err?.message || 'Failed to suggest content. Please try again.');
    } finally {
      setGeneratingFor(null);
    }
  };

  const handleCustomEvent = async (isVideo: boolean = false) => {
    if (!activeWorkspace || !selectedBrandId || !customEventName.trim()) {
      setError('Please select a brand and enter an event name.');
      return;
    }
    try {
      setGeneratingCustom(true);
      setError(null);
      await holidayService.suggestCustomEvent(activeWorkspace.id, {
        brandId: selectedBrandId,
        eventName: customEventName,
        adType: isVideo ? 2 : 0 // 0 = TextOnly, 2 = VideoText
      });
      setCustomEventName('');
      if (onSuccess) onSuccess();
    } catch (err: any) {
      setError(err?.message || 'Failed to generate custom event content.');
    } finally {
      setGeneratingCustom(false);
    }
  };

  if (loading) {
    return (
      <div className="mb-6 h-20 bg-surface-container/50 animate-pulse rounded-2xl border border-outline-variant/20 flex items-center justify-center">
        <span className="material-symbols-outlined text-outline/30 animate-spin">refresh</span>
      </div>
    );
  }

  const holiday = upcomingHolidays.length > 0 ? upcomingHolidays[0] : null;

  return (
    <div className="mb-6 overflow-hidden relative rounded-2xl border border-primary/20 bg-gradient-to-br from-primary/10 via-surface to-surface shadow-sm card-hover">
      <div className="absolute top-0 right-0 w-32 h-32 bg-primary/10 rounded-full blur-2xl pointer-events-none" />
      <div className="relative p-5 space-y-4">
        
        {/* Row 1: Upcoming Holiday */}
        {holiday && (
          <div className="flex flex-wrap items-center justify-between gap-4 border-b border-outline-variant/20 pb-4">
            <div className="flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-primary/20 flex items-center justify-center shrink-0">
                <span className="material-symbols-outlined text-primary text-[24px]">event</span>
              </div>
              <div>
                <h3 className="text-body-lg font-semibold text-on-surface mb-0.5 flex items-center gap-2">
                  Sắp tới: {holiday.localName || holiday.name}
                </h3>
                <p className="text-label-sm text-outline">
                  Ngày {new Date(holiday.exactDate).toLocaleDateString('vi-VN')}
                </p>
              </div>
            </div>
            
            <div className="flex flex-wrap items-center gap-3">
              {brands.length > 1 && (
                <select
                  value={selectedBrandId || ''}
                  onChange={(e) => setSelectedBrandId(e.target.value)}
                  className="px-3 py-2 rounded-xl border border-outline-variant/30 text-label-sm bg-surface text-on-surface outline-none focus:border-primary/50 transition-colors"
                >
                  {brands.map((b) => (
                    <option key={b.id} value={b.id}>{b.name}</option>
                  ))}
                </select>
              )}
              <div className="flex gap-2">
                <button 
                  onClick={() => handleSuggestCaption(holiday.id, false)}
                  disabled={generatingFor === holiday.id || !selectedBrandId}
                  className={`flex items-center gap-2 px-3 py-2 rounded-xl text-label-sm font-semibold transition-all ${
                    generatingFor === holiday.id || !selectedBrandId
                      ? 'bg-surface-container-highest text-outline cursor-not-allowed'
                      : 'bg-primary/10 text-primary hover:bg-primary/20'
                  }`}
                >
                  {generatingFor === holiday.id ? <span className="material-symbols-outlined text-[18px] animate-spin">refresh</span> : <span className="material-symbols-outlined text-[18px]">edit_document</span>}
                  Post
                </button>
                <button 
                  onClick={() => handleSuggestCaption(holiday.id, true)}
                  disabled={generatingFor === holiday.id || !selectedBrandId}
                  className={`flex items-center gap-2 px-3 py-2 rounded-xl text-label-sm font-semibold transition-all ${
                    generatingFor === holiday.id || !selectedBrandId
                      ? 'bg-surface-container-highest text-outline cursor-not-allowed'
                      : 'bg-primary text-on-primary hover:bg-primary/90 hover:-translate-y-0.5 shadow-sm'
                  }`}
                >
                  {generatingFor === holiday.id ? <span className="material-symbols-outlined text-[18px] animate-spin">refresh</span> : <span className="material-symbols-outlined text-[18px]">movie</span>}
                  Video
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Row 2: Custom Event */}
        <div className="hidden flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-4 flex-1">
            <div className="w-10 h-10 rounded-xl bg-tertiary/20 flex items-center justify-center shrink-0">
              <span className="material-symbols-outlined text-tertiary text-[20px]">celebration</span>
            </div>
            <div className="flex-1 max-w-sm">
              <input
                type="text"
                placeholder="Nhập tên sự kiện riêng (VD: Kỷ niệm 5 năm...)"
                value={customEventName}
                onChange={(e) => setCustomEventName(e.target.value)}
                className="w-full px-3 py-2 rounded-lg border border-outline-variant/30 text-label-sm bg-surface outline-none focus:border-tertiary/50"
              />
            </div>
          </div>
          <div className="flex flex-wrap items-center gap-3">
            {(!holiday && brands.length > 1) && (
              <select
                value={selectedBrandId || ''}
                onChange={(e) => setSelectedBrandId(e.target.value)}
                className="px-3 py-2 rounded-xl border border-outline-variant/30 text-label-sm bg-surface text-on-surface outline-none focus:border-tertiary/50 transition-colors"
              >
                {brands.map((b) => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
            )}
            <div className="flex gap-2">
              <button 
                onClick={() => handleCustomEvent(false)}
                disabled={generatingCustom || !selectedBrandId || !customEventName.trim()}
                className={`flex items-center gap-2 px-3 py-2 rounded-xl text-label-sm font-semibold transition-all ${
                  generatingCustom || !selectedBrandId || !customEventName.trim()
                    ? 'bg-surface-container-highest text-outline cursor-not-allowed'
                    : 'bg-tertiary/10 text-tertiary hover:bg-tertiary/20'
                }`}
              >
                {generatingCustom ? <span className="material-symbols-outlined text-[18px] animate-spin">refresh</span> : <span className="material-symbols-outlined text-[18px]">edit_document</span>}
                Post
              </button>
              <button 
                onClick={() => handleCustomEvent(true)}
                disabled={generatingCustom || !selectedBrandId || !customEventName.trim()}
                className={`flex items-center gap-2 px-3 py-2 rounded-xl text-label-sm font-semibold transition-all ${
                  generatingCustom || !selectedBrandId || !customEventName.trim()
                    ? 'bg-surface-container-highest text-outline cursor-not-allowed'
                    : 'bg-tertiary text-on-tertiary hover:bg-tertiary/90 hover:-translate-y-0.5 shadow-sm'
                }`}
              >
                {generatingCustom ? <span className="material-symbols-outlined text-[18px] animate-spin">refresh</span> : <span className="material-symbols-outlined text-[18px]">movie</span>}
                Video
              </button>
            </div>
          </div>
        </div>

      </div>
      {error && (
        <div className="px-5 pb-5">
          <div className="p-3 bg-red-50 border border-red-100 rounded-lg flex items-start gap-2">
            <span className="material-symbols-outlined text-red-500 text-[18px] mt-0.5">error</span>
            <span className="text-label-sm text-red-600">{error}</span>
          </div>
        </div>
      )}
    </div>
  );
};
