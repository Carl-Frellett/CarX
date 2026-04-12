using kcp2k;
using Mirror;
using System.Reflection;

namespace CarX.API.Features
{
    public static class Server
    {
        private static KcpTransport kcpTransport => (kcp2k.KcpTransport)NetworkManager.singleton.transport;
        private static Player host;
        private static global::Broadcast broadcast;
        private static MethodInfo sendSpawnMessage;
        private static BanPlayer banPlayer;
        public static Player Host
        {
            get
            {
                if (host == null || host.ReferenceHub == null)
                    host = new Player(PlayerManager.localPlayer);

                return host;
            }
        }
        public static global::Broadcast Broadcast
        {
            get
            {
                if (broadcast == null)
                    broadcast = PlayerManager.localPlayer.GetComponent<global::Broadcast>();

                return broadcast;
            }
        }
        public static MethodInfo SendSpawnMessage
        {
            get
            {
                if (sendSpawnMessage == null)
                {
                    sendSpawnMessage = typeof(NetworkServer).GetMethod(
                        "SendSpawnMessage",
                        BindingFlags.NonPublic | BindingFlags.Static);
                }

                return sendSpawnMessage;
            }
        }

        public static string Name
        {
            get => global::ServerConsole._serverName;
            set
            {
                global::ServerConsole._serverName = value;
                global::ServerConsole.singleton.RefreshServerName();
            }
        }

        public static BanPlayer BanPlayer
        {
            get
            {
                if (banPlayer == null)
                    banPlayer = PlayerManager.localPlayer.GetComponent<BanPlayer>();

                return banPlayer;
            }
        }

        public static ushort Port
        {
            get => kcpTransport.Port;
        }
    }
}
