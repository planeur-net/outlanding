using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaypointProcessor.Models
{
    internal class AltitudeCheckModel
    {
        public string Nom {  get; set; }
        public int AltiCup { get;set; }
        public int AltiTopo { get; set; }
        public int Delta { get; set; }

        public string ToMarkDownTableLine()
        {
            return $"| {Nom} | {AltiCup} | {AltiTopo} | {Delta} |";
        }
    }

    
}
