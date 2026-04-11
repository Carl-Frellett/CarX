using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

        private static readonly string[] AnsiColorMap = {
            "\u001b[30m", "\u001b[31m", "\u001b[32m", "\u001b[33m",
            "\u001b[34m", "\u001b[35m", "\u001b[36m", "\u001b[37m",
            "\u001b[90m", "\u001b[91m", "\u001b[92m", "\u001b[93m",
            "\u001b[94m", "\u001b[95m", "\u001b[96m", "\u001b[97m"
        };

        private static IntPtr _consoleHandle = IntPtr.Zero;
        private static IntPtr _inputHandle = IntPtr.Zero;

        private static bool _isInitialized = false;
        private static bool _isRunning = false;

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
        private static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten, IntPtr lpReserved);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleTitle(string lpConsoleTitle);

        public static void Initialize()
        {
            if (_isInitialized) return;

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
            }
            catch (Exception ex)
            {
                try { Console.WriteLine($"[ServerConsole Init Error] {ex.Message}"); }
                catch { }
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
                                        string purpleLog = $"[{timestamp}] \u001b[95m> {currentInput}\u001b[0m";
                                        WriteEntryRaw(purpleLog);

                                        string grayLog = $"Try enter the command - \"{currentInput}\"";
                                        SendRawInternal(grayLog, ConsoleColor.DarkGray);

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

        private static void WriteEntry(string content, ConsoleColor color)
        {
            if (!_isInitialized || _consoleHandle == IntPtr.Zero) return;

            try
            {
                string ansiColor = AnsiColorMap[(int)color];
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

                // 日期无颜色，内容有色
                string output = $"[{timestamp}] \u001b[0m{ansiColor}{content}\u001b[0m{Environment.NewLine}";
                WriteConsole(_consoleHandle, output, (uint)output.Length, out _, IntPtr.Zero);
            }
            catch { }
        }

        private static void SendRawInternal(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            string msg = message?.ToString() ?? "null";
            WriteEntry(msg, color);
        }

        private static void WriteEntryRaw(string content)
        {
            if (!_isInitialized || _consoleHandle == IntPtr.Zero) return;
            WriteConsole(_consoleHandle, content + Environment.NewLine, (uint)(content.Length + 2), out _, IntPtr.Zero);
        }

        public static void SendRaw(object message, ConsoleColor color = ConsoleColor.Gray)
        {
            if (!_isInitialized) Initialize();
            string msg = message?.ToString() ?? "null";
            WriteEntry(msg, color);
        }
    }
}