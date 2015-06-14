using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using HtmlAgilityPack;
using PitchFX.DAL;
using log4net;
using log4net.Config;

namespace PitchFXConsole
{
    class Program
    {
        private static List<string> gameIDs = new List<string>();
        private static List<string> innings = new List<string>();

        private static ILog log = LogManager.GetLogger("PitchFXConsole");

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            Game game;
            AtBat atBat;
            Pitch pitch;
            string inningHalf, inningXmlURL;
            int dbGameID = -1, dbAtBatID = -1, dbPitchID = -1;
            int year = 2015, month = 3, day = 1;

            for (year = 2015; year >= 2008; year--)
            {
                for (month = 3; month <= 11; month++)
                {
                    for (day = 1; day <= 31; day++)
                    {
                        using (var dbContext = new Model1Container())
                        {
                            //    month & day should start with a 0 if it's less than 10
                            string baseURL = string.Format("http://gd2.mlb.com/components/game/mlb/year_{0}/month_{1}/day_{2}/",
                                year, month < 10 ? "0" + month : month.ToString(), day < 10 ? "0" + day : day.ToString());

                            gameIDs = new List<string>();
                            PopulateCollection(baseURL, "gid_");

                            foreach (var gameID in gameIDs)
                            {
                                game = new Game
                                {
                                    Description = gameID,
                                    GameDate = new DateTime(year, month, day)
                                };
                                dbContext.Games.AddObject(game);
                                try
                                {
                                    dbContext.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    string err = string.Format("Error saving gameID {0}: {1}", gameID, ex.ToString());
                                    Console.WriteLine(err);
                                    log.Error(err);
                                }
                                dbGameID = game.Id;

                                try
                                {
                                    PlayerStaging player;
                                    var xmlDoc = new XmlDocument();
                                    xmlDoc.Load(string.Format("{0}{1}/players.xml", baseURL, gameID));
                                    var root = xmlDoc.DocumentElement;
                                    var teamNodes = root.ChildNodes;
                                    foreach (XmlNode node in teamNodes)
                                        if (node.Name == "team")
                                        {
                                            foreach (XmlNode playerNode in node.ChildNodes)
                                                if (playerNode.Name == "player")
                                                {
                                                    player = GetPlayerFromXml(playerNode);
                                                    dbContext.PlayerStagings.AddObject(player);
                                                    dbContext.SaveChanges();
                                                }
                                        }
                                }
                                catch (Exception ex)
                                {
                                    string err = string.Format("Error saving player for date {0}/{1}/{2}: {3}", month, day, year, ex.ToString());
                                    Console.WriteLine(err);
                                    log.Error(err);
                                }

                                innings = new List<string>();
                                PopulateCollection(string.Format("{0}{1}/inning", baseURL, gameID), "inning_");

                                foreach (var inningString in innings)
                                {
                                    var inningNum = Convert.ToInt16(inningString.Substring(inningString.IndexOf("_") + 1, 1));

                                    try
                                    {
                                        inningXmlURL = string.Format("{0}{1}/inning/{2}", baseURL, gameID, inningString);
                                        var xmlDoc = new XmlDocument();
                                        xmlDoc.Load(inningXmlURL);
                                        var root = xmlDoc.DocumentElement;
                                        var inningHalves = root.ChildNodes;
                                        foreach (XmlNode halfInning in inningHalves)
                                        {
                                            inningHalf = halfInning.Name;

                                            foreach (XmlNode atBatNode in halfInning.ChildNodes)
                                            {
                                                if (atBatNode.Name == "atbat")
                                                {
                                                    atBat = GetAtBatFromXml(atBatNode, dbGameID, inningNum, inningHalf);

                                                    dbContext.AtBats.AddObject(atBat);
                                                    dbContext.SaveChanges();
                                                    dbAtBatID = atBat.Id;

                                                    foreach (XmlNode pitchNode in atBatNode.ChildNodes)
                                                    {
                                                        if (pitchNode.Name == "pitch")
                                                        {
                                                            pitch = GetPitchFromXml(pitchNode, dbAtBatID);

                                                            dbContext.Pitches.AddObject(pitch);
                                                            dbContext.SaveChanges();
                                                            dbPitchID = pitch.Id;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string err = string.Format("Error for date {0}/{1}/{2}: {3}", month, day, year, ex.ToString());
                                        Console.WriteLine(err);
                                        log.Error(err);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void PopulateCollection(string url, string startsWith)
        {
            try
            {
                Uri uri = new Uri(url);
                WebClient wc = new WebClient();
                string htmlString = wc.DownloadString(uri);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlString);
                FindHtml(doc.DocumentNode.ChildNodes, startsWith);
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

        public static void FindHtml(HtmlNodeCollection nodes, string startsWith)
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
                                gameIDs.Add(gameNode.InnerText.Trim().TrimEnd('/'));
                            else if (startsWith == "inning_")
                            {
                                var inningstring = gameNode.InnerText.Trim();
                                var sub1 = inningstring.Substring(inningstring.IndexOf("_") + 1);
                                var sub2 = sub1.Substring(0, sub1.IndexOf("."));
                                int inning = -1;
                                if (int.TryParse(sub2, out inning))
                                    innings.Add(inningstring);
                            }
                        }
                    }
                    else
                    {
                        FindHtml(node.ChildNodes, startsWith);
                    }
                }
            }
        }

        private static AtBat GetAtBatFromXml(XmlNode atBatNode, int dbGameID, short inningNum, string inningHalf)
        {
            AtBat atBat = null;
            try
            {
                atBat = new AtBat
                {
                    atbat_num = Convert.ToInt16(atBatNode.Attributes["num"].Value),
                    Balls = Convert.ToInt16(atBatNode.Attributes["b"].Value),
                    Strikes = Convert.ToInt16(atBatNode.Attributes["s"].Value),
                    Outs = Convert.ToInt16(atBatNode.Attributes["o"].Value),
                    start_tfs = atBatNode.Attributes["start_tfs"] == null ? null : atBatNode.Attributes["start_tfs"].Value,
                    start_tfs_zulu = atBatNode.Attributes["start_tfs_zulu"] == null ? null : atBatNode.Attributes["start_tfs_zulu"].Value,
                    batter = Convert.ToInt32(atBatNode.Attributes["batter"].Value),
                    stand = atBatNode.Attributes["stand"].Value,
                    b_height = atBatNode.Attributes["b_height"] == null ? null : atBatNode.Attributes["b_height"].Value,
                    pitcher = Convert.ToInt32(atBatNode.Attributes["pitcher"].Value),
                    p_throws = atBatNode.Attributes["p_throws"] == null ? null : atBatNode.Attributes["p_throws"].Value,
                    atbat_des = atBatNode.Attributes["des"].Value,
                    atbat_event = atBatNode.Attributes["event"].Value,
                    GameId = dbGameID,
                    Inning = inningNum,
                    InningHalf = inningHalf
                };
            }
            catch (Exception ex)
            {
                string err = string.Format("Error in GetAtBatFromXml: {0}", ex.ToString());
                Console.WriteLine(err);
                log.Error(err);
            }

            return atBat;
        }

        private static Pitch GetPitchFromXml(XmlNode pitchNode, int dbAtBatID)
        {
            Pitch pitch = null;
            try
            {
                pitch = new Pitch
                {
                    des = pitchNode.Attributes["des"] == null ? null : pitchNode.Attributes["des"].Value,
                    pitch_id = Convert.ToInt32(pitchNode.Attributes["id"].Value),
                    type = pitchNode.Attributes["type"] == null ? null : pitchNode.Attributes["type"].Value,
                    tfs = pitchNode.Attributes["tfs"] == null ? null : pitchNode.Attributes["tfs"].Value,
                    tfs_zulu = pitchNode.Attributes["tfs_zulu"] == null ? null : pitchNode.Attributes["tfs_zulu"].Value,
                    x = pitchNode.Attributes["x"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["x"].Value),
                    y = pitchNode.Attributes["y"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["y"].Value),
                    sv_id = pitchNode.Attributes["sv_id"] == null ? null : pitchNode.Attributes["sv_id"].Value,
                    start_speed = pitchNode.Attributes["start_speed"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["start_speed"].Value),
                    end_speed = pitchNode.Attributes["end_speed"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["end_speed"].Value),
                    sz_top = pitchNode.Attributes["sz_top"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["sz_top"].Value),
                    sz_bot = pitchNode.Attributes["sz_bot"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["sz_bot"].Value),
                    pfx_x = pitchNode.Attributes["pfx_x"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["pfx_x"].Value),
                    pfx_z = pitchNode.Attributes["pfx_z"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["pfx_z"].Value),
                    px = pitchNode.Attributes["px"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["px"].Value),
                    pz = pitchNode.Attributes["pz"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["pz"].Value),
                    x0 = pitchNode.Attributes["x0"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["x0"].Value),
                    y0 = pitchNode.Attributes["y0"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["y0"].Value),
                    z0 = pitchNode.Attributes["z0"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["z0"].Value),
                    vx0 = pitchNode.Attributes["vx0"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["vx0"].Value),
                    vy0 = pitchNode.Attributes["vy0"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["vy0"].Value),
                    vz0 = pitchNode.Attributes["vz0"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["vz0"].Value),
                    ax = pitchNode.Attributes["ax"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["ax"].Value),
                    ay = pitchNode.Attributes["ay"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["ay"].Value),
                    az = pitchNode.Attributes["az"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["az"].Value),
                    break_y = pitchNode.Attributes["break_y"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["break_y"].Value),
                    break_angle = pitchNode.Attributes["break_angle"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["break_angle"].Value),
                    break_length = pitchNode.Attributes["break_length"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["break_length"].Value),
                    pitch_type = pitchNode.Attributes["pitch_type"] == null ? null : pitchNode.Attributes["pitch_type"].Value,
                    type_confidence = pitchNode.Attributes["type_confidence"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["type_confidence"].Value),
                    zone = pitchNode.Attributes["zone"] == null ? (int?)null : Convert.ToInt32(pitchNode.Attributes["zone"].Value),
                    nasty = pitchNode.Attributes["nasty"] == null ? (int?)null : Convert.ToInt32(pitchNode.Attributes["nasty"].Value),
                    spin_dir = pitchNode.Attributes["spin_dir"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["spin_dir"].Value),
                    spin_rate = pitchNode.Attributes["spin_rate"] == null ? (decimal?)null : Convert.ToDecimal(pitchNode.Attributes["spin_rate"].Value),
                    cc = pitchNode.Attributes["cc"] == null ? null : pitchNode.Attributes["cc"].Value,
                    mt = pitchNode.Attributes["mt"] == null ? null : pitchNode.Attributes["mt"].Value,
                    AtBatId = dbAtBatID
                };
            }
            catch (Exception ex)
            {
                string err = string.Format("Error in GetPitchFromXml: {0}", ex.ToString());
                Console.WriteLine(err);
                log.Error(err);
            }

            return pitch;
        }

        private static PlayerStaging GetPlayerFromXml(XmlNode playerNode)
        {
            decimal _era = 0;
            int _num;
            PlayerStaging player = null;

            try
            {
                player = new PlayerStaging()
                {
                    id = Convert.ToInt32(playerNode.Attributes["id"].Value),
                    first = playerNode.Attributes["first"].Value,
                    last = playerNode.Attributes["last"].Value,
                    num = playerNode.Attributes["num"] == null ? (int?)null :
                        int.TryParse(playerNode.Attributes["num"].Value, out _num) ? _num : (int?)null,
                    boxname = playerNode.Attributes["boxname"] == null ? null : playerNode.Attributes["boxname"].Value,
                    rl = playerNode.Attributes["rl"] == null ? null : playerNode.Attributes["rl"].Value,
                    position = playerNode.Attributes["position"] == null ? null : playerNode.Attributes["position"].Value,
                    status = playerNode.Attributes["status"] == null ? null : playerNode.Attributes["status"].Value,
                    bat_order = playerNode.Attributes["bat_order"] == null ? (int?)null : Convert.ToInt32(playerNode.Attributes["bat_order"].Value),
                    game_position = playerNode.Attributes["game_position"] == null ? null : playerNode.Attributes["game_position"].Value,
                    avg = playerNode.Attributes["avg"] == null ? (decimal?)null : Convert.ToDecimal(playerNode.Attributes["avg"].Value),
                    hr = playerNode.Attributes["hr"] == null ? (int?)null : Convert.ToInt32(playerNode.Attributes["hr"].Value),
                    rbi = playerNode.Attributes["rbi"] == null ? (int?)null : Convert.ToInt32(playerNode.Attributes["rbi"].Value),
                    wins = playerNode.Attributes["wins"] == null ? (int?)null : Convert.ToInt32(playerNode.Attributes["wins"].Value),
                    losses = playerNode.Attributes["losses"] == null ? (int?)null : Convert.ToInt32(playerNode.Attributes["losses"].Value),
                    era = playerNode.Attributes["era"] == null ? (decimal?)null :
                        Decimal.TryParse(playerNode.Attributes["era"].Value, out _era) ? _era : (decimal?)null
                };
            }
            catch (Exception ex)
            {
                string err = string.Format("Error in GetPlayerFromXml: {0}", ex.ToString());
                Console.WriteLine(err);
                log.Error(err);
            }

            return player;
        }

    }
}
