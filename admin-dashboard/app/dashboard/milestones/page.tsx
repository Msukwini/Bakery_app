'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Target, Plus, X, RefreshCw, Trash2, CircleCheck, Award } from 'lucide-react';
import ProfilePic from '@/components/ProfilePic';

interface Config {
  id: string;
  productVariantId: string;
  productName: string;
  variantName: string;
  targetUnits: number;
  bonusAmount: number;
  timeframe: string;
  isActive: boolean;
  createdAt: string;
}

interface Alert {
  id: string;
  resellerEmployeeId: string;
  resellerCode: string;
  resellerName: string;
  resellerPersonId: string | null;
  resellerHasPicture: boolean;
  productName: string;
  variantName: string;
  targetUnits: number;
  achievedUnits: number;
  bonusOwed: number;
  periodKey: string;
  triggeredAt: string;
  isPaid: boolean;
  paidAt: string | null;
  adminNotes: string | null;
}

interface Variant {
  id: string;
  productName: string;
  sizeName: string;
  unitPrice: number;
}

export default function MilestonesPage() {
  const [configs, setConfigs] = useState<Config[]>([]);
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [variants, setVariants] = useState<Variant[]>([]);
  const [loading, setLoading] = useState(true);
  const [showConfigModal, setShowConfigModal] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [showPaid, setShowPaid] = useState(false);

  const [configForm, setConfigForm] = useState({
    productVariantId: '',
    targetUnits: '',
    bonusAmount: '',
    timeframe: 'Monthly',
  });

  const [payingAlert, setPayingAlert] = useState<Alert | null>(null);
  const [payNotes, setPayNotes] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const [c, a, v] = await Promise.all([
        api.get('/api/admin/milestones/configs'),
        api.get(`/api/admin/milestones/alerts${showPaid ? '?paid=true' : '?paid=false'}`),
        api.get('/api/admin/variants'),
      ]);
      setConfigs(c.data);
      setAlerts(a.data);
      setVariants(v.data);
      if (v.data.length > 0 && !configForm.productVariantId) {
        setConfigForm((f) => ({ ...f, productVariantId: v.data[0].id }));
      }
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [showPaid]);

  const handleCreateConfig = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await api.post('/api/admin/milestones/configs', {
        productVariantId: configForm.productVariantId,
        targetUnits: parseInt(configForm.targetUnits),
        bonusAmount: parseFloat(configForm.bonusAmount),
        timeframe: configForm.timeframe,
      });
      setMessage('Milestone config created.');
      setShowConfigModal(false);
      setConfigForm({ productVariantId: variants[0]?.id || '', targetUnits: '', bonusAmount: '', timeframe: 'Monthly' });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to create');
    }
  };

  const handleDeleteConfig = async (id: string) => {
    if (!confirm('Deactivate this milestone config? Existing alerts are preserved.')) return;
    try {
      await api.delete(`/api/admin/milestones/configs/${id}`);
      setMessage('Config deactivated.');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  const handleMarkPaid = async () => {
    if (!payingAlert) return;
    try {
      await api.post(`/api/admin/milestones/alerts/${payingAlert.id}/mark-paid`, {
        notes: payNotes || null,
      });
      setMessage('Bonus marked as paid.');
      setPayingAlert(null);
      setPayNotes('');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  const totalPending = alerts.filter(a => !a.isPaid).reduce((sum, a) => sum + a.bonusOwed, 0);

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-gray-800 flex items-center gap-2">
            <Award className="text-amber-600" size={24} /> Milestones
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            Reward resellers who hit sales targets. Bonus paid separately from per-unit commission.
          </p>
        </div>
        <div className="flex gap-2">
          <button onClick={load} className="flex items-center gap-2 px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg">
            <RefreshCw size={14} /> Refresh
          </button>
          <button onClick={() => setShowConfigModal(true)} className="flex items-center gap-2 px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
            <Plus size={14} /> New Milestone
          </button>
        </div>
      </div>

      {message && (
        <div className="mb-4 p-3 bg-blue-50 text-blue-700 text-sm rounded-lg flex justify-between items-center">
          <span>{message}</span>
          <button onClick={() => setMessage('')}><X size={16} /></button>
        </div>
      )}
      {error && <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg">{error}</div>}

      {/* KPIs */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-xl border p-5">
          <p className="text-xs text-gray-500 uppercase">Active Configs</p>
          <p className="text-3xl font-bold text-gray-800 mt-2">{configs.filter(c => c.isActive).length}</p>
        </div>
        <div className="bg-white rounded-xl border p-5">
          <p className="text-xs text-gray-500 uppercase">Pending Bonuses</p>
          <p className="text-3xl font-bold text-orange-600 mt-2">{alerts.filter(a => !a.isPaid).length}</p>
        </div>
        <div className="bg-white rounded-xl border p-5">
          <p className="text-xs text-gray-500 uppercase">Total Owed</p>
          <p className="text-3xl font-bold text-green-600 mt-2">R {totalPending.toFixed(2)}</p>
        </div>
      </div>

      {/* Configs */}
      <div className="mb-8">
        <h2 className="text-lg font-semibold text-gray-800 mb-3 flex items-center gap-2">
          <Target size={20} className="text-blue-600" /> Milestone Rules
        </h2>
        {loading ? <p className="text-gray-500">Loading...</p> : configs.length === 0 ? (
          <div className="bg-white p-8 rounded-xl border text-center text-gray-500 text-sm">
            No milestone rules yet. Click "New Milestone" to create one.
          </div>
        ) : (
          <div className="bg-white rounded-xl border overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-600 text-left">
                <tr>
                  <th className="px-4 py-3">Product</th>
                  <th className="px-4 py-3 text-right">Target</th>
                  <th className="px-4 py-3 text-right">Bonus</th>
                  <th className="px-4 py-3">Timeframe</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3"></th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {configs.map((c) => (
                  <tr key={c.id} className={!c.isActive ? 'opacity-50' : ''}>
                    <td className="px-4 py-3">
                      <div className="font-medium">{c.productName}</div>
                      <div className="text-xs text-gray-500">{c.variantName}</div>
                    </td>
                    <td className="px-4 py-3 text-right font-medium">{c.targetUnits} units</td>
                    <td className="px-4 py-3 text-right font-semibold text-green-600">R {c.bonusAmount.toFixed(2)}</td>
                    <td className="px-4 py-3 text-xs text-gray-600">{c.timeframe}</td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-1 rounded text-xs font-medium ${c.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'}`}>
                        {c.isActive ? 'ACTIVE' : 'INACTIVE'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-right">
                      {c.isActive && (
                        <button onClick={() => handleDeleteConfig(c.id)} className="text-gray-400 hover:text-red-600">
                          <Trash2 size={14} />
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Alerts */}
      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-lg font-semibold text-gray-800 flex items-center gap-2">
            <Award size={20} className="text-amber-600" /> Milestone Alerts
          </h2>
          <div className="flex gap-2">
            <button
              onClick={() => setShowPaid(false)}
              className={`px-3 py-1.5 text-xs rounded-lg ${!showPaid ? 'bg-blue-600 text-white' : 'bg-white border border-gray-200 text-gray-700 hover:bg-gray-50'}`}
            >
              Pending
            </button>
            <button
              onClick={() => setShowPaid(true)}
              className={`px-3 py-1.5 text-xs rounded-lg ${showPaid ? 'bg-blue-600 text-white' : 'bg-white border border-gray-200 text-gray-700 hover:bg-gray-50'}`}
            >
              Paid
            </button>
          </div>
        </div>

        {loading ? <p className="text-gray-500">Loading...</p> : alerts.length === 0 ? (
          <div className="bg-white p-8 rounded-xl border text-center text-gray-500 text-sm">
            {showPaid ? 'No paid milestones yet.' : 'No pending milestone alerts. All caught up!'}
          </div>
        ) : (
          <div className="space-y-2">
            {alerts.map((a) => (
              <div key={a.id} className={`bg-white rounded-xl border p-4 ${!a.isPaid ? 'border-amber-300' : ''}`}>
                <div className="flex flex-col sm:flex-row sm:items-center gap-4">
                  <div className="flex items-center gap-3 flex-1 min-w-0">
                    {a.resellerPersonId ? (
                      <ProfilePic
                        personId={a.resellerPersonId}
                        hasPicture={a.resellerHasPicture}
                        firstName={a.resellerName.split(' ')[0]}
                        lastName={a.resellerName.split(' ')[1]}
                        size={48}
                      />
                    ) : (
                      <div className="w-12 h-12 rounded-full bg-gray-100" />
                    )}
                    <div className="min-w-0">
                      <div className="font-medium text-gray-800">{a.resellerName}</div>
                      <div className="text-xs text-gray-500 font-mono">{a.resellerCode}</div>
                    </div>
                  </div>

                  <div className="flex-1 text-sm">
                    <div className="text-gray-600">
                      Sold <strong>{a.achievedUnits}</strong> × {a.productName} · {a.variantName}
                    </div>
                    <div className="text-xs text-gray-500 mt-0.5">
                      Target: {a.targetUnits} units · Period: {a.periodKey} · {new Date(a.triggeredAt).toLocaleDateString('en-ZA')}
                    </div>
                  </div>

                  <div className="text-right">
                    <div className="text-xs text-gray-500 uppercase">Bonus</div>
                    <div className="text-xl font-bold text-amber-600">R {a.bonusOwed.toFixed(2)}</div>
                  </div>

                  <div className="sm:ml-4">
                    {a.isPaid ? (
                      <span className="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg bg-green-50 text-green-700 text-xs font-medium">
                        <CircleCheck size={14} /> Paid
                      </span>
                    ) : (
                      <button
                        onClick={() => { setPayingAlert(a); setPayNotes(''); }}
                        className="px-4 py-2 text-sm bg-green-600 hover:bg-green-700 text-white rounded-lg"
                      >
                        Mark Paid
                      </button>
                    )}
                  </div>
                </div>
                {a.adminNotes && (
                  <div className="mt-3 pt-3 border-t border-gray-100 text-xs text-gray-500">
                    Note: {a.adminNotes}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Config Modal */}
      {showConfigModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">New Milestone Rule</h2>
              <button onClick={() => { setShowConfigModal(false); setError(''); }} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleCreateConfig} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">Product <span className="text-red-500">*</span></label>
                <select required value={configForm.productVariantId}
                  onChange={(e) => setConfigForm({ ...configForm, productVariantId: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm bg-white">
                  {variants.map((v) => (
                    <option key={v.id} value={v.id}>{v.productName} · {v.sizeName}</option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium mb-1">Target Units <span className="text-red-500">*</span></label>
                  <input type="number" required min="1" value={configForm.targetUnits}
                    onChange={(e) => setConfigForm({ ...configForm, targetUnits: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg text-sm"
                    placeholder="e.g., 10" />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Bonus (R) <span className="text-red-500">*</span></label>
                  <input type="number" required min="0.01" step="0.01" value={configForm.bonusAmount}
                    onChange={(e) => setConfigForm({ ...configForm, bonusAmount: e.target.value })}
                    className="w-full px-3 py-2 border rounded-lg text-sm"
                    placeholder="e.g., 300" />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Timeframe</label>
                <select value={configForm.timeframe}
                  onChange={(e) => setConfigForm({ ...configForm, timeframe: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm bg-white">
                  <option value="Monthly">Monthly (resets on the 1st)</option>
                  <option value="Weekly">Weekly (resets every Monday)</option>
                  <option value="Lifetime">Lifetime (one-time achievement)</option>
                </select>
              </div>

              <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg text-xs text-amber-900">
                When a reseller hits the target, you'll receive an email and this dashboard shows a pending bonus to pay.
              </div>

              {error && <p className="text-sm text-red-600 bg-red-50 p-2 rounded">{error}</p>}

              <div className="flex justify-end gap-2 pt-2">
                <button type="button" onClick={() => { setShowConfigModal(false); setError(''); }} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button type="submit" className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
                  Create Rule
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Mark Paid Modal */}
      {payingAlert && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Mark Bonus as Paid</h2>
              <button onClick={() => setPayingAlert(null)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <div className="bg-gray-50 p-3 rounded-lg mb-4 text-sm">
              <p className="font-medium text-gray-800">{payingAlert.resellerName} ({payingAlert.resellerCode})</p>
              <p className="text-xs text-gray-500 mt-1">
                {payingAlert.achievedUnits} × {payingAlert.productName} · {payingAlert.variantName}
              </p>
              <p className="text-lg font-bold text-amber-600 mt-2">R {payingAlert.bonusOwed.toFixed(2)}</p>
            </div>

            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium mb-1">Notes (optional)</label>
                <textarea value={payNotes}
                  onChange={(e) => setPayNotes(e.target.value)}
                  rows={2} className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder="e.g., Paid via bank transfer" />
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button onClick={() => setPayingAlert(null)} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button onClick={handleMarkPaid} className="px-4 py-2 text-sm bg-green-600 hover:bg-green-700 text-white rounded-lg">
                  Confirm Payment
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
