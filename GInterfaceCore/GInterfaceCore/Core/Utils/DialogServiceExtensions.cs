using Microsoft.AspNetCore.Components;
using Radzen;

namespace GInterfaceCore.Core.Utils
{
    public static class DialogServiceExtensions
    {
        public static async Task<dynamic> OpenComponentExtension<T>(this DialogService dialogService, string title, Dictionary<string, object> parameters = null, DialogOptions options = null) where T : ComponentBase
        {
            string additionalStyle = "min-width:fit-content;min-height:fit-content;height:fit-content;width:fit-content;border: 1px solid black;";
            var newOptions = new DialogOptions();

            if (options is not null)
            {
                newOptions = options;
            }

            newOptions.Style += additionalStyle;

            dynamic dynamic = await dialogService.OpenAsync<T>(title, parameters, newOptions);
            return dynamic;
        }
    }
}
