using System.Net;
using System.Net.Http.Json;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;

namespace Abitonmind.Api.Tests;

public class PasosMinimosTests(AbitonmindFactory api) : IClassFixture<AbitonmindFactory>
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    /// <summary>
    /// Crear un paso da 201, y empieza con 0 repeticiones.
    /// </summary>
    [Fact]
    public async Task Crear_Da201_ConCeroRepeticiones()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/pasosminimos", Meditar(), _ct);
        PasoMinimoDto? paso = await respuesta.Content.ReadFromJsonAsync<PasoMinimoDto>(_ct);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.Equal(0, paso!.Repeticion);
    }

    /// <summary>
    /// Cada + y cada − responde con las repeticiones que quedan.
    /// </summary>
    [Fact]
    public async Task SumarTresYRestarUna_RespondenUnoDosTresYDos()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int id = await CrearAsync(cliente, Meditar());

        int[] repeticiones =
        [
            await RepeticionesTrasAsync(cliente, id, "sumar"),
            await RepeticionesTrasAsync(cliente, id, "sumar"),
            await RepeticionesTrasAsync(cliente, id, "sumar"),
            await RepeticionesTrasAsync(cliente, id, "restar"),
        ];

        Assert.Equal([1, 2, 3, 2], repeticiones);
    }

    /// <summary>
    /// Restar con 0 da 200, y se queda en 0 (P61).
    /// </summary>
    [Fact]
    public async Task Restar_ConCero_Da200_YSigueEnCero()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int id = await CrearAsync(cliente, Meditar());

        HttpResponseMessage respuesta = await cliente.PostAsync($"/api/pasosminimos/{id}/restar", null, _ct);
        PasoMinimoDto? paso = await respuesta.Content.ReadFromJsonAsync<PasoMinimoDto>(_ct);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(0, paso!.Repeticion);
    }

    /// <summary>
    /// Editar el nombre no toca las repeticiones.
    /// </summary>
    [Fact]
    public async Task EditarNombre_NoCambiaLasRepeticiones()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        GuardarPasoMinimoDto meditar = Meditar();
        int id = await CrearAsync(cliente, meditar);
        await RepeticionesTrasAsync(cliente, id, "sumar");
        await RepeticionesTrasAsync(cliente, id, "sumar");

        (await cliente.PutAsJsonAsync($"/api/pasosminimos/{id}", meditar with { Nombre = "Meditar 5 minutos" }, _ct)).EnsureSuccessStatusCode();
        PasoMinimoDto? visto = await cliente.GetFromJsonAsync<PasoMinimoDto>($"/api/pasosminimos/{id}", _ct);

        Assert.Equal(2, visto!.Repeticion);
    }

    /// <summary>
    /// Pasos que la API no tiene que aceptar.
    /// </summary>
    public static TheoryData<GuardarPasoMinimoDto> PasosNoValidos =>
    [
        Meditar() with { Nombre = "" },
        Meditar() with { ColorPasoMinimo = (ColorEtiqueta)99 },
    ];

    /// <summary>
    /// Crear un paso no válido da 400.
    /// </summary>
    [Theory]
    [MemberData(nameof(PasosNoValidos))]
    public async Task Crear_NoValido_Da400(GuardarPasoMinimoDto datos)
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/pasosminimos", datos, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Otro usuario no puede ver ni sumar el paso del primero: 404 y 404.
    /// </summary>
    [Fact]
    public async Task OtroUsuario_NoPuedeTocarElPaso_Da404()
    {
        HttpClient ana = await api.ClienteNuevoAsync();
        HttpClient luis = await api.ClienteNuevoAsync();
        int id = await CrearAsync(ana, Meditar());

        HttpResponseMessage[] respuestas =
        [
            await luis.GetAsync($"/api/pasosminimos/{id}", _ct),
            await luis.PostAsync($"/api/pasosminimos/{id}/sumar", null, _ct),
        ];

        Assert.All(respuestas, r => Assert.Equal(HttpStatusCode.NotFound, r.StatusCode));
    }

    /// <summary>
    /// Eliminar da 204. Después, pedirlo y sumarle dan 404, y no sale en la lista.
    /// </summary>
    [Fact]
    public async Task Eliminar_Da204_YYaNoSeVe()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int id = await CrearAsync(cliente, Meditar());

        HttpResponseMessage respuesta = await cliente.DeleteAsync($"/api/pasosminimos/{id}", _ct);
        HttpResponseMessage[] despues =
        [
            await cliente.GetAsync($"/api/pasosminimos/{id}", _ct),
            await cliente.PostAsync($"/api/pasosminimos/{id}/sumar", null, _ct),
        ];
        List<PasoMinimoDto>? pasos = await cliente.GetFromJsonAsync<List<PasoMinimoDto>>("/api/pasosminimos", _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.All(despues, r => Assert.Equal(HttpStatusCode.NotFound, r.StatusCode));
        Assert.DoesNotContain(pasos!, p => p.Id == id);
    }

    /// <summary>
    /// Un paso válido. Cada test cambia solo lo que le interesa, con "with".
    /// </summary>
    private static GuardarPasoMinimoDto Meditar()
    {
        return new GuardarPasoMinimoDto("Meditar", null, null, null);
    }

    /// <summary>
    /// Crea un paso y devuelve su id.
    /// </summary>
    private async Task<int> CrearAsync(HttpClient cliente, GuardarPasoMinimoDto datos)
    {
        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/pasosminimos", datos, _ct);
        respuesta.EnsureSuccessStatusCode();

        PasoMinimoDto? paso = await respuesta.Content.ReadFromJsonAsync<PasoMinimoDto>(_ct);

        return paso!.Id;
    }

    /// <summary>
    /// Pulsa + ("sumar") o − ("restar") y devuelve las repeticiones de la respuesta.
    /// </summary>
    private async Task<int> RepeticionesTrasAsync(HttpClient cliente, int id, string accion)
    {
        HttpResponseMessage respuesta = await cliente.PostAsync($"/api/pasosminimos/{id}/{accion}", null, _ct);
        respuesta.EnsureSuccessStatusCode();

        PasoMinimoDto? paso = await respuesta.Content.ReadFromJsonAsync<PasoMinimoDto>(_ct);

        return paso!.Repeticion;
    }
}
