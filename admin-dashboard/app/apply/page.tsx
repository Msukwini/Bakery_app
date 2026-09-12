'use client';

import { useState } from 'react';
import Link from 'next/link';

export default function ApplyPage() {
  const [form, setForm] = useState({
    firstName: '', lastName: '', email: '', phoneNumber: '',
    residenceName: '', roomNumber: '', estimatedResidencePopulation: '',
    universityName: '', studentEmail: '',
    preferredSellingArea: '', previousSalesExperience: '',
    availability: '', expectedTimeAtResidence: '', additionalInfo: '',
  });
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://ndlovubakery.duckdns.org';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      const res = await fetch(`${apiUrl}/api/ResellerApplications`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...form,
          estimatedResidencePopulation: form.estimatedResidencePopulation ? parseInt(form.estimatedResidencePopulation) : null,
        }),
      });
      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.error || 'Submission failed');
      }
      setSubmitted(true);
    } catch (err: any) {
      setError(err.message || 'Something went wrong');
    } finally {
      setLoading(false);
    }
  };

  if (submitted) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-2xl shadow-lg p-8 max-w-md text-center">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <h1 className="text-2xl font-bold text-gray-800 mb-2">Application Submitted!</h1>
          <p className="text-gray-600 mb-6 text-sm">
            We'll review your application and contact you at <strong>{form.email}</strong>.
          </p>
          <Link href="/login" className="inline-block px-6 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg">
            Staff Login
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-50 py-8 px-4">
      <div className="max-w-2xl mx-auto">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-800 mb-2">Ndlovu Bakery</h1>
          <p className="text-lg text-gray-600">Become a Reseller</p>
          <p className="text-sm text-gray-500 mt-2 max-w-lg mx-auto">
            Earn commission by selling our products in your residence.
          </p>
        </div>

        <div className="bg-white rounded-2xl shadow-lg p-6 sm:p-8">
          <form onSubmit={handleSubmit} className="space-y-5">
            <Section title="Personal Information">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <Input required label="First Name" value={form.firstName} onChange={(v) => setForm({ ...form, firstName: v })} />
                <Input required label="Last Name" value={form.lastName} onChange={(v) => setForm({ ...form, lastName: v })} />
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <Input required type="email" label="Email" value={form.email} onChange={(v) => setForm({ ...form, email: v })} />
                <Input required label="Phone Number" value={form.phoneNumber} onChange={(v) => setForm({ ...form, phoneNumber: v })} />
              </div>
            </Section>

            <Section title="Student Information (optional)">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <Input label="University" placeholder="e.g., DUT, UKZN" value={form.universityName} onChange={(v) => setForm({ ...form, universityName: v })} />
                <Input label="Student Email" placeholder="e.g., 12345@dut4life.ac.za" value={form.studentEmail} onChange={(v) => setForm({ ...form, studentEmail: v })} />
              </div>
            </Section>

            <Section title="Residence Details">
              <Input required label="Residence Name" placeholder="e.g., Sunrise Student Village" value={form.residenceName} onChange={(v) => setForm({ ...form, residenceName: v })} />
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <Input label="Room / Unit Number" value={form.roomNumber} onChange={(v) => setForm({ ...form, roomNumber: v })} />
                <Input type="number" label="Estimated Population" placeholder="e.g., 500" value={form.estimatedResidencePopulation} onChange={(v) => setForm({ ...form, estimatedResidencePopulation: v })} />
              </div>
            </Section>

            <Section title="Selling Details">
              <Input label="Preferred Selling Area" placeholder="e.g., Blocks A & B" value={form.preferredSellingArea} onChange={(v) => setForm({ ...form, preferredSellingArea: v })} />
              <Textarea label="Previous Sales Experience" rows={2} value={form.previousSalesExperience} onChange={(v) => setForm({ ...form, previousSalesExperience: v })} />
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <Input label="Availability" placeholder="e.g., Weekends only" value={form.availability} onChange={(v) => setForm({ ...form, availability: v })} />
                <Input label="Time at Residence" placeholder="e.g., 2 years remaining" value={form.expectedTimeAtResidence} onChange={(v) => setForm({ ...form, expectedTimeAtResidence: v })} />
              </div>
              <Textarea label="Anything else you'd like us to know?" rows={3} value={form.additionalInfo} onChange={(v) => setForm({ ...form, additionalInfo: v })} />
            </Section>

            {error && <div className="p-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg">{error}</div>}

            <button type="submit" disabled={loading}
              className="w-full py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-medium rounded-lg">
              {loading ? 'Submitting...' : 'Submit Application'}
            </button>
          </form>
        </div>

        <p className="text-center text-xs text-gray-500 mt-6">
          Already a reseller? <Link href="/login" className="text-blue-600 hover:underline">Sign in here</Link>
        </p>
      </div>
    </div>
  );
}

function Section({ title, children }: any) {
  return (
    <div>
      <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">{title}</h2>
      <div className="space-y-3">{children}</div>
    </div>
  );
}

function Input({ label, value, onChange, type = 'text', required, placeholder }: any) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">
        {label} {required && <span className="text-red-500">*</span>}
      </label>
      <input
        type={type}
        required={required}
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
      />
    </div>
  );
}

function Textarea({ label, value, onChange, rows = 2, placeholder }: any) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      <textarea
        rows={rows}
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-3 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
      />
    </div>
  );
}
