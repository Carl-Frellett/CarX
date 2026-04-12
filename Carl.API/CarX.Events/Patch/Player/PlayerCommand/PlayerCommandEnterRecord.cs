namespace CarX.Events.Patches.Player.PlayerCommand
{
    using HarmonyLib;
    using RemoteAdmin;
    using System;

    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
    internal static class PlayerCommandEnterRecord
    {
        private static void Prefix(QueryProcessor __instance, string query, bool encrypted)
        {
            try
            {
                if (__instance == null || string.IsNullOrWhiteSpace(query))
                    return;

                API.Features.Player? player = null;

                if (__instance.gameObject != null &&
                    API.Features.Player.Dictionary.TryGetValue(__instance.gameObject, out API.Features.Player existingPlayer) &&
                    existingPlayer != null)
                {
                    player = existingPlayer;
                }
                else
                {
                    ReferenceHub hub = ReferenceHub.GetHub(__instance.gameObject);

                    if (hub != null)
                        player = new API.Features.Player(hub);
                }

                PlayerCommandCache.PendingCommands[__instance] = (player, query);
            }
            catch (Exception exception)
            {
                API.Features.Log.Error($"{typeof(PlayerCommandEnterRecord).FullName}:\n{exception}");
            }
        }
    }
}