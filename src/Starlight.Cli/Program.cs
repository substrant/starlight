using System;
using System.Linq;
using System.Threading;
using CommandLine;
using Starlight.Cli.Verbs;
using Starlight.Misc;
using static Starlight.Cli.Native;

namespace Starlight.Cli
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.Title = "Starlight CLI";

#if DEBUG
            // In Debug mode: Starlight.Cli.exe [nodebug] <verb> [options], skips requirement to attach debugger.
            if (args.Length < 1 || args[0] != "nodebug")
            {
                Console.Write("Waiting for debugger..."); // Debug->Attach to process..."Shift + Alt + P" to reattach
                while (!IsDebuggerPresent())
                    Thread.Sleep(100);

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Attached!");
                Console.ForegroundColor = ConsoleColor.Gray;

                Logger.Init(true);
            }
            else
                args = args.Skip(1).ToArray();
#else
            if (args.Length > 0 && args[0] == "debug")
            {
                Logger.Init(true);
                args = args.Skip(1).ToArray();
            }
#endif

            // I do not like how this is done, but it works.
            var code = Parser.Default.ParseArguments<Hook, Install, RawLaunch, Launch, Unhook, Uninstall, Unlock>(args)
                .MapResult(
                    (Hook x) => x.Invoke(),
                    (Install x) => x.Invoke(),
                    (Launch x) => x.Invoke(),
                    (RawLaunch x) => x.Invoke(),
                    (Unhook x) => x.Invoke(),
                    (Uninstall x) => x.Invoke(),
                    (Unlock x) => x.Invoke(),
                    _ => 1);

#if DEBUG
            Console.WriteLine($"Exit code: 0x{code:X}. Press any key to exit...");
            Console.ReadKey();
#endif

            return code;
        }
    }
}
