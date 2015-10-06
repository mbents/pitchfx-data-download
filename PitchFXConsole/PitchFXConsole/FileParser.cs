using System;
using System.Threading;
using System.IO;
using System.Xml;

namespace PitchFXConsole
{
    public class FileParser
    {
        public static void Run()
        {
            while (true)
            {
                Thread.Sleep(2000);
                var item = QueueManager.Instance.GetFileToParse();
                if (item == null && QueueManager.Instance.IsFinishedDownloading())
                    break;
                else if (item != null)
                {
                    ParsePitchFile(item);
                    ParsePlayerFile(item);
                }
            }
        }

        private static void ParsePitchFile(QueueItem item)
        {
            var xmlString = File.ReadAllText(string.Format("{0}\\inning_all.xml", item.OutputDir));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            doc.ParsePitchXml(item.GameDate, item.GameID);
        }

        private static void ParsePlayerFile(QueueItem item)
        {
            var xmlString = File.ReadAllText(string.Format("{0}\\players.xml", item.OutputDir));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            doc.ParsePlayerXml(item.GameDate, item.GameID);
        }
    }
}
