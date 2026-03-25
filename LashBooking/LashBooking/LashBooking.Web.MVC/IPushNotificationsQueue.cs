using System.Threading;
using System.Threading.Tasks;
using AMSZerti.Web.Data.Models;

namespace AMSZerti.Web.Data.SharedService
{
    public interface IPushNotificationsQueue
    {
        void Enqueue(LogMessageEntry message);

        Task<LogMessageEntry> DequeueAsync(CancellationToken cancellationToken);
    }
}