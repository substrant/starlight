using System;
using System.Linq;
using System.Reflection;

using CommandLine;

namespace Starlight.Cli.Commands
{
    public class Command
    {
        static string SerializeOption(OptionAttribute opt)
        {
            if (opt.ShortName != string.Empty)
                return $"-{opt.ShortName}";
            else return $"--{opt.LongName}";
        }

        public string Serialize()
        {
            string options = string.Empty;

            foreach (PropertyInfo prop in typeof(HookOptions).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;
                
                foreach (var attr in prop.GetCustomAttributes(true).Where(x => x as OptionAttribute != null).Cast<OptionAttribute>())
                {
                    var value = prop.GetValue(this);
                    if (value == null)
                        continue;

                    if (prop.PropertyType == typeof(string))
                    {
                        if (string.IsNullOrEmpty((string)value))
                            continue;
                        options += SerializeOption(attr) + ' ' + value.ToString() + ' ';
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        if ((bool)value)
                            options += SerializeOption(attr) + ' ';
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        options += SerializeOption(attr) + ' ' + value.ToString() + ' ';
                    }
                }
            }

            if (options.Length < 1)
                return string.Empty;
            
            return options.Substring(0, options.Length - 1);
        }
    }

    [Verb("install", HelpText = "Install Roblox.")]
    public class InstallOptions : Command
    {
        [Option('h', "hash", Required = false, HelpText = "Install a specific hash of Roblox.")]
        public string Hash { get; set; }
    }
    
    [Verb("uninstall", HelpText = "Uninstall Roblox.")]
    public class UninstallOptions : Command
    {
        [Option('h', "hash", Required = false, HelpText = "Uninstall a specific hash of Roblox.")]
        public string Hash { get; set; }
    }

    [Verb("launch", HelpText = "Launch via scheme payload.")]
    public class LaunchOptions : Command, IStarlightLaunchParams
    {
        [Option('p', "payload", Required = false)]
        public string Payload { get; set; }

        [Option('s', "spoof", Required = false, Default = false, HelpText = "Roblox's launch schema to launch this program.")]
        public bool Spoof { get; set; }
        
        [Option('h', "hash", Required = false, Default = "", HelpText = "Run a specific hash of Roblox.")]
        public string Hash { get; set; }

        [Option("headless", Required = false, Default = false, HelpText = "Launch without opening the window. Good when combined with \"fps-cap\".")]
        public bool Headless { get; set; }

        [Option("fps-cap", Required = false, Default = 0, HelpText = "Set the maximum FPS (ignores if zero).")]
        public int FpsCap { get; set; }
    }

    [Verb("hook", HelpText = "Hook Roblox's schema to launch using this program.")]
    public class HookOptions : LaunchOptions
    {
        [Obsolete("This property is a placeholder and cannot be used.", true)]
        [Option('p', "payload", Required = false, Hidden = true)]
        public new string Payload { get; }
    }

    [Verb("unhook", HelpText = "Remove the hook on Roblox's schema.")]
    public class UnhookOptions : Command
    {
        
    }

    [Verb("unlock", HelpText = "Enable multiple Roblox clients.")]
    public class UnlockOptions : Command
    {
        [Option('e', "relock", Required = false, Default = false, HelpText = "Lock when all clients close.")]
        public bool Relock { get; set; }
    }
}
