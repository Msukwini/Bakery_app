'use client';

export default function ProfilePic({
  personId,
  hasPicture,
  firstName,
  lastName,
  size = 40,
}: {
  personId: string;
  hasPicture: boolean;
  firstName?: string;
  lastName?: string;
  size?: number;
}) {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://ndlovubakery.duckdns.org';

  if (hasPicture) {
    return (
      <img
        src={`${apiUrl}/api/Profile/picture/${personId}`}
        alt={`${firstName} ${lastName}`}
        className="rounded-full object-cover bg-gray-100"
        style={{ width: size, height: size }}
      />
    );
  }

  const initials = `${firstName?.[0] ?? ''}${lastName?.[0] ?? ''}`.toUpperCase() || '?';
  return (
    <div
      className="rounded-full bg-blue-100 text-blue-700 flex items-center justify-center font-semibold flex-shrink-0"
      style={{ width: size, height: size, fontSize: size * 0.4 }}
    >
      {initials}
    </div>
  );
}
