using System;
using System.IO;
using System.Windows.Forms;
using log4net;
using Newtonsoft.Json;
using Starlight.Core;
using Starlight.Misc;
using Starlight.RbxApp;

namespace Starlight.Launcher
{
    internal class LaunchParamsConfig : IStarlightLaunchParams
    {
        [JsonProperty("saveLog")]
        public bool SaveLog { get; set; }

        [JsonProperty("verbose")]
        public bool Verbose { get; set; }

        [JsonProperty("fpsCap")]
        public int FpsCap { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("headless")]
        public bool Headless { get; set; }

        [JsonProperty("spoof")]
        public bool Spoof { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
    
    internal class Program
    {
        // ReSharper disable once PossibleNullReferenceException
        internal static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static LaunchParamsConfig _config;

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                MessageBox.Show("You're not supposed to run this directly! Do \"Starlight.Cli.exe hook\" in the command prompt to hook Roblox's scheme.", "Starlight", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }

            var firstTime = false;
            var launchCfgFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GlobalLaunchSettings.json");
            if (!File.Exists(launchCfgFile)) // If no config file exists, create one and default it.
            {
                firstTime = true;
                _config = new LaunchParamsConfig();
                File.WriteAllText(launchCfgFile, JsonConvert.SerializeObject(_config, new JsonSerializerSettings { Formatting = Formatting.Indented }));
            }
            else // Otherwise, load the config file.
            {
                try
                {
                    _config = JsonConvert.DeserializeObject<LaunchParamsConfig>(File.ReadAllText(launchCfgFile)) ?? new LaunchParamsConfig();
                }
                catch (JsonException)
                {
                    MessageBox.Show("Could not parse configuration. Ensure that your configuration contains valid JSON.", "Starlight", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }
            }

            if (_config.SaveLog)
                Logger.Init(_config.Verbose);

            if (firstTime)
            {
                Log.Info("Welcome, new person!");
                MessageBox.Show("I believe it's your first time using Starlight. Thanks for checking out the project! When using the launcher, Roblox will update in the background (if it's not updated yet) and launch. This may take some time and I have no profiling implemented to show progress. I don't have a UI out for this project, so you'll have to bear with me until I can make one. If the installation or launch fails, a message box will pop up saying that it failed. Your launch settings can be found at Starlight's base directory called \"GlobalLaunchSettings.json\", which you can modify to set your default launch options.\n\nClick OK to launch Roblox and never show this tutorial again.", "Tutorial", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            var latest = Bootstrapper.GetLatestHash();
            if (Bootstrapper.QueryClient(latest) is null)
            {
                // Update Roblox
                Log.Info("Updating Roblox...");
                try
                {
                    Bootstrapper.Install(latest);
                }
                catch (BootstrapException ex)
                {
                    Log.Fatal("Failed to install Roblox.", ex);
                    return 1;
                }
            }

            Log.Info("Launching Roblox with payload...");
            try
            {
                var inst = Scheme.Launch(args[0], _config);
                return inst is null ? 1 : 0;
            }
            catch (LaunchException ex)
            {
                Log.Fatal("Failed to launch Roblox.", ex);
            }
            catch (AppModException ex)
            {
                // I wouldn't say fatal here because Roblox did open.
                Log.Error("Launch succeeded, but failed to post-launch.", ex);
            }

            MessageBox.Show($"Failed to launch Roblox.\nLog can be viewed at {Logger.LogFile}.", "Starlight", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }
}
