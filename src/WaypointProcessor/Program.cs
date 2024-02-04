using System;
using CommandLine;
using WaypointProcessor.Services;

namespace QuickStart
{
    class Program
    {
        [Verb("duplicates", HelpText = "Add file contents to the index.")]
        class DuplicatesOptions
        {
            [Option('b', "baseFileName", Required = true, HelpText = "Base file name")]
            public string? BaseFileName { get; set; }

            [Option('c', "comparedFileName", Required = true, HelpText = "Compared file name")]
            public string? ComparedFileName { get; set; }
        }


        static void Main(string[] args)
        {
            var program = new Program();
            CommandLine.Parser.Default.ParseArguments<DuplicatesOptions>(args)
                .WithParsed(program.RemoveDuplicates)
                .WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
        }

        private void RemoveDuplicates(DuplicatesOptions options)
        {
            var duplicatesFinderSercice = new DuplicatesFinderService(options.BaseFileName, options.ComparedFileName);
            duplicatesFinderSercice.RemoveDuplicates();

        }

    }
}