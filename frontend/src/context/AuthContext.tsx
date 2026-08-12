"use client";

/*
 * File: AuthContext.tsx (src/context/AuthContext.tsx)
 * Purpose: Global React Context managing logged-in user state, JWT token persistence in LocalStorage, and login/logout handlers.
 * 
 * Dependencies Used:
 * - src/types/index.ts: User and LoginResponse types.
 * - src/lib/api.ts: apiFetch utility for calling `/auth/me`.
 * 
 * Used By:
 * - src/app/layout.tsx: Context Provider wrapper.
 * - All components & pages via `useAuth()` hook.
 */

import React, { createContext, useContext, useEffect, useState } from "react";
import { User, LoginResponse } from "../types";
import { apiFetch } from "../lib/api";

interface AuthContextType {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  login: (data: LoginResponse, rememberMe: boolean) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    // Check sessionStorage first, then localStorage
    const storedToken = sessionStorage.getItem("auth_token") || localStorage.getItem("auth_token");
    if (storedToken) {
      setToken(storedToken);
      apiFetch<User>("/auth/me")
        .then((userData) => {
          setUser(userData);
        })
        .catch(() => {
          localStorage.removeItem("auth_token");
          sessionStorage.removeItem("auth_token");
          setToken(null);
          setUser(null);
        })
        .finally(() => setIsLoading(false));
    } else {
      setIsLoading(false);
    }
  }, []);

  const login = (data: LoginResponse, rememberMe: boolean) => {
    if (rememberMe) {
      localStorage.setItem("auth_token", data.token);
    } else {
      sessionStorage.setItem("auth_token", data.token);
    }
    setToken(data.token);
    setUser(data.user);
  };

  const logout = () => {
    localStorage.removeItem("auth_token");
    sessionStorage.removeItem("auth_token");
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, token, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
