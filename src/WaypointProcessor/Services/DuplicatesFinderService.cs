using CoordinateSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaypointProcessor.Models;

namespace WaypointProcessor.Services
{
    internal class DuplicatesFinderService
    {
        private string _baseFilename;
        private string _comparedFileName;

        public DuplicatesFinderService(string baseFileName, string comparedFilename) 
        {
            _baseFilename = baseFileName;
            _comparedFileName = comparedFilename;   
        }

        public void RemoveDuplicates()
        {
            // Load waypoints
            var parser = new CsvFileParser();
            var waypointsBase = parser.ParseFile(_baseFilename);
            var waypointCompared = parser.ParseFile(_comparedFileName);

            foreach ( var wpBase in waypointsBase )
            {
                foreach ( var wpComp in waypointCompared)
                {
                    var dist = new Distance(wpBase.Coordinate, wpComp.Coordinate);

                    if (dist.Meters < 300)
                    {
                        Console.WriteLine(wpBase.ToString());
                        Console.WriteLine(wpComp.ToString());
                        Console.WriteLine($"dist (m)= {dist.Meters}");
                        Console.WriteLine($"-------------");
                    }
                }
            }

        }
    }
}
