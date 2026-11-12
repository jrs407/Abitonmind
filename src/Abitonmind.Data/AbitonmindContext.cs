using Abitonmind.Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Abitonmind.Data;

/// <summary>
/// Clase que hereda de IdentityDbContext para la creación de la estructura de la base de datos.
/// </summary>
public class AbitonmindContext : IdentityDbContext<Usuario>
{
    /// <summary>
    /// Constructor de la base de datos.
    /// </summary>
    /// <param name="options"></param>
    public AbitonmindContext(DbContextOptions<AbitonmindContext> options) : base(options) { }

    /// <summary>
    /// Inicialización de la tabla habitos.
    /// </summary>
    public DbSet<Habito> Habitos => Set<Habito>();

    /// <summary>
    ///  Inicialización de la tabla Registro habitos.
    /// </summary>
    public DbSet<RegistroHabito> RegistroHabitos => Set<RegistroHabito>();

    /// <summary>
    /// Inicialización de la tabla Pasos Minimos.
    /// </summary>
    public DbSet<PasoMinimo> PasosMinimos => Set<PasoMinimo>();

    /// <summary>
    /// Inicialización de la tabla frases.
    /// </summary>
    public DbSet<Frase> Frases => Set<Frase>();

    /// <summary>
    /// Inicialización de la tabla DiasCerrados.
    /// </summary>
    public DbSet<DiaCerrado> DiasCerrados => Set<DiaCerrado>();

    /// <summary>
    /// Creación del modelo de base de datos.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define caracteristicas de Habito en los valores y relaciones entre tablas.
        modelBuilder.Entity<Habito>(habito =>
        {
            // Se limita el tamaño del nombre a 100 caracteres
            habito.Property(h => h.Nombre).HasMaxLength(100);

            // Relación entre usuario y habitos.
            habito.HasOne<Usuario>().WithMany().HasForeignKey(h => h.UsuarioId).OnDelete(DeleteBehavior.Cascade);

            // Relación entre habito y habito.
            habito.HasOne<Habito>().WithMany().HasForeignKey(h => h.HabitoAntecesorId).OnDelete(DeleteBehavior.SetNull);

            // Limite de caracteres en la preparación de un hábito.
            habito.Property(h => h.Preparacion).HasMaxLength(100);

            // Limite de caracteres en la descripción de un hábito.
            habito.Property(h => h.Descripcion).HasMaxLength(255);
        });

        // Define caracteristicas de RegistroHabito en los valores y relaciones entre tablas.
        modelBuilder.Entity<RegistroHabito>(registro =>
        {
            // Relación entre Habito y RegistroHabito.
            registro.HasOne<Habito>().WithMany().HasForeignKey(r => r.HabitoId).OnDelete(DeleteBehavior.Cascade);

            // Indice para facilitar y optimizar consultas.
            registro.HasIndex(r => new { r.Fecha, r.HabitoId }).IsUnique();

        });

        // Define caracteristicas de Usuario en los valores y relaciones entre tablas.
        modelBuilder.Entity<Usuario>(usuario =>
        {
            // Limite de caracteres en Identidad.
            usuario.Property(u => u.Identidad).HasMaxLength(255);

            // Limite de caracteres en Zona horaria
            usuario.Property(u => u.ZonaHoraria).HasMaxLength(40);
        });

        // Define caracteristicas de PasoMinimo en los valores y relaciones entre tablas.
        modelBuilder.Entity<PasoMinimo>(pasoMinimo =>
        {
            // Limite de caracteres en nombre.
            pasoMinimo.Property(ps => ps.Nombre).HasMaxLength(100);

            // Limite de caracteres en preparación.
            pasoMinimo.Property(ps => ps.Preparacion).HasMaxLength(100);

            // Limite de caracteres en descripción.
            pasoMinimo.Property(ps => ps.Descripcion).HasMaxLength(255);

            // Relación entre un Usuario y un PasoMinimo.
            pasoMinimo.HasOne<Usuario>().WithMany().HasForeignKey(ps => ps.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        });

        // Define caracteristicas de DiaCerrado en los valores y relaciones entre tablas.
        modelBuilder.Entity<DiaCerrado>(diaCerrado =>
        {
            // Relación entre Usuario y DiaCerrado.
            diaCerrado.HasOne<Usuario>().WithMany().HasForeignKey(dc => dc.UsuarioId).OnDelete(DeleteBehavior.Cascade);

            // Indice para facilitar y optimizar consultas.
            diaCerrado.HasIndex(dc => new {dc.UsuarioId, dc.Fecha}).IsUnique();
        });

        // Define algunas frases por defecto al ejecutar la aplicación.
        modelBuilder.Entity<Frase>().HasData(
            new Frase { Id = 1, Texto = "Hoy no busco la perfección, solo mejorar un poco respecto a ayer." },
            new Frase { Id = 2, Texto = "Cada hábito que cumples es un voto por la persona que quieres ser." },
            new Frase { Id = 3, Texto = "Empieza tan pequeño que no puedas decir que no." },
            new Frase { Id = 4, Texto = "Si ayer fallaste, hoy toca volver. Nunca falles dos veces seguidas." },
            new Frase { Id = 5, Texto = "Lo que repites cada día pesa más que lo que haces de vez en cuando." },
            new Frase { Id = 6, Texto = "No necesitas más motivación, necesitas un primer paso más pequeño." },
            new Frase { Id = 7, Texto = "Pon a la vista lo que quieres hacer y lejos lo que quieres evitar." },
            new Frase { Id = 8, Texto = "Engancha lo nuevo a algo que ya haces sin pensar." },
            new Frase { Id = 9, Texto = "Las metas marcan el destino; los hábitos son el camino." },
            new Frase { Id = 10, Texto = "Un 1 % cada día no se nota hoy, pero se nota en un año." },
            new Frase { Id = 11, Texto = "Hoy cuenta, aunque solo hagas un poco." },
            new Frase { Id = 12, Texto = "No se trata de lo que quieres conseguir, sino de quién quieres llegar a ser." }
        );
    }

}