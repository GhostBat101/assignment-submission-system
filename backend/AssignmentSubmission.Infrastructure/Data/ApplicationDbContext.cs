/*
 * File: ApplicationDbContext.cs
 * Purpose: Entity Framework Core database context managing data entity configurations and DB queries.
 * 
 * Dependencies Used:
 * - Microsoft.EntityFrameworkCore
 * - All entity models in AssignmentSubmission.Core.Entities (User, Course, Subject, CourseSubject, StudentCourse, Assignment, Submission)
 * 
 * Used By:
 * - Program.cs (Registered in Dependency Injection container)
 * - EF Core Migrations
 * - Repository & Service classes for database operations
 */

using AssignmentSubmission.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSubmission.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<CourseSubject> CourseSubjects => Set<CourseSubject>();
        public DbSet<StudentCourse> StudentCourses => Set<StudentCourse>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<Submission> Submissions => Set<Submission>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // CourseSubject (Teacher assigned to Course + Subject)
            modelBuilder.Entity<CourseSubject>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseSubjects)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseSubject>()
                .HasOne(cs => cs.Subject)
                .WithMany(s => s.CourseSubjects)
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseSubject>()
                .HasOne(cs => cs.Teacher)
                .WithMany(u => u.TaughtSubjects)
                .HasForeignKey(cs => cs.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentCourse (Student enrolled in Course)
            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(u => u.StudentCourses)
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Assignment -> CourseSubject relationship
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.CourseSubject)
                .WithMany(cs => cs.Assignments)
                .HasForeignKey(a => a.CourseSubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Submission relationships
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
