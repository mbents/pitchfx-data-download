using System;

namespace PitchFXConsole
{
    public class GameUrlCollector : CollectorBase
    {
        public override void Run()
        {
            foreach (var date in Dates)
            {
                string _url = string.Format(BaseURL, date.Year, date.Month < 10 ? "0" + date.Month : date.Month.ToString(), 
                    date.Day < 10 ? "0" + date.Day : date.Day.ToString());
                PopulateCollection(_url, "gid_", date);
            }
        }
    }
}
