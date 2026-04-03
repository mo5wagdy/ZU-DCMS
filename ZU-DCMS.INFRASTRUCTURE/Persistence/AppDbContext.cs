using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.INFRASTRUCTURE.Identity;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets for each entity in the domain model
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<InternDoctor> InternDoctors => Set<InternDoctor>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Clinic> Clinics => Set<Clinic>();
        public DbSet<Term> Terms => Set<Term>();
        public DbSet<TermRequirement> TermRequirements => Set<TermRequirement>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<DiagnosisRecord> DiagnosisRecords => Set<DiagnosisRecord>();
        public DbSet<CaseAssignment> CaseAssignments => Set<CaseAssignment>();
        public DbSet<CaseSession> CaseSessions => Set<CaseSession>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();

        // Override OnModelCreating to apply entity configurations
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Call the base method to ensure Identity configurations are applied
            base.OnModelCreating(builder);

            // Apply all configurations from the assembly containing AppDbContext
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
