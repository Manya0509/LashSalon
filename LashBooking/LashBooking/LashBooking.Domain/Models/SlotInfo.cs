using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LashBooking.Domain.Models
{
    // Описание одного временного слота в расписании.
    // Например: 10:00 — свободен, 14:00 — занят.
    // Используется в ScheduleService, BookingController, CalendarController.
    public class SlotInfo
    {
        // Время начала слота (например, 2026-03-27 10:00)
        public DateTime Time { get; set; }

        // Занят ли этот слот (true = нельзя записаться)
        public bool IsBusy { get; set; }
    }
}

