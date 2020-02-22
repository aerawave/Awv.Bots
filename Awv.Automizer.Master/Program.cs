using Awv.Bots.Logging;
using Awv.Bots.Runner;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;

namespace Awv.Automizer.Master
{
    class Program
    {
        static void Main(string[] inputArgs)
        {
            var logger = new AwvLogFactory().CreateLogger<Program>();
            var args = new Arguments(inputArgs);

            args.LogInitialization<Program>(logger);

            var file = Path.GetFullPath(args["-file"]);

            if (!string.IsNullOrWhiteSpace(file))
            {
                logger.LogInformation($"AppRunList file: '{file}'");
                var appsDirectory = Path.GetDirectoryName(file);
                Directory.SetCurrentDirectory(appsDirectory);
                var appList = JsonConvert.DeserializeObject<AppList>(File.ReadAllText(file));


                foreach (var app in appList.Apps)
                {
                    var exe = app.GetExecutable();
                    if (File.Exists(exe))
                    {
                        app.Run();
                        logger.LogInformation($"Running app '{app.Name}' in environment: '{app.Environment}'...");
                        logger.LogInformation($"Executable: '{exe}'");
                    }
                }
            }
        }
    }
}
