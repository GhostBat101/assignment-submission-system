/*
 * File: index.ts (src/types/index.ts)
 * Purpose: TypeScript type definitions matching backend DTOs and domain entities.
 * 
 * Dependencies Used: None
 * 
 * Used By:
 * - src/lib/api.ts: Typing API request and response payloads.
 * - src/context/AuthContext.tsx: User session typing.
 * - Frontend Pages & Components: Prop and state type checking.
 */

export type UserRole = "Admin" | "Teacher" | "Student";

export interface User {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface Course {
  id: number;
  name: string;
  description: string;
}

export interface Subject {
  id: number;
  name: string;
  code: string;
}

export interface CourseSubject {
  id: number;
  courseId: number;
  courseName: string;
  subjectId: number;
  subjectName: string;
  teacherId: number;
  teacherName: string;
}

export interface Assignment {
  id: number;
  title: string;
  description: string;
  maxMarks: number;
  deadline: string;
  status: "Draft" | "Published";
  createdAt: string;
  courseSubjectId: number;
  courseName: string;
  subjectName: string;
  teacherName: string;
}

export interface Submission {
  id: number;
  assignmentId: number;
  assignmentTitle: string;
  maxMarks: number;
  deadline: string;
  studentId: number;
  studentName: string;
  studentEmail: string;
  answerText: string;
  attachmentUrl?: string;
  submittedAt: string;
  updatedAt?: string;
  status: "Submitted" | "Graded" | "Resubmitted";
  marksAwarded?: number;
  feedback?: string;
  gradedAt?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
