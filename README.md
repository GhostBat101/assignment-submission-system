# Assignment & Submission Management System

A role-based web application built for schools and colleges to manage assignments, student submissions, and teacher evaluations. Built as an individual recruitment assignment for the Assistant Software Engineer role at OnnoRokom Projukti Limited.

---

## ⚡ 1-Click Windows Batch Launcher for Evaluators

Simply double-click **`run.bat`** in the root directory!

It will automatically:
1. Start the ASP.NET Core Web API server (`http://localhost:5000`).
2. Start the Next.js Frontend application (`http://localhost:3000`).
3. Seed the database with all Demo Accounts and sample assignments.
4. Automatically open your default web browser directly to `http://localhost:3000`.

---

## 🔑 Demo Login Credentials

For quick evaluation, pre-configured demo accounts are automatically seeded into the database upon application launch. You can also use the **1-Click Demo Buttons** directly on the Login page.

| Role | Email | Password | Allowed Access Scope |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@school.com` | `Admin123!` | User management, Course & Subject creation, Teacher assignments, Student enrollments. |
| **Teacher** | `teacher@school.com` | `Teacher123!` | Create/Edit/Delete/Publish assignments, view submissions, assign grades & feedback. |
| **Student** | `student@school.com` | `Student123!` | View assigned homework, submit answers, update work pre-deadline, check grades & feedback. |

---

## 🛠️ Technology Stack

- **Backend:** ASP.NET Core 8 Web API (C#), RESTful architecture, JWT Authentication, Swagger/OpenAPI.
- **Frontend:** Next.js 15+ (App Router), React, TypeScript, Tailwind CSS, Fetch API client.
- **Database:** PostgreSQL (with Entity Framework Core 8 ORM and automatic fallback to EF Core In-Memory DB / SQLite for zero-config local runs).
- **Testing:** xUnit unit test framework for business rules and authorization workflows.
- **Containerization:** Docker & Docker Compose.

---

## 📁 Project Architecture & Structure

The codebase is organized following N-Tier Clean Architecture principles:

```text
├── run.bat                                 # 1-Click Windows Launcher for Evaluators
├── backend/
│   ├── AssignmentSubmission.Api/           # API Controllers & HTTP Pipeline (AdminController, TeacherController, StudentController, AuthController)
│   ├── AssignmentSubmission.Core/          # Entities, DTOs & Enums (User, Course, Subject, Assignment, Submission)
│   ├── AssignmentSubmission.Infrastructure/# ApplicationDbContext, EF Core Migrations, PBKDF2 PasswordHasher, DbInitializer
│   ├── AssignmentSubmission.Services/      # Business Logic Services (AdminService, TeacherService, StudentService, AuthService)
│   └── AssignmentSubmission.Tests/         # xUnit Automated Unit Tests
├── frontend/
│   ├── src/app/                            # Next.js App Router Pages (/login, /admin, /teacher, /student)
│   ├── src/components/                     # Reusable UI Components (Navbar, Badges, Modals)
│   ├── src/context/                        # AuthContext for session management
│   ├── src/lib/                            # API Fetch client wrapper with JWT injection
│   └── src/types/                          # TypeScript interface contracts matching backend DTOs
└── docker-compose.yml                      # Single-command Docker environment orchestration
```

### Code Documentation Standard
Every source file across both backend (`.cs`) and frontend (`.ts`/`.tsx`) includes a structured top-of-file documentation header explaining:
1. **Purpose:** What the file does.
2. **Dependencies Used:** What modules/entities the file imports.
3. **Used By:** Where and how the file is consumed across the system.

---

## 🚀 Setup & Execution Options

### Option 1: 1-Click Windows Batch Script (Easiest)
Double-click `run.bat` in the root folder.

### Option 2: 1-Click Docker Setup
If you prefer running via Docker containers, execute:
```bash
docker-compose up --build
```
- **Frontend UI:** `http://localhost:3000`
- **Backend API:** `http://localhost:5000/api`
- **Swagger Documentation:** `http://localhost:5000/swagger`

