using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RepositoryLayer.ApplicationDb
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            //Database.EnsureCreated(); // For development, separate method in Program.cs
        }

        // DbSet properties for each entity in domain model
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<SystemWallet> SystemWallets { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<PaymentPage> PaymentPages { get; set; }
        public DbSet<PaymentPageTransaction> PaymentPageTransactions { get; set; }
        public DbSet<AmountDetails> AmountDetails { get; set; }
        public DbSet<Withdrawal> Withdrawals { get; set; }
        public DbSet<Earnings> Earnings { get; set; }

        // Is used to configure the model and relationships using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);  // This needs to be called first

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable(name: "Users"); // Rename AspNetUsers to Users
            });
            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Roles"); // Rename AspNetRoles to Roles
            });
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles"); // Rename AspNetUserRoles
            });
            modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims"); // Rename AspNetUserClaims
            });

            // Configure User relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Earnings)
                .WithOne(e => e.User)
                .HasForeignKey<Earnings>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserWallets)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.PaymentPages)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PaymentPage relationships
            modelBuilder.Entity<PaymentPage>()
                .HasMany(p => p.PaymentPageTransactions)
                .WithOne(t => t.PaymentPage)
                .HasForeignKey(t => t.PaymentPageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentPage>()
                .HasOne(p => p.AmountDetails)
                .WithOne()
                .HasForeignKey<PaymentPage>(p => p.AmountDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AmountDetails>()
                .HasOne(a => a.Currency)
                .WithMany(c => c.AmountDetailss)
                .HasForeignKey(a => a.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SystemWallet relationships
            modelBuilder.Entity<SystemWallet>()
                .HasMany(s => s.Withdrawals)
                .WithOne(c => c.SystemWallet)
                .HasForeignKey(s => s.SystemWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SystemWallet>()
                .HasMany(s => s.PaymentPages)
                .WithOne(t => t.SystemWallet)
                .HasForeignKey(t => t.SystemWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Withdrawal relationships
            modelBuilder.Entity<Withdrawal>()
                .HasOne(w => w.UserWallet)
                .WithMany(uw => uw.Withdrawals)
                .HasForeignKey(w => w.UserWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Withdrawal>()
                .HasOne(w => w.SystemWallet)
                .WithMany(uw => uw.Withdrawals)
                .HasForeignKey(w => w.SystemWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Withdrawal>()
                .HasOne(w => w.AmountDetails)
                .WithMany(uw => uw.Withdrawals)
                .HasForeignKey(w => w.AmountDetailsId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure precision for decimal properties in financial entities
            modelBuilder.Entity<PaymentPageTransaction>()
                .Property(p => p.TransactionFee)
                .HasColumnType("decimal(18, 6)");

            modelBuilder.Entity<SystemWallet>()
                .Property(w => w.Balance)
                .HasColumnType("decimal(18, 6)");

            modelBuilder.Entity<AmountDetails>()
                .Property(a => a.AmountUSD)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<AmountDetails>()
                .Property(a => a.AmountCrypto)
                .HasColumnType("decimal(18, 6)");

            modelBuilder.Entity<PaymentPage>()
                .HasIndex(p => p.UserId); // Index for UserId

            modelBuilder.Entity<PaymentPage>()
                .HasIndex(p => p.AmountDetailsId); // Index for AmountDetailsId

            modelBuilder.Entity<PaymentPageTransaction>()
                .HasIndex(p => p.PaymentPageId); // Index for PaymentPageId

            //modelBuilder.Entity<SystemWallet>().HasData(
            //    new SystemWallet { Id = 1, Address = "btc-system-wallet-address", CurrencyCode = "BTC" },
            //    new SystemWallet { Id = 2, Address = "eth-system-wallet-address", CurrencyCode = "ETH" }
            //);


            // Seed data for currencies
            modelBuilder.Entity<Currency>().HasData(
                new Currency { Id = 1, CurrencyCode = "BTC", CurrencyName = "BTC", Network = "BTC" },
                new Currency { Id = 2, CurrencyCode = "ETH", CurrencyName = "ETH", Network = "ETH" }
            );

            //// Seed system wallets
            modelBuilder.Entity<SystemWallet>().HasData(
                new SystemWallet { Id = 1, WalletNumber = "n1aabHTiBxSZQGhWhe8kKh62TX4RHtatC8", EncryptedWalletCodePhrase = "cSwDyuyCPx3QSThwpWkBeMxyqatsVCcfuUL8Mq2WYuncNnCAPJB2", Title = "BTC" },
                new SystemWallet { Id = 2, WalletNumber = "0xEcfA5c3215f5E9b41f379E1966501F3ba87Afdd3", EncryptedWalletCodePhrase = "0x351bb0bf2a6104eae12c50f9e44e38b439856f99698886d21abe09b6309bceb6", Title = "ETH" }
            );
        }
    }
}
