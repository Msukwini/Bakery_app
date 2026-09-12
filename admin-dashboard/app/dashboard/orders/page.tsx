'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

interface Order {
  id: string;
  orderNumber: string;
  customerName: string | null;
  customerPhone: string | null;
  totalAmount: number;
  status: number;
  isPaid: boolean;
  createdAt: string;
  items: { productName: string; quantity: number }[];
}

const statusLabels = ['PENDING', 'CONFIRMED', 'PREPARING', 'READY', 'OUT_FOR_DELIVERY', 'DELIVERED', 'PAID', 'CANCELLED', 'REFUNDED'];
const statusColors = [
  'bg-yellow-100 text-yellow-800',
  'bg-blue-100 text-blue-800',
  'bg-purple-100 text-purple-800',
  'bg-indigo-100 text-indigo-800',
  'bg-orange-100 text-orange-800',
  'bg-teal-100 text-teal-800',
  'bg-green-100 text-green-800',
  'bg-red-100 text-red-800',
  'bg-gray-100 text-gray-800',
];

export default function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/Orders');
      setOrders(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const updateStatus = async (id: string, status: number) => {
    try {
      await api.put(`/api/Orders/${id}/status`, { status, adminNotes: 'Updated from dashboard' });
      setMessage('Order updated.');
      load();
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed');
    }
  };

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Orders</h1>
        <button onClick={load} className="px-4 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg">
          Refresh
        </button>
      </div>

      {message && <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg">{message}</div>}

      {loading ? <p>Loading...</p> : orders.length === 0 ? (
        <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
          No orders yet.
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Order #</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Customer</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Items</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Total</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Status</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Paid</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Date</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Change</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {orders.map((o) => (
                <tr key={o.id} className="hover:bg-gray-50">
                  <td className="px-3 sm:px-4 py-2 sm:py-3 font-mono text-xs">{o.orderNumber}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">{o.customerName}<div className="text-xs text-gray-400">{o.customerPhone}</div></td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">{o.items.length} item(s)</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">R {o.totalAmount.toFixed(2)}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${statusColors[o.status]}`}>
                      {statusLabels[o.status]}
                    </span>
                  </td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">{o.isPaid ? '✅' : '—'}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3 text-gray-500">{new Date(o.createdAt).toLocaleDateString()}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">
                    <select
                      onChange={(e) => updateStatus(o.id, parseInt(e.target.value))}
                      className="text-xs border rounded px-2 py-1"
                      value=""
                    >
                      <option value="">Set status…</option>
                      {statusLabels.map((label, i) => (
                        <option key={i} value={i}>{label}</option>
                      ))}
                    </select>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
