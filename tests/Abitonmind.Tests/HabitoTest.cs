using Abitonmind.Core;

namespace Abitonmind.Tests;

public class HabitoTest
{
    /// <summary>
    /// Comprueba si dado un habito en un día concreto, devuelve true el método TocaEl.
    /// </summary>
    [Fact]
    public void TocaEl_DevuelveTrue_SiElDiaEstaMarcado()
    {
        var habito1 = new Habito {Nombre = "Hacer la cama", Descripcion = "Por la mañana hay que hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves};

        var result = habito1.TocaEl(new DateOnly(2026, 11, 2));

        Assert.True(result);
    }

    /// <summary>
    /// Comprueba si dado un habito que no tiene un dia marcado, devuelve false.
    /// </summary>
    [Fact]
    public void TocaEl_DevuelveFalse_SiElDiaSinMarcar()
    {
        var habito1 = new Habito {Nombre = "Hacer la cama", Descripcion = "Por la mañana hay que hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves};

        var result = habito1.TocaEl(new DateOnly(2026, 11, 3));

        Assert.False(result);
    }

    /// <summary>
    /// Comprueba si dado un habito marcado el domingo, devuelve true.
    /// </summary>
    [Fact]
    public void TocaEl_DevuelveTrue_SiElDiaEsDomingo()
    {
        var habito1 = new Habito {Nombre = "Hacer la cama", Descripcion = "Por la mañana hay que hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo};

        var result = habito1.TocaEl(new DateOnly(2026, 11, 8));

        Assert.True(result);
    }
}
