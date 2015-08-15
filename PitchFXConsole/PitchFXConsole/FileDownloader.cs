using System;
using System.Net;
using System.IO;

namespace PitchFXConsole
{
    public class FileDownloader
    {
        public static void Run()
        {
            while (QueueManager.Instance.GetUrlCount() > 0)
            {
                var item = QueueManager.Instance.GetUrlToDownload();

                Console.WriteLine("{0} : {1}", item.GameDate.ToShortDateString(), item.FileURL);

                var gameDir = Path.Combine(item.OutputDir, item.GameID);
                Helpers.VerifyDirectory(gameDir);
                
                var client = new WebClient();
                client.DownloadFile(item.FileURL, string.Format("{0}\\inning_all.xml", gameDir));
                item.OutputDir = gameDir;
                QueueManager.Instance.AddFileToParse(item);
            }

            QueueManager.Instance.SetFinishedDownloading(true);
        }
    }
}
