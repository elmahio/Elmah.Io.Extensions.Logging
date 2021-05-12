using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Elmah.Io.Extensions.Logging.AspNetCore31.SignalR.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            // Include the following line to log an exception happening as part of sending a message:
            //throw new ApplicationException("Error during SendMessage");

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
