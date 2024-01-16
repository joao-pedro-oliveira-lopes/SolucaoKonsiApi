using KonsiApi.Data;
using KonsiApi.Models;
using KonsiApi.Repositories;
using KonsiApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
builder.Services.Configure<ExternalApiConfig>(builder.Configuration.GetSection("ExternalApiConfig"));
builder.Services.AddHttpClient<BeneficioService>();
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddSingleton<CacheService>();
builder.Services.AddDbContext<DesafioKonsiContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DesafioKonsiDb")));
builder.Services.AddScoped<ICpfRepository, CpfRepository>();
var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQConfig").Get<RabbitMQConfig>();
var beneficiosConsultaQueueName = rabbitMQConfig.Queues["BeneficiosConsulta"];
builder.Services.AddSingleton<ElasticsearchService>();



builder.Services.AddTransient<CpfQueueService>(serviceProvider =>
{
    var cpfRepository = serviceProvider.GetRequiredService<ICpfRepository>();
    var rabbitMQService = serviceProvider.GetRequiredService<RabbitMQService>();
    return new CpfQueueService(cpfRepository, rabbitMQService, beneficiosConsultaQueueName);
});

var jwtConfig = builder.Configuration.GetSection("JwtConfig");
var secret = jwtConfig["Secret"];
var key = Encoding.ASCII.GetBytes(secret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(builder.Configuration.GetConnectionString("DesafioKonsiDb"),
                        tableName: "logs")
    .CreateLogger();


builder.Services.AddControllers();
builder.Host.UseSerilog();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<RabbitMQBackgroundService>();
var app = builder.Build();

// Inicializar e enfileirar CPFs
using (var scope = app.Services.CreateScope())
{
    var cpfQueueService = scope.ServiceProvider.GetRequiredService<CpfQueueService>();
    await cpfQueueService.EnqueueCpfsAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
