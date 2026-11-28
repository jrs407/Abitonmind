using System.ComponentModel.DataAnnotations;

namespace Abitonmind.Core.Contratos;

/// <summary>
/// Sirve para actualizar y obtener las preferencias de un usuario en opciones.
/// </summary>
/// <param name="HabitoPlegado"></param>
/// <param name="TipoRacha"></param>
/// <param name="ZonaHoraria"></param>
public record PreferenciaDto(bool HabitoPlegado,

                            [EnumDataType(typeof(ReglaRacha), ErrorMessage = "Ese tipo de racha no existe")]
                            ReglaRacha TipoRacha,

                            [MaxLength(Limites.ZonaHoraria, ErrorMessage = "Esa zona horaria no existe")]
                            string? ZonaHoraria) : IValidatableObject
{
    /// <summary>
    /// La zona horaria, sin espacios al principio ni al final. Si solo tenía espacios, queda en null.
    /// </summary>
    public string? ZonaHoraria { get; init; } = Textos.Limpio(ZonaHoraria);

    /// <summary>
    /// Comprueba que la zona horaria, si la hay, exista.
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ZonaHoraria is not null && !TimeZoneInfo.TryFindSystemTimeZoneById(ZonaHoraria, out _))
        {
            yield return new ValidationResult("Esa zona horaria no existe", [nameof(ZonaHoraria)]);
        }
    }
}
