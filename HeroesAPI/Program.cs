global using AspNetCoreRateLimit;
global using HeroesAPI.DataContext;
global using HeroesAPI.Interfaces;
global using HeroesAPI.Repository;
global using Microsoft.EntityFrameworkCore;
global using Serilog;
global using Serilog.Sinks.MSSqlServer;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using HealthChecks.UI.Client;
using HeroesAPI.GraphQL;
using HeroesAPI.Middlewares;
using HeroesAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using Twilio.Clients;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

#region Serilog Logging
string fullPath = Environment.CurrentDirectory + @"\logs.txt";
builder.Host.UseSerilog((ctx, lc) => lc.MinimumLevel.Error()
                                       .WriteTo.File(fullPath, rollingInterval: RollingInterval.Day)
                                       .WriteTo.MSSqlServer(builder.Configuration.GetConnectionString("MsSqlConnection"),
                                        new MSSqlServerSinkOptions
                                        {
                                            TableName = "Logs",
                                            SchemaName = "dbo",
                                            AutoCreateSqlTable = true
                                        }));
#endregion Serilog Logging

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = false;
})
.AddNewtonsoftJson(option => option.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
.AddXmlDataContractSerializerFormatters()
.AddOData(option => option.Filter().OrderBy());



//Endpoint HealthChecks
builder.Services.AddHealthChecks().AddSqlServer(builder.Configuration.GetConnectionString("MsSqlConnection"));

//HealthCheck Dashboard
builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(60); //Sets the time interval in which HealthCheck will be triggered
    options.MaximumHistoryEntriesPerEndpoint(10); //Sets the maximum number of records displayed in history
    options.AddHealthCheckEndpoint("Health Checks API", "/health"); //Sets the Health Check endpoint
}).AddInMemoryStorage(); //Here is the memory bank configuration

//Registered DBContext
builder.Services.AddDbContext<MainDbContextInfo>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MsSqlConnection"));
});

//Register SQLite
builder.Services.AddDbContext<SqliteDataContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));
});


builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
    .AddEntityFrameworkStores<MainDbContextInfo>()
    .AddDefaultTokenProviders();

//Identity Options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Default SignIn settings.
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;

});

// Password settings
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;
});


builder.Services.ConfigureApplicationCookie(option =>
{
    option.ExpireTimeSpan = TimeSpan.FromDays(1);
    option.SlidingExpiration = true;
});

// Add memory cache dependencies for api throttling
builder.Services.AddMemoryCache();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

//services cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

#region Authorization Roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("AdministratorRole"));
});
#endregion Authorization Roles

// Load configuration from appsettings.json
builder.Services.AddOptions();

// Store rate limit counters and ip rules
builder.Services.AddResponseCaching();

builder.Services.AddControllers(options => options.CacheProfiles.Add("10SecondsDuration", new Microsoft.AspNetCore.Mvc.CacheProfile { Duration = 10 }));

// Load general configuration from appsettings.json
builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("ClientRateLimiting"));

// Load ip rules from app settings.json
builder.Services.Configure<ClientRateLimitPolicies>(builder.Configuration.GetSection("ClientRateLimitPolicies"));

// Inject counter and rules stores
builder.Services.AddInMemoryRateLimiting();

// Load SmtpSettings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

//Load Data Protector
builder.Services.AddDataProtection();

//Load Automapper 
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Add File Extension Content Type Provider
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

#region Repositories
builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddTransient<IHeroRepository, HeroRepository>();
builder.Services.AddTransient<ISeriLogRepository, SeriLogRepository>();
builder.Services.AddTransient<IUnitOfWorkRepository, UnitOfWorkRepository>();
builder.Services.AddTransient<IAuthRepository, AuthRepository>();
builder.Services.AddTransient<IBarcodeRepository, BarcodeRepository>();
builder.Services.AddTransient<IQRCodeRepository, QRCodeRepository>();
builder.Services.AddTransient<IEmailSenderRepository, EmailSenderRepository>();
builder.Services.AddTransient<IRateLimitConfiguration, RateLimitConfiguration>(); // Configuration (resolvers, counter key builders)
#endregion Repositories

builder.Services.AddScoped<HeroDataSchema>();
builder.Services.AddGraphQL(options =>
{
    options.EnableMetrics = true;
})
    .AddSystemTextJson()
    .AddGraphTypes(typeof(HeroDataSchema), ServiceLifetime.Scoped);

//Authentication Settings
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>  // JWT Settings
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration["JWT:ValidAudience"],
        ValidIssuer = configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
    };
});


#region Twilio
builder.Services.AddHttpClient<ITwilioRestClient>();
#endregion Twilio

builder.Services.AddHttpContextAccessor();


WebApplication? app = builder.Build();

// Api Throttling
app.UseClientRateLimiting();


//Sets Health Check dashboard options
app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = p => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

//Sets the Health Check dashboard configuration
app.UseHealthChecksUI(options => { options.UIPath = "/dashboard"; });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(x => x.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowAnyOrigin());

app.UseResponseCaching();

//Order matters
app.UseAuthentication();

app.UseAuthorization();

app.UseGraphQL<HeroDataSchema>();
app.UseGraphQLPlayground(options: new PlaygroundOptions());

app.MapControllers();

app.Run();
