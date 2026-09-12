'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { useAuth } from '@/lib/auth';

export default function MySalesPage() {
  const { user } = useAuth();
  const [ledger, setLedger] = useState<any[]>([]);
  const [balance, setBalance] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user) return;
    // Use the reseller's employee ID from the token
    const employeeId = localStorage.getItem('employeeGuid') || '';
    api.get(`/api/Sales/commission/${employeeId}`)
      .then((r) => setLedger(r.data))
      .catch(() => {});
    api.get(`/api/Sales/balance/${employeeId}`)
      .then((r) => setBalance(r.data))
      .catch(() => {});
    setLoading(false);
  }, [user]);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-2">My Sales</h1>
      <p className="text-sm text-gray-500 mb-6">Track your commission earnings</p>

      {balance && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
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

      {loading ? <p>Loading...</p> : ledger.length === 0 ? (
        <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
          No sales recorded yet.
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-3 sm:px-4 py-3">Product</th>
                <th className="px-3 sm:px-4 py-3">Qty</th>
                <th className="px-3 sm:px-4 py-3">Earned</th>
                <th className="px-3 sm:px-4 py-3">Date</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {ledger.map((e) => (
                <tr key={e.id}>
                  <td className="px-3 sm:px-4 py-3">{e.productName} · {e.variantName}</td>
                  <td className="px-3 sm:px-4 py-3">{e.quantity}</td>
                  <td className="px-3 sm:px-4 py-3">R {e.amountEarned.toFixed(2)}</td>
                  <td className="px-3 sm:px-4 py-3 text-gray-500 text-xs">
                    {new Date(e.createdAt).toLocaleDateString()}
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
