"use client";

import { useState, useEffect } from "react";
import { motion, AnimatePresence } from "motion/react";
import { fetchCreditUsageHistory, CreditUsageRecord } from "@/services/workspaceService";

interface CreditUsageHistoryModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export default function CreditUsageHistoryModal({ isOpen, onClose }: CreditUsageHistoryModalProps) {
  const [history, setHistory] = useState<CreditUsageRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const pageSize = 10;

  useEffect(() => {
    if (isOpen) {
      loadHistory(1);
    }
  }, [isOpen]);

  const loadHistory = async (targetPage: number) => {
    setLoading(true);
    try {
      const res = await fetchCreditUsageHistory(targetPage, pageSize);
      if (res) {
        setHistory(res.data);
        setPage(res.page);
        setTotalPages(Math.ceil(res.totalCount / pageSize));
        setTotalCount(res.totalCount);
      }
    } catch (err) {
      console.error("Failed to load credit usage history:", err);
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <AnimatePresence>
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          exit={{ opacity: 0, scale: 0.95 }}
          className="bg-surface-container-lowest rounded-2xl border border-outline-variant/20 shadow-2xl w-full max-w-4xl max-h-[85vh] flex flex-col overflow-hidden"
        >
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-outline-variant/20">
            <div>
              <h3 className="text-headline-sm font-bold text-on-surface">Credit Usage History</h3>
              <p className="text-body-sm text-on-surface-variant mt-1">
                Total records: {totalCount}
              </p>
            </div>
            <button
              onClick={onClose}
              className="w-10 h-10 rounded-full flex items-center justify-center hover:bg-surface-container transition-colors text-on-surface-variant"
            >
              <span className="material-symbols-outlined">close</span>
            </button>
          </div>

          {/* Body */}
          <div className="flex-1 overflow-y-auto p-6">
            {loading ? (
              <div className="space-y-4">
                {[1, 2, 3, 4, 5].map((i) => (
                  <div key={i} className="h-16 bg-surface-container rounded-xl animate-pulse" />
                ))}
              </div>
            ) : history.length === 0 ? (
              <div className="text-center py-12">
                <span className="material-symbols-outlined text-outline/40 text-5xl mb-4 block">history</span>
                <p className="text-body-lg text-on-surface-variant">No credit usage history found</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="border-b border-outline-variant/20">
                      <th className="pb-3 font-semibold text-label-sm text-on-surface-variant">Date</th>
                      <th className="pb-3 font-semibold text-label-sm text-on-surface-variant">User</th>
                      <th className="pb-3 font-semibold text-label-sm text-on-surface-variant">Feature</th>
                      <th className="pb-3 font-semibold text-label-sm text-on-surface-variant">Action</th>
                      <th className="pb-3 font-semibold text-label-sm text-on-surface-variant text-right">Credits</th>
                      <th className="pb-3 font-semibold text-label-sm text-on-surface-variant text-right">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-outline-variant/10">
                    {history.map((record) => (
                      <tr key={record.id} className="hover:bg-surface-container-low/50 transition-colors">
                        <td className="py-4 text-body-sm text-on-surface">
                          {new Date(record.createdAt).toLocaleString()}
                        </td>
                        <td className="py-4 text-body-sm text-on-surface">{record.userName}</td>
                        <td className="py-4">
                          <span className="px-2.5 py-1 rounded-md text-label-xs bg-surface-container text-on-surface-variant font-medium">
                            {record.featureUsed}
                          </span>
                        </td>
                        <td className="py-4 text-body-sm text-on-surface">{record.action}</td>
                        <td className="py-4 text-body-sm font-semibold text-right text-primary">
                          {record.credits > 0 ? `-${record.credits}` : record.credits}
                        </td>
                        <td className="py-4 text-right">
                          <span
                            className={`px-2.5 py-1 rounded-full text-label-xs font-bold ${
                              record.status === "Success"
                                ? "bg-emerald-500/10 text-emerald-600"
                                : "bg-red-500/10 text-red-600"
                            }`}
                          >
                            {record.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          {/* Footer - Pagination */}
          {!loading && totalPages > 1 && (
            <div className="flex items-center justify-between p-4 border-t border-outline-variant/20 bg-surface-container-lowest">
              <span className="text-body-sm text-on-surface-variant">
                Page {page} of {totalPages}
              </span>
              <div className="flex items-center gap-2">
                <button
                  onClick={() => loadHistory(page - 1)}
                  disabled={page === 1}
                  className="px-4 py-2 rounded-lg border border-outline-variant/30 text-body-sm font-medium hover:bg-surface-container disabled:opacity-50 transition-colors"
                >
                  Previous
                </button>
                <button
                  onClick={() => loadHistory(page + 1)}
                  disabled={page === totalPages}
                  className="px-4 py-2 rounded-lg border border-outline-variant/30 text-body-sm font-medium hover:bg-surface-container disabled:opacity-50 transition-colors"
                >
                  Next
                </button>
              </div>
            </div>
          )}
        </motion.div>
      </div>
    </AnimatePresence>
  );
}
