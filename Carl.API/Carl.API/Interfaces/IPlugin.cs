namespace CarX.API.Interfaces
{
    using CarX.API.Enums;
    using System;
    using System.Reflection;

    public interface IPlugin<out TConfig> : IComparable<IPlugin<IConfig>>
        where TConfig : IConfig
    {
        Assembly Assembly { get; }

        string Name { get; }

        string Prefix { get; }

        string Author { get; }

        PluginPriority Priority { get; }

        Version Version { get; }

        Version RequiredCarXVersion { get; }

        TConfig Config { get; }

        void OnEnabled();

        void OnDisabled();

        void OnReloaded();
    }
}
