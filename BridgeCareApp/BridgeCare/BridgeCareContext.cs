using System.Configuration;

namespace BridgeCare
{
    using BridgeCare.EntityClasses;
    using BridgeCare.EntityClasses.CriteriaDrivenBudgets;
    using System.Data.Entity;

    public partial class BridgeCareContext : DbContext
    {
        public BridgeCareContext()
            : base("name=BridgeCareContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<NetworkEntity> NETWORKS { get; set; }

        public virtual DbSet<SimulationEntity> Simulations { get; set; }
        public virtual DbSet<YearlyInvestmentEntity> YearlyInvestments { get; set; }
        public virtual DbSet<InvestmentsEntity> Investments { get; set; }
        public virtual DbSet<PerformanceEntity> Performances { get; set; }
        public virtual DbSet<PriorityEntity> Priorities { get; set; }
        public virtual DbSet<PriorityFundEntity> PriorityFunds { get; set; }
        public virtual DbSet<DeficientsEntity> Deficients { get; set; }
        public virtual DbSet<TargetsEntity> Targets { get; set; }
        public virtual DbSet<AttributesEntity> Attributes { get; set; }
        public virtual DbSet<PennDotBridgeData> PennDotBridgeData { get; set; }
        public virtual DbSet<PennDotReportAData> PennDotReportAData { get; set; }
        public virtual DbSet<SdRisk> SdRisks { get; set; }
        public virtual DbSet<TreatmentsEntity> Treatments { get; set; }
        public virtual DbSet<CostsEntity> Costs { get; set; }
        public virtual DbSet<FeasibilityEntity> Feasibilities { get; set; }
        public virtual DbSet<ConsequencesEntity> Consequences { get; set; }
        public virtual DbSet<CommittedEntity> CommittedProjects { get; set; }
        public virtual DbSet<CommitConsequencesEntity> CommitConsequences { get; set; }
        public virtual DbSet<RemainingLifeLimitsEntity> RemainingLifeLimits { get; set; }
        public virtual DbSet<CriteriaDrivenBudgetsEntity> CriteriaDrivenBudgets { get; set; }
        public virtual DbSet<SplitTreatmentEntity> SplitTreatments { get; set; }
        public virtual DbSet<SplitTreatmentLimitEntity> SplitTreatmentLimits { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NetworkEntity>()
                .Property(e => e.NETWORK_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<NetworkEntity>()
                .HasMany(e => e.SIMULATIONS)
                .WithOptional(e => e.NETWORK)
                .WillCascadeOnDelete();

            modelBuilder.Entity<SimulationEntity>()
                .Property(e => e.SIMULATION)
                .IsUnicode(false);

            modelBuilder.Entity<YearlyInvestmentEntity>()
                .Property(e => e.BUDGETNAME)
                .IsUnicode(false);

            modelBuilder.Entity<InvestmentsEntity>()
                .Property(e => e.BUDGETORDER)
                .IsUnicode(false);

            modelBuilder.Entity<PriorityEntity>()
              .HasMany(e => e.PRIORITYFUNDS);

            modelBuilder.Entity<SplitTreatmentEntity>()
                .HasMany(e => e.SPLIT_TREATMENT_LIMITS);
        }
    }
}