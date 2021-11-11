using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FoscamFix
{
    class Program
    {
        static void Main(string[] args)
        {
            //var boomhutSource = "/cameras/boomhut-source";
            //var boomhutDestination = "/cameras/boomhut-destination";
            //var garageSource = "/cameras/garage-source";
            //var garageDestination = "/cameras/garage-destination";
            var boomhutSource = @"C:\Users\Robbe\Documents\Git\FoscamFix\boomhut-source";
            var boomhutDestination = @"C:\Users\Robbe\Documents\Git\FoscamFix\boomhut-destination";
            var garageSource = @"C:\Users\Robbe\Documents\Git\FoscamFix\garage-source";
            var garageDestination = @"C:\Users\Robbe\Documents\Git\FoscamFix\garage-destination";

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
            var sourceFiles = Directory.GetFiles(source)
                .Select(x => new
                {
                    DateString = Path.GetFileNameWithoutExtension(x).Substring(8, 8),
                    FileName = x
                })
                .GroupBy(x => x.DateString)
                .ToList();

            foreach (var group in sourceFiles)
            {
                var date = DateTime.ParseExact(group.Key, "yyyyMMdd", CultureInfo.InvariantCulture);
                var groupDestination = Path.Combine(destination, date.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(groupDestination);

                foreach (var file in group)
                {
                    var destinationFileName = Path.Combine(groupDestination, Path.GetFileName(file.FileName).Substring(8));
                    File.Move(file.FileName, destinationFileName);
                }
            }
        }
    }
}