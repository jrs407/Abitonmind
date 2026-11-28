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
            string usuarioId = usuario.IdDelUsuario();

            List<Habito> habitos = await db.Habitos.Where(h => h.UsuarioId == usuarioId && h.FechaArchivado == null).ToListAsync();

            List<HabitoDto> result = habitos.Select(h => HabitoToDto(h)).ToList();

            return Results.Ok(result);
        });

        // Endpoint que dado un id, devuelve un habito.
        grupo.MapGet("/{id:int}", async (int id, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {
            var habito = await BuscarHabitoAsync(id, usuario.IdDelUsuario(), db);

            if (habito is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(HabitoToDto(habito));
        });

        // Endpoint que crea un habito.
        grupo.MapPost("/", async (GuardarHabitoDto datos, ClaimsPrincipal usuario, AbitonmindContext db) =>
        {

            var habito = new Habito { UsuarioId = usuario.IdDelUsuario(), Nombre = datos.Nombre, Descripcion = datos.Descripcion, Preparacion = datos.Preparacion, Hora = datos.Hora, Dias = datos.Dias, Tipo = datos.Tipo, HabitoAntecesorId = datos.HabitoAntecesorId, ColorHabito = datos.ColorHabito, Recordatorio = datos.Recordatorio };

            if (datos.HabitoAntecesorId is not null)
            {
                var antecesor = await BuscarHabitoAsync(datos.HabitoAntecesorId.Value, usuario.IdDelUsuario(), db);

                if (antecesor is null)
                {
                    return Results.BadRequest("El hábito anterior no existe");
                }

                var errorAntecesor = habito.ErrorAlIrDespuesDe(antecesor);

                if (errorAntecesor is not null)
                {
                    return Results.BadRequest(errorAntecesor);
                }
            }

            db.Habitos.Add(habito);
            await db.SaveChangesAsync();


            return Results.Created($"/api/habitos/{habito.Id}", HabitoToDto(habito));
        });

        // Endpoint que edita un habito.
        grupo.MapPut("/{id:int}", async (int id, GuardarHabitoDto datos, ClaimsPrincipal usuario, AbitonmindContext db, HoyDelUsuario hoyDelUsuario) =>
        {
            string usuarioId = usuario.IdDelUsuario();

            var habito = await BuscarHabitoAsync(id, usuarioId, db);

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

            Dictionary<int, int?> habitos = await db.Habitos.Where(h => h.UsuarioId == usuarioId).ToDictionaryAsync(h => h.Id, h => h.HabitoAntecesorId);

            if (datos.HabitoAntecesorId is not null)
            {
                var antecesor = await BuscarHabitoAsync(datos.HabitoAntecesorId.Value, usuarioId, db);

                if (antecesor is null)
                {
                    return Results.BadRequest("El hábito anterior no existe");
                }

                var errorAntecesor = habito.ErrorAlIrDespuesDe(antecesor);

                if (errorAntecesor is not null)
                {
                    return Results.BadRequest(errorAntecesor);
                }

                if (Cadena.ProvocaCiclo(habito.Id, datos.HabitoAntecesorId.Value, habitos))
                {
                    return Results.BadRequest("Esto provocaría un ciclo, un habito sería anterior a si mismo");
                }
            }

            List<int> idsHabitosPosteriores = Cadena.HabitosPosteriores(habito.Id, habitos);

            List<Habito> habitosPosteriores = await db.Habitos.Where(h => idsHabitosPosteriores.Contains(h.Id) && h.FechaArchivado == null).ToListAsync();

            DateOnly hoy = await hoyDelUsuario.CalcularAsync(usuarioId);

            foreach (var habitoPosterior in habitosPosteriores)
            {
                habitoPosterior.Dias &= habito.Dias;

                if (habitoPosterior.Dias == DiasSemana.Ninguno)
                {
                    habitoPosterior.FechaArchivado = hoy;
                }
            }

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Endpoint de eliminacion de habito, usa borrado logico
        grupo.MapDelete("/{id:int}", async (int id, ClaimsPrincipal usuario, AbitonmindContext db, HoyDelUsuario hoyDelUsuario) =>
        {
            string usuarioId = usuario.IdDelUsuario();
            var habito = await BuscarHabitoAsync(id, usuarioId, db);

            if (habito is null)
            {
                return Results.NotFound();
            }

            Dictionary<int, int?> habitos = await db.Habitos.Where(h => h.UsuarioId == usuarioId).ToDictionaryAsync(h => h.Id, h => h.HabitoAntecesorId);

            List<int> idsHabitosPosteriores = Cadena.HabitosPosteriores(habito.Id, habitos);

            List<Habito> habitosPosteriores = await db.Habitos.Where(h => idsHabitosPosteriores.Contains(h.Id) && h.FechaArchivado == null).ToListAsync();

            DateOnly hoy = await hoyDelUsuario.CalcularAsync(usuarioId);

            habito.FechaArchivado = hoy;

            foreach (var habitoPosterior in habitosPosteriores)
            {
                habitoPosterior.FechaArchivado = hoy;
            }

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Endpoint que marca un habito como hecho en una fecha.
        grupo.MapPut("/{id:int}/registros/{fecha:datetime}", async (int id, DateOnly fecha, ClaimsPrincipal usuario, AbitonmindContext db, HoyDelUsuario hoyDelUsuario) =>
        {
            string usuarioId = usuario.IdDelUsuario();

            var habito = await BuscarHabitoAsync(id, usuarioId, db);

            if (habito is null)
            {
                return Results.NotFound();
            }

            DateOnly hoy = await hoyDelUsuario.CalcularAsync(usuarioId);

            if (fecha != hoy)
            {
                return Results.BadRequest("Solo se puede marcar el día de hoy.");
            }

            if (!habito.TocaEl(hoy))
            {
                return Results.BadRequest("Este habito no toca hoy.");
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
        grupo.MapDelete("/{id:int}/registros/{fecha:datetime}", async (int id, DateOnly fecha, ClaimsPrincipal usuario, AbitonmindContext db, HoyDelUsuario hoyDelUsuario) =>
        {
            string usuarioId = usuario.IdDelUsuario();
            var habito = await BuscarHabitoAsync(id, usuarioId, db);

            if (habito is null)
            {
                return Results.NotFound();
            }

            DateOnly hoy = await hoyDelUsuario.CalcularAsync(usuarioId);

            if (fecha != hoy)
            {
                return Results.BadRequest("Solo se puede marcar el día de hoy.");
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