using System.Net;
using System.Net.Http.Json;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using static Abitonmind.Api.Tests.HabitosDePrueba;

namespace Abitonmind.Api.Tests;

public class HabitosTests(AbitonmindFactory api) : IClassFixture<AbitonmindFactory>
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    /// <summary>
    /// Sin token, la API responde 401, y con un ProblemDetails.
    /// </summary>
    [Fact]
    public async Task Listar_SinSesion_Da401ConProblemDetails()
    {
        HttpClient cliente = api.CreateClient();

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/habitos", _ct);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Crear da 201 con la ruta del hábito, y al pedirlo sale igual.
    /// </summary>
    [Fact]
    public async Task Crear_Da201_YLuegoSeVeIgual()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage creado = await cliente.PostAsJsonAsync("/api/habitos", HabitoValido("Leer"), _ct);
        HabitoDto? habito = await creado.Content.ReadFromJsonAsync<HabitoDto>(_ct);
        HabitoDto? visto = await cliente.GetFromJsonAsync<HabitoDto>($"/api/habitos/{habito!.Id}", _ct);

        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        Assert.Equal($"/api/habitos/{habito.Id}", creado.Headers.Location?.ToString());
        Assert.Equal(habito, visto);
    }

    /// <summary>
    /// Hábitos que la API no tiene que aceptar.
    /// </summary>
    public static TheoryData<GuardarHabitoDto> HabitosNoValidos =>
    [
        HabitoValido(""),
        HabitoValido("   "),
        HabitoValido(new string('a', 101)),
        HabitoValido("Leer") with { Descripcion = new string('a', 256) },
        HabitoValido("Leer") with { Preparacion = new string('a', 101) },
        HabitoValido("Leer") with { ColorHabito = (ColorEtiqueta)99 },
        HabitoValido("Leer") with { ColorHabito = (ColorEtiqueta)0 },
        HabitoValido("Leer") with { Tipo = (TipoHabito)7 },
        HabitoValido("Leer") with { Dias = (DiasSemana)128 },
        HabitoValido("Leer") with { Recordatorio = true },
    ];

    /// <summary>
    /// Crear un hábito no válido da 400.
    /// </summary>
    [Theory]
    [MemberData(nameof(HabitosNoValidos))]
    public async Task Crear_NoValido_Da400(GuardarHabitoDto datos)
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/habitos", datos, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Hábitos en el límite de lo permitido, que la API sí tiene que aceptar.
    /// </summary>
    public static TheoryData<GuardarHabitoDto> HabitosValidos =>
    [
        HabitoValido("Leer") with { Dias = DiasSemana.Ninguno },
        HabitoValido("Leer") with { Dias = DiasSemana.Todos },
        HabitoValido("Leer") with { ColorHabito = ColorEtiqueta.Color1 },
        HabitoValido("Leer") with { Hora = new TimeOnly(21, 30), Recordatorio = true },
    ];

    /// <summary>
    /// Crear un hábito válido da 201 (P46: también sin días).
    /// </summary>
    [Theory]
    [MemberData(nameof(HabitosValidos))]
    public async Task Crear_Valido_Da201(GuardarHabitoDto datos)
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/habitos", datos, _ct);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    /// <summary>
    /// Otro usuario no puede ver, editar, eliminar ni marcar el hábito del primero: 404 en las cuatro.
    /// </summary>
    [Fact]
    public async Task OtroUsuario_NoPuedeTocarElHabito_Da404()
    {
        HttpClient ana = await api.ClienteNuevoAsync();
        HttpClient luis = await api.ClienteNuevoAsync();
        int id = await ana.CrearHabitoAsync(HabitoValido("Leer"));
        string ruta = $"/api/habitos/{id}";

        HttpResponseMessage[] respuestas =
        [
            await luis.GetAsync(ruta, _ct),
            await luis.PutAsJsonAsync(ruta, HabitoValido("Cambiado por otro usuario"), _ct),
            await luis.DeleteAsync(ruta, _ct),
            await luis.PutAsync($"{ruta}/registros/2026-11-15", null, _ct),
        ];

        Assert.All(respuestas, r => Assert.Equal(HttpStatusCode.NotFound, r.StatusCode));
    }

    /// <summary>
    /// Otro usuario lista sus hábitos: 200, y no ve los del primero.
    /// </summary>
    [Fact]
    public async Task OtroUsuario_ListaSusHabitos_Da200YVacia()
    {
        HttpClient ana = await api.ClienteNuevoAsync();
        HttpClient luis = await api.ClienteNuevoAsync();
        await ana.CrearHabitoAsync(HabitoValido("Leer"));

        HttpResponseMessage respuesta = await luis.GetAsync("/api/habitos", _ct);
        List<HabitoDto>? deLuis = await respuesta.Content.ReadFromJsonAsync<List<HabitoDto>>(_ct);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Empty(deLuis!);
    }

    /// <summary>
    /// Editar el nombre da 204, y al pedirlo sale el nombre nuevo.
    /// </summary>
    [Fact]
    public async Task EditarNombre_Da204_YSaleElNuevo()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        GuardarHabitoDto leer = HabitoValido("Leer");
        int id = await cliente.CrearHabitoAsync(leer);

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync($"/api/habitos/{id}", leer with { Nombre = "Leer 10 páginas" }, _ct);
        HabitoDto? visto = await cliente.GetFromJsonAsync<HabitoDto>($"/api/habitos/{id}", _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Equal("Leer 10 páginas", visto!.Nombre);
    }

    /// <summary>
    /// Eliminar da 204. Después, pedirlo da 404 y no sale en la lista.
    /// </summary>
    [Fact]
    public async Task Eliminar_Da204_YYaNoSeVe()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int id = await cliente.CrearHabitoAsync(HabitoValido("Leer"));

        HttpResponseMessage respuesta = await cliente.DeleteAsync($"/api/habitos/{id}", _ct);
        HttpResponseMessage pedido = await cliente.GetAsync($"/api/habitos/{id}", _ct);
        List<HabitoDto>? habitos = await cliente.GetFromJsonAsync<List<HabitoDto>>("/api/habitos", _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, pedido.StatusCode);
        Assert.DoesNotContain(habitos!, h => h.Id == id);
    }

    /// <summary>
    /// Pedir un hábito que no existe da 404.
    /// </summary>
    [Fact]
    public async Task Pedir_NoExiste_Da404()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/habitos/999999", _ct);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
