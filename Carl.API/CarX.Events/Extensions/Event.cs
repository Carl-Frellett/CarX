namespace CarX.Events.Extensions
{
    using System;

    using CarX.API.Features;

    public static class Event
    {
        public static void InvokeSafely<T>(this Plugin.CustomEventHandler<T> ev, T arg)
            where T : EventArgs
        {
            if (ev == null)
                return;

            var eventName = ev.GetType().FullName;
            foreach (Plugin.CustomEventHandler <T> handler in ev.GetInvocationList())
            {
                try
                {
                    handler(arg);
                }
                catch (Exception ex)
                {
                    LogException(ex, handler.Method.Name, handler.Method.ReflectedType.FullName, eventName);
                }
            }
        }

        public static void InvokeSafely(this Plugin.CustomEventHandler ev)
        {
            if (ev == null)
                return;

            string eventName = ev.GetType().FullName;
            foreach (Plugin.CustomEventHandler handler in ev.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    LogException(ex, handler.Method.Name, handler.Method.ReflectedType.FullName, eventName);
                }
            }
        }

        private static void LogException(Exception ex, string methodName, string sourceClassName, string eventName)
        {
            Log.Error($"Method \"{methodName}\" of the class \"{sourceClassName}\" caused an exception when handling the event \"{eventName}\"");
            Log.Error(ex);
        }
    }
}