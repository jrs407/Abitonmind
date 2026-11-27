using System.Net;
using System.Net.Http.Json;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using static Abitonmind.Api.Tests.HabitosDePrueba;

namespace Abitonmind.Api.Tests;

public class RegistrosTests(AbitonmindFactory api) : IClassFixture<AbitonmindFactory>
{
    private const string ZonaDelUsuario = "Europe/Madrid";

    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    /// <summary>
    /// Marcar hoy da 204, y marcar otra vez lo mismo, también.
    /// </summary>
    [Fact]
    public async Task Marcar_Hoy_DosVeces_Da204Y204()
    {
        HttpClient cliente = await ClienteEnSuZonaAsync();
        int id = await cliente.CrearHabitoAsync(HabitoValido("Leer"));
        string ruta = RutaDelRegistro(id, HoyDelUsuario());

        HttpResponseMessage primera = await cliente.PutAsync(ruta, null, _ct);
        HttpResponseMessage segunda = await cliente.PutAsync(ruta, null, _ct);

        Assert.Equal(HttpStatusCode.NoContent, primera.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, segunda.StatusCode);
    }

    /// <summary>
    /// Solo se marca hoy: ayer (-1) y mañana (+1) dan 400.
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public async Task Marcar_OtroDia_Da400(int diasDesdeHoy)
    {
        HttpClient cliente = await ClienteEnSuZonaAsync();
        int id = await cliente.CrearHabitoAsync(HabitoValido("Leer"));

        HttpResponseMessage respuesta = await cliente.PutAsync(RutaDelRegistro(id, HoyDelUsuario().AddDays(diasDesdeHoy)), null, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Un hábito sin días no toca nunca, así que no se puede marcar (P48).
    /// </summary>
    [Fact]
    public async Task Marcar_HoyUnoSinDias_Da400()
    {
        HttpClient cliente = await ClienteEnSuZonaAsync();
        int id = await cliente.CrearHabitoAsync(HabitoValido("Ordenar el trastero") with { Dias = DiasSemana.Ninguno });

        HttpResponseMessage respuesta = await cliente.PutAsync(RutaDelRegistro(id, HoyDelUsuario()), null, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Desmarcar hoy uno marcado da 204. Otra vez, ya sin marcar, también 204 (P45).
    /// </summary>
    [Fact]
    public async Task Desmarcar_Hoy_DosVeces_Da204Y204()
    {
        HttpClient cliente = await ClienteEnSuZonaAsync();
        int id = await cliente.CrearHabitoAsync(HabitoValido("Leer"));
        string ruta = RutaDelRegistro(id, HoyDelUsuario());
        (await cliente.PutAsync(ruta, null, _ct)).EnsureSuccessStatusCode();

        HttpResponseMessage primera = await cliente.DeleteAsync(ruta, _ct);
        HttpResponseMessage segunda = await cliente.DeleteAsync(ruta, _ct);

        Assert.Equal(HttpStatusCode.NoContent, primera.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, segunda.StatusCode);
    }

    /// <summary>
    /// Solo se desmarca hoy: ayer da 400.
    /// </summary>
    [Fact]
    public async Task Desmarcar_Ayer_Da400()
    {
        HttpClient cliente = await ClienteEnSuZonaAsync();
        int id = await cliente.CrearHabitoAsync(HabitoValido("Leer"));

        HttpResponseMessage respuesta = await cliente.DeleteAsync(RutaDelRegistro(id, HoyDelUsuario().AddDays(-1)), _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Un usuario nuevo, con su zona horaria guardada. Así el test sabe qué día es hoy para la API.
    /// </summary>
    private async Task<HttpClient> ClienteEnSuZonaAsync()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/preferencias", new PreferenciaDto(false, ReglaRacha.AlMenosUno, ZonaDelUsuario), _ct);
        respuesta.EnsureSuccessStatusCode();

        return cliente;
    }

    /// <summary>
    /// Hoy en la zona del usuario, calculado igual que lo calcula la API.
    /// </summary>
    private static DateOnly HoyDelUsuario()
    {
        TimeZoneInfo zona = TimeZoneInfo.FindSystemTimeZoneById(ZonaDelUsuario);

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zona).DateTime);
    }

    /// <summary>
    /// La ruta para marcar o desmarcar un hábito en una fecha. El formato "O" escribe la fecha en ISO 8601
    /// (2026-11-27), sea cual sea el idioma del equipo.
    /// </summary>
    private static string RutaDelRegistro(int id, DateOnly fecha)
    {
        return $"/api/habitos/{id}/registros/{fecha:O}";
    }
}
