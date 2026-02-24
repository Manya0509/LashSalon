using LashBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LashBooking.Infrastructure.Configurations;

// Конфигурация для сущности Appointment(запись на услугу).
public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment> // Позволяет задать правила отображения сущности в таблицу БД
{
    public void Configure(EntityTypeBuilder<Appointment> builder) // builder - объект, через который настраиваются поля, ключи, связи и индексы
    {
        builder.HasKey(a => a.Id); // Id - Первичный ключ

        builder.Property(a => a.Status) // Status - будет храниться в БД как int, а не строка
            .HasConversion<int>();

        builder.HasIndex(a => a.DateStart); // DateStart - Создается индекс. Это ускорит поиск записей по дате начала
         
        builder.HasOne(a => a.Client) // У Appointment есть один Client
            .WithMany(c => c.Appointments) // У Client может быть много Appointments
            .HasForeignKey(a => a.ClientId) // В таблице Appointment внешний ключ — ClientId
            .OnDelete(DeleteBehavior.Restrict); // нельзя удалить клиента, если у него есть записи

        builder.HasOne(a => a.Service) // Appointment относится к одному Service
            .WithMany(s => s.Appointments) // У Service много Appointments
            .HasForeignKey(a => a.ServiceId) // Внешний ключ — ServiceId
            .OnDelete(DeleteBehavior.Restrict); // Нельзя удалить Service, если есть записи
    }
}
