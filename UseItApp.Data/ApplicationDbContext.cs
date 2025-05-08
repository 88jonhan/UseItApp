using Microsoft.EntityFrameworkCore;
using UseItApp.Domain.Models;

namespace UseItApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<Loan> Loans { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User-Item relationship (one-to-many)
        modelBuilder.Entity<Item>()
            .HasOne(i => i.Owner)
            .WithMany(u => u.OwnedItems)
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // User-Loan relationship (one-to-many)
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Borrower)
            .WithMany(u => u.BorrowedItems)
            .HasForeignKey(l => l.BorrowerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Item-Loan relationship (one-to-many)
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Item)
            .WithMany(i => i.LoanHistory)
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}