namespace CarX.Events.Patches.Player.PlayerCommand
{
    using CarX.Events.EventArgs;
    using HarmonyLib;
    using Mirror;
    using RemoteAdmin;
    using System;

    [HarmonyPatch(typeof(GameConsoleTransmission), nameof(GameConsoleTransmission.SendToClient))]
    internal static class PlayerCommandEnterResponse
    {
        private static void Prefix(GameConsoleTransmission __instance, NetworkConnection connection, ref string text, ref string color)
        {
            try
            {
                if (__instance == null || __instance.Processor == null)
                    return;

                QueryProcessor processor = __instance.Processor;

                if (!PlayerCommandCache.PendingCommands.TryGetValue(processor, out var data))
                    return;

                var ev = new PlayerCommandEnterEventArgs(
                    data.Player,
                    data.Command,
                    text ?? string.Empty,
                    color ?? string.Empty);

                CarX.Events.Handlers.Player.OnPlayerCommandEnter(ev);

                text = ev.Response ?? string.Empty;
                color = ev.ResponseColor ?? string.Empty;

                PlayerCommandCache.PendingCommands.Remove(processor);
            }
            catch (Exception exception)
            {
                API.Features.Log.Error($"{typeof(PlayerCommandEnterResponse).FullName}:\n{exception}");
            }
        }
    }
}