namespace Abitonmind.Core.Contratos;

/// <summary>
/// Lo que la API devuelve de un hábito.
/// </summary>
/// <param name="Id"></param>
/// <param name="Nombre"></param>
/// <param name="Descripcion"></param>
/// <param name="Hora"></param>
/// <param name="Dias"></param>
/// <param name="Tipo"></param>
public record HabitoDto(int Id, string Nombre, string? Descripcion, TimeOnly? Hora, DiasSemana Dias, TipoHabito? Tipo);

/// <summary>
/// Lo que la app envía para crear o editar un hábito.
/// </summary>
/// <param name="Nombre"></param>
/// <param name="Descripcion"></param>
/// <param name="Hora"></param>
/// <param name="Dias"></param>
/// <param name="Tipo"></param>
public record GuardarHabitoDto(string Nombre, string? Descripcion, TimeOnly? Hora, DiasSemana Dias, TipoHabito? Tipo);