using System.Security.Claims;

namespace Abitonmind.Api;

/// <summary>
/// Clase estática que sirve para centralizar métodos de la api.
/// </summary>
public static class ExtensionesDeSesion
{
    /// <summary>
    /// Obtiene el id del usuario.
    /// </summary>
    /// <param name="usuario"></param>
    /// <returns></returns>
    public static string IdDelUsuario(this ClaimsPrincipal usuario)
    {
        return usuario.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}