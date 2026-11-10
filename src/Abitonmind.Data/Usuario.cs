using Abitonmind.Core;
using Microsoft.AspNetCore.Identity;

namespace Abitonmind.Data;

/// <summary>
/// Clase que sirve para identificar un usuario dentro de la aplicación.
/// </summary>
public class Usuario : IdentityUser
{
    /// <summary>
    /// Describe que es lo que desea ser un usuario.
    /// </summary>
    public string? Identidad { get; set; }

    /// <summary>
    /// Sirve para saber si tiene la lista de habitos hechos hoy contraida o desplegada.
    /// </summary>
    public bool HabitoPlegado { get; set; }

    /// <summary>
    /// Dice el tipo de racha (si tiene que cumplir al menos un habito, varios o todos para que se mantenga la racha).
    /// </summary>
    public ReglaRacha TipoRacha { get; set; }

    /// <summary>
    /// Guarda la zona horaria de un usuario, el formato es Europe/Madrid y null significa que aun no se tiene el dato, y se usa la zona horaria del servidor.
    /// </summary>
    public string? ZonaHoraria { get; set; }
}
