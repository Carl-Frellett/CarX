namespace CarX.Events.EventArgs
{
    using CarX.API.Features;

    public class LeftEventArgs : JoinedEventArgs
    {
        public LeftEventArgs(Player player)
            : base(player)
        {
        }
    }
}