using ContosoEvents.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Fabric.Health;
using System.Threading;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Contracts
{
    public interface ITicketOrderService : IService
    {
        Task<string> EnqueueOrder(TicketOrder order);
        Task<string> GetNodeName();
        Task<int> GetOrdersCounter(CancellationToken ct);
        Task UpdateHealthState(HealthState state, string message);
    }
}
