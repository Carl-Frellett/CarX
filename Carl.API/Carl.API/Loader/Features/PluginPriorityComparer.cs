namespace CarX.Loader.Features
{
    using System.Collections.Generic;

    using CarX.API.Interfaces;

    public sealed class PluginPriorityComparer : IComparer<IPlugin<IConfig>>
    {
        /// <summary>
        /// Public instance.
        /// </summary>
        public static readonly PluginPriorityComparer Instance = new PluginPriorityComparer();

        /// <inheritdoc/>
        public int Compare(IPlugin<IConfig> x, IPlugin<IConfig> y)
        {
            var value = y.Priority.CompareTo(x.Priority);
            if (value == 0)
                value = x.GetHashCode().CompareTo(y.GetHashCode());

            return value == 0 ? 1 : value;
        }
    }
}
