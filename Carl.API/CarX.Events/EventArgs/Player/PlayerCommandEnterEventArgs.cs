namespace CarX.Events.EventArgs
{
    using CarX.API.Features;
    using System;

    public class PlayerCommandEnterEventArgs : EventArgs
    {
        public PlayerCommandEnterEventArgs(Player? player, string command, string response, string responseColor)
        {
            Player = player;
            Command = command;
            Response = response;
            ResponseColor = responseColor;
        }

        public Player? Player { get; }

        public string Command { get; set; }

        public string Response { get; set; }

        public string ResponseColor { get; set; }
    }
}