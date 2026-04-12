namespace CarX.Events.Patches.Player
{
    using CarX.Events.EventArgs;
    using HarmonyLib;
    using Mirror;
    using System;

    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    internal static class Left
    {
        private static void Prefix(NetworkConnectionToClient conn)
        {
            try
            {
                if (conn?.identity?.gameObject == null)
                    return;

                if (!API.Features.Player.Dictionary.TryGetValue(conn.identity.gameObject, out API.Features.Player? player) || player is null)
                    return;

                var ev = new LeftEventArgs(player);
                Handlers.Player.OnLeft(ev);

                API.Features.Log.SendRaw(
                    $"Player {player.Nickname} ({player.UserId}) ({player.IPAddress}) ({player.Id}) left the server",
                    ConsoleColor.Green);

                API.Features.Player.Dictionary.Remove(conn.identity.gameObject);
                API.Features.Player.UserIdsCache.Remove(player.UserId);
                API.Features.Player.IdsCache.Remove(player.Id);
            }
            catch (Exception exception)
            {
                API.Features.Log.Error($"{typeof(Left).FullName}:\n{exception}");
            }
        }
    }
}