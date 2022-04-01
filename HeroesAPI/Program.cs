global using AspNetCoreRateLimit;
global using HeroesAPI.DataContext;
global using HeroesAPI.Interfaces;
global using HeroesAPI.Repository;
global using HeroesAPI.Repository.GenericRepository;
global using Microsoft.EntityFrameworkCore;
global using Serilog;
global using Serilog.Sinks.MSSqlServer;
using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using Twilio.Clients;

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



builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<MsSql>()
    .AddDefaultTokenProviders();


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

// Load SmtpSettings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

#region Repositories
builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddTransient<IHeroRepository, HeroRepository>();
builder.Services.AddTransient<ISeriLogRepository, SeriLogRepository>();
builder.Services.AddTransient<IUnitOfWorkRepository, UnitOfWorkRepository>();
builder.Services.AddTransient<IAuthRepository, AuthRepository>();
builder.Services.AddSingleton<IEmailSenderRepository, EmailSenderRepository>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>(); // Configuration (resolvers, counter key builders)
#endregion Repositories

#region JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
#endregion JWT


#region Authorization Roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole",
         policy => policy.RequireRole("Admin"));
});

#endregion Authorization Roles

#region Twilio
builder.Services.AddHttpClient<ITwilioRestClient, TwilioRepository>();
#endregion Twilio

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
app.UseRouting();

app.UseCors(x => x.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowAnyOrigin());

app.UseResponseCaching();

//Order matters
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
