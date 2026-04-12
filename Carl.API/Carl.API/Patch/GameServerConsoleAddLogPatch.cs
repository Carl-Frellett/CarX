using HarmonyLib;

using CustomServerConsole = CarX.API.ServerConsole;
using GameServerConsole = global::ServerConsole;

namespace CarX.API.Patches
{
    [HarmonyPatch(typeof(GameServerConsole), "AddLog")]
    internal static class GameServerConsoleAddLogPatch
    {
        private static bool Prefix(string q)
        {
            CustomServerConsole.SendGameLog(q);

            return false;
        }
    }
}