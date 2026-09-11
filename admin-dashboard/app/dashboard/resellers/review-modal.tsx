'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { X, Plus } from 'lucide-react';

interface Application {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  residenceName: string | null;
}

interface Residence {
  id: string;
  name: string;
  estimatedPopulation: number;
  maxResellerCapacity: number;
}

export default function ReviewApplicationModal({
  application,
  isOpen,
  onClose,
  onSuccess,
}: {
  application: Application | null;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}) {
  const [residences, setResidences] = useState<Residence[]>([]);
  const [selectedResidenceId, setSelectedResidenceId] = useState('');
  const [adminNotes, setAdminNotes] = useState('');
  const [showCreateResidence, setShowCreateResidence] = useState(false);
  const [newResidence, setNewResidence] = useState({ name: '', address: '', population: '', capacity: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isOpen) {
      api.get('/api/Admin/residences').then((r) => {
        setResidences(r.data);
        if (application?.residenceName) {
          const match = r.data.find(
            (res: Residence) => res.name.toLowerCase() === application.residenceName!.toLowerCase()
          );
          if (match) setSelectedResidenceId(match.id);
        }
      });
    }
  }, [isOpen, application]);

  if (!isOpen || !application) return null;

  const handleApprove = async () => {
    if (!selectedResidenceId) {
      setError('Please select a residence');
      return;
    }
    setError('');
    setLoading(true);
    try {
      await api.put(`/api/ResellerApplications/${application.id}/review`, {
        status: 2,
        residenceId: selectedResidenceId,
        adminNotes,
      });
      onSuccess();
      onClose();
      setAdminNotes('');
      setSelectedResidenceId('');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to approve');
    } finally {
      setLoading(false);
    }
  };

  const handleReject = async () => {
    setError('');
    setLoading(true);
    try {
      await api.put(`/api/ResellerApplications/${application.id}/review`, {
        status: 3,
        adminNotes,
      });
      onSuccess();
      onClose();
      setAdminNotes('');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to reject');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateResidence = async () => {
    if (!newResidence.name || !newResidence.address) return;
    try {
      const res = await api.post('/api/Residences', {
        name: newResidence.name,
        address: newResidence.address,
        estimatedPopulation: parseInt(newResidence.population) || 0,
        maxResellerCapacity: parseInt(newResidence.capacity) || 1,
      });
      const updated = await api.get('/api/Admin/residences');
      setResidences(updated.data);
      setSelectedResidenceId(res.data.id);
      setShowCreateResidence(false);
      setNewResidence({ name: '', address: '', population: '', capacity: '' });
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to create residence');
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-xl p-6 w-full max-w-lg max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold">Review Application</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <X size={20} />
          </button>
        </div>

        <div className="bg-gray-50 p-4 rounded-lg mb-4 text-sm">
          <p className="font-medium text-gray-800">{application.firstName} {application.lastName}</p>
          <p className="text-gray-600 mt-1">{application.email} · {application.phoneNumber}</p>
          {application.residenceName && (
            <p className="text-gray-600 mt-2">
              <span className="text-gray-500">Applicant said residence:</span>{' '}
              <span className="font-medium">"{application.residenceName}"</span>
            </p>
          )}
        </div>

        {!showCreateResidence ? (
          <>
            <label className="block text-sm font-medium mb-1">
              Assign to Residence <span className="text-red-500">*</span>
            </label>
            <select
              value={selectedResidenceId}
              onChange={(e) => setSelectedResidenceId(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg text-sm bg-white mb-2"
            >
              <option value="">— Select residence —</option>
              {residences.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.name} · {r.estimatedPopulation} residents · max {r.maxResellerCapacity} resellers
                </option>
              ))}
            </select>
            <button
              type="button"
              onClick={() => setShowCreateResidence(true)}
              className="flex items-center gap-1 text-xs text-blue-600 hover:text-blue-800 mb-4"
            >
              <Plus size={12} /> Residence not in list? Create new one
            </button>
          </>
        ) : (
          <div className="border border-blue-200 bg-blue-50 rounded-lg p-4 mb-4">
            <div className="flex items-center justify-between mb-3">
              <h3 className="font-medium text-sm">Create New Residence</h3>
              <button onClick={() => setShowCreateResidence(false)} className="text-gray-400">
                <X size={16} />
              </button>
            </div>
            <input
              placeholder="Residence name (e.g., Sunrise Student Village)"
              value={newResidence.name}
              onChange={(e) => setNewResidence({ ...newResidence, name: e.target.value })}
              className="w-full px-3 py-2 border rounded-lg text-sm mb-2"
            />
            <input
              placeholder="Address"
              value={newResidence.address}
              onChange={(e) => setNewResidence({ ...newResidence, address: e.target.value })}
              className="w-full px-3 py-2 border rounded-lg text-sm mb-2"
            />
            <div className="grid grid-cols-2 gap-2 mb-3">
              <input
                type="number"
                placeholder="Population (e.g., 500)"
                value={newResidence.population}
                onChange={(e) => setNewResidence({ ...newResidence, population: e.target.value })}
                className="px-3 py-2 border rounded-lg text-sm"
              />
              <input
                type="number"
                placeholder="Max resellers (e.g., 3)"
                value={newResidence.capacity}
                onChange={(e) => setNewResidence({ ...newResidence, capacity: e.target.value })}
                className="px-3 py-2 border rounded-lg text-sm"
              />
            </div>
            <button
              type="button"
              onClick={handleCreateResidence}
              disabled={!newResidence.name || !newResidence.address}
              className="w-full px-4 py-2 text-sm bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white rounded-lg"
            >
              Create Residence
            </button>
          </div>
        )}

        <label className="block text-sm font-medium mb-1">Admin Notes (optional)</label>
        <textarea
          value={adminNotes}
          onChange={(e) => setAdminNotes(e.target.value)}
          className="w-full px-3 py-2 border rounded-lg mb-4 text-sm"
          rows={2}
          placeholder="e.g., Approved based on strong application"
        />

        {error && <p className="text-sm text-red-600 bg-red-50 p-2 rounded mb-3">{error}</p>}

        <div className="flex justify-end gap-2">
          <button onClick={onClose} className="px-4 py-2 text-sm bg-gray-100 rounded-lg">
            Cancel
          </button>
          <button
            onClick={handleReject}
            disabled={loading}
            className="px-4 py-2 text-sm bg-red-600 hover:bg-red-700 text-white rounded-lg"
          >
            Reject
          </button>
          <button
            onClick={handleApprove}
            disabled={loading || !selectedResidenceId || showCreateResidence}
            className="px-4 py-2 text-sm bg-green-600 hover:bg-green-700 disabled:bg-green-300 text-white rounded-lg"
          >
            {loading ? 'Processing...' : 'Approve'}
          </button>
        </div>
      </div>
    </div>
  );
}
