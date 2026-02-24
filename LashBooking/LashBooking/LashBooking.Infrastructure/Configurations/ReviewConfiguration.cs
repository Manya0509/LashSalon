using LashBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Infrastructure.Configurations
{
    // Конфигурация EF Core для сущности Review
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        // Настройка маппинга таблицы Review в БД
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            // Установка первичного ключа
            builder.HasKey(r => r.Id);

            // Настройка поля Text: обязательное поле, макс. длина 1000 символов
            builder.Property(r => r.Text)
                .IsRequired()
                .HasMaxLength(1000);

            // Настройка поля Rating: обязательное поле
            builder.Property(r => r.Rating)
                .IsRequired();

            // Настройка связи с Client (один ко многим)
            builder.HasOne(r => r.Client)                 // У отзыва один клиент
                .WithMany(c => c.Reviews)                 // У клиента много отзывов
                .HasForeignKey(r => r.ClientId)           // Внешний ключ
                .OnDelete(DeleteBehavior.Restrict);       // Запрет каскадного удаления
        }
    }
}