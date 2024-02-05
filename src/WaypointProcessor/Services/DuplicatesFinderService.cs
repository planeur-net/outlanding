using CoordinateSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaypointProcessor.Models;

namespace WaypointProcessor.Services
{
    /// <summary>
    /// Finds duplicate points between two cup files
    /// </summary>
    internal class DuplicatesFinderService
    {
        private string _baseFilename;
        private string _comparedFileName;
        private int _distance;

        public DuplicatesFinderService(string baseFileName, string comparedFilename, int distance) 
        {
            _baseFilename = baseFileName;
            _comparedFileName = comparedFilename;   
            _distance = distance;
        }

        /// <summary>
        /// List duplicates when distance between 2 points <= distance
        /// </summary>
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

                    var isSamePoint = wpBase.Name == wpComp.Name && wpBase.Country == wpComp.Country;
                    if (!isSamePoint && dist.Meters < _distance)
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
