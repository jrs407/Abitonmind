using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Abitonmind.Api.Tests;

/// <summary>
/// La API con la hora que diga el test.
/// </summary>
public static class Relojes
{
    /// <summary>
    /// La misma API y la misma base de datos, pero su reloj marca siempre el momento indicado.
    /// </summary>
    /// <param name="api">La API arrancada en memoria.</param>
    /// <param name="momento">El momento que marca el reloj, en UTC.</param>
    /// <returns>Una API nueva, con el reloj parado en ese momento.</returns>
    public static WebApplicationFactory<Program> ConReloj(this WebApplicationFactory<Program> api, DateTimeOffset momento)
    {
        var reloj = new FakeTimeProvider(momento);

        return api.WithWebHostBuilder(b => b.ConfigureTestServices(servicios => servicios.AddSingleton<TimeProvider>(reloj)));
    }
}
