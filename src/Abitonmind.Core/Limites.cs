namespace Abitonmind.Core;

/// <summary>
/// Clase que sirve para recapitular las distintas restricciones en un mismo lugar.
/// </summary>
public static class Limites
{
    /// <summary>
    /// Limite de caracteres en el texto de una frase.
    /// </summary>
    public const int TextoFrase = 200;

    /// <summary>
    /// Limite de caracteres en el nombre de un habito y paso minimo.
    /// </summary>
    public const int Nombre = 100;

    /// <summary>
    /// Limite de caracteres en la preparación de un hábito y un paso mínimo.
    /// </summary>
    public const int Preparacion = 100;

    /// <summary>
    /// Límite de caracteres en la descripción de un hábito y un paso mínimo
    /// </summary>
    public const int Descripcion = 255;

    /// <summary>
    /// Límite de caracteres en la identidad de una persona.
    /// </summary>
    public const int Identidad = 255;

    /// <summary>
    /// Límite de caracteres en la zona horaria.
    /// </summary>
    public const int ZonaHoraria = 40;
}