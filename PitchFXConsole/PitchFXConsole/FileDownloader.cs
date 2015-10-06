using System;
using System.Net;
using System.IO;
using log4net;
using log4net.Config;

namespace PitchFXConsole
{
    public class FileDownloader
    {
        private static ILog log = LogManager.GetLogger("PitchFXConsole");
        static FileDownloader()
        {
            XmlConfigurator.Configure();
        }

        public static void Run()
        {
            while (QueueManager.Instance.GetUrlCount() > 0)
            {
                var item = QueueManager.Instance.GetUrlToDownload();

                Console.WriteLine("{0} : {1}", item.GameDate.ToShortDateString(), item.InningsFileURL);

                var gameDir = Path.Combine(item.OutputDir, item.GameID);
                Helpers.VerifyDirectory(gameDir);

                try
                {
                    var client = new WebClient();
                    client.DownloadFile(item.InningsFileURL, string.Format("{0}\\inning_all.xml", gameDir));
                    client.DownloadFile(item.PlayersFileURL, string.Format("{0}\\players.xml", gameDir));
                    item.OutputDir = gameDir;
                    QueueManager.Instance.AddFileToParse(item);
                }
                catch (Exception ex)
                {
                    string err = string.Format("Error in FileDownloader.Run: {0}", ex.ToString());
                    Console.WriteLine(err);
                    log.Error(err);
                }
            }

            QueueManager.Instance.SetFinishedDownloading(true);
        }
    }
}
