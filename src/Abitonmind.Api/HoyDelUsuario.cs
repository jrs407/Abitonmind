using Abitonmind.Data;
using Microsoft.EntityFrameworkCore;

namespace Abitonmind.Api;

/// <summary>
/// Clase que calcula el día de hoy de un usuario con su zona horaría
/// </summary>
/// <param name="db"></param>
/// <param name="reloj"></param>
public class HoyDelUsuario(AbitonmindContext db, TimeProvider reloj)
{
    /// <summary>
    /// El día de hoy en la zona del usuario, si no tiene zona, la del servidor.
    /// </summary>
    /// <param name="usuarioId"></param>
    /// <returns></returns>
    public async Task<DateOnly> CalcularAsync(string usuarioId)
    {
        string? nombreZona = await db.Users
           .Where(u => u.Id == usuarioId)
           .Select(u => u.ZonaHoraria)
           .FirstOrDefaultAsync();

        TimeZoneInfo zona = reloj.LocalTimeZone;

        if (nombreZona is not null)
        {
            zona = TimeZoneInfo.FindSystemTimeZoneById(nombreZona);
        }

        DateTimeOffset ahoraAlli = TimeZoneInfo.ConvertTime(reloj.GetUtcNow(), zona);

        return DateOnly.FromDateTime(ahoraAlli.DateTime);
    }
}