namespace Abitonmind.Core.Contratos;

/// <summary>
/// Sirve para obtener las frases almacenadas en la base de datos.
/// </summary>
/// <param name="Id"></param>
/// <param name="Texto"></param>
public record FraseDto(int Id, string Texto);