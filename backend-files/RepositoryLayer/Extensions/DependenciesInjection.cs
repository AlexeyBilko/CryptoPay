using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RepositoryLayer.ApplicationDb;
using RepositoryLayer.Repository.Abstraction;
using RepositoryLayer.Repository.Realization;
using RepositoryLayer.UnitOfWork_;

namespace RepositoryLayer.Extensions
{
    public static class DependenciesInjection
    {
        public static void AddAppDbContext(this IServiceCollection services, string connectionStr)
        {
            // Configures the application's DbContext with SQL Server and specific options
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionStr); // Use SQL Server as the database provider
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // Disables change tracking for performance
            });
        }

        public static void AddRepositoryDependencies(this IServiceCollection services)
        {
            // Registering the DbContext
            services.AddScoped<DbContext, ApplicationDbContext>();

            // Registering each repository with its interface
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            services.AddScoped<ISystemWalletRepository, SystemWalletRepository>();
            services.AddScoped<IUserWalletRepository, UserWalletRepository>();
            services.AddScoped<IPaymentPageRepository, PaymentPageRepository>();
            services.AddScoped<IPaymentPageTransactionRepository, PaymentPageTransactionRepository>();
            services.AddScoped<IAmountDetailsRepository, AmountDetailsRepository>();
            services.AddScoped<IWithdrawalRepository, WithdrawalRepository>();
            services.AddScoped<IEarningsRepository, EarningsRepository>();

            // Registering Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>(); // AddTransient can be used
        }
    }
}
