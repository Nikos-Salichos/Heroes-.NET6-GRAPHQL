global using AspNetCoreRateLimit;
global using HeroesAPI.DataContext;
global using HeroesAPI.Interfaces;
global using HeroesAPI.Repository;
global using HeroesAPI.Repository.GenericRepository;
global using Microsoft.EntityFrameworkCore;
global using Serilog;
global using Serilog.Sinks.MSSqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

#region Serilog Logging
string fullPath = Environment.CurrentDirectory + @"\logs.txt";
builder.Host.UseSerilog((ctx, lc) => lc.MinimumLevel.Error()
                                       .WriteTo.File(fullPath, rollingInterval: RollingInterval.Day)
                                       .WriteTo.MSSqlServer("server=localhost\\sqlexpress;database=superherodb;trusted_connection=true",
                                        new MSSqlServerSinkOptions
                                        {
                                            TableName = "Logs",
                                            SchemaName = "dbo",
                                            AutoCreateSqlTable = true
                                        }));
#endregion Serilog Logging

builder.Services.AddControllers();



builder.Services.AddDbContext<MsSql>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MsSqlConnection"));
});

builder.Services.AddDbContext<MsSql>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

//services cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Load configuration from appsettings.json
builder.Services.AddOptions();

// Store rate limit counters and ip rules
builder.Services.AddResponseCaching();

builder.Services.AddControllers(options => options.CacheProfiles.Add("60SecondsDuration", new Microsoft.AspNetCore.Mvc.CacheProfile { Duration = 30 }));

// Load general configuration from appsettings.json
builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("ClientRateLimiting"));

// Load ip rules from app settings.json
builder.Services.Configure<ClientRateLimitPolicies>(builder.Configuration.GetSection("ClientRateLimitPolicies"));

// Inject counter and rules stores
builder.Services.AddInMemoryRateLimiting();

// Configuration (resolvers, counter key builders)
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

#region Repositories
builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddTransient<IHeroRepository, HeroRepository>();
builder.Services.AddTransient<ISeriLogRepository, SeriLogRepository>();
builder.Services.AddTransient<IUnitOfWorkRepository, UnitOfWorkRepository>();
builder.Services.AddTransient<IAuthRepository, AuthRepository>();
#endregion Repositories

builder.Services.AddHttpContextAccessor();


WebApplication? app = builder.Build();


app.UseClientRateLimiting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(x => x.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowAnyOrigin());

app.UseResponseCaching();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
