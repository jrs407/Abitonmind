using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using Abitonmind.Data;
using Microsoft.EntityFrameworkCore;

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

        // Devuelve la lista de frases de la aplicacion.
        grupo.MapGet("/", async (AbitonmindContext db) =>
        {
            List<Frase> frases = await db.Frases.ToListAsync();

            List<FraseDto> frasesDtos = frases.Select(f => new FraseDto(Id: f.Id, Texto: f.Texto)).ToList();

            return Results.Ok(frasesDtos);
        });
    }
}