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
    internal class CheckAltitudeService(string baseFileName, int errorDelta, int warningDelta, string outputFilename)
    {
        private readonly string _baseFilename = baseFileName;
        private readonly int _errorDelta = errorDelta;
        private readonly int _warningDelta = warningDelta;
        private readonly string _outputFilename = outputFilename;

        private List<AltitudeCheckModel> listAltitudeChecks = [];

        /// <summary>
        /// List duplicates when distance between 2 points <= distance
        /// </summary>
        public void CheckAltitudes()
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

            var listLats = lats.Chunk(100).ToList();
            var listLons = lons.Chunk(100).ToList();

            var totalResponseModel = new ElevationResponseModel();


            for (var i = 0; i < listLats.Count; i++)
            {
                Console.WriteLine($"Sending request for chunk {i+1} / {listLats.Count}");
                var currentLats = listLats[i];
                var currentLons = listLons[i];

                var latParam = string.Join<double>("|", currentLats).Replace(",", ".");
                var lonParam = string.Join<double>("|", currentLons).Replace(",", ".");

                const string url = "https://wxs.ign.fr/calcul/alti/rest/elevation.json";
                var param = new Dictionary<string, string?>() { { "lat", latParam }, { "lon", lonParam }, { "zonly", "true" } };

                var newUrl = new Uri(QueryHelpers.AddQueryString(url, queryString: param));

                var client = new HttpClient();
                var response = client.GetStringAsync(newUrl).GetAwaiter().GetResult();
                var responseModel = JsonSerializer.Deserialize<ElevationResponseModel>(response);

                if (responseModel != null )
                    totalResponseModel.elevations.AddRange(responseModel.elevations);
            }

            if (totalResponseModel != null)
                ComputeDelta(waypointsBase, totalResponseModel);
        }

        private void ComputeDelta(List<WaypointModel> waypointsBase, ElevationResponseModel responseModel)
        {
            for (int i = 0; i< waypointsBase.Count; i++)
            {
                WaypointModel currentPoint = waypointsBase[i];
                var altBase = currentPoint.Altitude;
                var altApi = responseModel.elevations[i];
                var delta = altBase - altApi;
                var error = AltitudeCheckModel.GetErrorString((int)delta, _errorDelta, _warningDelta);

                if (!string.IsNullOrEmpty(error))
                {
                listAltitudeChecks.Add(new AltitudeCheckModel
                    {
                        Nom = currentPoint.Name,
                        AltiCup = (int)currentPoint.Altitude,
                        AltiTopo = (int)altApi,
                        Delta = (int)delta,
                        Error = error
                });
                }
            }

            OutputToFile();
        }

        private void OutputToFile()
        {
            var commandLine = Environment.CommandLine;

            Console.WriteLine($"Writing outpout to: {_outputFilename}");
            var commandLineMd = $"`{commandLine}`";
            var header = "| Nom | Alti .cup | Alti API | Delta | Err / Warn |";
            var header2 = "|---|---|---|---|---|";

            using (var outputFile = new StreamWriter(_outputFilename))
            {
                outputFile.WriteLine(commandLineMd);
                outputFile.WriteLine("  ");
                outputFile.WriteLine(header);
                outputFile.WriteLine(header2);
                foreach (AltitudeCheckModel altCheckModel in listAltitudeChecks)
                {
                    var line = altCheckModel.ToMarkDownTableLine();
                    outputFile.WriteLine(line);
                }
                    
            }

        }


    }
}
