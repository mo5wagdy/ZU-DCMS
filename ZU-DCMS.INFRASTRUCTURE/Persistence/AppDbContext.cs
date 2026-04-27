using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZU_DCMS.Domain.Entities;
using ZU_DCMS.INFRASTRUCTURE.Identity;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // __ Constructor that accepts DbContextOptions and passes it to the base class __ //
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // __________ DbSets for each entity in the domain model __________ //

        // __________ User-related entities __________ //
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<InternDoctor> InternDoctors => Set<InternDoctor>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<TeachingAssistant> TeachingAssistants => Set<TeachingAssistant>();

        // __________ Authentication-related entity __________ //
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // __ Clinic, Session, and Booking entities to manage clinical sessions and appointments __ //
        public DbSet<Clinic> Clinics => Set<Clinic>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Booking> Bookings => Set<Booking>();

        // __ Case management entities to handle patient cases, diagnoses, and assignments __ //
        public DbSet<DiagnosisRecord> DiagnosisRecords => Set<DiagnosisRecord>();
        public DbSet<CaseAssignment> CaseAssignments => Set<CaseAssignment>();
        public DbSet<CaseSession> CaseSessions => Set<CaseSession>();
        public DbSet<CaseReview> CaseReviews => Set <CaseReview>();

        // __ Entities for managing diagnoses and procedures __ //
        public DbSet<DiagnosisType> DiagnosisTypes => Set<DiagnosisType>();
        public DbSet<Procedure> Procedures => Set<Procedure>();
        public DbSet<ClinicDiagnosisType> ClinicDiagnosisTypes => Set<ClinicDiagnosisType>();
        public DbSet<ClinicProcedure> ClinicProcedures => Set<ClinicProcedure>();
        public DbSet<CaseSessionProcedure> CaseSessionProcedures => Set<CaseSessionProcedure>();

        // __ Academic entities to manage terms, requirements, and related information __ //
        public DbSet<Term> Terms => Set<Term>();
        public DbSet<TermRequirement> TermRequirements => Set<TermRequirement>();

        // __ Notification entity to handle user notifications __ //
        public DbSet<Notification> Notifications => Set<Notification>();

        // __ System configuration entity to store application settings __ //
        public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();


        // __ Override OnModelCreating to apply entity configurations __ //
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // => Call the base method to ensure Identity configurations are applied

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly); // => Apply all configurations from the assembly containing AppDbContext
        }
    }
}
