import Link from 'next/link';

export default function Home() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-center">
        <h1 className="text-3xl font-bold text-gray-800 mb-2">Ndlovu Bakery</h1>
        <p className="text-gray-500 mb-6">Admin Dashboard</p>
        <Link
          href="/login"
          className="inline-block px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg"
        >
          Sign in
        </Link>
      </div>
    </div>
  );
}
