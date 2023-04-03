using CompanyEmployees.IDP.CustomTokenProviders;
using CompanyEmployees.IDP.Entities;
using EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

namespace CompanyEmployees.IDP;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();
        builder.Services.AddAutoMapper(typeof(Program));

        var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
        builder.Services.AddSingleton(emailConfig);
        builder.Services.AddScoped<IEmailSender, EmailSender>();


        var migrationAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        builder.Services.AddDbContext<UserContext>(options => 
            options.UseSqlServer(builder.Configuration.GetConnectionString("identitySqlConnection")));

        builder.Services.AddIdentity<User, IdentityRole>(opt =>
        {
            opt.Password.RequireDigit = false;
            opt.Password.RequiredLength = 7;
            opt.User.RequireUniqueEmail = true;
            opt.SignIn.RequireConfirmedEmail = true;
            opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
            opt.Lockout.AllowedForNewUsers = true;
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
            opt.Lockout.MaxFailedAccessAttempts = 3;
        })
        .AddEntityFrameworkStores<UserContext>()
        .AddDefaultTokenProviders()
        .AddTokenProvider<EmailConfirmationTokenProvider<User>>("emailconfirmation");

        builder.Services.AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;
            })
            //.AddInMemoryIdentityResources(Config.IdentityResources)
            //.AddInMemoryApiScopes(Config.ApiScopes)
            //.AddInMemoryApiResources(Config.Apis)
            //.AddInMemoryClients(Config.Clients)
            //.AddTestUsers(TestUsers.Users)
            .AddConfigurationStore(opt =>
            {
                opt.ConfigureDbContext = c =>
                    c.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection"),
                    sql => sql.MigrationsAssembly(migrationAssembly));
            })
            .AddOperationalStore(opt =>
            {
                opt.ConfigureDbContext = o =>
                    o.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection"),
                    sql => sql.MigrationsAssembly(migrationAssembly));
            })
            .AddAspNetIdentity<User>();

        builder.Services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromHours(2));

        builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromDays(3));

        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = "1056091370610-lbr2emlcigohugh8smn508fqmagu7dtk.apps.googleusercontent.com";
                options.ClientSecret = "GOCSPY-1LcG-q4wrSpybwEpkQ_hcNcFiFCq";
            });

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();
            
        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
