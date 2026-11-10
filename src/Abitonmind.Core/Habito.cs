namespace Abitonmind.Core;

/// <summary>
/// Clase para definir que es un hábito.
/// </summary>
public class Habito
{
    /// <summary>
    /// Id del hábito.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// El id del usuario al que va asociado un hábito.
    /// </summary>
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre que tiene el hábito.
    /// </summary>
    public required string Nombre { get; set; }

    /// <summary>
    /// Descripción que puede ser opcional en un hábito.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Describe que preparación hace falta para poder hacer el hábito.
    /// </summary>
    public string? Preparacion { get; set; }

    /// <summary>
    /// Hora que puede ser opcional en un hábito.
    /// </summary>
    public TimeOnly? Hora { get; set; }

    /// <summary>
    /// Días que se hace un determinado hábito. (Lunes, martes, miercoles, etc).
    /// </summary>
    public DiasSemana Dias { get; set; }

    /// <summary>
    /// El tipo de hábito que es un hábito. (Positivo, Neutral, Negativo).
    /// </summary>
    public TipoHabito? Tipo { get; set; }

    /// <summary>
    /// Fecha en la que se archivó un hábito y no está activo. Un null, significa que el hábito sigue activo
    /// </summary>
    public DateOnly? FechaArchivado { get; set; }

    /// <summary>
    /// Guarda el id si lo hubiera de un habito que se hace antes de este, en caso de null, no tiene antecesor.
    /// </summary>
    public int? HabitoAntecesorId { get; set; }

    /// <summary>
    /// El color con el que se coloreará el habito.
    /// </summary>
    public ColorEtiqueta? ColorHabito { get; set; }

    /// <summary>
    /// Booleano que dice si un hábito produce un recordatorio, por defecto en false.
    /// </summary>
    public bool Recordatorio { get; set; }

    /// <summary>
    /// Método que devuelve un booleano si toca o no toca hoy este hábito.
    /// </summary>
    /// <param name="fecha"></param>
    /// <returns></returns>
    public bool TocaEl(DateOnly fecha)
    {
        if (FechaArchivado is not null && fecha > FechaArchivado)
            return false;

        DiasSemana diaConcreto = DiaDe(fecha.DayOfWeek);

        return Dias.HasFlag(diaConcreto);
    }

    /// <summary>
    /// Método que traduce el DayOfWeek de .NET a nuestro DiasSemana.
    /// </summary>
    /// <param name="diaSemana"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static DiasSemana DiaDe(DayOfWeek diaSemana) => diaSemana switch
    {
        DayOfWeek.Monday => DiasSemana.Lunes,
        DayOfWeek.Tuesday => DiasSemana.Martes,
        DayOfWeek.Wednesday => DiasSemana.Miercoles,
        DayOfWeek.Thursday => DiasSemana.Jueves,
        DayOfWeek.Friday => DiasSemana.Viernes,
        DayOfWeek.Saturday => DiasSemana.Sabado,
        DayOfWeek.Sunday => DiasSemana.Domingo,
        _ => throw new ArgumentOutOfRangeException(nameof(diaSemana), diaSemana, "El día tiene que estar entre el lunes al domingo")
    };
}