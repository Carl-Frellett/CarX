namespace CarX.Loader
{
    using System.ComponentModel;

    using CarX.API.Enums;
    using CarX.API.Interfaces;

    public sealed class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("Indicates whether outdated plugins should be loaded or not")]
        public bool ShouldLoadOutdatedPlugins { get; set; } = true;

        [Description("The working environment type (Development, Testing, Production, Ptb)")]
        public EnvironmentType Environment { get; set; } = EnvironmentType.Production;
    }
}
