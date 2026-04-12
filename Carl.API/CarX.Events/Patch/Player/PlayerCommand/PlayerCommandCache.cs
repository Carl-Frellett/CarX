namespace CarX.Events.Patches.Player.PlayerCommand
{
    using CarX.API.Features;
    using RemoteAdmin;
    using System.Collections.Generic;

    internal static class PlayerCommandCache
    {
        internal static readonly Dictionary<QueryProcessor, (Player? Player, string Command)> PendingCommands =
            new Dictionary<QueryProcessor, (Player? Player, string Command)>();
    }
}