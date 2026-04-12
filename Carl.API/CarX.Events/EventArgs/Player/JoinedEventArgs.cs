namespace CarX.Events.EventArgs
{
    using CarX.API.Features;
    using System;

    public class JoinedEventArgs : EventArgs
    {
        public JoinedEventArgs(Player? player) => Player = player;

        public Player? Player { get; }
    }
}
