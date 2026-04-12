using CarX.API.Features;
using HarmonyLib;

namespace CarX.Events
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Exiled Team & Carl Frellett";
        public override string Name => "CarX.Events";

        private Harmony? harmony;

        public delegate void CustomEventHandler<TEventArgs>(TEventArgs ev)
            where TEventArgs : System.EventArgs;

        public delegate void CustomEventHandler();

        public override void OnEnabled()
        {
            base.OnEnabled();
            harmony = new Harmony("carx.events");
            harmony.PatchAll();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            harmony?.UnpatchAll();
        }
    }
}
