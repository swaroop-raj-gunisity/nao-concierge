using Microsoft.EntityFrameworkCore;
using NaoConcierge.Domain.Entities;
using NaoConcierge.Domain.ValueObjects;

namespace NaoConcierge.Infrastructure.Persistence;

public class NaoConciergeDbContext : DbContext
{
    public NaoConciergeDbContext(DbContextOptions<NaoConciergeDbContext> options) : base(options) { }

    public DbSet<IntakeCase> IntakeCases => Set<IntakeCase>();
    public DbSet<CaseAttribute> CaseAttributes => Set<CaseAttribute>();
    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<JointOwner> JointOwners => Set<JointOwner>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<TrustedContact> TrustedContacts => Set<TrustedContact>();
    public DbSet<EntityInfo> EntityInfos => Set<EntityInfo>();
    public DbSet<ResponsibleParty> ResponsibleParties => Set<ResponsibleParty>();
    public DbSet<FundingInfo> FundingInfos => Set<FundingInfo>();
    public DbSet<ConversationTurn> ConversationTurns => Set<ConversationTurn>();
    public DbSet<EmbeddingDocument> EmbeddingDocuments => Set<EmbeddingDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IntakeCase>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Applicant).WithOne().HasForeignKey<Applicant>(a => a.CaseId);
            e.HasMany(x => x.Attributes).WithOne().HasForeignKey(a => a.CaseId);
            e.HasMany(x => x.ConversationHistory).WithOne().HasForeignKey(c => c.CaseId);
            e.HasMany(x => x.JointOwners).WithOne().HasForeignKey(j => j.CaseId);
            e.HasMany(x => x.Beneficiaries).WithOne().HasForeignKey(b => b.CaseId);
            e.HasOne(x => x.TrustedContact).WithOne().HasForeignKey<TrustedContact>(t => t.CaseId);
            e.HasOne(x => x.FundingInfo).WithOne().HasForeignKey<FundingInfo>(f => f.CaseId);
            e.HasOne(x => x.EntityInfo).WithOne().HasForeignKey<EntityInfo>(ei => ei.CaseId);
            e.Property(x => x.RegistrationType).HasConversion<string>();
            e.Property(x => x.AdvisoryProgram).HasConversion<string>();
            e.Property(x => x.Purpose).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<CaseAttribute>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.State).HasConversion<string>();
            e.OwnsOne(x => x.Confidence, c => c.Property(p => p.Value).HasColumnName("ConfidenceValue"));
        });

        modelBuilder.Entity<Applicant>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.LegalName);
            e.OwnsOne(x => x.LegalAddress);
            e.OwnsOne(x => x.Email, em => em.Property(p => p.Value).HasColumnName("Email"));
            e.OwnsOne(x => x.MobilePhone, ph => ph.Property(p => p.Value).HasColumnName("MobilePhone"));
            e.Property(x => x.EmploymentStatus).HasConversion<string>();
            e.Property(x => x.CitizenshipStatus).HasConversion<string>();
        });

        modelBuilder.Entity<JointOwner>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.LegalName);
            e.OwnsOne(x => x.LegalAddress);
            e.OwnsOne(x => x.Email, em => em.Property(p => p.Value).HasColumnName("Email"));
            e.OwnsOne(x => x.MobilePhone, ph => ph.Property(p => p.Value).HasColumnName("MobilePhone"));
            e.Property(x => x.CitizenshipStatus).HasConversion<string>();
        });

        modelBuilder.Entity<Beneficiary>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.LegalName);
            e.Property(x => x.Class).HasConversion<string>();
        });

        modelBuilder.Entity<TrustedContact>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.Phone, ph => ph.Property(p => p.Value).HasColumnName("Phone"));
            e.OwnsOne(x => x.Email, em => em.Property(p => p.Value).HasColumnName("ContactEmail"));
        });

        modelBuilder.Entity<EntityInfo>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.ResponsibleParties).WithOne().HasForeignKey(r => r.EntityInfoId);
        });

        modelBuilder.Entity<ResponsibleParty>(e =>
        {
            e.HasKey(x => x.Id);
            e.OwnsOne(x => x.LegalName);
            e.Property(x => x.Role).HasConversion<string>();
        });

        modelBuilder.Entity<FundingInfo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Method).HasConversion<string>();
            e.OwnsOne(x => x.InitialAmount);
            e.Property(x => x.TransferScope).HasConversion<string>();
        });

        modelBuilder.Entity<ConversationTurn>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Role).HasConversion<string>();
        });

        modelBuilder.Entity<EmbeddingDocument>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Embedding).HasColumnType("vector(384)");
        });
    }
}
