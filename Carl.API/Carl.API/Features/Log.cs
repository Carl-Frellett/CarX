using CarX.API.Enums;
using System.Runtime.InteropServices;

namespace CarX.API.Features
{
    public static class Log
    {
        public static void Info(object message) => Send(message, LogLevel.Info, System.ConsoleColor.Cyan);
        public static void Debug(object message, bool canBeSent = true)
        {
            if (canBeSent) Send(message, LogLevel.Debug, System.ConsoleColor.DarkYellow);
        }
        public static void Warn(object message) => Send(message, LogLevel.Warn, System.ConsoleColor.Magenta);
        public static void Error(object message) => Send(message, LogLevel.Error, System.ConsoleColor.DarkRed);

        public static void Send(object message, LogLevel level, System.ConsoleColor color = System.ConsoleColor.Gray)
        {
            string finalMessage = $"[{level.ToString().ToUpper()}] {message}";
            ServerConsole.SendRaw(finalMessage, color);
        }

        public static void SendRaw(object message, System.ConsoleColor color = System.ConsoleColor.Gray)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UnityEngine.Debug.Log(message);
            }
            else
            {
                ServerConsole.SendRaw(message, color);
            }
        }
    }
}