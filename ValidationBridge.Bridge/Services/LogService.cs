using System;

namespace ValidationBridge.Bridge.Services
{
    public class LogService
    {
        public bool IsEnabled { get; set; } = true;

        public void LogError(string message)
        {
            Log(message, ConsoleColor.Red);
        }

        public void LogWarning(string message)
        {
            Log(message, ConsoleColor.Yellow);
        }

        public void LogInfo(string message)
        {
            Log(message);
        }

        private void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
            if (IsEnabled)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}]\t {message}");
            }
        }
    }
}
