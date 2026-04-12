namespace CarX.API.Features
{
    using GameCore;

    public static class Round
    {
        public static bool IsStarted => RoundSummary.RoundInProgress();

        public static bool IsLocked
        {
            get => RoundSummary.singleton.RoundLock;
            set => RoundSummary.singleton.RoundLock = value;
        }

        public static bool IsLobbyLocked
        {
            get => RoundStart.LobbyLock;
            set => RoundStart.LobbyLock = value;
        }

        public static void Restart() => Server.Host.ReferenceHub.playerStats.Roundrestart();

        public static void Start() => Server.Host.ReferenceHub.characterClassManager.ForceRoundStart();
    }
}
