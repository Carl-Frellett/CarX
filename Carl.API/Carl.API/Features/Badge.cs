namespace CarX.API.Features
{
    public struct Badge
    {
        public Badge(string text, string color, int type, bool isGlobal = false)
        {
            Text = text;
            Color = color;
            Type = type;
            IsGlobal = isGlobal;
        }

        public string Text { get; private set; }

        public string Color { get; private set; }

        public int Type { get; private set; }

        public bool IsGlobal { get; private set; }
    }
}
