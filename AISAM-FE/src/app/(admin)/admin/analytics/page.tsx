"use client";

import { useEffect, useState } from "react";
import AdminHeader from "@/components/admin/AdminHeader";
import AdminStatsCard from "@/components/admin/AdminStatsCard";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, Legend } from "recharts";

const mockUserData = [
  { name: "Mon", users: 12 },
  { name: "Tue", users: 19 },
  { name: "Wed", users: 8 },
  { name: "Thu", users: 15 },
  { name: "Fri", users: 22 },
  { name: "Sat", users: 5 },
  { name: "Sun", users: 3 },
];

const mockRevenueData = [
  { name: "Week 1", revenue: 1200000 },
  { name: "Week 2", revenue: 1800000 },
  { name: "Week 3", revenue: 1500000 },
  { name: "Week 4", revenue: 2200000 },
];

export default function AdminAnalyticsPage() {
  return (
    <>
      <AdminHeader breadcrumbs={[{ label: "Analytics" }]} />
      <main className="flex-1 p-8 overflow-y-auto space-y-8">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Analytics</h2>
          <p className="text-gray-500 mt-1">Platform-wide metrics and trends.</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <AdminStatsCard title="New Users (Week)" value="84" icon="person_add" change="+12%" changePositive />
          <AdminStatsCard title="Active Workspaces" value="156" icon="apartment" change="+5%" changePositive />
          <AdminStatsCard title="Content Generated" value="1,247" icon="auto_awesome" change="+18%" changePositive />
          <AdminStatsCard title="Revenue (Month)" value="6,900,000 VND" icon="trending_up" change="+8%" changePositive />
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">New Users (Last 7 Days)</h3>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={mockUserData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis tick={{ fontSize: 12 }} />
                <Tooltip />
                <Bar dataKey="users" fill="#4f46e5" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
            <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Revenue (Last 4 Weeks)</h3>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={mockRevenueData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis tick={{ fontSize: 12 }} tickFormatter={(v) => `${(v / 1000000).toFixed(1)}M`} />
                <Tooltip formatter={(v) => `${Number(v).toLocaleString()} VND`} />
                <Legend />
                <Line type="monotone" dataKey="revenue" stroke="#10b981" strokeWidth={2} dot={{ r: 4 }} />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      </main>
    </>
  );
}
