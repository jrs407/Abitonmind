using System.Net.Http.Json;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;

namespace Abitonmind.Api.Tests;


public static class HabitosDePrueba
{
    /// <summary>
    /// Un hábito válido: todos los días, sin hora y sin anterior. Cada test cambia solo lo que le interesa, con "with".
    /// </summary>
    /// <param name="nombre">El nombre del hábito.</param>
    /// <returns>Los datos para crearlo o editarlo.</returns>
    public static GuardarHabitoDto HabitoValido(string nombre)
    {
        return new GuardarHabitoDto(nombre, null, null, null, DiasSemana.Todos, null, null, null, false);
    }

    /// <summary>
    /// Crea un hábito y devuelve su id. Si la API no lo crea, el test falla aquí.
    /// </summary>
    /// <param name="cliente">El cliente con la sesión del usuario.</param>
    /// <param name="datos">Los datos del hábito.</param>
    /// <returns>El id del hábito creado.</returns>
    public static async Task<int> CrearHabitoAsync(this HttpClient cliente, GuardarHabitoDto datos)
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        HttpResponseMessage respuesta = await cliente.PostAsJsonAsync("/api/habitos", datos, ct);
        respuesta.EnsureSuccessStatusCode();

        HabitoDto? habito = await respuesta.Content.ReadFromJsonAsync<HabitoDto>(ct);

        return habito!.Id;
    }
}
