using LibrarySystem.Data;
using LibrarySystem.Services;
using LibrarySystem.API.Middleware;
using LibrarySystem.API.Extensions;
using LibrarySystem.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();

builder.Services.AddDataInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."));
builder.Services.AddApplicationServices();


var app = builder.Build();

await app.SeedDatabaseAsync();
app.UseGlobalExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapBookEndpoints();

app.Run();
