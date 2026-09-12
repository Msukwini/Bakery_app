'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Users, RefreshCw, X, AlertCircle, CircleCheck, Clock, CircleX, PauseCircle } from 'lucide-react';

interface Reseller {
  employeeId: string;
  code: string;
  personId: string;
  fullName: string;
  email: string;
  phone: string;
  residenceName: string | null;
  status: number | null;
  statusReason: string | null;
  statusChangedAt: string | null;
  trialEndsAt: string | null;
  isActive: boolean;
  assignedAt: string;
}

const STATUS_LABELS = ['TRIAL', 'ACTIVE', 'SUSPENDED', 'INACTIVE', 'TERMINATED'];
const STATUS_COLORS = [
  'bg-amber-100 text-amber-800',
  'bg-green-100 text-green-800',
  'bg-orange-100 text-orange-800',
  'bg-gray-100 text-gray-700',
  'bg-red-100 text-red-800',
];
const STATUS_ICONS = [Clock, CircleCheck, PauseCircle, CircleX, CircleX];

function fmt(d: string | null) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-ZA', { day: '2-digit', month: 'short', year: 'numeric' });
}

export default function ResellerLifecyclePage() {
  const [resellers, setResellers] = useState<Reseller[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<number | ''>('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [changing, setChanging] = useState<Reseller | null>(null);
  const [form, setForm] = useState({ status: 1, reason: '', trialDays: '30' });

  const load = async () => {
    setLoading(true);
    try {
      const url = filter === '' ? '/api/admin/reseller-lifecycle' : `/api/admin/reseller-lifecycle?status=${filter}`;
      const res = await api.get(url);
      setResellers(res.data);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [filter]);

  const handleChange = async () => {
    if (!changing) return;
    setError('');
    try {
      await api.put(`/api/admin/reseller-lifecycle/${changing.employeeId}/status`, {
        status: form.status,
        reason: form.reason || null,
        trialDays: form.status === 0 ? parseInt(form.trialDays) : null,
      });
      setMessage(`${changing.fullName} → ${STATUS_LABELS[form.status]}`);
      setChanging(null);
      setForm({ status: 1, reason: '', trialDays: '30' });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-gray-800">Reseller Lifecycle</h1>
          <p className="text-sm text-gray-500 mt-1">Manage reseller statuses: trial, active, suspended, terminated</p>
        </div>
        <button onClick={load} className="flex items-center gap-2 px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg self-start sm:self-auto">
          <RefreshCw size={14} /> Refresh
        </button>
      </div>

      {message && (
        <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg flex justify-between items-center">
          <span>{message}</span>
          <button onClick={() => setMessage('')}><X size={16} /></button>
        </div>
      )}
      {error && (
        <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg">{error}</div>
      )}

      <div className="flex flex-wrap gap-2 mb-6">
        <button
          onClick={() => setFilter('')}
          className={`px-3 py-1.5 text-sm rounded-lg ${filter === '' ? 'bg-blue-600 text-white' : 'bg-white border border-gray-200 text-gray-700 hover:bg-gray-50'}`}
        >
          All
        </button>
        {STATUS_LABELS.map((label, i) => (
          <button
            key={i}
            onClick={() => setFilter(i)}
            className={`px-3 py-1.5 text-sm rounded-lg ${filter === i ? 'bg-blue-600 text-white' : 'bg-white border border-gray-200 text-gray-700 hover:bg-gray-50'}`}
          >
            {label}
          </button>
        ))}
      </div>

      {loading ? <p className="text-gray-500">Loading...</p> : resellers.length === 0 ? (
        <div className="bg-white p-12 rounded-xl border text-center">
          <Users className="mx-auto mb-3 text-gray-300" size={48} />
          <p className="text-gray-600 font-medium">No resellers found</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-gray-600 text-left">
              <tr>
                <th className="px-3 sm:px-4 py-3">Code</th>
                <th className="px-3 sm:px-4 py-3">Name</th>
                <th className="px-3 sm:px-4 py-3">Residence</th>
                <th className="px-3 sm:px-4 py-3">Status</th>
                <th className="px-3 sm:px-4 py-3">Since</th>
                <th className="px-3 sm:px-4 py-3 text-right">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {resellers.map((r) => {
                const statusIdx = r.status ?? -1;
                const StatusIcon = statusIdx >= 0 ? STATUS_ICONS[statusIdx] : AlertCircle;
                return (
                  <tr key={r.employeeId} className="hover:bg-gray-50">
                    <td className="px-3 sm:px-4 py-3 font-mono text-xs">{r.code}</td>
                    <td className="px-3 sm:px-4 py-3">
                      <div className="font-medium">{r.fullName}</div>
                      <div className="text-xs text-gray-500">{r.email}</div>
                    </td>
                    <td className="px-3 sm:px-4 py-3 text-xs text-gray-600">{r.residenceName ?? '—'}</td>
                    <td className="px-3 sm:px-4 py-3">
                      {statusIdx >= 0 ? (
                        <div>
                          <span className={`inline-flex items-center gap-1 px-2 py-1 rounded text-xs font-medium ${STATUS_COLORS[statusIdx]}`}>
                            <StatusIcon size={12} /> {STATUS_LABELS[statusIdx]}
                          </span>
                          {statusIdx === 0 && r.trialEndsAt && (
                            <div className="text-[10px] text-gray-500 mt-1">Ends {fmt(r.trialEndsAt)}</div>
                          )}
                        </div>
                      ) : (
                        <span className="text-xs text-amber-600">UNSET</span>
                      )}
                    </td>
                    <td className="px-3 sm:px-4 py-3 text-xs text-gray-500">{fmt(r.assignedAt)}</td>
                    <td className="px-3 sm:px-4 py-3 text-right">
                      <button
                        onClick={() => {
                          setChanging(r);
                          setForm({ status: r.status ?? 1, reason: r.statusReason ?? '', trialDays: '30' });
                        }}
                        className="text-xs text-blue-600 hover:text-blue-800 font-medium"
                      >
                        Change Status
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {changing && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Change Reseller Status</h2>
              <button onClick={() => setChanging(null)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <div className="bg-gray-50 p-3 rounded-lg mb-4 text-sm">
              <p className="font-medium text-gray-800">{changing.fullName}</p>
              <p className="text-xs text-gray-500">{changing.code} · {changing.email}</p>
            </div>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">New Status</label>
                <select
                  value={form.status}
                  onChange={(e) => setForm({ ...form, status: Number(e.target.value) })}
                  className="w-full px-3 py-2 border rounded-lg text-sm bg-white"
                >
                  {STATUS_LABELS.map((label, i) => (
                    <option key={i} value={i}>{label}</option>
                  ))}
                </select>
              </div>

              {form.status === 0 && (
                <div>
                  <label className="block text-sm font-medium mb-1">Trial Duration (days)</label>
                  <input
                    type="number"
                    min="1"
                    value={form.trialDays}
                    onChange={(e) => setForm({ ...form, trialDays: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg text-sm"
                  />
                </div>
              )}

              <div>
                <label className="block text-sm font-medium mb-1">Reason (optional)</label>
                <textarea
                  value={form.reason}
                  onChange={(e) => setForm({ ...form, reason: e.target.value })}
                  rows={2}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder={
                    form.status === 2 ? 'e.g., Non-payment of collected cash' :
                    form.status === 4 ? 'e.g., Repeated policy violations' :
                    form.status === 1 ? 'e.g., Completed trial successfully' :
                    'Optional note'
                  }
                />
              </div>

              <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg text-xs text-amber-900">
                <strong>Note:</strong> Status TRIAL or ACTIVE makes the reseller able to sell. Other statuses deactivate them.
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button onClick={() => setChanging(null)} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button onClick={handleChange} className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
                  Change Status
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
