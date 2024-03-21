using CoreAdvanceConcepts.DataContext;
using CoreAdvanceConcepts.Interface;
using CoreAdvanceConcepts.Repository;
using CoreAdvanceConcepts.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Ui.Web;
using Serilog.Ui.MsSqlServerProvider;
using CoreAdvanceConcepts.Middleware;
using NuGet.Protocol.Core.Types;
using static Dapper.SqlMapper;
using StackExchange.Redis;
using System.Configuration;
using CoreAdvanceConcepts.Caching;
using Asp.Versioning;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IEmployeeService, EmployeeServices>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Connection String
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EmployeeDbContext>(options => options.UseSqlServer(connectionString));

//Logger
var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        tableName: "SerilogLogs",
        autoCreateSqlTable: true
    ).CreateLogger();

builder.Host.UseSerilog(logger);
builder.Services.AddSerilogUi(options =>
      options.UseSqlServer(connectionString, "SerilogLogs")
);

//Redis Cache
builder.Services.AddSingleton(sp =>
{
    ConfigurationOptions config = new();
    var redisString = builder.Configuration.GetConnectionString("Redis");
    if (redisString != null)
    {
        config = ConfigurationOptions.Parse(redisString);
    }
    else
    {
        Console.WriteLine("Error: Redis connection string not found in configuration.");
    }
    return new RedisCacheService(config);
});

//ApiVersioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Employee Version V1", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "Employee Version V2", Version = "v2" });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});


var app = builder.Build();
app.UseMiddleware<LoggingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(x =>
    {
        x.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Version v1");
        x.SwaggerEndpoint("/swagger/v2/swagger.json", "Employee Version v2");
    });
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();
app.UseSerilogUi();

app.UseAuthorization();

app.MapControllers();

app.Run();
