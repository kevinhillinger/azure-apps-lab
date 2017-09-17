using ContosoEvents.Models;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Services
{
    public interface IPaymentProcessorService
    {
        Task<string> Authorize(TicketOrder order);
        Task<bool> Refund(TicketOrder order);
    }
}
