'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

interface Commission {
  id: string;
  productName: string;
  variantName: string;
  quantity: number;
  amountEarned: number;
  amountPaid: number;
  isSettled: boolean;
  createdAt: string;
}

export default function SalesPage() {
  const [resellerId, setResellerId] = useState('');
  const [ledger, setLedger] = useState<Commission[]>([]);
  const [balance, setBalance] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  const load = async () => {
    if (!resellerId) return;
    setLoading(true);
    try {
      const [l, b] = await Promise.all([
        api.get(`/api/Sales/commission/${resellerId}`),
        api.get(`/api/Sales/balance/${resellerId}`),
      ]);
      setLedger(l.data);
      setBalance(b.data);
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed to load');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-6">Sales & Commission</h1>

      <div className="bg-white p-6 rounded-xl border mb-6">
        <label className="block text-sm font-medium mb-2">Reseller Employee ID (GUID)</label>
        <div className="flex gap-2">
          <input
            type="text"
            placeholder="9ecfb906-6c94-4271-87d9-ef288592386c"
            value={resellerId}
            onChange={(e) => setResellerId(e.target.value)}
            className="flex-1 px-3 py-2 border rounded-lg text-sm font-mono"
          />
          <button onClick={load} className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm">
            Load
          </button>
        </div>
        <p className="text-xs text-gray-500 mt-2">
          Tip: RES-0001 = 9ecfb906-6c94-4271-87d9-ef288592386c
        </p>
      </div>

      {message && <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg">{message}</div>}

      {balance && (
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div className="bg-white p-5 rounded-xl border">
            <p className="text-xs text-gray-500 uppercase">Outstanding</p>
            <p className="text-2xl font-bold text-orange-600 mt-2">R {balance.outstanding.toFixed(2)}</p>
          </div>
          <div className="bg-white p-5 rounded-xl border">
            <p className="text-xs text-gray-500 uppercase">Total Earned</p>
            <p className="text-2xl font-bold text-blue-600 mt-2">R {balance.totalEarned.toFixed(2)}</p>
          </div>
          <div className="bg-white p-5 rounded-xl border">
            <p className="text-xs text-gray-500 uppercase">Total Paid</p>
            <p className="text-2xl font-bold text-green-600 mt-2">R {balance.totalPaid.toFixed(2)}</p>
          </div>
        </div>
      )}

      {loading ? <p>Loading...</p> : ledger.length > 0 && (
        <div className="bg-white rounded-xl border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-4 py-3">Product</th>
                <th className="px-4 py-3">Qty</th>
                <th className="px-4 py-3">Earned</th>
                <th className="px-4 py-3">Paid</th>
                <th className="px-4 py-3">Settled</th>
                <th className="px-4 py-3">Date</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {ledger.map((e) => (
                <tr key={e.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">{e.productName} · {e.variantName}</td>
                  <td className="px-4 py-3">{e.quantity}</td>
                  <td className="px-4 py-3">R {e.amountEarned.toFixed(2)}</td>
                  <td className="px-4 py-3">R {e.amountPaid.toFixed(2)}</td>
                  <td className="px-4 py-3">{e.isSettled ? '✅' : '⏳'}</td>
                  <td className="px-4 py-3 text-gray-500">{new Date(e.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
