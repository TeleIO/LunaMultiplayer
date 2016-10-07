using LunaCommon.Message.Interface;
using LunaServer.Client;

namespace LunaServer.Plugin
{
    public abstract class LmpPlugin : ILmpPlugin
    {
        public virtual void OnUpdate()
        {
        }

        public virtual void OnServerStart()
        {
        }

        public virtual void OnServerStop()
        {
        }

        public virtual void OnClientConnect(ClientStructure client)
        {
        }

        public virtual void OnClientAuthenticated(ClientStructure client)
        {
        }

        public virtual void OnClientDisconnect(ClientStructure client)
        {
        }

        public virtual void OnMessageReceived(ClientStructure client, IClientMessageBase messageData)
        {
        }

        public virtual void OnMessageSent(ClientStructure client, IServerMessageBase messageData)
        {
        }
    }
}