using Microsoft.AspNetCore.SignalR;

namespace GInterfaceCore.Hubing
{
    public class HubSign : Hub
    {
        public async Task SendMessage()
        {
            await Clients.All.SendAsync("FileUploadCompleted");
        }
    }
}
