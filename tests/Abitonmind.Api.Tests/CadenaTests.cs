using System.Net;
using System.Net.Http.Json;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using static Abitonmind.Api.Tests.HabitosDePrueba;

namespace Abitonmind.Api.Tests;

public class CadenaTests(AbitonmindFactory api) : IClassFixture<AbitonmindFactory>
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    /// <summary>
    /// Un hábito con hora no puede ir después de otro: o hora, o anterior.
    /// </summary>
    [Fact]
    public async Task CrearDespues_ConHora_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int levantarseId = await cliente.CrearHabitoAsync(HabitoValido("Levantarse"));

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/habitos", HabitoValido("Desayunar") with { Hora = new TimeOnly(7, 30), HabitoAntecesorId = levantarseId }, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Los días del hijo tienen que estar dentro de los de su anterior: el sábado sobra.
    /// </summary>
    [Fact]
    public async Task CrearDespues_ConUnDiaQueElAnteriorNoTiene_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int levantarseId = await cliente.CrearHabitoAsync(HabitoValido("Levantarse") with { Dias = DiasSemana.Lunes | DiasSemana.Martes });

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/habitos", HabitoValido("Desayunar") with { Dias = DiasSemana.Lunes | DiasSemana.Sabado, HabitoAntecesorId = levantarseId }, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Ir después de un hábito que no existe da 400.
    /// </summary>
    [Fact]
    public async Task CrearDespues_DeUnoQueNoExiste_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/habitos", HabitoValido("Desayunar") with { HabitoAntecesorId = 999999 }, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Ir después de un hábito de otro usuario da 400, como si no existiera.
    /// </summary>
    [Fact]
    public async Task CrearDespues_DeUnoDeOtroUsuario_Da400()
    {
        HttpClient ana = await api.ClienteNuevoAsync();
        HttpClient luis = await api.ClienteNuevoAsync();
        int levantarseDeAnaId = await ana.CrearHabitoAsync(HabitoValido("Levantarse"));

        HttpResponseMessage respuesta = await luis.PostAsJsonAsync("/api/habitos", HabitoValido("Desayunar") with { HabitoAntecesorId = levantarseDeAnaId }, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Con los mismos días que su anterior, se crea: 201, y lleva el id de su anterior.
    /// </summary>
    [Fact]
    public async Task CrearDespues_ConLosMismosDias_Da201ConSuAnterior()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int levantarseId = await cliente.CrearHabitoAsync(HabitoValido("Levantarse"));

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/habitos", HabitoValido("Hacer la cama") with { HabitoAntecesorId = levantarseId }, _ct);
        HabitoDto? hacerLaCama = await respuesta.Content.ReadFromJsonAsync<HabitoDto>(_ct);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.Equal(levantarseId, hacerLaCama!.HabitoAntecesorId);
    }

    /// <summary>
    /// Dientes va después de Levantarse. Poner Levantarse después de Dientes haría un ciclo: 400.
    /// </summary>
    [Fact]
    public async Task EditarParaHacerUnCiclo_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        GuardarHabitoDto levantarse = HabitoValido("Levantarse");
        int levantarseId = await cliente.CrearHabitoAsync(levantarse);
        int dientesId = await cliente.CrearHabitoAsync(HabitoValido("Dientes") with { HabitoAntecesorId = levantarseId });

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync($"/api/habitos/{levantarseId}", levantarse with { HabitoAntecesorId = dientesId }, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Un hábito no puede ir después de sí mismo: 400.
    /// </summary>
    [Fact]
    public async Task EditarDespuesDeSiMismo_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        GuardarHabitoDto levantarse = HabitoValido("Levantarse");
        int levantarseId = await cliente.CrearHabitoAsync(levantarse);

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync($"/api/habitos/{levantarseId}", levantarse with { HabitoAntecesorId = levantarseId }, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Un hábito sin días no puede ir en una cadena (P51).
    /// </summary>
    [Fact]
    public async Task CrearDespues_SinDias_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int levantarseId = await cliente.CrearHabitoAsync(HabitoValido("Levantarse"));

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/habitos", HabitoValido("Desayunar") with { Dias = DiasSemana.Ninguno, HabitoAntecesorId = levantarseId }, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Tampoco después de otro sin días, aunque los dos tengan los mismos (ninguno) (P51).
    /// </summary>
    [Fact]
    public async Task CrearSinDias_DespuesDeUnoSinDias_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int trasteroId = await cliente.CrearHabitoAsync(HabitoValido("Ordenar el trastero") with { Dias = DiasSemana.Ninguno });

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/habitos", HabitoValido("Tirar lo que sobra") with { Dias = DiasSemana.Ninguno, HabitoAntecesorId = trasteroId }, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Dejar sin días a un encadenado, sin quitarle el anterior, da 400 (P57).
    /// </summary>
    [Fact]
    public async Task EditarParaDejarSinDias_ConSuAnterior_Da400()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int levantarseId = await cliente.CrearHabitoAsync(HabitoValido("Levantarse"));
        GuardarHabitoDto dientes = HabitoValido("Dientes") with { HabitoAntecesorId = levantarseId };
        int dientesId = await cliente.CrearHabitoAsync(dientes);

        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync($"/api/habitos/{dientesId}", dientes with { Dias = DiasSemana.Ninguno }, _ct);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Levantarse → Dientes → Afeitarse. Eliminar Levantarse archiva toda la cadena: los tres dan 404.
    /// </summary>
    [Fact]
    public async Task EliminarElPrimero_Da204_YArchivaLaCadena()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        int levantarseId = await cliente.CrearHabitoAsync(HabitoValido("Levantarse"));
        int dientesId = await cliente.CrearHabitoAsync(HabitoValido("Dientes") with { HabitoAntecesorId = levantarseId });
        int afeitarseId = await cliente.CrearHabitoAsync(HabitoValido("Afeitarse") with { HabitoAntecesorId = dientesId });

        HttpResponseMessage respuesta = await cliente.DeleteAsync($"/api/habitos/{levantarseId}", _ct);
        HttpResponseMessage[] pedidos =
        [
            await cliente.GetAsync($"/api/habitos/{levantarseId}", _ct),
            await cliente.GetAsync($"/api/habitos/{dientesId}", _ct),
            await cliente.GetAsync($"/api/habitos/{afeitarseId}", _ct),
        ];

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.All(pedidos, p => Assert.Equal(HttpStatusCode.NotFound, p.StatusCode));
    }

    /// <summary>
    /// Quitar días al primero los quita también a sus encadenados, y archiva al que se queda sin días (P20).
    /// Despertar (L-V) → Estirar (L, X) → Meditar (X).
    /// </summary>
    [Fact]
    public async Task QuitarDiasAlPrimero_RecortaLosEncadenados_YArchivaLosQueSeQuedanSinDias()
    {
        HttpClient cliente = await api.ClienteNuevoAsync();
        GuardarHabitoDto despertar = HabitoValido("Despertar") with { Dias = DiasSemana.Lunes | DiasSemana.Martes | DiasSemana.Miercoles | DiasSemana.Jueves | DiasSemana.Viernes };
        int despertarId = await cliente.CrearHabitoAsync(despertar);
        int estirarId = await cliente.CrearHabitoAsync(HabitoValido("Estirar") with { Dias = DiasSemana.Lunes | DiasSemana.Miercoles, HabitoAntecesorId = despertarId });
        int meditarId = await cliente.CrearHabitoAsync(HabitoValido("Meditar") with { Dias = DiasSemana.Miercoles, HabitoAntecesorId = estirarId });

        // 1. Despertar pasa a L, X, J y V: el martes no lo tenía ninguno de los dos, así que no cambian.
        await EditarAsync(cliente, despertarId, despertar with { Dias = DiasSemana.Lunes | DiasSemana.Miercoles | DiasSemana.Jueves | DiasSemana.Viernes });

        Assert.Equal(DiasSemana.Lunes | DiasSemana.Miercoles, await DiasDeAsync(cliente, estirarId));
        Assert.Equal(DiasSemana.Miercoles, await DiasDeAsync(cliente, meditarId));

        // 2. Despertar pasa a L, J y V: Estirar pierde el miércoles, y Meditar se queda sin días y se archiva.
        await EditarAsync(cliente, despertarId, despertar with { Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Viernes });

        Assert.Equal(DiasSemana.Lunes, await DiasDeAsync(cliente, estirarId));
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/habitos/{meditarId}", _ct)).StatusCode);

        // 3. Despertar pasa a J y V: Estirar se queda sin días y se archiva.
        await EditarAsync(cliente, despertarId, despertar with { Dias = DiasSemana.Jueves | DiasSemana.Viernes });

        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/habitos/{estirarId}", _ct)).StatusCode);
        Assert.Equal(DiasSemana.Jueves | DiasSemana.Viernes, await DiasDeAsync(cliente, despertarId));
    }

    /// <summary>
    /// Edita un hábito y comprueba que la API responde 204.
    /// </summary>
    private async Task EditarAsync(HttpClient cliente, int id, GuardarHabitoDto datos)
    {
        HttpResponseMessage respuesta = await cliente.PutAsJsonAsync($"/api/habitos/{id}", datos, _ct);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
    }

    /// <summary>
    /// Pide un hábito y devuelve sus días.
    /// </summary>
    private async Task<DiasSemana> DiasDeAsync(HttpClient cliente, int id)
    {
        HabitoDto? habito = await cliente.GetFromJsonAsync<HabitoDto>($"/api/habitos/{id}", _ct);

        return habito!.Dias;
    }
}
