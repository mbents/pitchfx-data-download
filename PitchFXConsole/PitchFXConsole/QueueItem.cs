using System;

namespace PitchFXConsole
{
    public class QueueItem
    {
        public DateTime GameDate { get; set; }
        public string FileURL { get; set; }
        public string GameID { get; set; }
        public string OutputDir { get; set; }
    }
}
