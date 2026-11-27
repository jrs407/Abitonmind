using System.Net;
using System.Net.Http.Json;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;

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
    /// Una identidad de 256 caracteres no se guarda: 400.
    /// </summary>
    [Fact]
    public async Task Identidad_De256_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/identidad", new IdentidadDto(new string('a', 256)), _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
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
    /// Preferencias que la API no tiene que aceptar: una regla que no existe y una zona que no existe.
    /// </summary>
    public static TheoryData<PreferenciaDto> PreferenciasNoValidas =>
    [
        new PreferenciaDto(false, (ReglaRacha)99, "Europe/Madrid"),
        new PreferenciaDto(false, ReglaRacha.AlMenosUno, "Marte/Olimpo"),
    ];

    /// <summary>
    /// Guardar preferencias no válidas da 400.
    /// </summary>
    [Theory]
    [MemberData(nameof(PreferenciasNoValidas))]
    public async Task Preferencias_NoValidas_Da400(PreferenciaDto datos)
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync("/api/preferencias", datos, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
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
