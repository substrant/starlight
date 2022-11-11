using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Starlight.Misc
{
    public static class AsyncHelpers
    {
        static readonly TaskFactory TaskFactory = new(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static Task RunAsync(Action action) => TaskFactory.StartNew(action);

        public static Task<T> RunAsync<T>(Func<T> func) => TaskFactory.StartNew(func);
        
        public static void RunSync(Func<Task> func)
        {
            var threadCulture = CultureInfo.CurrentCulture;
            var threadUiCulture = CultureInfo.CurrentUICulture;
            
            RunAsync(() =>
            {
                Thread.CurrentThread.CurrentCulture = threadCulture;
                Thread.CurrentThread.CurrentUICulture = threadUiCulture;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }

        public static T RunSync<T>(Func<Task<T>> func)
        {
            var threadCulture = CultureInfo.CurrentCulture;
            var threadUiCulture = CultureInfo.CurrentUICulture;

            return RunAsync(() =>
            {
                Thread.CurrentThread.CurrentCulture = threadCulture;
                Thread.CurrentThread.CurrentUICulture = threadUiCulture;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }
    }
}