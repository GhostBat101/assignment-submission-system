"use client";

/*
 * File: page.tsx (src/app/teacher/page.tsx)
 * Purpose: Teacher Dashboard page for managing assignments (create, update, delete, draft/publish) and reviewing & grading student submissions.
 * 
 * Dependencies Used:
 * - src/context/AuthContext.tsx: Role verification.
 * - src/components/Navbar.tsx: Top navbar.
 * - src/lib/api.ts: apiFetch utility.
 * - src/types/index.ts: Assignment, Submission, CourseSubject types.
 * 
 * Used By: Next.js App Router `/teacher` route.
 */

import React, { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";
import Navbar from "../../components/Navbar";
import { apiFetch } from "../../lib/api";
import { Assignment, Submission, CourseSubject } from "../../types";
import { useRouter } from "next/navigation";

export default function TeacherDashboard() {
  const { user, isLoading } = useAuth();
  const router = useRouter();

  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [assignedSubjects, setAssignedSubjects] = useState<CourseSubject[]>([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

  // New Assignment Modal / Form State
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newAssignment, setNewAssignment] = useState({
    title: "",
    description: "",
    maxMarks: 100,
    deadline: "",
    courseSubjectId: "",
    isPublished: true,
  });

  // Selected Assignment for viewing Submissions
  const [selectedAssignment, setSelectedAssignment] = useState<Assignment | null>(null);
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [loadingSubmissions, setLoadingSubmissions] = useState(false);

  // Grading form state
  const [gradingSubmissionId, setGradingSubmissionId] = useState<number | null>(null);
  const [gradeForm, setGradeForm] = useState({ marks: 0, feedback: "" });

  useEffect(() => {
    if (!isLoading) {
      if (!user || user.role !== "Teacher") {
        router.push("/login");
      } else {
        loadData();
      }
    }
  }, [user, isLoading, router]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [aList, csList] = await Promise.all([
        apiFetch<Assignment[]>("/teacher/assignments"),
        apiFetch<CourseSubject[]>("/teacher/subjects"),
      ]);
      setAssignments(aList);
      setAssignedSubjects(csList);
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    } finally {
      setLoading(false);
    }
  };

  const handleCreateAssignment = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    try {
      await apiFetch("/teacher/assignments", {
        method: "POST",
        body: JSON.stringify({
          title: newAssignment.title,
          description: newAssignment.description,
          maxMarks: Number(newAssignment.maxMarks),
          deadline: new Date(newAssignment.deadline).toISOString(),
          courseSubjectId: Number(newAssignment.courseSubjectId),
          isPublished: newAssignment.isPublished,
        }),
      });
      setMessage({ type: "success", text: "Assignment created successfully!" });
      setShowCreateModal(false);
      setNewAssignment({
        title: "",
        description: "",
        maxMarks: 100,
        deadline: "",
        courseSubjectId: "",
        isPublished: true,
      });
      loadData();
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    }
  };

  const handleTogglePublish = async (assignmentId: number, currentStatus: string) => {
    setMessage(null);
    try {
      const willPublish = currentStatus === "Draft";
      await apiFetch(`/teacher/assignments/${assignmentId}/publish?publish=${willPublish}`, {
        method: "PATCH",
      });
      setMessage({
        type: "success",
        text: `Assignment ${willPublish ? "published" : "un-published to draft"} successfully.`,
      });
      loadData();
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    }
  };

  const handleDeleteAssignment = async (assignmentId: number) => {
    if (!confirm("Are you sure you want to delete this assignment?")) return;
    setMessage(null);
    try {
      await apiFetch(`/teacher/assignments/${assignmentId}`, { method: "DELETE" });
      setMessage({ type: "success", text: "Assignment deleted." });
      if (selectedAssignment?.id === assignmentId) {
        setSelectedAssignment(null);
      }
      loadData();
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    }
  };

  const handleViewSubmissions = async (assignment: Assignment) => {
    setSelectedAssignment(assignment);
    setLoadingSubmissions(true);
    try {
      const subList = await apiFetch<Submission[]>(`/teacher/assignments/${assignment.id}/submissions`);
      setSubmissions(subList);
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    } finally {
      setLoadingSubmissions(false);
    }
  };

  const handleGradeSubmission = async (submissionId: number) => {
    setMessage(null);
    try {
      await apiFetch(`/teacher/submissions/${submissionId}/grade`, {
        method: "POST",
        body: JSON.stringify({
          marks: Number(gradeForm.marks),
          feedback: gradeForm.feedback,
        }),
      });
      setMessage({ type: "success", text: "Grade & Feedback saved successfully!" });
      setGradingSubmissionId(null);
      if (selectedAssignment) {
        handleViewSubmissions(selectedAssignment);
      }
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    }
  };

  if (isLoading || loading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="p-8 text-center text-gray-500">Loading Teacher Dashboard...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="max-w-6xl mx-auto p-6 space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Teacher Workspace</h1>
            <p className="text-sm text-gray-500">Manage assignments and review student submissions</p>
          </div>
          <button
            onClick={() => setShowCreateModal(true)}
            className="bg-blue-600 hover:bg-blue-700 text-white font-medium px-4 py-2 rounded-lg text-sm shadow-sm transition"
          >
            + Create Assignment
          </button>
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

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* ASSIGNMENTS LISTING */}
          <div className="lg:col-span-2 space-y-4">
            <h2 className="text-lg font-semibold text-gray-900">Your Assignments ({assignments.length})</h2>

            {assignments.length === 0 ? (
              <div className="bg-white p-8 rounded-xl border border-gray-200 text-center text-gray-500 text-sm">
                No assignments created yet. Click "+ Create Assignment" to start.
              </div>
            ) : (
              <div className="space-y-3">
                {assignments.map((a) => (
                  <div
                    key={a.id}
                    className={`bg-white p-5 rounded-xl border transition shadow-sm space-y-3 ${
                      selectedAssignment?.id === a.id ? "border-blue-500 ring-2 ring-blue-100" : "border-gray-200"
                    }`}
                  >
                    <div className="flex justify-between items-start">
                      <div>
                        <div className="flex items-center space-x-2">
                          <h3 className="font-semibold text-gray-900 text-base">{a.title}</h3>
                          <span
                            className={`text-xs px-2 py-0.5 rounded font-semibold ${
                              a.status === "Published"
                                ? "bg-green-100 text-green-700"
                                : "bg-yellow-100 text-yellow-800"
                            }`}
                          >
                            {a.status}
                          </span>
                        </div>
                        <p className="text-xs text-gray-500 mt-0.5">
                          {a.courseName} • <span className="font-medium text-gray-700">{a.subjectName}</span>
                        </p>
                      </div>
                      <div className="text-right">
                        <span className="text-xs font-bold text-gray-900 bg-gray-100 px-2 py-1 rounded">
                          Max Marks: {a.maxMarks}
                        </span>
                      </div>
                    </div>

                    <p className="text-sm text-gray-600 line-clamp-2">{a.description}</p>

                    <div className="flex justify-between items-center pt-2 border-t border-gray-100 text-xs">
                      <span className="text-gray-500">
                        Deadline: <strong className="text-gray-700">{new Date(a.deadline).toLocaleString()}</strong>
                      </span>

                      <div className="flex space-x-2">
                        <button
                          onClick={() => handleTogglePublish(a.id, a.status)}
                          className="px-2.5 py-1 text-xs border rounded text-gray-700 hover:bg-gray-50"
                        >
                          {a.status === "Published" ? "Unpublish (Draft)" : "Publish"}
                        </button>
                        <button
                          onClick={() => handleViewSubmissions(a)}
                          className="px-3 py-1 text-xs bg-blue-50 text-blue-700 font-medium rounded hover:bg-blue-100"
                        >
                          View Submissions
                        </button>
                        <button
                          onClick={() => handleDeleteAssignment(a.id)}
                          className="px-2.5 py-1 text-xs text-red-600 hover:text-red-800"
                        >
                          Delete
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* SUBMISSIONS PANEL FOR SELECTED ASSIGNMENT */}
          <div className="lg:col-span-1 space-y-4">
            <h2 className="text-lg font-semibold text-gray-900">
              {selectedAssignment ? `Submissions (${submissions.length})` : "Student Submissions"}
            </h2>

            {!selectedAssignment ? (
              <div className="bg-white p-6 rounded-xl border border-gray-200 text-center text-gray-400 text-sm">
                Select an assignment on the left to view and grade student submissions.
              </div>
            ) : loadingSubmissions ? (
              <div className="bg-white p-6 rounded-xl border border-gray-200 text-center text-gray-400 text-sm">
                Loading submissions...
              </div>
            ) : submissions.length === 0 ? (
              <div className="bg-white p-6 rounded-xl border border-gray-200 text-center text-gray-500 text-sm">
                No students have submitted answers for this assignment yet.
              </div>
            ) : (
              <div className="space-y-4">
                {submissions.map((sub) => (
                  <div key={sub.id} className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm space-y-3">
                    <div className="flex justify-between items-start">
                      <div>
                        <div className="font-semibold text-sm text-gray-900">{sub.studentName}</div>
                        <div className="text-xs text-gray-500">{sub.studentEmail}</div>
                      </div>
                      <span
                        className={`text-xs font-semibold px-2 py-0.5 rounded ${
                          sub.status === "Graded"
                            ? "bg-purple-100 text-purple-700"
                            : "bg-blue-100 text-blue-700"
                        }`}
                      >
                        {sub.status}
                      </span>
                    </div>

                    <div className="bg-gray-50 p-2.5 rounded border border-gray-100 text-xs text-gray-800 space-y-1">
                      <div className="font-semibold text-gray-600">Answer Submission:</div>
                      <p className="whitespace-pre-wrap">{sub.answerText}</p>
                      {sub.attachmentUrl && (
                        <div className="pt-1 text-blue-600 underline">
                          <a href={sub.attachmentUrl} target="_blank" rel="noreferrer">
                            View Attachment Link
                          </a>
                        </div>
                      )}
                    </div>

                    {sub.status === "Graded" && gradingSubmissionId !== sub.id ? (
                      <div className="bg-green-50 p-2.5 rounded border border-green-100 text-xs text-green-900 space-y-1">
                        <div className="font-bold">
                          Grade: {sub.marksAwarded} / {sub.maxMarks}
                        </div>
                        {sub.feedback && <div>Feedback: {sub.feedback}</div>}
                        <button
                          onClick={() => {
                            setGradingSubmissionId(sub.id);
                            setGradeForm({ marks: sub.marksAwarded || 0, feedback: sub.feedback || "" });
                          }}
                          className="text-xs text-blue-600 hover:underline pt-1 block"
                        >
                          Edit Grade
                        </button>
                      </div>
                    ) : (
                      <div>
                        {gradingSubmissionId === sub.id ? (
                          <div className="space-y-2 pt-2 border-t border-gray-100">
                            <div>
                              <label className="block text-xs font-medium text-gray-700">
                                Marks (Max: {selectedAssignment.maxMarks})
                              </label>
                              <input
                                type="number"
                                max={selectedAssignment.maxMarks}
                                min={0}
                                required
                                className="w-full px-2 py-1 border rounded text-xs text-gray-900"
                                value={gradeForm.marks}
                                onChange={(e) => setGradeForm({ ...gradeForm, marks: Number(e.target.value) })}
                              />
                            </div>
                            <div>
                              <label className="block text-xs font-medium text-gray-700">Teacher Feedback</label>
                              <textarea
                                rows={2}
                                className="w-full px-2 py-1 border rounded text-xs text-gray-900"
                                placeholder="Write feedback..."
                                value={gradeForm.feedback}
                                onChange={(e) => setGradeForm({ ...gradeForm, feedback: e.target.value })}
                              />
                            </div>
                            <div className="flex space-x-2">
                              <button
                                type="button"
                                onClick={() => handleGradeSubmission(sub.id)}
                                className="flex-1 bg-green-600 hover:bg-green-700 text-white text-xs font-medium py-1.5 rounded"
                              >
                                Save Grade
                              </button>
                              <button
                                type="button"
                                onClick={() => setGradingSubmissionId(null)}
                                className="px-3 bg-gray-100 hover:bg-gray-200 text-gray-700 text-xs rounded"
                              >
                                Cancel
                              </button>
                            </div>
                          </div>
                        ) : (
                          <button
                            onClick={() => {
                              setGradingSubmissionId(sub.id);
                              setGradeForm({ marks: 0, feedback: "" });
                            }}
                            className="w-full bg-blue-600 hover:bg-blue-700 text-white text-xs font-medium py-1.5 rounded transition"
                          >
                            Grade Submission
                          </button>
                        )}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* CREATE ASSIGNMENT MODAL */}
        {showCreateModal && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
            <div className="bg-white rounded-xl max-w-lg w-full p-6 space-y-4 shadow-xl">
              <h2 className="text-lg font-bold text-gray-900">Create New Assignment</h2>

              <form onSubmit={handleCreateAssignment} className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-gray-700">Select Subject/Course</label>
                  <select
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900 bg-white"
                    value={newAssignment.courseSubjectId}
                    onChange={(e) => setNewAssignment({ ...newAssignment, courseSubjectId: e.target.value })}
                  >
                    <option value="">-- Choose Assigned Subject --</option>
                    {assignedSubjects.map((cs) => (
                      <option key={cs.id} value={cs.id}>
                        {cs.courseName} - {cs.subjectName}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-700">Title</label>
                  <input
                    type="text"
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                    placeholder="Assignment Title"
                    value={newAssignment.title}
                    onChange={(e) => setNewAssignment({ ...newAssignment, title: e.target.value })}
                  />
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-700">Description</label>
                  <textarea
                    rows={3}
                    required
                    className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                    placeholder="Detailed instructions..."
                    value={newAssignment.description}
                    onChange={(e) => setNewAssignment({ ...newAssignment, description: e.target.value })}
                  />
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-gray-700">Max Marks</label>
                    <input
                      type="number"
                      required
                      min={1}
                      className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                      value={newAssignment.maxMarks}
                      onChange={(e) => setNewAssignment({ ...newAssignment, maxMarks: Number(e.target.value) })}
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-medium text-gray-700">Deadline</label>
                    <input
                      type="datetime-local"
                      required
                      className="w-full px-3 py-1.5 border rounded-md text-sm text-gray-900"
                      value={newAssignment.deadline}
                      onChange={(e) => setNewAssignment({ ...newAssignment, deadline: e.target.value })}
                    />
                  </div>
                </div>

                <div className="flex items-center space-x-2 pt-2">
                  <input
                    type="checkbox"
                    id="isPublished"
                    checked={newAssignment.isPublished}
                    onChange={(e) => setNewAssignment({ ...newAssignment, isPublished: e.target.checked })}
                  />
                  <label htmlFor="isPublished" className="text-xs text-gray-700 font-medium">
                    Publish immediately (Uncheck to save as Draft)
                  </label>
                </div>

                <div className="flex justify-end space-x-2 pt-3">
                  <button
                    type="button"
                    onClick={() => setShowCreateModal(false)}
                    className="px-4 py-2 border rounded-lg text-sm text-gray-700 hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium"
                  >
                    Save Assignment
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
