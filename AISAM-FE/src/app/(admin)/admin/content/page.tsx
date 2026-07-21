"use client";

import { useEffect, useState, useCallback } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminDataTable from "@/components/admin/AdminDataTable";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminContent, setContentStatus, deleteContent, AdminContent } from "@/services/adminService";

const contentStatusLabels: Record<number, string> = { 0: "Draft", 1: "Pending", 2: "Approved", 3: "Rejected", 4: "Published", 5: "Flagged" };

export default function AdminContentPage() {
  const [contents, setContents] = useState<AdminContent[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState<number | "all">("all");
  const [selectedContent, setSelectedContent] = useState<any>(null);

  const loadContent = useCallback(async () => {
    setLoading(true);
    const data = await fetchAdminContent(page, 20, search || undefined, filterStatus === "all" ? undefined : filterStatus);
    if (data) {
      setContents(data.items);
      setTotal(data.total);
    }
    setLoading(false);
  }, [page, search, filterStatus]);

  useEffect(() => { loadContent(); }, [loadContent]);

  const handleStatusChange = async (id: string, newStatus: number) => {
    const ok = await setContentStatus(id, newStatus);
    if (ok) loadContent();
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Force delete this content? This cannot be undone.")) return;
    const ok = await deleteContent(id);
    if (ok) loadContent();
  };

  const columns = [
    { key: "title", header: "Title" },
    {
      key: "isAiGenerated",
      header: "AI Generated",
      render: (c: AdminContent) => <StatusBadge status={c.isAiGenerated ? "Yes" : "No"} variant={c.isAiGenerated ? "info" : "neutral"} />,
    },
    {
      key: "status",
      header: "Status",
      render: (c: AdminContent) => {
        const label = contentStatusLabels[c.status] ?? "Unknown";
        const variant = c.status === 2 || c.status === 4 ? "success" : c.status === 5 || c.status === 3 ? "error" : c.status === 1 ? "warning" : "neutral";
        return <StatusBadge status={label} variant={variant} />;
      },
    },
    {
      key: "createdAt",
      header: "Created",
      render: (c: AdminContent) => new Date(c.createdAt).toLocaleDateString(),
    },
    {
      key: "actions",
      header: "Actions",
      render: (c: AdminContent) => (
        <div className="flex items-center gap-2">
          {c.status !== 5 ? (
            <button
              onClick={(e) => { e.stopPropagation(); if(confirm("Flag this content for moderation?")) handleStatusChange(c.id, 5); }}
              className="text-xs px-2 py-1 rounded bg-amber-50 hover:bg-amber-100 text-amber-700 transition-colors"
            >
              Flag Content
            </button>
          ) : (
            <button
              onClick={(e) => { e.stopPropagation(); handleStatusChange(c.id, 1); }}
              className="text-xs px-2 py-1 rounded bg-gray-100 hover:bg-gray-200 text-gray-700 transition-colors"
            >
              Unflag (Set Pending)
            </button>
          )}
          <button
            onClick={(e) => { e.stopPropagation(); handleDelete(c.id); }}
            className="text-xs px-2 py-1 rounded bg-red-50 hover:bg-red-100 text-red-600 transition-colors"
          >
            Force Delete
          </button>
        </div>
      ),
    },
  ];

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Content Moderation" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Content Moderation Queue</h2>
          <p className="text-gray-500 mt-1">{total} total content items</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <input type="text" value={search} onChange={(e) => setSearch(e.target.value)} onKeyDown={(e) => { if (e.key === "Enter") loadContent(); }} placeholder="Search content..." className="w-64 rounded-lg border border-gray-300 px-3 py-2 text-sm" />
          <select 
            value={filterStatus} 
            onChange={(e) => setFilterStatus(e.target.value === "all" ? "all" : parseInt(e.target.value))}
            className="rounded-lg border border-gray-300 px-3 py-2 text-sm bg-white"
          >
            <option value="all">All Statuses</option>
            <option value={5}>Flagged</option>
            <option value={1}>Pending</option>
            <option value={2}>Approved</option>
            <option value={3}>Rejected</option>
            <option value={4}>Published</option>
            <option value={0}>Draft</option>
          </select>
          <button onClick={loadContent} className="px-3 py-2 text-sm rounded-lg bg-gray-100 hover:bg-gray-200">Search</button>
        </div>

        {loading ? (
          <div className="space-y-3">{[...Array(5)].map((_, i) => <div key={i} className="h-12 bg-gray-100 rounded animate-pulse" />)}</div>
        ) : (
          <>
            <AdminDataTable columns={columns} data={contents} keyField="id" onRowClick={(c) => setSelectedContent(c)} />
            <div className="flex items-center justify-between">
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Previous</button>
              <span className="text-sm text-gray-500">Page {page}</span>
              <button onClick={() => setPage((p) => p + 1)} disabled={page * 20 >= total} className="px-4 py-2 text-sm rounded-lg border border-gray-200 disabled:opacity-50">Next</button>
            </div>
          </>
        )}
      </main>

      {/* Content Detail Modal */}
      {selectedContent && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50" onClick={() => setSelectedContent(null)}>
          <div className="bg-white rounded-xl shadow-lg max-w-2xl w-full p-6 space-y-4 max-h-[90vh] flex flex-col" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between pb-3 border-b border-gray-100">
              <h3 className="text-xl font-semibold text-gray-900">{selectedContent.title || "Untitled Content"}</h3>
              <button onClick={() => setSelectedContent(null)} className="text-gray-400 hover:text-gray-600 text-2xl leading-none">&times;</button>
            </div>
            <div className="flex gap-4 text-sm text-gray-500">
              <div className="flex items-center gap-2">
                <span className="font-medium">Status:</span>
                <span>{contentStatusLabels[selectedContent.status] ?? "Unknown"}</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="font-medium">Created:</span>
                <span>{new Date(selectedContent.createdAt).toLocaleDateString()}</span>
              </div>
            </div>
            <div className="mt-4 flex-1 overflow-y-auto space-y-4">
              <div className="p-4 bg-gray-50 rounded-lg whitespace-pre-wrap text-sm text-gray-700 font-mono">
                {selectedContent.textContent || "No text content."}
              </div>
              {selectedContent.videoUrl && (
                <div>
                  <h4 className="font-semibold text-sm mb-2 text-gray-900">Video</h4>
                  <video src={selectedContent.videoUrl} controls className="max-w-full rounded-lg max-h-64 border border-gray-200" />
                </div>
              )}
              {selectedContent.imageUrl && (
                <div>
                  <h4 className="font-semibold text-sm mb-2 text-gray-900">Images</h4>
                  <div className="flex gap-2 overflow-x-auto pb-2">
                    {(() => {
                      let imgs: string[] = [];
                      try {
                        imgs = selectedContent.imageUrl.startsWith("[") ? JSON.parse(selectedContent.imageUrl) : [selectedContent.imageUrl];
                      } catch {}
                      return imgs.map((img, idx) => (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img key={idx} src={img} alt={`Image ${idx}`} className="h-32 object-cover rounded-md border border-gray-200" />
                      ));
                    })()}
                  </div>
                </div>
              )}
            </div>
            <div className="pt-4 border-t border-gray-100 flex justify-end">
              <button onClick={() => setSelectedContent(null)} className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50">Close</button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
