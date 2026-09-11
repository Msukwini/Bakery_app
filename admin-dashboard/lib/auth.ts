'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';

export interface User {
  email: string;
  role: string;
  employeeId: string;
  token: string;
}

export function useAuth() {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const token = localStorage.getItem('token');
    const userData = localStorage.getItem('user');
    if (token && userData) {
      setUser({ ...JSON.parse(userData), token });
    } else {
      router.push('/login');
    }
    setLoading(false);
  }, [router]);

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    router.push('/login');
  };

  return { user, loading, logout };
}

export function saveAuth(token: string, user: Omit<User, 'token'>) {
  localStorage.setItem('token', token);
  localStorage.setItem('user', JSON.stringify(user));
}
