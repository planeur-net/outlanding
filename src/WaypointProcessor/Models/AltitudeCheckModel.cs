using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaypointProcessor.Models
{
    internal class AltitudeCheckModel
    {
        public required string Nom {  get; set; }
        public int AltiCup { get;set; }
        public int AltiTopo { get; set; }
        public int Delta { get; set; }
        public string Error { get; set; } = "";

        public string ToMarkDownTableLine()
        {
            return $"| {Nom} | {AltiCup} | {AltiTopo} | {Delta} | {Error} |";
        }

        public static string GetErrorString(int delta, int errorDelta, int warningDelta, int altApi)
        {
            delta = Math.Abs(delta);
            if (altApi == -99999)               // API could not return altitude for point
                return ":grey_question:";
            else if (delta >= warningDelta && delta <= errorDelta)
                return "Warning";
            else if (delta >= errorDelta)
                return "Error";
            else return "";
        }
    }

    
}
