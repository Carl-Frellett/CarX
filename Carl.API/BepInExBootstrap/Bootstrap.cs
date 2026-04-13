using BepInEx;
using BepInEx.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CarX.BepInExBootstrap
{
    [BepInPlugin("com.carlfrellett.carx.bepInexbootstrap", "CarX BepInEx Bootstrap", "1.0.0")]
    public sealed class Bootstrap : BaseUnityPlugin
    {
        internal static ManualLogSource? LogInstance;

        private static bool _initialized;

        private void Awake()
        {
            LogInstance = Logger;

            if (_initialized)
            {
                Logger.LogInfo("Already initialized, skipping.");
                return;
            }

            _initialized = true;

            try
            {
                BootstrapCarXLoader();
                Logger.LogInfo("CarX loader bootstrap finished.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Bootstrap failed: {ex}");
            }
        }

        private void BootstrapCarXLoader()
        {
            string rootDir = AppDomain.CurrentDomain.BaseDirectory;
            string managedDir = Path.Combine(rootDir, "Taj Mod_Data", "Managed");

            if (!Directory.Exists(managedDir))
                throw new DirectoryNotFoundException($"Managed directory not found: {managedDir}");

            string harmonyPath = Path.Combine(managedDir, "0Harmony.dll");
            string carxApiPath = Path.Combine(managedDir, "CarX.API.dll");

            if (!File.Exists(harmonyPath))
                throw new FileNotFoundException("Cannot find 0Harmony.dll", harmonyPath);

            if (!File.Exists(carxApiPath))
                throw new FileNotFoundException("Cannot find CarX.API.dll", carxApiPath);

            AppDomain.CurrentDomain.AssemblyResolve -= ResolveManagedAssembly;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveManagedAssembly;

            Assembly harmonyAssembly = LoadOrGetAssembly(harmonyPath);
            Assembly carxApiAssembly = LoadOrGetAssembly(carxApiPath);

            Type loaderType = carxApiAssembly.GetType("CarX.Loader.Loader", throwOnError: true);
            MethodInfo runMethod = loaderType.GetMethod(
                "Run",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: new[] { typeof(Assembly[]) },
                modifiers: null);

            if (runMethod == null)
                throw new MissingMethodException("CarX.Loader.Loader", "Run(Assembly[] dependencies = null)");

            Logger.LogInfo($"Resolved Loader type from: {carxApiAssembly.Location}");
            Logger.LogInfo($"Invoking Loader.Run with dependency: {harmonyAssembly.FullName}");

            runMethod.Invoke(null, new object[] { new Assembly[] { harmonyAssembly } });
        }

        private static Assembly LoadOrGetAssembly(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            Assembly loaded = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a =>
                {
                    try
                    {
                        if (a.IsDynamic) return false;
                        return string.Equals(a.GetName().Name, fileName, StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                });

            if (loaded != null)
                return loaded;

            return Assembly.LoadFrom(path);
        }

        private static Assembly? ResolveManagedAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                string rootDir = AppDomain.CurrentDomain.BaseDirectory;
                string managedDir = Path.Combine(rootDir, "Taj Mod_Data", "Managed");

                AssemblyName requestedName = new AssemblyName(args.Name);
                string candidatePath = Path.Combine(managedDir, requestedName.Name + ".dll");

                if (!File.Exists(candidatePath))
                    return null;

                LogInstance?.LogInfo($"Resolving dependency from Managed: {candidatePath}");
                return LoadOrGetAssembly(candidatePath);
            }
            catch (Exception ex)
            {
                LogInstance?.LogError($"AssemblyResolve failed: {ex}");
                return null;
            }
        }
    }
}