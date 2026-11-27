using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Abitonmind.Api.Tests;

/// <summary>
/// Clase que arranca la api en memoría con su propía base de datos.
/// </summary>
public class AbitonmindFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Email simulado del administrador en la aplicación.
    /// </summary>
    public const string EmailAdmin = "admin@example.com";

    /// <summary>
    /// Crea un archivo distinto para cada factory en la carpeta temporal de windows.
    /// </summary>
    private readonly string _rutaBd = Path.Combine(Path.GetTempPath(), $"abitonmind-tests-{Guid.NewGuid()}.db");

    /// <summary>
    /// Configura la api antes de arrancar.
    /// </summary>
    /// <param name="builder"></param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Abitonmind", $"Data Source={_rutaBd}");
        builder.UseSetting("Admin:Email", EmailAdmin);
    }

    /// <summary>
    /// Cuando termina, apaga la api y elimina todos los datos de prueba.
    /// </summary>
    /// <returns></returns>
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        SqliteConnection.ClearAllPools();
        File.Delete(_rutaBd);
    }
}