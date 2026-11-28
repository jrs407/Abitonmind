using System.ComponentModel.DataAnnotations;

namespace Abitonmind.Core.Contratos;

/// <summary>
/// Lo que la API devuelve de un hábito.
/// </summary>
/// <param name="Id"></param>
/// <param name="Nombre"></param>
/// <param name="Descripcion"></param>
/// <param name="Preparacion"></param>
/// <param name="Hora"></param>
/// <param name="Dias"></param>
/// <param name="Tipo"></param>
/// <param name="HabitoAntecesorId"></param>
/// <param name="ColorHabito"></param>
/// <param name="Recordatorio"></param>
public record HabitoDto(int Id, string Nombre, string? Descripcion, string? Preparacion, TimeOnly? Hora, DiasSemana Dias, TipoHabito? Tipo, int? HabitoAntecesorId, ColorEtiqueta? ColorHabito, bool Recordatorio);

/// <summary>
/// Lo que la app envía para crear o editar un hábito.
/// </summary>
/// <param name="Nombre"></param>
/// <param name="Descripcion"></param>
/// <param name="Preparacion"></param>
/// <param name="Hora"></param>
/// <param name="Dias"></param>
/// <param name="Tipo"></param>
/// <param name="HabitoAntecesorId"></param>
/// <param name="ColorHabito"></param>
/// <param name="Recordatorio"></param>
public record GuardarHabitoDto([Required(ErrorMessage = "El nombre es obligatorio")]
                            [MaxLength(Limites.Nombre, ErrorMessage = "El nombre no puede superar los {1} caracteres")]
                            string Nombre,

                            [MaxLength(Limites.Descripcion, ErrorMessage = "La descripcion no puede superar los {1} caracteres")]
                            string? Descripcion,

                            [MaxLength(Limites.Preparacion, ErrorMessage = "La preparación no puede superar los {1} caracteres.")]
                            string? Preparacion,

                            TimeOnly? Hora,

                            [EnumDataType(typeof(DiasSemana), ErrorMessage = "No existe ese dia de la semana")]
                            DiasSemana Dias,

                            [EnumDataType(typeof(TipoHabito), ErrorMessage = "No existe ese tipo de habito")]
                            TipoHabito? Tipo,

                            int? HabitoAntecesorId,

                            [EnumDataType(typeof(ColorEtiqueta), ErrorMessage = "No existe ese tipo de color para un habito")]
                            ColorEtiqueta? ColorHabito,

                            bool Recordatorio) : IValidatableObject
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

    /// <summary>
    /// Comprueba que para que haya recordatorio, tenga que haber una hora.
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Recordatorio && Hora is null)
        {
            yield return new ValidationResult("Solo puede haber un recordatorio si hay una hora de aviso.", [nameof(Recordatorio)]);
        }
    }
}