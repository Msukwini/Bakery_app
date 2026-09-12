'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { DollarSign, Plus, X, RefreshCw, Pencil, Trash2, History, TrendingUp } from 'lucide-react';

interface Rule {
  id: string;
  ratePerUnit: number;
  effectiveDate: string;
  endDate: string | null;
  isActive: boolean;
  isCurrent: boolean;
}

interface VariantRules {
  variantId: string;
  productName: string;
  variantName: string;
  activeRule: { id: string; ratePerUnit: number; effectiveDate: string; endDate: string | null } | null;
  history: Rule[];
}

export default function CommissionRulesPage() {
  const [rules, setRules] = useState<VariantRules[]>([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [expanded, setExpanded] = useState<string | null>(null);

  const [showCreate, setShowCreate] = useState<VariantRules | null>(null);
  const [createForm, setCreateForm] = useState({
    ratePerUnit: '',
    effectiveDate: new Date().toISOString().split('T')[0],
  });

  const [editing, setEditing] = useState<{ ruleId: string; variant: VariantRules } | null>(null);
  const [editForm, setEditForm] = useState({ ratePerUnit: '' });

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/admin/commission-rules');
      setRules(res.data);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to load');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!showCreate) return;
    setError('');
    try {
      await api.post('/api/admin/commission-rules', {
        productVariantId: showCreate.variantId,
        ratePerUnit: parseFloat(createForm.ratePerUnit),
        effectiveDate: new Date(createForm.effectiveDate).toISOString(),
      });
      setMessage(`New rule applied to ${showCreate.productName} · ${showCreate.variantName}`);
      setShowCreate(null);
      setCreateForm({ ratePerUnit: '', effectiveDate: new Date().toISOString().split('T')[0] });
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to create');
    }
  };

  const handleUpdate = async () => {
    if (!editing) return;
    try {
      await api.put(`/api/admin/commission-rules/${editing.ruleId}`, {
        ratePerUnit: parseFloat(editForm.ratePerUnit),
        effectiveDate: new Date().toISOString(),
      });
      setMessage('Rule updated.');
      setEditing(null);
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to update');
    }
  };

  const handleDeactivate = async (ruleId: string) => {
    if (!confirm('Deactivate this rule? History is preserved but future sales will use the next active rule.')) return;
    try {
      await api.delete(`/api/admin/commission-rules/${ruleId}`);
      setMessage('Rule deactivated.');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed');
    }
  };

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-gray-800">Commission Rules</h1>
          <p className="text-sm text-gray-500 mt-1">
            Set how much resellers earn per unit sold. Rules are versioned — old sales keep their original rate.
          </p>
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
        <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg flex justify-between items-center">
          <span>{error}</span>
          <button onClick={() => setError('')}><X size={16} /></button>
        </div>
      )}

      {loading ? <p className="text-gray-500">Loading...</p> : rules.length === 0 ? (
        <div className="bg-white p-12 rounded-xl border text-center">
          <DollarSign className="mx-auto mb-3 text-gray-300" size={48} />
          <p className="text-gray-600 font-medium">No product variants yet</p>
          <p className="text-sm text-gray-500 mt-1">Add products first, then set commission rules here.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {rules.map((r) => {
            const isExpanded = expanded === r.variantId;
            return (
              <div key={r.variantId} className="bg-white rounded-xl border overflow-hidden">
                <div className="p-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
                  <div className="flex items-start gap-3 min-w-0 flex-1">
                    <div className={`p-2 rounded-lg flex-shrink-0 ${r.activeRule ? 'bg-green-50 text-green-600' : 'bg-gray-100 text-gray-400'}`}>
                      <DollarSign size={18} />
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="font-semibold text-gray-800 truncate">
                        {r.productName} · {r.variantName}
                      </p>
                      {r.activeRule ? (
                        <p className="text-sm text-gray-500 mt-0.5">
                          <span className="text-green-700 font-semibold">
                            R {r.activeRule.ratePerUnit.toFixed(2)}
                          </span> per unit
                          {' · '}since {new Date(r.activeRule.effectiveDate).toLocaleDateString('en-ZA')}
                        </p>
                      ) : (
                        <p className="text-sm text-amber-600 mt-0.5">No active commission rule</p>
                      )}
                    </div>
                  </div>

                  <div className="flex gap-2 self-start sm:self-auto">
                    <button
                      onClick={() => {
                        setShowCreate(r);
                        setCreateForm({
                          ratePerUnit: r.activeRule ? String(r.activeRule.ratePerUnit) : '',
                          effectiveDate: new Date().toISOString().split('T')[0],
                        });
                      }}
                      className="flex items-center gap-1.5 px-3 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg"
                    >
                      <Plus size={14} /> New Rate
                    </button>
                    {r.activeRule && (
                      <button
                        onClick={() => {
                          setEditing({ ruleId: r.activeRule!.id, variant: r });
                          setEditForm({ ratePerUnit: String(r.activeRule!.ratePerUnit) });
                        }}
                        className="p-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg"
                        title="Edit current rate"
                      >
                        <Pencil size={14} />
                      </button>
                    )}
                    <button
                      onClick={() => setExpanded(isExpanded ? null : r.variantId)}
                      className="flex items-center gap-1.5 px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg"
                    >
                      <History size={14} /> History ({r.history.length})
                    </button>
                  </div>
                </div>

                {isExpanded && (
                  <div className="border-t border-gray-100 p-4 bg-gray-50">
                    <h3 className="text-xs font-semibold text-gray-600 uppercase tracking-wide mb-3">
                      Rule History
                    </h3>
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm bg-white rounded-lg">
                        <thead className="bg-gray-50 text-gray-600 text-left">
                          <tr>
                            <th className="px-3 py-2">Rate/unit</th>
                            <th className="px-3 py-2">Effective From</th>
                            <th className="px-3 py-2">Ended</th>
                            <th className="px-3 py-2">Status</th>
                            <th className="px-3 py-2"></th>
                          </tr>
                        </thead>
                        <tbody className="divide-y">
                          {r.history.map((h) => (
                            <tr key={h.id}>
                              <td className="px-3 py-2 font-semibold">R {h.ratePerUnit.toFixed(2)}</td>
                              <td className="px-3 py-2 text-gray-500 text-xs">
                                {new Date(h.effectiveDate).toLocaleDateString('en-ZA')}
                              </td>
                              <td className="px-3 py-2 text-gray-500 text-xs">
                                {h.endDate ? new Date(h.endDate).toLocaleDateString('en-ZA') : '—'}
                              </td>
                              <td className="px-3 py-2">
                                {h.isCurrent ? (
                                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-medium bg-green-100 text-green-800">
                                    <TrendingUp size={10} /> CURRENT
                                  </span>
                                ) : !h.isActive ? (
                                  <span className="px-2 py-0.5 rounded text-[10px] font-medium bg-gray-100 text-gray-600">
                                    INACTIVE
                                  </span>
                                ) : (
                                  <span className="px-2 py-0.5 rounded text-[10px] font-medium bg-amber-100 text-amber-800">
                                    SUPERSEDED
                                  </span>
                                )}
                              </td>
                              <td className="px-3 py-2 text-right">
                                {h.isActive && (
                                  <button
                                    onClick={() => handleDeactivate(h.id)}
                                    className="text-gray-400 hover:text-red-600"
                                    title="Deactivate"
                                  >
                                    <Trash2 size={12} />
                                  </button>
                                )}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Create New Rule Modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">New Commission Rate</h2>
              <button onClick={() => setShowCreate(null)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <div className="bg-gray-50 p-3 rounded-lg mb-4 text-sm">
              <p className="font-medium text-gray-800">
                {showCreate.productName} · {showCreate.variantName}
              </p>
              {showCreate.activeRule && (
                <p className="text-xs text-gray-500 mt-1">
                  Current rate: <strong>R {showCreate.activeRule.ratePerUnit.toFixed(2)}</strong> / unit
                </p>
              )}
            </div>

            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">
                  Rate per Unit (R) <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  required
                  value={createForm.ratePerUnit}
                  onChange={(e) => setCreateForm({ ...createForm, ratePerUnit: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                  placeholder="e.g., 30.00"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Reseller earns this amount for every unit sold.
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Effective From <span className="text-red-500">*</span>
                </label>
                <input
                  type="date"
                  required
                  value={createForm.effectiveDate}
                  onChange={(e) => setCreateForm({ ...createForm, effectiveDate: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                />
                <p className="text-xs text-gray-500 mt-1">
                  Old rate automatically ends the day before this date. Sales before then keep the old rate.
                </p>
              </div>

              <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg text-xs text-amber-900">
                <strong>Note:</strong> This creates a new version. Existing sales keep their original rate — history is never lost.
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button type="button" onClick={() => setShowCreate(null)} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
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

      {/* Edit Current Rule Modal */}
      {editing && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold">Edit Current Rate</h2>
              <button onClick={() => setEditing(null)} className="text-gray-400 hover:text-gray-600">
                <X size={20} />
              </button>
            </div>

            <div className="bg-gray-50 p-3 rounded-lg mb-4 text-sm">
              <p className="font-medium text-gray-800">
                {editing.variant.productName} · {editing.variant.variantName}
              </p>
            </div>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">
                  New Rate per Unit (R)
                </label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  value={editForm.ratePerUnit}
                  onChange={(e) => setEditForm({ ...editForm, ratePerUnit: e.target.value })}
                  className="w-full px-3 py-2 border rounded-lg text-sm"
                />
              </div>

              <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-xs text-red-900">
                <strong>Warning:</strong> Editing the active rule changes future sales only. Past sales are not affected.
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button onClick={() => setEditing(null)} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
                  Cancel
                </button>
                <button onClick={handleUpdate} className="px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg">
                  Save
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
