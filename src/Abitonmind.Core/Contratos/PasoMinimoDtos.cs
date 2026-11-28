using System.ComponentModel.DataAnnotations;

namespace Abitonmind.Core.Contratos;

/// <summary>
/// Lo que la API devuelve de un paso mínimo.
/// </summary>
/// <param name="Id"></param>
/// <param name="Nombre"></param>
/// <param name="Descripcion"></param>
/// <param name="Preparacion"></param>
/// <param name="Repeticion"></param>
/// <param name="ColorPasoMinimo"></param>
public record PasoMinimoDto(int Id, string Nombre, string? Descripcion, string? Preparacion, int Repeticion, ColorEtiqueta? ColorPasoMinimo);

/// <summary>
/// Lo que la app envía para crear o editar un paso mínimo.
/// </summary>
/// <param name="Nombre"></param>
/// <param name="Descripcion"></param>
/// <param name="Preparacion"></param>
/// <param name="ColorPasoMinimo"></param>
public record GuardarPasoMinimoDto([Required(ErrorMessage = "El campo de nombre es obligatorio")]
                                [MaxLength(Limites.Nombre, ErrorMessage = "El campo de nombre no puede superar los {1} caracteres")]
                                string Nombre,
                                
                                [MaxLength(Limites.Descripcion, ErrorMessage = "El campo de descripcion no puede superar los {1} caracteres.")]
                                string? Descripcion,
                                
                                [MaxLength(Limites.Preparacion, ErrorMessage = "El campo preparación no puede superar los {1} caracteres.")]
                                string? Preparacion,
                                
                                [EnumDataType(typeof(ColorEtiqueta), ErrorMessage = "No existe ese tipo de color para un paso minimo")]
                                ColorEtiqueta? ColorPasoMinimo)
{
    /// <summary>
    /// El nombre, sin espacios al principio ni al final. Si solo tenía espacios, queda vacío y [Required] lo para.
    /// </summary>
    public string Nombre { get; init; } = Textos.Limpio(Nombre) ?? "";

    /// <summary>
    /// La descripción, sin espacios al principio ni al final. Si solo tenía espacios, queda en null.
    /// </summary>
    public string? Descripcion { get; init; } = Textos.Limpio(Descripcion);

    /// <summary>
    /// La preparación, sin espacios al principio ni al final. Si solo tenía espacios, queda en null.
    /// </summary>
    public string? Preparacion { get; init; } = Textos.Limpio(Preparacion);
}