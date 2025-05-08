using CoreManagerSP.API.CoreManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Configurations
{
    public class CoreManagerDbContext : DbContext
    {
        public CoreManagerDbContext(DbContextOptions<CoreManagerDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Admin> Administradores { get; set; }
        public DbSet<SolicitudPrestamo> Solicitudes { get; set; }
        public DbSet<TipoPrestamo> TiposPrestamo { get; set; }
        public DbSet<EntidadFinanciera> EntidadesFinancieras { get; set; }
        public DbSet<EntidadTipoPrestamo> EntidadesTipoPrestamo { get; set; }
        public DbSet<AnalisisResultado> AnalisisResultados { get; set; }
        public DbSet<MejoraSugerida> MejorasSugeridas { get; set; }
        public DbSet<HistorialMejoras> HistorialMejoras { get; set; }
        public DbSet<LogSistema> LogsSistema { get; set; }
        public DbSet<TokenUsuario> TokenUsuarios { get; set; }
        public DbSet<TokenAdmin> TokenAdmins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Precisión para decimales
            modelBuilder.Entity<Usuario>().Property(u => u.Ingreso).HasPrecision(18, 2);
            modelBuilder.Entity<Usuario>().Property(u => u.DeudasVigentes).HasPrecision(18, 2);
            modelBuilder.Entity<Usuario>().Property(u => u.CuotasMensualesComprometidas).HasPrecision(18, 2);

            // Relaciones
            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.Solicitudes)
                .WithOne(s => s.Usuario)
                .HasForeignKey(s => s.UsuarioId);

            modelBuilder.Entity<TipoPrestamo>()
                .HasMany(t => t.Solicitudes)
                .WithOne(s => s.TipoPrestamo)
                .HasForeignKey(s => s.TipoPrestamoId);

            modelBuilder.Entity<EntidadTipoPrestamo>()
                .HasKey(et => new { et.EntidadFinancieraId, et.TipoPrestamoId });

            modelBuilder.Entity<EntidadTipoPrestamo>()
                .HasOne(et => et.EntidadFinanciera)
                .WithMany(e => e.EntidadesTipoPrestamo)
                .HasForeignKey(et => et.EntidadFinancieraId);

            modelBuilder.Entity<EntidadTipoPrestamo>()
                .HasOne(et => et.TipoPrestamo)
                .WithMany(t => t.EntidadesTipoPrestamo)
                .HasForeignKey(et => et.TipoPrestamoId);

            modelBuilder.Entity<AnalisisResultado>()
                .HasOne(a => a.SolicitudPrestamo)
                .WithMany(s => s.AnalisisResultados)
                .HasForeignKey(a => a.SolicitudPrestamoId);

            modelBuilder.Entity<AnalisisResultado>()
                .HasOne(a => a.EntidadFinanciera)
                .WithMany(e => e.AnalisisResultados)
                .HasForeignKey(a => a.EntidadFinancieraId);

            modelBuilder.Entity<MejoraSugerida>()
                .HasOne(m => m.AnalisisResultado)
                .WithMany(a => a.MejorasSugeridas)
                .HasForeignKey(m => m.AnalisisResultadoId);

            modelBuilder.Entity<HistorialMejoras>()
                .HasOne(h => h.SolicitudPrestamo)
                .WithMany()
                .HasForeignKey(h => h.SolicitudPrestamoId);

            modelBuilder.Entity<LogSistema>()
                .HasOne(l => l.Usuario)
                .WithMany()
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TokenAdmin>(entity =>
            {
                entity.ToTable("TokenAdmins");
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Token).IsRequired();

                entity.HasOne(t => t.Admin)
                      .WithMany()
                      .HasForeignKey(t => t.AdminId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });



        }
    }
}
