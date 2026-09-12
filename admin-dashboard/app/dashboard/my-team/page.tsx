'use client';

import { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { Users, RefreshCw, MapPin, Phone, GraduationCap } from 'lucide-react';
import ProfilePic from '@/components/ProfilePic';

interface TeamMember {
  resellerEmployeeId: string;
  resellerCode: string;
  personId: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  universityName: string | null;
  hasProfilePicture: boolean;
  residenceName: string | null;
  assignmentStartDate: string;
}

export default function MyTeamPage() {
  const [team, setTeam] = useState<TeamMember[]>([]);
  const [deliveryPerson, setDeliveryPerson] = useState<any>(null);
  const [role, setRole] = useState<string>('');
  const [loading, setLoading] = useState(true);

  const load = async () => {
    setLoading(true);
    try {
      const res = await api.get('/api/Team/my-team');
      setRole(res.data.role);
      if (res.data.team) setTeam(res.data.team);
      if (res.data.deliveryPerson) setDeliveryPerson(res.data.deliveryPerson);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  return (
    <div>
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6">
        <div>
          <h1 className="text-xl sm:text-2xl font-bold text-gray-800">
            {role === 'Delivery' ? 'My Team' : 'My Delivery Person'}
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            {role === 'Delivery'
              ? 'Resellers you are assigned to deliver to'
              : 'The person who delivers your stock'}
          </p>
        </div>
        <button onClick={load} className="flex items-center gap-2 px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg self-start sm:self-auto">
          <RefreshCw size={14} /> Refresh
        </button>
      </div>

      {loading ? <p className="text-gray-500">Loading...</p> : role === 'Reseller' ? (
        deliveryPerson ? (
          <div className="bg-white rounded-xl border p-6 max-w-md">
            <div className="flex items-center gap-4">
              <ProfilePic
                personId={deliveryPerson.personId}
                hasPicture={deliveryPerson.hasProfilePicture}
                firstName={deliveryPerson.firstName}
                lastName={deliveryPerson.lastName}
                size={80}
              />
              <div className="flex-1 min-w-0">
                <h3 className="font-semibold text-gray-800">
                  {deliveryPerson.firstName} {deliveryPerson.lastName}
                </h3>
                <p className="text-xs text-gray-500 mt-0.5">{deliveryPerson.employeeCode}</p>
                {deliveryPerson.isTemporary && (
                  <span className="inline-block mt-2 px-2 py-0.5 rounded text-[10px] bg-orange-100 text-orange-800 font-medium">
                    TEMPORARY COVER
                  </span>
                )}
              </div>
            </div>
            <div className="flex items-center gap-2 mt-4 text-sm text-gray-600">
              <Phone size={14} />
              <span>{deliveryPerson.phoneNumber}</span>
            </div>
          </div>
        ) : (
          <div className="bg-white p-8 rounded-xl border text-center text-gray-500">
            No delivery person assigned yet. Contact admin.
          </div>
        )
      ) : (
        <>
          {team.length === 0 ? (
            <div className="bg-white p-8 sm:p-12 rounded-xl border text-center">
              <Users className="mx-auto mb-3 text-gray-300" size={48} />
              <p className="text-gray-600 font-medium">No resellers assigned to you</p>
              <p className="text-sm text-gray-500 mt-1">Contact admin to be assigned to a residence</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {team.map((m) => (
                <div key={m.resellerEmployeeId} className="bg-white rounded-xl border p-5">
                  <div className="flex items-start gap-4 mb-4">
                    <ProfilePic
                      personId={m.personId}
                      hasPicture={m.hasProfilePicture}
                      firstName={m.firstName}
                      lastName={m.lastName}
                      size={64}
                    />
                    <div className="flex-1 min-w-0">
                      <h3 className="font-semibold text-gray-800 truncate">
                        {m.firstName} {m.lastName}
                      </h3>
                      <p className="text-xs text-gray-500 mt-0.5">{m.resellerCode}</p>
                    </div>
                  </div>

                  <div className="space-y-2 text-sm">
                    {m.residenceName && (
                      <div className="flex items-start gap-2 text-gray-600">
                        <MapPin size={14} className="mt-0.5 flex-shrink-0" />
                        <span className="text-xs">{m.residenceName}</span>
                      </div>
                    )}
                    <div className="flex items-center gap-2 text-gray-600">
                      <Phone size={14} />
                      <span className="text-xs">{m.phoneNumber}</span>
                    </div>
                    {m.universityName && (
                      <div className="flex items-center gap-2 text-gray-600">
                        <GraduationCap size={14} />
                        <span className="text-xs">{m.universityName}</span>
                      </div>
                    )}
                  </div>

                  <div className="mt-4 pt-4 border-t border-gray-100 flex justify-between items-center text-xs text-gray-500">
                    <span>Since {new Date(m.assignmentStartDate).toLocaleDateString('en-ZA', { day: '2-digit', month: 'short', year: 'numeric' })}</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}
