using LashBooking.Domain.Entities;
using LashBooking.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LashBooking.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<BlockedSlot> BlockedSlots { get; set; }
    public DbSet<LogApplicationError> LogApplicationErrors { get; set; }  // таблица логов ошибок

    public LogApplicationError Insert(LogApplicationError logApplicationError) // сохраняет запись лога напрямую через контекст без репозитория
    {
        var item = LogApplicationErrors.Add(logApplicationError);
        this.SaveChanges();
        return item.Entity;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
    }
}
