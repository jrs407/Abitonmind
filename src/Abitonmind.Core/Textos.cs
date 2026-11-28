namespace Abitonmind.Core;

/// <summary>
/// Clase que sirve para dejar los textos que llegan de la app tal y como se guardan.
/// </summary>
public static class Textos
{
    /// <summary>
    /// Quita los espacios del principio y del final. Si no queda nada (o no había texto), devuelve null.
    /// </summary>
    /// <param name="texto"></param>
    /// <returns></returns>
    public static string? Limpio(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        return texto.Trim();
    }
}
