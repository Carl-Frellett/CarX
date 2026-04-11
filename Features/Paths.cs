namespace CarX.API.Features
{
    using Mirror;
    using System;
    using System.IO;

    public static class Paths
    {
        static Paths() => Reload();
        public static string ManagedAssemblies { get; } = Path.Combine(Environment.CurrentDirectory);
        public static string CarX { get; set; }
        public static string Plugins { get; set; }
        public static string Dependencies { get; set; }
        public static string Configs { get; set; }
        public static string Config { get; set; }
        public static string Log { get; set; }

        public static void Reload(string rootDirectoryName = "CarX")
        {
            CarX = Path.Combine(ManagedAssemblies, rootDirectoryName);
            Plugins = Path.Combine(CarX, "Plugins");
            Dependencies = Path.Combine(Plugins, "dependencies");
            Configs = Path.Combine(CarX, "Configs");
            Config = Path.Combine(Configs, $"{Server.Port}-config.yml");
            Log = Path.Combine(CarX, $"{Server.Port}-RemoteAdminLog.txt");
        }
    }
}