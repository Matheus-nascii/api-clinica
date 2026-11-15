using ApiClinica.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Data;

// Contexto do Entity Framework: centraliza as tabelas e as regras de mapeamento do SQLite.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Cada DbSet vira uma tabela no SQLite
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CPF único no banco (segunda camada de proteção além da validação no service)
        // O índice único garante no próprio SQLite que dois pacientes não tenham o mesmo CPF.
        modelBuilder.Entity<Paciente>()
            .HasIndex(p => p.Cpf)
            .IsUnique();

        // Relacionamentos 1:N
        modelBuilder.Entity<Consulta>()
            .HasOne(c => c.Paciente)
            .WithMany(p => p.Consultas)
            .HasForeignKey(c => c.PacienteId);

        modelBuilder.Entity<Consulta>()
            .HasOne(c => c.Medico)
            .WithMany(m => m.Consultas)
            .HasForeignKey(c => c.MedicoId);

        // Também impede contas duplicadas caso duas requisições de cadastro ocorram ao mesmo tempo.
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
