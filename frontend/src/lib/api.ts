/*
 * File: api.ts (src/lib/api.ts)
 * Purpose: Centralized HTTP client wrapper that handles API requests, appends JWT authorization tokens, and parses response errors.
 * 
 * Dependencies Used:
 * - src/types/index.ts: TypeScript data contracts.
 * 
 * Used By:
 * - AuthContext.tsx: Calling login and profile endpoints.
 * - All Frontend Pages: Communicating with the ASP.NET Core backend API.
 */

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api";

export async function apiFetch<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const token = typeof window !== "undefined" 
    ? (sessionStorage.getItem("auth_token") || localStorage.getItem("auth_token"))
    : null;

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    let errorMessage = "An unexpected error occurred.";
    try {
      const errorData = await response.json();
      errorMessage = errorData.message || errorData.title || JSON.stringify(errorData);
    } catch {
      errorMessage = `Server returned HTTP status ${response.status}`;
    }
    throw new Error(errorMessage);
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}
