using static GInterfaceCore.Models.EnumTypes;
using static GInterfaceCore.Components.Pages.Config;

namespace GInterfaceCore.Data
{
    public class ConfigItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ConfigArea Area { get; set; }
    }
}
