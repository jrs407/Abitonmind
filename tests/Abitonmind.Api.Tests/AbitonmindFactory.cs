using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Abitonmind.Api.Tests;

/// <summary>
/// Clase que arranca la api en memoría con su propía base de datos.
/// </summary>
/// <param name="postgres">El PostgreSQL de los tests, compartido por todas las clases.</param>
public class AbitonmindFactory(ContenedorPostgres postgres) : WebApplicationFactory<Program>
{
    /// <summary>
    /// Email simulado del administrador en la aplicación.
    /// </summary>
    public const string EmailAdmin = "admin@example.com";

    /// <summary>
    /// Una base de datos distinta para cada factory, dentro del PostgreSQL de los tests.
    /// </summary>
    private readonly string _cadena = postgres.CadenaNueva();

    /// <summary>
    /// Configura la api antes de arrancar.
    /// </summary>
    /// <param name="builder"></param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Abitonmind", _cadena);
        builder.UseSetting("Admin:Email", EmailAdmin);
    }
}