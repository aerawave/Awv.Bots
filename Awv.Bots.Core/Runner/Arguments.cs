using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Awv.Bots.Runner
{
    public class Arguments
    {
        public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
        public AppEnvironment Environment => Args.ContainsKey("-env") ? Enum.Parse<AppEnvironment>(Args["-env"]) : AppEnvironment.Local;

        public string this[string arg]
        {
            get => Args.ContainsKey(arg) ? Args[arg] : null;
            set
            {
                if (Args.ContainsKey(arg))
                {
                    Args[arg] = value;
                }
                else
                {
                    Args.Add(arg, value);
                }
            }
        }

        public Arguments() { }
        public Arguments(string[] args) : this()
        {
            if (args == null)
                return;
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith('-'))
                {
                    var key = args[i];
                    var value = args[++i];
                    this[key] = value;
                }
            }
        }

        public Arguments(Dictionary<string, string> args) : this()
        {
            Args = args;
        }

        public TTargetType ToObject<TTargetType>()
        {
            var type = typeof(TTargetType);
            var properties = type.GetProperties();

            var target = Activator.CreateInstance<TTargetType>();

            foreach (var property in properties)
            {
                if (Args.ContainsKey(property.Name))
                {
                    property.SetValue(target, Args[property.Name]);
                }
                else
                {
                    var attributes = property.GetCustomAttributes(typeof(DataMemberAttribute), true);

                    foreach (DataMemberAttribute attribute in attributes)
                    {
                        if (!string.IsNullOrWhiteSpace(attribute.Name) && Args.ContainsKey(attribute.Name))
                        {
                            property.SetValue(target, Args[attribute.Name]);
                            break;
                        }
                    }
                }
            }

            return target;
        }

        public void LogInitialization<TInitializedType>(ILogger logger)
        {
            logger.LogInformation($"Initializing {typeof(TInitializedType).FullName}...");
            logger.LogInformation("Arguments:");
            foreach (var key in Args.Keys)
                logger.LogInformation($"{key}: '{Args[key]}'");
        }

        public override string ToString()
        {
            var args = new List<string>();

            foreach (var key in Args.Keys)
            {
                args.Add(key);
                args.Add(Args[key]);
            }

            var argsFormatted = args.Select(arg => arg.Contains(' ') ? $"\"{arg}\"" : arg);

            return string.Join(' ', argsFormatted);
        }
    }
}
