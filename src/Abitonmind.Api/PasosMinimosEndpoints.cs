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
    /// Obtiene el id del usuario.
    /// </summary>
    /// <param name="usuario"></param>
    /// <returns></returns>
    private static string IdDelUsuario(ClaimsPrincipal usuario)
    {
        return usuario.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }

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

        if(datos.Nombre.Length > 100)
        {
            return "El nombre es demasiado largo.";
        }

        if(datos.Descripcion is not null)
        {
            if(datos.Descripcion.Length > 255)
            {
                return "La descripción es demasiado larga.";
            }
        }

        if(datos.Preparacion is not null)
        {
            if(datos.Preparacion.Length > 100)
            {
                return "La preparación es demasiado larga";
            }
        }

        if(datos.ColorPasoMinimo is not null)
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
    }
}