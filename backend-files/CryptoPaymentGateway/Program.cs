using CryptoPaymentGateway.Middleware;
using DomainLayer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RepositoryLayer.ApplicationDb;
using RepositoryLayer.Extensions;
using ServiceLayer.Extensions;
using ServiceLayer.Service.BlockchainService;
using ServiceLayer.Service.Realization;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddAppDbContext(connectionString);
builder.Services.AddRepositoryDependencies();
builder.Services.AddServicesDependencies();
builder.Services.ConfigureIdentityOptions();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<EmailService>();
//builder.Services.ConfigureJwtAuthentication(builder.Configuration);

builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });

builder.Services.AddDistributedMemoryCache();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Define security scheme for JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowedOrigins", policy =>
//    {
//        policy.WithOrigins("http://localhost:3000") // To replace with the React app's actual domain 
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});

var app = builder.Build();

InitializeDatabase(app);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
    app.UseExceptionHandler(a => a.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = feature.Error;
        var result = JsonSerializer.Serialize(new { error = exception.Message });
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }));
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseStaticFiles(); for now don't need them

app.UseCors();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<TokenVersionValidationMiddleware>();
app.UseAuthorization();


app.MapControllers();

app.Run();

void InitializeDatabase(IApplicationBuilder app)
{
    using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
    {
        var services = serviceScope.ServiceProvider;
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        //var bitcoinService = services.GetRequiredService<IBitcoinService>();
        //var ethereumService = services.GetRequiredService<IEthereumService>();
        //var cryptographyService = services.GetRequiredService<ICryptographyService>();

        //dbContext.Database.Migrate();

        dbContext.Database.EnsureCreated();
        
        //migration call can be here

        //if (!dbContext.SystemWallets.Any())
        //{
        //    var btcWallet = bitcoinService.CreateBitcoinWalletAsync(true).Result;
        //    var btcSystemWallet = new SystemWallet
        //    {
        //        WalletNumber = btcWallet.address,
        //        EncryptedWalletCodePhrase = btcWallet.encryptedPrivateKey,
        //        Title = "BTC"
        //    };
        //    dbContext.SystemWallets.Add(btcSystemWallet);

        //    // Generate and save ETH system wallet
        //    var ethWallet = ethereumService.CreateEthereumWalletAsync().Result;
        //    var ethSystemWallet = new SystemWallet
        //    {
        //        WalletNumber = ethWallet.address,
        //        EncryptedWalletCodePhrase = ethWallet.encryptedPrivateKey,
        //        Title = "ETH"
        //    };
        //    dbContext.SystemWallets.Add(ethSystemWallet);

        //    dbContext.SaveChanges();
        //}

    }
}
