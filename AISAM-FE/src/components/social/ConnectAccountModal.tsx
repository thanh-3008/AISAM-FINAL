import { useState } from "react";
import { PlatformIcon } from "@/lib/contentConstants";
import { type SocialPlatform } from "@/services/socialAccountService";
import { PLATFORM_INFO } from "./socialUtils";

interface ConnectAccountModalProps {
  open: boolean;
  onClose: () => void;
  onConnect: (platform: SocialPlatform) => void;
  isLoading: boolean;
}

export default function ConnectAccountModal({ open, onClose, onConnect, isLoading }: ConnectAccountModalProps) {
  const [selectedPlatform, setSelectedPlatform] = useState<SocialPlatform>("facebook");

  if (!open) return null;

  const handleConnect = () => {
    onConnect(selectedPlatform);
  };

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl" onClick={(e) => e.stopPropagation()}>
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">add_link</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Connect Account</h2>
                <p className="text-[10px] text-outline">Link your social media via OAuth</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>

          <div className="p-6 space-y-5">
            <div>
              <label className="text-[9px] text-outline uppercase font-bold tracking-widest block mb-2">Select Platform</label>
              <div className="grid grid-cols-3 gap-2">
                {(Object.keys(PLATFORM_INFO) as SocialPlatform[]).map((platform) => {
                  const info = PLATFORM_INFO[platform];
                  return (
                    <button key={platform} onClick={() => setSelectedPlatform(platform)}
                      className={`flex flex-col items-center gap-2 p-3 rounded-xl border-2 transition-all ${
                        selectedPlatform === platform
                          ? "border-primary bg-primary/5"
                          : "border-outline-variant/20 hover:border-outline-variant/40"
                      }`}>
                      <div className={`w-8 h-8 rounded-lg bg-gradient-to-br ${info.gradient} flex items-center justify-center text-white`}>
                        <PlatformIcon platform={platform} className="w-4 h-4" />
                      </div>
                      <span className="text-[10px] font-semibold text-on-surface">{info.label}</span>
                    </button>
                  );
                })}
              </div>
            </div>

            <div className="p-4 bg-primary/5 border border-primary/20 rounded-xl space-y-2">
              <p className="text-[11px] text-primary font-semibold flex items-center gap-2">
                <span className="material-symbols-outlined text-[16px]">security</span>
                Secure OAuth Authentication
              </p>
              <p className="text-[10px] text-on-surface-variant">
                You will be redirected to {PLATFORM_INFO[selectedPlatform].label} to authorize access. Your credentials are never stored on our servers.
              </p>
            </div>

            <div className="p-3 bg-surface-container-low rounded-xl">
              <p className="text-[10px] text-outline flex items-start gap-2">
                <span className="material-symbols-outlined text-[14px] shrink-0">info</span>
                After authorization, you can link pages/profiles from your {PLATFORM_INFO[selectedPlatform].label} account to publish content.
              </p>
            </div>
          </div>

          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3">
            <button onClick={onClose}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all">
              Cancel
            </button>
            <button onClick={handleConnect} disabled={isLoading}
              className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 disabled:opacity-50 disabled:hover:scale-100 flex items-center gap-2">
              {isLoading ? (
                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <span className="material-symbols-outlined text-[16px]">login</span>
              )}
              Connect with {PLATFORM_INFO[selectedPlatform].label}
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
