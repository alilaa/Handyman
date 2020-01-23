﻿using Handyman.Tools.Outdated.Model;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.Linq;

namespace Handyman.Tools.Outdated.Analyze
{
    [Command("analyze")]
    public class AnalyzeCommand
    {
        private readonly IConsole _console;
        private readonly ProjectLocator _projectLocator;
        private readonly ProjectUtil _projectUtil;
        private readonly ProjectAnalyzer _projectAnalyzer;
        private readonly IEnumerable<IFileWriter> _fileWriters;

        public AnalyzeCommand(IConsole console, ProjectLocator projectLocator, ProjectUtil projectUtil, ProjectAnalyzer projectAnalyzer, IEnumerable<IFileWriter> fileWriters)
        {
            _console = console;
            _projectLocator = projectLocator;
            _projectUtil = projectUtil;
            _projectAnalyzer = projectAnalyzer;
            _fileWriters = fileWriters;
        }

        [Argument(0, "path", Description = "Path to folder or project")]
        public string Path { get; set; }

        [Option(ShortName = "", Description = "Output file(s), supported format is .md")]
        public IEnumerable<string> OutputFile { get; set; }

        [Option(ShortName = "", Description = "Tags filter, start with ! to exclude")]
        public IEnumerable<string> Tags { get; set; }

        [Option(CommandOptionType.NoValue, ShortName = "", Description = "Skip dotnet restore")]
        public bool NoRestore { get; set; }

        [Option]
        public Verbosity Verbosity { get; set; }

        public int OnExecute()
        {
            var projects = _projectLocator.GetProjects(Path, (Tags ?? new string[] { }).ToList());

            if (ShouldWriteToConsole(Verbosity.Minimal))
            {
                _console.WriteLine();
                _console.WriteLine($"Discovered {projects.Count} projects.");
                _console.WriteLine();
            }

            if (projects.Count == 0)
            {
                return 0;
            }

            foreach (var project in projects)
            {
                if (ShouldWriteToConsole(Analyze.Verbosity.Minimal))
                    _console.WriteLine($"Analyzing {project.RelativePath}");

                if (NoRestore == false)
                    _projectUtil.Restore(project);

                _projectAnalyzer.Analyze(project);
            }

            new ResultConsoleWriter(_console, Verbosity).WriteResult(projects);
            WriteResultToFile(projects);

            return 0;
        }

        private bool ShouldWriteToConsole(Verbosity required)
        {
            var current = Verbosity;
            return current != Verbosity.Quiet && (int)required <= (int)current;
        }

        private void WriteResultToFile(IReadOnlyCollection<Project> projects)
        {
            foreach (var outputFile in OutputFile)
            {
                var extension = System.IO.Path.GetExtension(outputFile).ToLowerInvariant();
                var fileWriters = _fileWriters.Where(x => x.CanHandle(extension)).ToList();

                if (!fileWriters.Any())
                {
                    _console.WriteLine($"Unsupported output file format '{extension}'.");
                    continue;
                }

                fileWriters.ForEach(x => x.Write(outputFile, projects));
            }
        }
    }
}