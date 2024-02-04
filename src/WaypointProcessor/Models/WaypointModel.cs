using CoordinateSharp;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaypointProcessor.Models
{
    internal class WaypointModel
    {
        private Coordinate _coordinate = null;

        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Country { get; set; }
        public CoordinatePart? Lat { get; set; }
        public CoordinatePart? Lon { get; set; }
        public Coordinate? Coordinate
        {
            get
            {
                if (_coordinate == null)
                {
                    Coordinate c = new()
                    {
                        Latitude = Lat,
                        Longitude = Lon
                    };
                    _coordinate = c;
                }
                return _coordinate;
            }
            set { _coordinate = value; }
        }

        public override string ToString()
        {
            return $"{Name},{Code},{Country},{CoordConverter<CoordinatePart>.DegresDecimalMinuteToCupCoord(Coordinate)} ";
        }
    }


    internal class WaypointModelMap : ClassMap<WaypointModel>
    {
        public WaypointModelMap()
        {
            Map(m => m.Name).Name("name");
            Map(m => m.Code).Name("code");
            Map(m => m.Country).Name("country");
            Map(m => m.Lat).Name("lat").TypeConverter<CoordConverter<CoordinatePart>>();
            Map(m => m.Lon).Name("lon").TypeConverter<CoordConverter<CoordinatePart>>();
        }
    }

    internal class CoordConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            var coordPart = CupCoordToDegresDecimalMinute(text);
            return coordPart;
        }

        /// <summary>
        /// Convert a string from 4417.349N,00632.046E to N 44.289150 E 006.534100
        /// </summary>
        /// <param name="cupCoord"></param>
        /// <returns>CoordinatePart corresponding to the lat or lon</returns>
        private CoordinatePart CupCoordToDegresDecimalMinute(string cupCoord)
        {
            // 4417.349N
            var decimalPosition = cupCoord.IndexOf('.');
            var degres = Int32.Parse(cupCoord.Remove(decimalPosition - 2));
            var minutes = cupCoord.Substring(decimalPosition - 2, 6);
            var decimalMinute = double.Parse(minutes, System.Globalization.CultureInfo.InvariantCulture);
            var decimalMinuteInDegree = decimalMinute / 60;
            var decimalDegree = degres + decimalMinuteInDegree;


            var nsew = cupCoord.Remove(0, decimalPosition + 4);
            var coord = $"{nsew} {decimalDegree.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            var coordPart = CoordinatePart.Parse(coord);

            return coordPart;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public static string DegresDecimalMinuteToCupCoord(Coordinate coord)
        {
            var lat = coord.Latitude;
            var lon = coord.Longitude;

            var latitude = $"{lat.Degrees}{lat.DecimalMinute.ToString("0.000",System.Globalization.CultureInfo.InvariantCulture)}{lat.Position}";
            var longitude = $"00{lon.Degrees}{lon.DecimalMinute.ToString("0.000",System.Globalization.CultureInfo.InvariantCulture)}{lon.Position}";

            return $"{latitude},{longitude}";
        }
    }
}
