namespace Abitonmind.Core;

/// <summary>
/// Clase que sirve para guardar frases motivacionales.
/// </summary>
public class Frase
{
    /// <summary>
    /// Id de la clase Frase.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Texto motivacional a modo de decoración en la aplicación.
    /// </summary>
    public required string Texto { get; set; }
}