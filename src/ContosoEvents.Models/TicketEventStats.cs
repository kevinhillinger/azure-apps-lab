using System;
using System.Runtime.Serialization;

namespace ContosoEvents.Models
{

    [Serializable]
    [DataContract]
    public class TicketEventStats
    {
        public TicketEventStats()
        {
            Tickets = 0;
            RequestedTickets = 0;
            FailedTickets = 0;
            CancelledTickets = 0;
            Orders = 0;
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
        public int Orders { get; set; }
    }
}
