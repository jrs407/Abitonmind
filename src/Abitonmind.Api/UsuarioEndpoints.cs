using System.Security.Claims;
using Abitonmind.Core.Contratos;
using Abitonmind.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Abitonmind.Api;

/// <summary>
/// Clase que lista los endpoints relacionados con los usuarios.
/// </summary>
public static class UsuarioEndpoints
{
    /// <summary>
    /// Creación del mapa de endpoints de usuarios.
    /// </summary>
    /// <param name="app"></param>
    public static void MapUsuarios(this WebApplication app)
    {
        // Ruta base del endpoint.
        var grupo = app.MapGroup("/api").RequireAuthorization();

        // Sirve para obtener el rol del usuario.
        grupo.MapGet("/yo", Yo);

        // Devuelve la identidad de un usuario.
        grupo.MapGet("/identidad", async (ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = usuario.IdDelUsuario();

            var cuenta = await db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (cuenta is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new IdentidadDto(cuenta.Identidad));
        });

        // Crea una identidad para un usuario.
        grupo.MapPut("/identidad", async (IdentidadDto datos, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = usuario.IdDelUsuario();

            var cuenta = await db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (cuenta is null)
            {
                return Results.NotFound();
            }

            cuenta.Identidad = datos.Identidad;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Obtiene las preferencias de un usuario.
        grupo.MapGet("/preferencias", async (ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = usuario.IdDelUsuario();

            var cuenta = await db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (cuenta is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new PreferenciaDto(HabitoPlegado: cuenta.HabitoPlegado, TipoRacha: cuenta.TipoRacha, ZonaHoraria: cuenta.ZonaHoraria));
        });

        // Endpoint que edita las preferencias
        grupo.MapPut("/preferencias", async (PreferenciaDto datos, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = usuario.IdDelUsuario();

            var cuenta = await db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId);

            if(cuenta is null)
            {
                return Results.NotFound();
            }

            cuenta.HabitoPlegado = datos.HabitoPlegado;
            cuenta.TipoRacha = datos.TipoRacha;
            cuenta.ZonaHoraria = datos.ZonaHoraria;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    /// <summary>
    /// Método estático que dice si un usuario es admin.
    /// </summary>
    /// <param name="usuario"></param>
    /// <returns></returns>
    private static Ok<YoDto> Yo(ClaimsPrincipal usuario)
    {
        return TypedResults.Ok(new YoDto(usuario.IsInRole(Roles.Admin)));
    }
}