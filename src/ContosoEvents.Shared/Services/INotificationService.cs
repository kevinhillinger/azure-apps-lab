using ContosoEvents.Models;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Services
{
    public interface INotificationService
    {
        Task Notify(TicketOrder order);
    }
}
