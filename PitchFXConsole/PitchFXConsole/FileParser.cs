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
                    var xmlString = File.ReadAllText(string.Format("{0}\\inning_all.xml", item.OutputDir));
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlString);
                    doc.ParseXml(item.GameDate, item.GameID);
                }
            }
        }
    }
}
