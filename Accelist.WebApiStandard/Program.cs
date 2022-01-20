using Accelist.WebApiStandard.Entities;
using Accelist.WebApiStandard.Logics;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

// @Ryan: Add FluentValidation and Mediatr
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<RegisterUserRequestValidator>());
builder.Services.AddMediatR(typeof(RegisterUserRequestHandler));

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
