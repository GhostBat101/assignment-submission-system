"use client";

/*
 * File: Navbar.tsx (src/components/Navbar.tsx)
 * Purpose: Top navigation bar displaying application branding, logged-in user profile badge, and logout button.
 * 
 * Dependencies Used:
 * - src/context/AuthContext.tsx: Access user identity and logout function.
 * 
 * Used By:
 * - Admin, Teacher, and Student dashboard layout pages.
 */

import React from "react";
import { useAuth } from "../context/AuthContext";
import { useRouter } from "next/navigation";

export default function Navbar() {
  const { user, logout } = useAuth();
  const router = useRouter();

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

  const getRoleBadgeColor = (role?: string) => {
    switch (role) {
      case "Admin":
        return "bg-purple-100 text-purple-800 border-purple-300";
      case "Teacher":
        return "bg-blue-100 text-blue-800 border-blue-300";
      case "Student":
        return "bg-green-100 text-green-800 border-green-300";
      default:
        return "bg-gray-100 text-gray-800 border-gray-300";
    }
  };

  return (
    <header className="bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between shadow-sm">
      <div className="flex items-center space-x-3">
        <div className="bg-blue-600 text-white font-bold px-3 py-1.5 rounded-lg text-lg tracking-wide">
          AMS
        </div>
        <span className="font-semibold text-gray-900 text-lg hidden sm:inline">
          Assignment & Submission System
        </span>
      </div>

      {user && (
        <div className="flex items-center space-x-4">
          <div className="text-right">
            <div className="text-sm font-medium text-gray-900">{user.fullName}</div>
            <div className="text-xs text-gray-500">{user.email}</div>
          </div>
          <span
            className={`text-xs font-semibold px-2.5 py-1 rounded-full border ${getRoleBadgeColor(
              user.role
            )}`}
          >
            {user.role}
          </span>
          <button
            onClick={handleLogout}
            className="text-xs text-red-600 hover:text-red-800 font-medium px-3 py-1.5 rounded border border-red-200 hover:border-red-400 transition"
          >
            Logout
          </button>
        </div>
      )}
    </header>
  );
}
