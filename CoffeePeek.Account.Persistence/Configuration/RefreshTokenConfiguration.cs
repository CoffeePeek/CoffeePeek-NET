using CoffeePeek.Account.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoffeePeek.Account.Persistence.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Client-generated PK: domain sets Id = Guid.NewGuid() in the ctor.
        // Without ValueGeneratedNever, EF treats a non-empty Guid as an existing row
        // and issues UPDATE (0 rows) → DbUpdateConcurrencyException on login/AddSession.
        builder.Property(rt => rt.Id).ValueGeneratedNever();
        builder.HasIndex(rt => rt.Token);
    }
}
