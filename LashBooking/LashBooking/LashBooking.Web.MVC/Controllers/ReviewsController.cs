using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Entities;

namespace LashBooking.Web.MVC.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IRepository<Review> _reviewRepo;
        private readonly IRepository<Client> _clientRepo;
        private readonly IRepository<Appointment> _appointmentRepo;

        public ReviewsController(
            IRepository<Review> reviewRepo,
            IRepository<Client> clientRepo,
            IRepository<Appointment> appointmentRepo)
        {
            _reviewRepo = reviewRepo;
            _clientRepo = clientRepo;
            _appointmentRepo = appointmentRepo;
        }

        // GET: /Reviews
        public async Task<IActionResult> Index()
        {
            // Загружаем одобренные отзывы
            var allReviews = (await _reviewRepo.FindAsync(r => r.IsApproved)).ToList();

            // Подгружаем клиентов к каждому отзыву
            foreach (var review in allReviews)
            {
                if (review.ClientId > 0)
                {
                    var clients = await _clientRepo.FindAsync(c => c.Id == review.ClientId);
                    review.Client = clients.FirstOrDefault();
                }
            }

            var reviews = allReviews.OrderByDescending(r => r.CreatedAt).ToList();

            double averageRating = reviews.Count == 0
                ? 0
                : Math.Round(reviews.Average(x => x.Rating), 1);

            // Читаем данные авторизованного клиента из сессии
            var clientId = HttpContext.Session.GetInt32("ClientId");
            var isAuthenticated = clientId.HasValue;
            string phone = "";

            if (isAuthenticated)
            {
                var clients = await _clientRepo.FindAsync(c => c.Id == clientId!.Value);
                var client = clients.FirstOrDefault();
                if (client != null)
                    phone = client.Phone ?? "";
            }

            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = averageRating;
            ViewBag.IsAuthenticated = isAuthenticated;
            ViewBag.Phone = phone;
            ViewBag.Message = TempData["Message"];
            ViewBag.IsSuccess = TempData["IsSuccess"];

            return View();
        }

        // POST: /Reviews/Add
        [HttpPost]
        public async Task<IActionResult> Add(string phone, string text, int rating)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(phone))
            {
                TempData["Message"] = "Введите номер телефона";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Message"] = "Введите текст отзыва";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            if (text.Length < 10)
            {
                TempData["Message"] = "Отзыв должен содержать не менее 10 символов";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            // Поиск клиента
            Client? client = null;

            var clientId = HttpContext.Session.GetInt32("ClientId");
            if (clientId.HasValue)
            {
                var clients = await _clientRepo.FindAsync(c => c.Id == clientId.Value);
                client = clients.FirstOrDefault();
            }

            if (client == null)
            {
                var clients = await _clientRepo.FindAsync(c => c.Phone == phone);
                client = clients.FirstOrDefault();
            }

            if (client == null)
            {
                TempData["Message"] = "Клиент с таким телефоном не найден. Сначала запишитесь на услугу.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            // Проверка завершённого посещения
            var hasCompleted = (await _appointmentRepo.FindAsync(a =>
                a.ClientId == client.Id &&
                a.Status == AppointmentStatus.Completed)).Any();

            if (!hasCompleted)
            {
                TempData["Message"] = "Отзыв можно оставить только после посещения студии.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            // Проверка на повторный отзыв
            var alreadyReviewed = (await _reviewRepo.FindAsync(r =>
                r.ClientId == client.Id)).Any();

            if (alreadyReviewed)
            {
                TempData["Message"] = "Вы уже оставляли отзыв. Спасибо!";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            // Создание отзыва
            var review = new Review
            {
                ClientId = client.Id,
                Rating = rating,
                Text = text,
                IsApproved = false
            };

            await _reviewRepo.AddAsync(review);
            await _reviewRepo.SaveChangesAsync();

            TempData["Message"] = "✅ Отзыв отправлен на модерацию! Спасибо!";
            TempData["IsSuccess"] = true;

            return RedirectToAction("Index");
        }
    }
}