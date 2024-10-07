using System.Transactions;

namespace GInterface.Models
{
    public class FileCSV
    {

        public int ID { get; set; }
        public string FileNames { get; set; }
        public DateTime FileDate { get; set; }
        public TransactionStatus FileStatus { get; set; } // status the document
        public int FileFields { get; set; }  
        public string FileJsonObj { get; set; }

    }
}
