using DomainLayer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.ApplicationDb;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.BlockchainService;
using ServiceLayer.Service.Realization;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;
using ServiceLayer.Services.IdentityServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Extensions
{
    public static class AddProvidersExtensions
    {
        // Configures Identity with options and DbContext
        public static void ConfigureIdentityOptions(this IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }

        // Adds scoped services to the dependency injection container
        public static void AddServicesDependencies(this IServiceCollection services)
        {
            // Abstraction Layer Services

            services.AddScoped<MyMapper, MyMapper>();

            services.AddScoped<IAmountDetailsService, AmountDetailsService>();

            services.AddHttpClient<CurrencyService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IPaymentPageService, PaymentPageService>();
            services.AddScoped<IEarningsService, EarningsService>();
            services.AddScoped<IPaymentPageService, PaymentPageService>();
            services.AddScoped<IPaymentPageTransactionService, PaymentPageTransactionService>();
            services.AddScoped<ISystemWalletService, SystemWalletService>();
            services.AddScoped<IUserWalletService, UserWalletService>();
            services.AddScoped<IWithdrawalService, WithdrawalService>();


            services.AddHttpClient<BitcoinService>();
            services.AddScoped<IBitcoinService, BitcoinService>();

            services.AddHttpClient<EthereumService>();
            services.AddScoped<IEthereumService, EthereumService>();

            services.AddScoped<ICryptographyService, CryptographyService>();

            // Realization Layer Services - IdentityServices
            services.AddScoped<JwtTokenService, JwtTokenService>();
            services.AddScoped<RoleService, RoleService>();
            services.AddScoped<UserService, UserService>();
        }

        // Configures JWT authentication
        //public static void ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        //{
        //    var jwtSettings = configuration.GetSection("JwtSettings");
        //    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

        //    services.AddAuthentication(x =>
        //    {
        //        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //    })
        //    .AddJwtBearer(x =>
        //    {
        //        x.RequireHttpsMetadata = false; // Set to true in production
        //        x.SaveToken = true;
        //        x.TokenValidationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(key),
        //            ValidateIssuer = false,
        //            ValidateAudience = false,
        //            ClockSkew = TimeSpan.Zero
        //        };
        //    });
        //}
    }
}
