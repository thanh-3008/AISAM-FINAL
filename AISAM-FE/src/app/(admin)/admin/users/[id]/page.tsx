"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import AdminHeader from "@/components/admin/AdminHeader";
import StatusBadge from "@/components/admin/StatusBadge";
import { fetchAdminUserDetail, deleteUser, impersonateUser, fetchAdminAuditLogs } from "@/services/adminService";
import { setToken, setRefreshToken, setStoredUser, getToken } from "@/lib/auth";

export default function AdminUserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [user, setUser] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [showAllPayments, setShowAllPayments] = useState(false);
  const [impersonating, setImpersonating] = useState(false);
  
  // Activity Log
  const [activities, setActivities] = useState<any[]>([]);
  const [loadingActivities, setLoadingActivities] = useState(false);
  const [showActivities, setShowActivities] = useState(false);

  useEffect(() => {
    if (!id) return;
    fetchAdminUserDetail(id).then((data: any) => { setUser(data); setLoading(false); });
  }, [id]);

  useEffect(() => {
    if (showActivities && activities.length === 0) {
      setLoadingActivities(true);
      fetchAdminAuditLogs(1, 20, undefined, undefined, undefined, undefined, undefined, id).then(data => {
        setActivities(data?.items || []);
        setLoadingActivities(false);
      });
    }
  }, [showActivities, id, activities.length]);

  const handleImpersonate = async () => {
    if (!confirm(`Are you sure you want to login as ${user.email}?`)) return;
    setImpersonating(true);
    const data = await impersonateUser(user.id);
    if (data && data.accessToken) {
      // Save admin token to restore later
      const currentToken = getToken();
      if (currentToken) localStorage.setItem("aisam_admin_token", currentToken);
      
      setToken(data.accessToken);
      setRefreshToken(data.refreshToken);
      setStoredUser({
        id: data.user.id,
        fullName: data.user.fullName,
        email: data.user.email
      });
      // Redirect to the user's dashboard
      window.location.href = "/dashboard";
    } else {
      alert("Failed to impersonate user.");
      setImpersonating(false);
    }
  };

  if (loading) return (
    <><AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: "Loading..." }]} /><main className="flex-1 p-8"><div className="animate-pulse space-y-4"><div className="h-8 w-64 bg-gray-200 rounded" /></div></main></>
  );

  if (!user) return (
    <><AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: "Not Found" }]} /><main className="flex-1 p-8"><p className="text-gray-500">User not found.</p></main></>
  );

  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Users", href: "/admin/users" }, { label: user.email }]} />
      <main className="flex-1 p-8 space-y-6">
        {/* User Info */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <div className="flex justify-between items-start mb-4">
            <h3 className="text-lg font-semibold text-gray-900">User Details</h3>
            <button 
              onClick={handleImpersonate}
              disabled={impersonating || user.role === 2}
              className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
            >
              <span className="material-symbols-outlined text-[18px]">admin_panel_settings</span>
              {impersonating ? "Logging in..." : "Login as User"}
            </button>
          </div>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
            <div><dt className="text-gray-500">Email</dt><dd className="font-medium text-gray-900">{user.email}</dd></div>
            <div><dt className="text-gray-500">Full Name</dt><dd className="font-medium text-gray-900">{user.fullName}</dd></div>
            <div><dt className="text-gray-500">Role</dt><dd><StatusBadge status={user.roleName} variant={user.role === 2 ? "error" : "info"} /></dd></div>
            <div><dt className="text-gray-500">Status</dt><dd><StatusBadge status={user.isEmailVerified ? "Active" : "Inactive"} variant={user.isEmailVerified ? "success" : "warning"} /></dd></div>
            <div><dt className="text-gray-500">Created At</dt><dd className="font-medium text-gray-900">{new Date(user.createdAt).toLocaleDateString()}</dd></div>
            <div><dt className="text-gray-500">Workspaces</dt><dd className="font-medium text-gray-900">{user.workspaceCount ?? 0}</dd></div>
          </dl>
        </div>

        {/* Workspaces */}
        {user.workspaces && user.workspaces.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Workspaces ({user.workspaces.length})</h3>
            <div className="space-y-2">
              {user.workspaces.map((w: any) => (
                <div key={w.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900 text-sm">{w.name}</p>
                    <p className="text-xs text-gray-500">{w.typeName} · {w.status === 0 ? "Active" : "Limited"}</p>
                  </div>
                  <span className="text-xs text-gray-400">{new Date(w.createdAt).toLocaleDateString()}</span>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Sessions */}
        {user.sessions && user.sessions.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Recent Sessions ({user.sessions.length})</h3>
            <div className="space-y-2">
              {user.sessions.map((s: any, i: number) => (
                <div key={i} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg text-xs">
                  <span className="text-gray-500">{new Date(s.createdAt).toLocaleString()}</span>
                  <span className="text-gray-400 truncate max-w-xs ml-4">{s.userAgent || "Unknown"}</span>
                  <StatusBadge status={s.isActive ? "Active" : "Ended"} variant={s.isActive ? "success" : "neutral"} />
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Subscriptions */}
        {user.subscriptions && user.subscriptions.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Active Subscriptions ({user.subscriptions.length})</h3>
            <div className="space-y-2">
              {user.subscriptions.map((sub: any) => (
                <div key={sub.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900 text-sm">
                      Plan: {sub.planType === 1 ? "Pro" : sub.planType === 2 ? "Business" : "Free"}
                    </p>
                    <p className="text-xs text-gray-500">Workspace: {sub.workspaceName}</p>
                  </div>
                  <div className="text-right">
                    <StatusBadge status={sub.status === 1 ? "Active" : "Inactive"} variant={sub.status === 1 ? "success" : "neutral"} />
                    <p className="text-xs text-gray-400 mt-1">Ends: {sub.currentPeriodEnd ? new Date(sub.currentPeriodEnd).toLocaleDateString() : "N/A"}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Payments */}
        {user.payments && user.payments.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Payment History</h3>
              {user.payments.some((p: any) => new Date(p.createdAt).getTime() < Date.now() - 365 * 24 * 60 * 60 * 1000) && (
                <button 
                  onClick={() => setShowAllPayments(!showAllPayments)}
                  className="text-xs text-primary font-medium hover:underline"
                >
                  {showAllPayments ? "Hide older than 1 year" : "Show all history"}
                </button>
              )}
            </div>
            <div className="space-y-2 max-h-96 overflow-y-auto">
              {user.payments
                .filter((p: any) => showAllPayments || new Date(p.createdAt).getTime() >= Date.now() - 365 * 24 * 60 * 60 * 1000)
                .map((p: any) => (
                <div key={p.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div>
                    <p className="font-medium text-gray-900 text-sm">
                      {new Intl.NumberFormat('en-US', { style: 'currency', currency: p.currency || 'USD' }).format(p.amount)}
                    </p>
                    <p className="text-xs text-gray-500">{p.workspaceName ? `Workspace: ${p.workspaceName}` : "Direct Purchase"}</p>
                  </div>
                  <div className="text-right">
                    <StatusBadge status={p.status === 2 ? "Success" : p.status === 3 ? "Failed" : "Pending"} variant={p.status === 2 ? "success" : p.status === 3 ? "error" : "warning"} />
                    <p className="text-xs text-gray-400 mt-1">{new Date(p.createdAt).toLocaleDateString()}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Campaigns */}
        {user.campaigns && user.campaigns.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Ad Campaigns ({user.campaigns.length})</h3>
            <div className="space-y-2 max-h-96 overflow-y-auto">
              {user.campaigns.map((c: any) => (
                <div key={c.id} className="p-3 bg-gray-50 rounded-lg space-y-2">
                  <div className="flex justify-between items-center">
                    <p className="font-medium text-gray-900 text-sm">{c.name}</p>
                    <StatusBadge status={c.status === 2 ? "Active" : c.status === 0 ? "Draft" : "Completed"} variant={c.status === 2 ? "success" : "neutral"} />
                  </div>
                  <div className="grid grid-cols-4 gap-2 text-xs text-gray-500">
                    <div><span className="block text-gray-400">Workspace</span><span className="font-medium text-gray-700">{c.workspaceName}</span></div>
                    <div><span className="block text-gray-400">Impressions</span><span className="font-medium text-gray-700">{c.impressions?.toLocaleString() || 0}</span></div>
                    <div><span className="block text-gray-400">Clicks</span><span className="font-medium text-gray-700">{c.clicks?.toLocaleString() || 0}</span></div>
                    <div><span className="block text-gray-400">Spend</span><span className="font-medium text-gray-700">${c.spend?.toLocaleString() || 0}</span></div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Activity Logs */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider">Recent Activity Log</h3>
            <button 
              onClick={() => setShowActivities(!showActivities)}
              className="text-xs text-primary font-medium hover:underline"
            >
              {showActivities ? "Hide Activity" : "Load Activity"}
            </button>
          </div>
          
          {showActivities && (
            <div className="space-y-2 max-h-96 overflow-y-auto mt-4 pt-4 border-t border-gray-100">
              {loadingActivities ? (
                <div className="animate-pulse space-y-2">
                  <div className="h-10 bg-gray-100 rounded-lg" />
                  <div className="h-10 bg-gray-100 rounded-lg" />
                </div>
              ) : activities.length === 0 ? (
                <p className="text-sm text-gray-500 text-center py-4">No recent activity found.</p>
              ) : (
                activities.map((a: any) => (
                  <div key={a.id} className="flex flex-col sm:flex-row sm:items-center justify-between p-3 bg-gray-50 rounded-lg gap-2 text-sm">
                    <div>
                      <span className="font-medium text-gray-900 mr-2">{a.actionType}</span>
                      <span className="text-gray-500 text-xs px-2 py-0.5 bg-gray-200 rounded">{a.targetTable}</span>
                      {a.notes && <p className="text-xs text-gray-400 mt-1">{a.notes}</p>}
                    </div>
                    <span className="text-gray-400 text-xs whitespace-nowrap">{new Date(a.createdAt).toLocaleString()}</span>
                  </div>
                ))
              )}
              {activities.length >= 20 && (
                <div className="text-center mt-2">
                  <button onClick={() => router.push(`/admin/audit-logs?searchTerm=${encodeURIComponent(user.email)}`)} className="text-xs text-blue-600 hover:underline">
                    View full history in Audit Logs
                  </button>
                </div>
              )}
            </div>
          )}
        </div>

        <div className="flex items-center gap-3">
          <button onClick={() => router.push("/admin/users")} className="px-4 py-2 text-sm rounded-lg border border-gray-200 hover:bg-gray-50">Back to Users</button>
          {user.role !== 2 && (
            <button onClick={async () => { if (!confirm("Are you sure?")) return; await deleteUser(user.id); router.push("/admin/users"); }} className="px-4 py-2 text-sm rounded-lg bg-red-600 text-white hover:bg-red-700">Delete User</button>
          )}
        </div>
      </main>
    </>
  );
}
