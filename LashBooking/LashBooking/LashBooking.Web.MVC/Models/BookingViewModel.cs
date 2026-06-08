using System.ComponentModel.DataAnnotations;

namespace LashBooking.Web.MVC.Models
{
    public class BookingViewModel
    {

        [Required(ErrorMessage = "Введите имя")]
        [StringLength(100, MinimumLength = 2, 
            ErrorMessage = "Имя должно быть от 2 до 100 символов")]
        public string ClientName { get; set; } = "";

        [Required(ErrorMessage = "Введите телефон")]
        [RegularExpression(@"^[\d\+\-\(\)\s]{7,20}$",
            ErrorMessage = "Введите корректный номер телефона")]
        public string ClientPhone { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Выберите услугу")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Выберите время записи")]
        public string SelectedTime { get; set; } = "";
    }
}


