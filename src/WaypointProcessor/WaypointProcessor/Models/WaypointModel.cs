using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaypointProcessor.Models
{
    internal class WaypointModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Country { get; set;}
        public double Lat { get; set; }
        public string? Lon { get; set; }
    }


    internal class WaypointModelMap : ClassMap<WaypointModel>
    {
        public WaypointModelMap() 
        {
            Map(m => m.Name).Name("name");
            Map(m => m.Code).Name("code");
            Map(m => m.Country).Name("country");
            Map(m => m.Lat).Name("lat").TypeConverter<CoordConverter<double>>();
            Map(m => m.Lon).Name("lon");
        }
    }

    internal class CoordConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return 45.0;
        }
    }

 



}
