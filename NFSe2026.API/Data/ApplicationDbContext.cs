using Microsoft.EntityFrameworkCore;
using NFSe2026.API.Models;

namespace NFSe2026.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Tomador> Tomadores { get; set; }
    public DbSet<NotaFiscal> NotasFiscais { get; set; }
    public DbSet<ItemServico> ItensServico { get; set; }
    public DbSet<ConfiguracaoAPI> ConfiguracoesAPI { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações de Empresa
        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasIndex(e => e.CNPJ).IsUnique();
            
            // Configura ClientId e ClientSecret como opcionais
            entity.Property(e => e.ClientId)
                .HasMaxLength(500)
                .IsRequired(false);
            entity.Property(e => e.ClientSecret)
                .HasMaxLength(500)
                .IsRequired(false);
            
            // Configura ProvedorNFSe como opcional para evitar erro se a coluna não existir ou tiver NULL
            // Se a coluna não existir, execute a migration ou o script SQL
            // Não pode usar HasDefaultValue com propriedade nullable - o valor padrão será tratado no código
            entity.Property(e => e.ProvedorNFSe)
                .IsRequired(false); // Opcional temporariamente até garantir que a coluna existe e tem valores
        });

        // Configurações de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.EmpresaId);

            entity.HasOne(e => e.Empresa)
                .WithMany(emp => emp.Usuarios)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuração para colunas que podem não existir ainda (para evitar erro durante desenvolvimento)
            entity.Property(e => e.CodigoValidacao)
                .HasMaxLength(10)
                .IsRequired(false);

            entity.Property(e => e.DataExpiracaoCodigo)
                .IsRequired(false);

            entity.Property(e => e.EmailValidado)
                .HasDefaultValue(false);
        });

        // Configurações de Tomador
        modelBuilder.Entity<Tomador>(entity =>
        {
            entity.HasIndex(e => e.CPFCNPJ);
            entity.HasIndex(e => e.EmpresaId);

            entity.HasOne(e => e.Empresa)
                .WithMany(emp => emp.Tomadores)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurações de NotaFiscal
        modelBuilder.Entity<NotaFiscal>(entity =>
        {
            entity.HasIndex(e => e.Numero);
            entity.HasIndex(e => e.EmpresaId);
            entity.HasIndex(e => e.TomadorId);
            entity.HasIndex(e => e.DataEmissao);
            entity.HasIndex(e => e.Situacao);

            entity.HasOne(e => e.Empresa)
                .WithMany(emp => emp.NotasFiscais)
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Tomador)
                .WithMany(t => t.NotasFiscais)
                .HasForeignKey(e => e.TomadorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurações de ItemServico
        modelBuilder.Entity<ItemServico>(entity =>
        {
            entity.HasOne(e => e.NotaFiscal)
                .WithMany(n => n.ItensServico)
                .HasForeignKey(e => e.NotaFiscalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações de ConfiguracaoAPI
        modelBuilder.Entity<ConfiguracaoAPI>(entity =>
        {
            entity.HasIndex(e => e.Ambiente).IsUnique();
        });
    }
}

