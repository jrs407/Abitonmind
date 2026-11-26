using Abitonmind.Api;
using Abitonmind.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

string cadena = builder.Configuration.GetConnectionString("Abitonmind")!;

builder.Services.AddDbContext<AbitonmindContext>(opciones => opciones.UseSqlite(cadena));

builder.Services.AddIdentityApiEndpoints<Usuario>().AddRoles<IdentityRole>().AddEntityFrameworkStores<AbitonmindContext>();

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AbitonmindContext>();
    db.Database.Migrate();

    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if(!await roles.RoleExistsAsync(Roles.Admin))
    {
        await roles.CreateAsync(new IdentityRole(Roles.Admin));
    }

    var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

    string? email = app.Configuration["Admin:Email"];
    Usuario? elegido = null;

    if (!string.IsNullOrWhiteSpace(email))
    {
        elegido = await usuarios.FindByEmailAsync(email);
    }

    var conRol = await usuarios.GetUsersInRoleAsync(Roles.Admin);

    foreach (var usuario in conRol)
    {
        if (elegido is null || usuario.Id != elegido.Id)
        {
            await usuarios.RemoveFromRoleAsync(usuario, Roles.Admin);
        }
    }

    if (elegido is not null && !await usuarios.IsInRoleAsync(elegido, Roles.Admin))
    {
        await usuarios.AddToRoleAsync(elegido, Roles.Admin);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGroup("/api/cuenta").MapIdentityApi<Usuario>();

app.MapHabitos();

app.MapPasosMinimos();

app.MapUsuarios();

app.MapFrases();

app.Run();
