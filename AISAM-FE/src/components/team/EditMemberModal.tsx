"use client";

import { useState, useEffect } from "react";
import { type TeamMember, type MemberRole } from "@/services/teamService";

interface EditMemberModalProps {
  member: TeamMember | null;
  onClose: () => void;
  onUpdate: (id: string, role: MemberRole) => void;
  isLoading: boolean;
}

export default function EditMemberModal({ member, onClose, onUpdate, isLoading }: EditMemberModalProps) {
  const [role, setRole] = useState<MemberRole>("Viewer");

  useEffect(() => {
    if (member) {
      setRole(member.role);
    }
  }, [member]);

  if (!member) return null;

  const handleSubmit = () => {
    onUpdate(member.id, role);
  };

  return (
    <>
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50" onClick={onClose} />
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4" onClick={onClose}>
        <div className="w-full max-w-md bg-surface-container-lowest rounded-2xl shadow-2xl max-h-[90vh] overflow-hidden flex flex-col" onClick={(e) => e.stopPropagation()}>
          <div className="p-6 border-b border-outline-variant/20 flex items-center justify-between shrink-0">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-secondary/10 text-secondary flex items-center justify-center">
                <span className="material-symbols-outlined text-[20px]">person_edit</span>
              </div>
              <div>
                <h2 className="text-headline-sm font-bold text-on-surface">Edit Member</h2>
                <p className="text-label-xs text-outline">{member.name}</p>
              </div>
            </div>
            <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors">
              <span className="material-symbols-outlined text-[18px]">close</span>
            </button>
          </div>
          <div className="p-6 space-y-5 overflow-y-auto flex-1">
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Email</label>
              <div className="p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm text-outline">
                {member.email}
              </div>
            </div>
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-1.5">Current Status</label>
              <div className="p-3 bg-surface-container-low border border-outline-variant/20 rounded-xl text-body-sm flex items-center gap-2">
                <span className={`w-2 h-2 rounded-full ${member.status === "Active" ? "bg-success-green" : member.status === "Pending" ? "bg-warning-amber" : "bg-outline"}`} />
                {member.status}
              </div>
            </div>
            <div>
              <label className="text-label-2xs text-outline uppercase font-bold tracking-widest block mb-2">Role</label>
              <div className="space-y-2">
                {(["Owner", "Manager", "ContentCreator", "Viewer"] as MemberRole[]).map((r) => (
                  <button
                    key={r}
                    type="button"
                    onClick={() => setRole(r)}
                    className={`w-full flex items-center gap-3 p-3 rounded-xl border-2 transition-all text-left ${
                      role === r
                        ? "border-primary bg-primary/5"
                        : "border-outline-variant/20 hover:border-outline-variant/40"
                    }`}
                  >
                    <div className={`w-8 h-8 rounded-lg flex items-center justify-center text-label-2xs font-bold ${
                      role === r ? "bg-primary text-on-primary" : "bg-surface-container-high text-outline"
                    }`}>
                      {r === "Owner" ? "👑" : r === "Manager" ? "⚙️" : r === "ContentCreator" ? "✏️" : "👁️"}
                    </div>
                    <div className="flex-1 min-w-0">
                      <span className="text-label-sm font-semibold text-on-surface">{r === "ContentCreator" ? "Content Creator" : r}</span>
                      <p className="text-label-xs text-outline">
                        {r === "Owner" && "Full access to everything"}
                        {r === "Manager" && "Can manage teams and members"}
                        {r === "ContentCreator" && "Can create and manage content"}
                        {r === "Viewer" && "Read-only access"}
                      </p>
                    </div>
                    {role === r && (
                      <span className="material-symbols-outlined text-primary text-[18px] shrink-0">check_circle</span>
                    )}
                  </button>
                ))}
              </div>
            </div>
          </div>
          <div className="p-6 border-t border-outline-variant/20 flex items-center justify-end gap-3 shrink-0">
            <button
              onClick={onClose}
              className="px-5 py-2.5 border border-outline-variant/20 rounded-xl text-label-sm font-semibold text-outline hover:text-on-surface hover:bg-surface-container transition-all"
            >
              Cancel
            </button>
            <button
              onClick={handleSubmit}
              disabled={isLoading}
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
