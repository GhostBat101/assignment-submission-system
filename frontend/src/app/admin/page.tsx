"use client";

/*
 * File: page.tsx (src/app/admin/page.tsx)
 * Purpose: Admin Dashboard page for managing system users, courses, subjects, teacher assignments, and student enrollments.
 * 
 * Dependencies Used:
 * - src/context/AuthContext.tsx: Role verification.
 * - src/components/Navbar.tsx: Top navigation bar.
 * - src/lib/api.ts: apiFetch utility.
 * - src/types/index.ts: User, Course, Subject, CourseSubject types.
 * 
 * Used By: Next.js App Router `/admin` route.
 */

import React, { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";
import Navbar from "../../components/Navbar";
import { apiFetch } from "../../lib/api";
import { User, Course, Subject, CourseSubject, Assignment, Submission, PaginatedResponse } from "../../types";
import { useRouter } from "next/navigation";

export default function AdminDashboard() {
  const { user, isLoading } = useAuth();
  const router = useRouter();

  const [activeTab, setActiveTab] = useState<"users" | "courses" | "assignments" | "global">("users");

  // Data states
  const [users, setUsers] = useState<User[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [courseSubjects, setCourseSubjects] = useState<CourseSubject[]>([]);
  const [allAssignments, setAllAssignments] = useState<Assignment[]>([]);
  const [allSubmissions, setAllSubmissions] = useState<Submission[]>([]);
  const [loadingData, setLoadingData] = useState(true);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

  // User form
  const [newUser, setNewUser] = useState({ fullName: "", email: "", password: "", role: "Teacher" });
  // Course form
  const [newCourse, setNewCourse] = useState({ name: "", description: "" });
  // Subject form
  const [newSubject, setNewSubject] = useState({ name: "", code: "" });
  // Assign Teacher form
  const [assignForm, setAssignForm] = useState({ courseId: "", subjectId: "", teacherId: "" });
  // Enroll Student form
  const [enrollForm, setEnrollForm] = useState({ studentId: "", courseId: "" });

  useEffect(() => {
    if (!isLoading) {
      if (!user || user.role !== "Admin") {
        router.push("/login");
      } else {
        loadAllData();
      }
    }
  }, [user, isLoading, router]);

  const loadAllData = async () => {
    setLoadingData(true);
    try {
      const [uList, cList, sList, csList, aList, subList] = await Promise.all([
        apiFetch<User[]>("/admin/users"),
        apiFetch<Course[]>("/admin/courses"),
        apiFetch<Subject[]>("/admin/subjects"),
        apiFetch<CourseSubject[]>("/admin/course-subjects"),
        apiFetch<PaginatedResponse<Assignment>>("/admin/assignments?page=1&pageSize=50"),
        apiFetch<PaginatedResponse<Submission>>("/admin/submissions?page=1&pageSize=50"),
      ]);
      setUsers(uList);
      setCourses(cList);
      setSubjects(sList);
      setCourseSubjects(csList);
      setAllAssignments(aList.data);
      setAllSubmissions(subList.data);
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    } finally {
      setLoadingData(false);
    }
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    try {
      await apiFetch("/admin/users", {
        method: "POST",
        body: JSON.stringify({
          fullName: newUser.fullName,
          email: newUser.email,
          password: newUser.password,
          role: newUser.role === "Teacher" ? 2 : newUser.role === "Student" ? 3 : 1,
        }),
      });
      setMessage({ type: "success", text: "User created successfully!" });
      setNewUser({ fullName: "", email: "", password: "", role: "Teacher" });
      loadAllData();
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    }
  };

  const handleCreateCourse = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    try {
      await apiFetch("/admin/courses", {
        method: "POST",
        body: JSON.stringify(newCourse),
      });
      setMessage({ type: "success", text: "Course created successfully!" });
      setNewCourse({ name: "", description: "" });
      loadAllData();
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    }
  };

  const handleCreateSubject = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    try {
      await apiFetch("/admin/subjects", {
        method: "POST",
        body: JSON.stringify(newSubject),
      });
      setMessage({ type: "success", text: "Subject created successfully!" });
      setNewSubject({ name: "", code: "" });
      loadAllData();
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    }
  };

  const handleAssignTeacher = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    try {
      await apiFetch("/admin/assign-teacher", {
        method: "POST",
        body: JSON.stringify({
          courseId: Number(assignForm.courseId),
          subjectId: Number(assignForm.subjectId),
          teacherId: Number(assignForm.teacherId),
        }),
      });
      setMessage({ type: "success", text: "Teacher assigned to subject successfully!" });
      setAssignForm({ courseId: "", subjectId: "", teacherId: "" });
      loadAllData();
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    }
  };

  const handleEnrollStudent = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    try {
      await apiFetch("/admin/enroll-student", {
        method: "POST",
        body: JSON.stringify({
          studentId: Number(enrollForm.studentId),
          courseId: Number(enrollForm.courseId),
        }),
      });
      setMessage({ type: "success", text: "Student enrolled in course successfully!" });
      setEnrollForm({ studentId: "", courseId: "" });
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    }
  };

  if (isLoading || loadingData) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="p-8 text-center text-gray-500">Loading Admin Dashboard...</div>
      </div>
    );
  }

  const teachers = users.filter((u) => u.role === "Teacher");
  const students = users.filter((u) => u.role === "Student");

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="max-w-6xl mx-auto p-6 space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-2xl font-bold text-gray-900">Admin Control Panel</h1>
          <div className="flex space-x-2 bg-gray-200 p-1 rounded-lg">
            <button
              onClick={() => setActiveTab("users")}
              className={`px-4 py-1.5 text-sm font-medium rounded-md transition ${
                activeTab === "users" ? "bg-white text-gray-900 shadow-sm" : "text-gray-600 hover:text-gray-900"
              }`}
            >
              Users ({users.length})
            </button>
            <button
              onClick={() => setActiveTab("courses")}
              className={`px-4 py-1.5 text-sm font-medium rounded-md transition ${
                activeTab === "courses" ? "bg-white text-gray-900 shadow-sm" : "text-gray-600 hover:text-gray-900"
              }`}
            >
              Courses & Subjects
            </button>
            <button
              onClick={() => setActiveTab("assignments")}
              className={`px-4 py-1.5 text-sm font-medium rounded-md transition ${
                activeTab === "assignments" ? "bg-white text-gray-900 shadow-sm" : "text-gray-600 hover:text-gray-900"
              }`}
            >
              Enrollments & Setup
            </button>
            <button
              onClick={() => setActiveTab("global")}
              className={`px-4 py-1.5 text-sm font-medium rounded-md transition ${
                activeTab === "global" ? "bg-white text-gray-900 shadow-sm" : "text-gray-600 hover:text-gray-900"
              }`}
            >
              All Assignments & Submissions
            </button>
          </div>
        </div>

        {message && (
          <div
            className={`p-4 rounded-lg text-sm border ${
              message.type === "success"
                ? "bg-green-50 text-green-800 border-green-200"
                : "bg-red-50 text-red-800 border-red-200"
            }`}
          >
            {message.text}
          </div>
        )}

        {/* TAB 1: USERS */}
        {activeTab === "users" && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-1 bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-4">
              <h2 className="text-lg font-semibold text-gray-900">Create New User</h2>
              <form onSubmit={handleCreateUser} className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-gray-700">Full Name</label>
                  <input
                    type="text"
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                    value={newUser.fullName}
                    onChange={(e) => setNewUser({ ...newUser, fullName: e.target.value })}
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700">Email Address</label>
                  <input
                    type="email"
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                    value={newUser.email}
                    onChange={(e) => setNewUser({ ...newUser, email: e.target.value })}
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700">Password</label>
                  <input
                    type="password"
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                    value={newUser.password}
                    onChange={(e) => setNewUser({ ...newUser, password: e.target.value })}
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700">Role</label>
                  <select
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900 bg-white"
                    value={newUser.role}
                    onChange={(e) => setNewUser({ ...newUser, role: e.target.value })}
                  >
                    <option value="Teacher">Teacher</option>
                    <option value="Student">Student</option>
                    <option value="Admin">Admin</option>
                  </select>
                </div>
                <button
                  type="submit"
                  className="w-full bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium py-2 rounded-md transition"
                >
                  Create User
                </button>
              </form>
            </div>

            <div className="lg:col-span-2 bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-4">
              <h2 className="text-lg font-semibold text-gray-900">Registered Users</h2>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm text-gray-600">
                  <thead className="bg-gray-50 text-xs font-semibold text-gray-700 uppercase">
                    <tr>
                      <th className="p-3">ID</th>
                      <th className="p-3">Name</th>
                      <th className="p-3">Email</th>
                      <th className="p-3">Role</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {users.map((u) => (
                      <tr key={u.id}>
                        <td className="p-3 text-gray-400 font-mono">#{u.id}</td>
                        <td className="p-3 font-medium text-gray-900">{u.fullName}</td>
                        <td className="p-3 text-gray-600">{u.email}</td>
                        <td className="p-3">
                          <span
                            className={`px-2 py-0.5 rounded text-xs font-semibold ${
                              u.role === "Admin"
                                ? "bg-purple-100 text-purple-700"
                                : u.role === "Teacher"
                                ? "bg-blue-100 text-blue-700"
                                : "bg-green-100 text-green-700"
                            }`}
                          >
                            {u.role}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}

        {/* TAB 2: COURSES & SUBJECTS */}
        {activeTab === "courses" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-4">
              <h2 className="text-lg font-semibold text-gray-900">Manage Courses/Classes</h2>
              <form onSubmit={handleCreateCourse} className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-gray-700">Course Name</label>
                  <input
                    type="text"
                    required
                    placeholder="e.g. Grade 10 or CS 101"
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                    value={newCourse.name}
                    onChange={(e) => setNewCourse({ ...newCourse, name: e.target.value })}
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700">Description</label>
                  <input
                    type="text"
                    placeholder="Course details"
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                    value={newCourse.description}
                    onChange={(e) => setNewCourse({ ...newCourse, description: e.target.value })}
                  />
                </div>
                <button
                  type="submit"
                  className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-md transition"
                >
                  Add Course
                </button>
              </form>

              <div className="space-y-2 pt-2">
                <h3 className="text-xs font-semibold text-gray-500 uppercase">Existing Courses</h3>
                <ul className="divide-y divide-gray-100 text-sm">
                  {courses.map((c) => (
                    <li key={c.id} className="py-2 flex justify-between">
                      <span className="font-medium text-gray-900">{c.name}</span>
                      <span className="text-gray-500">{c.description}</span>
                    </li>
                  ))}
                </ul>
              </div>
            </div>

            <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-4">
              <h2 className="text-lg font-semibold text-gray-900">Manage Subjects</h2>
              <form onSubmit={handleCreateSubject} className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-gray-700">Subject Name</label>
                  <input
                    type="text"
                    required
                    placeholder="e.g. Mathematics"
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                    value={newSubject.name}
                    onChange={(e) => setNewSubject({ ...newSubject, name: e.target.value })}
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700">Subject Code</label>
                  <input
                    type="text"
                    required
                    placeholder="e.g. MATH-101"
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                    value={newSubject.code}
                    onChange={(e) => setNewSubject({ ...newSubject, code: e.target.value })}
                  />
                </div>
                <button
                  type="submit"
                  className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-md transition"
                >
                  Add Subject
                </button>
              </form>

              <div className="space-y-2 pt-2">
                <h3 className="text-xs font-semibold text-gray-500 uppercase">Existing Subjects</h3>
                <ul className="divide-y divide-gray-100 text-sm">
                  {subjects.map((s) => (
                    <li key={s.id} className="py-2 flex justify-between">
                      <span className="font-medium text-gray-900">{s.name}</span>
                      <span className="text-xs bg-gray-100 px-2 py-0.5 rounded text-gray-600 font-mono">
                        {s.code}
                      </span>
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          </div>
        )}

        {/* TAB 3: ASSIGNMENTS & ENROLLMENTS */}
        {activeTab === "assignments" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-4">
              <h2 className="text-lg font-semibold text-gray-900">Assign Teacher to Course Subject</h2>
              <form onSubmit={handleAssignTeacher} className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-gray-700">Select Course</label>
                  <select
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900 bg-white"
                    value={assignForm.courseId}
                    onChange={(e) => setAssignForm({ ...assignForm, courseId: e.target.value })}
                  >
                    <option value="">-- Choose Course --</option>
                    {courses.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700">Select Subject</label>
                  <select
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900 bg-white"
                    value={assignForm.subjectId}
                    onChange={(e) => setAssignForm({ ...assignForm, subjectId: e.target.value })}
                  >
                    <option value="">-- Choose Subject --</option>
                    {subjects.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.name} ({s.code})
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700">Assign Teacher</label>
                  <select
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900 bg-white"
                    value={assignForm.teacherId}
                    onChange={(e) => setAssignForm({ ...assignForm, teacherId: e.target.value })}
                  >
                    <option value="">-- Choose Teacher --</option>
                    {teachers.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.fullName} ({t.email})
                      </option>
                    ))}
                  </select>
                </div>
                <button
                  type="submit"
                  className="w-full bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium py-2 rounded-md transition"
                >
                  Assign Teacher
                </button>
              </form>

              <div className="pt-4 border-t border-gray-100">
                <h3 className="text-xs font-semibold text-gray-500 uppercase mb-2">Active Course-Subject Mappings</h3>
                <ul className="divide-y divide-gray-100 text-xs">
                  {courseSubjects.map((cs) => (
                    <li key={cs.id} className="py-2 flex justify-between items-center">
                      <div>
                        <span className="font-medium text-gray-900">{cs.courseName}</span>
                        <span className="text-gray-400"> → </span>
                        <span className="text-gray-700">{cs.subjectName}</span>
                      </div>
                      <span className="text-blue-600 bg-blue-50 px-2 py-0.5 rounded font-medium">
                        {cs.teacherName}
                      </span>
                    </li>
                  ))}
                </ul>
              </div>
            </div>

            <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-4">
              <h2 className="text-lg font-semibold text-gray-900">Enroll Student in Course</h2>
              <form onSubmit={handleEnrollStudent} className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-gray-700">Select Student</label>
                  <select
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900 bg-white"
                    value={enrollForm.studentId}
                    onChange={(e) => setEnrollForm({ ...enrollForm, studentId: e.target.value })}
                  >
                    <option value="">-- Choose Student --</option>
                    {students.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.fullName} ({s.email})
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700">Select Course</label>
                  <select
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900 bg-white"
                    value={enrollForm.courseId}
                    onChange={(e) => setEnrollForm({ ...enrollForm, courseId: e.target.value })}
                  >
                    <option value="">-- Choose Course --</option>
                    {courses.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                </div>
                <button
                  type="submit"
                  className="w-full bg-green-600 hover:bg-green-700 text-white text-sm font-medium py-2 rounded-md transition"
                >
                  Enroll Student
                </button>
              </form>
            </div>
          </div>
        )}

        {/* TAB 4: GLOBAL ASSIGNMENTS & SUBMISSIONS */}
        {activeTab === "global" && (
          <div className="space-y-6">
            <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-4">
              <h2 className="text-lg font-semibold text-gray-900">All Assignments (System-Wide)</h2>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm text-gray-600">
                  <thead className="bg-gray-50 text-xs font-semibold text-gray-700 uppercase">
                    <tr>
                      <th className="p-3">Title</th>
                      <th className="p-3">Course / Subject</th>
                      <th className="p-3">Teacher</th>
                      <th className="p-3">Deadline</th>
                      <th className="p-3">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {allAssignments.map((a) => (
                      <tr key={a.id}>
                        <td className="p-3 font-medium text-gray-900">{a.title}</td>
                        <td className="p-3">{a.courseName} - {a.subjectName}</td>
                        <td className="p-3">{a.teacherName}</td>
                        <td className="p-3">{new Date(a.deadline).toLocaleDateString()}</td>
                        <td className="p-3">
                          <span
                            className={`px-2 py-0.5 rounded text-xs font-semibold ${
                              a.status === "Published" ? "bg-green-100 text-green-700" : "bg-yellow-100 text-yellow-700"
                            }`}
                          >
                            {a.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm space-y-4">
              <h2 className="text-lg font-semibold text-gray-900">All Submissions (System-Wide)</h2>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm text-gray-600">
                  <thead className="bg-gray-50 text-xs font-semibold text-gray-700 uppercase">
                    <tr>
                      <th className="p-3">Student</th>
                      <th className="p-3">Assignment</th>
                      <th className="p-3">Submitted At</th>
                      <th className="p-3">Status</th>
                      <th className="p-3">Marks</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {allSubmissions.map((s) => (
                      <tr key={s.id}>
                        <td className="p-3 font-medium text-gray-900">
                          {s.studentName} <span className="block text-xs text-gray-400">{s.studentEmail}</span>
                        </td>
                        <td className="p-3">{s.assignmentTitle}</td>
                        <td className="p-3">{new Date(s.submittedAt).toLocaleString()}</td>
                        <td className="p-3">
                          <span
                            className={`px-2 py-0.5 rounded text-xs font-semibold ${
                              s.status === "Graded"
                                ? "bg-green-100 text-green-700"
                                : s.status === "Resubmitted"
                                ? "bg-orange-100 text-orange-700"
                                : "bg-blue-100 text-blue-700"
                            }`}
                          >
                            {s.status}
                          </span>
                        </td>
                        <td className="p-3 font-medium text-gray-900">
                          {s.marksAwarded !== null ? `${s.marksAwarded} / ${s.maxMarks}` : "—"}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
