using dgcp.domain.Models;
using Microsoft.EntityFrameworkCore;

namespace dgcp.infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { this.Database.SetCommandTimeout(3000); }

        public DbSet<Tender> Tenders { get; set; }
        public DbSet<TenderFinal> TendersFinal { get; set; }
        public DbSet<VisitedUrl> VisitedUrls { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Keyword> Keywords { get; set; }
        public DbSet<Rubro> Rubros { get; set; }
        public DbSet<EmpresaKeyword> EmpresaKeywords { get; set; }
        public DbSet<EmpresaRubro> EmpresaRubros { get; set; }
        public DbSet<CurrentUrl> CurrentUrls { get; set; }
        public DbSet<FailedUrl> FailedUrls { get; set; }
        public DbSet<TenderItem> TenderItem { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EmpresaKeyword>()
            .HasKey(ek => new { ek.EmpresaID, ek.KeywordID });

            modelBuilder.Entity<EmpresaKeyword>()
                .HasOne(ek => ek.Empresa)
                .WithMany(e => e.EmpresaKeywords)
                .HasForeignKey(ek => ek.EmpresaID);

            modelBuilder.Entity<EmpresaKeyword>()
                .HasOne(ek => ek.Keyword)
                .WithMany(k => k.EmpresaKeywords)
                .HasForeignKey(ek => ek.KeywordID);

            modelBuilder.Entity<EmpresaRubro>()
                .HasKey(er => new { er.EmpresaID, er.RubroID });

            modelBuilder.Entity<EmpresaRubro>()
                .HasOne(er => er.Empresa)
                .WithMany(e => e.EmpresaRubros)
                .HasForeignKey(er => er.EmpresaID);

            modelBuilder.Entity<EmpresaRubro>()
                .HasOne(er => er.Rubro)
                .WithMany(r => r.EmpresaRubros)
                .HasForeignKey(er => er.RubroID);

            modelBuilder.Entity<Tender>(entity =>
            {
                entity.HasKey(e => e.ReleaseOcid);
            });

            modelBuilder.Entity<TenderFinal>(entity =>
            {
                entity.HasKey(e => e.ReleaseOcid);

            });
        }
    }
}
