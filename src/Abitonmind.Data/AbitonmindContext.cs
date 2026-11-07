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
    /// Creación del modelo de base de datos.
    /// </summary>
    /// <param name="builder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Habito>(habito =>
        {
            // Se limita el tamaño del nombre a 100 caracteres
            habito.Property(h => h.Nombre).HasMaxLength(100);

            //Relación entre usuario y habitos.
            habito.HasOne<Usuario>().WithMany().HasForeignKey(h => h.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<RegistroHabito>(registro =>
        {
            registro.HasOne<Habito>().WithMany().HasForeignKey(r => r.HabitoId).OnDelete(DeleteBehavior.Cascade);

            registro.HasIndex(r => new { r.Fecha, r.HabitoId} ).IsUnique();
            
        });
    }

}