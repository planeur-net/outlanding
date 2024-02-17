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
            public required string BaseFileName { get; set; }

            [Option('c', "comparedFileName", Required = true, HelpText = "Compared file name")]
            public required string ComparedFileName { get; set; }

            [Option('d', "distance", Required = false, HelpText = "Max distance to consider waypoints equal")]
            public int Distance { get; set; } = 300;
        }

        [Verb("singleFileDuplicates", HelpText = "Find duplictes in a single file")]
        class SingelFileDuplicatesOptions
        {
            [Option('b', "baseFileName", Required = true, HelpText = "Base file name")]
            public required string BaseFileName { get; set; }

            [Option('d', "distance", Required = false, HelpText = "Max distance to consider waypoints equal")]
            public int Distance { get; set; } = 300;
        }

        [Verb("checkAltitudes", HelpText = "Check Altitudes against Public API")]
        class CheckAltitudesOptions
        {
            [Option('b', "baseFileName", Required = true, HelpText = "Base file name")]
            public required string BaseFileName { get; set; }

            [Option('e', "errorDelta", Required = false, HelpText = "Max altitude difference to report an error")]
            public int ErrorDelta { get; set; } = 50;

            [Option('w', "warningDelta", Required = false, HelpText = "Max altitude difference to report a warning")]
            public int WarningDelta { get; set; } = 50;

            [Option('o', "output", Required = true, HelpText = "MarkDown file output path")]
            public required string OutputFileName { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Verbos mode: output all points = not just Err and Warn")]
            public required bool IsVerbose { get; set; } = false;
        }


        static void Main(string[] args)
        {
            var program = new Program();
            CommandLine.Parser.Default.ParseArguments<DuplicatesOptions, SingelFileDuplicatesOptions, CheckAltitudesOptions>(args)
                .WithParsed<DuplicatesOptions>(program.RemoveDuplicates)
                .WithParsed<SingelFileDuplicatesOptions>(program.RemoveSingleFileDuplicates)
                .WithParsed<CheckAltitudesOptions>(program.CheckAltitudes)
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

        private void CheckAltitudes(CheckAltitudesOptions options)
        {
            var checkAltitudeService = new CheckAltitudeService(options.BaseFileName, options.ErrorDelta, options.WarningDelta, options.OutputFileName, options.IsVerbose);
            checkAltitudeService.CheckAltitudes();
        }

    }
}