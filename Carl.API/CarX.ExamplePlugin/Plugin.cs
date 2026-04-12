using CarX.API.Features;
using CarX.API.Interfaces;
using CarX.Events.EventArgs;

namespace CarX.ExamplePlugin
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Carl Frellett";

        public override string Name => "ExamplePlugin";

        public override void OnEnabled()
        {
            base.OnEnabled();
            CarX.Events.Handlers.Player.PlayerCommandEnter += OnPlayerEnterCommand;
        }

        public override void OnDisabled()
        {
            CarX.Events.Handlers.Player.PlayerCommandEnter -= OnPlayerEnterCommand;
            base.OnDisabled();
        }

        private void OnPlayerEnterCommand(PlayerCommandEnterEventArgs ev)
        {
            if (ev.Command.ToLower().StartsWith("bc"))
            {
                string content = ev.Command.Length > 2
                    ? ev.Command.Substring(2).TrimStart()
                    : string.Empty;

                Map.Broadcast(5, $"[Chat] {ev.Player?.Nickname}: {content}", false, false);

                ev.Response = "chat message sent successfully.";
                ev.ResponseColor = "green";
            }
        }
    }

    public class Config : IConfig
    {
        public bool IsEnabled { get; set; }
    }
}