using System;
using CommandLine;
using WaypointProcessor.Services;

namespace QuickStart
{
    class Program
    {
        [Verb("duplicates", HelpText = "Find duplicates in 2 files")]
        class DuplicatesOptions
        {
            [Option('b', "baseFileName", Required = true, HelpText = "Base file name")]
            public string BaseFileName { get; set; }

            [Option('c', "comparedFileName", Required = true, HelpText = "Compared file name")]
            public string? ComparedFileName { get; set; }

            [Option('d', "distance", Required = false, HelpText = "Max distance to consider waypoints equal")]
            public int Distance { get; set; } = 300;
        }

        [Verb("singleFileDuplicates", HelpText = "Find duplictes in a single file")]
        class SingelFileDuplicatesOptions
        {
            [Option('b', "baseFileName", Required = true, HelpText = "Base file name")]
            public string BaseFileName { get; set; }

            [Option('d', "distance", Required = false, HelpText = "Max distance to consider waypoints equal")]
            public int Distance { get; set; } = 300;
        }


            static void Main(string[] args)
        {
            var program = new Program();
            CommandLine.Parser.Default.ParseArguments<DuplicatesOptions, SingelFileDuplicatesOptions>(args)
                .WithParsed<DuplicatesOptions>(program.RemoveDuplicates)
                .WithParsed<SingelFileDuplicatesOptions>(program.RemoveSingleFileDuplicates)
                .WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
        }

        private void RemoveDuplicates(DuplicatesOptions options)
        {
            var duplicatesFinderSercice = new DuplicatesFinderService(options.BaseFileName, options.Distance, options.ComparedFileName);
            duplicatesFinderSercice.RemoveDuplicates();
        }
        private void RemoveSingleFileDuplicates(SingelFileDuplicatesOptions options)
        {
            var duplicatesFinderSercice = new DuplicatesFinderService(options.BaseFileName, options.Distance);
            duplicatesFinderSercice.RemoveSingleFileDuplicates();
        }

    }
}