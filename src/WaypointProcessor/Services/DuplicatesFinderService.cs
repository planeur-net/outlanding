using CoordinateSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        private string? _comparedFileName;
        private int _distance;

        public DuplicatesFinderService(string baseFileName, int distance, string? comparedFilename = null) 
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
                        OutputPoints(wpBase, wpComp, dist);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveSingleFileDuplicates()
        {
            // Load waypoints
            var parser = new CsvFileParser();
            var waypointsBase = parser.ParseFile(_baseFilename);

            for (int i=0;i< waypointsBase.Count; i++)
            {
                var current = i + 1;            // Index of current point in the list
                for (var j = current; j < waypointsBase.Count; j++)
                {
                    var wpBase = waypointsBase[i];
                    var wpComp = waypointsBase[j];
                    var dist = new Distance(wpBase.Coordinate, wpComp.Coordinate);

                    var isSamePoint = wpBase.Name == wpComp.Name && wpBase.Country == wpComp.Country;
                    if (!isSamePoint && dist.Meters < _distance)
                    {
                        OutputPoints(wpBase, wpComp, dist);
                    }
                }
            }
        }

        /// <summary>
        /// Displays 2 duplicate waypoints
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="dist"></param>
        private void OutputPoints(WaypointModel point1, WaypointModel point2, Distance dist)
        {
            Console.WriteLine(point1.ToString());
            Console.WriteLine(point2.ToString());
            Console.WriteLine($"dist (m)= {dist.Meters}");
            Console.WriteLine($"-------------");
        }
    }
}
