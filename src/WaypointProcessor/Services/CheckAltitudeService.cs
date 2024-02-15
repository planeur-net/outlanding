using CoordinateSharp;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WaypointProcessor.Models;

namespace WaypointProcessor.Services
{
    /// <summary>
    /// Finds duplicate points between two cup files
    /// </summary>
    internal class CheckAltitudeService
    {
        private string _baseFilename;
        private int _distance;
        private string _outputFilename;

        private List<AltitudeCheckModel> listAltitudeChecks = new List<AltitudeCheckModel>();

        public CheckAltitudeService(string baseFileName, int distance, string outputFilename)
        {
            _baseFilename = baseFileName;
            _distance = distance;
            _outputFilename = outputFilename;
        }

        /// <summary>
        /// List duplicates when distance between 2 points <= distance
        /// </summary>
        public async void CheckAltitudes()
        {
            // Load waypoints
            var parser = new CsvFileParser();
            var waypointsBase = parser.ParseFile(_baseFilename);

            var lats = new List<double>();
            var lons = new List<double>();

            foreach (var wpBase in waypointsBase)
            {
                lats.Add(wpBase.Lat.DecimalDegree);
                lons.Add(wpBase.Lon.DecimalDegree);
            }

            var latParam = string.Join<double>("|", lats).Replace(",", ".");
            var lonParam = string.Join<double>("|", lons).Replace(",", ".");


            const string url = "https://wxs.ign.fr/calcul/alti/rest/elevation.json";
            var param = new Dictionary<string, string>() { {"lat", latParam }, { "lon", lonParam }, { "zonly", "true" } };

            var newUrl = new Uri(QueryHelpers.AddQueryString(url, param));

            HttpClient client = new HttpClient();
            var response = client.GetStringAsync(newUrl).GetAwaiter().GetResult();
            var responseModel = JsonSerializer.Deserialize<ElevationResponseModel>(response);

            ComputeDelta(waypointsBase, responseModel);
        }

        private void ComputeDelta(List<WaypointModel> waypointsBase, ElevationResponseModel responseModel)
        {
            for (int i = 0; i< waypointsBase.Count; i++)
            {
                var currentPoint = waypointsBase[i];
                var altBase = currentPoint.Altitude;
                var altApi = responseModel.elevations[i];
                var delta = altBase - altApi;

                //if (Math.Abs((double)delta) > 50)
                //{
                    listAltitudeChecks.Add(new AltitudeCheckModel
                    {
                        Nom = currentPoint.Name,
                        AltiCup = (int)currentPoint.Altitude,
                        AltiTopo = (int)altApi,
                        Delta = (int)delta
                    });
                //}
            }

            OutputToFile();
        }

        private void OutputToFile()
        {
            var header = "| Nom | Alti .cup | Alti API | Delta |";
            var header2 = "|---|---|---|---|";

            using (StreamWriter outputFile = new StreamWriter(_outputFilename))
            {
                outputFile.WriteLine(header);
                outputFile.WriteLine(header2);
                foreach (AltitudeCheckModel altCheckModel in listAltitudeChecks)
                {
                    var line = altCheckModel.ToMarkDownTableLine();
                    outputFile.WriteLine(line);
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
