using LashBooking.Domain.Entities;            
using Microsoft.EntityFrameworkCore;                       
using Microsoft.EntityFrameworkCore.Metadata.Builders;   

namespace LashBooking.Infrastructure.Configurations
{
    public class AboutInfoConfiguration : IEntityTypeConfiguration<AboutInfo>
    {
        public void Configure(EntityTypeBuilder<AboutInfo> builder)
        {
            builder.HasKey(a => a.Id);           // Указываем, что Id — первичный ключ таблицы

            builder.Property(a => a.MasterName)
                .IsRequired()                               
                .HasMaxLength(100);                          // Максимум 100 символов

            builder.Property(a => a.Role)
                .IsRequired()
                .HasMaxLength(100);                          // "Мастер Lash-стилист"

            builder.Property(a => a.Experience)
                .IsRequired()
                .HasMaxLength(100);                          // "Опыт работы 5+ лет" 

            builder.Property(a => a.Quote)
                .IsRequired()
                .HasMaxLength(500);                          // Цитата 

            builder.Property(a => a.AboutText)
                .IsRequired()
                .HasMaxLength(2000);                         // Текст "Обо мне" 

            builder.Property(a => a.EducationText)
                .IsRequired()
                .HasMaxLength(2000);                         // Текст "Образование" 

            builder.Property(a => a.Address)
                .IsRequired()
                .HasMaxLength(200);                          // Адрес 

            builder.Property(a => a.WorkingHours)
                .IsRequired()
                .HasMaxLength(200);                          // "Ежедневно с 9:00 до 18:00"

            builder.Property(a => a.Phone)
                .IsRequired()
                .HasMaxLength(50);                           // Телефон — "+7 (999) 410-38-01"

            builder.Property(a => a.WhatsAppLink)
                .HasMaxLength(255);                          // Ссылка на WhatsApp 

            builder.Property(a => a.TelegramLink)
                .HasMaxLength(255);                          // Ссылка на Telegram 

            builder.Property(a => a.PhotoFileName)
                .HasMaxLength(255);                          // Имя файла фото

            builder.Property(a => a.StudioName)
                .HasMaxLength(200);            // Название студии

            builder.Property(a => a.HeroPhotoFileName)
                .HasMaxLength(255);            // Имя файла фото-фона 
        }
    }
}
