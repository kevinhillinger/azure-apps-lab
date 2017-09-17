using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace ContosoEvents.Shared.Services
{
    public class ActorLocationService : IActorLocationService
    {
        public TActorInterface Create<TActorInterface>(ActorId id, string applicationName) where TActorInterface : IActor
        {
            return ActorProxy.Create<TActorInterface>(id, applicationName);
        }

        public TActorInterface Create<TActorInterface>(string actorId, string applicationName) where TActorInterface : IActor
        {
            return ActorProxy.Create<TActorInterface>(new ActorId(actorId), applicationName);
        }

        public TActorInterface Create<TActorInterface>(long actorId, string applicationName) where TActorInterface : IActor
        {
            return ActorProxy.Create<TActorInterface>(new ActorId(actorId), applicationName);
        }
    }
}
