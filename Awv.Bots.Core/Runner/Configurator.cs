using Awv.Automation.SocialMedia.Facebook;
using Awv.Automation.SocialMedia.Twitter;
using Awv.Bots.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Awv.Bots.Runner
{
    public class Configurator
    {
        private string BasePath { get; set; }

        public Arguments Args { get; private set; }
        public IServiceCollection Services { get; set; }
        public IConfigurationRoot Config { get; set; }

        public ServiceProvider Provider { get; private set; }

        public Configurator()
        {
            BasePath = $"appsettings.json".ToLower();
        }

        public Configurator Configure(string[] inputArgs)
        {
            Args = BuildArguments(inputArgs);
            Config = BuildConfig(Args.Environment);
            Services = BuildServiceCollection();
            return this;
        }

        public ServiceProvider BuildServiceProvider()
        {
            return Provider = Services.BuildServiceProvider();
        }

        private IConfigurationRoot BuildConfig(AppEnvironment? environment = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(BasePath, false, true);

            if (environment.HasValue) builder.AddJsonFile(BasePath.Replace(".json", $".{environment.Value.ToString().ToLower()}.json"), true, true);

            return builder.Build();
        }

        private Arguments BuildArguments(string[] inputArgs) => new Arguments(inputArgs);

        public IServiceCollection BuildServiceCollection(IServiceCollection services)
        {
            services.AddSingleton(Args);
            services.AddSingleton(Config);
            return services;
        }

        public IServiceCollection AddDbContext<TContext>(string connectionStringKey)
            where TContext : DbContext => AddDbContext<TContext>("connectionStrings", connectionStringKey);

        public IServiceCollection AddDbContext<TContext>(string connectionStringSectionKey, string connectionStringKey)
            where TContext : DbContext
        {
            var config = Config.GetSection(connectionStringSectionKey).GetSection(connectionStringKey);
            var ip = config["ip"];
            var port = config["port"];
            var catalog = config["catalog"];
            var login = config["login"];
            var password = config["password"];

            var fragments = new List<string>();

            fragments.Add($"Data Source=\"{ip}, {port}\"");
            fragments.Add($"Initial Catalog={catalog}");
            fragments.Add($"Persist Security Info=True");
            fragments.Add($"User ID={login}");
            fragments.Add($"Password={password}");

            var connectionString = string.Join(";", fragments);

            return Services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString));
        }

        public IServiceCollection AddTwitterClient() => AddTwitterClient("twitter", "default");
        public IServiceCollection AddTwitterClient(string twitterClientKey) => AddTwitterClient("twitter", twitterClientKey);

        public IServiceCollection AddTwitterClient(string twitterSectionKey, string twitterClientKey)
        {
            var config = Config.GetSection(twitterSectionKey).GetSection(twitterClientKey);
            var apiKey = config["apiKey"];
            var apiSecret = config["apiSecret"];
            var apiAccessToken = config["apiAccessToken"];
            var apiAccessTokenSecret = config["apiAccessTokenSecret"];
            var output = Path.GetFullPath(config["output"] ?? ".");
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            switch (Args.Environment)
            {
                case AppEnvironment.P: return Services.AddSingleton<ITwitterClient>(new TwitterClient(apiKey, apiSecret, apiAccessToken, apiAccessTokenSecret));
                default: return Services.AddSingleton<ITwitterClient>(new TwitterFileClient(output));
            }
        }

        public IServiceCollection AddFacebookClient() => AddFacebookClient("facebook", "default");

        public IServiceCollection AddFacebookClient(string facebookClient) => AddFacebookClient("facebook", facebookClient);

        public IServiceCollection AddFacebookClient(string facebookSectionKey, string facebookClientKey)
        {
            var config = Config.GetSection(facebookSectionKey).GetSection(facebookClientKey);
            var accessToken = config["accessToken"];
            var target = config["target"];
            var output = Path.GetFullPath(config["output"] ?? ".");
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);
            switch (Args.Environment)
            {
                case AppEnvironment.P: return Services.AddSingleton<IFacebookClient>(new FacebookClient(accessToken, target));
                default: return Services.AddSingleton<IFacebookClient>(new FacebookFileClient(target ?? "LOCALMACHINE", output));
            }
        }

        private IServiceCollection BuildServiceCollection()
            => BuildServiceCollection(new ServiceCollection());
    }
}
