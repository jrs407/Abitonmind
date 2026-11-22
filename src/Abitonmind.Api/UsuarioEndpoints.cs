using System.Security.Claims;
using Abitonmind.Core;
using Abitonmind.Core.Contratos;
using Abitonmind.Data;
using Microsoft.EntityFrameworkCore;

namespace Abitonmind.Api;

/// <summary>
/// Clase que lista los endpoints relacionados con los usuarios.
/// </summary>
public static class UsuarioEndPoints
{
    public static void MapUsuarios(this WebApplication app)
    {
        // Ruta base del endpoint
        var grupo = app.MapGroup("/api/usuarios").RequireAuthorization();
    }
}