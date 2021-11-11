//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Reflection;
//using System.Timers;

//namespace FoscamFix
//{
//    class Program2
//    {
//        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
//        {
//            List<T> objects = new List<T>();
//            foreach (Type type in
//                Assembly.GetAssembly(typeof(T)).GetTypes()
//                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
//            {
//                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
//            }
//            return objects;
//        }

//        static void Main(string[] args)
//        {
//            GroupFiles();
//            //DeleteOldFiles();
//        }

//        private static void GroupFiles()
//        {
//            int totalCounter = 0;
//            Stopwatch timer = new Stopwatch();
//            Stopwatch groupTimer = new Stopwatch();
//            timer.Start();

//            //Create logging file
//            string logPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Logs", $"{DateTime.Now.ToString("yyyy-MM-dd HHumm")}.txt");
//            using (TextWriter log = File.CreateText(logPath))
//            {
//                try
//                {
//                    timer.Start();
//                    List<FileGrouper> fileGroupers = GetFileGroupers();

//                    //Execute the grouping for each grouper
//                    foreach (FileGrouper fileGrouper in fileGroupers)
//                    {
//                        int counter = 0;
//                        try
//                        {
//                            log.WriteLine($"---------------------------------------- {fileGrouper.Name} ----------------------------------------");
//                            groupTimer.Restart();

//                            //Get all relevant files from the source directory
//                            IEnumerable<string> sourceFiles = Directory.GetFiles(fileGrouper.Source).Where(x => fileGrouper.IsRelevantFile(x));

//                            //Group files per DateTime, which is parsed from the file name
//                            var fileGroups = sourceFiles.GroupBy(x => fileGrouper.GetDateFromFileName(x)).ToList();

//                            //Remove files from today
//                            fileGroups.RemoveAll(x => x.Key.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"));

//                            //Create a folder in the destination per group and move the files over there
//                            foreach (var group in fileGroups)
//                            {
//                                string groupDestination = Path.Combine(fileGrouper.Destination, group.Key.ToString("yyyy-MM-dd"));
//                                if (Directory.Exists(groupDestination)) groupDestination += Guid.NewGuid();

//                                Directory.CreateDirectory(groupDestination);
//                                foreach (string file in group)
//                                {
//                                    counter++;
//                                    string destination = Path.Combine(groupDestination, Path.GetFileName(file));
//                                    File.Move(file, destination);
//                                    log.WriteLine($"{counter,6} {file}   ->   {destination}");
//                                }
//                            }
//                        }
//                        catch (Exception e)
//                        {
//                            log.WriteLine($"An exception occured: {e.Message}");
//                            log.WriteLine();
//                        }
//                        finally
//                        {
//                            totalCounter += counter;
//                            log.WriteLine($"Processed {counter} files in {groupTimer.Elapsed.Seconds}.{groupTimer.Elapsed.Milliseconds}s");
//                            log.WriteLine();
//                            log.WriteLine();
//                        }
//                    }
//                    log.WriteLine($"Processed {totalCounter} total files in {timer.Elapsed.Seconds}.{timer.Elapsed.Milliseconds}s");
//                    timer.Stop();
//                    groupTimer.Stop();
//                }
//                catch (Exception e)
//                {
//                    //Log the exception
//                    log.WriteLine();
//                    log.WriteLine();
//                    log.WriteLine($"An exception occured: {e.Message}");
//                    log.WriteLine();
//                    log.WriteLine();
//                    log.WriteLine($"Processed {totalCounter} total files in {timer.Elapsed.Seconds}.{timer.Elapsed.Milliseconds}s");
//                    timer.Stop();
//                    groupTimer.Stop();
//                }
//            }
//        }

//        private static void DeleteOldFiles()
//        {
//            int totalCounter = 0;
//            double totalFreedBytes = 0;
//            Stopwatch timer = new Stopwatch();
//            Stopwatch groupTimer = new Stopwatch();

//            //Create logging file
//            string logPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Logs", $"{DateTime.Now.ToString("yyyy-MM-dd HHumm")} - Delete.txt");
//            using (TextWriter log = File.CreateText(logPath))
//            {
//                try
//                {
//                    timer.Start();
//                    List<FileGrouper> fileGroupers = GetFileGroupers().Where(x => x.IsDecimatable).ToList();

//                    //Execute the deleting for each grouper
//                    foreach (FileGrouper fileGrouper in fileGroupers)
//                    {
//                        int counter = 0;
//                        double freedBytes = 0;

//                        log.WriteLine($"---------------------------------------- {fileGrouper.Name} ----------------------------------------");
//                        groupTimer.Restart();

