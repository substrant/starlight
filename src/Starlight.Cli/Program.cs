using System;
using System.Linq;
using System.Threading;
using CommandLine;
using Starlight.Cli.Verbs;
using Starlight.Misc;

namespace Starlight.Cli;

internal class Program
{
    static int Main(string[] args)
    {
        Console.Title = "Starlight CLI";
        
        // I do not like how this is done, but it works.
        var code = Parser.Default.ParseArguments<Hook, Unhook>(args)
            .MapResult(
                (Hook x) => x.Invoke(),
                (Unhook x) => x.Invoke(),
                _ => 1);

#if DEBUG
            Console.WriteLine($"Exit code: 0x{code:X}. Press any key to exit...");
            Console.ReadKey();
#endif

        return code;
    }
}