using static GInterface.Models.EnumTypes;
using static GInterface.Pages.Config;

namespace GInterface.Data
{
    public class ConfigItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ConfigArea Area { get; set; }
    }
}
