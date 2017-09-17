using ContosoEvents.Models;
using Microsoft.ServiceFabric.Actors;
using System.Collections.Generic;
using System.Fabric.Health;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Contracts
{
    public interface IEventActor : IActor
    {
        Task Reload();
        Task<TicketEventStats> GetStats();
        Task<List<TicketOrder>> GetHistoricalOrders();
        Task<bool> CheckTickets(TicketOrder order);
        Task<bool> ReserveTickets(TicketOrder order);
        Task FailTickets(TicketOrder order);
        Task CancelTickets(TicketOrder order);
        Task UpdateHealthState(HealthState state, string message);
    }
}
