using Abitonmind.Api;
using Abitonmind.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

string cadena = builder.Configuration.GetConnectionString("Abitonmind")!;

builder.Services.AddDbContext<AbitonmindContext>(opciones => opciones.UseSqlite(cadena));

builder.Services.AddIdentityApiEndpoints<Usuario>().AddEntityFrameworkStores<AbitonmindContext>();

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AbitonmindContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGroup("/api/cuenta").MapIdentityApi<Usuario>();

app.MapHabitos();

app.Run();
