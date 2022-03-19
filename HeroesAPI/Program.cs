global using HeroesAPI.Data;
using AspNetCoreRateLimit;
using HeroesAPI.Interfaces;
using HeroesAPI.Repository;
using HeroesAPI.Repository.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;

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
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


// Add memory cache dependencies 
builder.Services.AddMemoryCache();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//services cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// Load configuration from appsettings.json
builder.Services.AddOptions();

// Store rate limit counters and ip rules
builder.Services.AddMemoryCache();

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
#endregion Repositories



WebApplication? app = builder.Build();

app.UseClientRateLimiting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowAnyOrigin());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
