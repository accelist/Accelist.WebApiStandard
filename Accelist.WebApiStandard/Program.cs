using Accelist.WebApiStandard.Entities;
using Accelist.WebApiStandard.Logics.Requests;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextPool<StandardDb>(dbContextBuilder =>
{
    // @Ryan: Make all queries no-tracking by default (tracking for UPDATE / DELETE ops must be explicit)
    dbContextBuilder
        .UseSqlite("Data Source=standard.db")
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// @Ryan: Add FluentValidation, Mediatr, AutoMapper from the other assembly
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<RegisterUserRequestValidator>());
builder.Services.AddMediatR(typeof(RegisterUserRequestHandler));
builder.Services.AddAutoMapper(typeof(ChangePasswordRequestAutomapperProfile));

var serviceName = "Accelist.WebApiStandard";
var serviceVersion = "1.0.0";

// @Ryan: Configure important OpenTelemetry settings, the console exporter, and automatic instrumentation
builder.Services.AddOpenTelemetryTracing(b =>
{
    b.AddConsoleExporter()
        .AddJaegerExporter(o =>
        {
            o.AgentHost = "jaeger";
            o.AgentPort = 6831; // use port number here
        })
        .AddSource(serviceName)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddNpgsql()
        .AddEntityFrameworkCoreInstrumentation();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // @Ryan: Code First Ensure Database is Created (but not migrated, dev only)
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StandardDb>();
    db.Database.EnsureCreated();
}

// @Ryan: RFC 7807 error handling
app.UseExceptionHandler("/error");

app.UseAuthorization();

app.MapControllers();

app.Run();
