namespace RemoveCows
{
    public sealed class Settings
    {
        private Settings()
        {
            Tag = "Remove Cows [Fixed for v1.6]";
        }

        private static readonly Settings _Instance = new Settings();
        public static Settings Instance { get { return _Instance; } }

        public readonly string Tag;
    }
}