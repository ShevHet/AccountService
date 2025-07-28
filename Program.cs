using AccountService.Behaviors;
using AccountService.Filters;
using AccountService.Services;
using FluentValidation;
using MediatR;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Account Service API",
        Version = "v1",
        Description = "Микросервис для управления банковскими счетами"
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory, "AccountService.xml");
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.OperationFilter<SwaggerDefaultResponseFilter>();
});

// Контроллеры
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<ValidationExceptionFilter>();
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Transient);


builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// Pipeline Behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Заглушки сервисов
builder.Services.AddSingleton<IAccountRepository, AccountRepositoryStub>();
builder.Services.AddSingleton<IClientService, ClientServiceStub>();
builder.Services.AddSingleton<ICurrencyService, CurrencyServiceStub>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Account Service v1");
    c.DisplayRequestDuration();
});

app.MapControllers();
app.Run();