using Elmah.Io.Client;

namespace Elmah.Io.Extensions.Logging
{
    public interface ICanHandleMessages
    {
        void AddMessage(CreateMessage message);
    }
}
