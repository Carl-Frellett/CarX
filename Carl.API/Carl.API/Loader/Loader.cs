namespace CarX.Loader
{
    using CarX.API;
    using CarX.API.Enums;
    using CarX.API.Features;
    using CarX.API.Interfaces;
    using CarX.Loader.Features;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public static class Loader
    {
        static Loader()
        {
            Log.Info($"Initializing at {Environment.CurrentDirectory}");
            ServerConsole.SendRaw($"{Assembly.GetExecutingAssembly().GetName().Name} - Version {Version.ToString(3)}", ConsoleColor.DarkYellow);
            CustomNetworkManager.Modded = true;

            if (Config.Environment != EnvironmentType.Production)
                Paths.Reload($"CarX-{Config.Environment.ToString().ToUpper()}");
            if (!Directory.Exists(Paths.Configs))
                Directory.CreateDirectory(Paths.Configs);
            if (!Directory.Exists(Paths.Plugins))
                Directory.CreateDirectory(Paths.Plugins);
            if (!Directory.Exists(Paths.Dependencies))
                Directory.CreateDirectory(Paths.Dependencies);
        }

        public static SortedSet<IPlugin<IConfig>> Plugins { get; } = new SortedSet<IPlugin<IConfig>>(PluginPriorityComparer.Instance);

        public static Random Random { get; } = new Random();

        public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

        public static Config Config { get; } = new Config();

        public static bool ShouldDebugBeShown => Config.Environment == EnvironmentType.Testing || Config.Environment == EnvironmentType.Development;

        public static List<Assembly> Dependencies { get; } = new List<Assembly>();

        public static void Run(Assembly[] dependencies = null)
        {
            ServerConsole.Initialize();

            if (dependencies?.Length > 0)
                Dependencies.AddRange(dependencies);

            LoadDependencies();
            LoadPlugins();

            ConfigManager.Reload();

            EnablePlugins();
        }

        public static void LoadPlugins()
        {
            foreach (string pluginPath in Directory.GetFiles(Paths.Plugins, "*.dll"))
            {
                Assembly assembly = LoadAssembly(pluginPath);

                if (assembly == null)
                    continue;

                IPlugin<IConfig> plugin = CreatePlugin(assembly);

                if (plugin == null)
                    continue;

                Plugins.Add(plugin);
            }
        }

        public static Assembly LoadAssembly(string path)
        {
            try
            {
                return Assembly.Load(File.ReadAllBytes(path));
            }
            catch (Exception exception)
            {
                Log.Error($"Error while loading an assembly at {path}! {exception}");
            }

            return null;
        }

        public static IPlugin<IConfig> CreatePlugin(Assembly assembly)
        {
            try
            {
                foreach (Type type in assembly.GetTypes().Where(type => !type.IsAbstract && !type.IsInterface))
                {
                    if (!type.BaseType.IsGenericType || type.BaseType.GetGenericTypeDefinition() != typeof(Plugin<>))
                    {
                        Log.Debug($"\"{type.FullName}\" does not inherit from Plugin<TConfig>, skipping.", ShouldDebugBeShown);
                        continue;
                    }

                    Log.Debug($"Loading type {type.FullName}", ShouldDebugBeShown);

                    IPlugin<IConfig> plugin = null;

                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        Log.Debug("Public default constructor found, creating instance...", ShouldDebugBeShown);

                        plugin = constructor.Invoke(null) as IPlugin<IConfig>;
                    }
                    else
                    {
                        Log.Debug($"Constructor wasn't found, searching for a property with the {type.FullName} type...", ShouldDebugBeShown);

                        var value = Array.Find(type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public), property => property.PropertyType == type)?.GetValue(null);

                        if (value != null)
                            plugin = value as IPlugin<IConfig>;
                    }

                    if (plugin == null)
                    {
                        Log.Error($"{type.FullName} is a valid plugin, but it cannot be instantiated! It either doesn't have a public default constructor without any arguments or a static property of the {type.FullName} type!");

                        continue;
                    }

                    Log.Debug($"Instantiated type {type.FullName}", ShouldDebugBeShown);

                    if (plugin.RequiredCarXVersion > Version)
                    {
                        if (!Config.ShouldLoadOutdatedPlugins)
                        {
                            Log.Error($"You're running an older version of CarX ({Version.ToString(3)})! {plugin.Name} won't be loaded! " + $"Required version to at least: {plugin.RequiredCarXVersion.ToString(3)}");

                            continue;
                        }
                        else
                        {
                            Log.Warn($"You're running an older version of CarX ({Version.ToString(3)})! " + $"You may encounter some bugs by loading {plugin.Name}! Update CarX to at least {plugin.RequiredCarXVersion.ToString(3)}");
                        }
                    }

                    return plugin;
                }
            }
            catch (Exception exception)
            {
                Log.Error($"Error while initializing plugin {assembly.GetName().Name} (at {assembly.Location})! {exception}");
            }

            return null;
        }

        public static void EnablePlugins()
        {
            foreach (IPlugin<IConfig> plugin in Plugins)
            {
                try
                {
                    if (plugin.Config.IsEnabled)
                    {
                        plugin.OnEnabled();
                    }
                }
                catch (Exception exception)
                {
                    Log.Error($"Plugin \"{plugin.Name}\" threw an exception while enabling: {exception}");
                }
            }
        }

        public static void ReloadPlugins()
        {
            foreach (IPlugin<IConfig> plugin in Plugins)
            {
                try
                {
                    plugin.OnReloaded();

                    plugin.Config.IsEnabled = false;

                    plugin.OnDisabled();
                }
                catch (Exception exception)
                {
                    Log.Error($"Plugin \"{plugin.Name}\" threw an exception while reloading: {exception}");
                }
            }

            Plugins.Clear();

            LoadPlugins();

            ConfigManager.Reload();

            EnablePlugins();
        }

        public static void DisablePlugins()
        {
            foreach (IPlugin<IConfig> plugin in Plugins)
            {
                try
                {
                    plugin.OnDisabled();
                }
                catch (Exception exception)
                {
                    Log.Error($"Plugin \"{plugin.Name}\" threw an exception while disabling: {exception}");
                }
            }
        }

        private static void LoadDependencies()
        {
            try
            {
                Log.Info($"Loading dependencies at {Paths.Dependencies}");

                foreach (string dependency in Directory.GetFiles(Paths.Dependencies, "*.dll"))
                {
                    Assembly assembly = LoadAssembly(dependency);

                    if (assembly == null)
                        continue;

                    Dependencies.Add(assembly);

                    Log.Info($"Loaded dependency {assembly.FullName}");
                }

                Log.Info("Dependencies loaded successfully!");
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while loading dependencies! {exception}");
            }
        }
    }
}
