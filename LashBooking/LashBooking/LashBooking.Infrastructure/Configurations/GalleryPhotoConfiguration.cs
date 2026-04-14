using LashBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LashBooking.Infrastructure.Configurations
{
    public class GalleryPhotoConfiguration : IEntityTypeConfiguration<GalleryPhoto>
    {
        public void Configure(EntityTypeBuilder<GalleryPhoto> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Description)
                .HasMaxLength(500);
        }
    }
}
