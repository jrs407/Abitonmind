using System.Security.Claims;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using Abitonmind.Data;
using Microsoft.EntityFrameworkCore;

namespace Abitonmind.Api;

/// <summary>
/// Clase que lista los endpoints relacionados con los usuarios.
/// </summary>
public static class UsuarioEndpoints
{
    /// <summary>
    /// Obtiene el id del usuario.
    /// </summary>
    /// <param name="usuario"></param>
    /// <returns></returns>
    private static string IdDelUsuario(ClaimsPrincipal usuario)
    {
        return usuario.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }

    /// <summary>
    /// Método estatico que sirve para comprobar que todos los datos son correctos.
    /// </summary>
    /// <param name="datos"></param>
    /// <returns></returns>
    private static string? ErrorEnDatos(PreferenciaDto datos)
    {
        if (!Enum.IsDefined(datos.TipoRacha))
        {
            return "Ese tipo de racha no existe";
        }

        if (datos.ZonaHoraria is not null)
        {
            if (datos.ZonaHoraria.Length > 40 || !TimeZoneInfo.TryFindSystemTimeZoneById(datos.ZonaHoraria, out _))
            {
                return "Esa zona horaria no existe";
            }
        }

        return null;
    }

    /// <summary>
    /// Creación del mapa de endpoints de usuarios.
    /// </summary>
    /// <param name="app"></param>
    public static void MapUsuarios(this WebApplication app)
    {
        // Ruta base del endpoint.
        var grupo = app.MapGroup("/api").RequireAuthorization();

        // Devuelve la identidad de un usuario.
        grupo.MapGet("/identidad", async (ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = IdDelUsuario(usuario);

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
            string usuarioId = IdDelUsuario(usuario);

            string? identidad = null;

            if (!string.IsNullOrWhiteSpace(datos.Identidad))
            {
                identidad = datos.Identidad.Trim();
            }

            if (identidad is not null)
            {
                if (identidad.Length > 255)
                {
                    return Results.BadRequest("La identidad es demasiado larga");
                }
            }

            var cuenta = await db.Users.FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (cuenta is null)
            {
                return Results.NotFound();
            }

            cuenta.Identidad = identidad;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Obtiene las preferencias de un usuario.
        grupo.MapGet("/preferencias", async (ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = IdDelUsuario(usuario);

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
            string? error = ErrorEnDatos(datos);

            if (error is not null)
            {
                return Results.BadRequest(error);
            }

            string usuarioId = IdDelUsuario(usuario);

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
}