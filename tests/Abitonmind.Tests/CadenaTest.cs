using Abitonmind.Core;

namespace Abitonmind.Tests;

public class CadenaTest
{
    /// <summary>
    /// Comprueba que un habito, no pueda tener como padre, a si mismo.
    /// </summary>
    [Fact]
    public void ProvocaCiclo_DevuelveTrue_SiADespuesDeSiMismo()
    {
        var antecesores = new Dictionary<int, int?> { [1] = null, [2] = 1 };

        var result = Cadena.ProvocaCiclo(1, 1, antecesores);

        Assert.True(result);
    }

    /// <summary>
    /// Comprueba que si B, va despues de A, de true si se comprueba si A va despues de B
    /// </summary>
    [Fact]
    public void ProvocaCiclo_DevuelveTrue_SiSePoneElHijoComoPadre()
    {
        var antecesores = new Dictionary<int, int?> { [1] = null, [2] = 1 };

        var result = Cadena.ProvocaCiclo(1, 2, antecesores);

        Assert.True(result);
    }

    /// <summary>
    /// Comprueba que aunque haya una serie de habitos por medio, se sigue provocando un bucle como en ProvocaCiclo_DevuelveTrue_SiSePoneElHijoComoPadre.
    /// </summary>
    [Fact]
    public void ProvocaCiclo_DevuelveTrue_SiSePoneElNietoComoPadre()
    {
        var antecesores = new Dictionary<int, int?> { [1] = null, [2] = 1, [3] = 2 };

        var result = Cadena.ProvocaCiclo(1, 3, antecesores);

        Assert.True(result);
    }

    /// <summary>
    /// Comprueba que dos habitos independientes de false.
    /// </summary>
    [Fact]
    public void ProvocaCiclo_DevuelveFalse_SiDosHabitosIndependientes()
    {
        var antecesores = new Dictionary<int, int?> { [1] = null, [2] = null };

        var result = Cadena.ProvocaCiclo(1, 2, antecesores);

        Assert.False(result);
    }

    /// <summary>
    /// Comprueba que de false si se introduce una secuencia correcta de habitos.
    /// </summary>
    [Fact]
    public void ProvocaCiclo_DevuelveFalse_SiSePoneSecuenciaCorrecta()
    {
        var antecesores = new Dictionary<int, int?> { [1] = null, [2] = 1, [3] = 2 };

        var result = Cadena.ProvocaCiclo(3, 1, antecesores);

        Assert.False(result);
    }

    /// <summary>
    /// Test que comprueba que un habito independiente, siga devolviendo false a pesar de recorrer una cadena larga.
    /// </summary>
    [Fact]
    public void ProvocaCiclo_DevuelveFalse_SiSePoneSecuenciaLargaHabitoIndependiente()
    {
        var antecesores = new Dictionary<int, int?> { [1] = null, [2] = 1, [3] = 2, [4] = null };

        var result = Cadena.ProvocaCiclo(4, 3, antecesores);

        Assert.False(result);
    }

    /// <summary>
    /// Comprueba que en el caso de que hubiese un fallo en la BD, sigue detectando ciclos.
    /// </summary>
    [Fact]
    public void ProvocaCiclo_DevuelveTrue_ComprobacionSeguridad()
    {
        var antecesores = new Dictionary<int, int?> { [1] = 2, [2] = 1, [3] = null };

        var result = Cadena.ProvocaCiclo(3, 1, antecesores);

        Assert.True(result);
    }

    /// <summary>
    /// Comprueba que devuelve los hijos de un habito en cadena.
    /// </summary>
    [Fact]
    public void HabitosPosteriores_Devuelve2y3_CompruebaQueDevuelveLaListadeHabitosPosterioresEnCadena()
    {
        var habitos = new Dictionary<int, int?> { [1] = null, [2] = 1, [3] = 2 };

        var result = Cadena.HabitosPosteriores(1, habitos);

        Assert.Equal(new[] { 2, 3 }, result.Order());
    }

    /// <summary>
    /// Comprueba que devuelve los hijos de un habito en dos ramas distintas.
    /// </summary>
    [Fact]
    public void HabitosPosteriores_Devuelve2y3_CompruebaQueDevuelveLaListadeHabitosPosterioresDosHijos()
    {
        var habitos = new Dictionary<int, int?> { [1] = null, [2] = 1, [3] = 1 };

        var result = Cadena.HabitosPosteriores(1, habitos);

        Assert.Equal(new[] { 2, 3 }, result.Order());
    }

    /// <summary>
    /// Comprueba que devuelve una lista vacia si un habito no tiene hijos.
    /// </summary>
    [Fact]
    public void HabitosPosteriores_DevuelveListaVacia_CompruebaQueDevuelveLaListaVaciaDeUnHabitoSinHijos()
    {
        var habitos = new Dictionary<int, int?> { [1] = null };

        List<int> result = Cadena.HabitosPosteriores(1, habitos);

        Assert.Empty(result);
    }

    /// <summary>
    /// Comprueba que devuelve solamente el habito posterior.
    /// </summary>
    [Fact]
    public void HabitosPosteriores_Devuelve3_CompruebaQueSoloDevuelvaElPosterior()
    {
        var habitos = new Dictionary<int, int?> { [1] = null, [2] = 1, [3] = 2, [4] = null };

        List<int> result = Cadena.HabitosPosteriores(2, habitos);

        Assert.Equal(new[] { 3 }, result.Order());
    }
}