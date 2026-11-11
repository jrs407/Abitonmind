namespace Abitonmind.Core.Contratos;

/// <summary>
/// Sirve para actualizar y obtener las preferencias de un usuario en opciones.
/// </summary>
/// <param name="HabitoPlegado"></param>
/// <param name="TipoRacha"></param>
/// <param name="ZonaHoraria"></param>
public record PreferenciaDto(bool HabitoPlegado, ReglaRacha TipoRacha, string? ZonaHoraria);