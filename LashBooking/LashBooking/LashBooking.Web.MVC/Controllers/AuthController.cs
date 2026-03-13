using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Entities;
using LashBooking.Web.MVC.Models;
using System.Security.Cryptography;

namespace LashBooking.Web.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IRepository<Client> _clientRepo;

        public AuthController(IRepository<Client> clientRepo)
        {
            _clientRepo = clientRepo;
        }

        // GET: /Auth/Login
        public IActionResult Login(string? mode)
        {
            ViewBag.IsLoginMode = mode != "register";
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View("~/Views/Auth/Login.cshtml");
        }

        // POST: /Auth/HandleLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Заполните все поля";
                return RedirectToAction("Login");
            }

            var cleanPhone = CleanPhone(model.LoginPhone);
            var clients = await _clientRepo.FindAsync(c => c.Phone == cleanPhone);
            var client = clients.FirstOrDefault();

            if (client == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден";
                return RedirectToAction("Login");
            }

            if (!VerifyPassword(model.LoginPassword, client.Password ?? ""))
            {
                TempData["ErrorMessage"] = "Неверный пароль";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetInt32("ClientId", client.Id);
            HttpContext.Session.SetString("ClientName", client.Name);

            return RedirectToAction("Index", "Profile");
        }

        // POST: /Auth/HandleRegister
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Заполните все поля";
                return RedirectToAction("Login", new { mode = "register" });
            }

            var cleanPhone = CleanPhone(model.RegPhone);
            var existing = await _clientRepo.FindAsync(c => c.Phone == cleanPhone);

            if (existing.Any())
            {
                TempData["ErrorMessage"] = "Этот телефон уже зарегистрирован";
                return RedirectToAction("Login", new { mode = "register" });
            }

            var newClient = new Client
            {
                Name = model.RegName,
                Phone = cleanPhone,
                Password = HashPassword(model.RegPassword),
                Email = string.IsNullOrWhiteSpace(model.RegEmail) ? null : model.RegEmail,
                CreatedAt = DateTime.Now
            };

            await _clientRepo.AddAsync(newClient);
            await _clientRepo.SaveChangesAsync();

            HttpContext.Session.SetInt32("ClientId", newClient.Id);
            HttpContext.Session.SetString("ClientName", newClient.Name);

            return RedirectToAction("Index", "Profile");
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("ClientId");
            HttpContext.Session.Remove("ClientName");
            return RedirectToAction("Index", "Home");
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            byte[] hashBytes = new byte[48];
            Buffer.BlockCopy(salt, 0, hashBytes, 0, 16);
            Buffer.BlockCopy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(storedHash);
                if (hashBytes.Length != 48) return false;

                byte[] salt = new byte[16];
                Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(32);

                for (int i = 0; i < 32; i++)
                    if (hash[i] != hashBytes[i + 16]) return false;

                return true;
            }
            catch { return false; }
        }

        private string CleanPhone(string phone) =>
            new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
    }
}
