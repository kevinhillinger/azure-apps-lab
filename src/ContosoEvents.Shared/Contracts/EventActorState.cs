using ContosoEvents.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ContosoEvents.Shared.Contracts
{
    /// <summary>
    /// This class contains each actor's replicated state.
    /// Each instance of this class is serialized and replicated every time an actor's state is saved.
    /// For more information, see http://aka.ms/servicefabricactorsstateserialization
    /// </summary>
    [Serializable]
    [DataContract]
    public class EventActorState
    {
        public EventActorState()
        {
            Tickets = 0;
            RequestedTickets = 0;
            FailedTickets = 0;
            CancelledTickets = 0;
            Orders = new List<TicketOrder>();
        }

        public EventActorState(TicketEvent ev)
        {
            Tickets = ev.TotalTickets;
            RequestedTickets = 0;
            FailedTickets = 0;
            CancelledTickets = 0;
            Orders = new List<TicketOrder>();
        }

        public EventActorState(TicketEventStats stats)
        {
            Tickets = stats.Tickets;
            RequestedTickets = stats.RequestedTickets;
            FailedTickets = stats.FailedTickets;
            CancelledTickets = stats.CancelledTickets;
            Orders = new List<TicketOrder>(); // Yes ..we lose the historical orders!
        }

        [DataMember]
        public int Tickets { get; set; }

        [DataMember]
        public int RequestedTickets { get; set; }

        [DataMember]
        public int FailedTickets { get; set; }

        [DataMember]
        public int CancelledTickets { get; set; }

        [DataMember]
        public List<TicketOrder> Orders { get; set; }
    }
}
