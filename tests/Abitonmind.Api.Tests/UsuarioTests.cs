using System.Net;
using System.Net.Http.Json;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using Microsoft.AspNetCore.Http;

namespace Abitonmind.Api.Tests;

public class UsuarioTests(AbitonmindFactory api) : IClassFixture<AbitonmindFactory>
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    /// <summary>
    /// Una identidad de 255 caracteres justos se guarda, y al pedirla sale igual.
    /// </summary>
    [Fact]
    public async Task Identidad_De255_Da204_YSaleIgual()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        string texto = new('a', 255);

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/identidad", new IdentidadDto(texto), _ct);
        IdentidadDto? vista = await cliente.GetFromJsonAsync<IdentidadDto>("/api/identidad", _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Equal(texto, vista!.Identidad);
    }

    /// <summary>
    /// Una identidad de 256 caracteres no se guarda: 400, y la respuesta dice que falla la identidad.
    /// </summary>
    [Fact]
    public async Task Identidad_De256_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/identidad", new IdentidadDto(new string('a', 256)), _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        HttpValidationProblemDetails? problema = await respuesta.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(_ct);
        Assert.Contains("Identidad", problema!.Errors.Keys);
    }

    /// <summary>
    /// Los espacios del principio y del final no se guardan, y el máximo se cuenta sin ellos: una identidad
    /// de 255 con espacios alrededor da 204.
    /// </summary>
    [Fact]
    public async Task Identidad_De255ConEspaciosEnLosExtremos_Da204_YSeGuardaSinEllos()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        string texto = new('a', 255);

        // Con "with", la identidad llega a la API tal cual, con sus espacios.
        IdentidadDto datos = new IdentidadDto(null) with { Identidad = $"  {texto}  " };
        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/identidad", datos, _ct);
        IdentidadDto? vista = await cliente.GetFromJsonAsync<IdentidadDto>("/api/identidad", _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Equal(texto, vista!.Identidad);
    }

    /// <summary>
    /// Una identidad solo con espacios da 204, y se guarda como vacía (null).
    /// </summary>
    [Fact]
    public async Task Identidad_SoloEspacios_Da204_YQuedaNull()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/identidad", new IdentidadDto("   "), _ct);
        IdentidadDto? vista = await cliente.GetFromJsonAsync<IdentidadDto>("/api/identidad", _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Null(vista!.Identidad);
    }

    /// <summary>
    /// La identidad de un usuario no la ve otro: la del otro sigue en null.
    /// </summary>
    [Fact]
    public async Task Identidad_DeUnUsuario_NoLaVeOtro()
    {
        HttpClient ana = await api.ClienteNuevoAsync();
        HttpClient luis = await api.ClienteNuevoAsync();
        (await ana.PutAsJsonAsync("/api/identidad", new IdentidadDto("Soy una persona que lee cada día"), _ct)).EnsureSuccessStatusCode();

        IdentidadDto? deLuis = await luis.GetFromJsonAsync<IdentidadDto>("/api/identidad", _ct);

        Assert.Null(deLuis!.Identidad);
    }

    /// <summary>
    /// Editar las preferencias da 204, y al pedirlas salen iguales.
    /// </summary>
    [Fact]
    public async Task Preferencias_Editar_Da204_YSalenIguales()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        PreferenciaDto preferencias = new(true, ReglaRacha.Todos, "Europe/Madrid");

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/preferencias", preferencias, _ct);
        PreferenciaDto? vistas = await cliente.GetFromJsonAsync<PreferenciaDto>("/api/preferencias", _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Equal(preferencias, vistas);
    }

    /// <summary>
    /// Los espacios del principio y del final de la zona no se guardan.
    /// </summary>
    [Fact]
    public async Task Preferencias_ZonaConEspacios_Da204_YSeGuardaSinEllos()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        PreferenciaDto preferencias = new(true, ReglaRacha.Todos, "Europe/Madrid");

        // Con "with", la zona llega a la API tal cual, con sus espacios.
        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/preferencias", preferencias with { ZonaHoraria = "  Europe/Madrid  " }, _ct);
        PreferenciaDto? vistas = await cliente.GetFromJsonAsync<PreferenciaDto>("/api/preferencias", _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Equal(preferencias, vistas);
    }

    /// <summary>
    /// Preferencias que la API no tiene que aceptar, cada una con el campo que tiene que fallar.
    /// </summary>
    public static TheoryData<PreferenciaDto, string> PreferenciasNoValidas =>
    [
        (new PreferenciaDto(false, (ReglaRacha)99, "Europe/Madrid"), "TipoRacha"),
        (new PreferenciaDto(false, ReglaRacha.AlMenosUno, "Marte/Olimpo"), "ZonaHoraria"),
        (new PreferenciaDto(false, ReglaRacha.AlMenosUno, new string('a', 41)), "ZonaHoraria"),
    ];

    /// <summary>
    /// Guardar preferencias no válidas da 400, y la respuesta dice qué campo es el que falla.
    /// </summary>
    [Theory]
    [MemberData(nameof(PreferenciasNoValidas))]
    public async Task Preferencias_NoValidas_DiceQueCampoFalla(PreferenciaDto datos, string campo)
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/preferencias", datos, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        HttpValidationProblemDetails? problema = await respuesta.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(_ct);
        Assert.Contains(campo, problema!.Errors.Keys);
    }

    /// <summary>
    /// Sin token, pedir las preferencias da 401.
    /// </summary>
    [Fact]
    public async Task Preferencias_SinSesion_Da401()
    {
        HttpClient cliente = api.CreateClient();

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/preferencias", _ct);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }
}
