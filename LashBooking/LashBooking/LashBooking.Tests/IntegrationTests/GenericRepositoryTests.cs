using LashBooking.Domain.Entities;
using LashBooking.Infrastructure.Data;
using LashBooking.Infrastructure.Repositories;
using LashBooking.Reports;
using LashBooking.Web.MVC.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LashBooking.Tests.IntegrationTests
{
    public class GenericRepositoryTests
    {
        private ApplicationDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_AddsEntityToDatabase() // Создание клиента, проверка, что он появился в БД
        {
            var db = CreateDb();

            var clientRepo = new GenericRepository<Client>(db);

            await clientRepo.AddAsync(new Client
            {
                Id = 1,
                Phone = "+79001234567",
                Name = "Иван"
            });
            await clientRepo.SaveChangesAsync();

            Assert.Equal(1, db.Clients.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectEntity() // Добавление клиента - поиск по Id
        {
            var db = CreateDb();

            var clientRepo = new GenericRepository<Client>(db);

            await clientRepo.AddAsync(new Client
            {
                Id = 1,
                Phone = "+79001234567",
                Name = "Иван"
            });
            await clientRepo.SaveChangesAsync();

            var client = await clientRepo.GetByIdAsync(1);

            Assert.Equal("Иван", client.Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities() // Добавление трёх клиентов - возвращает всех
        {
            var db = CreateDb();

            var clientRepo = new GenericRepository<Client>(db);

            await clientRepo.AddAsync(new Client
            {
                Id = 1,
                Phone = "+79001234567",
                Name = "Иван"
            });
            await clientRepo.SaveChangesAsync();

            await clientRepo.AddAsync(new Client
            {
                Id = 2,
                Phone = "+79001234568",
                Name = "Ирина"
            });
            await clientRepo.SaveChangesAsync();

            await clientRepo.AddAsync(new Client
            {
                Id = 3,
                Phone = "+79001234569",
                Name = "Кристина"
            });
            await clientRepo.SaveChangesAsync();

            var clients = await clientRepo.GetAllAsync();

            Assert.Equal(3, clients.Count());
        }

        [Fact]
        public async Task FindAsync_ReturnsFilteredEntities() // Добавление двух клиентов — фильтр возвращает одного
        {
            var db = CreateDb();

            var clientRepo = new GenericRepository<Client>(db);

            await clientRepo.AddAsync(new Client
            {
                Id = 1,
                Phone = "+79001234567",
                Name = "Иван"
            });
            await clientRepo.SaveChangesAsync();

            await clientRepo.AddAsync(new Client
            {
                Id = 2,
                Phone = "+79001234568",
                Name = "Ирина"
            });
            await clientRepo.SaveChangesAsync();

            var clients = await clientRepo.FindAsync(c => c.Name == "Иван");

            Assert.Equal(1, clients.Count());
        }

        [Fact]
        public async Task Update_UpdatesEntityInDatabase() // Добавили → изменили имя → в базе новое имя
        {
            var db = CreateDb();

            var clientRepo = new GenericRepository<Client>(db);
                        
            await clientRepo.AddAsync(new Client
            {
                Id = 1,
                Phone = "+79001234567",
                Name = "Иван"
            });
            await clientRepo.SaveChangesAsync();

            var client = await clientRepo.GetByIdAsync(1);
            client.Name = "Пётр";
            clientRepo.Update(client);
            await clientRepo.SaveChangesAsync();

            var updated = await clientRepo.GetByIdAsync(1);

            Assert.Equal("Пётр", updated.Name);
        }

        [Fact]
        public async Task Delete_RemovesEntityFromDatabase() // Добавили → удалили → в базе пусто
        {
            var db = CreateDb();

            var clientRepo = new GenericRepository<Client>(db);

            await clientRepo.AddAsync(new Client
            {
                Id = 1,
                Phone = "+79001234567",
                Name = "Иван"
            });
            await clientRepo.SaveChangesAsync();

            var client = await clientRepo.GetByIdAsync(1);
            clientRepo.Delete(client);
            await clientRepo.SaveChangesAsync();

            Assert.Equal(0, db.Clients.Count());
        }
    }
}
