using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using log4net;
using log4net.Config;

namespace PitchFXConsole
{
    public abstract class CollectorBase
    {
        private static ILog log = LogManager.GetLogger("PitchFXConsole");

        protected static List<string> coll;

        public IEnumerable<DateTime> Dates { get; set; }

        protected static string BaseURL 
        {
            get { return "http://gd2.mlb.com/components/game/mlb/year_{0}/month_{1}/day_{2}/"; }
        }

        protected static string BaseOutputDir
        {
            get { return @"C:\pitchfx"; }
        }

        public virtual void Run()
        {
            throw new NotImplementedException();
        }

        public static void PopulateCollection(string url, string startsWith, DateTime date)
        {
            XmlConfigurator.Configure();

            try
            {
                Uri uri = new Uri(url);
                WebClient wc = new WebClient();
                string htmlString = wc.DownloadString(uri);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlString);
                FindHtml(doc.DocumentNode.ChildNodes, startsWith, date);
            }
            catch (WebException wex)
            {
                // probably just a 404 error - should maybe do something better than just swallow it
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error in PopulateCollection for url {1}: {0}", ex.ToString(), url));
            }
        }

        public static void FindHtml(HtmlNodeCollection nodes, string startsWith, DateTime date)
        {
            foreach (var node in nodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (node.Name == "ul")
                    {
                        foreach (var gameNode in node.ChildNodes.Where(x => x.InnerText.Trim().StartsWith(startsWith)))
                        {
                            if (startsWith == "gid_")
                            {
                                string _url = string.Format("{0}{1}inning/inning_all.xml", string.Format(BaseURL, date.Year, date.Month < 10 ? "0" + date.Month : date.Month.ToString(),
                                    date.Day < 10 ? "0" + date.Day : date.Day.ToString()), gameNode.InnerText.Trim());
                                QueueManager.Instance.AddUrlToDownload(new QueueItem 
                                    { 
                                        FileURL = _url, 
                                        GameDate = date,
                                        GameID = gameNode.InnerText.Trim().TrimEnd('/'),
                                        OutputDir = System.IO.Path.Combine(BaseOutputDir, date.Year.ToString(), date.Month.ToString(),
                                            date.Day.ToString())
                                    });
                            }
                            else if (startsWith == "inning_")
                            {
                                var inningstring = gameNode.InnerText.Trim();
                                var sub1 = inningstring.Substring(inningstring.IndexOf("_") + 1);
                                var sub2 = sub1.Substring(0, sub1.IndexOf("."));
                                int inning = -1;
                                if (int.TryParse(sub2, out inning))
                                    coll.Add(inningstring);
                            }
                        }
                    }
                    else
                    {
                        FindHtml(node.ChildNodes, startsWith, date);
                    }
                }
            }
        }
    }
}
