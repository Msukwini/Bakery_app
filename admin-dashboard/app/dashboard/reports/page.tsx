'use client';

import { api } from '@/lib/api';
import { FileText, Download } from 'lucide-react';

export default function ReportsPage() {
  const download = async (endpoint: string, filename: string) => {
    try {
      const res = await api.get(endpoint, { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      alert('Download failed');
    }
  };

  const reports = [
    { label: 'Orders Report', description: 'All buyer orders with customer details', endpoint: '/api/Reports/orders/csv', filename: `orders_${Date.now()}.csv` },
    { label: 'Commissions Report', description: 'Reseller commission ledger', endpoint: '/api/Reports/commissions/csv', filename: `commissions_${Date.now()}.csv` },
    { label: 'Delivery Earnings', description: 'Delivery employee earnings', endpoint: '/api/Reports/delivery/csv', filename: `delivery_${Date.now()}.csv` },
  ];

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-6">Reports</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {reports.map((r) => (
          <div key={r.label} className="bg-white p-6 rounded-xl border border-gray-200">
            <div className="flex items-center gap-3 mb-3">
              <div className="p-2 bg-blue-50 text-blue-600 rounded-lg">
                <FileText size={20} />
              </div>
              <h3 className="font-semibold text-gray-800">{r.label}</h3>
            </div>
            <p className="text-sm text-gray-500 mb-4">{r.description}</p>
            <button
              onClick={() => download(r.endpoint, r.filename)}
              className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg"
            >
              <Download size={14} /> Download CSV
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
