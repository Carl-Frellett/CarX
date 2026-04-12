namespace CarX.Events.Patches.Player
{
    using CarX.API.Features;
    using CarX.Events.EventArgs;
    using HarmonyLib;
    using MEC;
    using System;
    using System.Collections.Generic;

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkIsVerified), MethodType.Setter)]
    internal static class Joined
    {
        private static void Prefix(CharacterClassManager __instance, bool value)
        {
            try
            {
                Timing.RunCoroutine(HandleJoined(__instance, value));
            }
            catch (Exception exception)
            {
                API.Features.Log.Error($"{typeof(Joined).FullName}:\n{exception}");
            }
        }

        private static IEnumerator<float> HandleJoined(CharacterClassManager ccm, bool value)
        {
            if (!value || (string.IsNullOrEmpty(ccm.NetworkSteamId) && Server.Host.ReferenceHub.characterClassManager.OnlineMode))
                yield break;

            if (API.Features.Player.Dictionary.TryGetValue(ccm.gameObject, out API.Features.Player existingPlayer) && existingPlayer != null)
                yield break;

            ReferenceHub hub = ReferenceHub.GetHub(ccm.gameObject);
            if (hub == null)
                yield break;

            var player = new API.Features.Player(hub);
            API.Features.Player.Dictionary[ccm.gameObject] = player;

            float timeout = 5f;
            float elapsed = 0f;

            while (elapsed < timeout)
            {
                if (!string.IsNullOrWhiteSpace(player.Nickname))
                    break;

                elapsed += 0.2f;
                yield return Timing.WaitForSeconds(0.2f);
            }

            string nickname = string.IsNullOrWhiteSpace(player.Nickname) ? "<unknown>" : player.Nickname;

            API.Features.Log.SendRaw(
                $"Player {nickname} ({player.UserId}) ({player.IPAddress}) ({player.Id}) joined the server",
                ConsoleColor.Green);

            if (player.IsMuted)
                player.ReferenceHub.characterClassManager.SetSyncVarDirtyBit(2UL);

            var ev = new JoinedEventArgs(player);
            CarX.Events.Handlers.Player.OnJoined(ev);
        }
    }
}