using System.Net.Http.Json;

namespace LashBooking.Web.MVC.Services
{
    public interface ITelegramNotificationService
    {
        Task SendNewAppointmentAsync(
            string clientName,
            string clientPhone,
            string serviceName,
            DateTime appointmentDate,
            decimal price);

        Task SendAppointmentCancelledAsync(
            string clientName,
            string clientPhone,
            string serviceName,
            DateTime appointmentDate,
            decimal price);
    }

    public class TelegramNotificationService : ITelegramNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TelegramNotificationService> _logger;
        private readonly string _botToken;
        private readonly string _adminChatId;

        public TelegramNotificationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TelegramNotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _botToken = configuration["Telegram:BotToken"] ?? "";
            _adminChatId = configuration["Telegram:AdminChatId"] ?? "";
        }

        public async Task SendNewAppointmentAsync(  // Формирует текст сообщения с эмодзи и HTML-разметкой
            string clientName,
            string clientPhone,
            string serviceName,
            DateTime appointmentDate,
            decimal price)
        {
            // Если токен или chat_id не настроены — тихо пропускаем (не ломаем запись)
            if (string.IsNullOrWhiteSpace(_botToken) || string.IsNullOrWhiteSpace(_adminChatId))
            {
                _logger.LogWarning("Telegram уведомления не настроены — токен или chat_id пустые.");
                return;
            }

            var message =
                $"🎉 <b>Новая запись!</b>\n\n" +
                $"👤 <b>Клиент:</b> {EscapeHtml(clientName)}\n" +
                $"📞 <b>Телефон:</b> {EscapeHtml(clientPhone)}\n" +
                $"💅 <b>Услуга:</b> {EscapeHtml(serviceName)}\n" +
                $"📅 <b>Дата:</b> {appointmentDate:dd.MM.yyyy HH:mm}\n" +
                $"💰 <b>Стоимость:</b> {price:N0} ₽";

            await SendMessageAsync(message);
        }

        public async Task SendAppointmentCancelledAsync(
    string clientName,
    string clientPhone,
    string serviceName,
    DateTime appointmentDate,
    decimal price)
        {
            if (string.IsNullOrWhiteSpace(_botToken) || string.IsNullOrWhiteSpace(_adminChatId))
            {
                _logger.LogWarning("Telegram уведомления не настроены — токен или chat_id пустые.");
                return;
            }

            var message =
                $"❌ <b>Клиент отменил запись</b>\n\n" +
                $"👤 <b>Клиент:</b> {EscapeHtml(clientName)}\n" +
                $"📞 <b>Телефон:</b> {EscapeHtml(clientPhone)}\n" +
                $"💅 <b>Услуга:</b> {EscapeHtml(serviceName)}\n" +
                $"📅 <b>Дата:</b> {appointmentDate:dd.MM.yyyy HH:mm}\n" +
                $"💰 <b>Стоимость:</b> {price:N0} ₽";

            await SendMessageAsync(message);
        }

        private async Task SendMessageAsync(string text)
        {
            try
            {
                var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

                var payload = new
                {
                    chat_id = _adminChatId,
                    text = text,
                    parse_mode = "HTML"
                };

                var response = await _httpClient.PostAsJsonAsync(url, payload);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Ошибка отправки Telegram-уведомления. Status: {Status}, Body: {Body}",
                        response.StatusCode, errorBody);
                }
            }
            catch (Exception ex)
            {
                // Не бросаем исключение наружу — уведомление не должно ломать запись
                _logger.LogError(ex, "Не удалось отправить Telegram-уведомление.");
            }
        }

        // Экранирование HTML-спецсимволов, чтобы не сломать разметку Telegram
        private static string EscapeHtml(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }
    }
}