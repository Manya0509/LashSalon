using LashBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LashBooking.Infrastructure.Configurations;
// Конфигурация для сущности Client(Клиент).
public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder) // builder - объект, через который настраиваются ключи, обязательные поля, длина строк, индексы, связи
    { 
        builder.HasKey(c => c.Id); // Id - Первичный ключ

        builder.Property(c => c.Name) // Name - обязательное поле, max - 100 символов
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Phone) // Phone - обязательное поле, max - 20 символов
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Email) // Email - Необязательное поле, max - 100 символов
            .HasMaxLength(100);

        builder.Property(c => c.Notes) // Notes - Необязательное поле, max - 500 символов
            .HasMaxLength(500);
    }
}


