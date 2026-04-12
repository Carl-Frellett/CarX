using CarX.Events.EventArgs;
using CarX.Events.Extensions;
using static CarX.Events.Plugin;

namespace CarX.Events.Handlers
{
    public class Player
    {
        public static event CustomEventHandler<JoinedEventArgs>? Joined;
        public static event CustomEventHandler<LeftEventArgs>? Left;
        public static event CustomEventHandler<PlayerCommandEnterEventArgs>? PlayerCommandEnter;

        public static void OnJoined(JoinedEventArgs ev) => Joined?.InvokeSafely(ev);

        public static void OnLeft(LeftEventArgs ev) => Left?.InvokeSafely(ev);

        public static void OnPlayerCommandEnter(PlayerCommandEnterEventArgs ev) => PlayerCommandEnter?.InvokeSafely(ev);
    }
}