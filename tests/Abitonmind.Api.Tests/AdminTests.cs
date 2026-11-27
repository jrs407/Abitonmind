using System.Net;
using System.Net.Http.Json;
using Abitonmind.Core.Contratos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Abitonmind.Api.Tests;

public class AdminTests
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    /// <summary>
    /// Si el secreto pasa a otro email, el admin de antes deja de serlo y el nuevo lo es (P113).
    /// </summary>
    [Fact]
    public async Task CambiarElSecreto_ElAnteriorDejaDeSerlo_YElNuevoLoEs()
    {
        await using var api = new AbitonmindFactory();
        await api.ClienteAdminAsync();
        await api.ClienteConSesionAsync("otro@example.com");

        WebApplicationFactory<Program> conOtro = api.WithWebHostBuilder(b => b.UseSetting("Admin:Email", "otro@example.com"));
        HttpClient anterior = await conOtro.ClienteConSesionAsync(AbitonmindFactory.EmailAdmin);
        HttpClient nuevo = await conOtro.ClienteConSesionAsync("otro@example.com");

        YoDto? yoAnterior = await anterior.GetFromJsonAsync<YoDto>("/api/yo", _ct);
        YoDto? yoNuevo = await nuevo.GetFromJsonAsync<YoDto>("/api/yo", _ct);

        Assert.False(yoAnterior?.EsAdmin);
        Assert.True(yoNuevo?.EsAdmin);
    }

    /// <summary>
    /// Si se quita el secreto, el admin de antes deja de serlo (P118).
    /// </summary>
    [Fact]
    public async Task QuitarElSecreto_ElAnteriorDejaDeSerlo()
    {
        await using var api = new AbitonmindFactory();
        await api.ClienteAdminAsync();

        WebApplicationFactory<Program> sinSecreto = api.WithWebHostBuilder(b => b.UseSetting("Admin:Email", null));
        HttpClient anterior = await sinSecreto.ClienteConSesionAsync(AbitonmindFactory.EmailAdmin);

        YoDto? yo = await anterior.GetFromJsonAsync<YoDto>("/api/yo", _ct);

        Assert.False(yo?.EsAdmin);
    }

    /// <summary>
    /// No se puede borrar la última frase: 400, con su type (P119).
    /// </summary>
    [Fact]
    public async Task BorrarLaUltimaFrase_Da400ConSuType()
    {
        await using var api = new AbitonmindFactory();
        HttpClient admin = await api.ClienteAdminAsync();
        List<FraseDto> frases = (await admin.GetFromJsonAsync<List<FraseDto>>("/api/frases", _ct))!;

        foreach (FraseDto frase in frases.Skip(1))
        {
            await admin.DeleteAsync($"/api/frases/{frase.Id}", _ct);
        }

        HttpResponseMessage respuesta = await admin.DeleteAsync($"/api/frases/{frases[0].Id}", _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        ProblemDetails? problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>(_ct);
        Assert.Equal(TiposDeError.UltimaFrase, problema?.Type);
    }
}
