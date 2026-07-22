"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { apiClient } from "@/lib/apiClient";
import { deleteWorkspace } from "@/services/adminService";

const typeLabels: Record<number, string> = { 1: "Personal", 2: "Business" };
const statusLabels: Record<number, string> = { 0: "Active", 1: "Limited", 2: "Archived", 3: "Deleted" };

export default function AdminWorkspaceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [ws, setWs] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [selectedPost, setSelectedPost] = useState<any>(null);

  useEffect(() => {
    if (!id) return;
    apiClient(`/admin/workspaces/${id}`).then((res: any) => { setWs(res?.data); setLoading(false); });
  }, [id]);

  if (loading) return (
    <><AdminHeader breadcrumbs={[{ label: "Workspaces", href: "/admin/workspaces" }, { label: "Loading..." }]} /><main className="flex-1 p-8"><div className="animate-pulse h-64 bg-gray-100 rounded-xl" /></main></>
  );

  if (!ws) return (
    <><AdminHeader breadcrumbs={[{ label: "Workspaces", href: "/admin/workspaces" }, { label: "Not Found" }]} /><main className="flex-1 p-8"><p className="text-gray-500">Workspace not found.</p></main></>
  );

  const handleDelete = async () => {
    if (!confirm("Delete this workspace permanently?")) return;
    await deleteWorkspace(ws.id);
    router.push("/admin/workspaces");
  };

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Workspaces", href: "/admin/workspaces" }, { label: ws.name }]} />
      <main className="flex-1 p-8 space-y-6">
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Workspace Details</h3>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div><dt className="text-gray-500">Name</dt><dd className="font-medium text-gray-900">{ws.name}</dd></div>
            <div><dt className="text-gray-500">Type</dt><dd><StatusBadge status={typeLabels[ws.workspaceType] ?? "Unknown"} variant={ws.workspaceType === 2 ? "info" : "neutral"} /></dd></div>
            <div><dt className="text-gray-500">Status</dt><dd><StatusBadge status={statusLabels[ws.status] ?? "Unknown"} variant={ws.status === 0 ? "success" : ws.status === 1 ? "warning" : "error"} /></dd></div>
            <div><dt className="text-gray-500">Member Limit</dt><dd className="font-medium text-gray-900">{ws.memberLimit ?? "N/A"}</dd></div>
            <div><dt className="text-gray-500">Created</dt><dd className="font-medium text-gray-900">{new Date(ws.createdAt).toLocaleDateString()}</dd></div>
            <div><dt className="text-gray-500">ID</dt><dd className="font-mono text-xs text-gray-500">{ws.id}</dd></div>
          </dl>
        </div>

        {/* Members Section */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Members ({ws.members?.length || 0})</h3>
          {ws.members?.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="border-b border-gray-200 bg-gray-50">
                    <th className="px-4 py-3 font-medium text-gray-600 rounded-tl-lg">User</th>
                    <th className="px-4 py-3 font-medium text-gray-600">Email</th>
                    <th className="px-4 py-3 font-medium text-gray-600">Role</th>
                    <th className="px-4 py-3 font-medium text-gray-600">Status</th>
                    <th className="px-4 py-3 font-medium text-gray-600 rounded-tr-lg">Joined At</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {ws.members.map((m: any) => (
                    <tr key={m.userId} className="hover:bg-gray-50 transition-colors">
                      <td className="px-4 py-3 font-medium text-gray-900">{m.fullName}</td>
                      <td className="px-4 py-3 text-gray-500">{m.email}</td>
                      <td className="px-4 py-3 text-gray-500">{m.roleName}</td>
                      <td className="px-4 py-3 text-gray-500">{m.isActive ? "Active" : "Inactive"}</td>
                      <td className="px-4 py-3 text-gray-500">{new Date(m.joinedAt).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-sm text-gray-500">No members found.</p>
          )}
        </div>

        {/* Posts Section */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Recent Posts ({ws.posts?.length || 0})</h3>
          {ws.posts?.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="border-b border-gray-200 bg-gray-50">
                    <th className="px-4 py-3 font-medium text-gray-600 rounded-tl-lg">Title / Content</th>
                    <th className="px-4 py-3 font-medium text-gray-600">Status</th>
                    <th className="px-4 py-3 font-medium text-gray-600 rounded-tr-lg">Created At</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {ws.posts.map((p: any) => (
                    <tr 
                      key={p.id} 
                      className="hover:bg-gray-50 transition-colors cursor-pointer"
                      onClick={() => setSelectedPost(p)}
                    >
                      <td className="px-4 py-3 text-gray-900">
                        <div className="font-medium">{p.title || "Untitled"}</div>
                        <div className="text-xs text-gray-500 truncate max-w-md">{p.textContent || "-"}</div>
                      </td>
                      <td className="px-4 py-3 text-gray-500">{p.statusName}</td>
                      <td className="px-4 py-3 text-gray-500">{new Date(p.createdAt).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-sm text-gray-500">No recent posts found.</p>
          )}
        </div>

        <div className="flex items-center gap-3 mt-8">
          <button onClick={() => router.push("/admin/workspaces")} className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50">Back to Workspaces</button>
          <button onClick={handleDelete} className="px-4 py-2 text-sm rounded-lg bg-red-600 text-white hover:bg-red-700">Delete Workspace</button>
        </div>
      </main>

      {/* Post Detail Modal */}
      {selectedPost && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50" onClick={() => setSelectedPost(null)}>
          <div className="bg-white rounded-xl shadow-lg max-w-2xl w-full p-6 space-y-4 max-h-[90vh] flex flex-col" onClick={(e) => e.stopPropagation()}>
            <div className="flex items-center justify-between pb-3 border-b border-gray-100">
              <h3 className="text-xl font-semibold text-gray-900">{selectedPost.title || "Untitled Post"}</h3>
              <button onClick={() => setSelectedPost(null)} className="text-gray-400 hover:text-gray-600 text-2xl leading-none">&times;</button>
            </div>
            <div className="flex gap-4 text-sm text-gray-500">
              <div className="flex items-center gap-2">
                <span className="font-medium">Status:</span>
                <span>{selectedPost.statusName}</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="font-medium">Created:</span>
                <span>{new Date(selectedPost.createdAt).toLocaleDateString()}</span>
              </div>
            </div>
            <div className="mt-4 flex-1 overflow-y-auto space-y-4">
              <div className="p-4 bg-gray-50 rounded-lg whitespace-pre-wrap text-sm text-gray-700 font-mono">
                {selectedPost.textContent || "No text content."}
              </div>
              {selectedPost.videoUrl && (
                <div>
                  <h4 className="font-semibold text-sm mb-2 text-gray-900">Video</h4>
                  <video src={selectedPost.videoUrl} controls className="max-w-full rounded-lg max-h-64 border border-gray-200" />
                </div>
              )}
              {selectedPost.imageUrl && (
                <div>
                  <h4 className="font-semibold text-sm mb-2 text-gray-900">Images</h4>
                  <div className="flex gap-2 overflow-x-auto pb-2">
                    {(() => {
                      let imgs: string[] = [];
                      try {
                        imgs = selectedPost.imageUrl.startsWith("[") ? JSON.parse(selectedPost.imageUrl) : [selectedPost.imageUrl];
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
              <button onClick={() => setSelectedPost(null)} className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50">Close</button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
