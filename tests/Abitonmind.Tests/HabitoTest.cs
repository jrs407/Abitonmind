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
        var habito1 = new Habito { Nombre = "Hacer la cama", Descripcion = "Por la mañana hay que hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves };

        var result = habito1.TocaEl(new DateOnly(2026, 11, 2));

        Assert.True(result);
    }

    /// <summary>
    /// Comprueba si dado un habito que no tiene un dia marcado, devuelve false.
    /// </summary>
    [Fact]
    public void TocaEl_DevuelveFalse_SiElDiaSinMarcar()
    {
        var habito1 = new Habito { Nombre = "Hacer la cama", Descripcion = "Por la mañana hay que hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves };

        var result = habito1.TocaEl(new DateOnly(2026, 11, 3));

        Assert.False(result);
    }

    /// <summary>
    /// Comprueba si dado un habito marcado el domingo, devuelve true.
    /// </summary>
    [Fact]
    public void TocaEl_DevuelveTrue_SiElDiaEsDomingo()
    {
        var habito1 = new Habito { Nombre = "Hacer la cama", Descripcion = "Por la mañana hay que hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo };

        var result = habito1.TocaEl(new DateOnly(2026, 11, 8));

        Assert.True(result);
    }

    /// <summary>
    /// Comprueba que si se da de baja un hábito, no siga siendo válido después de la baja.
    /// </summary>
    [Fact]
    public void TocaEl_DevuelveFalse_SiLaFechaEsPosteriorALaBaja()
    {
        var habito1 = new Habito { Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo, FechaArchivado = new DateOnly(2026, 11, 16) };

        var result = habito1.TocaEl(new DateOnly(2026, 11, 23));

        Assert.False(result);
    }

    /// <summary>
    /// Comprueba que si se da de baja un hábito, siga siendo válido antes de la baja.
    /// </summary>
    [Fact]
    public void TocaEl_DevuelveTrue_SiLaFechaEsAnteriorALaBaja()
    {
        var habito1 = new Habito { Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo, FechaArchivado = new DateOnly(2026, 11, 16) };

        var result = habito1.TocaEl(new DateOnly(2026, 11, 9));

        Assert.True(result);
    }

    /// <summary>
    /// Comprueba que si se da de baja un hábito, siga siendo válido el mismo día de la baja.
    /// </summary>
    [Fact]
    public void TocaEl_DevuelveTrue_SiLaFechaEsMismoDiaALaBaja()
    {
        var habito1 = new Habito { Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo, FechaArchivado = new DateOnly(2026, 11, 16) };

        var result = habito1.TocaEl(new DateOnly(2026, 11, 16));

        Assert.True(result);
    }

    /// <summary>
    /// Comprueba que funcione bien que un habito vaya despues de otro si coinciden en días.
    /// </summary>
    [Fact]
    public void ErrorAlIrDespuesDe_DevuelveNull_SiHijoSinHoraMismoDiaAnterior()
    {
        var habito1 = new Habito { Id = 1, Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo };
        var habito2 = new Habito { Id = 2, Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo, HabitoAntecesorId = habito1.Id };
        var result = habito2.ErrorAlIrDespuesDe(habito1);

        Assert.Null(result);
    }

    /// <summary>
    /// Comprueba que un hijo si tiene solamente algun día del padre siga funcionando.
    /// </summary>
    [Fact]
    public void ErrorAlIrDespuesDe_DevuelveNull_SiHijoSinHoraContenidoDiaAnterior()
    {
        var habito1 = new Habito { Id = 1, Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo };
        var habito2 = new Habito { Id = 2, Nombre = "Hacer la cama", Dias = DiasSemana.Domingo, HabitoAntecesorId = habito1.Id };
        var result = habito2.ErrorAlIrDespuesDe(habito1);

        Assert.Null(result);
    }

    /// <summary>
    /// Comprueba que un hijo no pueda tener tener padre y hora al mismo tiempo.
    /// </summary>
    [Fact]
    public void ErrorAlIrDespuesDe_DevuelveString_SiHijoConHora()
    {
        var habito1 = new Habito { Id = 1, Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo };
        var habito2 = new Habito { Id = 2, Nombre = "Hacer la cama", Dias = DiasSemana.Domingo, HabitoAntecesorId = habito1.Id, Hora = new TimeOnly(2, 0) };
        var result = habito2.ErrorAlIrDespuesDe(habito1);

        Assert.NotNull(result);
    }

    /// <summary>
    /// Comprueba que de error si un hijo tiene un día que no tenga el padre.
    /// </summary>
    [Fact]
    public void ErrorAlIrDespuesDe_DevuelveString_SiHijoSinHoraTieneDiaDistintoPadre()
    {
        var habito1 = new Habito { Id = 1, Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo };
        var habito2 = new Habito { Id = 2, Nombre = "Hacer la cama", Dias = DiasSemana.Martes | DiasSemana.Domingo, HabitoAntecesorId = habito1.Id };
        var result = habito2.ErrorAlIrDespuesDe(habito1);

        Assert.NotNull(result);
    }

    /// <summary>
    /// Comprueba que un habito hijo no pueda ser padre de si mismo.
    /// </summary>
    [Fact]
    public void ErrorAlIrDespuesDe_DevuelveString_SiHijoIgualPadre()
    {
        var habito1 = new Habito { Id = 1, Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo};
        habito1.HabitoAntecesorId = habito1.Id;

        var result = habito1.ErrorAlIrDespuesDe(habito1);

        Assert.NotNull(result);
    }

    /// <summary>
    /// Comprueba que de error si el padre está archivado.
    /// </summary>
    [Fact]
    public void ErrorAlIrDespuesDe_DevuelveString_SiPadreArchivado()
    {
        var habito1 = new Habito { Id = 1, Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Jueves | DiasSemana.Domingo, FechaArchivado = new DateOnly(2026, 10, 16) };
        var habito2 = new Habito { Id = 2, Nombre = "Hacer la cama", Dias = DiasSemana.Lunes | DiasSemana.Domingo, HabitoAntecesorId = habito1.Id };
        var result = habito2.ErrorAlIrDespuesDe(habito1);

        Assert.NotNull(result);
    }
}