//                        DecimateFolders(log, fileGrouper.Destination, 3, 6, 4, ref counter, ref freedBytes);
//                        DecimateFolders(log, fileGrouper.Destination, 6, 9, 8, ref counter, ref freedBytes);
//                        DecimateFolders(log, fileGrouper.Destination, 9, 9999, 16, ref counter, ref freedBytes);
//                        totalCounter += counter;
//                        totalFreedBytes += freedBytes;

//                        log.WriteLine($"Deleted {counter} files ({Math.Round(freedBytes / 1024 / 1024, 2)} MB) in {groupTimer.Elapsed.Seconds}.{groupTimer.Elapsed.Milliseconds}s");
//                        log.WriteLine();
//                        log.WriteLine();
//                    }
//                    log.WriteLine($"Deleted {totalCounter} total files ({Math.Round(totalFreedBytes / 1024 / 1024, 2)} MB) in {timer.Elapsed.Seconds}.{timer.Elapsed.Milliseconds}s");
//                    timer.Stop();
//                    groupTimer.Stop();
//                }
//                catch (Exception e)
//                {
//                    //Log the exception
//                    log.WriteLine();
//                    log.WriteLine();
//                    log.WriteLine($"An exception occured: {e.Message}");
//                    log.WriteLine();
//                    log.WriteLine();
//                    log.WriteLine($"Deleted {totalCounter} total files ({Math.Round(totalFreedBytes / 1024 / 1024, 2)} MB) in {timer.Elapsed.Seconds}.{timer.Elapsed.Milliseconds}s");
//                    timer.Stop();
//                    groupTimer.Stop();
//                }
//            }
//        }

//        private static List<FileGrouper> GetFileGroupers()
//        {
//            //Create all instances of the fileGroupers
//            List<FileGrouper> fileGroupClasses = GetEnumerableOfType<FileGrouper>().ToList();

//            //Populate the fileGroupers with the JSON data
//            List<FileGrouper> fileGroupers = new List<FileGrouper>();
//            using (StreamReader reader = new StreamReader(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "settings.json")))
//            {
//                IEnumerable<dynamic> json = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(reader.ReadToEnd());
//                foreach (var fgJson in json)
//                {
//                    FileGrouper fg = fileGroupClasses.Single(x => x.AppliesTo(fgJson.name.ToString()));
//                    fg.Source = fgJson.source;
//                    fg.Destination = fgJson.destination;
//                    fg.IsDecimatable = fgJson.isDecimatable == 1 ? true : false;
//                    fileGroupers.Add(fg);
//                }
//            }

//            return fileGroupers;
//        }

//        private static void DecimateFolders(TextWriter log, string rootFolder, int minMonths, int maxMonths, int decimateAmount, ref int totalDeleted, ref double totalSize)
//        {
//            var foldersToDecimate = Directory.GetDirectories(rootFolder)
//                .Where(x =>
//                {
//                    try
//                    {
//                        double totalDays = DateTime.Now.Subtract(DateTime.Parse(x.Remove(0, x.LastIndexOf('\\') + 1))).TotalDays;
//                        return totalDays >= minMonths * 30 && totalDays < maxMonths * 30;
//                    }
//                    catch
//                    {
//                        return false;
//                    }
//                });

//            foreach (string folder in foldersToDecimate)
//            {
//                Console.WriteLine("Decimating: " + folder);
//                try
//                {
//                    string indicatorPath = Path.Combine(folder, $"decimation indicator");
//                    int previousDecimator = 1;
//                    if (File.Exists(indicatorPath)) previousDecimator = int.Parse(File.ReadAllText(indicatorPath));

//                    int adjustedDecimateAmount = decimateAmount / previousDecimator;
//                    if (adjustedDecimateAmount == 1) continue;
//                    var files = Directory.GetFiles(folder);

//                    if (files.Length < 2701) continue;

//                    //Delete all files except the n-th (adjustedDecimateAmount) ones
//                    for (int i = 0; i < files.Length; i++)
//                    {
//                        if (i % adjustedDecimateAmount > 0)
//                        {
//                            string filePath = files[i];
//                            totalDeleted++;
//                            totalSize += new FileInfo(filePath).Length;

//                            File.Delete(filePath);
//                        }
//                    }

//                    //Create decimation indicator file
//                    File.WriteAllText(indicatorPath, string.Empty);
//                    File.WriteAllText(indicatorPath, decimateAmount.ToString());
//                    File.SetAttributes(indicatorPath, File.GetAttributes(indicatorPath) | FileAttributes.Hidden);

//                    string text = $"Decimated folder {folder} - removed {adjustedDecimateAmount - 1} out of {adjustedDecimateAmount} files";
//                    if (adjustedDecimateAmount > 1) text += $" ({previousDecimator - 1} out of {previousDecimator} were already removed)";
//                    log.WriteLine(text);
//                }
//                catch
//                {
//                }
//            }
//        }
//    }
//}