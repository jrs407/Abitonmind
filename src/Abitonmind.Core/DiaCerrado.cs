namespace Abitonmind.Core;

/// <summary>
/// Clase que sirve para guardar los datos de días concretos, actualmente sirve para saber si ese día se cumplió.
/// </summary>
public class DiaCerrado
{
    /// <summary>
    /// Id del día que se ha cerrado y guardado.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Usuario que va asociado el día cerrado.
    /// </summary>
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del día en el que se cerró.
    /// </summary>
    public DateOnly Fecha { get; set; }

    /// <summary>
    /// Dice si un día se cumplió.
    /// </summary>
    public bool SeCumplio { get; set; }
}