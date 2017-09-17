using ContosoEvents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Services
{
    /// <summary>
    /// This interface provides access to the OLTP back-end system.  
    /// </summary>
    public interface IDataStoreService
    {
        Task<List<TicketEvent>> GetEvents();
        Task<TicketEvent> GetEventById(string eventId);
        Task<List<TicketOrder>> GetOrdersByUserName(string userName);
        Task<List<TicketOrderStats>> GetOrderStats();

        Task<string> CreateEvent(TicketEvent tEvent);
        Task<TicketEventStats> GetEventStats(string eventId);

        Task DeleteAllEvents();
        Task DeleteAllOrders();
        Task DeleteAllLogMessages();
    }
}
