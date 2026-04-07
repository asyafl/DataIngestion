using DataIngestion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataIngestion.Infrastructure.Persistence
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.CustomerId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.TransactionDateUtc)
                .IsRequired();

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(x => x.SourceChannel)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.DeduplicationKey)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => x.CustomerId);

            builder.HasIndex(x => x.DeduplicationKey)
                .IsUnique();
        }
    }
}
