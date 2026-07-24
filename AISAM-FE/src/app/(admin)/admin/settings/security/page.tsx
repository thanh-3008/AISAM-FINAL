"use client";

import { useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import { changePassword } from "@/services/profileSettingsService";
import { useToast } from "@/contexts/ToastContext";

export default function AdminSecuritySettingsPage() {
  const [passwordForm, setPasswordForm] = useState({ currentPassword: "", newPassword: "", confirmPassword: "" });
  const [changingPassword, setChangingPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showPasswords, setShowPasswords] = useState({ current: false, new: false, confirm: false });
  const { showToast } = useToast();

  const handleUpdatePassword = async () => {
    if (!passwordForm.currentPassword || !passwordForm.newPassword || !passwordForm.confirmPassword) {
      setError("All password fields are required");
      return;
    }
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setError("New passwords do not match");
      return;
    }
    if (passwordForm.newPassword.length < 8) {
      setError("Password must be at least 8 characters");
      return;
    }
    
    setChangingPassword(true);
    setError(null);
    try {
      const success = await changePassword({
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
        confirmPassword: passwordForm.confirmPassword,
      });
      if (success) {
        setPasswordForm({ currentPassword: "", newPassword: "", confirmPassword: "" });
        setError(null);
        showToast({ type: "success", title: "Password changed", message: "Your password has been updated successfully." });
      } else {
        setError("Failed to change password. Please check your current password.");
      }
    } catch (err: any) {
      setError(err?.message || "Network error. Please check your connection");
    } finally {
      setChangingPassword(false);
    }
  };

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Settings", href: "/admin/settings" }, { label: "Security" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Security Settings</h2>
          <p className="text-gray-500 mt-1">Manage your account security and password.</p>
        </div>
        
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 space-y-6 max-w-2xl">
          <div>
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Change Password</h3>
            
            {error && (
              <div className="mb-4 p-3 bg-red-50 text-red-600 rounded-lg text-sm">
                {error}
              </div>
            )}

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Current Password</label>
                <div className="relative mt-1">
                  <input 
                    type={showPasswords.current ? "text" : "password"}
                    className="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" 
                    value={passwordForm.currentPassword}
                    onChange={(e) => setPasswordForm({ ...passwordForm, currentPassword: e.target.value })}
                  />
                  <button 
                    type="button" 
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                    onClick={() => setShowPasswords(prev => ({ ...prev, current: !prev.current }))}
                  >
                    <span className="material-symbols-outlined text-sm">{showPasswords.current ? 'visibility_off' : 'visibility'}</span>
                  </button>
                </div>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700">New Password</label>
                <div className="relative mt-1">
                  <input 
                    type={showPasswords.new ? "text" : "password"}
                    className="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" 
                    value={passwordForm.newPassword}
                    onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })}
                  />
                  <button 
                    type="button" 
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                    onClick={() => setShowPasswords(prev => ({ ...prev, new: !prev.new }))}
                  >
                    <span className="material-symbols-outlined text-sm">{showPasswords.new ? 'visibility_off' : 'visibility'}</span>
                  </button>
                </div>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700">Confirm New Password</label>
                <div className="relative mt-1">
                  <input 
                    type={showPasswords.confirm ? "text" : "password"}
                    className="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" 
                    value={passwordForm.confirmPassword}
                    onChange={(e) => setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })}
                  />
                  <button 
                    type="button" 
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                    onClick={() => setShowPasswords(prev => ({ ...prev, confirm: !prev.confirm }))}
                  >
                    <span className="material-symbols-outlined text-sm">{showPasswords.confirm ? 'visibility_off' : 'visibility'}</span>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div className="border-t pt-6 flex items-center gap-3">
            <button 
              onClick={handleUpdatePassword} 
              disabled={changingPassword}
              className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
            >
              {changingPassword ? "Updating..." : "Update Password"}
            </button>
          </div>
        </div>
      </main>
    </>
  );
}
