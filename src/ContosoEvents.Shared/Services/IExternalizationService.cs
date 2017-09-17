using ContosoEvents.Models;
using ContosoEvents.Shared.Contracts;
using System.Threading.Tasks;

namespace ContosoEvents.Shared.Services
{
    public interface IExternalizationService
    {
        Task Externalize(TicketOrder order);
        Task Simulate(TicketOrderSimulationRequest request);
        Task Backup(EventActorState eventState);
    }
}
