using System.Transactions;
using static GInterfaceCore.Models.EnumTypes;
using TransactionStatus = GInterfaceCore.Models.EnumTypes.TransactionStatus;

namespace GInterfaceCore.Models
{
    public class FileCSV
    {

        public int ID { get; set; }
        public string FileNames { get; set; }
        public DateTime FileDate { get; set; }
        public TransactionStatus FileStatus { get; set; } // status the document
        public int FileFields { get; set; }
        public Dictionary<int, string> FileType { get; set; } // status the document
        public string InboundOutbound { get; set; }
        public string FileJsonObj { get; set; }

    }
}
