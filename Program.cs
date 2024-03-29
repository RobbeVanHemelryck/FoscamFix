﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;

namespace FoscamFix
{
    class Program
    {
        private static Logger _logger = new Logger("/logs");

        static async Task Main(string[] args)
        {
            RecurringJob.AddOrUpdate(() => Run(), "0 1 * * *");

            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }
        
        public static async Task Run()
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

            while (true)
            {
                var tasks = new List<Task>();
                
                if(bool.Parse(Environment.GetEnvironmentVariable("MoveBoomhutSnap")))
                    tasks.Add(Task.Run(() => MoveFiles(boomhutSourceSnap, boomhutDestinationSnap)));
                
                if(bool.Parse(Environment.GetEnvironmentVariable("MoveBoomhutRecording")))
                    tasks.Add(Task.Run(() => MoveFiles(boomhutSourceRecordings, boomhutDestinationRecordings)));
                
                if(bool.Parse(Environment.GetEnvironmentVariable("MoveGarageSnap")))
                    tasks.Add(Task.Run(() => MoveFiles(garageSourceSnap, garageDestinationSnap)));
                
                if(bool.Parse(Environment.GetEnvironmentVariable("MoveGarageRecording")))
                    tasks.Add(Task.Run(() => MoveFiles(garageSourceRecordings, garageDestinationRecordings)));

                await Task.WhenAll(tasks);
            }
        }

        private static HashSet<string> _blacklist = new();

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

                var allFiles = Directory.GetFiles(source);
                var sourceFiles = allFiles
                    .Select(x => new
                    {
                        DateString = TrimFoscamPrefix(Path.GetFileNameWithoutExtension(x)).Substring(0, "yyyyMMdd".Length),
                        FileName = x
                    })
                    .Where(x => !_blacklist.Contains(x.FileName))
                    .GroupBy(x => x.DateString)
                    .ToList();
                
                _logger.Log($"Moving files from '{source}': {allFiles.Length} total for {source.Length} different days");

                foreach (var group in sourceFiles)
                {
                    try
                    {
                        _logger.Log($"Moving files from {group.Key}");
                        var date = DateTime.ParseExact(group.Key, "yyyyMMdd", CultureInfo.InvariantCulture);
                        var groupDestination = Path.Combine(destination, date.ToString("yyyy-MM-dd"));
                        Directory.CreateDirectory(groupDestination);

                        Parallel.ForEach(group, x =>
                        {
                            try
                            {
                                var destinationFileName = Path.Combine(
                                    groupDestination,
                                    TrimFoscamPrefix(Path.GetFileName(x.FileName)));

                                _logger.Log(
                                    $"Moving {Path.GetFileName(x.FileName)} to {Path.GetFileName(destinationFileName)}");
                                File.Move(x.FileName, destinationFileName, true);
                            }
                            catch (Exception e)
                            {
                                _blacklist.Add(x.FileName);
                                _logger.Log(e.Message);
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        foreach (var file in group)
                            _blacklist.Add(file.FileName);

                        _logger.Log(e.Message);
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