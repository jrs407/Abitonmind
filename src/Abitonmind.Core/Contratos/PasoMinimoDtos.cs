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
public record GuardarPasoMinimoDto(string Nombre, string? Descripcion, string? Preparacion, ColorEtiqueta? ColorPasoMinimo);