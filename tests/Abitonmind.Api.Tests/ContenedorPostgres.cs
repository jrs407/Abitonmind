using Abitonmind.Api.Tests;
using Npgsql;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(ContenedorPostgres))]

namespace Abitonmind.Api.Tests;

/// <summary>
/// Un PostgreSQL desechable para todos los tests. Se arranca una vez y se borra al acabar.
/// </summary>
public class ContenedorPostgres : IAsyncLifetime
{
    /// <summary>
    /// El contenedor, con la misma imagen que compose.yaml.
    /// </summary>
    private readonly PostgreSqlContainer _contenedor = new PostgreSqlBuilder("postgres:18").Build();

    /// <summary>
    /// Una cadena de conexión a una base de datos nueva, con un nombre que no se repite.
    /// </summary>
    public string CadenaNueva()
    {
        var cadena = new NpgsqlConnectionStringBuilder(_contenedor.GetConnectionString())
        {
            Database = $"prueba_{Guid.NewGuid():N}"
        };

        return cadena.ConnectionString;
    }

    /// <summary>
    /// Arranca el contenedor, antes del primer test.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _contenedor.StartAsync();
    }

    /// <summary>
    /// Borra el contenedor, después del último test.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _contenedor.DisposeAsync();
    }
}
