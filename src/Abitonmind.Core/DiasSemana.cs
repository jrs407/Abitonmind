namespace Abitonmind.Core;

/// <summary>
/// Valor utilizado para asignar un día, o varios días de la semana a un habito concreto.
/// </summary>
[Flags]
public enum DiasSemana
{
    Ninguno = 0,
    Lunes = 1,
    Martes = 2,
    Miercoles = 4,
    Jueves = 8,
    Viernes = 16,
    Sabado = 32,
    Domingo = 64,

    Todos = Lunes | Martes | Miercoles | Jueves | Viernes | Sabado | Domingo
}