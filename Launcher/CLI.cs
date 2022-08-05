using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;

namespace Starlight
{
    public class CLI
    {
        [Verb("launch", HelpText = "Launch using a Roblox launcher URL.")]
        public class LaunchOptions
        {
            [Option("payload", Required = false, HelpText = "The Roblox launch payload.")]
            public string Payload { get; set; }

            [Option('g', "gui", Required = false, Default = true, HelpText = "Launch using the gui.")]
            public bool Gui { get; set; }

            [Option("headless", Required = false, Default = false, HelpText = "Launch without opening the Roblox window.")]
            public bool Headless { get; set; }

            [Option("no-spoof", Required = false, Default = false, HelpText = "Don't evad.")]
            public bool NoSpoof { get; set; }

            [Option('h', "git-hash", Required = false, Default = "", HelpText = "Force launcher to run a specific verison of Roblox instead of the latest version.")]
            public string GitHash { get; set; }
            public bool ForceVersion { get => !GitHash.IsEmpty(); }

            [Option('s', "strict", Required = false, Default = false, HelpText = "If you launch with strict mode, the launcher will not update Roblox if the binary is obsolete.")]
            public bool Strict { get; set; }
        }

        [Verb("hook", HelpText = "Hook Roblox's schema to launch using this program.")]
        public class HookOptions : LaunchOptions // Mirror the options
        {
            [Obsolete("This property is a placeholder and cannot be used", true)]
            [Option("payload", Required = false, Hidden = true)]
            public new string Payload { get; }
        }

        [Verb("unhook", HelpText = "Remove the hook on Roblox's scheme.")]
        public class UnhookOptions
        {

        }

        [Verb("install", HelpText = "Install Roblox.")]
        public class InstallOptions
        {
            [Option('h', "git-hash", Required = false, HelpText = "Install a specific version of Roblox.")]
            public string GitHash { get; set; }
        }

        public static string SerializeOption(OptionAttribute opt)
        {
            if (opt.ShortName != string.Empty)
                return $"-{opt.ShortName}";
            else return $"--{opt.LongName}";
        }

        public static string SerializeOptions(object options, IEnumerable<string> ignore = null) // Reflection is hot
        {
            StringBuilder szOptions = new();
            foreach (PropertyInfo prop in typeof(LaunchOptions).GetProperties())
            {
                if (ignore?.Contains(prop.Name) == true) continue;
                if (!prop.CanRead || !prop.CanWrite) continue;

                foreach (OptionAttribute attr in prop.GetCustomAttributes(true).Where(x => x as OptionAttribute != null))
                {
                    var value = prop.GetValue(options);
                    if (value == null) continue;

                    //Console.WriteLine("{0} -> {1} ({2}", prop.Name, value, prop.PropertyType.Name);
                    if (prop.PropertyType == typeof(string))
                    {
                        if (!((string)value).IsEmpty())
                            szOptions.Append(SerializeOption(attr) + ' ' + value.ToString() + ' ');
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        if ((bool)value)
                            szOptions.Append(SerializeOption(attr) + ' ');
                    }
                    else throw new Exception("Invalid command value type.");
                }
            }
            return szOptions.ToString();
        }
    }
}
