'use client';

import { useEffect, useState, useRef } from 'react';
import { api } from '@/lib/api';
import { Camera, Save, User, AlertCircle } from 'lucide-react';
import ProfilePic from '@/components/ProfilePic';

export default function ProfilePage() {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [profile, setProfile] = useState<any>(null);
  const [form, setForm] = useState({
    firstName: '', lastName: '', phoneNumber: '',
    universityName: '', studentEmail: '',
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/Profile/me');
      setProfile(res.data);
      setForm({
        firstName: res.data.firstName,
        lastName: res.data.lastName,
        phoneNumber: res.data.phoneNumber,
        universityName: res.data.universityName || '',
        studentEmail: res.data.studentEmail || '',
      });
    } catch (err: any) {
      setError('Failed to load profile');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleSave = async () => {
    setSaving(true);
    setError('');
    try {
      await api.put('/api/Profile/me', form);
      setMessage('Profile updated successfully');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to save');
    } finally {
      setSaving(false);
    }
  };

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploading(true);
    setError('');
    try {
      const formData = new FormData();
      formData.append('file', file);
      await api.post('/api/Profile/picture', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setMessage('Profile picture uploaded');
      load();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to upload');
    } finally {
      setUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const isReseller = profile?.roles?.some((r: any) => r.role === 'Reseller');
  const isDelivery = profile?.roles?.some((r: any) => r.role === 'Delivery');

  if (loading) return <p className="text-gray-500">Loading...</p>;

  return (
    <div>
      <h1 className="text-xl sm:text-2xl font-bold text-gray-800 mb-6">My Profile</h1>

      {message && (
        <div className="mb-4 p-3 bg-green-50 text-green-700 text-sm rounded-lg">{message}</div>
      )}
      {error && (
        <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded-lg flex items-center gap-2">
          <AlertCircle size={16} /> {error}
        </div>
      )}

      {/* Picture Section */}
      <div className="bg-white rounded-xl border p-6 mb-6 flex flex-col sm:flex-row items-center gap-6">
        <div className="relative">
          <ProfilePic
            personId={profile.personId}
            hasPicture={profile.hasProfilePicture}
            firstName={profile.firstName}
            lastName={profile.lastName}
            size={120}
          />
          <button
            onClick={() => fileInputRef.current?.click()}
            disabled={uploading}
            className="absolute bottom-0 right-0 bg-blue-600 hover:bg-blue-700 text-white p-2 rounded-full disabled:bg-blue-300"
          >
            <Camera size={16} />
          </button>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/jpeg,image/png,image/webp"
            onChange={handleFileSelect}
            className="hidden"
          />
        </div>
        <div className="text-center sm:text-left">
          <h2 className="text-xl font-bold text-gray-800">{profile.firstName} {profile.lastName}</h2>
          <p className="text-sm text-gray-500 mt-1">{profile.email}</p>
          <div className="flex flex-wrap gap-2 mt-3 justify-center sm:justify-start">
            {profile.roles.map((r: any) => (
              <span key={r.code} className={`px-2 py-1 rounded text-xs font-medium ${
                r.role === 'Admin' ? 'bg-purple-100 text-purple-800' :
                r.role === 'Reseller' ? 'bg-blue-100 text-blue-800' :
                r.role === 'Delivery' ? 'bg-green-100 text-green-800' :
                'bg-gray-100 text-gray-800'
              }`}>
                {r.code} · {r.role}
              </span>
            ))}
          </div>
          {uploading && <p className="text-xs text-blue-600 mt-2">Uploading...</p>}
        </div>
      </div>

      {/* Editable Fields */}
      <div className="bg-white rounded-xl border p-6">
        <h3 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
          <User size={18} /> Personal Information
        </h3>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field label="First Name" value={form.firstName} onChange={(v: string) => setForm({ ...form, firstName: v })} />
          <Field label="Last Name" value={form.lastName} onChange={(v: string) => setForm({ ...form, lastName: v })} />
          <Field label="Phone Number" value={form.phoneNumber} onChange={(v: string) => setForm({ ...form, phoneNumber: v })} />
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input value={profile.email} disabled className="w-full px-3 py-2 border rounded-lg text-sm bg-gray-50 text-gray-500" />
          </div>
        </div>

        {(isReseller || isDelivery) && (
          <div className="mt-6 pt-6 border-t border-gray-100">
            <h3 className="font-semibold text-gray-800 mb-4">Student Information (optional)</h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <Field label="University" value={form.universityName} onChange={(v: string) => setForm({ ...form, universityName: v })} placeholder="e.g., DUT, UKZN" />
              <Field label="Student Email" value={form.studentEmail} onChange={(v: string) => setForm({ ...form, studentEmail: v })} placeholder="e.g., 12345@dut4life.ac.za" />
            </div>
          </div>
        )}

        <div className="mt-6 flex justify-end">
          <button
            onClick={handleSave}
            disabled={saving}
            className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white rounded-lg text-sm"
          >
            <Save size={14} /> {saving ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </div>
    </div>
  );
}

function Field({ label, value, onChange, placeholder }: any) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
      />
    </div>
  );
}
