using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Awv.Bots.Runner
{
    public class AppRun
    {
        public string Name { get; set; }
        public string Environment { get; set; }

        [JsonProperty("args")]
        private Dictionary<string, string> internalArgs
        {
            get => Arguments.Args;
            set => Arguments.Args = value;
        }
        public Arguments Arguments { get; set; } = new Arguments();

        public string GetExecutableLocation(string directory) => Path.Combine(directory, Environment, Name);
        public string GetExecutableLocation() => GetExecutableLocation(Directory.GetCurrentDirectory());

        public string GetExecutable(string directory)
        {
            var exeLocation = GetExecutableLocation(directory);
            var exeName = $"{Name}.exe";
            if (!File.Exists(Path.Combine(exeLocation, exeName)))
                exeName = Directory.GetFiles(exeLocation, "*.exe").FirstOrDefault();

            return !string.IsNullOrWhiteSpace(exeName) ? Path.Combine(exeLocation, exeName) : null;
        }

        public string GetExecutable()
            => GetExecutable(Directory.GetCurrentDirectory());

        public void Run()
        {
            var oldDir = Directory.GetCurrentDirectory();
            var args = new Arguments();

            args["-env"] = Environment;
            args["-time"] = DateTime.Now.Ticks.ToString();

            Directory.SetCurrentDirectory(GetExecutableLocation(oldDir));
            Console.WriteLine($"Trying to start: {GetExecutableLocation(oldDir)}/{Name}.exe {args.ToString()}");
            Process.Start(new ProcessStartInfo(GetExecutable(oldDir), args.ToString()));
            Directory.SetCurrentDirectory(oldDir);
        }
    }
}
