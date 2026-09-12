'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

export default function MyEarningsPage() {
  const [balance, setBalance] = useState<any>(null);
  const [ledger, setLedger] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const employeeId = localStorage.getItem('employeeGuid') || '';
    Promise.all([
      api.get(`/api/Delivery/balance/${employeeId}`).catch(() => ({ data: null })),
      api.get(`/api/Delivery/earnings/${employeeId}`).catch(() => ({ data: [] })),
    ]).then(([b, l]) => {
      setBalance(b.data);
      setLedger(l.data);
    }).finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-6">My Earnings</h1>

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

      {loading ? <p>Loading...</p> : ledger.length > 0 && (
        <div className="bg-white rounded-xl border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-3 sm:px-4 py-3">Date</th>
                <th className="px-3 sm:px-4 py-3">Amount</th>
                <th className="px-3 sm:px-4 py-3">Paid</th>
                <th className="px-3 sm:px-4 py-3">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {ledger.map((e) => (
                <tr key={e.id}>
                  <td className="px-3 sm:px-4 py-3 text-gray-500 text-xs">
                    {new Date(e.earningDate).toLocaleDateString()}
                  </td>
                  <td className="px-3 sm:px-4 py-3">R {e.amountEarned.toFixed(2)}</td>
                  <td className="px-3 sm:px-4 py-3">R {e.amountPaid.toFixed(2)}</td>
                  <td className="px-3 sm:px-4 py-3">{e.isSettled ? '✅' : '⏳'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
