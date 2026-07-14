using Scalar.AspNetCore;
using Ticketing.Command.Application;
using Ticketing.Command.Features.Apis;
using Ticketing.Command.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRegisterMinimalApis();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(scalar =>
    {
        scalar.Title = "Microservice Ticketing.Command con Scalar";
        scalar.DarkMode = true;
        scalar.Theme = ScalarTheme.DeepSpace;
        scalar.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);

    });
}

//app.UseHttpsRedirection();

app.MapMinimalApiEndpoints();
app.Run();