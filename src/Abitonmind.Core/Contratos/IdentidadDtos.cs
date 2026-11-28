using System.ComponentModel.DataAnnotations;

namespace Abitonmind.Core.Contratos;

/// <summary>
/// Sirve tanto para editar la identidad de un usuario como para leerlo.
/// </summary>
/// <param name="Identidad"></param>
public record IdentidadDto([MaxLength(Limites.Identidad, ErrorMessage = "La identidad es demasiado larga")]
                            string? Identidad)
{
    /// <summary>
    /// La identidad, sin espacios al principio ni al final. Si solo tenía espacios, queda en null.
    /// </summary>
    public string? Identidad { get; init; } = Textos.Limpio(Identidad);
}
