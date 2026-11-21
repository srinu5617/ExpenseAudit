namespace ExpenseAudit.API.Data
{
    using ExpenseAudit.API.Models;
    using Microsoft.EntityFrameworkCore;

    public class ExpenseDbContext : DbContext
    {
        public ExpenseDbContext(
            DbContextOptions<ExpenseDbContext> options)
            : base(options)
        {
        }

        public DbSet<ExpenseReceipt> ExpenseReceipts =>
            Set<ExpenseReceipt>();

        public DbSet<AuditViolation> AuditViolations =>
            Set<AuditViolation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExpenseReceipt>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<AuditViolation>()
                .HasOne(x => x.ExpenseReceipt)
                .WithMany(x => x.Violations)
                .HasForeignKey(x => x.ExpenseReceiptId);
        }
    }
}
