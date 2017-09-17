using ContosoEvents.Models;
using Microsoft.ServiceFabric.Actors;
using System.Fabric.Health;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Contracts
{
    /// <summary>
    /// This interface represents the actions a client app can perform on an actor.
    /// It MUST derive from IActor and all methods MUST return a Task.
    /// </summary>
    public interface ITicketOrderActor : IActor
    {
        Task<TicketOrder> GetOrder();
        Task ProcessOrder(TicketOrder order);
        Task CancelOrder();
        Task UpdateHealthState(HealthState state, string message);
    }
}
