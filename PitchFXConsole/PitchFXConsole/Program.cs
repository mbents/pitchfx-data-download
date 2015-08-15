using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PitchFXConsole
{
    class Program
    {
        private static List<string> gameIDs = new List<string>();
        private static List<string> innings = new List<string>();

        private static DateTime? StartDate = null, EndDate = null;
        private static bool DownloadFiles = false, ParseFiles = false, ShowHelp = false;
        private static string outputDir = @"C:\pitchfx";

        static void Main(string[] args)
        {
            ReadArgs(args);
            if (ShowHelp)
                DisplayHelpOptions();
            else
            {
                Setup();

                var taskList = new List<Task>();
                // TODO: implement DownloadFiles
                //if (DownloadFiles)
                {
                    var task = new Task(() => FileDownloader.Run());
                    task.Start();
                    taskList.Add(task);
                }

                // TODO: implement ParseFiles
                //if (ParseFiles)
                {
                    var task = new Task(() => FileParser.Run());
                    task.Start();
                    taskList.Add(task);
                }

                if (taskList.Count > 0)
                    Task.WaitAll(taskList.ToArray());
            }
        }

        private static void ReadArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                    case "-help":
                        ShowHelp = true;
                        break;
                    case "-startdate":
                        StartDate = Convert.ToDateTime(args[i + 1]);
                        break;
                    case "-enddate":
                        EndDate = Convert.ToDateTime(args[i + 1]);
                        break;
                    case "-d":
                    case "-download":
                        DownloadFiles = true;
                        break;
                    case "-p":
                    case "-parse":
                        ParseFiles = true;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Sets StartDate and EndDate for collection, ensures directories exist for XML download, 
        /// and populates queue with URLs for XML download
        /// </summary>
        private static void Setup()
        {
            if (!StartDate.HasValue && !EndDate.HasValue)
                StartDate = EndDate = DateTime.Now.AddDays(-1);
            else if (!StartDate.HasValue)
                StartDate = new DateTime(EndDate.Value.Year, 3, 1);
            else if (!EndDate.HasValue)
                EndDate = new DateTime(StartDate.Value.Year, 11, 30);

            var _dates = Helpers.GetDays(StartDate.Value, EndDate.Value);
            foreach (var date in _dates)
            {
                var _path = System.IO.Path.Combine(outputDir, date.Year.ToString(), date.Month.ToString(), date.Day.ToString());
                Helpers.VerifyDirectory(_path);
            }

            GameUrlCollector collector = new GameUrlCollector { Dates = _dates };
            collector.Run();
        }

        private static void DisplayHelpOptions()
        {
            var helpMessage = new StringBuilder();

            helpMessage.AppendLine("Usage: PitchFxDataDownload [options] directory");
            helpMessage.AppendLine("options:");
            helpMessage.AppendLine(" -h or -help         Show this help screen");
            helpMessage.AppendLine(" -d or -download     Download XML files.");
            helpMessage.AppendLine(" -p or -parse        Parse XML files.");
            helpMessage.AppendLine(" -startdate          Set date from which download will begin.");
            helpMessage.AppendLine(" -enddate            Set date at which download will end. ");
            helpMessage.AppendLine("Format for dates should be something like: 4-15-2000 or 10-2-1997.");
            helpMessage.AppendLine("If neither startdate nor enddate is provided, both will default to yesterday's date.");
            helpMessage.AppendLine("If only startdate is provided, enddate will default to 11/30 of startdate's year.");
            helpMessage.AppendLine("If only enddate is provided, startdate will default to 3/1 of enddate's year.");
            helpMessage.AppendLine("Default value for download and parse arguments is false.");

            Console.WriteLine(helpMessage.ToString());
        }

    }
}
