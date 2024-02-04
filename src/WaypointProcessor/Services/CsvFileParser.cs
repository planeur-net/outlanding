using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaypointProcessor.Models;
using static System.Net.Mime.MediaTypeNames;

namespace WaypointProcessor.Services
{
    /// <summary>
    /// CSV file Parser.
    /// </summary>
    internal class CsvFileParser
    {
        private List<WaypointModel> _waypoints;

        public List<WaypointModel> ParseFile(string filename)
        {
            _waypoints = new List<WaypointModel>();

            // Configure CSV parser
            var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                HasHeaderRecord = true,
                Delimiter = ",",
                ShouldSkipRecord = args => args.Row[0].Contains("version=") || args.Row[0].Contains("-----Related Tasks")
            };

            using (var reader = new StreamReader(filename))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<WaypointModelMap>();
                _waypoints = csv.GetRecords<WaypointModel>().ToList();
            }
            return _waypoints;
        }
    }
}
