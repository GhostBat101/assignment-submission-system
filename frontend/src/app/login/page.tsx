"use client";

/*
 * File: page.tsx (src/app/login/page.tsx)
 * Purpose: Authentication Login page allowing users to enter credentials or use 1-click Demo Account filler buttons.
 * 
 * Dependencies Used:
 * - src/context/AuthContext.tsx: Call `login()` action.
 * - src/lib/api.ts: `apiFetch` for calling `/auth/login`.
 * - src/types/index.ts: `LoginResponse` type.
 * 
 * Used By:
 * - Next.js Router (`/login` route).
 */

import React, { useState } from "react";
import { useAuth } from "../../context/AuthContext";
import { apiFetch } from "../../lib/api";
import { LoginResponse } from "../../types";
import { useRouter } from "next/navigation";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(false);
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { login } = useAuth();
  const router = useRouter();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      const response = await apiFetch<LoginResponse>("/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });

      login(response, rememberMe);

      // Redirect based on user role
      if (response.user.role === "Admin") {
        router.push("/admin");
      } else if (response.user.role === "Teacher") {
        router.push("/teacher");
      } else if (response.user.role === "Student") {
        router.push("/student");
      } else {
        router.push("/");
      }
    } catch (err: any) {
      setError(err.message || "Failed to log in. Please check your credentials.");
    } finally {
      setIsSubmitting(false);
    }
  };

  // Quick fill demo credentials helper for evaluators
  const fillDemoAccount = (role: "admin" | "teacher" | "student") => {
    if (role === "admin") {
      setEmail("admin@school.com");
      setPassword("Admin123!");
    } else if (role === "teacher") {
      setEmail("teacher@school.com");
      setPassword("Teacher123!");
    } else {
      setEmail("student@school.com");
      setPassword("Student123!");
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col justify-center items-center p-4">
      <div className="max-w-md w-full bg-white rounded-xl shadow-md border border-gray-200 p-8 space-y-6">
        <div className="text-center space-y-2">
          <h1 className="text-2xl font-bold text-gray-900">Sign in to your account</h1>
          <p className="text-sm text-gray-500">Assignment & Submission Management System</p>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleLogin} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email Address</label>
            <input
              type="email"
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-900"
              placeholder="e.g. teacher@school.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input
              type="password"
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-900"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          <div className="flex items-center">
            <input
              id="remember-me"
              type="checkbox"
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
            />
            <label htmlFor="remember-me" className="ml-2 block text-sm text-gray-900">
              Remember me
            </label>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2.5 rounded-lg transition duration-150 disabled:opacity-50"
          >
            {isSubmitting ? "Signing in..." : "Sign In"}
          </button>
        </form>

        {/* Evaluator Quick Demo Accounts */}
        <div className="border-t border-gray-200 pt-6">
          <p className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3 text-center">
            Evaluator 1-Click Demo Logins
          </p>
          <div className="grid grid-cols-3 gap-2">
            <button
              type="button"
              onClick={() => fillDemoAccount("admin")}
              className="px-2 py-1.5 bg-purple-50 text-purple-700 border border-purple-200 rounded text-xs font-medium hover:bg-purple-100 transition"
            >
              Demo Admin
            </button>
            <button
              type="button"
              onClick={() => fillDemoAccount("teacher")}
              className="px-2 py-1.5 bg-blue-50 text-blue-700 border border-blue-200 rounded text-xs font-medium hover:bg-blue-100 transition"
            >
              Demo Teacher
            </button>
            <button
              type="button"
              onClick={() => fillDemoAccount("student")}
              className="px-2 py-1.5 bg-green-50 text-green-700 border border-green-200 rounded text-xs font-medium hover:bg-green-100 transition"
            >
              Demo Student
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
