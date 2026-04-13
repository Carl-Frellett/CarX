using HarmonyLib;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace CarX.API
{
    public static class ServerConsole
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_INPUT_HANDLE = -10;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        private const uint ENABLE_LINE_INPUT = 0x0002;
        private const uint ENABLE_ECHO_INPUT = 0x0004;
        private const int KEY_EVENT = 0x0001;

        private static IntPtr _consoleHandle = IntPtr.Zero;
        private static IntPtr _inputHandle = IntPtr.Zero;

        private static bool _isInitialized = false;
        private static bool _isRunning = false;

        private static readonly Regex LogTypeRegex = new Regex(
    @"LOGTYPE\s*[:=]?\s*(-?\d+)",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public delegate void CommandReceivedHandler(string command);
        public static event CommandReceivedHandler OnCommandReceived;

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT_RECORD
        {
            public ushort EventType;
            public KEY_EVENT_RECORD KeyEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEY_EVENT_RECORD
        {
            public bool bKeyDown;
            public ushort wRepeatCount;
            public ushort wVirtualKeyCode;
            public ushort wVirtualScanCode;
            public char UnicodeChar;
            public uint dwControlKeyState;
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleOutputCP(uint wCodePageID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCP(uint wCodePageID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadConsoleInput(
            IntPtr hConsoleInput,
            [Out] INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteConsole(
            IntPtr hConsoleOutput,
            string lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleTitle(string lpConsoleTitle);

        private static Harmony _harmony;

        public static void Initialize()
        {
            if (_isInitialized) return;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Debug.Log("You are using a non-Windows system to load the CarX framework.");
                return;
            }

            try
            {
                AllocConsole();

                _consoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                _inputHandle = GetStdHandle(STD_INPUT_HANDLE);

                SetConsoleOutputCP(65001);
                SetConsoleCP(65001);
                SetConsoleTitle("Taj Mod Server Console");

                if (GetConsoleMode(_consoleHandle, out uint mode))
                {
                    mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                    SetConsoleMode(_consoleHandle, mode);
                }

                if (GetConsoleMode(_inputHandle, out uint inputMode))
                {
                    inputMode &= ~(ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT);
                    SetConsoleMode(_inputHandle, inputMode);
                }

                _isInitialized = true;
                _isRunning = true;

                Task.Run(InputLoop);

                _harmony = new Harmony("carx.api");
                _harmony.PatchAll();
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine($"[ServerConsole Init Error] {ex.Message}");
                }
                catch
                {
                }
            }
        }

        private static async Task InputLoop()
        {
            INPUT_RECORD[] buffer = new INPUT_RECORD[128];
            string currentInput = "";

            while (_isRunning)
            {
                try
                {
                    if (ReadConsoleInput(_inputHandle, buffer, (uint)buffer.Length, out uint eventsRead))
                    {
                        for (int i = 0; i < eventsRead; i++)
                        {
                            if (buffer[i].EventType == KEY_EVENT && buffer[i].KeyEvent.bKeyDown)
                            {
                                char keyChar = buffer[i].KeyEvent.UnicodeChar;

                                if (keyChar == '\r' || keyChar == '\n')
                                {
                                    if (!string.IsNullOrEmpty(currentInput))
                                    {
                                        WriteConsole(_consoleHandle, Environment.NewLine, 2, out _, IntPtr.Zero);

                                        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                                        string purpleLog = $"[{timestamp}] \u001b[38;2;199;13;206m> {EscapeAnsi(currentInput)}\u001b[0m";
                                        WriteEntryRaw(purpleLog);

                                        string grayLog = $"Try enter the command - \"{currentInput}\"";
                                        SendRawInternal(grayLog, "#5b5b5b");

                                        OnCommandReceived?.Invoke(currentInput);
                                    }

                                    currentInput = "";
                                }
                                else if (keyChar == '\b')
                                {
                                    if (currentInput.Length > 0)
                                    {
                                        currentInput = currentInput.Substring(0, currentInput.Length - 1);
                                        WriteConsole(_consoleHandle, "\b \b", 3, out _, IntPtr.Zero);
                                    }
                                }
                                else if (keyChar >= 32)
                                {
                                    currentInput += keyChar;
                                    WriteConsole(_consoleHandle, keyChar.ToString(), 1, out _, IntPtr.Zero);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    await Task.Delay(10);
                }
            }
        }

        public static void SendRaw(object message)
        {
            if (!_isInitialized) Initialize();

            string msg = message?.ToString() ?? "null";
            WriteProcessedEntry(msg);
        }

        public static void SendRaw(object message, ConsoleColor color)
        {
            if (!_isInitialized) Initialize();

            string msg = message?.ToString() ?? "null";
            string hex = ConsoleColorToHex(color);
            WriteEntry(msg, hex);
        }

        public static void SendGameLog(object message)
        {
            if (!_isInitialized) Initialize();

            string msg = message?.ToString() ?? "null";
            WriteProcessedEntry(msg);
        }

        private static void WriteProcessedEntry(string rawMessage)
        {
            string cleanMessage = StripLogType(rawMessage).TrimEnd('\r', '\n');
            string colorHex = GetColorFromMessage(rawMessage);

            WriteEntry(cleanMessage, colorHex);
        }

        private static void WriteEntry(string content, string hexColor)
        {
            if (!_isInitialized || _consoleHandle == IntPtr.Zero) return;

            try
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string ansiColor = HexToAnsi(hexColor);
                string safeContent = EscapeAnsi(content);

                string output = $"[{timestamp}] {ansiColor}{safeContent}\u001b[0m{Environment.NewLine}";
                WriteConsole(_consoleHandle, output, (uint)output.Length, out _, IntPtr.Zero);
            }
            catch
            {
            }
        }

        private static void SendRawInternal(string message, string hexColor)
        {
            string msg = message?.ToString() ?? "null";
            WriteEntry(msg, hexColor);
        }

        private static void WriteEntryRaw(string content)
        {
            if (!_isInitialized || _consoleHandle == IntPtr.Zero) return;

            WriteConsole(
                _consoleHandle,
                content + Environment.NewLine,
                (uint)(content.Length + Environment.NewLine.Length),
                out _,
                IntPtr.Zero);
        }

        private static string StripLogType(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            return LogTypeRegex.Replace(text, "").Trim();
        }

        private static string GetColorFromMessage(string text)
        {
            int num = 3;

            if (!string.IsNullOrEmpty(text) && text.Contains("LOGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Match match = LogTypeRegex.Match(text);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int parsed))
                    {
                        num = parsed;
                    }
                }
                catch
                {
                    num = 3;
                }
            }

            switch (num)
            {
                case 0: return "#000000";
                case 1: return "#183487";
                case 2: return "#0b7011";
                case 3: return "#0a706c";
                case 4: return "#700a0a";
                case 5: return "#5b0a40";
                case 6: return "#aaa800";
                case 7: return "#afafaf";
                case 8: return "#5b5b5b";
                case 9: return "#0055ff";
                case 10: return "#10ce1a";
                case 11: return "#0fc7ce";
                case 12: return "#ce0e0e";
                case 13: return "#c70dce";
                case 14: return "#ffff07";
                case 15: return "#e0e0e0";
                default: return "#ffffff";
            }
        }

        private static string HexToAnsi(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return "\u001b[37m";

            string value = hex.Trim().TrimStart('#');

            if (value.Length == 3)
            {
                value = string.Concat(
                    value[0], value[0],
                    value[1], value[1],
                    value[2], value[2]);
            }

            if (value.Length != 6)
                return "\u001b[37m";

            if (!int.TryParse(value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int r))
                return "\u001b[37m";
            if (!int.TryParse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int g))
                return "\u001b[37m";
            if (!int.TryParse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int b))
                return "\u001b[37m";

            return $"\u001b[38;2;{r};{g};{b}m";
        }

        private static string ConsoleColorToHex(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => "#000000",
                ConsoleColor.DarkBlue => "#00008B",
                ConsoleColor.DarkGreen => "#006400",
                ConsoleColor.DarkCyan => "#008B8B",
                ConsoleColor.DarkRed => "#8B0000",
                ConsoleColor.DarkMagenta => "#8B008B",
                ConsoleColor.DarkYellow => "#B8860B",
                ConsoleColor.Gray => "#C0C0C0",
                ConsoleColor.DarkGray => "#5b5b5b",
                ConsoleColor.Blue => "#0055ff",
                ConsoleColor.Green => "#10ce1a",
                ConsoleColor.Cyan => "#0fc7ce",
                ConsoleColor.Red => "#ce0e0e",
                ConsoleColor.Magenta => "#c70dce",
                ConsoleColor.Yellow => "#ffff07",
                ConsoleColor.White => "#ffffff",
                _ => "#ffffff"
            };
        }

        private static string EscapeAnsi(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace("\u001b", string.Empty);
        }
    }
}