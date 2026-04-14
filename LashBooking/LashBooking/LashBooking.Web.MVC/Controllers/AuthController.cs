using Microsoft.AspNetCore.Mvc;
using LashBooking.Domain.Interfaces;
using LashBooking.Domain.Entities;
using LashBooking.Domain.Constants;
using LashBooking.Web.MVC.Models;
using System.Security.Cryptography;

namespace LashBooking.Web.MVC.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IRepository<Client> _clientRepo;

        public AuthController(
            IRepository<Client> clientRepo,
            ILogger logger) : base(logger)
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
            try
            {
                InitRequestInfo();

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

                // Пользователь не найден — Warning, не ошибка приложения
                if (client == null)
                {
                    TempData["ErrorMessage"] = "Пользователь не найден";
                    return RedirectToAction("Login");
                }

                // Неверный пароль — Warning
                if (!VerifyPassword(model.LoginPassword, client.Password ?? ""))
                {
                    TempData["ErrorMessage"] = "Неверный пароль";
                    return RedirectToAction("Login");
                }

                HttpContext.Session.SetInt32("ClientId", client.Id);
                HttpContext.Session.SetString("ClientName", client.Name);

                if (client.IsAdmin)
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Profile");

            }
            catch (Exception ex)
            {
                // Неожиданная ошибка при входе — Error
                CatchException(ex, "AuthController/HandleLogin", ErrorLevel.Error);
                TempData["ErrorMessage"] = "Произошла ошибка при входе. Попробуйте ещё раз.";
                return RedirectToAction("Login");
            }
        }

        // POST: /Auth/HandleRegister
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleRegister(RegisterViewModel model)
        {
            try
            {
                InitRequestInfo();

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
                var existingClient = existing.FirstOrDefault();

                if (existingClient != null)
                {
                    // Клиент с таким телефоном уже есть
                    if (!string.IsNullOrEmpty(existingClient.Password))
                    {
                        // У него уже есть пароль — значит он уже зарегистрирован
                        TempData["ErrorMessage"] = "Этот телефон уже зарегистрирован. Войдите в аккаунт.";
                        return RedirectToAction("Login");
                    }

                    // Пароля нет — клиент записывался без аккаунта
                    // Привязываем пароль и обновляем данные
                    existingClient.Password = HashPassword(model.RegPassword);
                    existingClient.Name = model.RegName;
                    existingClient.Email = string.IsNullOrWhiteSpace(model.RegEmail) ? null : model.RegEmail;
                    _clientRepo.Update(existingClient);
                    await _clientRepo.SaveChangesAsync();

                    HttpContext.Session.SetInt32("ClientId", existingClient.Id);
                    HttpContext.Session.SetString("ClientName", existingClient.Name);

                    if (existingClient.IsAdmin)
                    {
                        HttpContext.Session.SetString("IsAdmin", "true");
                    }
                }
                else
                {
                    // Телефон новый — создаём нового клиента
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

                    if (newClient.IsAdmin)
                    {
                        HttpContext.Session.SetString("IsAdmin", "true");
                    }
                }
                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                // Неожиданная ошибка при регистрации — Error
                CatchException(ex, "AuthController/HandleRegister", ErrorLevel.Error);
                TempData["ErrorMessage"] = "Произошла ошибка при регистрации. Попробуйте ещё раз.";
                return RedirectToAction("Login", new { mode = "register" });
            }
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Remove("ClientId");
                HttpContext.Session.Remove("ClientName");
                HttpContext.Session.Remove("IsAdmin");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Ошибка при выходе — Warning, не критично
                CatchException(ex, "AuthController/Logout", ErrorLevel.Warning);
                return RedirectToAction("Index", "Home");
            }
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, 100000, HashAlgorithmName.SHA256);
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

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, 100000, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(32);

                for (int i = 0; i < 32; i++)
                    if (hash[i] != hashBytes[i + 16]) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string CleanPhone(string phone) =>
            new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
    }
}
