using System.Net;
using System.Net.Http.Json;
using Abitonmind.Core.Contratos;
using Microsoft.AspNetCore.Mvc;

namespace Abitonmind.Api.Tests;

public class FrasesTests(AbitonmindFactory api) : IClassFixture<AbitonmindFactory>
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    /// <summary>
    /// Sin token, la API responde 401, y con un ProblemDetails.
    /// </summary>
    [Fact]
    public async Task Listar_SinSesion_Da401ConProblemDetails()
    {
        HttpClient cliente = api.CreateClient();

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/frases", _ct);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>
    /// Un usuario normal puede listar las frases: 200, y la lista no está vacía.
    /// </summary>
    [Fact]
    public async Task Listar_UsuarioNormal_Da200ConFrases()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/frases", _ct);
        List<FraseDto>? frases = await respuesta.Content.ReadFromJsonAsync<List<FraseDto>>(_ct);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.NotEmpty(frases!);
    }

    /// <summary>
    /// Un usuario normal no puede crear, editar ni borrar frases: 403, con un ProblemDetails.
    /// </summary>
    [Theory]
    [InlineData("POST", "/api/frases")]
    [InlineData("PUT", "/api/frases/1")]
    [InlineData("DELETE", "/api/frases/1")]
    public async Task UsuarioNormal_NoPuedeGestionarFrases_Da403(string metodo, string ruta)
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        var peticion = new HttpRequestMessage(new HttpMethod(metodo), ruta)
        {
            Content = JsonContent.Create(new GuardarFraseDto("Un paso cada día"))
        };

        HttpResponseMessage respuesta = await cliente.SendAsync(peticion, _ct);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        ProblemDetails? problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>(_ct);
        Assert.Equal(403, problema?.Status);
    }

    /// <summary>
    /// Sin token, no se puede crear, editar ni borrar frases: 401.
    /// </summary>
    [Theory]
    [InlineData("POST", "/api/frases")]
    [InlineData("PUT", "/api/frases/1")]
    [InlineData("DELETE", "/api/frases/1")]
    public async Task SinSesion_NoPuedeGestionarFrases_Da401(string metodo, string ruta)
    {
        HttpClient cliente = api.CreateClient();
        var peticion = new HttpRequestMessage(new HttpMethod(metodo), ruta)
        {
            Content = JsonContent.Create(new GuardarFraseDto("Un paso cada día"))
        };

        HttpResponseMessage respuesta = await cliente.SendAsync(peticion, _ct);

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    /// <summary>
    /// El admin crea una frase: 201, con la ruta de la frase y su texto en el cuerpo.
    /// </summary>
    [Fact]
    public async Task Admin_CreaUnaFrase_Da201ConSuRutaYSuTexto()
    {
        HttpClient admin = await api.ClienteAdminAsync();

        HttpResponseMessage creado = await admin.PostAsJsonAsync("/api/frases", new GuardarFraseDto("Un paso cada día"), _ct);
        FraseDto? frase = await creado.Content.ReadFromJsonAsync<FraseDto>(_ct);

        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        Assert.Equal($"/api/frases/{frase!.Id}", creado.Headers.Location?.ToString());
        Assert.Equal("Un paso cada día", frase.Texto);
    }

    /// <summary>
    /// Una frase vacía, o solo con espacios, no se guarda: 400 (P114).
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Admin_CreaFraseVacia_Da400(string texto)
    {
        HttpClient admin = await api.ClienteAdminAsync();

        HttpResponseMessage respuesta = await admin.PostAsJsonAsync("/api/frases", new GuardarFraseDto(texto), _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Una frase de 200 caracteres justos se guarda. Una de 201, no (P115).
    /// </summary>
    [Theory]
    [InlineData(200, HttpStatusCode.Created)]
    [InlineData(201, HttpStatusCode.BadRequest)]
    public async Task Admin_CreaFraseLarga(int caracteres, HttpStatusCode esperado)
    {
        HttpClient admin = await api.ClienteAdminAsync();

        HttpResponseMessage respuesta = await admin.PostAsJsonAsync("/api/frases", new GuardarFraseDto(new string('a', caracteres)), _ct);

        Assert.Equal(esperado, respuesta.StatusCode);
    }

    /// <summary>
    /// Los espacios del principio y del final no se guardan, y el máximo se cuenta sin ellos: una frase de 200
    /// con espacios alrededor da 201.
    /// </summary>
    [Fact]
    public async Task Admin_CreaFraseConEspaciosEnLosExtremos_SeGuardaSinEllos()
    {
        HttpClient admin = await api.ClienteAdminAsync();
        string texto = new string('a', 200);

        // Con "with", el texto llega a la API tal cual, con sus espacios.
        GuardarFraseDto datos = new GuardarFraseDto("") with { Texto = $"  {texto}  " };
        HttpResponseMessage creado = await admin.PostAsJsonAsync("/api/frases", datos, _ct);
        FraseDto? frase = await creado.Content.ReadFromJsonAsync<FraseDto>(_ct);

        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);
        Assert.Equal(texto, frase!.Texto);
    }

    /// <summary>
    /// El admin edita una frase que ha creado: 204, y al listar sale cambiada.
    /// </summary>
    [Fact]
    public async Task Admin_EditaUnaFrase_Da204_YSaleCambiada()
    {
        HttpClient admin = await api.ClienteAdminAsync();
        HttpResponseMessage creado = await admin.PostAsJsonAsync("/api/frases", new GuardarFraseDto("Un paso cada día"), _ct);
        FraseDto? frase = await creado.Content.ReadFromJsonAsync<FraseDto>(_ct);

        HttpResponseMessage respuesta = await admin.PutAsJsonAsync($"/api/frases/{frase!.Id}", new GuardarFraseDto("Dos pasos cada día"), _ct);
        List<FraseDto>? frases = await admin.GetFromJsonAsync<List<FraseDto>>("/api/frases", _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Contains(new FraseDto(frase.Id, "Dos pasos cada día"), frases!);
    }

    /// <summary>
    /// Editar una frase que no existe: 404.
    /// </summary>
    [Fact]
    public async Task Admin_EditaFraseQueNoExiste_Da404()
    {
        HttpClient admin = await api.ClienteAdminAsync();

        HttpResponseMessage respuesta = await admin.PutAsJsonAsync("/api/frases/999999", new GuardarFraseDto("Un paso cada día"), _ct);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    /// <summary>
    /// El admin borra una frase que ha creado: 204. Si la vuelve a borrar, 404.
    /// </summary>
    [Fact]
    public async Task Admin_BorraUnaFrase_Da204_YOtraVezDa404()
    {
        HttpClient admin = await api.ClienteAdminAsync();
        HttpResponseMessage creado = await admin.PostAsJsonAsync("/api/frases", new GuardarFraseDto("Un paso cada día"), _ct);
        FraseDto? frase = await creado.Content.ReadFromJsonAsync<FraseDto>(_ct);

        HttpResponseMessage primera = await admin.DeleteAsync($"/api/frases/{frase!.Id}", _ct);
        HttpResponseMessage segunda = await admin.DeleteAsync($"/api/frases/{frase.Id}", _ct);

        Assert.Equal(HttpStatusCode.NoContent, primera.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, segunda.StatusCode);
    }
}
