using kcp2k;
using Mirror;

namespace CarX.API.Features
{
    public static class Server
    {
        private static KcpTransport kcpTransport => (kcp2k.KcpTransport)NetworkManager.singleton.transport;

        public static ushort Port
        {
            get => kcpTransport.Port;
        }
    }
}
