"use client";

import { useState } from "react";
import { motion } from "motion/react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import AdminDataTable from "@/components/admin/AdminDataTable";
import AdminStatusBadge from "@/components/admin/AdminStatusBadge";

interface PayItem { id: string; userEmail: string; amount: number; currency: string; status: string; paymentMethod?: string; createdAt: string; }

export default function AdminPaymentsPage() {
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ["admin", "payments", page],
    queryFn: async () => {
      const res = await apiClient(`/admin/payments?page=${page}&pageSize=10`);
      return res.data as { data: PayItem[]; totalCount: number; totalPages: number };
    },
  });

  const columns = [
    { key: "user", header: "User", render: (p: PayItem) => <span className="font-medium">{p.userEmail}</span> },
    { key: "amount", header: "Amount", render: (p: PayItem) => `${(p.amount / 1000).toFixed(0)}K ${p.currency}` },
    { key: "status", header: "Status", render: (p: PayItem) => <AdminStatusBadge status={p.status} /> },
    { key: "method", header: "Method", render: (p: PayItem) => p.paymentMethod || "-" },
    { key: "created", header: "Date", render: (p: PayItem) => new Date(p.createdAt).toLocaleDateString() },
  ];

  return (
    <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }} className="space-y-6">
      <h1 className="text-headline-sm text-on-surface">Payments</h1>
      <div className="bg-surface-container-lowest border border-outline-variant/20 rounded-2xl overflow-hidden">
        <AdminDataTable columns={columns} data={data?.data || []}
          totalCount={data?.totalCount || 0} page={page} pageSize={10}
          totalPages={data?.totalPages || 1} onPageChange={setPage} isLoading={isLoading}
          emptyMessage="No payments found." />
      </div>
    </motion.div>
  );
}
