using static GInterfaceCore.Models.EnumTypes;

namespace GInterfaceCore.Models
{
    public class TransactionEvent
    {
        public Guid TransactionId { get; set; }
        public string Message { get; set; }
        public TransactionStatus Status { get; set; }
        public TransactionTask TransactionTaskProcess { get; set; }
    }
}
