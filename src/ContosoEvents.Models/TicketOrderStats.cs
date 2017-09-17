namespace ContosoEvents.Models
{
    public class TicketOrderStats
    {
        public string Tag { get; set; }
        public int Count { get; set; }
        public double SumSeconds { get; set; }
        public double AverageSeconds { get; set; }
    }
}
