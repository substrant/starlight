using System;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using static Starlight.CLI;

namespace Starlight.Gui
{
    static class Program
    {
        public static int Load(LaunchOptions launchOptions)
        {
            var app = new Application();
            var window = new MainWindow(launchOptions);
            app.Exit += (s, e) => e.ApplicationExitCode = window.ExitCode;
            return app.Run(window);
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            string str = "";
            Exception ex = (Exception)args.ExceptionObject;
            if (ex as AggregateException != null)
            {
                AggregateException aex = (AggregateException)args.ExceptionObject;
                foreach (Exception inner in aex.InnerExceptions)
                    str += string.Format("An unhandled exception was caught: {0}\nTrace: {1}", inner.ToString(), inner.StackTrace);
            }
            else str += string.Format("An unhandled exception was caught: {0}\nTrace: {1}", ex.Message, ex.StackTrace);
            MessageBox.Show(str);
        }

        [STAThread]
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
            return Parser.Default.ParseArguments<LaunchOptions>(args)
                .MapResult(
                Load,
                x => 1);
        }
    }
}
