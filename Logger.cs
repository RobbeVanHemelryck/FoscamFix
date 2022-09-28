using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace FoscamFix
{
    public class Logger
    {
        private bool _logToFile;
        private string _logDir;
        private string _logFile;
        private DateTime _lastDate = DateTime.Today;
        
        public Logger(string logDir)
        {
            _logToFile = bool.Parse(Environment.GetEnvironmentVariable("LogToFile"));
            _logDir = logDir;
            UpdateLogFile();
        }

        public void Log(string text)
        {
             Console.WriteLine(text);

             if (!_logToFile)
                 return;
             
             if (DateTime.Today != _lastDate)
             {
                 UpdateLogFile();
                 _lastDate = DateTime.Today;
             }
             File.AppendAllText(_logFile, text + Environment.NewLine);
        }
        
        private void UpdateLogFile()
        => _logFile = Path.Combine(_logDir, DateTime.UtcNow.ToString("yyyy-MM-dd dddd") + ".txt");
    }
}