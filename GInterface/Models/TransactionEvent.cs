using static GInterface.Models.EnumTypes;

namespace GInterface.Models
{
    public class TransactionEvent
    {
        public Guid TransactionId { get; set; }
        public string Message { get; set; }
        public TransactionStatus Status { get; set; }
        public TransactionTask TransactionTaskProcess { get; set; }
    }
}
