'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Mail, CheckCircle, AlertCircle, RefreshCw } from 'lucide-react';

interface Notification {
  id: string;
  type: string;
  recipientEmail: string;
  subject: string;
  sent: boolean;
  errorMessage: string | null;
  createdAt: string;
  sentAt: string | null;
}

const typeColors: Record<string, string> = {
  ORDER_STATUS: 'bg-purple-100 text-purple-800',
  DEPOSIT_SUBMITTED: 'bg-green-100 text-green-800',
  STOCK_REQUEST_SUBMITTED: 'bg-blue-100 text-blue-800',
  STOCK_REQUEST_APPROVED: 'bg-teal-100 text-teal-800',
  STOCK_REQUEST_REJECTED: 'bg-red-100 text-red-800',
  RESELLER_APPLICATION: 'bg-orange-100 text-orange-800',
  LOW_STOCK: 'bg-yellow-100 text-yellow-800',
};

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'all' | 'sent' | 'failed'>('all');

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/Notifications?limit=100');
      setNotifications(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const filtered = notifications.filter((n) => {
    if (filter === 'sent') return n.sent;
    if (filter === 'failed') return !n.sent;
    return true;
  });

  const totalSent = notifications.filter(n => n.sent).length;
  const totalFailed = notifications.filter(n => !n.sent).length;

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Email Notifications</h1>
          <p className="text-sm text-gray-500 mt-1">Automated email log</p>
        </div>
        <button
          onClick={load}
          className="flex items-center gap-2 px-4 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg"
        >
          <RefreshCw size={14} /> Refresh
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-white p-5 rounded-xl border">
          <p className="text-xs text-gray-500 uppercase tracking-wide">Total Sent</p>
          <p className="text-3xl font-bold text-gray-800 mt-2">{totalSent}</p>
        </div>
        <div className="bg-white p-5 rounded-xl border">
          <p className="text-xs text-gray-500 uppercase tracking-wide">Failed</p>
          <p className={`text-3xl font-bold mt-2 ${totalFailed > 0 ? 'text-red-600' : 'text-gray-800'}`}>
            {totalFailed}
          </p>
        </div>
        <div className="bg-white p-5 rounded-xl border">
          <p className="text-xs text-gray-500 uppercase tracking-wide">All Time</p>
          <p className="text-3xl font-bold text-gray-800 mt-2">{notifications.length}</p>
        </div>
      </div>

      <div className="flex gap-2 mb-4">
        {(['all', 'sent', 'failed'] as const).map((f) => (
          <button
            key={f}
            onClick={() => setFilter(f)}
            className={`px-3 py-1.5 text-sm rounded-lg capitalize ${
              filter === f
                ? 'bg-blue-600 text-white'
                : 'bg-white border border-gray-200 text-gray-700 hover:bg-gray-50'
            }`}
          >
            {f}
          </button>
        ))}
      </div>

      {loading ? (
        <p className="text-gray-500">Loading...</p>
      ) : filtered.length === 0 ? (
        <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
          <Mail className="mx-auto mb-3 text-gray-300" size={40} />
          <p>No notifications yet</p>
          <p className="text-xs mt-1">Emails will appear here as they are triggered</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Status</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Type</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Recipient</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Subject</th>
                <th className="px-3 sm:px-4 py-2 sm:py-3">Sent At</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {filtered.map((n) => (
                <tr key={n.id} className="hover:bg-gray-50">
                  <td className="px-3 sm:px-4 py-2 sm:py-3">
                    {n.sent ? (
                      <CheckCircle size={18} className="text-green-600" />
                    ) : (
                      <AlertCircle size={18} className="text-red-500" />
                    )}
                  </td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${typeColors[n.type] ?? 'bg-gray-100 text-gray-800'}`}>
                      {n.type}
                    </span>
                  </td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3 text-gray-700">{n.recipientEmail}</td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3">
                    <div className="font-medium text-gray-800">{n.subject}</div>
                    {!n.sent && n.errorMessage && (
                      <div className="text-xs text-red-600 mt-1 truncate max-w-md">
                        {n.errorMessage}
                      </div>
                    )}
                  </td>
                  <td className="px-3 sm:px-4 py-2 sm:py-3 text-gray-500 text-xs">
                    {n.sentAt ? new Date(n.sentAt).toLocaleString() : new Date(n.createdAt).toLocaleString()}
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
