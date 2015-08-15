using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using PitchFX.DAL;
using log4net;
using log4net.Config;

namespace PitchFXConsole
{
    public static class Helpers
    {
        private static ILog log = LogManager.GetLogger("PitchFXConsole");

        static Helpers()
        {
            XmlConfigurator.Configure();
        }

        public static IEnumerable<DateTime> GetDays(DateTime start, DateTime end)
        {
            for (var day = start.Date; day.Date <= end.Date; day = day.AddDays(1))
                if (day.Month >= 3 && day.Month <= 11)
                    yield return day;
        }

        public static void VerifyDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    string err = string.Format("Error in VerifyDirectory creating path {0}: {1}", path, ex.ToString());
                    Console.WriteLine(err);
                    log.Error(err);
                }
            }
        }

        public static void ParseXml(this XmlDocument xmlDoc, DateTime date, string gameID)
        {
            using (var dbContext = new Model1Container())
            {
                try
                {
                    if (dbContext.Connection.State != System.Data.ConnectionState.Open)
                        dbContext.Connection.Open();

                    var inningNodes = xmlDoc.SelectNodes("/game/inning");
                    if (inningNodes.Count > 0)
                    {
                        int dbGameID = AddGame(date, gameID, inningNodes[0].Attributes["home_team"].Value.ToUpper(),
                                inningNodes[0].Attributes["away_team"].Value.ToUpper(), dbContext);

                        foreach (XmlNode node in inningNodes)
                        {
                            var inningNum = Convert.ToInt16(node.Attributes["num"].Value);
                            var inningHalves = node.ChildNodes;
                            AddAtBats(inningHalves, dbGameID, inningNum, dbContext);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string err = string.Format("Error in ParseXml: {0}", ex.ToString());
                    Console.WriteLine(err);
                    log.Error(err);
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

        private static int AddGame(DateTime date, string gameID, string homeTeam, string awayTeam, Model1Container dbContext)
        {
            var game = new Game
            {
                Description = gameID,
                GameDate = date,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam
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
            return game.Id;
        }

        private static void AddAtBats(XmlNodeList inningHalves, int dbGameID, short inningNum, Model1Container dbContext)
        {
            foreach (XmlNode halfInning in inningHalves)
            {
                var inningHalf = halfInning.Name;
                foreach (XmlNode atBatNode in halfInning.ChildNodes)
                {
                    if (atBatNode.Name == "atbat")
                    {
                        var atBat = GetAtBatFromXml(atBatNode, dbGameID, inningNum, inningHalf);

                        dbContext.AtBats.AddObject(atBat);
                        dbContext.SaveChanges();
                        int dbAtBatID = atBat.Id;

                        AddPitches(atBatNode, dbAtBatID, dbContext);
                    }
                }
            }
        }

        private static void AddPitches(XmlNode atBatNode, int dbAtBatID, Model1Container dbContext)
        {
            foreach (XmlNode pitchNode in atBatNode.ChildNodes)
                if (pitchNode.Name == "pitch")
                {
                    var pitch = GetPitchFromXml(pitchNode, dbAtBatID);

                    dbContext.Pitches.AddObject(pitch);
                    dbContext.SaveChanges();
                    int dbPitchID = pitch.Id;
                }
        }
    }
}
