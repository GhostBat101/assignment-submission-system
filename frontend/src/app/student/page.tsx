"use client";

/*
 * File: page.tsx (src/app/student/page.tsx)
 * Purpose: Student Dashboard page for viewing assigned homework, submitting answers, updating submissions before deadlines, and checking grades/feedback.
 * 
 * Dependencies Used:
 * - src/context/AuthContext.tsx: Role verification.
 * - src/components/Navbar.tsx: Top navbar.
 * - src/lib/api.ts: apiFetch utility.
 * - src/types/index.ts: Assignment, Submission types.
 * 
 * Used By: Next.js App Router `/student` route.
 */

import React, { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";
import Navbar from "../../components/Navbar";
import { apiFetch } from "../../lib/api";
import { Assignment, Submission } from "../../types";
import { useRouter } from "next/navigation";

export default function StudentDashboard() {
  const { user, isLoading } = useAuth();
  const router = useRouter();

  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [submissions, setSubmissions] = useState<Record<number, Submission>>({});
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

  // Submit / Edit Submission Modal State
  const [activeAssignment, setActiveAssignment] = useState<Assignment | null>(null);
  const [answerText, setAnswerText] = useState("");
  const [attachmentUrl, setAttachmentUrl] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!isLoading) {
      if (!user || user.role !== "Student") {
        router.push("/login");
      } else {
        loadData();
      }
    }
  }, [user, isLoading, router]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [aList, sList] = await Promise.all([
        apiFetch<Assignment[]>("/student/assignments"),
        apiFetch<Submission[]>("/student/submissions"),
      ]);
      setAssignments(aList);

      // Map submissions by assignmentId
      const subMap: Record<number, Submission> = {};
      sList.forEach((s) => {
        subMap[s.assignmentId] = s;
      });
      setSubmissions(subMap);
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    } finally {
      setLoading(false);
    }
  };

  const handleOpenSubmitModal = (assignment: Assignment) => {
    setActiveAssignment(assignment);
    const existingSub = submissions[assignment.id];
    if (existingSub) {
      setAnswerText(existingSub.answerText || "");
      setAttachmentUrl(existingSub.attachmentUrl || "");
    } else {
      setAnswerText("");
      setAttachmentUrl("");
    }
  };

  const handleSubmitAnswer = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!activeAssignment) return;

    setMessage(null);
    setSubmitting(true);

    const existingSub = submissions[activeAssignment.id];

    try {
      if (existingSub) {
        // Update pre-deadline submission
        await apiFetch(`/student/submissions/${existingSub.id}`, {
          method: "PUT",
          body: JSON.stringify({
            assignmentId: activeAssignment.id,
            answerText,
            attachmentUrl: attachmentUrl || null,
          }),
        });
        setMessage({ type: "success", text: "Submission updated successfully!" });
      } else {
        // New submission
        await apiFetch("/student/submissions", {
          method: "POST",
          body: JSON.stringify({
            assignmentId: activeAssignment.id,
            answerText,
            attachmentUrl: attachmentUrl || null,
          }),
        });
        setMessage({ type: "success", text: "Assignment submitted successfully!" });
      }

      setActiveAssignment(null);
      loadData();
    } catch (err: any) {
      setMessage({ type: "error", text: err.message });
    } finally {
      setSubmitting(false);
    }
  };

  const isDeadlinePassed = (deadlineStr: string) => {
    return new Date() > new Date(deadlineStr);
  };

  if (isLoading || loading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="p-8 text-center text-gray-500">Loading Student Workspace...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <main className="max-w-5xl mx-auto p-6 space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Student Portal</h1>
          <p className="text-sm text-gray-500">View upcoming assignments, submit work, and check your grades</p>
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

        <div className="space-y-4">
          <h2 className="text-lg font-semibold text-gray-900">Assigned Homework ({assignments.length})</h2>

          {assignments.length === 0 ? (
            <div className="bg-white p-8 rounded-xl border border-gray-200 text-center text-gray-500 text-sm">
              No active assignments assigned to your enrolled courses.
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {assignments.map((a) => {
                const sub = submissions[a.id];
                const expired = isDeadlinePassed(a.deadline);

                return (
                  <div
                    key={a.id}
                    className="bg-white p-5 rounded-xl border border-gray-200 shadow-sm flex flex-col justify-between space-y-4"
                  >
                    <div className="space-y-2">
                      <div className="flex justify-between items-start">
                        <div>
                          <span className="text-xs font-semibold text-blue-600 bg-blue-50 px-2 py-0.5 rounded">
                            {a.subjectName}
                          </span>
                          <h3 className="font-bold text-gray-900 text-base mt-1">{a.title}</h3>
                        </div>
                        <span className="text-xs font-bold text-gray-700 bg-gray-100 px-2 py-1 rounded">
                          Max Marks: {a.maxMarks}
                        </span>
                      </div>

                      <p className="text-xs text-gray-500">Course: {a.courseName} • Instructor: {a.teacherName}</p>
                      <p className="text-xs text-gray-700 line-clamp-3 bg-gray-50 p-2.5 rounded border border-gray-100">
                        {a.description}
                      </p>
                    </div>

                    <div className="space-y-3 pt-2 border-t border-gray-100">
                      <div className="flex justify-between items-center text-xs">
                        <span className="text-gray-500">
                          Deadline:{" "}
                          <strong className={expired ? "text-red-600 font-bold" : "text-gray-800"}>
                            {new Date(a.deadline).toLocaleString()}
                          </strong>
                        </span>
                        {expired && <span className="text-xs text-red-600 font-semibold">(Expired)</span>}
                      </div>

                      {/* SUBMISSION STATUS OR GRADE CARD */}
                      {sub ? (
                        <div className="space-y-2">
                          <div className="flex justify-between items-center p-2 rounded bg-green-50 border border-green-200 text-xs">
                            <span className="font-semibold text-green-800">
                              Status: {sub.status}
                            </span>
                            <span className="text-gray-500 text-[11px]">
                              Submitted: {new Date(sub.submittedAt).toLocaleDateString()}
                            </span>
                          </div>

                          {sub.status === "Graded" ? (
                            <div className="p-3 bg-purple-50 rounded-lg border border-purple-200 text-xs space-y-1">
                              <div className="font-bold text-purple-900 text-sm">
                                Your Grade: {sub.marksAwarded} / {a.maxMarks}
                              </div>
                              {sub.feedback && (
                                <div className="text-purple-800">
                                  <strong>Teacher Feedback:</strong> {sub.feedback}
                                </div>
                              )}
                            </div>
                          ) : (
                            !expired && (
                              <button
                                onClick={() => handleOpenSubmitModal(a)}
                                className="w-full text-center text-xs bg-gray-100 hover:bg-gray-200 text-gray-800 font-medium py-1.5 rounded transition"
                              >
                                Edit Answer (Pre-Deadline)
                              </button>
                            )
                          )}
                        </div>
                      ) : (
                        <button
                          disabled={expired}
                          onClick={() => handleOpenSubmitModal(a)}
                          className={`w-full text-center text-xs font-semibold py-2 rounded-lg transition ${
                            expired
                              ? "bg-gray-100 text-gray-400 cursor-not-allowed"
                              : "bg-blue-600 hover:bg-blue-700 text-white"
                          }`}
                        >
                          {expired ? "Submission Closed" : "Submit Answer"}
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {/* SUBMIT ANSWER MODAL */}
        {activeAssignment && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
            <div className="bg-white rounded-xl max-w-lg w-full p-6 space-y-4 shadow-xl">
              <h2 className="text-lg font-bold text-gray-900">
                {submissions[activeAssignment.id] ? "Edit Submission" : "Submit Answer"}
              </h2>
              <div className="text-xs text-gray-500">
                Assignment: <strong className="text-gray-800">{activeAssignment.title}</strong>
              </div>

              <form onSubmit={handleSubmitAnswer} className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Your Answer Text</label>
                  <textarea
                    rows={5}
                    required
                    className="w-full px-3 py-2 border rounded-md text-xs text-gray-900 focus:ring-2 focus:ring-blue-500"
                    placeholder="Write your answer solution here..."
                    value={answerText}
                    onChange={(e) => setAnswerText(e.target.value)}
                  />
                </div>

                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">
                    Attachment / Project Link (Optional)
                  </label>
                  <input
                    type="url"
                    className="w-full px-3 py-1.5 border rounded-md text-xs text-gray-900"
                    placeholder="https://github.com/my-solution"
                    value={attachmentUrl}
                    onChange={(e) => setAttachmentUrl(e.target.value)}
                  />
                </div>

                <div className="flex justify-end space-x-2 pt-3">
                  <button
                    type="button"
                    onClick={() => setActiveAssignment(null)}
                    className="px-4 py-2 border rounded-lg text-xs text-gray-700 hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={submitting}
                    className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-semibold disabled:opacity-50"
                  >
                    {submitting ? "Submitting..." : "Confirm & Submit"}
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
