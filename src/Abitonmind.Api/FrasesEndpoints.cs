using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using Abitonmind.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Abitonmind.Api;

/// <summary>
/// Clase que lista los endpoints relacionados con las frases.
/// </summary>
public static class FrasesEndpoints
{

    /// <summary>
    /// Creación del mapa de endpoints de frases.
    /// </summary>
    /// <param name="app"></param>
    public static void MapFrases(this WebApplication app)
    {
        // Ruta base del endpoint.
        var grupo = app.MapGroup("/api/frases").RequireAuthorization();

        // Subgrupo para las operaciones de admin.
        var soloAdmin = grupo.MapGroup("").RequireAuthorization(Politicas.SoloAdmin);

        // Devuelve la lista de frases de la aplicacion.
        grupo.MapGet("/", ListarAsync);

        // Endpoint que crea una frase.
        soloAdmin.MapPost("/", CrearAsync);

        // Endpoint que edita una frase.
        soloAdmin.MapPut("/{id:int}", EditarAsync);

        // Endpoint que elimina una frase.
        soloAdmin.MapDelete("/{id:int}", EliminarAsync);
    }

    /// <summary>
    /// Método estático para devolver las frases de la bd.
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    private static async Task<Ok<List<FraseDto>>> ListarAsync(AbitonmindContext db)
    {
        List<FraseDto> frases = await db.Frases
            .Select(f => new FraseDto(f.Id, f.Texto))
            .ToListAsync();

        return TypedResults.Ok(frases);
    }

    /// <summary>
    /// Método estático que sirve para crear una frase.
    /// </summary>
    /// <param name="datos"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    private static async Task<Created<FraseDto>> CrearAsync(GuardarFraseDto datos, AbitonmindContext db)
    {
        var frase = new Frase { Texto = datos.Texto };

        db.Frases.Add(frase);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/api/frases/{frase.Id}", new FraseDto(frase.Id, frase.Texto));
    }

    /// <summary>
    /// Método estático que sirve para editar una frase
    /// </summary>
    /// <param name="id"></param>
    /// <param name="datos"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    private static async Task<Results<NoContent, NotFound>> EditarAsync(int id, GuardarFraseDto datos, AbitonmindContext db)
    {
        Frase? frase = await db.Frases.FindAsync(id);

        if (frase is null)
        {
            return TypedResults.NotFound();
        }

        frase.Texto = datos.Texto;
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    /// <summary>
    /// Método estático que elimina una frase, salvo si es la última
    /// </summary>
    /// <param name="id"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> EliminarAsync(int id, AbitonmindContext db)
    {
        Frase? frase = await db.Frases.FindAsync(id);

        if (frase is null)
        {
            return TypedResults.NotFound();
        }

        bool hayOtras = await db.Frases.AnyAsync(f => f.Id != id);

        if (!hayOtras)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                type: TiposDeError.UltimaFrase,
                title: "No se puede borrar la última frase.");
        }

        db.Frases.Remove(frase);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}