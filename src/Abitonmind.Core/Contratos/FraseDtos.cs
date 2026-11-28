using System.ComponentModel.DataAnnotations;

namespace Abitonmind.Core.Contratos;

/// <summary>
/// Sirve para obtener las frases almacenadas en la base de datos.
/// </summary>
/// <param name="Id"></param>
/// <param name="Texto"></param>
public record FraseDto(int Id, string Texto);

/// <summary>
/// Sirve para guardar una frase a la base de datos. Se comprueba las condiciones.
/// </summary>
/// <param name="Texto"></param>
public record GuardarFraseDto(
    [Required(ErrorMessage = "El texto es obligatorio.")]
    [MaxLength(Limites.TextoFrase, ErrorMessage = "El texto no puede pasar de {1} caracteres.")]
    string Texto)
{
    /// <summary>
    /// El texto, sin espacios al principio ni al final. Si solo tenía espacios, queda vacío y [Required] lo para.
    /// </summary>
    public string Texto { get; init; } = Textos.Limpio(Texto) ?? "";
}

/// <summary>
/// Sirve para saber si un usuario es admin.
/// </summary>
/// <param name="EsAdmin"></param>
public record YoDto(bool EsAdmin);
