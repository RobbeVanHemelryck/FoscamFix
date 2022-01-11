using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace FoscamFix
{
    class Program
    {
        private static Logger _logger = new Logger("/logs");
        static void Main(string[] args)
        {
            var boomhutSource = "/cameras/boomhut-source";
            var boomhutDestination = "/cameras/boomhut-destination";
            var garageSource = "/cameras/garage-source";
            var garageDestination = "/cameras/garage-destination";

            var boomhutSourceSnap = Path.Combine(boomhutSource, "snap");
            var boomhutDestinationSnap = Path.Combine(boomhutDestination, "Snaps");
            var garageSourceSnap = Path.Combine(garageSource, "snap");
            var garageDestinationSnap = Path.Combine(garageDestination, "Snaps");

            var boomhutSourceRecordings = Path.Combine(boomhutSource, "record");
            var boomhutDestinationRecordings = Path.Combine(boomhutDestination, "Videos");
            var garageSourceRecordings = Path.Combine(garageSource, "record");
            var garageDestinationRecordings = Path.Combine(garageDestination, "Videos");

            MoveFiles(boomhutSourceSnap, boomhutDestinationSnap);
            MoveFiles(boomhutSourceRecordings, boomhutDestinationRecordings);
            MoveFiles(garageSourceSnap, garageDestinationSnap);
            MoveFiles(garageSourceRecordings, garageDestinationRecordings);
        }

        private static void MoveFiles(
            string source,
            string destination)
        {
            try
            {
                if (!Directory.Exists(source))
                {
                    _logger.Log($"{source} does not exist. Skipped.");
                    return;
                }

                var sourceFiles = Directory.GetFiles(source)
                    .Select(x => new
                    {
                        DateString = TrimFoscamPrefix(Path.GetFileNameWithoutExtension(x)).Substring(0, "yyyyMMdd".Length),
                        FileName = x
                    })
                    .GroupBy(x => x.DateString)
                    .ToList();

                foreach (var group in sourceFiles)
                {
                    _logger.Log($"Moving files from {group.Key}");
                    var date = DateTime.ParseExact(group.Key, "yyyyMMdd", CultureInfo.InvariantCulture);
                    var groupDestination = Path.Combine(destination, date.ToString("yyyy-MM-dd"));
                    Directory.CreateDirectory(groupDestination);

                    foreach (var file in group)
                    {
                        try
                        {
                            var destinationFileName = Path.Combine(
                                groupDestination,
                                TrimFoscamPrefix(Path.GetFileName(file.FileName)));

                            _logger.Log($"Moving {file.FileName} to {destinationFileName}");
                            File.Move(file.FileName, destinationFileName, true);
                        }
                        catch (Exception e)
                        {
                            _logger.Log(e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log(e.Message);
            }
        }
        
        private static string TrimFoscamPrefix(string fileName)
        {
            return fileName
                .Replace("MDAlarm_", "")
                .Replace("MDalarm_", "")
                .Replace("Schedule_", "");
        }
    }
}