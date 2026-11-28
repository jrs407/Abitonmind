using System.Net;
using System.Net.Http.Json;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using static Abitonmind.Api.Tests.HabitosDePrueba;

namespace Abitonmind.Api.Tests;

public class RegistrosTests(AbitonmindFactory api) : IClassFixture<AbitonmindFactory>
{
    private const string Madrid = "Europe/Madrid";

    // UTC+14: el primer sitio del mundo en empezar el día.
    private const string Kiritimati = "Pacific/Kiritimati";

    // Jueves 26 de noviembre de 2026, a las 12:00 en UTC (13:00 en Madrid).
    private static readonly DateTimeOffset JuevesAMediodia = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);

    // Domingo 29 de noviembre de 2026, a las 09:59 en UTC (23:59 del domingo en Kiritimati).
    private static readonly DateTimeOffset UltimoMinutoDelDomingoEnKiritimati = new(2026, 11, 29, 9, 59, 0, TimeSpan.Zero);

    // Domingo 29 de noviembre de 2026, a las 10:00 en UTC (00:00 del lunes en Kiritimati).
    private static readonly DateTimeOffset PrimerMinutoDelLunesEnKiritimati = new(2026, 11, 29, 10, 0, 0, TimeSpan.Zero);

    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    /// <summary>
    /// El jueves, marcar el jueves da 204, y marcar otra vez lo mismo, también.
    /// </summary>
    [Fact]
    public async Task Marcar_Hoy_DosVeces_Da204Y204()
    {
        var (cliente, id) = await UsuarioConHabitoAsync(JuevesAMediodia, Madrid, DiasSemana.Todos);
        string ruta = RutaDelRegistro(id, "2026-11-26");

        HttpResponseMessage primera = await cliente.PutAsync(ruta, null, _ct);
        HttpResponseMessage segunda = await cliente.PutAsync(ruta, null, _ct);

        Assert.Equal(HttpStatusCode.NoContent, primera.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, segunda.StatusCode);
    }

    /// <summary>
    /// Solo se marca hoy: el jueves, marcar el miércoles (ayer) o el viernes (mañana) da 400.
    /// </summary>
    [Theory]
    [InlineData("2026-11-25")]
    [InlineData("2026-11-27")]
    public async Task Marcar_OtroDia_Da400(string fecha)
    {
        var (cliente, id) = await UsuarioConHabitoAsync(JuevesAMediodia, Madrid, DiasSemana.Todos);

        HttpResponseMessage respuesta = await cliente.PutAsync(RutaDelRegistro(id, fecha), null, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Un hábito sin días no toca nunca, así que no se puede marcar (P48).
    /// </summary>
    [Fact]
    public async Task Marcar_HoyUnoSinDias_Da400()
    {
        var (cliente, id) = await UsuarioConHabitoAsync(JuevesAMediodia, Madrid, DiasSemana.Ninguno);

        HttpResponseMessage respuesta = await cliente.PutAsync(RutaDelRegistro(id, "2026-11-26"), null, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// El jueves, un hábito de todos los días menos el jueves no se puede marcar (P53).
    /// </summary>
    [Fact]
    public async Task Marcar_HoyUnoQueNoToca_Da400()
    {
        var (cliente, id) = await UsuarioConHabitoAsync(JuevesAMediodia, Madrid, DiasSemana.Todos & ~DiasSemana.Jueves);

        HttpResponseMessage respuesta = await cliente.PutAsync(RutaDelRegistro(id, "2026-11-26"), null, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// A las 23:59 del domingo en Kiritimati, todavía es domingo allí: un hábito de los domingos se marca.
    /// </summary>
    [Fact]
    public async Task Marcar_UltimoMinutoDelDomingoEnSuZona_Da204()
    {
        var (cliente, id) = await UsuarioConHabitoAsync(UltimoMinutoDelDomingoEnKiritimati, Kiritimati, DiasSemana.Domingo);

        HttpResponseMessage respuesta = await cliente.PutAsync(RutaDelRegistro(id, "2026-11-29"), null, _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
    }

    /// <summary>
    /// Un minuto después ya es lunes en Kiritimati, aunque en UTC siga siendo domingo: el domingo ya no se marca.
    /// </summary>
    [Fact]
    public async Task Marcar_DomingoCuandoEnSuZonaYaEsLunes_Da400()
    {
        var (cliente, id) = await UsuarioConHabitoAsync(PrimerMinutoDelLunesEnKiritimati, Kiritimati, DiasSemana.Domingo);

        HttpResponseMessage respuesta = await cliente.PutAsync(RutaDelRegistro(id, "2026-11-29"), null, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// En Kiritimati ya es lunes, pero un hábito de los domingos no toca el lunes.
    /// </summary>
    [Fact]
    public async Task Marcar_LunesEnSuZonaUnoDeLosDomingos_Da400()
    {
        var (cliente, id) = await UsuarioConHabitoAsync(PrimerMinutoDelLunesEnKiritimati, Kiritimati, DiasSemana.Domingo);

        HttpResponseMessage respuesta = await cliente.PutAsync(RutaDelRegistro(id, "2026-11-30"), null, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Desmarcar hoy uno marcado da 204. Otra vez, ya sin marcar, también 204 (P45).
    /// </summary>
    [Fact]
    public async Task Desmarcar_Hoy_DosVeces_Da204Y204()
    {
        var (cliente, id) = await UsuarioConHabitoAsync(JuevesAMediodia, Madrid, DiasSemana.Todos);
        string ruta = RutaDelRegistro(id, "2026-11-26");
        (await cliente.PutAsync(ruta, null, _ct)).EnsureSuccessStatusCode();

        HttpResponseMessage primera = await cliente.DeleteAsync(ruta, _ct);
        HttpResponseMessage segunda = await cliente.DeleteAsync(ruta, _ct);

        Assert.Equal(HttpStatusCode.NoContent, primera.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, segunda.StatusCode);
    }

    /// <summary>
    /// Solo se desmarca hoy: el jueves, desmarcar el miércoles (ayer) da 400.
    /// </summary>
    [Fact]
    public async Task Desmarcar_Ayer_Da400()
    {
        var (cliente, id) = await UsuarioConHabitoAsync(JuevesAMediodia, Madrid, DiasSemana.Todos);

        HttpResponseMessage respuesta = await cliente.DeleteAsync(RutaDelRegistro(id, "2026-11-25"), _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Un usuario nuevo con su zona horaria y un hábito suyo, en una API con el reloj parado en el momento indicado.
    /// </summary>
    /// <param name="momento">La hora de la API, en UTC.</param>
    /// <param name="zona">La zona horaria del usuario.</param>
    /// <param name="dias">Los días en los que toca el hábito.</param>
    /// <returns>El cliente con la sesión del usuario y el id del hábito.</returns>
    private async Task<(HttpClient Cliente, int Id)> UsuarioConHabitoAsync(DateTimeOffset momento, string zona, DiasSemana dias)
    {
        HttpClient cliente = await api.ConReloj(momento).ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/preferencias", new PreferenciaDto(false, ReglaRacha.AlMenosUno, zona), _ct);
        respuesta.EnsureSuccessStatusCode();

        int id = await cliente.CrearHabitoAsync(HabitoValido("Leer") with { Dias = dias });

        return (cliente, id);
    }

    /// <summary>
    /// La ruta para marcar o desmarcar un hábito en una fecha, escrita como año-mes-día (2026-11-26).
    /// </summary>
    private static string RutaDelRegistro(int id, string fecha)
    {
        return $"/api/habitos/{id}/registros/{fecha}";
    }
}
