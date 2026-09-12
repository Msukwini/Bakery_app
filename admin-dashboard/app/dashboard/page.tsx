'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Package, Users, ShoppingCart, DollarSign, TrendingUp, CircleAlert } from 'lucide-react';

export default function DashboardPage() {
  const [stats, setStats] = useState({
    pendingStockRequests: 0,
    pendingDeposits: 0,
    activeResellers: 0,
    todaysOrders: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadStats() {
      try {
        const [requests, deposits, resellers, orders] = await Promise.all([
          api.get('/api/StockRequests?status=0').catch(() => ({ data: [] })),
          api.get('/api/Collections/deposits?status=0').catch(() => ({ data: [] })),
          api.get('/api/Admin/resellers').catch(() => ({ data: [] })),
          api.get('/api/Orders').catch(() => ({ data: [] })),
        ]);

        setStats({
          pendingStockRequests: requests.data.length || 0,
          pendingDeposits: deposits.data.length || 0,
          activeResellers: resellers.data.length || 0,
          todaysOrders: orders.data.length || 0,
        });
      } finally {
        setLoading(false);
      }
    }
    loadStats();
  }, []);

  const cards = [
    { label: 'Pending Stock', value: stats.pendingStockRequests, icon: Package, color: 'text-orange-600 bg-orange-50' },
    { label: 'Pending Deposits', value: stats.pendingDeposits, icon: DollarSign, color: 'text-green-600 bg-green-50' },
    { label: 'Active Resellers', value: stats.activeResellers, icon: Users, color: 'text-blue-600 bg-blue-50' },
    { label: 'Total Orders', value: stats.todaysOrders, icon: ShoppingCart, color: 'text-purple-600 bg-purple-50' },
  ];

  return (
    <div>
      <h1 className="text-xl sm:text-2xl font-bold text-gray-800 mb-6">Dashboard</h1>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4 mb-6 sm:mb-8">
        {cards.map((card) => {
          const Icon = card.icon;
          return (
            <div key={card.label} className="bg-white p-4 sm:p-5 rounded-xl border border-gray-200">
              <div className="flex items-center justify-between">
                <div className="min-w-0">
                  <p className="text-[10px] sm:text-xs text-gray-500 uppercase tracking-wide truncate">{card.label}</p>
                  <p className="text-2xl sm:text-3xl font-bold text-gray-800 mt-2">
                    {loading ? '—' : card.value}
                  </p>
                </div>
                <div className={`p-2 sm:p-3 rounded-lg flex-shrink-0 ${card.color}`}>
                  <Icon size={18} />
                </div>
              </div>
            </div>
          );
        })}
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4 sm:p-6">
        <div className="flex items-center gap-2 mb-4">
          <TrendingUp size={20} className="text-blue-600" />
          <h2 className="font-semibold text-gray-800">Quick Actions</h2>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 text-sm">
          <a href="/dashboard/stock-requests" className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition">
            Review Stock Requests
          </a>
          <a href="/dashboard/deposits" className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition">
            Verify Deposits
          </a>
          <a href="/dashboard/users" className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition">
            Manage Users
          </a>
          <a href="/dashboard/reports" className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition">
            Download Reports
          </a>
        </div>
      </div>

      <div className="mt-6 flex items-start gap-3 p-4 bg-blue-50 border border-blue-100 rounded-lg">
        <CircleAlert size={20} className="text-blue-600 flex-shrink-0 mt-0.5" />
        <div className="text-sm text-blue-900">
          <p className="font-medium">Connected to API</p>
          <p className="text-blue-700 mt-1 break-all">Live data from ndlovubakery.duckdns.org</p>
        </div>
      </div>
    </div>
  );
}
