using System.Security.Claims;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using Abitonmind.Data;
using Microsoft.EntityFrameworkCore;

namespace Abitonmind.Api;

/// <summary>
/// Clase que lista los endpoints de pasos minimos.
/// </summary>
public static class PasosMinimosEndpoints
{
    /// <summary>
    /// Transforma un objeto de la clase PasoMinimo en PasoMinimoDto.
    /// </summary>
    /// <param name="pasoMinimo"></param>
    /// <returns></returns>
    private static PasoMinimoDto PasoMinimoToDto(PasoMinimo pasoMinimo)
    {
        return new PasoMinimoDto(pasoMinimo.Id, pasoMinimo.Nombre, pasoMinimo.Descripcion, pasoMinimo.Preparacion, pasoMinimo.Repeticion, pasoMinimo.ColorPasoMinimo);
    }

    /// <summary>
    /// Busca un paso mínimo por su id, pero solo si es del usuario indicado y no está archivado. Si no se cumple alguna de las tres cosas, devuelve null.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="usuarioId"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    private static async Task<PasoMinimo?> BuscarPasoMinimoAsync(int id, string usuarioId, AbitonmindContext db)
    {
        return await db.PasosMinimos.FirstOrDefaultAsync(pm => pm.Id == id && pm.UsuarioId == usuarioId && pm.FechaArchivado == null);
    }

    /// <summary>
    /// Método estatico que sirve para comprobar que todos los datos son correctos.
    /// </summary>
    /// <param name="datos"></param>
    /// <returns></returns>
    private static string? ErrorEnDatos(GuardarPasoMinimoDto datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
        {
            return "El nombre es obligatorio.";
        }

        if (datos.Nombre.Length > 100)
        {
            return "El nombre es demasiado largo.";
        }

        if (datos.Descripcion is not null)
        {
            if (datos.Descripcion.Length > 255)
            {
                return "La descripción es demasiado larga.";
            }
        }

        if (datos.Preparacion is not null)
        {
            if (datos.Preparacion.Length > 100)
            {
                return "La preparación es demasiado larga";
            }
        }

        if (datos.ColorPasoMinimo is not null)
        {
            if (!Enum.IsDefined(datos.ColorPasoMinimo.Value))
            {
                return "El color no existe";
            }
        }

        return null;
    }

    /// <summary>
    /// Creación del mapa de endpoints de pasos minimos.
    /// </summary>
    /// <param name="app"></param>
    public static void MapPasosMinimos(this WebApplication app)
    {
        // Ruta base del endpoint
        var grupo = app.MapGroup("/api/pasosminimos").RequireAuthorization();

        // Endpoint que devuelve todos los pasos minimos del usuario que hace la petición (sin los archivados).
        grupo.MapGet("/", async (ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = usuario.IdDelUsuario();

            List<PasoMinimo> pasosMinimos = await db.PasosMinimos.Where(pm => pm.UsuarioId == usuarioId && pm.FechaArchivado == null).ToListAsync();

            List<PasoMinimoDto> result = pasosMinimos.Select(pm => PasoMinimoToDto(pm)).ToList();

            return Results.Ok(result);
        });

        // Endpoint que dado un id, devuelve un Paso minimo.
        grupo.MapGet("/{id:int}", async (int id, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            var pasoMinimo = await BuscarPasoMinimoAsync(id, usuario.IdDelUsuario(), db);

            if (pasoMinimo is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(PasoMinimoToDto(pasoMinimo));
        });

        // Endpoint que crea un paso minimo.
        grupo.MapPost("/", async (GuardarPasoMinimoDto datos, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string? error = ErrorEnDatos(datos);

            if (error is not null)
            {
                return Results.BadRequest(error);
            }

            var pasoMinimo = new PasoMinimo { UsuarioId = usuario.IdDelUsuario(), Nombre = datos.Nombre, Descripcion = datos.Descripcion, Preparacion = datos.Preparacion, ColorPasoMinimo = datos.ColorPasoMinimo };

            db.PasosMinimos.Add(pasoMinimo);
            await db.SaveChangesAsync();

            return Results.Created($"/api/pasosminimos/{pasoMinimo.Id}", PasoMinimoToDto(pasoMinimo));
        });

        // Endpoint que edita un paso minimo
        grupo.MapPut("/{id:int}", async (int id, GuardarPasoMinimoDto datos, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string? error = ErrorEnDatos(datos);

            if (error is not null)
            {
                return Results.BadRequest(error);
            }

            string usuarioId = usuario.IdDelUsuario();

            var pasoMinimo = await BuscarPasoMinimoAsync(id, usuarioId, db);

            if (pasoMinimo is null)
            {
                return Results.NotFound();
            }

            pasoMinimo.Nombre = datos.Nombre;
            pasoMinimo.Descripcion = datos.Descripcion;
            pasoMinimo.Preparacion = datos.Preparacion;
            pasoMinimo.ColorPasoMinimo = datos.ColorPasoMinimo;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Endpoint de eliminacion de un paso minimo, usa borrado logico
        grupo.MapDelete("/{id:int}", async (int id, ClaimsPrincipal usuario, AbitonmindContext db, HoyDelUsuario hoyDelUsuario) =>
        {
            string usuarioId = usuario.IdDelUsuario();
            var pasoMinimo = await BuscarPasoMinimoAsync(id, usuarioId, db);

            if (pasoMinimo is null)
            {
                return Results.NotFound();
            }

            DateOnly hoy = await hoyDelUsuario.CalcularAsync(usuarioId);

            pasoMinimo.FechaArchivado = hoy;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Suma uno a las repeticiones de un paso minimo.
        grupo.MapPost("/{id:int}/sumar", async (int id, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = usuario.IdDelUsuario();
            var pasoMinimo = await BuscarPasoMinimoAsync(id, usuarioId, db);

            if (pasoMinimo is null)
            {
                return Results.NotFound();
            }

            pasoMinimo.Repeticion++;

            await db.SaveChangesAsync();

            return Results.Ok(PasoMinimoToDto(pasoMinimo));
        });

        // Resta uno a las repeticiones de un paso minimo.
        grupo.MapPost("/{id:int}/restar", async (int id, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = usuario.IdDelUsuario();
            var pasoMinimo = await BuscarPasoMinimoAsync(id, usuarioId, db);

            if (pasoMinimo is null)
            {
                return Results.NotFound();
            }

            if (pasoMinimo.Repeticion >= 1)
            {
                pasoMinimo.Repeticion--;
            }

            await db.SaveChangesAsync();

            return Results.Ok(PasoMinimoToDto(pasoMinimo));
        });
    }
}