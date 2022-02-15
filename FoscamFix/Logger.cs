using System;
using System.IO;

namespace FoscamFix
{
    public class Logger
    {
        private string _logDir;
        private string _logFile;
        private DateTime _lastDate = DateTime.Today;
        
        public Logger(string logDir)
        {
            _logDir = logDir;
            UpdateLogFile();
        }

        public void Log(string text)
        {
             Console.WriteLine(text);

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