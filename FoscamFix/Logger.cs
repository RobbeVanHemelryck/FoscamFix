using System;
using System.IO;

namespace FoscamFix
{
    public class Logger
    {
        private string _logFile;
        public Logger(string logDir)
        {
            _logFile = Path.Combine(logDir, DateTime.UtcNow.ToString("yyyy-MM-dd dddd") + ".txt");
        }

        public  void Log(string text)
        {
             Console.WriteLine(text);
             Console.WriteLine($"Writing to {_logFile}");
             File.AppendAllText(_logFile, text + Environment.NewLine);
        }
    }
}