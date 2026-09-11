'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

interface Reseller {
  id: string;
  code: string;
  name: string;
  email: string;
  residenceName: string | null;
}

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
  const [resellers, setResellers] = useState<Reseller[]>([]);
  const [selectedId, setSelectedId] = useState('');
  const [ledger, setLedger] = useState<Commission[]>([]);
  const [balance, setBalance] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    api.get('/api/Admin/resellers')
      .then((res) => setResellers(res.data))
      .catch(() => setMessage('Failed to load resellers'));
  }, []);

  const load = async () => {
    if (!selectedId) return;
    setLoading(true);
    setMessage('');
    try {
      const [l, b] = await Promise.all([
        api.get(`/api/Sales/commission/${selectedId}`),
        api.get(`/api/Sales/balance/${selectedId}`),
      ]);
      setLedger(l.data);
      setBalance(b.data);
    } catch (err: any) {
      setMessage(err.response?.data?.error || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-6">Sales & Commission</h1>

      <div className="bg-white p-6 rounded-xl border mb-6">
        <label className="block text-sm font-medium mb-2">Select Reseller</label>
        <div className="flex gap-2">
          <select
            value={selectedId}
            onChange={(e) => setSelectedId(e.target.value)}
            className="flex-1 px-3 py-2 border rounded-lg text-sm bg-white"
          >
            <option value="">— Choose a reseller —</option>
            {resellers.map((r) => (
              <option key={r.id} value={r.id}>
                {r.code} · {r.name} {r.residenceName ? `(${r.residenceName})` : ''}
              </option>
            ))}
          </select>
          <button
            onClick={load}
            disabled={!selectedId}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white rounded-lg text-sm"
          >
            Load
          </button>
        </div>
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

      {loading ? <p className="text-gray-500">Loading...</p> : ledger.length > 0 && (
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
