"use client";

import { useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import { broadcastNotification } from "@/services/adminService";

export default function AdminBroadcastPage() {
  const [title, setTitle] = useState("");
  const [message, setMessage] = useState("");
  const [excludeAdmins, setExcludeAdmins] = useState(true);
  const [sending, setSending] = useState(false);
  const [result, setResult] = useState<string | null>(null);

  const handleSend = async () => {
    if (!title.trim() || !message.trim()) return;
    setSending(true);
    const ok = await broadcastNotification(title, message, excludeAdmins);
    setResult(ok ? "Notification sent successfully!" : "Failed to send notification.");
    setSending(false);
    if (ok) { setTitle(""); setMessage(""); }
  };

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Broadcast" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Broadcast Notification</h2>
          <p className="text-gray-500 mt-1">Send a system-wide notification to all users.</p>
        </div>

        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6 space-y-4 max-w-2xl">
          <div>
            <label className="block text-sm font-medium text-gray-700">Title</label>
            <input type="text" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="System Announcement" className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Message</label>
            <textarea value={message} onChange={(e) => setMessage(e.target.value)} placeholder="Enter notification message..." rows={4} className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm" />
          </div>
          <div className="flex items-center gap-3">
            <input type="checkbox" id="excludeAdmins" checked={excludeAdmins} onChange={(e) => setExcludeAdmins(e.target.checked)} className="rounded" />
            <label htmlFor="excludeAdmins" className="text-sm text-gray-700">Exclude admin users</label>
          </div>
          <div className="flex items-center gap-3 pt-2">
            <button onClick={handleSend} disabled={sending || !title.trim() || !message.trim()} className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors">
              {sending ? "Sending..." : "Send Broadcast"}
            </button>
            {result && <span className={`text-sm ${result.includes("success") ? "text-emerald-600" : "text-red-600"}`}>{result}</span>}
          </div>
        </div>
      </main>
    </>
  );
}
