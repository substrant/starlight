using CommandLine;
using System;
using Starlight.Core;
using System.Threading;
using static Starlight.Cli.Native;

namespace Starlight.Cli.Verbs
{
    [Verb("unlock", HelpText = "Enable multiple Roblox clients.")]
    public class Unlock : VerbBase
    {
        [Option('e', "relock", Required = false, Default = false, HelpText = "Lock when all clients close.")]
        public bool Relock { get; set; }

        protected override int Init()
        {
            return 0;
        }

        static void CommitThread(CancellationToken cancelToken)
        {
            // I use FindWindow because scanning process list kills my CPU.
            while (!cancelToken.IsCancellationRequested)
            {
                while (FindWindow(null, "Roblox") == 0) // If Roblox isn't open yet, wait for open.
                {
                    cancelToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(1.0d / 15));
                    if (cancelToken.IsCancellationRequested)
                        break;
                }

                Launcher.CommitSingleton();

                while (FindWindow(null, "Roblox") != 0) // Wait for all instances to close.
                {
                    cancelToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(1.0d / 15));
                    if (cancelToken.IsCancellationRequested)
                        break;
                }
                
                Launcher.ReleaseSingleton();
            }
        }

        protected override int InternalInvoke()
        {
            if (FindWindow(null, "Roblox") != 0)
            {
                Console.WriteLine("Waiting for all Roblox processes to close...");
                while (FindWindow(null, "Roblox") != 0)
                    Thread.Sleep(TimeSpan.FromSeconds(1.0d / 15));
            }

            if (Relock)
            {
                Launcher.CommitSingleton();

                while (FindWindow(null, "Roblox") != 0)
                    Thread.Sleep(TimeSpan.FromSeconds(1.0d / 15));

                Launcher.ReleaseSingleton();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Press enter to release singleton...");

                CancellationTokenSource commitTask = new();
                new Thread(() => CommitThread(commitTask.Token)).Start();

                Console.ReadLine();
                commitTask.Cancel(); // Stop the commit thread
            }

            return 0;
        }
    }
}
