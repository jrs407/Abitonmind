using System.Security.Claims;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using Abitonmind.Data;
using Microsoft.EntityFrameworkCore;

namespace Abitonmind.Api;

/// <summary>
/// Clase que lista los endpoints de habitos.
/// </summary>
public static class HabitosEndpoints
{

    /// <summary>
    /// Obtiene el token del usuario.
    /// </summary>
    /// <param name="usuario"></param>
    /// <returns></returns>
    private static string idUsuario(ClaimsPrincipal usuario)
    {
        return usuario.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
    
    /// <summary>
    /// Transforma un objeto de la clase Habito en HabitoDto.
    /// </summary>
    /// <param name="habito"></param>
    /// <returns></returns>
    private static HabitoDto HabitoToDto(Habito habito)
    {
        return new HabitoDto(habito.Id, habito.Nombre, habito.Descripcion, habito.Preparacion, habito.Hora, habito.Dias, habito.Tipo, habito.HabitoAntecesorId, habito.ColorHabito, habito.Recordatorio);
    }

    /// <summary>
    /// Busca un hábito por su id, pero solo si es del usuario indicado y no está archivado. Si no se cumple alguna de las tres cosas, devuelve null.
    /// </summary>
    private static async Task<Habito?> BuscarHabitoAsync(int id, string usuarioId, AbitonmindContext db)
    {
        return await db.Habitos.FirstOrDefaultAsync(
            h => h.Id == id && h.UsuarioId == usuarioId && h.FechaArchivado == null);
    }

    /// <summary>
    /// Creación del mapa de endpoints de habitos.
    /// </summary>
    /// <param name="app"></param>
    public static void MapHabitos(this WebApplication app)
    {
        // Ruta base del endpoint
        var grupo = app.MapGroup("/api/habitos").RequireAuthorization();

        // Endpoint que devuelve todos los habitos del usuario que hace la petición (sin los archivados).
        grupo.MapGet("/", async (ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            string usuarioId = idUsuario(usuario);

            List<Habito> habitos = await db.Habitos.Where(h => h.UsuarioId == usuarioId && h.FechaArchivado == null).ToListAsync();

            List<HabitoDto> result = habitos.Select(h => HabitoToDto(h)).ToList();

            return Results.Ok(result);
        });

        // Endpoint que dado un id, devuelve un habito.
        grupo.MapGet("/{id:int}", async (int id, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            var habito = await BuscarHabitoAsync(id, idUsuario(usuario), db);

            if (habito is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(HabitoToDto(habito));
        });

        // Endpoint que crea un habito.
        grupo.MapPost("/", async (GuardarHabitoDto datos, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            if (string.IsNullOrWhiteSpace(datos.Nombre))
            {
                return Results.BadRequest("El nombre es obligatorio");
            }

            var habito = new Habito { UsuarioId = idUsuario(usuario), Nombre = datos.Nombre, Descripcion = datos.Descripcion, Preparacion = datos.Preparacion, Hora = datos.Hora, Dias = datos.Dias, Tipo = datos.Tipo, HabitoAntecesorId = datos.HabitoAntecesorId, ColorHabito = datos.ColorHabito, Recordatorio = datos.Recordatorio };

            db.Habitos.Add(habito);
            await db.SaveChangesAsync();


            return Results.Created($"/api/habitos/{habito.Id}", HabitoToDto(habito));
        });

        // Endpoint que edita un habito.
        grupo.MapPut("/{id:int}", async (int id, GuardarHabitoDto datos, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            if (string.IsNullOrWhiteSpace(datos.Nombre))
            {
                return Results.BadRequest("El nombre es obligatorio.");
            }

            var habito = await BuscarHabitoAsync(id, idUsuario(usuario), db);

            if (habito is null)
            {
                return Results.NotFound();
            }

            habito.Nombre = datos.Nombre;
            habito.ColorHabito = datos.ColorHabito;
            habito.Descripcion = datos.Descripcion;
            habito.Dias = datos.Dias;
            habito.HabitoAntecesorId = datos.HabitoAntecesorId;
            habito.Hora = datos.Hora;
            habito.Preparacion = datos.Preparacion;
            habito.Recordatorio = datos.Recordatorio;
            habito.Tipo = datos.Tipo;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Endpoint de eliminacion de habito, usa borrado logico
        grupo.MapDelete("/{id:int}", async (int id, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            var habito = await BuscarHabitoAsync(id, idUsuario(usuario), db);

            if (habito is null)
            {
                return Results.NotFound();
            }

            habito.FechaArchivado = DateOnly.FromDateTime(DateTime.Today);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Endpoint que marca un habito como hecho en una fecha.
        grupo.MapPut("/{id:int}/registros/{fecha:datetime}", async (int id, DateOnly fecha, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            var habito = await BuscarHabitoAsync(id, idUsuario(usuario), db);

            if (habito is null)
            {
                return Results.NotFound();
            }

            bool yaMarcado = await db.RegistroHabitos.AnyAsync(rh => rh.HabitoId == id && rh.Fecha == fecha);

            if (!yaMarcado)
            {
                db.RegistroHabitos.Add(new RegistroHabito { HabitoId = id, Fecha = fecha });
                await db.SaveChangesAsync();
            }

            return Results.NoContent();
        });

        // Endpoint que desmarca un habito en una fecha.
        grupo.MapDelete("/{id:int}/registros/{fecha:datetime}", async (int id, DateOnly fecha, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            var habito = await BuscarHabitoAsync(id, idUsuario(usuario), db);

            if (habito is null)
            {
                return Results.NotFound();
            }

            var registroHabito = await db.RegistroHabitos.FirstOrDefaultAsync(rh => rh.HabitoId == id && rh.Fecha == fecha);

            if (registroHabito is not null)
            {
                db.RegistroHabitos.Remove(registroHabito);
                await db.SaveChangesAsync();
            }

            return Results.NoContent();
        });
    }
}