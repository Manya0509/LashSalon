using LashBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LashBooking.Infrastructure.Configurations;
// Конфигурация для сущности Service(Услуга).
public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder) // builder - объект, через который настраиваются поля, ключи и индексы
    {
        builder.HasKey(s => s.Id); // Id - Первичный ключ

        builder.Property(s => s.Name) // Name - обязательное поле, max - 100 символов
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Price); // Price - обязательное поле, обычно decimal

        builder.HasIndex(s => s.IsActive); // активна ли услуга
    }
}
