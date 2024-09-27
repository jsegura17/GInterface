namespace GInterfaceCore.Models
{
    public class JsonTemplate
    {
        public List<string> FileColumName { get; set; }
        public List<string> FileColumNamePosition { get; set; }
        public List<string> RequireFields { get; set; }
        public List<string> DataRequireFields { get; set; }
        public List<object> StartEndHead { get; set; }
    }
}
