'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { TrendingUp, TrendingDown, DollarSign, Package, Users, Truck, RefreshCw, Calendar } from 'lucide-react';

interface Summary {
  from: string;
  to: string;
  revenue: number;
  guestRevenue: number;
  resellerRevenue: number;
  costOfGoods: number;
  commissions: number;
  deliveryEarnings: number;
  grossProfit: number;
  netProfit: number;
  ordersCount: number;
  salesCount: number;
}

interface MonthRow {
  year: number;
  month: number;
  monthName: string;
  revenue: number;
  costOfGoods: number;
  commissions: number;
  deliveryEarnings: number;
  netProfit: number;
}

export default function FinanceDashboardPage() {
  const [summary, setSummary] = useState<Summary | null>(null);
  const [monthly, setMonthly] = useState<MonthRow[]>([]);
  const [stockValue, setStockValue] = useState(0);
  const [loading, setLoading] = useState(true);
  const [year, setYear] = useState(new Date().getFullYear());
  const [range, setRange] = useState<'today' | 'week' | 'month' | 'year' | 'all'>('month');
  const [message, setMessage] = useState('');

  const getDateRange = (): { from?: string; to?: string } => {
    const now = new Date();
    const to = now.toISOString();
    switch (range) {
      case 'today': {
        const from = new Date(now.getFullYear(), now.getMonth(), now.getDate()).toISOString();
        return { from, to };
      }
      case 'week': {
        const from = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000).toISOString();
        return { from, to };
      }
      case 'month': {
        const from = new Date(now.getFullYear(), now.getMonth(), 1).toISOString();
        return { from, to };
      }
      case 'year': {
        const from = new Date(now.getFullYear(), 0, 1).toISOString();
        return { from, to };
      }
      case 'all':
      default:
        return {};
    }
  };

  const load = async () => {
    setLoading(true);
    try {
      const { from, to } = getDateRange();
      const params = new URLSearchParams();
      if (from) params.append('from', from);
      if (to) params.append('to', to);

      const [s, m, sv] = await Promise.all([
        api.get(`/api/admin/finance/summary?${params}`),
        api.get(`/api/admin/finance/monthly?year=${year}`),
        api.get('/api/admin/finance/stock-value'),
      ]);
      setSummary(s.data);
      setMonthly(m.data);
      setStockValue(sv.data.stockValue);
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed to load finance data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [range, year]);

  const fmt = (n: number) => `R ${n.toFixed(2)}`;
  const fmtCompact = (n: number) => {
    if (Math.abs(n) >= 1000000) return `R ${(n / 1000000).toFixed(2)}M`;
    if (Math.abs(n) >= 1000) return `R ${(n / 1000).toFixed(1)}k`;
    return `R ${n.toFixed(2)}`;
  };

  const isProfit = (summary?.netProfit ?? 0) >= 0;

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-gray-800">Financial Dashboard</h1>
          <p className="text-sm text-gray-500 mt-1">Revenue, costs, and profit tracking</p>
        </div>
        <button onClick={load} className="flex items-center gap-2 px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg self-start sm:self-auto">
          <RefreshCw size={14} /> Refresh
        </button>
      </div>

      {message && <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg">{message}</div>}

      {/* Range filter */}
      <div className="flex flex-wrap gap-2 mb-6">
        {(['today', 'week', 'month', 'year', 'all'] as const).map((r) => (
          <button
            key={r}
            onClick={() => setRange(r)}
            className={`px-3 py-1.5 text-sm rounded-lg capitalize ${
              range === r ? 'bg-blue-600 text-white' : 'bg-white border border-gray-200 text-gray-700 hover:bg-gray-50'
            }`}
          >
            {r === 'today' ? 'Today' : r === 'week' ? 'Last 7 Days' : r === 'month' ? 'This Month' : r === 'year' ? 'This Year' : 'All Time'}
          </button>
        ))}
      </div>

      {loading || !summary ? (
        <p className="text-gray-500">Loading...</p>
      ) : (
        <>
          {/* Top KPIs */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
            <KPI
              label="Revenue"
              value={fmtCompact(summary.revenue)}
              sub={`${summary.ordersCount} orders · ${summary.salesCount} reseller sales`}
              icon={DollarSign}
              color="text-green-600 bg-green-50"
            />
            <KPI
              label="Cost of Goods"
              value={fmtCompact(summary.costOfGoods)}
              sub="Production cost for sold items"
              icon={Package}
              color="text-orange-600 bg-orange-50"
            />
            <KPI
              label="Commissions Paid"
              value={fmtCompact(summary.commissions)}
              sub="Reseller earnings settled"
              icon={Users}
              color="text-blue-600 bg-blue-50"
            />
            <KPI
              label="Delivery Paid"
              value={fmtCompact(summary.deliveryEarnings)}
              sub="Delivery earnings settled"
              icon={Truck}
              color="text-purple-600 bg-purple-50"
            />
          </div>

          {/* Profit */}
          <div className={`rounded-xl p-6 mb-6 ${isProfit ? 'bg-gradient-to-r from-green-50 to-emerald-50 border border-green-200' : 'bg-gradient-to-r from-red-50 to-orange-50 border border-red-200'}`}>
            <div className="flex items-center justify-between">
              <div>
                <p className="text-xs uppercase tracking-wide text-gray-600 mb-1">Net Profit</p>
                <p className={`text-4xl font-bold ${isProfit ? 'text-green-700' : 'text-red-700'}`}>
                  {fmt(summary.netProfit)}
                </p>
                <p className="text-xs text-gray-500 mt-2">
                  Gross Profit: {fmt(summary.grossProfit)} − Commissions − Delivery
                </p>
              </div>
              <div className={`w-16 h-16 rounded-full flex items-center justify-center ${isProfit ? 'bg-green-100' : 'bg-red-100'}`}>
                {isProfit ? <TrendingUp size={32} className="text-green-600" /> : <TrendingDown size={32} className="text-red-600" />}
              </div>
            </div>
          </div>

          {/* Revenue breakdown + Stock value */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-6">
            <div className="lg:col-span-2 bg-white rounded-xl border p-5">
              <h3 className="font-semibold text-gray-800 mb-4">Revenue Sources</h3>
              <div className="space-y-3">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Guest Orders (paid)</span>
                  <span className="font-semibold text-gray-800">{fmt(summary.guestRevenue)}</span>
                </div>
                <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                  <div className="h-full bg-amber-500" style={{ width: `${summary.revenue > 0 ? (summary.guestRevenue / summary.revenue) * 100 : 0}%` }} />
                </div>
                <div className="flex justify-between items-center pt-2">
                  <span className="text-sm text-gray-600">Reseller Sales</span>
                  <span className="font-semibold text-gray-800">{fmt(summary.resellerRevenue)}</span>
                </div>
                <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                  <div className="h-full bg-blue-500" style={{ width: `${summary.revenue > 0 ? (summary.resellerRevenue / summary.revenue) * 100 : 0}%` }} />
                </div>
              </div>
            </div>
            <div className="bg-white rounded-xl border p-5">
              <h3 className="font-semibold text-gray-800 mb-4">Current Stock Value</h3>
              <p className="text-3xl font-bold text-gray-800">{fmtCompact(stockValue)}</p>
              <p className="text-xs text-gray-500 mt-2">Value of current inventory at cost</p>
            </div>
          </div>

          {/* Monthly breakdown */}
          <div className="bg-white rounded-xl border p-5 mb-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-semibold text-gray-800 flex items-center gap-2">
                <Calendar size={18} /> Monthly Breakdown
              </h3>
              <div className="flex items-center gap-2">
                <button onClick={() => setYear(year - 1)} className="px-2 py-1 bg-gray-100 rounded text-xs">←</button>
                <span className="font-medium text-sm">{year}</span>
                <button onClick={() => setYear(year + 1)} className="px-2 py-1 bg-gray-100 rounded text-xs">→</button>
              </div>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 text-gray-600 text-left">
                  <tr>
                    <th className="px-3 py-2">Month</th>
                    <th className="px-3 py-2 text-right">Revenue</th>
                    <th className="px-3 py-2 text-right">COGS</th>
                    <th className="px-3 py-2 text-right">Commissions</th>
                    <th className="px-3 py-2 text-right">Delivery</th>
                    <th className="px-3 py-2 text-right">Net Profit</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {monthly.map((m) => {
                    const hasData = m.revenue > 0 || m.costOfGoods > 0;
                    return (
                      <tr key={m.month} className={hasData ? '' : 'text-gray-300'}>
                        <td className="px-3 py-2 font-medium">{m.monthName}</td>
                        <td className="px-3 py-2 text-right">{fmt(m.revenue)}</td>
                        <td className="px-3 py-2 text-right">{fmt(m.costOfGoods)}</td>
                        <td className="px-3 py-2 text-right">{fmt(m.commissions)}</td>
                        <td className="px-3 py-2 text-right">{fmt(m.deliveryEarnings)}</td>
                        <td className={`px-3 py-2 text-right font-semibold ${m.netProfit >= 0 ? 'text-green-700' : 'text-red-700'}`}>
                          {fmt(m.netProfit)}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>

          <div className="p-4 bg-amber-50 border border-amber-200 rounded-xl text-sm text-amber-900">
            <p className="font-medium mb-1">ℹ️ How this is calculated</p>
            <p className="text-xs">
              <strong>Revenue</strong> = guest orders paid + reseller sales value.
              <strong> COGS</strong> = average stock cost × units sold.
              <strong> Profit</strong> = revenue − COGS − commissions − delivery payouts.
              Make sure every product variant has a <strong>Stock Cost</strong> set (Products page) for accurate COGS.
            </p>
          </div>
        </>
      )}
    </div>
  );
}

function KPI({ label, value, sub, icon: Icon, color }: any) {
  return (
    <div className="bg-white p-5 rounded-xl border">
      <div className="flex items-center justify-between">
        <div className="min-w-0">
          <p className="text-xs text-gray-500 uppercase tracking-wide">{label}</p>
          <p className="text-2xl font-bold text-gray-800 mt-2">{value}</p>
          {sub && <p className="text-xs text-gray-500 mt-1">{sub}</p>}
        </div>
        <div className={`p-3 rounded-lg ${color} flex-shrink-0`}>
          <Icon size={20} />
        </div>
      </div>
    </div>
  );
}
