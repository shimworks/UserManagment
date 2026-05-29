using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        // OwnsOne mapeia o Value Object Email como colunas na mesma tabela.
        // O indice unico e definido DENTRO do OwnsOne — tentar defini-lo
        // fora via builder.HasIndex(u => u.Email) causa o erro
        // "property or navigation already exists" pois o EF trata
        // owned types de forma diferente de propriedades escalares.
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(255);

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
        });

        builder.OwnsOne(u => u.Password, pwd =>
        {
            pwd.Property(p => p.Hash)
                .HasColumnName("PasswordHash")
                .IsRequired();
        });

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();
    }
}