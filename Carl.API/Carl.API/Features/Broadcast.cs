namespace CarX.API.Features
{
    using System.ComponentModel;

    public class Broadcast
    {
        public Broadcast()
            : this(string.Empty)
        {
        }

        public Broadcast(string content, ushort duration = 10, bool show = true)
        {
            Content = content;
            Duration = duration;
            Show = show;
        }

        [Description("The broadcast content")]
        public string Content { get; set; }

        [Description("The broadcast duration")]
        public ushort Duration { get; set; }

        [Description("Indicates whether the broadcast should be shown or not")]
        public bool Show { get; set; }
    }
}
