using AccountService.Behaviors;
using AccountService.Configuration;
using AccountService.Filters;
using AccountService.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using AccountService.HealthChecks;
using System.Net.Http.Headers;



var builder = WebApplication.CreateBuilder(args);

// Конфигурация Health Checks
builder.Services.Configure<MemoryCheckOptions>(
    builder.Configuration.GetSection("HealthChecks:Memory"));

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database_health")
    .AddCheck<MemoryHealthCheck>("memory_health")
    .AddCheck<KeycloakHealthCheck>("keycloak_health");

// Конфигурация HttpClient для Keycloak
var keycloakAuthority = builder.Configuration["Jwt:Authority"];
if (string.IsNullOrWhiteSpace(keycloakAuthority))
    throw new InvalidOperationException("JWT Authority не настроен");

builder.Services.AddHttpClient<KeycloakHealthCheck>(client =>
{
    client.BaseAddress = new Uri(keycloakAuthority);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Конфигурация сервиса
builder.Services.Configure<AccountServiceOptions>(
    builder.Configuration.GetSection(AccountServiceOptions.SectionName));

// Настройка FluentValidation
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

// Контроллеры с фильтрами
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<ValidationExceptionFilter>();
    opt.Filters.Add<GlobalExceptionFilter>();
});

builder.Services.AddHttpClient<KeycloakHealthCheck>(client =>
{
    client.BaseAddress = new Uri(keycloakAuthority);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Account Service",
        Version = "v1",
        Description = "API ��� ���������� ����������� �������"
    });

    try
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    }
    catch { }

    
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authorization",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });

    c.OperationFilter<SwaggerDefaultResponseFilter>();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var jwtAuthority = builder.Configuration["Jwt:Authority"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtAuthority))
    throw new InvalidOperationException("JWT Authority is not configured.");
if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("JWT Audience is not configured.");


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = jwtAuthority; // из конфигурации
        options.Audience = jwtAudience;   // из конфигурации
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtAuthority,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true
        };

        if (builder.Environment.IsDevelopment())
        {
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidateIssuer = false;
            options.TokenValidationParameters.ValidateLifetime = false;
            options.TokenValidationParameters.RequireExpirationTime = false;
        }
    });

// Авторизация
builder.Services.AddAuthorization();

// MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);


// Валидаторы
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Заглушки сервисов
builder.Services.AddSingleton<AccountRepositoryStub>();
builder.Services.AddSingleton<ClientServiceStub>();
builder.Services.AddSingleton<CurrencyServiceStub>();

builder.Services.AddSingleton<IAccountRepository>(sp =>
    sp.GetRequiredService<AccountRepositoryStub>());
builder.Services.AddSingleton<IClientService>(sp =>
    sp.GetRequiredService<ClientServiceStub>());
builder.Services.AddSingleton<ICurrencyService>(sp =>
    sp.GetRequiredService<CurrencyServiceStub>());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Account Service v1");
});


app.MapGet("/health", () => Results.Ok("Healthy"));
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();