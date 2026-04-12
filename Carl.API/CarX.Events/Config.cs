using CarX.API.Interfaces;
using System.ComponentModel;

namespace CarX.Events
{
    public class Config : IConfig
    {
        [Description("Is this plugin enable?")]
        public bool IsEnabled { get; set; } = true;
    }
}
