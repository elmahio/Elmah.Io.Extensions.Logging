using Elmah.Io.Client.Models;

namespace Elmah.Io.Extensions.Logging
{
    public interface ICanHandleMessages
    {
        void AddMessage(CreateMessage message);
    }
}