---

## 🧪 Running Automated Unit Tests

The backend includes xUnit unit tests validating critical business rules (e.g., deadline enforcement, course enrollment access control, teacher assignment ownership, grade upper bounds).

To execute the unit test suite:

```bash
cd backend
dotnet test
```

---

## 💡 Documented Assumptions (The "Hidden Tests")

Following the directive to make "reasonable assumptions" for edge cases not explicitly defined in the requirements, I implemented the following business rules:

1. **Draft Privacy:** Students cannot fetch or view assignments that a Teacher has marked as "Draft". They only see "Published" assignments.
2. **Strict Deadline Enforcement:** If `DateTime.UtcNow > Assignment.Deadline`, the backend hard-blocks submissions/updates (returning `400 Bad Request`).
3. **Grading Lockouts:** If a Teacher has already graded a submission, the student is locked out of updating it, even if the deadline hasn't passed yet.
4. **Grade Bounds Validation:** A Teacher cannot award more marks than the `MaximumMarks` defined for the assignment (enforced in backend and validated via xUnit tests).
5. **Teacher Ownership:** A Teacher can only view, edit, and grade submissions for assignments *they* created. They cannot interfere with another teacher's subject.
6. **Password Security:** Although the requirements just specified "Login, JWT-based authentication", storing plaintext passwords is a severe security risk. I implemented PBKDF2 SHA-256 password hashing.
7. **Database Seeding:** To ensure the evaluator can set up the database without manually executing SQL scripts, I implemented `DbInitializer` which automatically creates the schema and injects Demo Users, Courses, and Subjects on the first run.
8. **Admin Global View:** Admin can view all assignments and submissions system-wide across all teachers and students.

---

## 🔧 Version Compatibility & Troubleshooting

To avoid version mismatches and ensure a flawless evaluation, please note the following environment requirements if running manually (without Docker):

### 1. Node.js Version (Frontend)
- **Requirement:** Node.js **v18.18.0 or higher** (v20 LTS recommended).
- **Why:** The frontend is built with Next.js 15 and React 19, which strictly require modern Node.js versions. Running `npm run dev` with Node v16 or older will fail with syntax errors.
- **Tip:** Use `nvm` (Node Version Manager) to easily switch to Node 20 if needed.

### 2. .NET SDK Version (Backend)
- **Requirement:** .NET 8.0 SDK.
- **Why:** The backend targets `net8.0`. Running `dotnet build` on a machine with only .NET 6 or 7 will result in a targeting framework error.
- **Note on `run.bat`:** If you are using Windows, the `run.bat` script automatically searches `%LocalAppData%\Microsoft\dotnet\dotnet.exe` to bypass common system `PATH` registration issues.

### 3. Database Engine (PostgreSQL vs SQLite)
- **Requirement:** PostgreSQL 15+ (if using Postgres).
- **The SQLite Fallback (Zero-Setup):** The requirement asked for PostgreSQL. However, to ensure the app doesn't crash on machines without a running Postgres server, I built an **automatic fallback to SQLite** (`assignment_system.db`). 
  - If you run locally via `run.bat` or `dotnet run` (without setting `USE_POSTGRES=true`), it gracefully writes all data to a local file database. No installation required!
  - If you use `docker-compose up`, it provisions and connects to a real PostgreSQL 16 container automatically.

### 4. Package Manager
- **Requirement:** `npm` (v9 or higher).
- **Why:** The frontend contains a `package-lock.json`. Please use `npm install` rather than `yarn` or `pnpm` to ensure deterministic dependency resolution.

### 🆘 The Ultimate Fallback: Docker
If you encounter **any** version mismatch issues on your host machine (e.g., wrong Node version, wrong .NET version), you can bypass them entirely by using Docker:
```bash
docker-compose up --build
```
This isolates the environment and guarantees 100% compatibility regardless of what is installed on your local OS.
