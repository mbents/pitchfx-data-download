using System;

namespace PitchFXConsole
{
    public class QueueItem
    {
        public DateTime GameDate { get; set; }
        public string InningsFileURL { get; set; }
        public string PlayersFileURL { get; set; }
        public string GameID { get; set; }
        public string OutputDir { get; set; }
    }
}
