namespace Abitonmind.Core;

/// <summary>
/// Un hábito hecho en una fecha concreta
/// </summary>
public class RegistroHabito
{
    /// <summary>
    /// Id del registro
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Id del hábito al que hace referencia el registro.
    /// </summary>
    public int HabitoId { get; set; }

    /// <summary>
    /// Fecha de realización del hábito.
    /// </summary>
    public DateOnly Fecha { get; set; }
}