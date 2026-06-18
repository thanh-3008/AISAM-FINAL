"use client";

import { useState } from "react";
import { type Team, type CreateTeamData } from "@/services/teamService";
import { BRANDS } from "./teamUtils";

interface EditTeamModalProps {
  team: Team | null;
  onClose: () => void;
  onUpdate: (id: string, data: CreateTeamData) => void;
  isLoading: boolean;
}

export default function EditTeamModal({ team, onClose, onUpdate, isLoading }: EditTeamModalProps) {
  const [name, setName] = useState(team?.name ?? "");
  const [description, setDescription] = useState(team?.description ?? "");
  const [selectedBrands, setSelectedBrands] = useState<string[]>([]);

  if (!team) return null;

  const handleSubmit = () => {
    if (!name.trim()) return;
    onUpdate(team.id, {
      name: name.trim(),
      description: description.trim(),
      brandIds: selectedBrands,
      memberIds: team.memberIds,
    });
  };

  const toggleBrand = (id: string) => {
    setSelectedBrands((prev) => (prev.includes(id) ? prev.filter((b) => b !== id) : [...prev, id]));
  };

  const isValid = name.trim();

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-lg bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between sticky top-0 bg-surface-container-lowest z-10">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-secondary/10 text-secondary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">edit</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Edit Team</h2>
                <p className="text-label-xs text-outline">Update team information</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>
          <div className="p-6 space-y-5">
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Team Name</label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g., Creative Explorers"
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 placeholder:text-outline/40 transition-all"
              />
            </div>
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Description</label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                rows={3}
                placeholder="Describe the team's focus and goals..."
                className="w-full p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-on-surface outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary/30 placeholder:text-outline/40 resize-none transition-all"
              />
            </div>
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Assign Brands</label>
              <div className="grid grid-cols-2 gap-2">
                {BRANDS.map((brand) => (
                  <button
                    key={brand.id}
                    type="button"
                    onClick={() => toggleBrand(brand.id)}
                    className={`flex items-center gap-2 p-3 rounded-xl border-2 transition-all text-left ${
                      selectedBrands.includes(brand.id)
                        ? "border-primary bg-primary/5"
                        : "border-outline-variant/20 hover:border-outline-variant/40"
                    }`}
                  >
                    <div className={`w-8 h-8 rounded-lg flex items-center justify-center text-label-2xs font-bold ${
                      selectedBrands.includes(brand.id) ? "bg-primary text-on-primary" : "bg-surface-container-high text-outline"
                    }`}>
                      {brand.name.charAt(0)}
                    </div>
                    <span className="text-label-sm font-semibold text-on-surface">{brand.name}</span>
                  </button>
                ))}
              </div>
            </div>
          </div>
          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3 sticky bottom-0 bg-surface-container-lowest">
            <button
              onClick={onClose}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all"
            >
              Cancel
            </button>
            <button
              onClick={handleSubmit}
              disabled={!isValid || isLoading}
              className="px-6 py-2.5 bg-primary text-on-primary rounded-xl text-label-sm font-bold shadow-lg shadow-primary/20 hover:scale-105 transition-transform active:scale-95 disabled:opacity-50 disabled:hover:scale-100 flex items-center gap-2"
            >
              {isLoading ? (
                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              ) : (
                <span className="material-symbols-outlined text-[16px]">save</span>
              )}
              Save Changes
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
