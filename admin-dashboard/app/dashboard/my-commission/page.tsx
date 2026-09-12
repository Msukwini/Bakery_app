'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';

export default function MyCommissionPage() {
  const [balance, setBalance] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const employeeId = localStorage.getItem('employeeGuid') || '';
    api.get(`/api/Sales/balance/${employeeId}`)
      .then((r) => setBalance(r.data))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-6">My Commission</h1>
      {loading ? <p>Loading...</p> : balance ? (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
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
      ) : (
        <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
          No commission data yet.
        </div>
      )}
    </div>
  );
}
