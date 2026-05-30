using Bloomy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloomyBE.Data.Configurations
{
    public class ShopConfiguration : IEntityTypeConfiguration<Shop>
    {
        public void Configure(EntityTypeBuilder<Shop> builder)
        {
            builder.ToTable("Shops");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasColumnType("nvarchar(max)");
            builder.Property(x => x.LogoUrl).HasMaxLength(500);
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.PhoneNumber).HasMaxLength(20);

            builder.HasIndex(x => x.OwnerId).IsUnique();

            builder.HasOne(x => x.Owner)
                .WithOne(u => u.OwnedShop)
                .HasForeignKey<Shop>(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
