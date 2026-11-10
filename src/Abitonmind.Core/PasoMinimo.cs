namespace Abitonmind.Core;

/// <summary>
/// Clase para definir que es un paso minimo, (Un habito autoconclusivo y autoreferencial que se repite muchas veces)
/// </summary>
public class PasoMinimo
{
    /// <summary>
    /// Id del paso mínimo.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Id que hace referencia al usuario que va asociado un paso mínimo.
    /// </summary>
    public string UsuarioId { get; set; }  = string.Empty;

    /// <summary>
    /// Nombre descriptivo del paso mínimo.
    /// </summary>
    public required string Nombre { get; set; }

    /// <summary>
    /// Descripción que detalla el paso mínimo, puede ser opcional.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Describe que preparación hace falta para poder hacer el paso mínimo.
    /// </summary>
    public string? Preparacion { get; set; }

    /// <summary>
    /// Describe cuantas veces se ha repetido el paso mínimo.
    /// </summary>
    public int Repeticion { get; set; }

    /// <summary>
    /// El color con el que se coloreará el paso mínimo.
    /// </summary>
    public ColorEtiqueta? ColorPasoMinimo { get; set; }

    /// <summary>
    /// Fecha en la que se archivó un paso mínimo y no está activo. Un null, significa que sigue activo.
    /// </summary>
    public DateOnly? FechaArchivado { get; set; }
}