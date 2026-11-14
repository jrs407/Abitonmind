namespace Abitonmind.Core;

/// <summary>
/// Clase estática que sirve para hacer comprobaciones sobre cadenas de hábitos.
/// </summary>
public static class Cadena
{
    /// <summary>
    /// La idea es partir de un id de un antecesor y tratar de deducir, si se forma un ciclo con ese habito, recorriendo la cadena.
    /// </summary>
    /// <param name="habitoId"></param>
    /// <param name="habitoAntecesorId"></param>
    /// <param name="cadena"></param>
    /// <returns></returns>
    public static bool ProvocaCiclo(int habitoId, int habitoAntecesorId, IReadOnlyDictionary<int, int?> cadena)
    {
        int? actual = habitoAntecesorId;
        int pasos = 0;

        while (actual is not null)
        {
            if (actual == habitoId)
            {
                return true;
            }

            if (pasos++ > cadena.Count)
            {
                return true;
            }

            actual = cadena.GetValueOrDefault(actual.Value);
        }

        return false;
    }

    /// <summary>
    /// Dado la lista de habitos que tiene un usuario y un id de un habito concreto, se obtienen todos los hábitos posteriores a este.
    /// </summary>
    /// <param name="habitoId"></param>
    /// <param name="antecesores"></param>
    /// <returns></returns>
    public static List<int> HabitosPosteriores(int habitoId, IReadOnlyDictionary<int, int?> antecesores)
    {
        var cola = new Queue<int>();
        List<int> result = new List<int>();

        cola.Enqueue(habitoId);

        while(cola.Count > 0)
        {
            int actual = cola.Dequeue();

            IEnumerable<int> listaIdHabitos = antecesores.Where(a => a.Value == actual).Select(a => a.Key);

            foreach (int idHabitoActual in listaIdHabitos)
            {
                cola.Enqueue(idHabitoActual);
                result.Add(idHabitoActual);
            }
 
        }

        return result;
    }
}