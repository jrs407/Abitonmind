using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Abitonmind.Api.Tests;

public static class Sesiones
{
    /// <summary>
    /// Contraseña de todos los usuarios de los tests.
    /// </summary>
    public const string Contrasena = "Contrasena1234.";

    /// <summary>
    /// Registra el email (si ya existe, da 400 y no pasa nada), inicia sesión y devuelve un cliente con el token puesto.
    /// </summary>
    /// <param name="api">La API arrancada en memoria.</param>
    /// <param name="email">El email del usuario.</param>
    /// <returns>Un cliente que manda el token en cada petición.</returns>
    public static async Task<HttpClient> ClienteConSesionAsync(this WebApplicationFactory<Program> api, string email)
    {
        CancellationToken ct = TestContext.Current.CancellationToken;
        HttpClient cliente = api.CreateClient();

        await cliente.PostAsJsonAsync("/api/cuenta/register", new RegisterRequest { Email = email, Password = Contrasena }, ct);

        HttpResponseMessage login = await cliente.PostAsJsonAsync("/api/cuenta/login", new LoginRequest { Email = email, Password = Contrasena }, ct);
        login.EnsureSuccessStatusCode();

        AccessTokenResponse? token = await login.Content.ReadFromJsonAsync<AccessTokenResponse>(ct);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        return cliente;
    }

    /// <summary>
    /// Un usuario nuevo, con un email que no se repite.
    /// </summary>
    /// <param name="api">La API arrancada en memoria.</param>
    /// <returns>Un cliente con la sesión del usuario nuevo.</returns>
    public static Task<HttpClient> ClienteNuevoAsync(this WebApplicationFactory<Program> api)
    {
        return api.ClienteConSesionAsync($"{Guid.NewGuid()}@example.com");
    }

    /// <summary>
    /// Registra al admin, reinicia la API para que le dé el rol, y entra.
    /// </summary>
    /// <param name="api">La API arrancada en memoria.</param>
    /// <returns>Un cliente con la sesión del admin, cuyo token ya lleva el rol.</returns>
    public static async Task<HttpClient> ClienteAdminAsync(this WebApplicationFactory<Program> api)
    {
        CancellationToken ct = TestContext.Current.CancellationToken;

        // 1. Se registra. Si ya existe, da 400 y no pasa nada.
        await api.CreateClient().PostAsJsonAsync("/api/cuenta/register", new RegisterRequest { Email = AbitonmindFactory.EmailAdmin, Password = Contrasena }, ct);

        // 2. Reinicia la API, sin cambiar nada: al arrancar, le da el rol.
        WebApplicationFactory<Program> reiniciada = api.WithWebHostBuilder(_ => { });

        // 3. Entra en la API reiniciada. Su token ya lleva el rol.
        return await reiniciada.ClienteConSesionAsync(AbitonmindFactory.EmailAdmin);
    }
}
