using Microsoft.AspNetCore.SignalR;

namespace GInterfaceCore.Hubing
{
    public class Servicehubs
    {
        private readonly IHubContext<HubSign> _hubContext;

        public Servicehubs(IHubContext<HubSign> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyClients(string user, string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
